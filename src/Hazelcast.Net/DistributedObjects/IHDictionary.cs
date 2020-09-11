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

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <remarks>
    /// <para>Keys are identified by their own hash code and equality.</para>
    /// <para>Methods return clones of the original keys and values. Modifying these clones does not change
    /// the actual keys and values in the dictionary. One should put the modified entries back, to make
    /// changes visible to all nodes.</para>
    /// </remarks>
    // ReSharper disable UnusedTypeParameter
    public partial interface IHDictionary<TKey, TValue> : IDistributedObject
    // ReSharper restore UnusedTypeParameter
    {
        // NOTES
        //
        // In most cases it would be pointless to return async enumerable since we must fetch
        // everything from the network anyways (else we'd hang the socket) before returning,
        // and therefore all that remains is CPU-bound de-serialization of data.
    }
}
