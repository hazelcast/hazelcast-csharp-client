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
	public class ClientSemaphoreTest:HazelcastBaseTest
	{
        internal const string name = "ClientSemaphoreTest";


		internal static ISemaphore s;
        [SetUp]
        public void Init()
        {
            s = Client.GetSemaphore(TestSupport.RandomString());

            s.ReducePermits(100);
            s.Release(9);
            s.Release();

        }

        [TearDown]
        public static void Destroy()
        {
        }


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAcquire()
		{
			Assert.AreEqual(10, s.DrainPermits());
            CountdownEvent latch = new CountdownEvent(1);

		    var t = new Thread(delegate(object o)
		    {
                try
                {
                    s.Acquire();
                    latch.Signal();
                }
                catch (Exception e)
                {
                }
		    });
            t.Start();

            Thread.Sleep(100);
			s.Release(2);
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(1, s.AvailablePermits());
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TryAcquire()
		{
			Assert.IsTrue(s.TryAcquire());
			Assert.IsTrue(s.TryAcquire(9));
			Assert.AreEqual(0, s.AvailablePermits());
			Assert.IsFalse(s.TryAcquire(1, TimeUnit.SECONDS));
			Assert.IsFalse(s.TryAcquire(2, 1, TimeUnit.SECONDS));

            CountdownEvent latch = new CountdownEvent(1);

            var t = new Thread(delegate(object o)
            {
                try
                {
                    if (s.TryAcquire(2, 5, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t.Start();

			s.Release(2);
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(0, s.AvailablePermits());
		}

	    [Test]
	    public void TestInit()
	    {
	        var semInit = Client.GetSemaphore(TestSupport.RandomString());
	        semInit.Init(2);
	        Assert.AreEqual(2, semInit.AvailablePermits());
	        semInit.Destroy();
	    }

	    [Test]
        [ExpectedException(typeof(ArgumentException))]
	    public void TestInitNeg()
        {
            var semInit = Client.GetSemaphore(TestSupport.RandomString());
            semInit.Init(-2);
            semInit.Destroy();
        }

	}
}
