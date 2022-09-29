using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using Hazelcast.Linq.Visitors;
using System.Linq.Expressions;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Tests.Linq
{
    public class QueryBinderTests
    {
        public class DummyType
        {
            public int ColumnInteger { get; set; }
            public string ColumnString { get; set; }
        }


        [Test]
        public void TestGetNextAlias()
        {
            var qb = new QueryBinder();

            var prefix = "t";
            var count = 0;

            Assert.AreEqual((prefix) + (count++), qb.GetNextAlias());
            Assert.AreEqual((prefix) + (count++), qb.GetNextAlias());
        }


        [Test]
        public void TestStripQuotes()
        {
            var quotedExp = Expression.Quote(Expression.Add(Expression.Constant(1), Expression.Constant(2)));
            var striped = QueryBinder.StripQuotes(quotedExp);
            Assert.AreEqual(striped.NodeType, typeof(BinaryExpression));
        }

        [Test]
        public void TestQueryBinderBindsCorrecytly()
        {
            var dummyData = new List<DummyType>();
            var val = 0;
            var exp = dummyData.AsQueryable().Where(p => p.ColumnInteger == val);

            //Overcome the referenced values, such as `val` in the where clause
            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

            var projection = projectedExp.Source as SelectExpression;
            Assert.That(projection.Columns.Count, Is.EqualTo(2));
            Assert.AreEqual(projection.Columns.Select(p => p.Name).Intersect(columnNames), columnNames);
            Assert.AreEqual(((MapExpression)((SelectExpression)projection.From).From).Alias, nameof(DummyType));//redundandt queries are another PR's deal.

            var where = projectedExp.Source.Where as BinaryExpression;
            Assert.AreEqual(where.NodeType, ExpressionType.Equal);
            Assert.AreEqual(((ColumnExpression)where.Left).Name, nameof(DummyType.ColumnInteger));
            Assert.AreEqual(((ConstantExpression)where.Right).Value, val);
        }

    }
}
