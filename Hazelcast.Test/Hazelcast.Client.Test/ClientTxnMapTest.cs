// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnMapTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _name = TestSupport.RandomString();
            _map = Client.GetMap<object, object>(_name);
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
            _map.Destroy();
        }

        private string _name;
        private IMap<object, object> _map;

        [Test]
        public void GetForUpdate()
        {
            const string key = "key";
            const int initialValue = 111;
            const int newValue = 123;
            _map.Put(key, initialValue);

            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);

            var val = txnMap.GetForUpdate(key);
            Assert.AreEqual(initialValue, val);
            Assert.IsTrue(_map.IsLocked(key));
            txnMap.Put(key, newValue);
            context.CommitTransaction();
            Assert.IsFalse(_map.IsLocked(key));
            Assert.AreEqual(newValue, _map.Get(key));
        }

        [Test]
        public void KeySetPredicate()
        {
            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            _map.Put("key3", "value3");

            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);

            var sqlPredicate = new SqlPredicate("this == value1");
            var keys = txnMap.KeySet(sqlPredicate);

            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual("key1", keys.First());

            var values = txnMap.Values(sqlPredicate);

            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("value1", values.First());

            context.CommitTransaction();
        }

        [Test]
        public void KeySetValues()
        {
            _map.Put("key1", "value1");
            _map.Put("key2", "value2");
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            Assert.IsNull(txMap.Put("key3", "value3"));
            Assert.AreEqual(3, txMap.Size());
            Assert.AreEqual(3, txMap.KeySet().Count);
            Assert.AreEqual(3, txMap.Values().Count);
            context.CommitTransaction();
            Assert.AreEqual(3, _map.Size());
            Assert.AreEqual(3, _map.KeySet().Count);
            Assert.AreEqual(3, _map.Values().Count);
        }

        [Test]
        public void PutAndRoleBack()
        {
            const string key = "key";
            const string value = "value";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var mapTxn = context.GetMap<object, object>(_name);
            mapTxn.Put(key, value);
            context.RollbackTransaction();
            Assert.IsNull(_map.Get(key));
        }

        [Test]
        public void PutGet()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            Assert.IsNull(txnMap.Put("key1", "value1"));
            Assert.AreEqual("value1", txnMap.Get("key1"));
            Assert.IsNull(_map.Get("key1"));
            context.CommitTransaction();
            Assert.AreEqual("value1", _map.Get("key1"));
        }

        [Test]
        public void PutWithTTL()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            const int ttlMillis = 100;

            Assert.IsNull(txnMap.Put("key1", "value1", ttlMillis, TimeUnit.Milliseconds));
            Assert.AreEqual("value1", txnMap.Get("key1"));

            context.CommitTransaction();

            Assert.AreEqual("value1", _map.Get("key1"));
            TestSupport.AssertTrueEventually(() => Assert.IsNull(_map.Get("key1")));
        }

        [Test]
        public void TnxMapContainsKey()
        {
            _map.Put("key1", "value1");
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Put("key2", "value2");
            Assert.IsTrue(txMap.ContainsKey("key1"));
            Assert.IsTrue(txMap.ContainsKey("key2"));
            Assert.IsFalse(txMap.ContainsKey("key3"));
            context.CommitTransaction();
        }

        [Test]
        public void TnxMapDelete()
        {
            const string key = "key1";
            const string value = "old1";
            _map.Put(key, value);
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Delete(key);
            context.CommitTransaction();
            Assert.IsNull(_map.Get(key));
        }

        [Test]
        public void TnxMapIsEmpty()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            Assert.IsTrue(txMap.IsEmpty());
            context.CommitTransaction();
        }

        [Test]
        public void TnxMapPutIfAbsent()
        {
            const string keyValue1 = "keyValue1";
            const string keyValue2 = "keyValue2";
            _map.Put(keyValue1, keyValue1);
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.PutIfAbsent(keyValue1, "NOT_THIS");
            txMap.PutIfAbsent(keyValue2, keyValue2);
            context.CommitTransaction();
            Assert.AreEqual(keyValue1, _map.Get(keyValue1));
            Assert.AreEqual(keyValue2, _map.Get(keyValue2));
        }

        [Test]
        public void TnxMapRemove()
        {
            const string key = "key1";
            const string value = "old1";
            _map.Put(key, value);
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Remove(key);
            context.CommitTransaction();
            Assert.IsNull(_map.Get(key));
        }

        [Test]
        public void TnxMapRemoveKeyValue()
        {
            const string key1 = "key1";
            const string oldValue1 = "old1";
            const string key2 = "key2";
            const string oldValue2 = "old2";
            _map.Put(key1, oldValue1);
            _map.Put(key2, oldValue2);
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Remove(key1, oldValue1);
            txMap.Remove(key2, "NO_REMOVE_AS_NOT_VALUE");
            context.CommitTransaction();
            Assert.IsNull(_map.Get(key1));
            Assert.AreEqual(oldValue2, _map.Get(key2));
        }

        [Test]
        public void TnxMapReplace()
        {
            const string key1 = "key1";
            const string key2 = "key2";
            const string replaceValue = "replaceValue";
            _map.Put(key1, "OLD_VALUE");
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Replace(key1, replaceValue);
            txMap.Replace(key2, "NOT_POSSIBLE");
            context.CommitTransaction();
            Assert.AreEqual(replaceValue, _map.Get(key1));
            Assert.IsNull(_map.Get(key2));
        }

        [Test]
        public void TnxMapReplaceKeyValue()
        {
            const string key1 = "key1";
            const string oldValue1 = "old1";
            const string newValue1 = "new1";
            const string key2 = "key2";
            const string oldValue2 = "old2";
            _map.Put(key1, oldValue1);
            _map.Put(key2, oldValue2);
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txMap = context.GetMap<object, object>(_name);
            txMap.Replace(key1, oldValue1, newValue1);
            txMap.Replace(key2, "NOT_OLD_VALUE", "NEW_VALUE_CANT_BE_THIS");
            context.CommitTransaction();
            Assert.AreEqual(newValue1, _map.Get(key1));
            Assert.AreEqual(oldValue2, _map.Get(key2));
        }

        [Test]
        public void TxnMapGet_BeforeCommit()
        {
            const string key = "key";
            const string value = "Value";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            txnMap.Put(key, value);
            Assert.AreEqual(value, txnMap.Get(key));
            Assert.IsNull(_map.Get(key));
            context.CommitTransaction();
        }

        [Test]
        public void TxnMapPut()
        {
            const string key = "key";
            const string value = "Value";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            txnMap.Put(key, value);
            context.CommitTransaction();
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void TxnMapPut_BeforeCommit()
        {
            const string key = "key";
            const string value = "Value";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            Assert.IsNull(txnMap.Put(key, value));
            context.CommitTransaction();
        }

        [Test]
        public void TxnMapSet()
        {
            const string key = "key";
            const string value = "Value";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            txnMap.Set(key, value);
            context.CommitTransaction();
            Assert.AreEqual(value, _map.Get(key));
        }

        [Test]
        public void UnlockAfterRollback()
        {
            const string key = "key";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnMap = context.GetMap<object, object>(_name);
            txnMap.Put(key, "value");
            context.RollbackTransaction();
            Assert.IsFalse(_map.IsLocked(key));
        }
    }
}