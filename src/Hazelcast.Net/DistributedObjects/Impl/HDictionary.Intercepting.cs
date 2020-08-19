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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HDictionary<TKey, TValue> // Intercepting
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public Task<string> AddInterceptorAsync(IDictionaryInterceptor interceptor)
            => AddInterceptorAsync(interceptor, CancellationToken.None);

        private async Task<string> AddInterceptorAsync(IDictionaryInterceptor interceptor, CancellationToken cancellationToken)
        {
            var interceptorData = ToSafeData(interceptor);

            var requestMessage = MapAddInterceptorCodec.EncodeRequest(Name, interceptorData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapAddInterceptorCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task RemoveInterceptorAsync(string id)
            => RemoveInterceptorAsync(id, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task RemoveInterceptorAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(id));

            var requestMessage = MapRemoveInterceptorCodec.EncodeRequest(Name, id);
            var task = Cluster.Messaging.SendAsync(requestMessage, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}
