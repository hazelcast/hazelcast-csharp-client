// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec
{
    internal static class EntryViewCodec
    {
        public static SimpleEntryView<IData, IData> Decode(IClientMessage clientMessage)
        {
            var dataEntryView = new SimpleEntryView<IData, IData>();
            dataEntryView.SetKey(clientMessage.GetData());
            dataEntryView.SetValue(clientMessage.GetData());
            dataEntryView.SetCost(clientMessage.GetLong());
            dataEntryView.SetCreationTime(clientMessage.GetLong());
            dataEntryView.SetExpirationTime(clientMessage.GetLong());
            dataEntryView.SetHits(clientMessage.GetLong());
            dataEntryView.SetLastAccessTime(clientMessage.GetLong());
            dataEntryView.SetLastStoredTime(clientMessage.GetLong());
            dataEntryView.SetLastUpdateTime(clientMessage.GetLong());
            dataEntryView.SetVersion(clientMessage.GetLong());
            dataEntryView.SetEvictionCriteriaNumber(clientMessage.GetLong());
            dataEntryView.SetTtl(clientMessage.GetLong());
            return dataEntryView;
        }
    }
}