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
    /// Builds a negated predicate.
    /// </summary>
    public class DoesNotPredicateBuilder
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoesNotPredicateBuilder"/> class.
        /// </summary>
        /// <param name="name">The name of the target.</param>
        internal DoesNotPredicateBuilder(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(name));
            _name = name;
        }



        /// <summary>
        /// Succeeds if the target value matches the specified regular expression.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        /// <returns>A new predicate.</returns>
        /// <remarks>
        /// <para>The pattern is interpreted exactly in the same way as described in the
        /// documentation for the Java <c>java.util.regex.Pattern</c> class.</para>
        /// </remarks>
        public virtual IPredicate Match(string regex)
            => PredicateBuilder.Not(new RegexPredicate(_name, regex));
    }
}