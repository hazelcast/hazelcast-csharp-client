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

namespace Hazelcast.Predicates
{
    /// <summary>
    /// Builds a logical predicate.
    /// </summary>
    public class LogicalPredicateBuilder : PredicateBuilder
    {
        private readonly IPredicate _source;
        private readonly Op _op;

        private enum Op
        {
            Or = 1,
            And
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalPredicateBuilder"/> class.
        /// </summary>
        /// <param name="source">The source predicate.</param>
        /// <param name="op">The logical operation.</param>
        /// <param name="name">The name of the attribute.</param>
        private LogicalPredicateBuilder(IPredicate source, Op op, string name)
            : base(name)
        {
            _source = source;
            _op = op;
        }



        /// <summary>
        /// Negates the next condition.
        /// </summary>
        /// <returns>A predicate builder.</returns>
        public override IsNotPredicateBuilder IsNot()
            => new LogicalIsNotPredicateBuilder(this, _name);

        /// <summary>
        /// Negates the next condition.
        /// </summary>
        /// <returns>A predicate builder.</returns>
        public override DoesNotPredicateBuilder DoesNot()
            => new LogicalDoesNotPredicateBuilder(this, _name);



        /// <summary>
        /// Creates a new logical predicate builder for the AND logical operation.
        /// </summary>
        /// <param name="predicate">The source predicate.</param>
        /// <param name="name">The name of the </param>
        /// <returns></returns>
        internal static LogicalPredicateBuilder And(IPredicate predicate, string name)
            => new LogicalPredicateBuilder(predicate, Op.And, name);

        /// <summary>
        /// Creates a new logical predicate builder for the OR logical operation.
        /// </summary>
        /// <param name="predicate">The source predicate.</param>
        /// <param name="name">The name of the </param>
        /// <returns></returns>
        internal static LogicalPredicateBuilder Or(IPredicate predicate, string name)
            => new LogicalPredicateBuilder(predicate, Op.Or, name);

        /// <summary>
        /// Creates a logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate to append.</param>
        /// <returns>The created logical predicate.</returns>
        internal IPredicate Append(IPredicate predicate)
        {
            // if the source predicate is already a logical predicate of the
            // required type, we can simply append the new predicate to its
            // list of predicates; otherwise we need to create a new logical
            // predicate:
            //
            //   a + b = AND(a,b)
            //   AND(a,b) + c = AND(a,b,c)
            //
            // this applies the usual logical rule of precedence: AND > OR
            // therefore,
            //   a | b & c = or(a,and(b,c))
            //   a & b | c = or(and(a,b),c)

            switch (_op)
            {
                case Op.Or:
                {
                    return _source is OrPredicate orPredicate
                        ? orPredicate.Concat(predicate)
                        : new OrPredicate(_source, predicate);
                }

                case Op.And:
                {
                    switch (_source)
                    {
                        case AndPredicate andPredicate:
                            return andPredicate.Concat(predicate);

                        case OrPredicate orPredicate:
                            orPredicate.Last = new AndPredicate(orPredicate.Last, predicate);
                            return orPredicate;

                        default:
                            return new AndPredicate(_source, predicate);
                    }
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Succeeds if the item attribute value is between the specified inclusive bounds.
        /// </summary>
        /// <param name="lowerBound">The lower inclusive bound.</param>
        /// <param name="upperBound">The upper inclusive bound.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsBetween(object lowerBound, object upperBound)
            => Append(base.IsBetween(lowerBound, upperBound));

        /// <summary>
        /// Succeeds if the item attribute value is equal to one of the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsIn(params object[] values)
            => Append(base.IsIn(values));

        /// <summary>
        /// Succeeds if the item attribute value is equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsEqualTo(object value)
            => Append(base.IsEqualTo(value));

        /// <summary>
        /// Succeeds if the item attribute value is not equal to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsNotEqualTo(object value)
            => Append(base.IsNotEqualTo(value));

        /// <summary>
        /// Succeeds if the item attribute value is less than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsLessThan(object value)
            => Append(base.IsGreaterThanOrEqualTo(value));

        /// <summary>
        /// Succeeds if the item attribute value is less than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsLessThanOrEqualTo(object value)
            => Append(base.IsLessThanOrEqualTo(value));

        /// <summary>
        /// Succeeds if the item attribute value is greater than the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsGreaterThan(object value)
            => Append(base.IsGreaterThan(value));

        /// <summary>
        /// Succeeds if the item attribute value is greater than, or equal to, the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A new predicate.</returns>
        public override IPredicate IsGreaterThanOrEqualTo(object value)
            => Append(base.IsGreaterThanOrEqualTo(value));

        /// <summary>
        /// Succeeds if the item attribute value matches the specified case-insensitive pattern.
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
        public override IPredicate IsILike(string pattern)
            => Append(base.IsILike(pattern));

        /// <summary>
        /// Succeeds if the item attribute value matches the specified case-sensitive pattern.
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
        public override IPredicate IsLike(string pattern)
            => Append(base.IsLike(pattern));

        /// <summary>
        /// Succeeds if the item attribute value matches the specified regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is interpreted exactly in the same way as described in the
        /// documentation for the Java <c>java.util.regex.Pattern</c> class.</para>
        /// </remarks>
        public override IPredicate Matches(string regex)
            => Append(base.Matches(regex));
    }
}