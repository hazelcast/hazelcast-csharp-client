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

namespace Hazelcast.DistributedObjects.Implementation.List
{
    internal partial class HList<T> // Remove
    {
        /// <inheritdoc />
        public override async Task<bool> RemoveAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListRemoveCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<T> RemoveAtAsync(int index, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = RemoveAtAsync(index, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> RemoveAtAsync(int index, CancellationToken cancellationToken)
        {
            var requestMessage = ListRemoveWithIndexCodec.EncodeRequest(Name, index);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = ListRemoveWithIndexCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public override async Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListCompareAndRemoveAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListCompareAndRemoveAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListCompareAndRetainAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return ListCompareAndRetainAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task ClearAsync(CancellationToken cancellationToken)
        {
            var requestMessage = ListClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            _ = ListClearCodec.DecodeResponse(responseMessage);
        }
    }
}