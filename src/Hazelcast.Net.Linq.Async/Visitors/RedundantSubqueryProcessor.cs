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

using System.Linq.Expressions;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Detects and removes redundant sub-queries. A basic select query can be layered with 
    /// all columns and demanded projection columns. In similar cases, we try to get rid of 
    /// unnecessary layers, so that will also effect the translated sql statements.
    /// <code>SELECT c1.Name FROM
    ///     (SELECT c0.Name, c0.LastName, .., c0.Phone FROM persons c0) c1</code>
    /// The inner query is redundant here. So we can remove it if it's 
    /// a really simple projection layer.
    /// </summary>
    internal class RedundantSubqueryProcessor : HzExpressionVisitor
    {
        /// <summary>
        /// Reduces the redundant subqueries, and combined their conditions.
        /// </summary>
        /// <param name="select">Expression to be cleaned.</param>
        /// <returns>Cleaned expression</returns>
        internal Expression Clean(SelectExpression select)
        {
            return Visit(select);
        }

        protected override Expression VisitSelect(SelectExpression node)
        {
            var visitedSelect = base.VisitSelect(node) as SelectExpression;

            // Try to find and clean redundant subqueries.
            if (RedundantCollector.TryCollect(visitedSelect, out var redundants))
                visitedSelect = (SelectExpression)SubqueryRemover.Remove(visitedSelect, redundants);

            //There could be a queries on the same level but different nodes,
            // they can be merged it they are also a simple select expression.
            var from = visitedSelect.From as SelectExpression;

            //No chance, give up.
            if (from == null) return visitedSelect;

            if (RedundantCollector.HasSimpleProjection(from))
            {
                // From part of the projection is a simple projection, remove it.
                visitedSelect = (SelectExpression)SubqueryRemover.Remove(visitedSelect, from);

                // Try to combine conditions since projections cleaned.            
                var where = visitedSelect.Where;

                if (where != null)
                    where = Expression.And(from.Where, where);
                else
                    where = from.Where;

                if (where != visitedSelect.Where)
                    return new SelectExpression(visitedSelect.Alias, visitedSelect.Type, visitedSelect.Columns, visitedSelect.From, where);
            }

            return visitedSelect;
        }
    }
}
