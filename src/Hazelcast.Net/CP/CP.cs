// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.DistributedObjects;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="ICP"/> implementation.
    /// </summary>
    internal class CP : ICP
    {
        private readonly DistributedObjectFactory _distributedOjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="CP"/> class.
        /// </summary>
        /// <param name="distributedOjects">A distributed objects factory.</param>
        public CP(DistributedObjectFactory distributedOjects)
        {
            _distributedOjects = distributedOjects;
        }

        /// <inheritdoc />
        public async Task<IAtomicLong> GetAtomicLongAsync(string name)
        {
            return await _distributedOjects.GetOrCreateAsync<IAtomicLong, AtomicLong>(ServiceNames.AtomicLong, name, true,
                (n, f, c, sr, lf)
                    => new AtomicLong(n, f, c, sr, lf));
        }
    }
}