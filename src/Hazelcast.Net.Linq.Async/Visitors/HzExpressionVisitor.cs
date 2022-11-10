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
using System.Text;
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

            return (HzExpressionType)node.NodeType switch
            {
                HzExpressionType.Map => VisitMap((MapExpression)node),
                HzExpressionType.Column => VisitColumn((ColumnExpression)node),
                HzExpressionType.Select => VisitSelect((SelectExpression)node),
                HzExpressionType.Projection => VisitProjection((ProjectionExpression)node),
                HzExpressionType.Join => VisitJoin((JoinExpression)node),
                _ => base.Visit(node),
            };
            }

        protected virtual Expression VisitJoin(JoinExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            var condtition = Visit(node.JoinCondition);

            if (left != node.Left || right != node.Right || condtition != node.JoinCondition)
                return new JoinExpression(left, right, condtition, node.Type);

            return node;
        }

        protected virtual Expression VisitProjection(ProjectionExpression node)
        {
            var visitedSource = (SelectExpression)Visit(node.Source);
            var visitedProjector = Visit(node.Projector);

            if (node.Source != visitedSource || node.Projector != visitedProjector)
                return new ProjectionExpression(visitedSource, visitedProjector, visitedProjector.Type);

            return node;
        }

        protected virtual Expression VisitSelect(SelectExpression node)
        {
            var from = Visit(node.From);
            var where = Visit(node.Where);
            var columns = VisitColumnDefinititions(node.Columns);

            if (from != node.From || where != node.Where || columns != node.Columns)
                return new SelectExpression(node.Alias, node.Type, columns, from, where);

            return node;
        }

        protected virtual ReadOnlyCollection<ColumnDefinition> VisitColumnDefinititions(ReadOnlyCollection<ColumnDefinition> columns)
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

        protected virtual Expression VisitColumn(ColumnExpression node)
        {
            return node;
        }

        protected virtual Expression VisitMap(MapExpression node)
        {
            return node;
        }
    }
}
