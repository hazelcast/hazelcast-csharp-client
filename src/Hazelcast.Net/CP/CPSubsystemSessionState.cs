// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Hazelcast.Core;

namespace Hazelcast.CP
{
    /// <summary>
    /// Represents the Session State on CPSubsystem
    /// </summary>
    internal class CPSubsystemSessionState
    {
        private readonly long _id;
        private int _acquireCount = 0;

        private readonly long _creationTime;
        private readonly long _ttlMiliseconds;

        public CPSubsystemSessionState(long id, long ttlMiliseconds)
        {
            _id = id;
            _ttlMiliseconds = ttlMiliseconds;
            _creationTime = Clock.Milliseconds;
        }

        /// <summary>
        /// Gets whether session is valid.
        /// </summary>
        public bool IsValid => IsInUse || !IsExpired;

        /// <summary>
        /// Gets whether session is active.
        /// </summary>
        public bool IsInUse => _acquireCount > 0;

        /// <summary>
        /// Gets whether session is expired.
        /// </summary>
        public bool IsExpired => IsSessionExpired(Clock.Milliseconds);

        /// <summary>
        /// Gets Session Id
        /// </summary>
        public long Id => _id;

        /// <summary>
        /// Checks session is expired to given milliseconds
        /// </summary>
        /// <param name="timestamp">milliseonds</param>
        /// <returns>Is session expired</returns>
        private bool IsSessionExpired(long timestamp)
        {
            long expiration = _creationTime + _ttlMiliseconds;

            if (expiration < 0)
                expiration = long.MaxValue;

            return timestamp > expiration;
        }

        /// <summary>
        /// Increases the acquire count
        /// </summary>
        /// <param name="count">Acquire count to increase</param>
        /// <returns>Session Id</returns>
        public long Acquire(int count)
        {
            Interlocked.Add(ref _acquireCount, count);
            return _id;
        }

        /// <summary>
        /// Decreases the acquire count
        /// </summary>
        /// <param name="count">Acquire count to decrease</param>
        /// <returns>Session Id</returns>
        public long Release(int count)
        {
            int minusCount = -count;
            Interlocked.Add(ref _acquireCount, minusCount);
            return _id;
        }

        /// <summary>
        /// Gets acquire count
        /// </summary>
        public int AcquireCount => _acquireCount;

        /// <inheritdoc/>     
        public override bool Equals(object obj)
        {
            return obj is CPSubsystemSessionState state &&
                   _id == state._id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
