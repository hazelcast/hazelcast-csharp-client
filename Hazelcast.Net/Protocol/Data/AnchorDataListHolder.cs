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

using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Data
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

        internal IEnumerable<KeyValuePair<int, KeyValuePair<object, object>>> AsAnchorIterator(ISerializationService serializationService)
        {
            var dataEntryIterator = AnchorDataList.GetEnumerator();
            dataEntryIterator.MoveNext();
            foreach (var pageNumber in AnchorPageList)
            {
                var dataEntry = dataEntryIterator.Current;
                var key = serializationService.ToObject(dataEntry.Key);
                var value = serializationService.ToObject(dataEntry.Value);
                var entry = new KeyValuePair<object, object>(key, value);
                yield return new KeyValuePair<int, KeyValuePair<object, object>>(pageNumber, entry);
            }
        }
    }
}