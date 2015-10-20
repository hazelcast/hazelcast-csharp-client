/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Concurrent, distributed implementation of <see cref="IList{T}"/>IList 
    /// </summary>
    public interface IHList<E> : IList<E>, IHCollection<E>
    {
        E Get(int index);
        E Set(int index, E element);

        void Add(int index, E element);

        E Remove(int index);

        int LastIndexOf(E o);

        bool AddAll<_T0>(int index, ICollection<_T0> c) where _T0 : E;

        IList<E> SubList(int fromIndex, int toIndex);

        //int IndexOf(E item);
        //void Insert(int index, E item);
        //void RemoveAt(int index);
        //E this[int index] { get; set; }
    }
}