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
using System.Threading;

#nullable enable

namespace Hazelcast.Core
{
    // this is equivalent to MS' DefaultObjectPool with the default policy
    // a fixed-size object pool

    internal class SimpleObjectPool<T>
        where T : class
    {
        private readonly Func<T> _create;
        private readonly ObjectWrapper[] _items;
        private T? _item;

        public SimpleObjectPool(Func<T> create, int size)
        {
            _create = create;
            _items = new ObjectWrapper[size - 1];
        }

        public T Get()
        {
            var item = _item;
            if (item == null || Interlocked.CompareExchange(ref _item, null, item) != item)
            {
                var items = _items;
                for (var i = 0; i < items.Length; i++)
                {
                    item = items[i].Item;
                    if (item != null && Interlocked.CompareExchange(ref items[i].Item, null, item) == item)
                    {
                        return item;
                    }
                }

                item = _create();
            }

            return item;
        }

        public void Return(T obj)
        {
            if (_item != null || Interlocked.CompareExchange(ref _item, obj, null) != null)
            {
                var items = _items;
                for (var i = 0; i < items.Length && Interlocked.CompareExchange(ref items[i].Item, obj, null) != null; ++i)
                { }
            }
        }

        // borrowed from MS' DefaultObjectPool
        // PERF: the struct wrapper avoids array-covariance-checks from the runtime when assigning to elements of the array.
        private protected struct ObjectWrapper
        {
            public T? Item;
        }
    }
}
