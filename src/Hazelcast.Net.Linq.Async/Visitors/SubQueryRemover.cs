// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Removes(stripes) the provided portions of the given expression.
    /// </summary>
    internal class SubQueryRemover : HzExpressionVisitor
    {
        private HashSet<SelectExpression> _subQueriesToRemove;
        /// <summary>
        /// Map alias to columns and columns to its expressions
        /// </summary>
        private Dictionary<string, Dictionary<string, Expression>> _subQueries;

#pragma warning disable 8618
        private SubQueryRemover() { }
#pragma warning restore 8618

        public static Expression Remove(SelectExpression root, params SelectExpression[] toBeRemoved)
        {
            return new SubQueryRemover().RemoveInternal(root, toBeRemoved);
        }

        private Expression RemoveInternal(SelectExpression root, params SelectExpression[] toBeRemoved)
        {
            _subQueriesToRemove = new HashSet<SelectExpression>(toBeRemoved);
            _subQueries = _subQueriesToRemove.ToDictionary(p => p.Alias,
                p => p.Columns.ToDictionary(c => c.Name, c => c.Expression));
            return Visit(root);
        }

        // internal for testing
        internal override Expression VisitSelect(SelectExpression node)
        {
            // Cut it from `From` expression as requested.
            return _subQueriesToRemove.Contains(node) ? Visit(node.From) : base.VisitSelect(node);
        }
        
        // internal for testing
        internal override Expression VisitColumn(ColumnExpression node)
        {
            //Check whether `node` is in the provided remove list.
            if (_subQueries.TryGetValue(node.Alias, out var map))
                if (map.TryGetValue(node.Name, out var expression))
                    return Visit(expression);
                else
                    throw new Exception("Reference to undefined column");

            return node;
        }

    }
}
