// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Models;

/// <summary>
/// Represents a memory capacity.
/// </summary>
internal class Capacity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Capacity"/> class.
    /// </summary>
    /// <param name="value">The memory capacity expressed in bytes.</param>
    public Capacity(long value)
        : this(value, MemoryUnit.Bytes)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Capacity"/> class.
    /// </summary>
    /// <param name="value">The memory capacity expressed in the specified <paramref name="unit"/>.</param>
    /// <param name="unit">The memory capacity unit.</param>
    public Capacity(long value, MemoryUnit unit)
    {
        if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.", nameof(value));

        Value = value;
        Unit = unit switch
        {
            MemoryUnit.Bytes or
            MemoryUnit.KiloBytes or
            MemoryUnit.MegaBytes or 
            MemoryUnit.GigaBytes => unit,
            _ => throw new ArgumentException("Value must be a valid MemoryUnit.", nameof(unit))
        };
    }

    /// <summary>
    /// Gets the memory capacity expressed in <see cref="Unit"/>.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Gets the memory capacity unit.
    /// </summary>
    public MemoryUnit Unit { get; }

    // NOTE:
    // not implementing all the conversion logic for now
    // (see Capacity.java and MemoryUnit.java)
}
