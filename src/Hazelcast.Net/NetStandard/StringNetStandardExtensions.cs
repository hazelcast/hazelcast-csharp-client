﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class StringNetStandardExtensions
    {
#if NET462 || NETSTANDARD2_0
#pragma warning disable CA1801 // Review unused parameters - we need them
#pragma warning disable IDE0060 // Remove unused parameter

        public static int GetHashCode(this string s, StringComparison comparison)
            => s.GetHashCode();

        public static int IndexOf(this string s, char c, StringComparison comparison)
            => s.IndexOf(c);

        public static string Replace(this string s, string o, string r, StringComparison comparison)
            => s.Replace(o, r);

#pragma warning restore CA1801
#pragma warning restore IDE0060
#endif
    }
}
