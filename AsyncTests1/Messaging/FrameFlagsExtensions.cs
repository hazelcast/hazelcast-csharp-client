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

namespace AsyncTests1.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="FrameFlags"/> enumeration.
    /// </summary>
    public static class FrameFlagsExtensions
    {
        /// <summary>
        /// Determines whether all the specified flags are set in the current instance.
        /// </summary>
        /// <param name="value">The instance.</param>
        /// <param name="flags">The flags</param>
        /// <returns>True if all specified flags are set.</returns>
        public static bool Has(this FrameFlags value, FrameFlags flags)
            // Enum.HasFlag is slower
            => ((ushort) value & (ushort) flags) == (ushort) flags;
    }
}