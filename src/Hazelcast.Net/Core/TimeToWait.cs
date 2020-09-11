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

using System;
using System.Threading;

namespace Hazelcast.Core
{
    /// <summary>
    /// Utilities for managing time-to-wait.
    /// </summary>
    public static class TimeToWait
    {
        /// <summary>
        /// A constants used to specify an infinite time-to-wait (wait forever).
        /// </summary>
        public static readonly TimeSpan InfiniteTimeSpan = Timeout.InfiniteTimeSpan;

        /// <summary>
        /// A constants used to specify a zero time-to-wait (do not wait).
        /// </summary>
        public static readonly TimeSpan Zero = TimeSpan.Zero;
    }
}
