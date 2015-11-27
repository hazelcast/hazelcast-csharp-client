// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    public class DataDataSerializable : IIdentifiedDataSerializable
    {
        internal IData Data;

        public DataDataSerializable()
        {
        }

        public DataDataSerializable(IData data)
        {
            Data = data;
        }

        public void ReadData(IObjectDataInput input)
        {
            Data = input.ReadData();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteData(Data);
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.DATA_SERIALIZABLE_FACTORY_ID;
        }

        public int GetId()
        {
            return TestSerializationConstants.DATA_DATA_SERIALIZABLE_ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            return (Data != null ? Data.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Data: {0}", Data);
        }

        protected bool Equals(DataDataSerializable other)
        {
            return Equals(Data, other.Data);
        }
    }
}