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
    /// Provides extension methods for the <see cref="IPredicate"/> interface.
    /// </summary>
    public static class PredicateExtentions
    {

        public static IPredicate And(this IPredicate firstPredicate, IPredicate secondPredicate)
        {
            return new AndPredicate(firstPredicate, secondPredicate);
        }

        public static IPredicate Between(this PredicateProperty predicateProperty, object from, object to)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new BetweenPredicate(predicateProperty.Property,  from, to);
        }

        public static IPredicate Equal(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new EqualPredicate(predicateProperty.Property, value);
        }

        public static IPredicate GreaterThan(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new GreaterLessPredicate(predicateProperty.Property, value, false, false);
        }

        public static IPredicate GreaterThanOrEqual(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new GreaterLessPredicate(predicateProperty.Property, value, true, false);
        }

        public static IPredicate ILike(this PredicateProperty predicateProperty, string expression)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new CaseInsensitiveLikePredicate(predicateProperty.Property, expression);
        }

        public static IPredicate In(this PredicateProperty predicateProperty, params object[] values)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new InPredicate(predicateProperty.Property, values);
        }

        public static IPredicate LessThan(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new GreaterLessPredicate(predicateProperty.Property, value, false, true);
        }

        public static IPredicate LessThanOrEqual(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new GreaterLessPredicate(predicateProperty.Property, value, true, true);
        }

        public static IPredicate Like(this PredicateProperty predicateProperty, string expression)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new LikePredicate(predicateProperty.Property, expression);
        }

        public static IPredicate NotEqual(this PredicateProperty predicateProperty, object value)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new NotEqualPredicate(predicateProperty.Property, value);
        }

        public static IPredicate MatchesRegex(this PredicateProperty predicateProperty, string regex)
        {
            if (predicateProperty == null) throw new ArgumentNullException(nameof(predicateProperty));
            return new RegexPredicate(predicateProperty.Property, regex);
        }

        public static IPredicate Not(this IPredicate predicate)
        {
            return new NotPredicate(predicate);
        }

        public static IPredicate Or(this IPredicate firstPredicate, IPredicate secondPredicate)
        {
            return new OrPredicate(firstPredicate, secondPredicate);
        }
    }
}
