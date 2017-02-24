// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol
{
    public interface IClientMessage
    {
        IClientMessage AddFlag(short flags);
        bool GetBoolean();
        byte GetByte();
        long GetCorrelationId();
        IData GetData();
        int GetInt();
        long GetLong();
        KeyValuePair<IData, IData> GetMapEntry();
        int GetMessageType();
        int GetPartitionId();
        string GetStringUtf8();
        bool IsFlagSet(short listenerEventFlag);
        bool IsRetryable();

        /// <summary>Sets the correlation id field.</summary>
        /// <param name="correlationId">The value to set in the correlation id field.</param>
        /// <returns>The ClientMessage with the new correlation id field value.</returns>
        IClientMessage SetCorrelationId(long correlationId);

        IClientMessage SetPartitionId(int partitionId);
    }
}