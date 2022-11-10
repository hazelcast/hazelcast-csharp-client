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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using Hazelcast.Linq.Expressions;
using Ionic.Zip;

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
        private List<object> _values;

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

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override Expression Visit(Expression node)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return node;
#pragma warning restore CS8603 // Possible null reference return.

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


        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (column.Alias is not null)
            {
                Write(column.Alias);
                Write(".");
            }
            Write("`");
            Write(column.Name);
            Write("`");
            return column;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
        {
            Write("(");
            var _ = Visit(proj.Source);
            Write(")");
            return proj;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            Write("SELECT ");
            WriteColumns(select.Columns);

            if (select.From is not null)
            {
                Write("FROM ");
                VisitSource(select.From);
            }

            if (select.Where is not null)
            {
                Write("WHERE");
                VisitPredicate(select.Where);
            }

            return select;
        }

        protected virtual Expression VisitSource(Expression from)
        {
            switch ((HzExpressionType)from.NodeType)
            {
                case HzExpressionType.Map:
                    WriteMapName((MapExpression)from);
                    break;
                case HzExpressionType.Select:
                    Write("(");
                    Visit(from);
                    Write(") ");
                    Write(((SelectExpression)from).Alias);
                    break;
                case HzExpressionType.Join:
                    VisitJoin((JoinExpression)from);
                    break;
            }

            return from;
        }


        protected override Expression VisitJoin(JoinExpression join)
        {
            VisitSource(join.Left);
            Write("INNER JOIN "); // TODO: PM -> implement other types of join?
            VisitSource(join.Right);
            Write("ON ");
            VisitPredicate(join.JoinCondition);
            return join;
        }

        protected virtual Expression VisitPredicate(Expression predicate)
        {
            Visit(predicate);
            if (!IsPredicate(predicate))
            {
                Write(" <> 0");
            }
            return predicate;
        }

        #region Helpers
        private void WriteColumns(ReadOnlyCollection<ColumnDefinition> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (i > 0)
                    Write(", ");

                var visitedColumn = Visit(column.Expression) as ColumnExpression;

                if (!string.IsNullOrEmpty(column.Name) && (visitedColumn is null || column.Name != visitedColumn.Name))
                {
                    Write(" ");
                    Write(column.Name);
                }
            }

            if (columns.Count > 0)
                Write(" ");
        }

        private void WriteMapName(MapExpression from)
        {
            Write(from.Name);
            Write(" ");
            Write(from.Alias);
        }

        private bool IsPredicate(Expression predicate)
        {
            switch (predicate.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(((BinaryExpression)predicate).Type);
                case ExpressionType.Not:
                    return IsBoolean(((UnaryExpression)predicate).Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(((MethodCallExpression)predicate).Type);
                default:
                    return false;
            }
        }

        protected virtual string GetOperator(BinaryExpression b)
        {
            return b.NodeType switch
            {
                ExpressionType.And or ExpressionType.AndAlso => (IsBoolean(b.Left.Type)) ? "AND" : "&",
                ExpressionType.Or or ExpressionType.OrElse => (IsBoolean(b.Left.Type) ? "OR" : "|"),
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.Add or ExpressionType.AddChecked => "+",
                ExpressionType.Subtract or ExpressionType.SubtractChecked => "-",
                ExpressionType.Multiply or ExpressionType.MultiplyChecked => "*",
                ExpressionType.Divide => "/",
                ExpressionType.Modulo => "%",
                ExpressionType.ExclusiveOr => "^",
                ExpressionType.LeftShift => "<<",
                ExpressionType.RightShift => ">>",
                _ => "",
            };
        }

        protected virtual string GetOperator(UnaryExpression u)
        {
            return u.NodeType switch
            {
                ExpressionType.Negate or ExpressionType.NegateChecked => "-",
                ExpressionType.UnaryPlus => "+",
                ExpressionType.Not => "NOT",
                _ => "",
            };
        }

        protected virtual string? GetOperator(string methodName)
        {
            return methodName switch
            {
                "Add" => "+",
                "Subtract" => "-",
                "Multiply" => "*",
                "Divide" => "/",
                "Negate" => "-",
                "Remainder" => "%",
                _ => null,
            };
        }

        private bool IsBoolean(Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        private void Write(string v)
        {
            _sb.Append(v);
        }
        #endregion
    }
}
