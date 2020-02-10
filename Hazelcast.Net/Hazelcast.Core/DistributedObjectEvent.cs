// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
        public DistributedEventType EventType { get; }

        public string ServiceName { get; }

        public string ObjectName { get; }
        public Guid Source { get; }

        private IDistributedObject _distributedObject;

        private readonly ProxyManager _proxyManager;

        internal DistributedObjectEvent(DistributedEventType eventType, string serviceName, string objectName,
            IDistributedObject distributedObject, Guid source, ProxyManager proxyManager)
        {
            EventType = eventType;
            ServiceName = serviceName;
            ObjectName = objectName;
            _distributedObject = distributedObject;
            Source = source;
            _proxyManager = proxyManager;
        }

        public T GetDistributedObject<T>() where T : IDistributedObject
        {
            if (DistributedEventType.DESTROYED == EventType)
            {
                throw new DistributedObjectDestroyedException();
            }

            if (!(_distributedObject is T))
            {
                _distributedObject = _proxyManager.GetOrCreateProxy<T>(ServiceName, ObjectName);
            }

            return (T) _distributedObject;
        }

        public override string ToString()
        {
            return
                $"DistributedObjectEvent{{ eventType={EventType}, serviceName='{ServiceName}',  objectName='{ObjectName}', source={Source}}}";
        }

        public enum DistributedEventType
        {
            CREATED,
            DESTROYED
        }
    }
}