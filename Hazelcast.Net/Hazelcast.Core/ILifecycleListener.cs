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

namespace Hazelcast.Core
{
    /// <summary>Listener object for listening lifecycle events of hazelcast instance</summary>
    /// <seealso cref="LifecycleEvent">LifecycleEvent</seealso>
    /// <seealso cref="IHazelcastInstance.GetLifecycleService()">IHazelcastInstance.GetLifecycleService()</seealso>
    public interface ILifecycleListener : IEventListener
    {
        /// <summary>Called when instance's state changes</summary>
        /// <param name="lifecycleEvent">Lifecycle event</param>
        void StateChanged(LifecycleEvent lifecycleEvent);
    }
}