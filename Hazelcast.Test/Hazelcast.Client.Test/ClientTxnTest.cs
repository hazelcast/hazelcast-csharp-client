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

using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnTest : SingleMemberBaseTest
    {
        [Test]
		public void NegativeDurability()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new TransactionOptions();
                options.SetDurability(-1);
            });
        }

        [Test]
		public void NegativeTimeout()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var options = new TransactionOptions();
                options.SetTimeout(-1, TimeUnit.Milliseconds);
            });
        }

        [Test]
		public void TxnNotActive()
        {
            Assert.Throws<TransactionNotActiveException>(() =>
            {
                var context = Client.NewTransactionContext();
                context.CommitTransaction();
                context.CommitTransaction();
            });
        }

        [Test]
		public void TxnTimeOut()
        {
            Assert.Throws<TransactionNotActiveException>(() =>
            {
                var options = new TransactionOptions();
                options.SetTimeout(100, TimeUnit.Milliseconds);
                var context = Client.NewTransactionContext(options);
                Thread.Sleep(500);
                context.CommitTransaction();
            });
        }
    }
}