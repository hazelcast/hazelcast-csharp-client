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
    internal class NamedPortable : IPortable
    {
        internal string name;
        internal int k;

        public NamedPortable()
        { }

        public NamedPortable(string name, int k)
        {
            this.name = name;
            this.k = k;
        }

        public int FactoryId => SerializationTestsConstants.PORTABLE_FACTORY_ID;

        public int ClassId => SerializationTestsConstants.NAMED_PORTABLE;

        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteInt("myint", k);
        }

        public virtual void ReadPortable(IPortableReader reader)
        {
            k = reader.ReadInt("myint");
            name = reader.ReadUTF("name");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NamedPortable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0)*397) ^ k;
            }
        }

        public override string ToString()
        {
            return string.Format("K: {0}, Name: {1}", k, name);
        }

        protected bool Equals(NamedPortable other)
        {
            return string.Equals(name, other.name) && k == other.k;
        }
    }
}
