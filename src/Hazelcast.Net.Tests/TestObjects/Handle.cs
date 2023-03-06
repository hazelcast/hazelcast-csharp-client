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

using Hazelcast.Serialization;

namespace Hazelcast.Tests.TestObjects
{
    public struct Handle : IPortable
    {
        private long _handle;

        public Handle(bool isActive)
        {
            _handle = isActive ? 1L : 0L;
        }

        public bool IsActive
        {
            get { return _handle % 2L == 1L; }
        }

        public int ClassId => ClassIds.Handle;

        public int FactoryId => ClassIds.Factory;

        void IPortable.ReadPortable(IPortableReader reader)
        {
            _handle = reader.ReadLong("handle");
        }

        void IPortable.WritePortable(IPortableWriter writer)
        {
            writer.WriteLong("handle", _handle);
        }

        public bool Equals(Handle other)
        {
            return _handle == other._handle;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Handle && Equals((Handle)obj);
        }

        public override int GetHashCode()
        {
            return _handle.GetHashCode();
        }
    }
}
