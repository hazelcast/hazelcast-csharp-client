// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing.Constraints;

namespace Hazelcast.Testing
{
    // If we name it 'Is', then there is a confusion with NUnit's 'Is', and if we name it
    // 'Iz', there is *also* a confusion because NUnit *also* defines 'Iz' as a synonym
    // to 'Is', for Visual Basic where 'Is' is a keyword. Currently running with 'Ish' as
    // it is fast enough to type and kinda make sense.
    //
    // And then it should just inherit from NUnit.Framework.Is which is conveniently not
    // static but abstract - but even though, this causes "IDE0002: Name can be simplified"
    // warnings because directly using Is.Something would indeed be simpler. So... no.

    /// <summary>
    /// Provides properties and methods that supply constraints used in Asserts.
    /// </summary>
    public abstract class Ish //: NUnit.Framework.Is
    {
        /// <summary>
        /// Returns a constraint that test for "equatability".
        /// </summary>
        /// <param name="equal">Tests that the value is equal to that value.</param>
        /// <param name="different">Tests that the value is different from all those values.</param>
        public static EquatableConstraint Equatable(object equal, params object[] different)
            => new EquatableConstraint(equal, different);
    }
}
