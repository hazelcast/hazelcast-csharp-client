// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.CompilerServices;
using Hazelcast.Exceptions;

namespace Hazelcast.Core;

/// <summary>
/// Provides extension methods for validating arguments.
/// </summary>
internal static class ArgumentExtensions
{
    // note: ideally we'd use [CallerArgumentExpression("value")] for the name,
    // but that attribute is not supported for netstandard (eg framework builds)

    /// <summary>
    /// Ensures that a string is not null nor whitespace.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ThrowIfNullNorWhiteSpace(this string value, string name = null)
        => !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException(ExceptionMessages.NullOrEmpty, name ?? nameof(value));

    /// <summary>
    /// Ensures that an object reference is not null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TObject ThrowIfNull<TObject>(this TObject value, string name = null) where TObject : class
        => value ?? throw new ArgumentNullException(name ?? nameof(value));

    /// <summary>
    /// Ensures that a numeric value is not less than zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ThrowIfLessThanZero(this int value, string name = null)
        => value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(name ?? nameof(value), "Value cannot be negative.");

    /// <summary>
    /// Ensures that a numeric value is not less than zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ThrowIfLessThanZero(this long value, string name = null)
        => value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(name ?? nameof(value), "Value cannot be negative.");

    /// <summary>
    /// Ensures that a numeric value is not less than or equal to zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ThrowIfLessThanOrZero(this int value, string name = null)
        => value > 0
            ? value
            : throw new ArgumentOutOfRangeException(name ?? nameof(value), "Value cannot be negative nor zero.");

    /// <summary>
    /// Ensures that a numeric value is not less than or equal to zero.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ThrowIfLessThanOrZero(this long value, string name = null)
        => value > 0
            ? value
            : throw new ArgumentOutOfRangeException(name ?? nameof(value), "Value cannot be negative nor zero.");

    /// <summary>
    /// Ensures that a numeric value is within a specified range.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ThrowIfOutOfRange(this int value, int minValueInclusive, int maxValueInclusive, string name = null)
        => value >= minValueInclusive && value <= maxValueInclusive
            ? value
            : throw new ArgumentOutOfRangeException(name ?? nameof(value), $"Value cannot be out of [{minValueInclusive},{maxValueInclusive}].");
}