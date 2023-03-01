// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Models
{
    /// <summary>
    /// Specifies the collection item event types.
    /// </summary>
    [Flags]
    public enum CollectionItemEventTypes
    {
        /// <summary>
        /// The item was added.
        /// </summary>
        Added = 1, // zero is for default, make sure we start at 1

        /// <summary>
        /// The item was removed.
        /// </summary>
        Removed = 2
    }
}
