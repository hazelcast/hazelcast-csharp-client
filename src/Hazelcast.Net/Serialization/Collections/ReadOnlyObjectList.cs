﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Serialization.Collections
{
    internal sealed class ReadOnlyObjectList<T>: IReadOnlyList<object>
    {
        private readonly IList<T> _list;

        public int Count => _list.Count;

        public ReadOnlyObjectList(IList<T> list)
        {
            _list = list;
        }

        // If T is struct, this leads to boxing on each enumeration
        public IEnumerator<object> GetEnumerator() => _list.Cast<object>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // If T is struct, this leads to boxing on each access
        public object this[int index] => _list[index];
    }
}
