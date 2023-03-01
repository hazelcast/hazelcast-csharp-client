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

namespace Hazelcast.Models;

/// <summary>
/// Represents a memory size unit.
/// </summary>
internal enum MemoryUnit
{
    /// <summary>
    /// Memory size in bytes.
    /// </summary>
    Bytes = 0,

    /// <summary>
    /// Memory size in kilobytes.
    /// </summary>
    KiloBytes = 1,

    /// <summary>
    /// Memory size in megabytes.
    /// </summary>
    MegaBytes = 2,

    /// <summary>
    /// MemorySize in gigabytes.
    /// </summary>
    GigaBytes = 3
}
