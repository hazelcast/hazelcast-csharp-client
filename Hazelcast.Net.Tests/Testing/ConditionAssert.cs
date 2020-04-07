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
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Hazelcast.Tests.Testing
{
    [Obsolete("remove this code")]
    public static class ConditionAssert
    {
        private static bool GetCondition()
        {
            var testContext = TestContext.CurrentContext;
            var test = testContext.Test;
            var className = test.ClassName;
            var classType = Type.GetType(className);
            if (classType == null)
                throw new InvalidOperationException("no class");
            var methodName = test.MethodName;
            var methodInfos = classType.GetMethods().Where(x => x.Name == methodName); // overloads?
            MethodInfo methodInfo = null;
            foreach (var mi in methodInfos)
            {
                if (methodInfo == null)
                    methodInfo = mi;
                else
                    throw new NotSupportedException("overload");
            }
            if (methodInfo == null)
                throw new InvalidOperationException("no method");

            //var attribute = methodInfo.GetCustomAttribute<ConditionAttribute>();
            //if (attribute == null)
            //    throw new InvalidOperationException("no attribute");
            //return attribute.Condition;

            return false;
        }

        public static void IfCondition(TestDelegate code)
            => IfCondition(GetCondition(), code);

        public static void IfCondition(bool condition, TestDelegate code)
        {
            if (!condition)
                XAssert.Throws<NotImplementedException>(code);
            else
                code();
        }
    }
}