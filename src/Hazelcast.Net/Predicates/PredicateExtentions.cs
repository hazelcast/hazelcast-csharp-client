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
using Hazelcast.Exceptions;

namespace Hazelcast.Predicates
{
    /// <summary>
    /// Provides extension methods for the <see cref="IPredicate"/> interface.
    /// </summary>
    public static class PredicateExtentions
    {
        /// <summary>
        /// Begins a query predicate for the key,
        /// which will be combined with this predicate into an AND logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A logical predicate builder.</returns>
        /// <remarks>
        /// <para>Chaining logical (AND/OR) operations respect the usual rule of logical
        /// precedence where AND > OR. Therefore, "A OR B AND C" will become
        /// OR(A,AND(B,C)) but "A AND B OR C" will become "OR(AND(A,B),C).</para>
        /// </remarks>
        public static LogicalPredicateBuilder AndKey(this IPredicate predicate)
            => LogicalPredicateBuilder.And(predicate, Query.KeyName);

        /// <summary>
        /// Begins a query predicate for the value,
        /// which will be combined with this predicate into an AND logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder AndValue(this IPredicate predicate)
            => LogicalPredicateBuilder.And(predicate, Query.ValueName);

        /// <summary>
        /// Begins a query predicate for an attribute of the key,
        /// which will be combined with this predicate into an AND logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="name">The optional name of the key.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder AndKey(this IPredicate predicate, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return LogicalPredicateBuilder.And(predicate, name);
        }

        /// <summary>
        /// Begins a query predicate for an attribute of the value.
        /// which will be combined with this predicate into an AND logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder And(this IPredicate predicate, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return LogicalPredicateBuilder.And(predicate, name);
        }



        /// <summary>
        /// Begins a query predicate for the key,
        /// which will be combined with this predicate into an OR logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder OrKey(this IPredicate predicate)
            => LogicalPredicateBuilder.Or(predicate, Query.KeyName);

        /// <summary>
        /// Begins a query predicate for the value,
        /// which will be combined with this predicate into an OR logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder OrValue(this IPredicate predicate)
            => LogicalPredicateBuilder.Or(predicate, Query.ValueName);

        /// <summary>
        /// Begins a query predicate for an attribute of the key,
        /// which will be combined with this predicate into an OR logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="name">The optional name of the key.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder OrKey(this IPredicate predicate, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return LogicalPredicateBuilder.Or(predicate, name);
        }

        /// <summary>
        /// Begins a query predicate for an attribute of the value.
        /// which will be combined with this predicate into an OR logical predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>A logical predicate builder.</returns>
        public static LogicalPredicateBuilder Or(this IPredicate predicate, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            return LogicalPredicateBuilder.Or(predicate, name);
        }



        /// <summary>
        /// Succeeds if this predicate and the other predicate succeed.
        /// </summary>
        /// <param name="predicate">This predicate.</param>
        /// <param name="other">The other predicate.</param>
        /// <returns>The And predicate.</returns>
        public static IPredicate And(this IPredicate predicate, IPredicate other)
        {
            return predicate is AndPredicate andPredicate
                ? andPredicate.Concat(other)
                : new AndPredicate(predicate, other);
        }

        /// <summary>
        /// Succeeds if this predicate or the other predicate succeed.
        /// </summary>
        /// <param name="predicate">This predicate.</param>
        /// <param name="other">The other predicate.</param>
        /// <returns></returns>
        public static IPredicate Or(this IPredicate predicate, IPredicate other)
        {
            return predicate is OrPredicate orPredicate
                ? orPredicate.Concat(other)
                : new OrPredicate(predicate, other);
        }

        /// <summary>
        /// Negates this predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>This is syntactic sugar so that <c>Query.WhereNot(predicate)</c> can
        /// also be written as <c>predicate.Not()</c>.</para>
        /// </remarks>
        public static IPredicate Not(this IPredicate predicate)
            => new NotPredicate(predicate);
    }
}
