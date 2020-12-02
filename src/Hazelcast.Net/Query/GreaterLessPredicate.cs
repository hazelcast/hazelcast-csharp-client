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
using Hazelcast.Serialization;

namespace Hazelcast.Query
{
    internal class GreaterLessPredicate : IPredicate, IIdentifiedDataSerializable
    {
        private string _attributeName;
        private bool _equal;
        private bool _less;
        private object _value;

        public GreaterLessPredicate()
        {
        }

        public GreaterLessPredicate(string attributeName, object value, bool isEqual, bool isLess)
        {
            _attributeName = attributeName;
            _value = value;
            _equal = isEqual;
            _less = isLess;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _attributeName = input.ReadString();
            _value = input.ReadObject<object>();
            _equal = input.ReadBool();
            _less = input.ReadBool();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write(_attributeName);
            output.WriteObject(_value);
            output.Write(_equal);
            output.Write(_less);
        }

        public int FactoryId =>FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.GreaterLessPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is GreaterLessPredicate other && Equals(this, other);
        }

        private static bool Equals(GreaterLessPredicate left, GreaterLessPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._value.Equals(right._value) &&
                   left._less == right._less &&
                   left._equal == right._equal &&
                   left._attributeName == right._attributeName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = _value.GetHashCode();
                hashCode = (hashCode*397) ^ _less.GetHashCode();
                hashCode = (hashCode*397) ^ _equal.GetHashCode();
                hashCode = (hashCode*397) ^ _attributeName.GetHashCode(StringComparison.Ordinal);
                // ReSharper enable NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }

        public override string ToString()
        {
            return "(" + _attributeName + " " + (_less ? "<" : ">") + (_equal ? "=" : "") + " " + _value + ")";
        }
    }
}
