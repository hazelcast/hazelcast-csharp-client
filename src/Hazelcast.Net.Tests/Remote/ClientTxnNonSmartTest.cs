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

using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Testing;
using Hazelcast.Transactions;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    public class ClientTxnNonSmartTest : MultiMembersRemoteTestBase
    {
        [SetUp]
        public async Task Setup()
        {
            await AddMember();
            await AddMember();
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            options.Networking.SmartRouting = false;
            return options;
        }

        [Test]
        public async Task TestListenerWithNonSmartRouting()
        {
            var client = await CreateAndStartClientAsync();

            var txOptions = new TransactionOptions { Type = TransactionOptions.TransactionType.TwoPhase };

            var cm  = ((HazelcastClient) client).Cluster.Members;

            for (int i = 0; i < 100; i++)
            {
                var context = await client.BeginTransactionAsync(txOptions);

                var d1 = await context.GetMapAsync<int, string>("test1");
                var d2 = await context.GetMapAsync<int, string>("test2");

                await d1.SetAsync(i, "value");
                await d2.SetAsync(i, "value");

                await context.CommitAsync();
            }
        }

    }
}
