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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.Topic
{
    /// <summary>
    /// Implements <see cref="Topic{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the message objects.</typeparam>
    internal sealed partial class Topic<T> : DistributedObjectBase, ITopic<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Topic{T}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public Topic(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(Topic.ServiceName, name, cluster, serializationService, loggerFactory)
        { }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
            Task PublishAsync(T message, TimeSpan timeout)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = PublishAsync(message, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
            Task PublishAsync(T message, CancellationToken cancellationToken)
        {
            var messageData = ToSafeData(message);
            var requestMessage = TopicPublishCodec.EncodeRequest(Name, messageData);
            var task = Cluster.SendAsync(requestMessage, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}