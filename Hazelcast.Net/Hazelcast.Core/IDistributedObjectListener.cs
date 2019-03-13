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

namespace Hazelcast.Core
{
    /// <summary>
    ///     IDistributedObjectListener allows to get notified when a
    ///     <see cref="IDistributedObject">IDistributedObject</see>
    ///     is created or destroyed cluster-wide.
    /// </summary>
    /// <seealso cref="IDistributedObject">IDistributedObject</seealso>
    /// <seealso cref="IHazelcastInstance.AddDistributedObjectListener(IDistributedObjectListener)">IHazelcastInstance.AddDistributedObjectListener(IDistributedObjectListener)</seealso>
    public interface IDistributedObjectListener : IEventListener
    {
        /// <summary>Invoked when a IDistributedObject is created.</summary>
        /// <remarks>Invoked when a IDistributedObject is created.</remarks>
        /// <param name="event">event</param>
        void DistributedObjectCreated(DistributedObjectEvent @event);

        /// <summary>Invoked when a IDistributedObject is destroyed.</summary>
        /// <remarks>Invoked when a IDistributedObject is destroyed.</remarks>
        /// <param name="event">event</param>
        void DistributedObjectDestroyed(DistributedObjectEvent @event);
    }
}