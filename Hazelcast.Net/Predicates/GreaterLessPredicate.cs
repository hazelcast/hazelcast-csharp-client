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
using System.Text;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class GreaterLessPredicate : IPredicate
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
            
            _attributeName = input.ReadUtf();
            _value = input.ReadObject<object>();
            _equal = input.ReadBoolean();
            _less = input.ReadBoolean();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteUtf(_attributeName);
            output.WriteObject(_value);
            output.WriteBoolean(_equal);
            output.WriteBoolean(_less);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.GreaterLessPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is GreaterLessPredicate other && Equals(this, other);
        }

        private static bool Equals(GreaterLessPredicate obj1, GreaterLessPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._value.Equals(obj2._value) &&
                   obj1._less == obj2._less &&
                   obj1._equal == obj2._equal &&
                   obj1._attributeName == obj2._attributeName;
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
            var sb = new StringBuilder();
            sb.Append(_attributeName);
            sb.Append(_less ? "<" : ">");
            if (_equal)
            {
                sb.Append("=");
            }
            sb.Append(_value);
            return sb.ToString();
        }
    }
}
