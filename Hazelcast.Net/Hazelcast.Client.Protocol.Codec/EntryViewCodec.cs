/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class EntryViewCodec
    {
        private EntryViewCodec()
        {
        }

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

        public static void Encode(SimpleEntryView<IData, IData> dataEntryView, ClientMessage clientMessage)
        {
            var key = dataEntryView.GetKey();
            var value = dataEntryView.GetValue();
            var cost = dataEntryView.GetCost();
            var creationTime = dataEntryView.GetCreationTime();
            var expirationTime = dataEntryView.GetExpirationTime();
            var hits = dataEntryView.GetHits();
            var lastAccessTime = dataEntryView.GetLastAccessTime();
            var lastStoredTime = dataEntryView.GetLastStoredTime();
            var lastUpdateTime = dataEntryView.GetLastUpdateTime();
            var version = dataEntryView.GetVersion();
            var ttl = dataEntryView.GetTtl();
            var evictionCriteriaNumber = dataEntryView.GetEvictionCriteriaNumber();
            clientMessage.Set(key)
                .Set(value)
                .Set(cost)
                .Set(creationTime)
                .Set(expirationTime)
                .Set(hits)
                .Set(lastAccessTime)
                .Set(lastStoredTime)
                .Set(lastUpdateTime)
                .Set(version)
                .Set(evictionCriteriaNumber)
                .Set(ttl);
        }

        public static int CalculateDataSize(SimpleEntryView<IData, IData> entryView)
        {
            var dataSize = ClientMessage.HeaderSize;
            var key = entryView.GetKey();
            var value = entryView.GetValue();
            return dataSize + ParameterUtil.CalculateDataSize(key) + ParameterUtil.CalculateDataSize(value) +
                   Bits.LongSizeInBytes*10;
        }
    }
}