// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using System;

namespace Hazelcast.Models;

/// <summary>
/// Represents a memory capacity.
/// </summary>
public class Capacity
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

        Value = value.ThrowIfLessThanOrZero();
        Unit = unit.ThrowIfUndefined();
    }

    /// <summary>
    /// Gets the memory capacity expressed in <see cref="Unit"/>.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Gets the memory capacity unit.
    /// </summary>
    public MemoryUnit Unit { get; }

    /// <summary>
    /// Gets the value of the capacity in bytes.
    /// </summary>
    public long Bytes => MemoryUnitExtensions.Convert(MemoryUnit.Bytes, Unit, Value);

    /// <summary>
    /// Gets the value of the capacity in kilo-bytes.
    /// </summary>
    public long KiloBytes => MemoryUnitExtensions.Convert(MemoryUnit.KiloBytes, Unit, Value);

    /// <summary>
    /// Gets the value of the capacity in mega-bytes.
    /// </summary>
    public long MegaBytes => MemoryUnitExtensions.Convert(MemoryUnit.MegaBytes, Unit, Value);

    /// <summary>
    /// Gets the value of the capacity in giga-bytes.
    /// </summary>
    public long GigaBytes => MemoryUnitExtensions.Convert(MemoryUnit.GigaBytes, Unit, Value);

    /// <summary>
    /// Creates a new instance of the <see cref="Capacity"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="unit">The unit.</param>
    /// <returns>The new <see cref="Capacity"/> instance.</returns>
    public static Capacity Of(long value, MemoryUnit unit) => new Capacity(value, unit);

    /// <summary>
    /// Parses the string representation of a capacity.
    /// </summary>
    /// <param name="value">The string representation of a capacity.</param>
    /// <returns>The capacity.</returns>
    public static Capacity Parse(string value) => Parse(value, MemoryUnit.Bytes);

    /// <summary>
    /// Parses the string representation of a capacity.
    /// </summary>
    /// <param name="value">The string representation of a capacity.</param>
    /// <param name="defaultUnit">The unit to use if none is specified in the string.</param>
    /// <returns>The capacity.</returns>
    public static Capacity Parse(string value, MemoryUnit defaultUnit)
    {
        if (string.IsNullOrWhiteSpace(value)) return new Capacity(0, MemoryUnit.Bytes);

        var unitChar = value[^1];
        var hasUnit = !char.IsDigit(unitChar);

        var unit = defaultUnit;
        if (hasUnit)
            unit = unitChar switch
            {
                'b' or 'B' => MemoryUnit.Bytes,
                'k' or 'K' => MemoryUnit.KiloBytes,
                'm' or 'M' => MemoryUnit.MegaBytes,
                'g' or 'G' => MemoryUnit.GigaBytes,
                _ => throw new ArgumentException($"Invalid unit specifier '{unitChar}'.", nameof(value))
            };

        return new Capacity(long.Parse(hasUnit ? value[..^1] : value), unit);
    }

    /// <summary>
    /// Formats this capacity.
    /// </summary>
    /// <returns>The formatted capacity.</returns>
    public string ToPrettyString() => ToPrettyString(Value, Unit);

    /// <inheritdoc />
    public override string ToString() => $"{Value} {Unit}";

    /// <summary>
    /// Formats the capacity.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    /// <returns>The formatted capacity.</returns>
    public static string ToPrettyString(long capacity)
    {
        return ToPrettyString(capacity, MemoryUnit.Bytes);
    }

    /// <summary>
    /// Formats the capacity.
    /// </summary>
    /// <param name="capacity">The capacity.</param>
    /// <param name="unit">The capacity unit.</param>
    /// <returns>The formatted capacity.</returns>
    public static string ToPrettyString(long capacity, MemoryUnit unit)
    {
        // following Java's pattern

        var bytes = MemoryUnitExtensions.Convert(MemoryUnit.Bytes, unit, capacity);
        return bytes switch
        {
            >= 10_000_000_000 => To(MemoryUnit.GigaBytes),
            >= 10_000_000 => To(MemoryUnit.MegaBytes),
            >= 10_000 => To(MemoryUnit.KiloBytes),
            _ => To(MemoryUnit.Bytes)
        };

        string To(MemoryUnit toUnit) => $"{MemoryUnitExtensions.Convert(toUnit, unit, capacity)} {toUnit.Abbrev()}";
    }
}
