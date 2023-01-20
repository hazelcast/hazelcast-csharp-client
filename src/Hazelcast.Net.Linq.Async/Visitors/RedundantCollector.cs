// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Linq.Expressions;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Determines and collects redundant subqueries.
    /// </summary>
    internal class RedundantCollector : HzExpressionVisitor
    {
        private List<SelectExpression> _redundants = new();

        /// <summary>
        /// Collects redundant subQueries.
        /// </summary>
        /// <param name="expression">Expression to be looked</param>
        /// <param name="redundants">Redundant subQueries</param>
        /// <returns></returns>
        public static bool TryCollect(Expression expression, out SelectExpression[] redundants)
        {
            var list = new RedundantCollector().CollectInternal(expression);
            redundants = list.ToArray();
            return list.Count > 0;
        }

        private List<SelectExpression> CollectInternal(Expression expression)
        {
            Visit(expression);
            return _redundants;
        }

        internal override Expression VisitSelect(SelectExpression node)
        {
            if (IsRedundant(node))
                _redundants.Add(node);

            return node;
        }

        private static bool IsRedundant(SelectExpression select)
        {
            return (select.Where is null && HasSimpleProjection(select))
                || IsWrapperOfFrom(select);
        }

        public static bool HasSimpleProjection(SelectExpression select)
        {
            foreach (var item in select.Columns)
            {
                //If column name is changed, so projection too.
                if (item.Expression is not ColumnExpression column || column.Name != item.Name) return false;
            }
            return true;
        }

        private static bool IsWrapperOfFrom(SelectExpression node)
        {
            if (node.From is MapExpression) return false;

            // to be redundant, current node should be pure wrapper of its from node.
            if (node.From is not SelectExpression from || node.Columns.Count != from.Columns.Count) return false;

            for (int i = 0, n = node.Columns.Count; i < n; i++)
            {
                //check order of the columns between select and from.
                if (node.Columns[i].Expression is not ColumnExpression col || col.Name != from.Columns[i].Name) return false;
            }

            return true;
        }
    }
}
