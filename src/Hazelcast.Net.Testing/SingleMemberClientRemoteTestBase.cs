// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    public class SingleMemberClientRemoteTestBase : SingleMemberRemoteTestBase
    {
        [OneTimeSetUp]
        public async Task ClientOneTimeSetUp()
        {
            Client = await CreateAndStartClientAsync().CAF();
        }

        [OneTimeTearDown]
        public async Task ClientOneTimeTearDown()
        {
            if (Client == null) return;

            await Client.DisposeAsync();
            Client = null;
        }

        /// <summary>
        /// Gets the Hazelcast client.
        /// </summary>
        public IHazelcastClient Client { get; private set; }

        /// <summary>
        /// Gets a disposable object that will destroy and dispose a distributed
        /// object when disposed.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>A disposable object.</returns>
        protected IAsyncDisposable DestroyAndDispose(IDistributedObject o)
        {
            return new AsyncDisposable(async () =>
            {
                await Client.DestroyAsync(o);
                await o.DisposeAsync();
            });
        }
    }
}
