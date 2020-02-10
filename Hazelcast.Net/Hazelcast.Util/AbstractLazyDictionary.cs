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
    internal abstract class AbstractLazyDictionary<TKey, TValue, D> : IEnumerable<KeyValuePair<TKey, TValue>> where D : class
    {
        protected readonly IList<KeyValuePair<IData, D>> _content;
        protected readonly ISerializationService _serializationService;

        protected AbstractLazyDictionary(IList<KeyValuePair<IData, D>> content, ISerializationService serializationService)
        {
            _content = content;
            _serializationService = serializationService;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var keyData = _serializationService.ToData(item.Key);

            foreach (var pair in _content)
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
            return _content.Any(pair => pair.Key.Equals(keyData));
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
            return DeserializeIterator(_content, _serializationService);
        }

        public int Count => _content.Count;

        public bool IsReadOnly => true;

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
            if (FindEntry(key, out var entry))
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
                if (FindEntry(key, out var entry))
                {
                    return _serializationService.ToObject<TValue>(entry.Value);
                }
                throw new KeyNotFoundException();
            }
            set => throw new NotSupportedException("Readonly dictionary");
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Readonly Set");
        }

        private bool FindEntry(TKey key, out KeyValuePair<IData, D> entry)
        {
            var keyData = _serializationService.ToData(key);
            try
            {
                entry = _content.First(pair => pair.Key.Equals(keyData));
                return true;
            }
            catch (Exception)
            {
                entry = default(KeyValuePair<IData, D>);
                return false;
            }
        }

        private static IEnumerator<KeyValuePair<TKey, TValue>> DeserializeIterator(IEnumerable<KeyValuePair<IData, D>> source,
            ISerializationService ss)
        {
            foreach (var kvp in source)
            {
                var key = ss.ToObject<TKey>(kvp.Key);
                var value = ss.ToObject<TValue>(kvp.Value);
                yield return new KeyValuePair<TKey, TValue>(key, value);
            }
        }
    }
}