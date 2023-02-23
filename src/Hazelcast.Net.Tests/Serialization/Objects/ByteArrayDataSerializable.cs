// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    public class ByteArrayDataSerializable : IIdentifiedDataSerializable
    {
        private byte[] _data;

        public ByteArrayDataSerializable()
        { }

        public ByteArrayDataSerializable(byte[] data)
        {
            _data = data;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(_data.Length);
            output.Write(_data);
        }

        public int FactoryId => SerializationTestsConstants.DATA_SERIALIZABLE_FACTORY_ID;

        public int ClassId => SerializationTestsConstants.BYTE_ARRAY_DATA_SERIALIZABLE_ID;

        public void ReadData(IObjectDataInput input)
        {
            var len = input.ReadInt();
            _data = new byte[len];
            input.Read(_data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ByteArrayDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            return (_data != null ? _data.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"Data: {string.Join(", ", _data)}";
        }

        protected bool Equals(ByteArrayDataSerializable other)
        {
            return _data.SequenceEqual(other._data);
        }
    }
}
