// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>Lifecycle event fired when IHazelcastInstance's state changes.</summary>
    /// <remarks>
    ///     Lifecycle event fired when IHazelcastInstance's state changes.
    ///     Events are fired when instance:
    ///     <ul>
    ///         <li>Starting</li>
    ///         <li>Started</li>
    ///         <li>Shutting down</li>
    ///         <li>Shut down completed</li>
    ///         <li>Merging</li>
    ///         <li>Merged</li>
    ///     </ul>
    /// </remarks>
    /// <seealso cref="ILifecycleListener">ILifecycleListener</seealso>
    /// <seealso cref="IHazelcastInstance.GetLifecycleService()">IHazelcastInstance.GetLifecycleService()</seealso>
    public sealed class LifecycleEvent
    {
        /// <summary>lifecycle states</summary>
        public enum LifecycleState
        {
            Starting,
            Started,
            ShuttingDown,
            Shutdown,
            Merging,
            Merged,
            ClientConnected,
            ClientDisconnected
        }

        private readonly LifecycleState _state;

        public LifecycleEvent(LifecycleState state)
        {
            _state = state;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (!(o is LifecycleEvent))
            {
                return false;
            }
            var that = (LifecycleEvent) o;
            if (_state != that._state)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return _state.GetHashCode();
        }

        public LifecycleState GetState()
        {
            return _state;
        }

        public override string ToString()
        {
            return "LifecycleEvent [state=" + _state + "]";
        }
    }
}