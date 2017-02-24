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

using System;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Examples.Models
{

    /// <summary>
    /// Hazezelcast-test.jar has the same EntryProcessor to be used on server side
    ///  named <c>com.hazelcast.client.test.IdentifiedEntryProcessor</c>
    /// </summary>
    public class UpdateEntryProcessor : IEntryProcessor, IIdentifiedDataSerializable
    {
        public const int ClassId = 1;

        private string value;

        public UpdateEntryProcessor(string value=null)
        {
            this.value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            value = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(value);
        }

        public int GetFactoryId()
        {
            return EntryProcessorDataSerializableFactory.FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
    }


    public class EntryProcessorDataSerializableFactory : IDataSerializableFactory
    {
        public const int FactoryId = 66;

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if(typeId == UpdateEntryProcessor.ClassId)
            {
                    return new UpdateEntryProcessor();
            }
            throw new InvalidOperationException("Unknown type id");
        }
    }
}