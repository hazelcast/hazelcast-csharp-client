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
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Hazelcast.Core
{
    // We want to have:
    //   IdleTime
    //   LeaseTime
    //   TimeOut
    //   TimeToLive
    //   TimeToWait
    //
    // which each define their own constants (default, max, infinite...) and
    // also have FromSeconds(...) etc methods so it looks nice in code - they
    // cannot inherit from TimeSpan (a struct) and if we make them inherit
    // static methods from a base class, we get annoying "access to a static
    // member of a type via a derived type" in users' code, and so we end up
    // duplicating things here, which is certainly not pretty, but?

    public partial class IdleTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromDays(double value)
            => TimeSpan.FromDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromHours(double value)
            => TimeSpan.FromHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMilliseconds(double value)
            => TimeSpan.FromMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMinutes(double value)
            => TimeSpan.FromMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromSeconds(double value)
            => TimeSpan.FromSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromTicks(long value)
            => TimeSpan.FromTicks(value);

#pragma warning disable CA1305 // Specify IFormatProvider
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string s)
            => TimeSpan.Parse(s);
#pragma warning restore CA1305

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string input, IFormatProvider? formatProvider)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, formats, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);

#if NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider? formatProvider = null)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);
#endif
    }

    public partial class LeaseTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromDays(double value)
            => TimeSpan.FromDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromHours(double value)
            => TimeSpan.FromHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMilliseconds(double value)
            => TimeSpan.FromMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMinutes(double value)
            => TimeSpan.FromMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromSeconds(double value)
            => TimeSpan.FromSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromTicks(long value)
            => TimeSpan.FromTicks(value);

#pragma warning disable CA1305 // Specify IFormatProvider
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string s)
            => TimeSpan.Parse(s);
#pragma warning restore CA1305

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string input, IFormatProvider? formatProvider)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, formats, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);

#if NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider? formatProvider = null)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);
#endif
    }

    public partial class TimeOut
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromDays(double value)
            => TimeSpan.FromDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromHours(double value)
            => TimeSpan.FromHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMilliseconds(double value)
            => TimeSpan.FromMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMinutes(double value)
            => TimeSpan.FromMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromSeconds(double value)
            => TimeSpan.FromSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromTicks(long value)
            => TimeSpan.FromTicks(value);

#pragma warning disable CA1305 // Specify IFormatProvider
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string s)
            => TimeSpan.Parse(s);
#pragma warning restore CA1305

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string input, IFormatProvider? formatProvider)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, formats, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);

#if NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider? formatProvider = null)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);
#endif
    }

    public partial class TimeToLive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromDays(double value)
            => TimeSpan.FromDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromHours(double value)
            => TimeSpan.FromHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMilliseconds(double value)
            => TimeSpan.FromMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMinutes(double value)
            => TimeSpan.FromMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromSeconds(double value)
            => TimeSpan.FromSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromTicks(long value)
            => TimeSpan.FromTicks(value);

#pragma warning disable CA1305 // Specify IFormatProvider
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string s)
            => TimeSpan.Parse(s);
#pragma warning restore CA1305

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string input, IFormatProvider? formatProvider)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, formats, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);

#if NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider? formatProvider = null)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);
#endif
    }

    public partial class TimeToWait
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromDays(double value)
            => TimeSpan.FromDays(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromHours(double value)
            => TimeSpan.FromHours(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMilliseconds(double value)
            => TimeSpan.FromMilliseconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromMinutes(double value)
            => TimeSpan.FromMinutes(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromSeconds(double value)
            => TimeSpan.FromSeconds(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan FromTicks(long value)
            => TimeSpan.FromTicks(value);

#pragma warning disable CA1305 // Specify IFormatProvider
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string s)
            => TimeSpan.Parse(s);
#pragma warning restore CA1305

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(string input, IFormatProvider? formatProvider)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, format, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider)
            => TimeSpan.ParseExact(input, formats, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string format, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(string input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);

#if NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan Parse(ReadOnlySpan<char> input, IFormatProvider? formatProvider = null)
            => TimeSpan.Parse(input, formatProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, format, formatProvider, styles);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan ParseExact(ReadOnlySpan<char> input, string[] formats, IFormatProvider? formatProvider, TimeSpanStyles styles = TimeSpanStyles.None)
            => TimeSpan.ParseExact(input, formats, formatProvider, styles);
#endif
    }

}