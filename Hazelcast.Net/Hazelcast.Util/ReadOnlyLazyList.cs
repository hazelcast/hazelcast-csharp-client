// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
    public class ReadOnlyLazyList<T> : IList<T>
    {
        private readonly IList<IData> list;
        private readonly ISerializationService serializationService;

        public ReadOnlyLazyList(IList<IData> list, ISerializationService serializationService)
        {
            this.list = list;
            this.serializationService = serializationService;
        }

        #region IList<T>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(list, serializationService);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException("Readonly list");
        }

        public void Clear()
        {
            throw new NotSupportedException("Readonly list");
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

        public bool Remove(T item)
        {
            throw new NotSupportedException("Readonly list");
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

        public int IndexOf(T item)
        {
            var data = serializationService.ToData(item);
            return list.IndexOf(data);
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
            get
            {
                return serializationService.ToObject<T>(list[index]);
            }
            set
            {
                throw new NotSupportedException("Readonly list");
            }
        }
        #endregion

        internal struct Enumerator : IEnumerator<T>
        {
            private readonly IList<IData> _dataList;
            private readonly ISerializationService _ss;
            private int _nextIndex;
            private IData _current;

            internal Enumerator(IList<IData> dataList, ISerializationService ss)
            {
                _dataList = dataList;
                _ss = ss;
                _nextIndex = 0;
                _current = null;
            }

            object IEnumerator.Current
            {
                get
                {
                    if( _nextIndex == 0 || _nextIndex == _dataList.Count + 1) {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            public T Current
            {
                get
                {
                    return _ss.ToObject<T>(_current);
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var localList = _dataList;

                if ((uint)_nextIndex < (uint)localList.Count)
                {
                    _current = localList[_nextIndex];
                    _nextIndex++;
                    return true;
                }
                _nextIndex = _dataList.Count + 1;
                _current = null;
                return false;
            }

            void IEnumerator.Reset()
            {
                _nextIndex = 0;
                _current = null;
            }
        }
    }


}
