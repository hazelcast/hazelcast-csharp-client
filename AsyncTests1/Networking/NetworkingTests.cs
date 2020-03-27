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

using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncTests1.Networking
{
    [TestFixture]
    public class NetworkingTests
    {
        // NOTES
        //
        // read
        //  danger of completion source
        //    https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        //
        // sending: we can either queue messages, or just send them immediately
        //  it does not make a diff for user, who's going to wait anyways
        //  but queueing may prevent flooding the server
        //  and, in order to be multi-threaded, we HAVE to serialize the sending
        //  of messages through the socket - either by queuing or by forcing
        //  the client to wait - which is kind of a nice way to apply back-
        //  pressure?
        //
        // TODO
        //  must: implement something, lock in connection
        //  alternative: implement queuing in connection
        //
        // receiving: we process messages immediately so there is no queuing of
        //  messages - should we have some?
        //
        // TODO: understand the schedulers in HZ code

        [Test]
        public async Task Test()
        {
            var log = new Log("TST");
            log.WriteLine("Begin");

            log.WriteLine("Start server");
            var server = new Server("localhost", 11000);
            await server.StartAsync();

            log.WriteLine("Start client");
            var client = new Client("localhost", 11000);

            log.WriteLine("Send message");
            var message = new Message("ping");
            var response = await client.SendAsync(message);

            log.WriteLine("Got response: " + response.Text);

            log.WriteLine("Stop client");
            await client.CloseAsync();

            log.WriteLine("Stop server");
            await server.StopAsync();
            await Task.Delay(1000);

            log.WriteLine("End");
            await Task.Delay(100);
        }
    }
}