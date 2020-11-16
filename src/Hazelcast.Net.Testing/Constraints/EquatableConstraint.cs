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

using NUnit.Framework.Constraints;

namespace Hazelcast.Testing.Constraints
{
    public class EquatableConstraint : Constraint // IResolveConstraint
    {
        private readonly object _equal;
        private readonly object[] _different;

        public EquatableConstraint(object equal, object[] different)
        {
            _equal = equal;
            _different = different;
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            // uses the overloaded TActual.Equals() methods

            // all these are intentional in this tests
            // ReSharper disable EqualExpressionComparison
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable NegativeEqualityExpression
#pragma warning disable CS1718 // Comparison made to same variable

            if (!actual.Equals(actual)) return EquatableResult.Fail(this, actual, "'x.Equals(x)' should not be false");
            if (actual.Equals(null)) return EquatableResult.Fail(this, actual, "'x.Equals(null)' should not be true");
            if (!actual.Equals(_equal)) return EquatableResult.Fail(this, actual, "'x.Equals(equal)' should not be false");

            foreach (var x in _different)
                if (actual.Equals(x)) return EquatableResult.Fail(this, actual, "'x.Equals(different)' should not be true");

            // the compiler has no way to know that T overrides ==
            // so that would use object == only and fail
            // we need to use dynamic to force late binding
            //Assert.That(x == otherEqual);

            dynamic dActual = actual;

            if (dActual == null) return EquatableResult.Fail(this, actual, "'x == null' should not be true");
            if (!(dActual != null)) return EquatableResult.Fail(this, actual, "'x != null' should not be false");
            if (!(null != dActual)) return EquatableResult.Fail(this, actual, "'null != x' should not be false");
            if (!(dActual == dActual)) return EquatableResult.Fail(this, actual, "'x == x' should not be false");

            dynamic dEqual = _equal;
            if (dActual != dEqual) return EquatableResult.Fail(this, actual, "'x != equal' should not be true");
            if (!(dActual == dEqual)) return EquatableResult.Fail(this, actual, "'x == equal' should not be false");

            foreach (dynamic dDifferent in _different)
            {
                if (dActual == dDifferent) return EquatableResult.Fail(this, actual, "'x == different' should not be true");
                if (!(dActual != dDifferent)) return EquatableResult.Fail(this, actual, "'x != different' should not be false");
            }

            // success
            return EquatableResult.Success(this, actual);
        }
    }
}