﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Testing;
using Hazelcast.Transactions;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientTxnTest : SingleMemberClientRemoteTestBase
    {
        [Test]
		public async Task TestNegativeDurability()
		{
            await AssertEx.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Client.BeginTransactionAsync(new TransactionOptions
                {
                    Durability = -1
                });
            });
		}

        // TODO: negative timeout is OK and means infinite?
        /*
        [Test]
		public async Task TestNegativeTimeout()
        {
            await AssertEx.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await Client.BeginTransactionAsync(new TransactionOptions
                {
                    Timeout = TimeSpan.FromMilliseconds(-1)
                });
            });
		}
        */

        [Test]
        public async Task InvalidToNestTransactions()
        {
            await using var context = await Client.BeginTransactionAsync();

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await using var context2 = await Client.BeginTransactionAsync();
            });
        }

        [Test]
		public async Task InvalidToCommitTwice()
        {
            await using var context = await Client.BeginTransactionAsync();

            await context.CommitAsync();

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await context.CommitAsync();
            });
		}

        [Test]
		public async Task TransactionTimesOut()
		{
            await using var context = await Client.BeginTransactionAsync(new TransactionOptions
            {
                Timeout = TimeSpan.FromMilliseconds(100)
            });

            await Task.Delay(500);

            await AssertEx.ThrowsAsync<TransactionException>(async () =>
            {
                await context.CommitAsync();
            });
		}
    }
}