/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Config;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientRetryTest : HazelcastBaseTest
    {
        private readonly int Count = 1000;

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetNetworkConfig().SetRedoOperation(true);
        }

        [Test]
        public void TestRetryRequestsWhenInstanceIsShutdown()
        {
            var nodeId = AddNodeAndWait();
            var map = Client.GetMap<int,string>(TestSupport.RandomString());
            for (int i = 0; i < Count; i++)
            {
                map.Put(i, TestSupport.RandomString());
                if (i == Count / 2)
                {
                    Cluster.RemoveNode(nodeId);
                }
            }
    
            TestSupport.AssertTrueEventually(() =>
            {
                var keys = map.KeySet();
                for (int i = 0; i < Count; i++)
                {
                    Assert.IsTrue(keys.Contains(i), "Key " + i + " was not found");
                }
                Assert.AreEqual(Count, map.Size());
            }, timeoutSeconds: 60);
        }

        [Test]
        public void TestRetryAsyncRequest()
        {
            int count = 100;
            var nodeId = AddNodeAndWait();
            var map = Client.GetMap<int, string>(TestSupport.RandomString());
            for (var i = 0; i < count; i++)
            {
                map.PutAsync(i, TestSupport.RandomString());
            }
            Cluster.RemoveNode(nodeId);
            TestSupport.AssertTrueEventually(() =>
            {
                var keys = map.KeySet();
                for (var i = 0; i < count; i++)
                {
                    Assert.IsTrue(keys.Contains(i), "Key " + i + " was not found");
                }
                Assert.AreEqual(count, map.Size());
            }, timeoutSeconds: 60);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ClientTransactionRetry()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();

            var map = context.GetMap<int, string>(TestSupport.RandomString());

            Cluster.RemoveNode();
            Cluster.AddNode();
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    // put should eventually fail as the node which the transaction is running against 
                    // will be shut down
                    map.Put(i, TestSupport.RandomString());
                }
            }
            finally
            {
                context.RollbackTransaction();
            }
        }
    }
}
