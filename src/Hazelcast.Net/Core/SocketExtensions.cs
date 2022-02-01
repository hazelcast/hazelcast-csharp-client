// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
        // our own extension, not provided by any framework
        //
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds)
            => socket.ConnectAsync(endPoint, timeoutMilliseconds, default);

#if !NET5_0_OR_GREATER

        // that one is built-in starting with .NET 5
        //
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, CancellationToken cancellationToken)
            => socket.ConnectAsync(endPoint, -1, cancellationToken);

#endif

        // our own extension, not provided by any framework
        // there *are* some ConnectAsync methods but none supporting a timeout
        //
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            if (socket == null) throw new ArgumentNullException(nameof(socket));

#if !NET5_0_OR_GREATER
            // use the ConnectAsync4 method that relies on Begin/End since there is no ConnectAsync yet
            return ConnectAsync4(socket, endPoint, timeoutMilliseconds, cancellationToken);
#else
            // use the ConnectAsync5 method that relies on the now-available ConnectAsync
            return ConnectAsync5(socket, endPoint, timeoutMilliseconds, cancellationToken);
#endif
        }

        private static async Task ConnectAsync5(this Socket socket, EndPoint endPoint, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            static void ConnectCallback(object sender, SocketAsyncEventArgs a)
            {
                var completionSource = (TaskCompletionSource<Socket>) a.UserToken;
                if (a.SocketError == SocketError.Success)
                    completionSource.TrySetResult(a.ConnectSocket);
                else
                    completionSource.TrySetException(new SocketException((int) a.SocketError));
            }

            var connected = new TaskCompletionSource<Socket>();
            var e = new SocketAsyncEventArgs { UserToken = connected };
            e.RemoteEndPoint = endPoint;
            e.Completed += ConnectCallback;
            var pending = socket.ConnectAsync(e);

            Task task;
            if (pending)
            {
                var cancel = new CancellationTokenSource();
                var cancelled = Task.Delay(timeoutMilliseconds, cancel.Token);
                var reg = cancellationToken.Register(() => cancel.Cancel());

                task = await Task.WhenAny(connected.Task, cancelled).CfAwait();
                await reg.DisposeAsync().CfAwait();
                cancel.Cancel();
                cancel.Dispose();
                await cancelled.CfAwaitNoThrow();
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

        public static async Task ConnectAsync4(this Socket socket, EndPoint endPoint, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            // note that purely using this code in .NET 6 leaves an unobserved exception behind
            // read https://github.com/dotnet/runtime/issues/61411

            if (socket == null) throw new ArgumentNullException(nameof(socket));

            var tcs = new TaskCompletionSource<bool>(socket);
            socket.BeginConnect(endPoint, iar =>
            {
                var innerTcs = (TaskCompletionSource<bool>)iar.AsyncState;
                try
                {
                    ((Socket)innerTcs.Task.AsyncState).EndConnect(iar);
                    innerTcs.TrySetResult(true);
                }
                catch (Exception e) { innerTcs.TrySetException(e); }
            }, tcs);

            // only, we don't return the task, but handle the cancellation
            //return tcs.Task;

            var cancellation = cancellationToken == default
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                var t = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMilliseconds, cancellation.Token)).CfAwait();

                // if the connect task has completed successfully...
                if (t == tcs.Task)
                {
                    cancellation.Cancel(); // cancel the delay
                    await t.CfAwait(); // may throw
                    return;
                }
            }
            finally
            {
                cancellation.Dispose(); // make sure we dispose the cancellation
            }

            // else the delay has been cancelled due to either timeout, or cancellation
            // we need to close and dispose the socket,
            // and this will trigger the callback, thus EndConnect, which will throw,
            // and this will fault the connection task, so await will throw too,
            // and then everything will be ok

            TryCloseAndDispose(socket);
            await tcs.Task.CfAwaitNoThrow();

            // finally, throw the correct exception
            // favor cancellation over timeout
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
