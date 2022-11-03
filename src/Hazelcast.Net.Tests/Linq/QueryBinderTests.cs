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
using NUnit.Framework;

using Hazelcast.Linq.Visitors;
using System.Linq.Expressions;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Tests.Linq
{
    public class QueryBinderTests
    {
        private class DummyType
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
            Assert.AreEqual(prefix + count, qb.GetNextAlias());
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
        public void TestQueryBinderBindsCorrectly()
        {
            var dummyData = new List<DummyType>();
            var val = 0;
            var exp = dummyData.AsQueryable().Where(p => p.ColumnInteger == val);

            //Overcome the referenced values, such as `val` in the where clause
            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

            var projection = projectedExp.Source;
            Assert.That(projection.Columns.Count, Is.EqualTo(2));
            Assert.AreEqual(projection.Columns.Select(p => p.Name).Intersect(columnNames), columnNames);
            Assert.AreEqual(((MapExpression)((SelectExpression)projection.From).From).Alias, nameof(DummyType));//redundant queries are another PR's deal.

            var where = projectedExp.Source.Where as BinaryExpression;
            Assert.AreEqual(where.NodeType, ExpressionType.Equal);
            Assert.AreEqual(((ColumnExpression)where.Left).Name, nameof(DummyType.ColumnInteger));
            Assert.AreEqual(((ConstantExpression)where.Right).Value, val);
        }

    }
}
