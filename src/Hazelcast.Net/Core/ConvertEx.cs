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

#nullable enable

using System;

namespace Hazelcast.Core;

internal class ConvertEx
{
    /// <summary>
    /// Converts an object value to a value that must not be <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The object value.</param>
    /// <returns>The value.</returns>
    /// <exception cref="InvalidOperationException">The object value is <c>null</c> or not of type <typeparamref name="T"/>.</exception>
    public static T UnboxNonNull<T>(object? value) where T : struct
    {
        return value switch
        {
            T v => v,
            null => throw new InvalidOperationException($"Cannot convert null value to type {typeof (T)}."),
            _ => throw new InvalidOperationException($"Cannot convert value of type {value.GetType()} to type {typeof (T)}.")
        };
    }

    /// <summary>
    /// Converts a nullable value to a value that must not be <c>null</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="InvalidOperationException">The value is <c>null</c>.</exception>
    public static T ValueNonNull<T>(T? value) where T : struct
    {
        if (value.HasValue) return value.Value;
        throw new InvalidOperationException($"Cannot convert null value to type {typeof(T)}.");
    }
}