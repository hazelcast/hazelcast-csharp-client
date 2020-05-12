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

using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation;

namespace Hazelcast
{
    // partial: distributed objects
    internal partial class HazelcastClient
    {
        // TODO: implement HazelcastClient access to other Distributed Objects

        /// <summary>
        /// Gets an <see cref="IMap{TKey,TValue}"/> distributed object.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="name">The unique name of the map.</param>
        /// <returns>A task that will complete when the map has been retrieved or created,
        /// and represents the map that has been retrieved or created.</returns>
#if DEBUG // maintain full stack traces
        public async Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => await GetDistributedObjectAsync<IMap<TKey, TValue>>(Constants.ServiceNames.Map, name);
#else
        public Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => GetDistributedObjectAsync<IMap<TKey,TValue>>(Constants.ServiceNames.Map, name);
#endif

        /// <summary>
        /// Gets a distributed object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <returns>A task that will complete when the object has been retrieved or created,
        /// and represents the object that has been retrieved or created.</returns>
#if DEBUG // maintain full stack traces
        private async ValueTask<TObject> GetDistributedObjectAsync<TObject>(string serviceName, string name)
            where TObject : IDistributedObject
            => await _distributedObjectFactory.GetOrCreateAsync<TObject>(serviceName, name);
#else
        private ValueTask<T> GetDistributedObjectAsync<T>(string serviceName, string name)
            where T : IDistributedObject
            => _distributedObjectFactory.GetOrCreateAsync<T>(serviceName, name);
#endif
    }
}