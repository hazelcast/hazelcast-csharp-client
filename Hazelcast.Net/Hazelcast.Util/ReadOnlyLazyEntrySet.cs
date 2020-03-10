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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    internal class ReadOnlyLazyEntrySet<TKey, TValue, D> : AbstractLazyDictionary<TKey, TValue, D>,
        ISet<KeyValuePair<TKey, TValue>> where D : class
    {
        public ReadOnlyLazyEntrySet(IList<KeyValuePair<IData, D>> content, ISerializationService serializationService) : base(
            content, serializationService)
        {
        }

        public bool SetEquals(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            foreach (var pair in other)
            {
                if (!Contains(pair))
                {
                    return false;
                }
            }
            return other == this || other.All(Contains);
        }

        public void ExceptWith(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void IntersectWith(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsProperSubsetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsProperSupersetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsSubsetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool IsSupersetOf(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public bool Overlaps(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void SymmetricExceptWith(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        public void UnionWith(IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            throw new NotSupportedException("Readonly Set");
        }

        bool ISet<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("Readonly Set");
        }
    }
}