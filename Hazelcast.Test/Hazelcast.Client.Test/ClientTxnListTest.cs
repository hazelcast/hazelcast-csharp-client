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

using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnListTest : SingleMemberBaseTest
    {
        [TearDown]
        public void Destroy()
        {
            _list.Clear();
            _list.Destroy();
        }

        private IHList<object> _list;

        [Test]
        public void AddRemove()
        {
            var name = TestSupport.RandomString();
            _list = Client.GetList<object>(name);
            _list.Add("item1");
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var listTx = context.GetList<object>(name);
            Assert.IsTrue(listTx.Add("item2"));
            Assert.AreEqual(2, listTx.Size());
            Assert.AreEqual(1, _list.Count);
            Assert.IsFalse(listTx.Remove("item3"));
            Assert.IsTrue(listTx.Remove("item1"));
            context.CommitTransaction();
            Assert.AreEqual(1, _list.Count);
            listTx.Destroy();
        }
    }
}