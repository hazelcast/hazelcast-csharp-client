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
            _values = new();
        }

        /// <summary>
        /// Translate the tree into SQL statement
        /// </summary>
        /// <param name="expression">tree</param>
        /// <returns>(SQL statement, Value of variables)</returns>
        public static (string, IEnumerable<object>) Format(Expression expression)
        {
            var f = new QueryFormatter();
            f.Visit(expression);
            return (f.ToString(), f._values);
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


        protected override Expression VisitColumn(ColumnExpression column)
        {
            if (!string.IsNullOrEmpty(column.Alias))
            {
                Write(column.Alias);
                Write(".");
            }

            //Write("\"");
            Write(column.Name);
            //Write("\"");
            return column;
        }

        protected override Expression VisitProjection(ProjectionExpression proj)
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
                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{val}' is not supported.");
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        WriteParameter(val);
                        break;
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

        private void Write(object v)
        {
            _sb.Append(v);
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            string op = this.GetOperator(u);
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    if (IsBoolean(u.Operand.Type) || op.Length > 1)
                    {
                        Write(op);
                        Write(" ");
                        VisitPredicate(u.Operand);
                    }
                    else
                    {
                        Write(op);
                        Visit(u.Operand);
                    }

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
                    // ignore conversions for now
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            var op = GetOperator(b);
            var left = b.Left;
            var right = b.Right;

            Write("(");
            switch (b.NodeType)
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
                        Visit(left);
                        Write(" ");
                        Write(op);
                        Write(" ");
                        Visit(right);
                    }

                    break;
                case ExpressionType.Equal:
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

                    goto case ExpressionType.LessThan;
                case ExpressionType.NotEqual:
                    if (right.NodeType == ExpressionType.Constant)
                    {
                        ConstantExpression ce = (ConstantExpression) right;
                        if (ce.Value == null)
                        {
                            this.Visit(left);
                            this.Write(" IS NOT NULL");
                            break;
                        }
                    }
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

                    goto case ExpressionType.LessThan;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    // check for special x.CompareTo(y) && type.Compare(x,y)
                    if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
                    {
                        var mc = (MethodCallExpression) left;
                        var ce = (ConstantExpression) right;
                        if (ce.Value is not null && ce.Value.GetType() == typeof(int) && ((int) ce.Value) == 0)
                        {
                            if (mc.Method.Name == "CompareTo" && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                            {
                                left = mc.Object;
                                right = mc.Arguments[0];
                            }
                            else if (
                                (mc.Method.DeclaringType == typeof(string) ||
                                 mc.Method.DeclaringType == typeof(decimal))
                                && mc.Method.Name == "Compare" && mc.Method.IsStatic && mc.Arguments.Count == 2)
                            {
                                left = mc.Arguments[0];
                                right = mc.Arguments[1];
                            }
                        }
                    }

                    goto case ExpressionType.Add;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                    Visit(left);
                    Write(" ");
                    Write(op);
                    Write(" ");
                    Visit(right);
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            this.Write(")");
            return b;
        }

        protected override Expression VisitSelect(SelectExpression select)
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
                case HzExpressionType.Select:
                    Write("(");
                    Visit(from);
                    Write(") ");
                    Write(((SelectExpression) from).Alias);
                    break;
                case HzExpressionType.Join:
                    VisitJoin((JoinExpression) from);
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
