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

using Hazelcast.Core;

namespace Hazelcast.Map
{
    internal class SimpleEntryView<TKey, TValue> : SimpleEntryView, IEntryView<TKey, TValue>
    {
        public new TKey Key
        {
            get => (TKey) base.Key;
            set => base.Key = value;
        }

        public new TValue Value
        {
            get => (TValue) base.Value;
            set => base.Value= value;
        }
    }

    internal class SimpleEntryView
    {
        public long Cost { get; set; }
        public long CreationTime { get; set; }
        public long EvictionCriteriaNumber { get; set; }
        public long ExpirationTime { get; set; }
        public long Hits { get; set; }
        public object Key { get; set; }
        public long LastAccessTime { get; set; }
        public long LastStoredTime { get; set; }
        public long LastUpdateTime { get; set; }
        public long Ttl { get; set; }
        public object Value { get; set; }
        public long Version { get; set; }
        public long MaxIdle { get; set; }
    }
}