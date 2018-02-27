// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Hazelcast.Client.Spi;

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
        protected IDistributedObject _distributedObject;

        private readonly string _eventType;

        private readonly string _serviceName;

        private readonly string _objectName;

        protected DistributedObjectEvent(string eventType, string serviceName, string objectName)
        {
            _eventType = eventType;
            _serviceName = serviceName;
            _objectName = objectName;
        }

        /// <summary>Returns IDistributedObject instance</summary>
        /// <returns>IDistributedObject</returns>
        public virtual IDistributedObject GetDistributedObject()
        {
            return GetDistributedObject<IDistributedObject>();
        }

        /// <summary>Returns IDistributedObject instance</summary>
        /// <returns>IDistributedObject</returns>
        public virtual T GetDistributedObject<T>() where T : IDistributedObject
        {
            if (EventType.Destroyed.Equals(_eventType))
            {
                throw new DistributedObjectDestroyedException();
            }

            if (!(_distributedObject is T) )
            {
                InitDistributedObject<T>();
            }

            return (T) _distributedObject;
        }
        
        protected  virtual void InitDistributedObject<T>() where T : IDistributedObject
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns type of this event; one of
        ///     <see cref="EventType.Created">EventType.Created</see>
        ///     or
        ///     <see cref="EventType.Destroyed">EventType.Destroyed</see>
        /// </summary>
        /// <returns>eventType</returns>
        public string GetEventType()
        {
            return _eventType;
        }

        /// <summary>
        /// Return name of the source distributed object
        /// </summary>
        /// <returns>distributed object name</returns>
        public string GetObjectName()
        {
            return _objectName;
        }

        /// <summary>
        /// Return service name of the source distributed object
        /// </summary>
        /// <returns>service name</returns>
        public string GetServiceName()
        {
            return _serviceName;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("DistributedObjectEvent{");
            sb.Append("eventType=").Append(_eventType);
            sb.Append(", serviceName='").Append(_serviceName).Append('\'');
            sb.Append(", objectName='").Append(_serviceName).Append('\'');
            sb.Append(", distributedObject=").Append(_distributedObject);
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// Event types class for event name constants
        /// </summary>
        public static class EventType
        {
            /// <summary>
            /// Distributed Object Created Event
            /// </summary>
            public const string Created = "CREATED";

            /// <summary>
            /// Distributed Object Destroyed Event
            /// </summary>
            public const string Destroyed = "DESTROYED";
        }
    }

    internal class LazyDistributedObjectEvent : DistributedObjectEvent
    {
        private readonly ProxyManager _proxyManager;

        public LazyDistributedObjectEvent(string eventType, string serviceName, string name, ProxyManager proxyManager)
            : base(eventType, serviceName, name)
        {
            _proxyManager = proxyManager;
        }

        protected override void InitDistributedObject<T>()
        {
            _distributedObject = _proxyManager.GetOrCreateProxy<T>(GetServiceName(), GetObjectName());
        }
    }
}