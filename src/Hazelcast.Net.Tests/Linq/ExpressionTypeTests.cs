﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq.Expressions;
using Hazelcast.Linq.Expressions;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    internal class ExpressionTypeTests
    {
        [Test]
        public void TestExpressionTypesDontOverlap()
        {
            var hzExpressions = Enum.GetValues(typeof(HzExpressionType)).Cast<int>();
            var builtInExpressions = Enum.GetValues(typeof(ExpressionType)).Cast<int>();
            Assert.Greater(hzExpressions.Count(), 0);
            Assert.Greater(builtInExpressions.Count(), 0);
            Assert.IsEmpty(hzExpressions.Intersect(builtInExpressions));
        }
    }
}
