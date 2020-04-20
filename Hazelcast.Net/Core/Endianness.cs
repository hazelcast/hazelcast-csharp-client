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

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the order of the bytes within a binary representation of a number.
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// Endianness is not specified (this is the default value).
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The native endianness.
        /// </summary>
        Native = 1,

        /// <summary>
        /// Big-endian.
        /// </summary>
        BigEndian = 2,

        /// <summary>
        /// Little-endian.
        /// </summary>
        LittleEndian = 3
    }
}