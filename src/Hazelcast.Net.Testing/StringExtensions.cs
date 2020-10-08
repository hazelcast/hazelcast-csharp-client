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

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts all cr/lf in this string to lf.
        /// </summary>
        /// <param name="s">This string.</param>
        /// <returns>The converted string.</returns>
        public static string ToLf(this string s)
        {
            return s.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        /// <summary>
        /// Appends a string to this string, with a dot separator.
        /// </summary>
        /// <param name="s">This string.</param>
        /// <param name="dotted">The string to append.</param>
        /// <returns>This string, with the dotted string appended.</returns>
        public static string Dot(this string s, string dotted)
        {
            return string.IsNullOrWhiteSpace(dotted)
                ? s
                : s + "." + dotted;
        }
    }
}
