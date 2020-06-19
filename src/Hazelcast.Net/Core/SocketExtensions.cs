// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Socket"/> class.
    /// </summary>
    public static class SocketExtensions
    {
        public static async Task ConnectAsync(this Socket socket, EndPoint endPoint, int timeoutMilliseconds)
        {
            // this is the code that the runtime uses, unchanged

            if (socket == null) throw new ArgumentNullException(nameof(socket));

            var tcs = new TaskCompletionSource<bool>(socket);
            socket.BeginConnect(endPoint, iar =>
            {
                var innerTcs = (TaskCompletionSource<bool>) iar.AsyncState;
                try
                {
                    ((Socket) innerTcs.Task.AsyncState).EndConnect(iar);
                    innerTcs.TrySetResult(true);
                }
                catch (Exception e) { innerTcs.TrySetException(e); }
            }, tcs);

            // only, we don't return the task, but handle the timeout
            //return tcs.Task;

            var cancellation = new CancellationTokenSource();
            var t = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMilliseconds, cancellation.Token)).CAF();

            if (t == tcs.Task)
            {
                cancellation.Cancel(); // cancel the delay
                await t.CAF(); // throw or return
                cancellation.Dispose();
                return;
            }

            // else the delay has expired, kill the socket
            cancellation.Dispose();
            try
            {
                // triggers the callback, thus EndConnect
                socket.Close();
                socket.Dispose();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            throw new TimeoutException();
        }

        public static async Task ConnectAsync(this Socket socket, EndPoint endPoint, CancellationToken cancellationToken)
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

            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var t = await Task.WhenAny(tcs.Task, Task.Delay(-1, cancellation.Token)).CAF();

            if (t == tcs.Task)
            {
                cancellation.Cancel(); // cancel the delay
                await t.CAF(); // throw or return
                cancellation.Dispose();
                return;
            }

            // else the delay has been cancelled, kill the socket
            cancellation.Dispose();
            try
            {
                // triggers the callback, thus EndConnect
                socket.Close();
                socket.Dispose();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }

            throw new TimeoutException();
        }
    }
}
