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
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Linq.Expressions;
using Ionic.Zip;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Traverse and translate tree to SQL string statement.
    /// </summary>
    internal class QueryFormatter : HzExpressionVisitor
    {
        private StringBuilder _sb;
        private List<object> _values;

        // internal for test
        internal QueryFormatter()
        {
            _sb = new();
            _values = new();
        }

        /// <summary>
        /// Translate the tree into SQL statement
        /// </summary>
        /// <param name="expression">tree</param>
        /// <returns>(SQL statement, Value of variables)</returns>
        public static (string, object[]) Format(Expression expression)
        {
            var f = new QueryFormatter();
            f.Visit(expression);
            return (f.ToString(), f._values.ToArray());
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public override Expression Visit(Expression? node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node is null) return node;
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
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.Equal:
                case ExpressionType.Multiply:
                case ExpressionType.Subtract:
                case ExpressionType.Parameter:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Call:
                case ExpressionType.UnaryPlus:
                case ExpressionType.MemberAccess:
                case (ExpressionType) HzExpressionType.Map:
                case (ExpressionType) HzExpressionType.Column:
                case (ExpressionType) HzExpressionType.Projection:
                case (ExpressionType) HzExpressionType.Select:
                case (ExpressionType) HzExpressionType.Join:
                    return base.Visit(node);
                case ExpressionType.Convert:
                    if (node.GetType().IsNullable())
                        return base.Visit(node);
                    throw new NotSupportedException($"{node.NodeType} is not supported.");
                default:
                    throw new NotSupportedException($"{node.NodeType} is not supported.");
            }
        }

        internal override Expression VisitColumn(ColumnExpression column)
        {
            if (!string.IsNullOrEmpty(column.Alias))
            {
                Write(column.Alias);
                Write(".");
            }

            Write(column.Name);
            return column;
        }

        internal override Expression VisitProjection(ProjectionExpression proj)
        {
            var _ = Visit(proj.Source);
            return proj;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            WriteValue(node.Value);
            return node;
        }

        private void WriteValue(object? val)
        {
            if (val is null)
                Write("NULL");
            else if (val.GetType().IsEnum)
                WriteParameter(val);
            else
            {
                var valType = val.GetType().IsNullable()
                    ? Type.GetTypeCode(Nullable.GetUnderlyingType(val.GetType()))
                    : Type.GetTypeCode(val.GetType());
                switch (valType)
                {
                    case TypeCode.Boolean:
                        WriteParameter(val);
                        break;
                    case TypeCode.String:
                        WriteParameter(val);
                        break;
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Decimal:
                        WriteParameter(val);
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{val}' is not supported.");
                    default:
                        WriteParameter((val as IConvertible)?.ToString(CultureInfo.InvariantCulture) ?? val);
                        break;
                }
            }
        }

        private void WriteParameter(object v)
        {
            Write("?");
            _values.Add(v);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "ToString" && node.Object?.Type == typeof(string))
            {
                Visit(node.Object);
            }
            else if (node.Method.Name == "Equals")
            {
                var op = GetOperator("Equal");
                switch (node.Method.IsStatic)
                {
                    case true when node.Method.DeclaringType == typeof(object):
                        Write("(");
                        Visit(node.Arguments[0]);
                        Write(" " + op + " ");
                        Visit(node.Arguments[1]);
                        Write(")");
                        break;
                    case false when node.Arguments.Count == 1 && node.Arguments[0].Type == node.Object?.Type:
                        Write("(");
                        Visit(node.Object!);
                        Write(" " + op + " ");
                        Visit(node.Arguments[0]);
                        Write(")");
                        break;
                }
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            var op = GetOperator(u);
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    Write(op);
                    Write(" ");
                    VisitPredicate(u.Operand);
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    Write(op);
                    Visit(u.Operand);
                    break;
                case ExpressionType.UnaryPlus:
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression bExp)
        {
            var op = GetOperator(bExp);
            var left = bExp.Left;
            var right = bExp.Right;

            void VisitAndWriteInOrder(Expression l, Expression r, string o)
            {
                Visit(l);
                Write(" ");
                Write(o);
                Write(" ");
                Visit(r);
            }

            Write("(");
            switch (bExp.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (IsBoolean(left.Type))
                    {
                        VisitPredicate(left);
                        Write(" ");
                        Write(op);
                        Write(" ");
                        VisitPredicate(right);
                    }
                    else
                    {
                        VisitAndWriteInOrder(left, right, op);
                    }

                    break;
                case ExpressionType.Equal:
                    // Something == null
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        var ce = (ConstantExpression) right;
                        if (ce.Value == null)
                        {
                            Visit(left);
                            Write(" IS NULL");
                            break;
                        }
                    }
                    // null == Something
                    else if (left.NodeType == ExpressionType.Constant)
                    {
                        var ce = (ConstantExpression) left;
                        if (ce.Value == null)
                        {
                            Visit(right);
                            Write(" IS NULL");
                            break;
                        }
                    }

                    VisitAndWriteInOrder(left, right, op);
                    break;
                case ExpressionType.NotEqual:
                    // Something != null
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        var ce = (ConstantExpression) right;
                        if (ce.Value == null)
                        {
                            Visit(left);
                            Write(" IS NOT NULL");
                            break;
                        }
                    }
                    // null != Something
                    else if (left.NodeType == ExpressionType.Constant)
                    {
                        var ce = (ConstantExpression) left;
                        if (ce.Value == null)
                        {
                            Visit(right);
                            Write(" IS NOT NULL");
                            break;
                        }
                    }

                    VisitAndWriteInOrder(left, right, op);
                    break;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.MemberAccess:
                    VisitAndWriteInOrder(left, right, op);
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{bExp.NodeType}' is not supported");
            }

            Write(")");
            return bExp;
        }

        internal override Expression VisitSelect(SelectExpression select)
        {
            Write("SELECT ");
            WriteColumns(select.Columns);

            Write("FROM ");
            VisitSource(select.From);

            if (select.Where is null) return select;

            Write(" WHERE ");
            VisitPredicate(select.Where);

            return select;
        }

        protected virtual Expression VisitSource(Expression from)
        {
            switch ((HzExpressionType) from.NodeType)
            {
                case HzExpressionType.Map:
                    WriteMapName((MapExpression) from);
                    break;
                // Currently no support for:
                // case HzExpressionType.Join:
                // case HzExpressionType.Select -> Nested Query.
            }

            return from;
        }

        protected virtual Expression VisitPredicate(Expression predicate)
        {
            Visit(predicate);

            if (!IsPredicate(predicate))
                Write(" != FALSE");

            return predicate;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                if( node.Member.Name=="Key")
                    Write("__key");
            }

            return node;
        }

        #region Helpers

        private void WriteColumns(IReadOnlyList<ColumnDefinition> columns)
        {
            for (var i = 0; i < columns.Count; i++)
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

        // internal for tests
        internal bool IsPredicate(Expression predicate)
        {
            switch (predicate.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return IsBoolean(((BinaryExpression) predicate).Type);
                case ExpressionType.Not:
                    return IsBoolean(((UnaryExpression) predicate).Type);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    return true;
                case ExpressionType.Call:
                    return IsBoolean(((MethodCallExpression) predicate).Type);
                default:
                    return false;
            }
        }

        // internal for test
        internal virtual string GetOperator(BinaryExpression b)
        {
            return b.NodeType switch
            {
                ExpressionType.And or ExpressionType.AndAlso => "AND",
                ExpressionType.Or or ExpressionType.OrElse => "OR",
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.Add or ExpressionType.AddChecked => "+",
                ExpressionType.Subtract or ExpressionType.SubtractChecked => "-",
                ExpressionType.Multiply or ExpressionType.MultiplyChecked => "*",
                ExpressionType.Divide => "/",
                _ => throw new NotSupportedException($"Operation '{b.NodeType}' is not supported.")
            };
        }

        // internal for test
        internal virtual string GetOperator(UnaryExpression u)
        {
            return u.NodeType switch
            {
                ExpressionType.Negate or ExpressionType.NegateChecked => "-",
                ExpressionType.UnaryPlus => "+",
                ExpressionType.Not => "NOT",
                ExpressionType.Convert => "",
                _ => throw new NotSupportedException($"Operation '{u.NodeType}' is not supported."),
            };
        }

        // internal for test
        internal virtual string? GetOperator(string methodName)
        {
            return methodName switch
            {
                "Add" => "+",
                "Subtract" => "-",
                "Multiply" => "*",
                "Divide" => "/",
                "Negate" => "-",
                "Remainder" => "%",
                "Equal" => "=",
                _ => throw new NotSupportedException($"Operation '{methodName}' is not supported."),
            };
        }


        private static bool IsBoolean(Type type)
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
