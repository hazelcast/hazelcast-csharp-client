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
        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds)
            => socket.ConnectAsync(endPoint, timeoutMilliseconds, default);

        public static Task ConnectAsync(this Socket socket, EndPoint endPoint, CancellationToken cancellationToken)
            => socket.ConnectAsync(endPoint, -1, cancellationToken);

        public static async Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            // this is the code that the runtime uses, unchanged

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
            await TryAwait(tcs.Task).CfAwait();

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

        [ExcludeFromCodeCoverage] // catch statement is a pain to cover properly
        private static async Task TryAwait(Task task)
        {
            try
            {
                await task.CfAwait();
            }
            catch { /* may happen, don't care */ }
        }
    }
}
