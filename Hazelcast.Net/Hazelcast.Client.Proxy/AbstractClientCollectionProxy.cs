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
using System.Linq;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal abstract class AbstractClientCollectionProxy<T> : ClientProxy, IHCollection<T>
    {
        protected AbstractClientCollectionProxy(string serviceName, string objectName) : base(serviceName, objectName)
        {
        }

        public abstract string AddItemListener(IItemListener<T> listener, bool includeValue);
        public abstract bool RemoveItemListener(string registrationId);
        public abstract int Size();
        public abstract bool IsEmpty();
        public abstract bool ContainsAll<TE>(IEnumerable<TE> c);
        public abstract bool RemoveAll<TE>(IEnumerable<TE> c);
        public abstract bool RetainAll<TE>(IEnumerable<TE> c);
        public abstract bool AddAll<TE>(IEnumerable<TE> c);
        public abstract bool Add(T item);
        public abstract void Clear();
        public abstract bool Contains(T item);
        public abstract bool Remove(T item);

        public virtual IEnumerator<T> GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual T[] ToArray()
        {
            return GetAll().ToArray();
        }

        public virtual TE[] ToArray<TE>(TE[] a)
        {
            var array = GetAll().ToArray();
            if (a.Length < array.Length)
            {
                a = new TE[array.Length];
                Array.Copy(array, 0, a, 0, array.Length);
                return a;
            }

            Array.Copy(array, 0, a, 0, array.Length);
            if (a.Length > array.Length)
            {
                a[array.Length] = default(TE);
            }
            return a;
        }

        public virtual void CopyTo(T[] array, int index)
        {
            ValidationUtil.ThrowExceptionIfNull(array);
            if (index < 0) throw new IndexOutOfRangeException("Index cannot be negative.");

            var all = GetAll();
            if ((array.Length - index) < all.Count)
            {
                throw new ArgumentException("The number of source elements is greater" +
                                            " than the available space from index to the end of the destination array.");
            }

            var i = index;
            foreach (var item in all)
            {
                array[i++] = item;
            }
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public virtual int Count
        {
            get { return Size(); }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        protected abstract ICollection<T> GetAll();

        protected void HandleItemListener(IData itemData, string uuid, ItemEventType eventType,
            IItemListener<T> listener, bool includeValue)
        {
            var item = includeValue
                ? ToObject<T>(itemData)
                : default(T);
            var member = GetContext().GetClusterService().GetMember(uuid);
            var itemEvent = new ItemEvent<T>(GetName(), eventType, item, member);
            if (eventType == ItemEventType.Added)
            {
                listener.ItemAdded(itemEvent);
            }
            else
            {
                listener.ItemRemoved(itemEvent);
            }
        }

        protected override TE Invoke<TE>(IClientMessage request, Func<IClientMessage, TE> decodeResponse)
        {
            return base.Invoke(request, GetPartitionKey(), decodeResponse);
        }

        protected override IClientMessage Invoke(IClientMessage request)
        {
            return base.Invoke(request, GetPartitionKey());
        }
    }
}