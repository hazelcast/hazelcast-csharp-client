﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Events
{
    /// <summary>
    /// Represents event data for the connection added event.
    /// </summary>
    internal class ConnectionOpenedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpenedEventArgs"/> class.
        /// </summary>
        /// <param name="isFirst">Whether the connection is the first one.</param>
        public ConnectionOpenedEventArgs(bool isFirst)
        {
            IsFirst = isFirst;
        }

        /// <summary>
        /// Whether the connection is the first one.
        /// </summary>
        /// <returns><c>true</c> if the connection is the first one; otherwise <c>false</c>.</returns>
        public bool IsFirst { get; }
    }
}
