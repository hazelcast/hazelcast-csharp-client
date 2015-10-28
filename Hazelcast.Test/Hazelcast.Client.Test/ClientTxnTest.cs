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
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnTest : HazelcastBaseTest
    {
        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestNegativeDurability()
        {
            var options = new TransactionOptions();
            options.SetDurability(-1);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestNegativeTimeout()
        {
            var options = new TransactionOptions();
            options.SetTimeout(-1, TimeUnit.MILLISECONDS);
        }

        [Test, ExpectedException(typeof (TransactionNotActiveException))]
        public void TestTxnNotActi()
        {
            var context = Client.NewTransactionContext();
            context.CommitTransaction();
            context.CommitTransaction();
        }

        [Test, ExpectedException(typeof (TransactionNotActiveException))]
        public void TestTxnTimeOut()
        {
            var options = new TransactionOptions();
            options.SetTimeout(100, TimeUnit.MILLISECONDS);
            var context = Client.NewTransactionContext(options);
            Thread.Sleep(500);
            context.CommitTransaction();
        }
    }
}