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

using Hazelcast.Client.Test;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Model
{
    internal class IdentifiedEntryProcessor : IIdentifiedDataSerializable, IEntryProcessor
    {
        internal const int ClassId = 1;

        private string value;

        public IdentifiedEntryProcessor(string value = null)
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
            return IdentifiedFactory.FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
    }
}