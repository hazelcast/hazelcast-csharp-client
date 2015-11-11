// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
        private readonly IDistributedObject _distributedObject;

        private readonly string _eventType;

        private readonly string _serviceName;

        public DistributedObjectEvent(string eventType, string serviceName, IDistributedObject distributedObject)
        {
            _eventType = eventType;
            _serviceName = serviceName;
            _distributedObject = distributedObject;
        }

        /// <summary>Returns IDistributedObject instance</summary>
        /// <returns>IDistributedObject</returns>
        public virtual IDistributedObject GetDistributedObject()
        {
            return _distributedObject;
        }

        /// <summary>
        ///     Returns type of this event; one of
        ///     <see cref="EventType.Created">EventType.Created</see>
        ///     or
        ///     <see cref="EventType.Destroyed">EventType.Destroyed</see>
        /// </summary>
        /// <returns>eventType</returns>
        public virtual string GetEventType()
        {
            return _eventType;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("DistributedObjectEvent{");
            sb.Append("eventType=").Append(_eventType);
            sb.Append(", serviceName='").Append(_serviceName).Append('\'');
            sb.Append(", distributedObject=").Append(_distributedObject);
            sb.Append('}');
            return sb.ToString();
        }

        public static class EventType
        {
            public const string Created = "CREATED";
            public const string Destroyed = "DESTROYED";
        }
    }
}