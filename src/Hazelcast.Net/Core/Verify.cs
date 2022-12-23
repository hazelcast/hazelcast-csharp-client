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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Hazelcast.Exceptions;

namespace Hazelcast.Core;

/// <summary>
/// Verifies assertions for "impossible" situations that should never occur and that we want to exclude from coverage.
/// </summary>
internal static class Verify
{
    /// <summary>
    /// Verifies that a condition is met, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="condition">The condition to verify, and should be met.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="args">A object array that contains zero or more objects to format in <paramref name="message"/>.</param>
    /// <remarks>
    /// <para>This method is excluded from code coverage. Use it only for "impossible"
    /// situations that should never occur and that we want to exclude from coverage.</para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static void Condition<TException>([DoesNotReturnIf(false)] bool condition, string? message = null, params object[] args)
        where TException : Exception
    {
        if (!condition) Throw<TException>(message, args);
    }

    /// <summary>
    /// Verifies that an object is of a specified type and returns the cast object, otherwise throws an exception.
    /// </summary>
    /// <typeparam name="T">The expected type of the object.</typeparam>
    /// <typeparam name="TException">The type of the exception.</typeparam>
    /// <param name="o">The object.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="args">A object array that contains zero or more objects to format in <paramref name="message"/>.</param>
    /// <returns>The cast object.</returns>
    /// <remarks>
    /// <para>This method is excluded from code coverage. Use it only for "impossible"
    /// situations that should never occur and that we want to exclude from coverage.</para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static T MustBe<T, TException>(object? o, string? message = null, params object[] args)
        where TException : Exception
    {
        if (o is T x) return x;
        Throw<TException>(message, args);

        // unreachable but [DoesNotReturn] is only for nullable, C#
        // has no way to indicate that Throw<> does not ever ever return
        return default;
    }

    [DoesNotReturn]
    private static void  Throw<TException>(string? message, params object[] args)
    {
        if (message == null)
        {
            var ctor = typeof(TException).GetConstructor(Array.Empty<Type>());

            if (ctor == null) throw new HazelcastException(
                $"{typeof(TException).Name}: {message}. In addition, no proper constructor was found for this exception type.");

            throw (Exception)ctor.Invoke(Array.Empty<object>());
        }
        else
        {
#pragma warning disable CA1305 // Specify IFormatProvider - since .NET 7
            if (args.Length > 0) message = string.Format(message, args);
#pragma warning restore CA1305 // Specify IFormatProvider

            var ctor = typeof(TException).GetConstructor(new[] { typeof(string) });

            if (ctor == null) throw new HazelcastException(
                $"{typeof(TException).Name}: {message}. In addition, no proper constructor was found for this exception type.");

            throw (Exception)ctor.Invoke(new object[] { message });
        }
    }
}