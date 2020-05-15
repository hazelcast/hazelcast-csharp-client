// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Predicates
{
    /// <summary>
    /// Creates predicates.
    /// </summary>
    public static class Predicates
    {
        public const string KeyConst = "__key";
        private const string ThisConst = "this";

        public static PredicateProperty Key(string property = null)
        {
            return new PredicateProperty(property != null ? KeyConst + "." + property : KeyConst);
        }

        public static PredicateProperty Property(string property)
        {
            return new PredicateProperty(property);
        }

        public static PredicateProperty This()
        {
            return new PredicateProperty(ThisConst);
        }

        public static InstanceofPredicate InstanceOf(string fullJavaClassName)
        {
            return new InstanceofPredicate(fullJavaClassName);
        }

        public static AndPredicate And(params IPredicate[] predicates)
        {
            return new AndPredicate(predicates);
        }

        public static FalsePredicate False()
        {
            return new FalsePredicate();
        }

        public static BetweenPredicate IsBetween(string attributeName, object from, object to)
        {
            return new BetweenPredicate(attributeName, from, to);
        }

        public static EqualPredicate IsEqual(string attributeName, object value)
        {
            return new EqualPredicate(attributeName, value);
        }

        public static GreaterLessPredicate IsGreaterThan(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, false, false);
        }

        public static GreaterLessPredicate IsGreaterThanOrEqual(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, true, false);
        }

        public static CaseInsensitiveLikePredicate IsILike(string attributeName, string expression)
        {
            return new CaseInsensitiveLikePredicate(attributeName, expression);
        }

        public static InPredicate IsIn(string attributeName, params object[] values)
        {
            return new InPredicate(attributeName, values);
        }

        public static GreaterLessPredicate IsLessThan(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, false, true);
        }

        public static GreaterLessPredicate IsLessThanOrEqual(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, true, true);
        }

        public static LikePredicate IsLike(string attributeName, string expression)
        {
            return new LikePredicate(attributeName, expression);
        }

        public static NotEqualPredicate IsNotEqual(string attributeName, object value)
        {
            return new NotEqualPredicate(attributeName, value);
        }

        public static RegexPredicate MatchesRegex(string attributeName, string regex)
        {
            return new RegexPredicate(attributeName, regex);
        }

        public static NotPredicate Not(IPredicate predicate)
        {
            return new NotPredicate(predicate);
        }

        public static OrPredicate Or(params IPredicate[] predicates)
        {
            return new OrPredicate(predicates);
        }

        public static SqlPredicate Sql(string sql)
        {
            return new SqlPredicate(sql);
        }

        public static TruePredicate True()
        {
            return new TruePredicate();
        }
    }
}