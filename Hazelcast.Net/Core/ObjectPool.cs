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
using System.Collections.Generic;

namespace Hazelcast.Core
{
    // TODO: refactor this entirely with Serialization
    // this class is *not* thread safe and probably not efficient either
    internal class ObjectPool<T>
        where T : class
    {
        private readonly Queue<T> _queue;
        private readonly int _maxItems;
        private readonly Func<T> _factory;
        private readonly Action<T> _clear;

        public ObjectPool(int maxItems, Func<T> factory, Action<T> clear)
        {
            _maxItems = maxItems;
            _factory = factory;
            _clear = clear;
            _queue = new Queue<T>(_maxItems);
        }

        public T Take()
        {
            try
            {
                return _queue.Dequeue();
            }
            catch
            {
                return _factory();
            }
        }

        public void Return(T item)
        {
            if (item == default) return;

            _clear(item);

            if (_queue.Count == _maxItems)
            {
                if (item is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception)
                    {
                        // why don't we just throw
                        // instead of logging & swallowing?
                        throw;
                    }
                }

                return;
            }

            // is there a race condition here?

            _queue.Enqueue(item);
        }
    }
}
