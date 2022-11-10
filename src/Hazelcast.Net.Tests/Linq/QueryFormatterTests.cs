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
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;
using Hazelcast.Linq.Visitors;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    internal class QueryFormatterTests
    {
        private class DummyType
        {
            public int ColumnInteger { get; set; }
            public string ColumnString { get; set; }
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
            public DummyExpression(ExpressionType nodeType) : base(nodeType, typeof(Expression)) { }
#endif

        }

        [Test]
        [TestCase(ExpressionType.Throw, ExpectedResult = false)]
        [TestCase(ExpressionType.And, ExpectedResult = true)]
        [TestCase(ExpressionType.AndAlso, ExpectedResult = true)]
        [TestCase(ExpressionType.Or, ExpectedResult = true)]
        [TestCase(ExpressionType.OrElse, ExpectedResult = true)]
        [TestCase(ExpressionType.Not, ExpectedResult = true)]
        [TestCase(ExpressionType.Constant, ExpectedResult = true)]
        [TestCase(ExpressionType.Divide, ExpectedResult = true)]
        [TestCase(ExpressionType.Modulo, ExpectedResult = true)]
        [TestCase(ExpressionType.ExclusiveOr, ExpectedResult = true)]
        [TestCase(ExpressionType.GreaterThan, ExpectedResult = true)]
        [TestCase(ExpressionType.GreaterThanOrEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.LessThan, ExpectedResult = true)]
        [TestCase(ExpressionType.LessThanOrEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.NotEqual, ExpectedResult = true)]
        [TestCase(ExpressionType.Equal, ExpectedResult = true)]
        [TestCase(ExpressionType.Multiply, ExpectedResult = true)]
        [TestCase(ExpressionType.Subtract, ExpectedResult = true)]
        [TestCase(ExpressionType.Parameter, ExpectedResult = true)]
        [TestCase((ExpressionType)HzExpressionType.Map, ExpectedResult = true)]
        [TestCase((ExpressionType)HzExpressionType.Column, ExpectedResult = true)]
        [TestCase((ExpressionType)HzExpressionType.Projection, ExpectedResult = true)]
        [TestCase((ExpressionType)HzExpressionType.Select, ExpectedResult = true)]
        [TestCase((ExpressionType)HzExpressionType.Join, ExpectedResult = true)]

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
                   || (ex is InvalidCastException && ex.Message.StartsWith("Unable to cast object of type 'DummyExpression'")))
                    return true;

                throw;
            }


            return true;
        }

        [Test]
        public void TestFormatSelectQuery()
        {
            var dummyData = new List<DummyType>();
            var exp = dummyData.AsQueryable();

            var evaluated = ExpressionEvaluator.EvaluatePartially(exp.Expression);
            var bindedExp = (ProjectionExpression)new QueryBinder().Bind(evaluated) as Expression;

            bindedExp = UnusedColumnProcessor.Clean(bindedExp);
            bindedExp = RedundantSubqueryProcessor.Clean(bindedExp);
            var formattedQuery = QueryFormatter.Format(bindedExp);
            Console.WriteLine(formattedQuery);

        }
    }
}
