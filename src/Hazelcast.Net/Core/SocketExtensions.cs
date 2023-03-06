// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

            static void ConnectCallback(object sender, SocketAsyncEventArgs a)
            {
                var completionSource = (TaskCompletionSource<Socket>) a.UserToken;
                if (a.SocketError == SocketError.Success)
                    completionSource.TrySetResult(a.ConnectSocket);
                else
                    completionSource.TrySetException(new SocketException((int) a.SocketError));
            }

            var connected = new TaskCompletionSource<Socket>();
            var e = new SocketAsyncEventArgs();
            e.UserToken = connected;
            e.RemoteEndPoint = endPoint;
            e.Completed += ConnectCallback;
            var pending = socket.ConnectAsync(e);

            Task task;
            if (pending)
            {
                var timeoutCancel = new CancellationTokenSource();
                var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);
                var reg = cancellationToken.Register(() => timeoutCancel.Cancel());

                task = await Task.WhenAny(connected.Task, timeoutTask).CfAwait();
                await reg.DisposeAsync().CfAwait();
                timeoutCancel.Cancel();
                timeoutCancel.Dispose();
                await timeoutTask.CfAwaitNoThrow();
            }
            else
            {
                task = connected.Task;
                ConnectCallback(null, e);
            }

            e.Dispose();

            if (task == connected.Task)
            {
                await task.CfAwait(); // throw if there is anything to throw (failed to connect...)
                return;
            }

            // else, it was the timeout task

            // we *need* to await connected.Task even when it was not the task returned by WhenAny else
            // it may leak unobserved exceptions, as ConnectCallback may be invoked even when the socket
            // is being teared down - however, ConnectCallback may also *not* be invoked, so we try to
            // set result to force the task to complete, making sure we don't hang waiting for it
            connected.TrySetResult(null);
            await connected.Task.CfAwaitNoThrow(); // don't leave exceptions unobserved

            TryCloseAndDispose(socket);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("The socket connection operation has been canceled.");

            throw new TimeoutException("The socket connection operation has timed out.");
        }

        [ExcludeFromCodeCoverage] // catch statement is a pain to cover properly
        private static void TryCloseAndDispose(Socket socket)
        {
            try
            {
                socket.Close();
                socket.Dispose();
            }
            catch { /* may happen, don't care */ }
        }
    }
}
