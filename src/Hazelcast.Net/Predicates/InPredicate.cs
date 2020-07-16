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
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _attributeName = input.ReadUtf();
            var size = input.ReadInt();
            _values = new object[size];
            for (var i = 0; i < size; i++)
            {
                _values[i] = input.ReadObject<object>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteUtf(_attributeName);
            output.WriteInt(_values.Length);
            foreach (var value in _values)
            {
                output.WriteObject(value);
            }
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.InPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is InPredicate other && Equals(this, other);
        }

        private static bool Equals(InPredicate left, InPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._attributeName == right._attributeName &&
                   left._values.SequenceEqual(right._values);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((_attributeName != null ? _attributeName.GetHashCode(StringComparison.Ordinal) : 0)*397) ^
                       (_values != null ? _values.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public override string ToString()
        {
            return "(" + _attributeName + " IN " + string.Join(", ", _values) + ")";
        }
    }
}
