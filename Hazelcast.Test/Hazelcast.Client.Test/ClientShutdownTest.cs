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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientShutdownTest : HazelcastBaseTest
    {
        [SetUp]
        public void Setup()
        {
            if (Client == null)
            {
                Client = CreateClient();
            }
        }

        [Test, ExpectedException(typeof(HazelcastException), ExpectedMessage = "Client is shut down.")]
        public void TestOperationAfterShutdown()
        {
            var map = Client.GetMap<int, string>(TestSupport.RandomString());
            for (int i = 0; i < 100; i++)
            {
                map.Put(i, TestSupport.RandomString());
            } 
            Client.Shutdown();
            Client = null;
            map.Get(0);
        }

        [Test, ExpectedException(typeof(HazelcastException))]
        public void TestOperationDuringClientShutdown()
        {
            var map = Client.GetMap<int, string>(TestSupport.RandomString());
            for (int i = 0; i < 100; i++)
            {
                map.Put(i, TestSupport.RandomString());
                Task.Factory.StartNew(() =>
                {
                    Client.Shutdown();
                    Client = null;
                });
            }
        }

        [Test, ExpectedException(typeof(HazelcastException), ExpectedMessage = "Client is shutting down.")]
        public void TestAsyncOperationDuringClientShutdown()
        {
            var map = Client.GetMap<int, string>(TestSupport.RandomString());

            var count = 10000;
            var tasks = new List<Task>();
            for (var i = 0; i < count; i++)
            {
                tasks.Add(map.PutAsync(i, TestSupport.RandomString()));
            }
            Client.Shutdown();
            Client = null;
            try
            {
                foreach (var task in tasks)
                {
                    task.Wait();
                }
            }
            catch (AggregateException e)
            {
                throw e.InnerExceptions.First();
            }
        }
    }
}
