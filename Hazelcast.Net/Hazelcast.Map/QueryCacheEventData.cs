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
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Map
{
    public class QueryCacheEventData
    {
        private object key;
        private object value;

        public QueryCacheEventData()
        {
            CreationTime = Clock.CurrentTimeMillis();
        }

        public IData DataKey { get; set; }
        public IData DataNewValue { get; set; }
        public IData DataOldValue { get; set; }
        public int PartitionId { get; set; }
        public long CreationTime { get; }
        public long Sequence { get; set; }
        public int EventType { get; set; }
        public ISerializationService SerializationService { set; private get; }

        public object Key
        {
            get
            {
                if (key == null && DataKey != null)
                {
                    key = SerializationService.ToObject(DataKey);
                }

                return key;
            }
            set { this.key = value; }
        }

        public object Value
        {
            get
            {
                if (value == null && DataNewValue != null)
                {
                    value = SerializationService.ToObject(DataNewValue);
                }

                return value;
            }
            set { this.value = value; }
        }

        public override string ToString()
        {
            return $"{nameof(PartitionId)}: {PartitionId}, " +
                   $"{nameof(CreationTime)}: {CreationTime}, " +
                   $"{nameof(Sequence)}: {Sequence}, " +
                   $"{nameof(EventType)}: {EventType}";
        }
    }
}