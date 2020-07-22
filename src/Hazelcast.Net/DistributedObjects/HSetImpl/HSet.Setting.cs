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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.HSetImpl
{
    internal partial class HSet<T> // Setting
    {
        public override async Task<bool> AddAsync(T item, CancellationToken cancellationToken = default)
        {
            var itemData = ToSafeData(item);
            var requestMessage = SetAddCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return SetAddCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken = default)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = SetAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return SetAddAllCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
