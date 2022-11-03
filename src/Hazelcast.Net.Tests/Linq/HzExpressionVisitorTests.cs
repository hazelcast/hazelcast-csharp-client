using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
            var columndDefs = new List<ColumnDefinition>() { columnDef };

            Expression<Func<DummyType, string>> joinFieldExp = p => p.LastName;
            var joinExp = new JoinExpression(joinFieldExp, joinFieldExp, Expression.Equal(joinFieldExp, joinFieldExp), typeof(DummyType));

            var selectExp = new SelectExpression("m1", typeof(DummyType), columndDefs.AsReadOnly(), joinExp);

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

