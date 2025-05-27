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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class ReproduceDotNetIssues : HazelcastTestBase
    {
        // these tests reproduce issue https://github.com/dotnet/runtime/issues/61411
        //
        // this issue breaks the APM (Asynchronous Programming Model, based upon Begin+End and callbacks) in
        // .NET 6 - prior to .NET 6 they were implementing TAP (Task-based Asynchronous Pattern, based on
        // async calls) on top of APM and in .NET 6 they now switched to implementing APM on top of TAP, as
        // async becomes the recommended approach - but with some issues - essentially leaving an underlying
        // task hidden and non-awaited/awaitable - which means its exceptions are bubbled as unobserved - and
        // this is bad for the health of the overall process.
        //
        // for this reason we cannot continue using APM (which we used for historical reasons) with .NET 6
        // and have to switch to TAP (which is OK for all .NET versions) -> SocketExtensions is updated to
        // use TAP for all .NET versions.
        //
        // note that the issue was fixed in .NET 7 (but it's still best to use TAP anyways)

        [Test]
        [Timeout(20_000)]
        public async Task ReproduceIssue61411()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            var socket = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            socket.Listen(10);
            var okToDispose = new TaskCompletionSource<object>();
            var calledBack = false;
            var caughtObjectDisposedException = false;
            var caughtSocketException = false;
            var caughtOtherException = false;
            socket.BeginAccept(asyncResult =>
            {
                try
                {
                    var s = (Socket)asyncResult.AsyncState;
                    var accepted = s.EndAccept(asyncResult);
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("caught ObjectDisposedException");
                    caughtObjectDisposedException = true;

                    // this (reflection) would be the only way in .NET 6 to observe the exception
                    /*
                    try
                    {
                        var taskField = asyncResult.GetType().GetField("_task", BindingFlags.NonPublic | BindingFlags.Instance);
                        var task = (Task<Socket>)taskField.GetValue(asyncResult);
                        task.GetAwaiter().GetResult();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("caught! " + e);
                        caughtAsyncResultException = true;
                    }
                    */
                }
                catch (SocketException)
                {
                    Console.WriteLine("caught SocketException");
                    caughtSocketException = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("caught " + e);
                    caughtOtherException = true;
                }
                calledBack = true;
                okToDispose.TrySetResult(null);
            }, socket);

            await Task.Delay(1000);

            // invalid - SocketException: cannot shutdown a socket that was not connected
            //socket.Shutdown(SocketShutdown.Both);

            // useless - Close does Dispose immediately
            //socket.Close();

            //await okToDispose.Task; // if we dispose before the callback, EndAccept cannot run

            // callback hasn't run yet
            Assert.That(!calledBack);

            // dispose the socket
            socket.Dispose();

            // callback eventually runs
            await AssertEx.SucceedsEventually(() => Assert.That(calledBack), 2000, 100);

            // and here is the issue we are reproducing:
#if NET7_0_OR_GREATER
            Assert.That(!caughtObjectDisposedException); // EndAccept does not throw about the disposed object = expected
            Assert.That(caughtSocketException); // but throws about the socket being closed = expected
            Assert.That(!caughtOtherException); // and nothing else
#else
            Assert.That(caughtObjectDisposedException); // EndAccept throws because the socket has been disposed = expected
            Assert.That(!caughtOtherException && !caughtSocketException); // and nothing else
#endif

            var e = GetUnobservedExceptions();

#if NET6_0
            // .NET 6 has 1 unobserved exception
            Assert.That(e.Count, Is.EqualTo(1));
            ClearUnobservedExceptions(); // don't fail the test
#else
            // anything before and after .NET 6 is safe
            Assert.That(e.Count, Is.Zero);
#endif
        }

        [Test]
        [Timeout(20_000)]
        public async Task AsyncHasNoIssue()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
            var socket = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            socket.Listen(10);

            var e = new SocketAsyncEventArgs
            {
                UserToken = null // could be used to pass a user state
            };
            e.Completed += (sender, args) =>
            {
                var socket2 = args.AcceptSocket; // the new socket for the accepted connection
            };
            var pending = socket.AcceptAsync(e);
            Assert.That(pending, Is.True);

            await Task.Delay(1000);

            // here, we can dispose the socket and everything will go down without leaving an unobserved
            // exception (this test class inherits from HazelcastTestBase which detects unobserved exceptions
            // on teardown).
            socket.Dispose();
        }
    }
}
