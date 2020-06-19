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
    public class RegexPredicate : IPredicate
    {
        private string _attributeName;
        private string _regex;

        public RegexPredicate()
        {
        }

        public RegexPredicate(string attributeName, string regex)
        {
            _attributeName = attributeName;
            _regex = regex;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _attributeName = input.ReadUtf();
            _regex = input.ReadUtf();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.WriteUtf(_attributeName);
            output.WriteUtf(_regex);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.RegexPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is RegexPredicate other && Equals(this, other);
        }

        private static bool Equals(RegexPredicate obj1, RegexPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._attributeName == obj2._attributeName &&
                   obj1._regex == obj2._regex;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((_attributeName != null ? _attributeName.GetHashCode(StringComparison.Ordinal) : 0)*397) ^
                       (_regex != null ? _regex.GetHashCode(StringComparison.Ordinal) : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public override string ToString()
        {
            return _attributeName + " REGEX '" + _regex + "'";
        }
    }
}
