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

using Hazelcast.Data;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    public class LikePredicate : IPredicate
    {
        protected string AttributeName;
        protected string Expression;

        public LikePredicate()
        {
        }

        public LikePredicate(string attributeName, string expression)
        {
            AttributeName = attributeName;
            Expression = expression;
        }

        public void ReadData(IObjectDataInput input)
        {
            AttributeName = input.ReadUtf();
            Expression = input.ReadUtf();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUtf(AttributeName);
            output.WriteUtf(Expression);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public virtual int GetId()
        {
            return PredicateDataSerializerHook.LikePredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is LikePredicate other && Equals(this, other);
        }

        private static bool Equals(LikePredicate obj1, LikePredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1.AttributeName == obj2.AttributeName &&
                   obj1.Expression == obj2.Expression;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AttributeName != null ? AttributeName.GetHashCode() : 0)*397) ^
                       (Expression != null ? Expression.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return AttributeName + " LIKE '" + Expression + "'";
        }
    }
}