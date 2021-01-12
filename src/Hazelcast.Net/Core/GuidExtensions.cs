// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="Guid"/> struct.
    /// </summary>
    internal static class GuidExtensions
    {
        /// <summary>
        /// Returns a short string representation of the value of this <see cref="Guid"/> instance.
        /// </summary>
        /// <param name="guid">This guid.</param>
        /// <returns>A short string (first 7 lower-cased hexadecimal chars) representation of the value of this <see cref="Guid"/> instance.</returns>
        public static string ToShortString(this Guid guid)
            => guid.ToString("N").Substring(0, 7);
    }
}
