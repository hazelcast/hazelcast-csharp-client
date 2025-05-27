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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hazelcast.Linq.Evaluation
{
    /// <summary>
    /// Travers the tree and evaluates marked nodes to make them constant.
    /// </summary>
    internal class SubtreeEvaluator : ExpressionVisitor
    {
        private readonly HashSet<Expression> _markedNodes;

        public SubtreeEvaluator(HashSet<Expression> markedNodes)
        {
            _markedNodes = markedNodes;
        }
#pragma warning disable 8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override Expression Visit(Expression node)
#pragma warning restore 8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return node;
#pragma warning restore CS8603 // Possible null reference return.

            if (_markedNodes.Contains(node))
            {
                return CompileExpression(node);
            }

            return base.Visit(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return node;
        }

        /// <summary>
        /// Compiles and invokes the node that can be evaluated.
        /// </summary>
        /// <param name="node">Node to be evaluated</param>
        /// <returns>Constant Node</returns>
        private static Expression CompileExpression(Expression node)
        {
            if (node.NodeType == ExpressionType.Constant) return node;

            var lambda = Expression.Lambda(node).Compile();

            return Expression.Constant(lambda.DynamicInvoke(), node.Type);
        }
    }
}
