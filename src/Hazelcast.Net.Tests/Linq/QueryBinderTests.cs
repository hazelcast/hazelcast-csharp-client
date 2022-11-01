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

            var prefix = "m";
            var count = 0;

            Assert.AreEqual((prefix) + (count++), qb.GetNextAlias());
            Assert.AreEqual((prefix) + (count++), qb.GetNextAlias());
        }


        [Test]
        public void TestStripQuotes()
        {
            Expression<Func<int>> fn = () => 1 + 2;
            var quotedExp = Expression.Quote(fn);
            var striped = QueryBinder.StripQuotes(quotedExp);
            Assert.AreEqual(striped.NodeType, ExpressionType.Lambda);
        }

        [Test]
        public void TestQueryWhereBinderBindsCorrecytly()
        {
            var dummyData = new List<DummyType>();
            var val = 0;
            var exp = dummyData.AsQueryable().Where(p => p.ColumnInteger == val);

            //Overcome the referenced values, such as `val` in the where clause
            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

            //projection aka select
            var projection = projectedExp.Source as SelectExpression;
            Assert.That(projection.Columns.Count, Is.EqualTo(2));
            Assert.AreEqual(projection.Columns.Select(p => p.Name).Intersect(columnNames), columnNames);
            Assert.AreEqual(((MapExpression)((SelectExpression)projection.From).From).Alias, nameof(DummyType));//redundandt queries are another PR's deal.

            var where = projectedExp.Source.Where as BinaryExpression;
            Assert.AreEqual(where.NodeType, ExpressionType.Equal);
            Assert.AreEqual(((ColumnExpression)where.Left).Name, nameof(DummyType.ColumnInteger));
            Assert.AreEqual(((ConstantExpression)where.Right).Value, val);
        }

        [Test]
        public void TestQuerySelectBinderBindsCorrecytly()
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsQueryable().Select(p => p.ColumnInteger);

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name == nameof(DummyType.ColumnInteger)).ToArray();

            var projection = projectedExp.Source as SelectExpression;

            Assert.That(projection.Columns.Count, Is.EqualTo(1));
            Assert.AreEqual(projection.Columns[0].Name, nameof(DummyType.ColumnInteger));
            Assert.IsNull(projection.Where);
        }

        [Test]
        public void TestQueryJoinBinderBindsCorrecytly()
        {
            var dummyData = new List<DummyType>();
            var dummyData2 = new List<DummyType>();
            Expression<Func<DummyType, DummyType, string>> tempPredicate = (DummyType o, DummyType i) => i.ColumnString;
            var query = dummyData
                .AsQueryable()
                // the other data source must be also Querable. Otherwise, it will be interpred as a (constant)value on expression tree which
                // doesn't work for us. Note: HMap is Querable. It will be used as joined data source in normal usage. Current usage 
                // is only for testing.
                .Join(dummyData2.AsQueryable(),
                o => o.ColumnInteger,
                i => i.ColumnInteger,
                tempPredicate);

            var evaluated = ExpressionEvaluator.EvaluatePartially(query.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

            var projection = projectedExp.Source as SelectExpression;
            
            Assert.AreEqual((ExpressionType)HzExpressionType.Join, projectedExp.Source.From.NodeType);
            Assert.AreEqual(nameof(DummyType.ColumnInteger),
                ((MemberExpression)((BinaryExpression)((JoinExpression)projectedExp.Source.From).JoinCondition).Left).Member.Name);

            Assert.AreEqual(nameof(DummyType.ColumnInteger),
                ((MemberExpression)((BinaryExpression)((JoinExpression)projectedExp.Source.From).JoinCondition).Right).Member.Name);

            Assert.AreEqual(ExpressionType.Equal, ((JoinExpression)projectedExp.Source.From).JoinCondition.NodeType);

        }

    }
}
