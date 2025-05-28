﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models
{
    internal class AnchorDataListHolder
    {
        public AnchorDataListHolder(IList<int> anchorPageList, IList<KeyValuePair<IData, IData>> anchorDataList)
        {
            AnchorPageList = anchorPageList;
            AnchorDataList = anchorDataList;
        }

        internal IList<int> AnchorPageList { get; }

        internal IList<KeyValuePair<IData, IData>> AnchorDataList { get; }

        internal IEnumerable<KeyValuePair<int, KeyValuePair<object, object>>> AsAnchorIterator(SerializationService serializationService)
        {
            using var dataEntryIterator = AnchorDataList.GetEnumerator();
            foreach (var pageNumber in AnchorPageList)
            {
                dataEntryIterator.MoveNext();

                var (keyData, valueData) = dataEntryIterator.Current;

                var key = serializationService.ToObject(keyData);
                var value = serializationService.ToObject(valueData);

                var entry = new KeyValuePair<object, object>(key, value);
                yield return new KeyValuePair<int, KeyValuePair<object, object>>(pageNumber, entry);
            }
        }

        internal async IAsyncEnumerable<KeyValuePair<int, KeyValuePair<object, object>>> AsAnchorAsyncIterator(SerializationService serializationService)
        {
            using var dataEntryIterator = AnchorDataList.GetEnumerator();
            foreach (var pageNumber in AnchorPageList)
            {
                dataEntryIterator.MoveNext();

                var (keyData, valueData) = dataEntryIterator.Current;

                if (!serializationService.TryToObject<object>(keyData, out var key, out var keyToObjectState))
                    key = await serializationService.ToObjectAsync<object>(keyData, keyToObjectState);
                if (!serializationService.TryToObject<object>(valueData, out var value, out var valueToObjectState))
                    key = await serializationService.ToObjectAsync<object>(valueData, valueToObjectState);

                var entry = new KeyValuePair<object, object>(key, value);
                yield return new KeyValuePair<int, KeyValuePair<object, object>>(pageNumber, entry);
            }
        }
    }
}
