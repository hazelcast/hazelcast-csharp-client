﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides idle-time constants.
    /// </summary>
    public static partial class IdleTime
    {
        /// <summary>
        /// An infinite idle-time (never idle).
        /// </summary>
        public static TimeSpan Infinite => TimeSpanExtensions.MinusOneMillisecond;

        /// <summary>
        /// The default idle-time (use the value configured on the server).
        /// </summary>
        public static TimeSpan Default => TimeSpan.Zero;
    }
}
