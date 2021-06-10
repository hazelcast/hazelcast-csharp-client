// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.Core
{
    internal class DictionaryBinder : IDictionary<string, string>
    {
        private readonly Action<string, string> _onItem;

        public DictionaryBinder(Action<string, string> onItem)
        {
            _onItem = onItem;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        // dotCover does not understand that GetEnumerator() throws and does not return
        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            _onItem(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotSupportedException();
        }

        public int Count => throw new NotSupportedException();

        public bool IsReadOnly => throw new NotSupportedException();

        public void Add(string key, string value)
        {
            _onItem(key, value);
        }

        public bool ContainsKey(string key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(string key)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotSupportedException();
        }

        public string this[string key]
        {
            get => throw new NotSupportedException();
            set => _onItem(key, value);
        }

        public ICollection<string> Keys => throw new NotSupportedException();

        public ICollection<string> Values => throw new NotSupportedException();
    }
}
