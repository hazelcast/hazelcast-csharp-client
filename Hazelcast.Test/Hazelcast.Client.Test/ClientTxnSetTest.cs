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

using System.Collections.Generic;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[NUnit.Framework.TestFixture]
	public class ClientTxnSetTest:HazelcastBaseTest
	{
        //internal const string name = "test";

        [SetUp]
        public void Init()
        {
            //map = client.GetMap<object, object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            //map.Clear();
            //client.GetLifecycleService().Shutdown();
        }
		/// <exception cref="System.Exception"></exception>
		[NUnit.Framework.Test]
		public virtual void TestAddRemove()
		{
		    var name = TestSupport.RandomString();
            var s = Client.GetSet<object>(name);
			s.Add("item1");
			ITransactionContext context = Client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalSet<object> set = context.GetSet<object>(name);
			NUnit.Framework.Assert.IsTrue(set.Add("item2"));
			NUnit.Framework.Assert.AreEqual(2, set.Size());
			NUnit.Framework.Assert.AreEqual(1, s.Count);
			NUnit.Framework.Assert.IsFalse(set.Remove("item3"));
			NUnit.Framework.Assert.IsTrue(set.Remove("item1"));
			context.CommitTransaction();
			NUnit.Framework.Assert.AreEqual(1, s.Count);
		}
	}
}
