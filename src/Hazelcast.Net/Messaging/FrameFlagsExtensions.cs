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

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="FrameFlags"/> enumeration.
    /// </summary>
    public static class FrameFlagsExtensions
    {
        /// <summary>
        /// Converts the value of a <see cref="FrameFlags"/> instance to its equivalent string representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A better string representation of the value that what <see cref="FrameFlags.ToString()"/> would return.</returns>
        /// <remarks>
        /// <para>The same field holds both <see cref="FrameFlags"/> and <see cref="ClientMessageFlags"/>, this
        /// method returns a better string representation by dealing with both enumerations at once.</para>
        /// </remarks>
        public static string ToBetterString(this FrameFlags value)
        {
            var frameFlags = value & FrameFlags.AllFlags;
            var messagFlags = (ClientMessageFlags) value & ClientMessageFlags.AllFlags;

            var s1 = frameFlags > 0 ? frameFlags.ToString() : null;
            var s2 = messagFlags > 0 ? messagFlags.ToString() : null;

            return s1 == null && s2 == null
                ? "Default"
                : s1 + (s1 == null || s2 == null ? "" : ", ") + s2;
        }

        /// <summary>
        /// Converts the value of a <see cref="ClientMessageFlags"/> instance to its equivalent string representation.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A better string representation of the value that what <see cref="ClientMessageFlags.ToString()"/> would return.</returns>
        /// <remarks>
        /// <para>The same field holds both <see cref="FrameFlags"/> and <see cref="ClientMessageFlags"/>, this
        /// method returns a better string representation by dealing with both enumerations at once.</para>
        /// </remarks>
        public static string ToBetterString(this ClientMessageFlags value)
            => ((FrameFlags) value).ToString();
    }
}
