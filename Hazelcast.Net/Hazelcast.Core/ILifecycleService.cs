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

namespace Hazelcast.Core
{
    /// <summary>
    ///     ILifecycleService allows you to shutdown, terminate and listen to
    ///     <see cref="LifecycleEvent">LifecycleEvent</see>
    ///     's
    ///     on IHazelcastInstance.
    /// </summary>
    public interface ILifecycleService
    {
        /// <summary>Add listener object to listen lifecycle events.</summary>
        /// <remarks>Add listener object to listen lifecycle events.</remarks>
        /// <param name="lifecycleListener">Listener object</param>
        /// <returns>listener id</returns>
        string AddLifecycleListener(ILifecycleListener lifecycleListener);

        /// <summary>whether the instance is running</summary>
        /// <returns>true if instance is active and running</returns>
        bool IsRunning();

        /// <summary>Remove lifecycle listener</summary>
        /// <param name="registrationId">
        ///     The listener id returned by
        ///     <see cref="AddLifecycleListener(ILifecycleListener)">AddLifecycleListener(ILifecycleListener)</see>
        /// </param>
        /// <returns>true if removed successfully</returns>
        bool RemoveLifecycleListener(string registrationId);

        /// <summary>gracefully shutdowns IHazelcastInstance.</summary>
        /// <remarks>
        ///     gracefully shutdowns IHazelcastInstance. Different from
        ///     <see cref="Terminate()">Terminate()</see>
        ///     , waits partition operations to be completed.
        /// </remarks>
        void Shutdown();

        /// <summary>terminate IHazelcastInstance ungracefully.</summary>
        /// <remarks>terminate IHazelcastInstance ungracefully. Does not wait partition operations, forces immediate shutdown.</remarks>
        void Terminate();
    }
}