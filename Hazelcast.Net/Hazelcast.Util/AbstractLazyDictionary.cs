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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    internal abstract class AbstractLazyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected readonly ConcurrentQueue<KeyValuePair<IData, object>> _contentQueue;
        protected readonly ISerializationService _serializationService;

        protected AbstractLazyDictionary(ConcurrentQueue<KeyValuePair<IData, object>> contentQueue,
            ISerializationService serializationService)
        {
            _contentQueue = contentQueue;
            _serializationService = serializationService;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var keyData = _serializationService.ToData(item.Key);

            foreach (var pair in _contentQueue)
            {
                if (pair.Key.Equals(keyData))
                {
                    var itemValueData = _serializationService.ToData(item.Value);
                    var pairValueData = _serializationService.ToData(pair.Value);
                    return itemValueData.Equals(pairValueData);
                }
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            var keyData = _serializationService.ToData(key);
            foreach (var pair in _contentQueue)
            {
                if (pair.Key.Equals(keyData))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var ix = arrayIndex;
            using (var enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    array[ix] = enumerator.Current;
                    ix++;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(_contentQueue.GetEnumerator(), _serializationService);
        }

        public int Count
        {
            get { return _contentQueue.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Readonly dictionary");
        }

        public void Clear()
        {
            throw new NotSupportedException("Readonly dictionary");
        }


        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("Readonly dictionary");
        }


        public bool Remove(TKey key)
        {
            throw new NotSupportedException("Readonly dictionary");
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<IData, object> entry;
            if (FindEntry(key, out entry))
            {
                value = _serializationService.ToObject<TValue>(entry.Value);
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                KeyValuePair<IData, object> entry;
                if (FindEntry(key, out entry))
                {
                    return _serializationService.ToObject<TValue>(entry.Value);
                }
                throw new KeyNotFoundException();
            }
            set { throw new NotSupportedException("Readonly dictionary"); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Readonly Set");
        }

        private bool FindEntry(TKey key, out KeyValuePair<IData, object> entry)
        {
            var keyData = _serializationService.ToData(key);
            try
            {
                entry = _contentQueue.First(pair => pair.Key.Equals(keyData));
                return true;
            }
            catch (Exception)
            {
                entry = default(KeyValuePair<IData, object>);
                return false;
            }
        }

        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IEnumerator<KeyValuePair<IData, object>> _enumerator;
            private readonly ISerializationService _ss;

            internal Enumerator(IEnumerator<KeyValuePair<IData, object>> enumerator, ISerializationService ss)
            {
                _enumerator = enumerator;
                _ss = ss;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    var current = _enumerator.Current;
                    var key = _ss.ToObject<TKey>(current.Key);
                    var value = _ss.ToObject<TValue>(current.Value);
                    return new KeyValuePair<TKey, TValue>(key, value);
                }
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}