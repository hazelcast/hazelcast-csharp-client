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
    public class LikePredicate : IPredicate
    {
        protected string AttributeName { get;private set; }
        protected string Expression { get; private set; }

        public LikePredicate()
        { }

        public LikePredicate(string attributeName, string expression)
        {
            AttributeName = attributeName;
            Expression = expression;
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            AttributeName = input.ReadUtf();
            Expression = input.ReadUtf();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteUtf(AttributeName);
            output.WriteUtf(Expression);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public virtual int ClassId => PredicateDataSerializerHook.LikePredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is LikePredicate other && Equals(this, other);
        }

        private static bool Equals(LikePredicate left, LikePredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left.AttributeName == right.AttributeName &&
                   left.Expression == right.Expression;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return ((AttributeName != null ? AttributeName.GetHashCode(StringComparison.Ordinal) : 0)*397) ^
                       (Expression != null ? Expression.GetHashCode(StringComparison.Ordinal) : 0);
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public override string ToString()
        {
            return "LIKE(" + AttributeName + ", '" + Expression + "')";
        }
    }
}
