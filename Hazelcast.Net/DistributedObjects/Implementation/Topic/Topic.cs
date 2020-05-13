using Hazelcast.Clustering;
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
        public void Publish(T message)
        {
            var messageData = ToSafeData(message);
            var requestMessage = TopicPublishCodec.EncodeRequest(Name, messageData);
            _ = Cluster.SendAsync(requestMessage);
        }
    }
}
