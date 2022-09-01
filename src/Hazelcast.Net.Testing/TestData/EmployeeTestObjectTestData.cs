// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.Tests.TestObjects;

namespace Hazelcast.Testing.TestData
{
    public class EmployeeTestObjectTestData
    {
        public static IEnumerable<EmployeeTestObject> EmployeeTestObjects
        {
            get
            {
                yield return new EmployeeTestObject() { Id = 1, Name = "$'Marvin Gay'", Salary = 1.7976931348623157E+308, Type = 'B' };

                yield return new EmployeeTestObject() { Id = 2, Name = "$'Marvin Gay'", Salary = 1.7976931348623157E+308, Type = 'A' };

                yield return new EmployeeTestObject() { Id = 3, Name = "\nGroove Armada", Salary = -1.7976931348623157E+308, Type = 'A' };

                yield return new EmployeeTestObject() { Id = 4, Name = "Boby Womack Text with special character \"'\b\f\t\r\n.", Salary = 0, Type = 'A' };

                yield return new EmployeeTestObject() { Id = 5, Name = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged.", Salary = 1.3, Type = 'A' };
            }
        }
    }
}
