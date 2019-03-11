﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class ObjectCarryingPortable : IPortable
    {
        private object _object;

        public ObjectCarryingPortable()
        {
        }

        public ObjectCarryingPortable(object @object)
        {
            _object = @object;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public int GetClassId()
        {
            return TestSerializationConstants.OBJECT_CARRYING_PORTABLE;
        }

        public void WritePortable(IPortableWriter writer)
        {
            var output = writer.GetRawDataOutput();
            output.WriteObject(_object);
        }

        public void ReadPortable(IPortableReader reader)
        {
            var input = reader.GetRawDataInput();
            _object = input.ReadObject<object>();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ObjectCarryingPortable) obj);
        }

        public override int GetHashCode()
        {
            return (_object != null ? _object.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("O: {0}", _object);
        }

        protected bool Equals(ObjectCarryingPortable other)
        {
            return Equals(_object, other._object);
        }
    }
}