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

namespace Hazelcast.Predicates
{
    // FIXME DOCUMENT
    public class LogicalIsNotPredicateBuilder : IsNotPredicateBuilder
    {
        private readonly LogicalPredicateBuilder _builder;

        internal LogicalIsNotPredicateBuilder(LogicalPredicateBuilder builder, string name)
            : base(name)
        {
            _builder = builder;
        }

        /// <summary>
        /// Succeeds if the target value is between the specified inclusive bounds.
        /// </summary>
        /// <param name="lowerBound">The lower inclusive bound.</param>
        /// <param name="upperBound">The upper inclusive bound.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate Between(object lowerBound, object upperBound)
            => _builder.Append(base.Between(lowerBound, upperBound));

        /// <summary>
        /// Succeeds if the target value is equal to one of the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate In(params object[] values)
            => _builder.Append(base.In(values));

        /// <summary>
        /// Succeeds if the target value is equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate EqualTo(object value)
            => _builder.Append(base.EqualTo(value));

        /// <summary>
        /// Succeeds if the target value is not equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate NotEqualTo(object value)
            => _builder.Append(base.NotEqualTo(value));

        /// <summary>
        /// Succeeds if the target value is less than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate LessThan(object value)
            => _builder.Append(base.LessThan(value));

        /// <summary>
        /// Succeeds if the target value is less than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate LessThanOrEqualTo(object value)
            => _builder.Append(base.LessThanOrEqualTo(value));

        /// <summary>
        /// Succeeds if the target value is greater than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate GreaterThan(object value)
            => _builder.Append(base.GreaterThan(value));

        /// <summary>
        /// Succeeds if the target value is greater than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate GreaterThanOrEqualTo(object value)
            => _builder.Append(base.GreaterThanOrEqualTo(value));

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
        public override IPredicate ILike(string pattern)
            => _builder.Append(base.ILike(pattern));

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
        public override IPredicate Like(string pattern)
            => _builder.Append(base.LessThan(pattern));
    }
}