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
using Hazelcast.Configuration;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // partial: indexing
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task AddIndexAsync(IndexType type, params string[] attributes)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            await AddIndexAsync(new IndexConfig { Type = type, Attributes = attributes });
        }

        /// <inheritdoc />
        public async Task AddIndexAsync(IndexConfig indexConfig)
        {
            if (indexConfig == null) throw new ArgumentNullException(nameof(indexConfig));

            var requestMessage = MapAddIndexCodec.EncodeRequest(Name, indexConfig.ValidateAndNormalize(Name));
            await Cluster.SendAsync(requestMessage);
        }
    }
}