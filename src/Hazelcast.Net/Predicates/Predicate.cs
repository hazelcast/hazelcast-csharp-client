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

using System.Collections;
using System.Collections.Generic;

namespace Hazelcast.Predicates
{
    /// <summary>
    /// Creates predicates.
    /// </summary>
    public static class Predicate
    {
        internal const string KeyConst = "__key";
        private const string ThisConst = "this";

        public static PredicateProperty Key(string property = null)
            => new PredicateProperty(property != null ? KeyConst + "." + property : KeyConst);

        public static PredicateProperty Property(string property)
            => new PredicateProperty(property);

        public static PredicateProperty This()
            => new PredicateProperty(ThisConst);

        public static IPredicate InstanceOf(string fullJavaClassName)
            => new InstanceofPredicate(fullJavaClassName);

        public static IPredicate And(params IPredicate[] predicates)
            => new AndPredicate(predicates);

        public static IPredicate False()
            => new FalsePredicate();

        public static IPredicate IsBetween(string attributeName, object from, object to)
            => new BetweenPredicate(attributeName, from, to);

        public static IPredicate IsEqual(string attributeName, object value)
            => new EqualPredicate(attributeName, value);

        public static IPredicate IsGreaterThan(string attributeName, object value)
            => new GreaterLessPredicate(attributeName, value, false, false);

        public static IPredicate IsGreaterThanOrEqual(string attributeName, object value)
            => new GreaterLessPredicate(attributeName, value, true, false);

        public static IPredicate IsILike(string attributeName, string expression)
            => new CaseInsensitiveLikePredicate(attributeName, expression);

        public static IPredicate IsIn(string attributeName, params object[] values)
            => new InPredicate(attributeName, values);

        public static IPredicate IsLessThan(string attributeName, object value)
            => new GreaterLessPredicate(attributeName, value, false, true);

        public static IPredicate IsLessThanOrEqual(string attributeName, object value)
            => new GreaterLessPredicate(attributeName, value, true, true);

        public static IPredicate IsLike(string attributeName, string expression)
            => new LikePredicate(attributeName, expression);

        public static IPredicate IsNotEqual(string attributeName, object value)
            => new NotEqualPredicate(attributeName, value);

        public static IPredicate MatchesRegex(string attributeName, string regex)
            => new RegexPredicate(attributeName, regex);

        public static IPredicate Not(IPredicate predicate)
        => new NotPredicate(predicate);

        public static IPredicate Or(params IPredicate[] predicates)
            => new OrPredicate(predicates);

        public static IPredicate Sql(string sql)
            => new SqlPredicate(sql);

        public static IPredicate True()
            => new TruePredicate();

        public static IPartitionPredicate Partition(object partitionKey, IPredicate predicate)
            => new PartitionPredicate(partitionKey, predicate);

        public static IPagingPredicate Page(int pageSize, IPredicate predicate, IComparer<KeyValuePair<object, object>> comparer)
            => new PagingPredicate(pageSize, predicate, comparer);
    }
}
