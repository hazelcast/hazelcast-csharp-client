// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Tests.TestObjects
{
    public struct Header : IPortable
    {
        private long _id;
        private Handle _handle;

        public Header(long id, Handle handle)
        {
            _id = id;
            _handle = handle;
        }

        public long Id => _id;

        public Handle Handle => _handle;

        public int ClassId => ClassIds.Header;

        public int FactoryId => ClassIds.Factory;

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _id = reader.ReadLong("id");
            _handle = reader.ReadPortable<Handle>("handle");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("id", _id);
            writer.WritePortable("handle", _handle);
        }

        public bool Equals(Header other)
        {
            return _id == other._id && _handle.Equals(other._handle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Header && Equals((Header)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_id.GetHashCode() * 397) ^ _handle.GetHashCode();
            }
        }
    }
}
