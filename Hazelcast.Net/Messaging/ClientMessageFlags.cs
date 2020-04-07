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

namespace Hazelcast.Messaging
{
    [Flags]
    public enum ClientMessageFlags : ushort
    {
        /// <summary>
        /// Default value (all flags).
        /// </summary>
        Default       = 0,

        /// <summary>
        /// All flags (mask).
        /// </summary>
        AllFlags      = 0b1100_0011_1000_0000,

        /// <summary>
        /// Flags the first fragment of a fragmented message.
        /// </summary>
        BeginFragment = 0b1000_0000_0000_0000,

        /// <summary>
        /// Flags the last fragment of a a fragmented message.
        /// </summary>
        EndFragment   = 0b0100_0000_0000_0000,

        /// <summary>
        /// Flags an un-fragmented message.
        /// </summary>
        Unfragmented  = BeginFragment | EndFragment,

        /// <summary>
        /// Flags an event message.
        /// </summary>
        Event         = 0b0000_0010_0000_0000,

        /// <summary>
        /// Flags a backup-aware message.
        /// </summary>
        BackupAware   = 0b0000_0001_0000_0000,

        /// <summary>
        /// Flags an event message.
        /// </summary>
        BackupEvent   = 0b0000_0000_1000_0000
    }
}