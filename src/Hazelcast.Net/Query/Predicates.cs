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
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Exceptions;

namespace Hazelcast.Query
{
    /// <summary>
    /// Creates <see cref="IPredicate"/> instances.
    /// </summary>
    public static class Predicates
    {
        internal const string KeyName = "__key";
        internal const string ValueName = "this";



        /// <summary>
        /// Begins a predicate for the key.
        /// </summary>
        /// <returns>A predicate builder.</returns>
        public static PredicateBuilder Key()
            => new PredicateBuilder(KeyName);

        /// <summary>
        /// Begins a predicate for an attribute of the key.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>A predicate builder.</returns>
        public static PredicateBuilder Key(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return new PredicateBuilder(KeyName + "." + name);
        }

        /// <summary>
        /// Begins a predicate for the value.
        /// </summary>
        /// <returns>A predicate builder.</returns>
        public static PredicateBuilder Value()
            => new PredicateBuilder(ValueName);

        /// <summary>
        /// Begins a predicate for an attribute of the value.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>A predicate builder.</returns>
        public static PredicateBuilder Value(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return new PredicateBuilder(name);
        }



        /// <summary>
        /// Succeeds if all the specified predicates succeed.
        /// </summary>
        /// <param name="predicates">The predicates.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate And(params IPredicate[] predicates)
            => new AndPredicate(predicates);

        /// <summary>
        /// Succeeds if at least one of the specified predicates succeed.
        /// </summary>
        /// <param name="predicates">The predicates.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate Or(params IPredicate[] predicates)
            => new OrPredicate(predicates);

        /// <summary>
        /// Succeeds if the specified predicate does not succeed.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate Not(IPredicate predicate)
            => new NotPredicate(predicate);



        /// <summary>
        /// Always succeeds.
        /// </summary>
        /// <returns>A new predicate.</returns>
        public static IPredicate True()
            => new TruePredicate();

        /// <summary>
        /// Never succeeds.
        /// </summary>
        /// <returns>A new predicate.</returns>
        public static IPredicate False()
            => new FalsePredicate();



        /// <summary>
        /// Succeeds if the target value is between the specified inclusive bounds.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="lowerBound">The lower inclusive bound.</param>
        /// <param name="upperBound">The upper inclusive bound.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate Between(string name, object lowerBound, object upperBound)
            => new BetweenPredicate(name, lowerBound, upperBound);

        /// <summary>
        /// Succeeds if the target value is equal to one of the specified values.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="values">The values.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate In(string name, params object[] values)
            => new InPredicate(name, values);

        /// <summary>
        /// Succeeds if the target value is equal to one of the specified values.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="values">The values.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate In<T>(string name, IEnumerable<T> values)
            => new InPredicate(name, values.Cast<object>().ToList());

        /// <summary>
        /// Succeeds if the target value is equal to the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate EqualTo(string name, object value)
            => new EqualPredicate(name, value);

        /// <summary>
        /// Succeeds if the target value is not equal to the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate NotEqualTo(string name, object value)
            => new NotEqualPredicate(name, value);

        /// <summary>
        /// Succeeds if the target value is less than the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate LessThan(string name, object value)
            => new GreaterLessPredicate(name, value, false, true);

        /// <summary>
        /// Succeeds if the target value is less than, or equal to, the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate LessThanOrEqualTo(string name, object value)
            => new GreaterLessPredicate(name, value, true, true);

        /// <summary>
        /// Succeeds if the target value is greater than the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate GreaterThan(string name, object value)
            => new GreaterLessPredicate(name, value, false, false);

        /// <summary>
        /// Succeeds if the target value is greater than, or equal to, the specified value.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate GreaterThanOrEqualTo(string name, object value)
            => new GreaterLessPredicate(name, value, true, false);

        /// <summary>
        /// Succeeds if the target value matches the specified case-insensitive pattern.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is case-insensitive.</para>
        /// <para>In the pattern, the % character (percentage sign) is a placeholder for
        /// multiple characters, and the _ character (underscore) is a placeholder for
        /// a single character.</para>
        /// <para>These two special characters can be escaped with a backslash.</para>
        /// </remarks>
        public static IPredicate ILike(string name, string pattern)
            => new CaseInsensitiveLikePredicate(name, pattern);

        /// <summary>
        /// Succeeds if the target value matches the specified case-sensitive pattern.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is case-sensitive.</para>
        /// <para>In the pattern, the % character (percentage sign) is a placeholder for
        /// multiple characters, and the _ character (underscore) is a placeholder for
        /// a single character.</para>
        /// <para>These two special characters can be escaped with a backslash.</para>
        /// </remarks>
        public static IPredicate Like(string name, string pattern)
            => new LikePredicate(name, pattern);

        /// <summary>
        /// Succeeds if the target value matches the specified regular expression.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is interpreted exactly in the same way as described in the
        /// documentation for the Java <c>java.util.regex.Pattern</c> class.</para>
        /// </remarks>
        public static IPredicate Match(string name, string regex)
            => new RegexPredicate(name, regex);



        /// <summary>
        /// Succeeds if the specified SQL query succeeds.
        /// </summary>
        /// <param name="sql">The SQL query.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>(to be completed with SQL documentation)</para>
        /// </remarks>
        public static IPredicate Sql(string sql)
            => new SqlPredicate(sql);

        /// <summary>
        /// Succeeds if the item is an instance of the specified class.
        /// </summary>
        /// <param name="fullJavaClassName">The full Java class name.</param>
        /// <returns>A new predicate.</returns>
        public static IPredicate InstanceOf(string fullJavaClassName)
            => new InstanceofPredicate(fullJavaClassName);

        /// <summary>
        /// Restricts the execution of a predicate to a single partition.
        /// </summary>
        /// <param name="partitionKey">The key of the partition.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new predicate.</returns>
        public static IPartitionPredicate Partition(object partitionKey, IPredicate predicate)
            => new PartitionPredicate(partitionKey, predicate);

        /// <summary>
        /// Paginates results.
        /// </summary>
        /// <param name="pageSize">The size of a page.</param>
        /// <returns>A new predicate.</returns>
        public static IPagingPredicate Page(int pageSize)
            => new PagingPredicate(pageSize);

        /// <summary>
        /// Paginates results.
        /// </summary>
        /// <param name="pageSize">The size of a page.</param>
        /// <param name="comparer">A comparer used to order results.</param>
        /// <returns>A new predicate.</returns>
        public static IPagingPredicate Page(int pageSize, IComparer<KeyValuePair<object, object>> comparer)
            => new PagingPredicate(pageSize, comparer: comparer);

        /// <summary>
        /// Paginates results of a predicate.
        /// </summary>
        /// <param name="pageSize">The size of a page.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new predicate.</returns>
        public static IPagingPredicate Page(int pageSize, IPredicate predicate)
            => new PagingPredicate(pageSize, predicate);

        /// <summary>
        /// Paginates results of a predicate.
        /// </summary>
        /// <param name="pageSize">The size of a page.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="comparer">A comparer used to order results.</param>
        /// <returns>A new predicate.</returns>
        public static IPagingPredicate Page(int pageSize, IPredicate predicate, IComparer<KeyValuePair<object, object>> comparer)
            => new PagingPredicate(pageSize, predicate, comparer);
    }
}
