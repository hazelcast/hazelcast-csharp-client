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

using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientCountDownLatchTest:HazelcastBaseTest
	{
        //internal const string name = "ClientCountDownLatchTest";

		internal ICountDownLatch l;

        [SetUp]
        public void Init()
        {
            l = Client.GetCountDownLatch(TestSupport.RandomString());
            
        }

        [TearDown]
        public void Destroy()
        {
            l.Destroy();
            Console.WriteLine("destroy");
        }


		[Test]
		public virtual void TestLatch()
		{
			Assert.IsTrue(l.TrySetCount(20));
			Assert.IsFalse(l.TrySetCount(10));
			Assert.AreEqual(20, l.GetCount());

		    var t1 = new Thread(delegate(object o)
		    {
                for (int i = 0; i < 20; i++)
                {
                    l.CountDown();
                    try
                    {
                        Thread.Sleep(60);
                    }
                    catch
                    {
                        
                    }
                }
		    });
            t1.Start();

            
            Assert.IsFalse(l.Await(1, TimeUnit.SECONDS));
			Assert.IsTrue(l.Await(5, TimeUnit.SECONDS));

		    t1.Join();
		}

	}
}
