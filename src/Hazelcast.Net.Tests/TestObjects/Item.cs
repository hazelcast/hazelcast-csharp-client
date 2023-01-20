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

namespace Hazelcast.Tests.TestObjects
{
    public class Item : IPortable
    {
        private int[] _disabled;
        private int[] _enabled;
        private Header _header;

        internal Item()
        { }

        public Item(Header header, int[] enabled, int[] disabled)
        {
            _header = header;
            _enabled = enabled;
            _disabled = disabled;
        }

        public Header Header => _header;

        public int ClassId => ClassIds.Item;

        public int FactoryId => ClassIds.Factory;

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _header = reader.ReadPortable<Header>("header");
            _enabled = reader.ReadIntArray("enabled");
            _disabled = reader.ReadIntArray("disabled");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WritePortable("header", _header);
            writer.WriteIntArray("enabled", _enabled);
            writer.WriteIntArray("disabled", _disabled);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Item)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _header.GetHashCode();
                hashCode = (hashCode * 397) ^ (_enabled != null ? _enabled.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_disabled != null ? _disabled.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(Item other)
        {
            return _header.Equals(other._header) && _enabled.SequenceEqual(other._enabled) &&
                   _disabled.SequenceEqual(other._disabled);
        }
    }
}
