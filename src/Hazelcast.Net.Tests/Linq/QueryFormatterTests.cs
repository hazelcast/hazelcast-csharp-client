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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml.Xsl;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;
using Hazelcast.Testing.Linq;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    public class QueryFormatterTests
    {
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
        [TestCase(ExpressionType.Negate, ExpectedResult = true)]
        [TestCase(ExpressionType.NegateChecked, ExpectedResult = true)]
        [TestCase(ExpressionType.UnaryPlus, ExpectedResult = true)]
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

                if (ex is NotSupportedException)
                    return false;

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
        [TestCase("noColumn", null,
            ExpectedResult =
                "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0")]
        public string TestValueTypesOnQuery(string columnName, object? val)
        {
            var exp = GetQuery();

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
                    var enm = (ExpressionType) val!;
                    exp = exp.Where(p => p.ColumnEnum == enm);
                    break;
            }

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var boundExp = (ProjectionExpression) new QueryBinder().Bind(evaluated) as Expression;

            boundExp = UnusedColumnProcessor.Clean(boundExp);
            boundExp = RedundantSubQueryProcessor.Clean(boundExp);
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

            try
            {
                return qf.GetOperator(name);
            }
            catch (NotSupportedException)
            {
                return null;
            }
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
        [TestCase(ExpressionType.Quote, true, ExpectedResult = null)]
        public string TestGetOperatorAsExpression(ExpressionType type, bool isBinary)
        {
            var qf = new QueryFormatter();
            var exp = MakeExpressionBy(type, isBinary, true);
            try
            {
                return isBinary ? qf.GetOperator(((BinaryExpression) exp)!) : qf.GetOperator(((UnaryExpression) exp)!);
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        [TestCase(ExpressionType.AndAlso, false)]
        [TestCase(ExpressionType.And, false)]
        [TestCase(ExpressionType.Or, false)]
        [TestCase(ExpressionType.OrElse, false)]
        [TestCase(ExpressionType.Not, false)]
        [TestCase(ExpressionType.Equal, false)]
        [TestCase(ExpressionType.NotEqual, false)]
        [TestCase(ExpressionType.LessThan, true)]
        [TestCase(ExpressionType.LessThanOrEqual, true)]
        [TestCase(ExpressionType.GreaterThan, true)]
        [TestCase(ExpressionType.GreaterThanOrEqual, true)]
        public void TestIsPredicate(ExpressionType type, bool argAsNumb)
        {
            // Predicate should return boolean to be a predicate.
            // So, have expressions with boolean args.
            var isBinary = type != ExpressionType.Not;
            var exp = MakeExpressionBy(type, isBinary, argAsNumb);
            var qf = new QueryFormatter();
            Assert.True(qf.IsPredicate(exp));
        }

        [Test]
        public void TestEqual()
        {
            var val = "myvalue";
            object val2 = 3;
            var q = GetQuery()
                .Where(p => p.ColumnString.Equals(val) || object.Equals(val2, p.ColumnInt))
                .Select(p => p.ColumnString);

            var exp = HandleExpression(q);
            var (query, values) = QueryFormatter.Format(exp);

            Assert.AreEqual(
                "SELECT m0.ColumnString FROM DummyType m0 WHERE ((m0.ColumnString = ?) OR (? = m0.ColumnInt))",
                query);
            Assert.True(values.Contains(val));
            Assert.True(values.Contains(val2));
        }

        [Test]
        public void TestToString()
        {
            var val = 99;
            var q = GetQuery()
                .Where(p => p.ColumnString == val.ToString())
                .Select(p => p.ColumnString);

            var exp = HandleExpression(q);
            var (query, values) = QueryFormatter.Format(exp);

            Assert.AreEqual("SELECT m0.ColumnString FROM DummyType m0 WHERE (m0.ColumnString = ?)", query);
            Assert.True(values.Contains(val.ToString()));
        }

        [Test]
        public void TestUnaryOperations()
        {
            var val = 99;
            var q = GetQuery()
                .Where(p => +p.ColumnInt > +val || -p.ColumnInt > -val || !p.ColumnBool)
                .Select(p => p.ColumnString);

            var exp = HandleExpression(q);
            var (query, values) = QueryFormatter.Format(exp);

            // + sign doesn't change the sign of the value, so we don't write it.
            // Also, note that third expression on the where is in another level of parenthesis. 
            // That is because of nature of the structure. But it doesn't effect the logical result.
            // Like;
            // BinaryExp(Left: BinaryExp(Left: Expression1, Right: Expression2), Right: Expression3)
            Assert.AreEqual(
                "SELECT m0.ColumnString FROM DummyType m0 WHERE (((m0.ColumnInt > ?) OR (-m0.ColumnInt > ?)) OR NOT m0.ColumnBool != FALSE)",
                query);
            Assert.True(values.Contains(+val));
            Assert.True(values.Contains(-val));
        }

        [Test]
        public void TestProjectFewColumns()
        {
            var exp = GetQuery().Select(p => new {p.ColumnInt, p.ColumnString});
            var boundExp = HandleExpression(exp);

            var (query, values) = QueryFormatter.Format(boundExp);

            Assert.AreEqual("SELECT m0.ColumnInt, m0.ColumnString FROM DummyType m0", query);
        }

        [TestCaseSource(nameof(QueryFormatterTests.ConditionCases))]
        public void TestWhereCondition(ValueTuple<Expression<Func<DummyType, bool>>, string, IEnumerable<object>> t)
        {
            var exp = GetQuery().Where(t.Item1);

            var (q, v) = QueryFormatter.Format(HandleExpression(exp));

            Assert.AreEqual(t.Item2, q);
            Assert.That(t.Item3, Is.EquivalentTo(v));
        }

        #region Helpers

        // Test cases for where clause.
        // Predicate, Query, Values
        private static IEnumerable<(Expression<Func<DummyType, bool>>, string, IEnumerable<object>)> ConditionCases
        {
            get
            {
                yield return ((DummyType p) => p.ColumnBool == true,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnBool = ?)",
                    new object[] {true});
                yield return ((DummyType p) => p.ColumnInt > 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt > ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnInt < 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt < ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnInt == 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt = ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnInt >= 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt >= ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnInt <= 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt <= ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnInt != 0,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnInt != ?)",
                    new object[] {0});
                yield return ((DummyType p) => p.ColumnString != null,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnString IS NOT NULL)",
                    new object[] { });
                yield return ((DummyType p) => null != p.ColumnString,
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE (m0.ColumnString IS NOT NULL)",
                    new object[] { });
                yield return ((DummyType p) => p.ColumnInt != 0 && p.ColumnString != 7.ToString(),
                    "SELECT m0.ColumnString, m0.ColumnDouble, m0.ColumnFloat, m0.ColumnInt, m0.ColumnLong, m0.ColumnBool, m0.ColumnEnum FROM DummyType m0 WHERE ((m0.ColumnInt != ?) AND (m0.ColumnString != ?))",
                    new object[] {0, "7"});
            }
        }

        public class DummyType
        {
            public string ColumnString { get; set; }
            public double ColumnDouble { get; set; }
            public float ColumnFloat { get; set; }
            public int ColumnInt { get; set; }
            public long ColumnLong { get; set; }
            public bool ColumnBool { get; set; }
            public ExpressionType ColumnEnum { get; set; }
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

        private static IAsyncQueryable<DummyType> GetQuery()
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsTestingAsyncQueryable();
            return exp;
        }

        private static Expression MakeExpressionBy(ExpressionType type, bool isBinary, bool argsAsNumb)
        {
            Expression exp;
            var arg1 = argsAsNumb ? Expression.Constant(1) : Expression.Constant(true);
            var arg2 = argsAsNumb ? Expression.Constant(1) : Expression.Constant(true);
            var name = type.ToString();
            switch (isBinary)
            {
                case true when type is ExpressionType.AndAlso or ExpressionType.Or or ExpressionType.OrElse:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression), typeof(Expression)})
                        ?.Invoke(null, new object[] {Expression.Constant(true), Expression.Constant(true)});
                    break;
                case true when type is ExpressionType.Quote: // We don't support Quote, using it for testing purposes 
                    exp = Expression.LeftShift(arg1, arg2);
                    break;
                case true:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression), typeof(Expression)})
                        ?.Invoke(null, new object[] {arg1, arg2});
                    break;
                case false when type is ExpressionType.Quote:
                    exp = Expression.Quote(() => 1);
                    break;
                default:
                    exp = (Expression) typeof(Expression)
                        .GetMethod(name, new[] {typeof(Expression)})
                        ?.Invoke(null, new object[] {arg1});
                    break;
            }

            return exp;
        }

        private static Expression HandleExpression(IAsyncQueryable exp)
        {
            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var boundExp = (ProjectionExpression) new QueryBinder().Bind(evaluated) as Expression;

            boundExp = UnusedColumnProcessor.Clean(boundExp);
            boundExp = RedundantSubQueryProcessor.Clean(boundExp);
            return boundExp;
        }

        #endregion
    }
}
