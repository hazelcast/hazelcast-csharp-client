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
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation
{
    // partial: intercepting
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<string> AddInterceptorAsync(IMapInterceptor interceptor)
        {
            var interceptorData = ToSafeData(interceptor);

            var requestMessage = MapAddInterceptorCodec.EncodeRequest(Name, interceptorData);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapAddInterceptorCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task RemoveInterceptorAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(id));

            var requestMessage = MapRemoveInterceptorCodec.EncodeRequest(Name, id);
            await Cluster.SendAsync(requestMessage);
        }
    }
}