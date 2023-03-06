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
using System.Linq;
using System.Linq.Expressions;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;
using Hazelcast.Testing.Linq;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    public class QueryBuildingTests
    {
        public class DummyType
        {
            public int ColumnInteger { get; set; }
            public string ColumnString { get; set; }
        }


        public static string[] ColumnNames => typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

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
            var exp = dummyData.AsTestingAsyncQueryable().Where(p => p.ColumnInteger == val);

            //Overcome the referenced values, such as `val` in the where clause
            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);
            var columnNames = typeof(DummyType).GetProperties().Select(p => p.Name).ToArray();

            //projection aka select
            var projection = projectedExp.Source as SelectExpression;
            Assert.That(projection.Columns.Count, Is.EqualTo(ColumnNames.Count()));
            Assert.AreEqual(projection.Columns.Select(p => p.Name).Intersect(ColumnNames), ColumnNames);
            Assert.AreEqual(((MapExpression)((SelectExpression)projection.From).From).Name, nameof(DummyType));//redundandt queries are another PR's deal.

            var where = projectedExp.Source.Where as BinaryExpression;
            Assert.AreEqual(where.NodeType, ExpressionType.Equal);
            Assert.AreEqual(((ColumnExpression)where.Left).Name, nameof(DummyType.ColumnInteger));
            Assert.AreEqual(((ConstantExpression)where.Right).Value, val);
        }

        [Test]
        public void TestQuerySelectBinderBindsCorrecytly()
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsTestingAsyncQueryable().Select(p => p.ColumnInteger);

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
                .AsTestingAsyncQueryable()
                // the other data source must be also Queryable. Otherwise, it will be interpreted as a (constant)value on expression tree which
                // doesn't work for us. Note: HMap is Queryable. It will be used as joined data source in normal usage. Current usage
                // is only for testing.
                .Join(dummyData2.AsTestingAsyncQueryable(),
                o => o.ColumnInteger,
                i => i.ColumnInteger,
                tempPredicate);

            var evaluated = ExpressionEvaluator.EvaluatePartially(query.Expression);

            var projectedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated);


            var projection = projectedExp.Source as SelectExpression;

            Assert.AreEqual((ExpressionType)HzExpressionType.Join, projectedExp.Source.From.NodeType);
            Assert.AreEqual(nameof(DummyType.ColumnInteger),
                ((MemberExpression)((BinaryExpression)((JoinExpression)projectedExp.Source.From).JoinCondition).Left).Member.Name);

            Assert.AreEqual(nameof(DummyType.ColumnInteger),
                ((MemberExpression)((BinaryExpression)((JoinExpression)projectedExp.Source.From).JoinCondition).Right).Member.Name);

            Assert.AreEqual(ExpressionType.Equal, ((JoinExpression)projectedExp.Source.From).JoinCondition.NodeType);

        }

        [Test]
        public void TestRemoveRedundantProjections()
        {
            bool DoesConditionExist<T>(Expression exp, string column, ExpressionType operatorType, T value)
            {
                if (exp == null) return false;

                if (exp is BinaryExpression condition
                    && condition.Left is ColumnExpression c1
                    && c1.Name == column && condition.NodeType == operatorType
                    && condition.Right is ConstantExpression c2 && c2.Value is T c2Val && c2Val.Equals(value))
                {
                    return true;
                }

                return exp switch
                {
                    SelectExpression s => DoesConditionExist(s.From, column, operatorType, value) || DoesConditionExist(s.Where, column, operatorType, value),
                    ProjectionExpression p => DoesConditionExist(p.Source, column, operatorType, value),
                    BinaryExpression b => DoesConditionExist(b.Left, column, operatorType, value) || DoesConditionExist(b.Right, column, operatorType, value),
                    _ => false,
                };
            }

            var dummyData = new List<DummyType>();
            var val = 10;
            var exp = dummyData.AsTestingAsyncQueryable()
                .Where(p => p.ColumnString == "param1")
                .Where(p => p.ColumnString == "param2")
                .Select(p => new { i = p.ColumnInteger })
                .Where(p => p.i > val)
                .Select(p => p.i);

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var bindedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated) as Expression;

            bindedExp = UnusedColumnProcessor.Clean(bindedExp);
            bindedExp = RedundantSubQueryProcessor.Clean(bindedExp);
            var projected = bindedExp as ProjectionExpression;

            // unused columns and sub queries are removed.
            // Only one of the columns are used.
            Assert.That(projected.Source.Columns.Count, Is.EqualTo(1));
            Assert.AreEqual(projected.Source.Columns[0].Name, nameof(DummyType.ColumnInteger));
            //One layer query, From clause should point to Map directly.
            Assert.AreEqual(projected.Source.From.NodeType, (ExpressionType)HzExpressionType.Map);
            Assert.IsInstanceOf(typeof(BinaryExpression), projected.Source.Where);

            Assert.True(DoesConditionExist<string>(projected, nameof(DummyType.ColumnString), ExpressionType.Equal, "param1"));
            Assert.True(DoesConditionExist<string>(projected, nameof(DummyType.ColumnString), ExpressionType.Equal, "param2"));
            Assert.True(DoesConditionExist<int>(projected, nameof(DummyType.ColumnInteger), ExpressionType.GreaterThan, val));
        }

    }
}