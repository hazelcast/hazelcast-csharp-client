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
using System.Threading.Tasks;
using Hazelcast.Data;

namespace Hazelcast.DistributedObjects
{
    internal sealed class DictionaryEntryExpiredEventHandler<TKey, TValue, TSender> : DictionaryEntryEventHandlerBase<TKey, TValue, TSender, DictionaryEntryExpiredEventArgs<TKey, TValue>>
    {
        public DictionaryEntryExpiredEventHandler(Func<TSender, DictionaryEntryExpiredEventArgs<TKey, TValue>, ValueTask> handler)
            : base(HDictionaryEventTypes.Expired, handler)
        { }

        protected override DictionaryEntryExpiredEventArgs<TKey, TValue> CreateEventArgs(MemberInfo member, Lazy<TKey> key, Lazy<TValue> value, Lazy<TValue> oldValue, Lazy<TValue> mergeValue, HDictionaryEventTypes eventType, int numberOfAffectedEntries, object state)
            => new DictionaryEntryExpiredEventArgs<TKey, TValue>(member, key, oldValue, state);
    }
}
