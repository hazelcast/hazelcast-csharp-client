// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    internal class ReadOnlyLazyList<T, D> : IList<T> where D : class
    {
        private readonly IList<D> _content;
        private readonly ISerializationService serializationService;

        public ReadOnlyLazyList(IList<D> content, ISerializationService serializationService)
        {
            this._content = content;
            this.serializationService = serializationService;
        }

        #region IList<T>

        public IEnumerator<T> GetEnumerator()
        {
            return DeserializeIterator(_content, serializationService);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException("Readonly lazy list");
        }

        public void Clear()
        {
            throw new NotSupportedException("Readonly lazy list");
        }

        public bool Contains(T item)
        {
            var itemData = serializationService.ToData(item);
            foreach (var dValue in _content)
            {
                if (dValue is IData)
                {
                    if (itemData.Equals(dValue))
                        return true;
                }
                else
                {
                    if (itemData.Equals(serializationService.ToData(dValue)))
                        return true;
                }
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var ix = arrayIndex;
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                array[ix] = enumerator.Current;
                ix++;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("Readonly list");
        }

        public int Count => _content.Count;

        public bool IsReadOnly => true;

        public int IndexOf(T item)
        {
            var ix = _content.IndexOf(item as D);
            if (ix < 0)
            {
                var data = serializationService.ToData(item);
                return _content.IndexOf(data as D);
            }
            return ix;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Readonly list");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("Readonly list");
        }

        public T this[int index]
        {
            get => serializationService.ToObject<T>(_content[index]);
            set => throw new NotSupportedException("Readonly list");
        }

        #endregion
        
        internal static IEnumerator<T> DeserializeIterator(IEnumerable source, ISerializationService ss)
        {
            foreach (var item in source)
            {
                yield return ss.ToObject<T>(item);
            }
        }
    }
}