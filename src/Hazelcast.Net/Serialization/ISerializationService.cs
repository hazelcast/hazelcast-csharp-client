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
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;

namespace Hazelcast.Serialization
{
    public interface ISerializationService : IDisposable
    {
        IBufferObjectDataInput CreateObjectDataInput(byte[] data);
        IBufferObjectDataInput CreateObjectDataInput(IData data);
        IBufferObjectDataOutput CreateObjectDataOutput(int size);
        IPortableReader CreatePortableReader(IData data);
        void Destroy();
        void DisposeData(IData data);
        Endianness Endianness { get; }
        IPortableContext GetPortableContext();
        byte GetVersion();
        T ReadObject<T>(IObjectDataInput input);
        IData ToData(object obj);
        IData ToData(object obj, IPartitioningStrategy strategy);
        T ToObject<T>(object data);
        object ToObject(object data);
        void WriteObject(IObjectDataOutput output, object obj);
        IBufferObjectDataOutput CreateObjectDataOutput();
    }
}
