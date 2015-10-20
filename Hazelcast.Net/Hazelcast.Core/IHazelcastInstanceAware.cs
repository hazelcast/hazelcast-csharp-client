/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Core
{
    /// <summary>Used to get IHazelcastInstance reference when submitting a Runnable/Callable using Hazelcast ExecutorService.</summary>
    /// <remarks>
    ///     Used to get IHazelcastInstance reference when submitting a Runnable/Callable using Hazelcast ExecutorService.
    ///     Before executing the Runnable/Callable Hazelcast will invoke
    ///     <see cref="SetHazelcastInstance(IHazelcastInstance)">SetHazelcastInstance(IHazelcastInstance)</see>
    ///     method with
    ///     the reference to IHazelcastInstance that is executing. This way the implementer will have a chance to get the
    ///     reference to IHazelcastInstance.
    /// </remarks>
    public interface IHazelcastInstanceAware
    {
        void SetHazelcastInstance(IHazelcastInstance hazelcastInstance);
    }
}