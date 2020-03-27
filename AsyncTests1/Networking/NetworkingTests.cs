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
            client.Open();

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