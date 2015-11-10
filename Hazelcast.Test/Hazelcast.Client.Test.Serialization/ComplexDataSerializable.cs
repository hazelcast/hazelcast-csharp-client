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
    public class ComplexDataSerializable : IDataSerializable
    {
        private IDataSerializable ds;
        private IDataSerializable ds2;
        private IPortable portable;

        public ComplexDataSerializable()
        {
        }

        public ComplexDataSerializable(IPortable portable, IDataSerializable ds, IDataSerializable ds2)
        {
            this.portable = portable;
            this.ds = ds;
            this.ds2 = ds2;
        }

        public void ReadData(IObjectDataInput input)
        {
            ds = input.ReadObject<IDataSerializable>();
            portable = input.ReadObject<IPortable>();
            ds2 = input.ReadObject<IDataSerializable>();
        }

        public string GetJavaClassName()
        {
            return typeof (ComplexDataSerializable).Name;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(ds);
            output.WriteObject(portable);
            output.WriteObject(ds2);
        }

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
                var hashCode = (ds != null ? ds.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ds2 != null ? ds2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (portable != null ? portable.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(ComplexDataSerializable other)
        {
            return Equals(ds, other.ds) && Equals(ds2, other.ds2) && Equals(portable, other.portable);
        }
    }
}