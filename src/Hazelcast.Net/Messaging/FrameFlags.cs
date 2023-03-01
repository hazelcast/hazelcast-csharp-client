// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Defines the <see cref="Frame"/> flags.
    /// </summary>
    [Flags]
    internal enum FrameFlags
    {
        /// <summary>
        /// Default value (no flags).
        /// </summary>
        Default     = 0,

        /// <summary>
        /// All flags (mask).
        /// </summary>
        AllFlags    = 0b0011_1100_0000_0000,

        /// <summary>
        /// Flags the last frame of a message or fragment.
        /// </summary>
        Final       = 0b0010_0000_0000_0000,

        /// <summary>
        /// Flags a frame that begins a data structure.
        /// </summary>
        BeginStruct = 0b0001_0000_0000_0000,

        /// <summary>
        /// Flags a frame that ends a data structure.
        /// </summary>
        EndStruct   = 0b0000_1000_0000_0000,

        /// <summary>
        /// Flags a null frame.
        /// </summary>
        Null        = 0b0000_0100_0000_0000,
    }
}
