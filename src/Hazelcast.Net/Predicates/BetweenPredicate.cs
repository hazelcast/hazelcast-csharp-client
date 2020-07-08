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

namespace Hazelcast.Predicates
{
    public class BetweenPredicate : IPredicate
    {
        private string _attributeName;
        private object _from;
        private object _to;

        public BetweenPredicate()
        {
        }

        public BetweenPredicate(string attributeName, object from, object to)
        {
            _attributeName = attributeName;
            _from = from;
            _to = to;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            _attributeName = input.ReadUtf();
            _to = input.ReadObject<object>();
            _from = input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteUtf(_attributeName);
            output.WriteObject(_to);
            output.WriteObject(_from);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.BetweenPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is BetweenPredicate other && Equals(this, other);
        }

        private static bool Equals(BetweenPredicate obj1, BetweenPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._attributeName == obj2._attributeName &&
                   Equals(obj1._from, obj2._from) &&
                   Equals(obj1._to, obj2._to);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                var hashCode = (_attributeName != null ? _attributeName.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode*397) ^ (_from != null ? _from.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_to != null ? _to.GetHashCode() : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
                return hashCode;
            }
        }

        public override string ToString()
        {
            return _attributeName + " BETWEEN " + _from + " AND " + _to;
        }
    }
}
