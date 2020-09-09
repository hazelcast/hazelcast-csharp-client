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

using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    internal class NamedPortableV2 : NamedPortable, IVersionedPortable
    {
        public int v;

        public NamedPortableV2()
        { }

        public NamedPortableV2(string name, int k, int v) : base(name, k)
        {
            this.v = v;
        }


        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("v", v);
        }

        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            if (reader.HasField("v"))
            {
                v = reader.ReadInt("v");
            }
        }

        public int ClassVersion => 2;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() == typeof (NamedPortable)) return base.Equals((NamedPortable) obj);
            if (obj.GetType() != GetType()) return false;
            return Equals((NamedPortableV2) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ v;
            }
        }

        protected bool Equals(NamedPortableV2 other)
        {
            return base.Equals(other) && v == other.v;
        }
    }
}