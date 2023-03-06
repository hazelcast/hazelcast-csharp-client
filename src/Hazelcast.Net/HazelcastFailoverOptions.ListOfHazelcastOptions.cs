// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast;

public sealed partial class HazelcastFailoverOptions
{
    // implements IList<HazelcastOptions> making sure that an IServiceProvider can be propagated to all items
    // - if the IServiceProvider is assigned to the IList<>, it is propagated to all items
    // - if an item is added to the IList<>, the IServiceProvider is assigned to it

    private class ListOfHazelcastOptions : IList<HazelcastOptions>
    {
        private readonly List<HazelcastOptions> _list = new();
        private IServiceProvider _serviceProvider;

        public ListOfHazelcastOptions()
        { }

        public ListOfHazelcastOptions(IEnumerable<HazelcastOptions> source, IServiceProvider serviceProvider = null)
        {
            _list.AddRange(source);
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider
        {
            get => _serviceProvider;
            set
            {
                _serviceProvider = value;
                foreach (var item in _list) item.ServiceProvider = value;
            }
        }


        /// <inheritdoc />
        public IEnumerator<HazelcastOptions> GetEnumerator() => _list.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public void Add(HazelcastOptions item)
        {
            if (item != null) item.ServiceProvider = _serviceProvider;
            _list.Add(item);
        }

        /// <inheritdoc />
        public void Clear() => _list.Clear();

        public bool Contains(HazelcastOptions item) => _list.Contains(item);

        /// <inheritdoc />
        public void CopyTo(HazelcastOptions[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(HazelcastOptions item) => _list.Remove(item);

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsReadOnly => ((IList<HazelcastOptions>)_list).IsReadOnly;

        /// <inheritdoc />
        public int IndexOf(HazelcastOptions item) => _list.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, HazelcastOptions item)
        {
            if (item != null) item.ServiceProvider = _serviceProvider;
            _list.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index) => _list.RemoveAt(index);

        /// <inheritdoc />
        public HazelcastOptions this[int index]
        {
            get => _list[index];
            set
            {
                if (value != null) value.ServiceProvider = _serviceProvider;
                _list[index] = value;
            }
        }
    }
}
