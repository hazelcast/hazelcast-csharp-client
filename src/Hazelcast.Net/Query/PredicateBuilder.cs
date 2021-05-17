// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;

namespace Hazelcast.Query
{
    /// <summary>
    /// Builds a predicate.
    /// </summary>
    public class PredicateBuilder
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PredicateBuilder"/> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        internal PredicateBuilder(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            _name = name;
        }



        /// <summary>
        /// Negates a predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new predicate.</returns>
        internal static IPredicate Not(IPredicate predicate)
            => new NotPredicate(predicate);



        /// <summary>
        /// Succeeds if the target value is between the specified inclusive bounds.
        /// </summary>
        /// <param name="lowerBound">The lower inclusive bound.</param>
        /// <param name="upperBound">The upper inclusive bound.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsBetween(object lowerBound, object upperBound)
            => new BetweenPredicate(_name, lowerBound, upperBound);

        /// <summary>
        /// Succeeds if the target value is equal to one of the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsIn(params object[] values)
            => new InPredicate(_name, values);

        /// <summary>
        /// Succeeds if the target value is equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsEqualTo(object value)
            => new EqualPredicate(_name, value);

        /// <summary>
        /// Succeeds if the target value is not equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsNotEqualTo(object value)
            => new NotEqualPredicate(_name, value);

        /// <summary>
        /// Succeeds if the target value is less than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsLessThan(object value)
            => new GreaterLessPredicate(_name, value, false, true);

        /// <summary>
        /// Succeeds if the target value is less than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsLessThanOrEqualTo(object value)
            => new GreaterLessPredicate(_name, value, true, true);

        /// <summary>
        /// Succeeds if the target value is greater than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsGreaterThan(object value)
            => new GreaterLessPredicate(_name, value, false, false);

        /// <summary>
        /// Succeeds if the target value is greater than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public virtual IPredicate IsGreaterThanOrEqualTo(object value)
            => new GreaterLessPredicate(_name, value, true, false);

        /// <summary>
        /// Succeeds if the target value matches the specified case-insensitive pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is case-insensitive.</para>
        /// <para>In the pattern, the % character (percentage sign) is a placeholder for
        /// multiple characters, and the _ character (underscore) is a placeholder for
        /// a single character.</para>
        /// <para>These two special characters can be escaped with a backslash.</para>
        /// </remarks>
        public virtual IPredicate IsILike(string pattern)
            => new CaseInsensitiveLikePredicate(_name, pattern);

        /// <summary>
        /// Succeeds if the target value matches the specified case-sensitive pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is case-sensitive.</para>
        /// <para>In the pattern, the % character (percentage sign) is a placeholder for
        /// multiple characters, and the _ character (underscore) is a placeholder for
        /// a single character.</para>
        /// <para>These two special characters can be escaped with a backslash.</para>
        /// </remarks>
        public virtual IPredicate IsLike(string pattern)
            => new LikePredicate(_name, pattern);

        /// <summary>
        /// Succeeds if the target value matches the specified regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is interpreted exactly in the same way as described in the
        /// documentation for the Java <c>java.util.regex.Pattern</c> class.</para>
        /// </remarks>
        public virtual IPredicate Matches(string regex)
            => new RegexPredicate(_name, regex);
    }
}
