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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation.Set
{
    internal partial class HSet<T> // Getting
    {
        public override Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> ContainsAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = SetContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return SetContainsCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = SetSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return SetSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = SetContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return SetContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> IsEmptyAsync(CancellationToken cancellationToken)
        {
            var requestMessage = SetIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return SetIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }
    }
}