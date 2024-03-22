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

using Hazelcast.Core;
using NUnit.Framework.Constraints;

namespace Hazelcast.Testing.Constraints
{
    public class AttemptConstraint : Constraint
    {
        private readonly bool _success;

        public AttemptConstraint(bool success)
        {
            _success = success;
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            // TActual may be Attempt or Attempt<TValue>
            // both being independent structs

            var actualType = typeof (TActual);
            var isAttempt = actualType == typeof (Attempt) ||
                            (actualType.IsGenericType && actualType.GetGenericTypeDefinition() == typeof (Attempt<>));

            if (!isAttempt)
                return AttemptResult.Fail(this, actual, $"Expected: Attempt\nBut was: {actualType.Name}");

            var success = ((dynamic) actual).Success;

            if (success != _success)
                return AttemptResult.Fail(this, actual, "Expected: " +
                                                        (_success ? "successful" : "failed") +
                                                        "Attempt\nBut was: " +
                                                        (_success ? "failed" : "successful") +
                                                        "Attempt");

            return AttemptResult.Success(this, actual);
        }
    }
}
