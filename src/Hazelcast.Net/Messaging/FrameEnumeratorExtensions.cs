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

using System.Collections.Generic;

namespace Hazelcast.Messaging
{
    internal static class FrameEnumeratorExtensions
    {
        /// <summary>
        /// Determines whether the frame enumerator has more non-end frames.
        /// </summary>
        /// <param name="enumerator">The enumerator.</param>
        /// <returns><c>true</c> is the enumerator can move next to a non-end frame; otherwise <c>false</c>.</returns>
        public static bool NextIsNotTheEnd(this IEnumerator<Frame> enumerator)
        {
            var current = enumerator.Current;
            if (current == null) return false; // what else?
            if (current.IsEndStruct) return false;
            var next = current.Next;
            return next != null && !next.IsEndStruct;
        }
    }
}
