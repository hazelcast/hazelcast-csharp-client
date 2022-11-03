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
using System.Linq;
using System.Linq.Expressions;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Custom visitor for restructured expression tree.
    /// </summary>
    internal class HzExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression? node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return node;
#pragma warning restore CS8603 // Possible null reference return.

            switch ((HzExpressionType)node.NodeType)
            {
                case HzExpressionType.Map:
                    return VisitMap((MapExpression)node);
                case HzExpressionType.Column:
                    return VisitColumn((ColumnExpression)node);
                case HzExpressionType.Select:
                    return VisitSelect((SelectExpression)node);
                case HzExpressionType.Projection:
                    return VisitProjection((ProjectionExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        private Expression VisitProjection(ProjectionExpression node)
        {
            throw new NotImplementedException();
        }

        private Expression VisitSelect(SelectExpression node)
        {
            var from = Visit(node.From);
            var where = Visit(node.Where);
            var columns = VisitColumnDefinititions(node.Columns);

            if (from != node.From || where != node.Where || columns != node.Columns)
                return new SelectExpression(node.Alias, columns, from, where, node.Type);

            return node;
        }

        private ReadOnlyCollection<ColumnDefinition> VisitColumnDefinititions(ReadOnlyCollection<ColumnDefinition> columns)
        {
            List<ColumnDefinition>? definitions = null;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var exp = Visit(column.Expression);

                if (definitions == null && exp != column.Expression)
                    definitions = columns.Take(i).ToList();

                if (definitions != null)
                    definitions.Add(new ColumnDefinition(column.Name, exp));
            }

            return definitions == null ? columns : definitions.AsReadOnly();
        }

        private Expression VisitColumn(ColumnExpression node)
        {
            return node;
        }

        private Expression VisitMap(MapExpression node)
        {
            return node;
        }
    }
}
