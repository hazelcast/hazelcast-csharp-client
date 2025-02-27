// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Socket"/> class.
    /// </summary>
    internal static class SocketExtensions
    {
        // The purpose of this class is to provide Socket.ConnectAsync methods that support a combination
        // of timeout and/or cancellation token. Prior to .NET 5, none of these options are built-in.
        // Starting with .NET 5, cancellation token support token is built-in, but not timeout support.
        //
        // Support for timeout + cancellation token

        // our own extension, not provided by any framework
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds)
            => socket.ConnectAsync(endPoint, timeoutMilliseconds, default);

#if !NET5_0_OR_GREATER

        // that one is built-in starting with .NET 5
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, CancellationToken cancellationToken)
            => socket.ConnectAsync(endPoint, -1, cancellationToken);

#endif

        // our own extension, not provided by any framework
        public static async Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

            static void Complete(SocketAsyncEventArgs sea, TaskCompletionSource<Socket> cs)
            {
                if (sea.SocketError == SocketError.Success)
                    cs.TrySetResult(sea.ConnectSocket);
                else
                    cs.TrySetException(new SocketException((int)sea.SocketError));
            }

            static void CompleteCallback(object sender, SocketAsyncEventArgs sea)
                => Complete(sea, (TaskCompletionSource<Socket>) sea.UserToken);

            var connected = new TaskCompletionSource<Socket>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var sea = new SocketAsyncEventArgs
            {
                UserToken = connected,
                RemoteEndPoint = endPoint
            };

            sea.Completed += CompleteCallback;
            var pending = socket.ConnectAsync(sea);

            if (!pending)
            {
                // "The I/O operation completed synchronously. In this case, The Completed event on the
                // sea parameter will not be raised and the sea object passed as a parameter may be
                // examined immediately"
                if (sea.SocketError != SocketError.Success)
                    throw new SocketException((int) sea.SocketError);
                return;
            }

            // "the I/O operation is pending. The Completed event on the sea parameter will be raised
            // upon completion of the operation."
            // but, we don't want to wait forever, and have to deal with timeout + cancellation.

            using var timeoutCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);
            var task = await Task.WhenAny(connected.Task, timeoutTask).CfAwait();

            if (!timeoutTask.IsCompletedSuccessfully())
            {
                timeoutCancel.Cancel(); // don't leave the timeoutTask running
                await timeoutTask.CfAwaitNoThrow(); // and make sure it does not leak unobserved exception
            }

            if (task != timeoutTask)
            {
                connected.Task.GetAwaiter().GetResult(); // we know the task has completed - throw if needed
                return;
            }

            // we *have* timed out (or got canceled)

            // we *need* to await connected.Task even when it was not the task returned by WhenAny else
            // it may leak unobserved exceptions, as ConnectCallback may be invoked even when the socket
            // is being teared down - however, ConnectCallback may also *not* be invoked, so we try to
            // set result to force the task to complete, making sure we don't hang waiting for it
            connected.TrySetResult(null);
            await connected.Task.CfAwaitNoThrow(); // don't leave exceptions unobserved

            TryDispose(socket);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("The socket connection operation has been canceled.");

            throw new TimeoutException("The socket connection operation has timed out.");
        }

        [ExcludeFromCodeCoverage] // catch statement is a pain to cover properly
        private static void TryDispose(IDisposable disposable)
        {
            try
            {
                disposable.Dispose();
            }
            catch { /* may happen, don't care */ }
        }
    }
}
