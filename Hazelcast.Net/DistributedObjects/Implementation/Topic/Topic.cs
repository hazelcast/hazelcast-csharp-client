﻿using System;
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
    /// Provides constants for the topic type.
    /// </summary>
    internal class Topic
    {
        /// <summary>
        /// Gets the service name.
        /// </summary>
        public const string ServiceName = "hz:impl:topicService";
    }

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
            await task.ConfigureAwait(false);
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
            await task.ConfigureAwait(false);
#endif
        }
    }
}
