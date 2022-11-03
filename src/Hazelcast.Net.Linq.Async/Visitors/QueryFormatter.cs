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
using System.Linq.Expressions;
using System.Text;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Traverse and translete tree to SQL string statement.
    /// </summary>
    internal class QueryFormatter : HzExpressionVisitor
    {
        private StringBuilder _sb;
        private int _indentSize = 2;
        private bool _isDebug = false;

        private QueryFormatter()
        {
            _sb = new();
        }

        public static string Format(Expression expression)
        {
            var f = new QueryFormatter();
            f.Visit(expression);
            return f.ToString();
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public override Expression Visit(Expression node)
        {
            if (node == null) return node;

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Not:
                case ExpressionType.Constant:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.Equal:
                case ExpressionType.Multiply:
                case ExpressionType.Subtract:
                case ExpressionType.Parameter:
                case (ExpressionType)HzExpressionType.Map:
                case (ExpressionType)HzExpressionType.Column:
                case (ExpressionType)HzExpressionType.Projection:
                case (ExpressionType)HzExpressionType.Select:
                case (ExpressionType)HzExpressionType.Join:
                    return base.Visit(node);
                default:
                    throw new NotSupportedException($"{node.NodeType} is not supported.");
            }
        }

        protected override Expression VisitSelect(SelectExpression node)
        {
            return base.VisitSelect(node);
        }
    }
}
