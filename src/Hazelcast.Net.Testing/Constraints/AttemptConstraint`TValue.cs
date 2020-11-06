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

using Hazelcast.Core;
using NUnit.Framework.Constraints;

namespace Hazelcast.Testing.Constraints
{
    public class AttemptConstraint<TValue> : AttemptConstraint
    {
        private readonly bool _success;
        private readonly TValue _value;

        public AttemptConstraint(bool success, TValue value)
            : base(true)
        {
            _success = success;
            _value = value;
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            // TActual may be Attempt or Attempt<TValue>
            // both being independent structs

            var actualType = typeof (TActual);
            var valueType = typeof (TValue);
            var isAttempt = actualType.IsGenericType &&
                            actualType.GetGenericTypeDefinition() == typeof (Attempt<>) &&
                            actualType.GetGenericArguments()[0] == valueType;

            if (!isAttempt)
                return AttemptResult.Fail(this, actual, $"Expected: Attempt<{typeof (TValue).Name}>\nBut was: {actualType.Name}");

            var success = ((dynamic) actual).Success;

            if (success != _success)
                return AttemptResult.Fail(this, actual, "Expected: " +
                                                        (_success ? "successful" : "failed") +
                                                        "Attempt\nBut was: " +
                                                        (_success ? "failed" : "successful") +
                                                        "Attempt");

            var value = ((dynamic) actual).Value;

            if (!value.Equals(_value))
                return AttemptResult.Fail(this, actual, $"Expected: Attempt with value {_value}\nBut was: Attempt with value {value}");

            return AttemptResult.Success(this, actual);
        }
    }
}