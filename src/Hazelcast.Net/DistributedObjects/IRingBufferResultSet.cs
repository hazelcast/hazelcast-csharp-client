// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.DistributedObjects;

/// <summary>
/// It is a <see cref="IReadOnlyList{T}"/> extension with ring buffer sequence abilities. 
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public interface IRingBufferResultSet<out T> : IReadOnlyList<T>
{
    /// <summary>
    /// Determine sequence of item at given index.
    /// </summary>
    /// <param name="index">Index of item in the list.</param>
    /// <returns>Sequence of item.</returns>
    public long GetSequence(int index);

    /// <summary>
    /// Gets sequence number of the item following the last read item.
    /// </summary>
    public long NextSequence { get; }
}
