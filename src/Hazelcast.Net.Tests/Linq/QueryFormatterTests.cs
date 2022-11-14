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
using System.Linq.Expressions;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    public class QueryFormatterTests
    {
        private class DummyType
        {
            public string ColumnString { get; set; }
            public double ColumnDouble { get; set; }
            public float ColumnFloat { get; set; }
            public int ColumnInt { get; set; }
            public long ColumnLong { get; set; }
            public bool ColumnBool { get; set; }
            public HzExpressionType ColumnEnum { get; set; }
        }

        class DummyExpression : Expression
        {
#if NETSTANDARD2_1_OR_GREATER
            public override ExpressionType NodeType { get; }

            public DummyExpression(ExpressionType nodeType)
            {
                NodeType = nodeType;
            }
#else
            public DummyExpression(ExpressionType nodeType) : base(nodeType, typeof(Expression))
            { }
#endif
        }

        [TestCase(ExpressionType.Throw, ExpectedResult = false)]
        [TestCase(ExpressionType.And, ExpectedResult = true)]
        [TestCase(ExpressionType.AndAlso, ExpectedResult = true)]
        [TestCase(ExpressionType.Or, ExpectedResult = true)]
        [TestCase(ExpressionType.OrElse, ExpectedResult = true)]
        [TestCase(ExpressionType.Not, ExpectedResult = true)]
        [TestCase(ExpressionType.Constant, ExpectedResult = true)]
        [TestCase(ExpressionType.Divide, ExpectedResult = true)]
        [TestCase(ExpressionType.Convert, ExpectedResult = true)]
        [TestCase(ExpressionType.Modulo, ExpectedResult = true)]
        [TestCase(ExpressionType.GreaterThan, ExpectedResult = true)]
        [TestCase(ExpressionType.GreaterThanOrEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.LessThan, ExpectedResult = true)]
        [TestCase(ExpressionType.LessThanOrEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.NotEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.Equal, ExpectedResult = true)]
        [TestCase(ExpressionType.Multiply, ExpectedResult = true)]
        [TestCase(ExpressionType.Subtract, ExpectedResult = true)]
        [TestCase(ExpressionType.Parameter, ExpectedResult = true)]
        [TestCase((ExpressionType) HzExpressionType.Map, ExpectedResult = true)]
        [TestCase((ExpressionType) HzExpressionType.Column, ExpectedResult = true)]
        [TestCase((ExpressionType) HzExpressionType.Projection, ExpectedResult = true)]
        [TestCase((ExpressionType) HzExpressionType.Select, ExpectedResult = true)]
        [TestCase((ExpressionType) HzExpressionType.Join, ExpectedResult = true)]
        public bool TestFormatSupportedTypes(ExpressionType nodeType)
        {
            var exp = new DummyExpression(nodeType);

            try
            {
                QueryFormatter.Format(exp);
            }
            catch (Exception ex)
            {
                // We expect that because our dummy node type is too dummy to be visited.
                // If we got the exception, then means that the type is supported but couldn't be visited. 
                if ((ex is ArgumentException && ex.Message == "must be reducible node")
                    || (ex is InvalidCastException &&
                        ex.Message.StartsWith("Unable to cast object of type 'DummyExpression'")))
                    return true;

                throw;
            }

            return true;
        }

        [TestCase(nameof(DummyType.ColumnString), null,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnString IS NULL)")]
        [TestCase(nameof(DummyType.ColumnString), "str-value",
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnString = ?)")]
        [TestCase(nameof(DummyType.ColumnBool), true,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnBool = ?)")]
        [TestCase(nameof(DummyType.ColumnFloat), 1.1f,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnFloat = ?)")]
        [TestCase(nameof(DummyType.ColumnDouble), 1.1d,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnDouble = ?)")]
        [TestCase(nameof(DummyType.ColumnInt), 1,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt = ?)")]
        [TestCase(nameof(DummyType.ColumnLong), 1L,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnLong = ?)")]
        [TestCase(nameof(DummyType.ColumnEnum), HzExpressionType.Column,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnEnum = ?)")]
        public string TestValueTypesOnQuery(string columnName, object? val)
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsQueryable();

            switch (columnName)
            {
                case nameof(DummyType.ColumnString):
                    var str = (string?) val;
                    exp = exp.Where(p => p.ColumnString == str);
                    break;
                case nameof(DummyType.ColumnDouble):
                    var db = (double?) val;
                    exp = exp.Where(p => p.ColumnDouble == db);
                    break;
                case nameof(DummyType.ColumnLong):
                    var lng = (long?) val;
                    exp = exp.Where(p => p.ColumnLong == lng);
                    break;
                case nameof(DummyType.ColumnFloat):
                    var flt = (float?) val;
                    exp = exp.Where(p => p.ColumnFloat == flt);
                    break;
                case nameof(DummyType.ColumnInt):
                    var num = (int?) val;
                    exp = exp.Where(p => p.ColumnInt == num);
                    break;
                case nameof(DummyType.ColumnBool):
                    var bln = (bool?) val;
                    exp = exp.Where(p => p.ColumnBool == bln);
                    break;
                case nameof(DummyType.ColumnEnum):
                    var enm = (HzExpressionType) val!;
                    exp = exp.Where(p => p.ColumnEnum == enm);
                    break;
            }

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var boundExp = (ProjectionExpression) new QueryBinder().Bind(evaluated) as Expression;

            boundExp = UnusedColumnProcessor.Clean(boundExp);
            boundExp = RedundantSubqueryProcessor.Clean(boundExp);
            var (query, values) = QueryFormatter.Format(boundExp);

            if (val != null && !val.GetType().IsEnum)
                Assert.That(values, Contains.Item(val));
            else if (val != null && val.GetType().IsEnum)
                Assert.That(values, Contains.Item((int) val));

            return query;
        }

        [TestCase("Add", ExpectedResult = "+")]
        [TestCase("Subtract", ExpectedResult = "-")]
        [TestCase("Multiply", ExpectedResult = "*")]
        [TestCase("Divide", ExpectedResult = "/")]
        [TestCase("Negate", ExpectedResult = "-")]
        [TestCase("Remainder", ExpectedResult = "%")]
        [TestCase("NotSupportedOpt", ExpectedResult = null)]
        public string TestGetOperator(string name)
        {
            var qf = new QueryFormatter();
            return qf.GetOperator(name);
        }

        [TestCase(ExpressionType.Negate, false, ExpectedResult = "-")]
        [TestCase(ExpressionType.UnaryPlus, false, ExpectedResult = "+")]
        [TestCase(ExpressionType.Not, false, ExpectedResult = "NOT")]
        [TestCase(ExpressionType.AndAlso, true, ExpectedResult = "AND")]
        [TestCase(ExpressionType.Or, true, ExpectedResult = "OR")]
        [TestCase(ExpressionType.OrElse, true, ExpectedResult = "OR")]
        [TestCase(ExpressionType.Equal, true, ExpectedResult = "=")]
        [TestCase(ExpressionType.NotEqual, true, ExpectedResult = "!=")]
        [TestCase(ExpressionType.LessThan, true, ExpectedResult = "<")]
        [TestCase(ExpressionType.LessThanOrEqual, true, ExpectedResult = "<=")]
        [TestCase(ExpressionType.GreaterThan, true, ExpectedResult = ">")]
        [TestCase(ExpressionType.GreaterThanOrEqual, true, ExpectedResult = ">=")]
        [TestCase(ExpressionType.Add, true, ExpectedResult = "+")]
        [TestCase(ExpressionType.AddChecked, true, ExpectedResult = "+")]
        [TestCase(ExpressionType.Subtract, true, ExpectedResult = "-")]
        [TestCase(ExpressionType.SubtractChecked, true, ExpectedResult = "-")]
        [TestCase(ExpressionType.Multiply, true, ExpectedResult = "*")]
        [TestCase(ExpressionType.MultiplyChecked, true, ExpectedResult = "*")]
        [TestCase(ExpressionType.Divide, true, ExpectedResult = "/")]
        public string TestGetOperatorAsExpression(ExpressionType type, bool isBinary)
        {
            var qf = new QueryFormatter();
            var exp = MakeExpressionBy(type, isBinary);
            return isBinary ? qf.GetOperator(((BinaryExpression) exp)!) : qf.GetOperator(((UnaryExpression) exp)!);
        }

        private static Expression MakeExpressionBy(ExpressionType type, bool isBinary)
        {
            Expression exp;
            var arg1 = Expression.Constant(1);
            var arg2 = Expression.Constant(2);
            var name = type.ToString();
            switch (isBinary)
            {
                case true when type is ExpressionType.AndAlso or ExpressionType.Or or ExpressionType.OrElse:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression), typeof(Expression)})
                        ?.Invoke(null, new object[] {Expression.Constant(true), Expression.Constant(true)});
                    break;
                case true:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression), typeof(Expression)})
                        ?.Invoke(null, new object[] {arg1, arg2});
                    break;
                default:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression)})
                        ?.Invoke(null, new object[] {arg1});
                    break;
            }

            return exp;
        }

        [TestCase(ExpressionType.And)]
        public void TestIsPredicate(ExpressionType type)
        {
            var isBinary = type != ExpressionType.Not;
            var exp = MakeExpressionBy(type, isBinary);
            var qf = new QueryFormatter();
            Assert.True(qf.IsPredicate(exp.Reduce()));
        }

        [Test]
        public void TestCustom()
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsQueryable()
                .Select(p => new {p.ColumnInt, p.ColumnString})
                .Where(p => p.ColumnInt > 1 && p.ColumnInt < 10)
                .Where(p => p.ColumnString == "asd" || p.ColumnString == "cvb");

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var boundExp = (ProjectionExpression) new QueryBinder().Bind(evaluated) as Expression;

            boundExp = UnusedColumnProcessor.Clean(boundExp);
            boundExp = RedundantSubqueryProcessor.Clean(boundExp);
            var (query, values) = QueryFormatter.Format(boundExp);
            Console.WriteLine(query);
        }
    }
}
