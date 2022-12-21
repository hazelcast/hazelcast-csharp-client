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
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    internal class UnusedColumnProcessor : HzExpressionVisitor
    {
        private readonly Dictionary<string, HashSet<string>> _columnsInUseByAlias = new();

        public static Expression? Clean(Expression expression)
        {
            return new UnusedColumnProcessor().CleanInternal(expression) as ProjectionExpression;
        }

        private Expression CleanInternal(Expression expression)
        {
            return Visit(expression);
        }

        // internal for testing
        internal override Expression VisitColumn(ColumnExpression node)
        {
            // Collect column names by alias.
            if (_columnsInUseByAlias.TryGetValue(node.Alias, out var columns))
                columns.Add(node.Name);
            else
                _columnsInUseByAlias[node.Alias] = new HashSet<string> { node.Name };

            return node;
        }

        // internal for testing
        internal override Expression VisitSelect(SelectExpression node)
        {
            if (_columnsInUseByAlias.TryGetValue(node.Alias, out var usedColumns))
            {
                List<ColumnDefinition> alternate = null!;

                for (int i = 0; i < node.Columns.Count; i++)
                {
                    var currentColumn = node.Columns[i];

                    if (IsUsed(node.Alias, currentColumn.Name))
                    {
                        var visitedExp = Visit(currentColumn.Expression);
                        if (visitedExp != currentColumn.Expression)
                            currentColumn = new ColumnDefinition(currentColumn.Name, visitedExp);
                    }
                    else
                        currentColumn = null; // the column is not in use.

                    if (currentColumn != node.Columns[i] && alternate is null)
                    {
                        alternate = node.Columns.Take(i).ToList();
                    }

                    if (currentColumn is not null && alternate is not null)
                        alternate.Add(currentColumn);
                }

                var visitedWhere = Visit(node.Where);
                var visitedFrom = Visit(node.From);
                var columns = alternate?.AsReadOnly() ?? node.Columns;

                usedColumns.Clear();

                if (visitedWhere != node.Where || visitedFrom != node.From || columns != node.Columns)
                {
                    node = new SelectExpression(node.Alias, node.Type, columns, visitedFrom, visitedWhere);
                }
            }

            return node;
        }

        // internal for testing
        internal override Expression VisitProjection(ProjectionExpression node)
        {
            var visitedProjector = Visit(node.Projector);// first collect the columns at projector
            var visitedSource = (SelectExpression)Visit(node.Source);

            if (visitedProjector != node.Projector || visitedSource != node.Source)
                return new ProjectionExpression(visitedSource, visitedProjector, node.Type);

            return node;
        }

        // internal for testing
        private bool IsUsed(string alias, string name)
        {
            if (_columnsInUseByAlias.TryGetValue(alias, out var columns))
                return columns is not null && columns.Contains(name);

            return false;
        }
    }
}
