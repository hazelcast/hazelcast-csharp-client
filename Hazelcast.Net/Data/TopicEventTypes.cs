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

using System;

namespace Hazelcast.Data
{
    /// <summary>
    /// Specifies the topic event types.
    /// </summary>
    [Flags]
    public enum TopicEventTypes
    {
        /// <summary>
        /// Nothing (default value).
        /// </summary>
        Nothing = 0,

        /// <summary>
        /// A message was received.
        /// </summary>
        Message = 1
    }
}
