// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    internal class BetweenPredicate : IPredicate, IIdentifiedDataSerializable
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
            _attributeName = input.ReadString();
            _to = input.ReadObject<object>();
            _from = input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteString(_attributeName);
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

        private static bool Equals(BetweenPredicate left, BetweenPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._attributeName == right._attributeName &&
                   Equals(left._from, right._from) &&
                   Equals(left._to, right._to);
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
            return "BETWEEN(" + _attributeName + ", " + _from + ", " + _to + ")";
        }
    }
}
