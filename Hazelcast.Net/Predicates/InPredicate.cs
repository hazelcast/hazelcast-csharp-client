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

using System;
using System.Linq;
using Hazelcast.Data;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class InPredicate : IPredicate
    {
        private string _attributeName;
        private object[] _values;

        public InPredicate()
        {
        }

        public InPredicate(string attributeName, params object[] values)
        {
            _attributeName = attributeName;
            _values = values ?? throw new NullReferenceException("Array can't be null");
        }

        public void ReadData(IObjectDataInput input)
        {
            _attributeName = input.ReadUTF();
            var size = input.ReadInt();
            _values = new object[size];
            for (var i = 0; i < size; i++)
            {
                _values[i] = input.ReadObject<object>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_attributeName);
            output.WriteInt(_values.Length);
            foreach (var value in _values)
            {
                output.WriteObject(value);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.InPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is InPredicate other && Equals(this, other);
        }

        private static bool Equals(InPredicate obj1, InPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._attributeName == obj2._attributeName &&
                   obj1._values.SequenceEqual(obj2._values);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_attributeName != null ? _attributeName.GetHashCode() : 0)*397) ^
                       (_values != null ? _values.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return _attributeName + " IN (" + string.Join(", ", _values) + ")";
        }
    }
}