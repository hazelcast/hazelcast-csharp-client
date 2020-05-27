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

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Defines common exception messages.
    /// </summary>
    public static class ExceptionMessages
    {
        /// <summary>
        /// Not enough bytes.
        /// </summary>
        public const string NotEnoughBytes = "Not enough bytes.";

        /// <summary>
        /// Invalid target.
        /// </summary>
        public const string InvalidTarget = "Invalid target.";

        /// <summary>
        /// Value cannot be null nor empty.
        /// </summary>
        public const string NullOrEmpty = "Value cannot be null nor empty.";

        /// <summary>
        /// Cached value is not of the expected type.
        /// </summary>
        public const string InvalidCacheCast = "Cached value is not of the expected type.";

        /// <summary>
        /// Property is now read only and cannot be modified.
        /// </summary>
        public const string PropertyIsNowReadOnly = "The property is now readonly and cannot be modified.";
    }
}
