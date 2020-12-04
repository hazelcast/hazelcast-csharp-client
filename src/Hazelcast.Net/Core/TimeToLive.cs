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

#nullable enable

using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides time-to-live constants.
    /// </summary>
    public static partial class TimeToLive
    {
        /// <summary>
        /// 0ms, meaning: never expire.
        /// </summary>
        public static TimeSpan Infinite => TimeSpan.Zero;

        /// <summary>
        /// -1ms meaning: server-configured time-to-live.
        /// </summary>
        public static TimeSpan Default => TimeSpanExtensions.MinusOneMillisecond;
    }
}
