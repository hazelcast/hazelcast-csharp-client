// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    public class ComplexDataSerializable : IIdentifiedDataSerializable
    {
        private IIdentifiedDataSerializable _ds;
        private IIdentifiedDataSerializable _ds2;
        private IPortable _portable;

        public ComplexDataSerializable()
        { }

        public ComplexDataSerializable(IPortable portable, IIdentifiedDataSerializable ds, IIdentifiedDataSerializable ds2)
        {
            _portable = portable;
            _ds = ds;
            _ds2 = ds2;
        }

        public void ReadData(IObjectDataInput input)
        {
            _ds = input.ReadObject<IIdentifiedDataSerializable>();
            _portable = input.ReadObject<IPortable>();
            _ds2 = input.ReadObject<IIdentifiedDataSerializable>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(_ds);
            output.WriteObject(_portable);
            output.WriteObject(_ds2);
        }

        public int FactoryId => SerializationTestsConstants.DATA_SERIALIZABLE_FACTORY_ID;

        public int ClassId => SerializationTestsConstants.COMPLEX_DATA_SERIALIZABLE_ID;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ComplexDataSerializable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_ds != null ? _ds.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_ds2 != null ? _ds2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_portable != null ? _portable.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(ComplexDataSerializable other)
        {
            return Equals(_ds, other._ds) && Equals(_ds2, other._ds2) && Equals(_portable, other._portable);
        }
    }
}
