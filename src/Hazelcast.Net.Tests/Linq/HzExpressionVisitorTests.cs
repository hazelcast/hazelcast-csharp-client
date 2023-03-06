// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq.Expressions;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    internal class HzExpressionVisitorTests
    {
        private class DummyType
        {
            public string Name { get; set; }
            public string LastName { get; set; }
        }

        [Test]
        public void TestVisit()
        {
            Expression<Func<DummyType, string>> exp = p => p.Name;

            var columnDef = new ColumnDefinition("name", exp);
            var columnDefs = new List<ColumnDefinition>() { columnDef };

            Expression<Func<DummyType, string>> joinFieldExp = p => p.LastName;
            var joinExp = new JoinExpression(joinFieldExp, joinFieldExp, Expression.Equal(joinFieldExp, joinFieldExp), typeof(DummyType));

            var selectExp = new SelectExpression("m1", typeof(DummyType), columnDefs.AsReadOnly(), joinExp);

            var projector = new ColumnProjector(p => p.NodeType == (ExpressionType)HzExpressionType.Column)
                .Project(exp, "m1", new string[] { "m", "m1" });

            //Projection represents the whole structure which are query part (SelectExpression)
            //and reconstruction of the result object(Projector).
            //The structure is;
            //              Projection
            //                /    \
            //             Select  Projector
            //              / | \           \
            //       Columns,From,Where     Column Definitions
            //                 |
            //               Join
            //             /    |   \
            //          Left,Condition,Right
            // So, each type of expression should be visited.

            var projection = new ProjectionExpression(selectExp, projector.Projector, typeof(DummyType));

            var moqVisitor = new Mock<HzExpressionVisitor>();
            moqVisitor.CallBase = true;

            var visitedNode = moqVisitor.Object.Visit(projection);

            //We don't expect any change since HzExpressionVisitor is kinda router for custom HZ expressions.
            Assert.AreEqual(projection, visitedNode);
            moqVisitor.Verify(p => p.VisitProjection(projection), Times.Once(), "Projection not visited.");
            moqVisitor.Verify(p => p.VisitSelect(projection.Source), Times.Once(), "Select not visited.");
            moqVisitor.Verify(p => p.VisitJoin((JoinExpression)projection.Source.From), Times.Once(), "Join not visited.");
        }
    }
}
