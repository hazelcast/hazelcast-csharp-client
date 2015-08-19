using System;
using System.Text;

namespace Hazelcast.Core
{
    /// <summary>
    ///     DistributedObjectEvent is fired when a
    ///     <see cref="IDistributedObject">IDistributedObject</see>
    ///     is created or destroyed cluster-wide.
    /// </summary>
    /// <seealso cref="IDistributedObject">IDistributedObject</seealso>
    /// <seealso cref="IDistributedObjectListener">IDistributedObjectListener</seealso>
    public class DistributedObjectEvent
    {
        
        public static class EventType
        {
            public const string Created = "CREATED";
            public const string Destroyed = "DESTROYED";
        }

        private readonly IDistributedObject distributedObject;

        private readonly String eventType;

        private readonly string serviceName;

        public DistributedObjectEvent(String eventType, string serviceName, IDistributedObject distributedObject)
        {
            this.eventType = eventType;
            this.serviceName = serviceName;
            this.distributedObject = distributedObject;
        }

        /// <summary>
        ///     Returns type of this event; one of
        ///     <see cref="EventType.Created">EventType.Created</see>
        ///     or
        ///     <see cref="EventType.Destroyed">EventType.Destroyed</see>
        /// </summary>
        /// <returns>eventType</returns>
        public virtual String GetEventType()
        {
            return eventType;
        }

        /// <summary>Returns IDistributedObject instance</summary>
        /// <returns>IDistributedObject</returns>
        public virtual IDistributedObject GetDistributedObject()
        {
            return distributedObject;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("DistributedObjectEvent{");
            sb.Append("eventType=").Append(eventType);
            sb.Append(", serviceName='").Append(serviceName).Append('\'');
            sb.Append(", distributedObject=").Append(distributedObject);
            sb.Append('}');
            return sb.ToString();
        }
    }
}