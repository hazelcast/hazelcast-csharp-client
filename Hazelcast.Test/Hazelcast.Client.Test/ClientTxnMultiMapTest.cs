// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnMultiMapTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _name = TestSupport.RandomString();
        }

        [TearDown]
        public static void Destroy()
        {
        }

        private string _name;

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestPutGetRemove()
        {
            var mm = Client.GetMultiMap<object, object>(_name);

            for (var i = 0; i < 10; i++)
            {
                var key = i + "key";
                Client.GetMultiMap<object, object>(_name).Put(key, "value");
                var context = Client.NewTransactionContext();
                context.BeginTransaction();
                var multiMap = context.GetMultiMap<object, object>(_name);
                Assert.IsFalse(multiMap.Put(key, "value"));
                Assert.IsTrue(multiMap.Put(key, "value1"));
                Assert.IsTrue(multiMap.Put(key, "value2"));
                Assert.AreEqual(3, multiMap.Get(key).Count);
                context.CommitTransaction();
                Assert.AreEqual(3, mm.Get(key).Count);
            }
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestPutGetRemove2()
        {
            var mm = Client.GetMultiMap<object, object>(_name);
            var key = "key";
            Client.GetMultiMap<object, object>(_name).Put(key, "value");
            var context = Client.NewTransactionContext();

            context.BeginTransaction();

            var multiMap = context.GetMultiMap<object, object>(_name);

            Assert.IsFalse(multiMap.Put(key, "value"));
            Assert.IsTrue(multiMap.Put(key, "value1"));
            Assert.IsTrue(multiMap.Put(key, "value2"));
            Assert.AreEqual(3, multiMap.Get(key).Count);

            context.CommitTransaction();

            Assert.AreEqual(3, mm.Get(key).Count);
        }

        [Test]
        public virtual void TestRemove()
        {
            const string key = "key";
            const string value = "value";

            var multiMap = Client.GetMultiMap<string, string>(_name);

            multiMap.Put(key, value);
            var tx = Client.NewTransactionContext();

            tx.BeginTransaction();
            tx.GetMultiMap<string, string>(_name).Remove(key, value);
            tx.CommitTransaction();

            Assert.AreEqual(new List<string>(), multiMap.Get(key));
        }

        [Test]
        public virtual void TestRemoveAll()
        {
            const string key = "key";
            const string value = "value";
            var name = TestSupport.RandomString();
            var multiMap = Client.GetMultiMap<string, string>(name);
            for (var i = 0; i < 10; i++)
            {
                multiMap.Put(key, value + i);
            }


            var tx = Client.NewTransactionContext();

            tx.BeginTransaction();
            tx.GetMultiMap<string, string>(name).Remove(key);
            tx.CommitTransaction();

            Assert.AreEqual(new List<string>(), multiMap.Get(key));
        }

        [Test]
        public void TestSize()
        {
            var key = "key";
            var value = "value";

            var mm = Client.GetMultiMap<object, object>(_name);
            mm.Put(key, value);

            var tx = Client.NewTransactionContext();
            tx.BeginTransaction();
            var txMultiMap = tx.GetMultiMap<object, object>(_name);

            txMultiMap.Put(key, "newValue");
            txMultiMap.Put("newKey", value);

            Assert.AreEqual(3, txMultiMap.Size());

            tx.CommitTransaction();
        }

        [Test]
        public void TestValueCount()
        {
            var key = "key";
            var value = "value";

            var mm = Client.GetMultiMap<object, object>(_name);
            mm.Put(key, value);

            var tx = Client.NewTransactionContext();
            tx.BeginTransaction();
            var txMultiMap = tx.GetMultiMap<object, object>(_name);

            txMultiMap.Put(key, "newValue");

            Assert.AreEqual(2, txMultiMap.ValueCount(key));

            tx.CommitTransaction();
        }
    }
}