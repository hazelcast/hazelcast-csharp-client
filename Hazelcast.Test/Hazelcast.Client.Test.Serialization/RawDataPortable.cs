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

using System.Linq;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class RawDataPortable : IPortable
    {
        protected char[] c;
        protected int k;
        protected long l;
        protected NamedPortable p;
        protected string s;
        protected ByteArrayDataSerializable sds;

        public RawDataPortable()
        {
        }

        public RawDataPortable(long l, char[] c, NamedPortable p, int k, string s, ByteArrayDataSerializable sds)
        {
            this.l = l;
            this.c = c;
            this.p = p;
            this.k = k;
            this.s = s;
            this.sds = sds;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public virtual int GetClassId()
        {
            return TestSerializationConstants.RAW_DATA_PORTABLE;
        }

        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("l", l);
            writer.WriteCharArray("c", c);
            writer.WritePortable("p", p);
            var output = writer.GetRawDataOutput();
            output.WriteInt(k);
            output.WriteUTF(s);
            output.WriteObject(sds);
        }

        public virtual void ReadPortable(IPortableReader reader)
        {
            l = reader.ReadLong("l");
            c = reader.ReadCharArray("c");
            p = reader.ReadPortable<NamedPortable>("p");
            var input = reader.GetRawDataInput();
            k = input.ReadInt();
            s = input.ReadUTF();
            sds = input.ReadObject<ByteArrayDataSerializable>();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RawDataPortable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (c != null ? c.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ k;
                hashCode = (hashCode*397) ^ l.GetHashCode();
                hashCode = (hashCode*397) ^ (p != null ? p.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (s != null ? s.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (sds != null ? sds.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("C: {0}, K: {1}, L: {2}, P: {3}, S: {4}, Sds: {5}", c, k, l, p, s, sds);
        }

        protected bool Equals(RawDataPortable other)
        {
            return c.SequenceEqual(other.c) && k == other.k && l == other.l && Equals(p, other.p) &&
                   string.Equals(s, other.s) &&
                   Equals(sds, other.sds);
        }
    }
}