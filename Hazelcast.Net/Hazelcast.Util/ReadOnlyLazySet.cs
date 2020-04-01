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

using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    internal class ReadOnlyLazySet<T> : ISet<T>
    {
        //builtin data list
        private readonly IList<IData> list;
        private readonly ISerializationService serializationService;

        //this set receives a data list which is assumed to have unique elements
        public ReadOnlyLazySet(IList<IData> list, ISerializationService serializationService)
        {
            this.list = list;
            this.serializationService = serializationService;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReadOnlyLazyList<T, IData>.Enumerator(list, serializationService);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            var data = serializationService.ToData(item);
            return list.Contains(data);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var ix = arrayIndex;
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                array[ix]=enumerator.Current;
                ix++;
            }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
        
        public bool SetEquals(IEnumerable<T> other)
        {
            //TODO set equal implementation 
            throw new NotSupportedException("Readonly Set");
        }

        //Not supported methods to make it readonly
        public void Add(T item)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        bool ISet<T>.Add(T item)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void Clear()
        {
            throw new NotSupportedException("Readonly Set");
        }
        
        public bool Remove(T item)
        {
            throw new NotSupportedException("Readonly Set");
        }

    }
}