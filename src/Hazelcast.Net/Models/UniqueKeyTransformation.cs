﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Models
{
    /// <summary>
    /// Defines transformations which can be applied to <see cref="BitmapIndexOptions.UniqueKey"/> values.
    /// </summary>
    public enum UniqueKeyTransformation
    {
#pragma warning disable CA1720 // Identifier contains type name - well, yes

        /// <summary>
        /// The unique key value is interpreted as an object value.
        /// Non-negative unique ID is assigned to every distinct object value.
        /// </summary>
        Object = 0,

        /// <summary>
        /// The unique key value is interpreted as a whole integer value of
        /// byte, short, int or long type. The extracted value is upcasted to
        /// long (if necessary) and unique non-negative ID is assigned to every
        /// distinct value.
        /// </summary>
        Long = 1,

        /// <summary>
        /// The unique key value is interpreted as a whole integer value of
        /// byte, short, int or long type. The extracted value is upcasted to
        /// long (if necessary) and the resulting value is used directly as an ID.
        /// </summary>
        Raw = 2

#pragma warning restore CA1720
    }
}
