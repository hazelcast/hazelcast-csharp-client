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
using System.Linq.Expressions;

namespace Hazelcast.Linq.Evaluation
{
    /// <summary>
    /// Travers the tree bottom up to find nodes that can be evaluated partially.
    /// </summary>
    internal class ExpressionNominator : ExpressionVisitor
    {
        private bool _cannotBeEvaluated;
        private Func<Expression, bool> _canBeEvaluatedFunc;
        private HashSet<Expression> _canditateNodes;

        public ExpressionNominator(Func<Expression, bool> canBeEvaluated)
        {
            _canBeEvaluatedFunc = canBeEvaluated;

        }

        /// <summary>
        /// Travers and find nodes that be evaluated on the tree.
        /// </summary>
        /// <param name="expression">Root of the tree</param>
        /// <returns>Subtrees that can be evaluated partially.</returns>
        public HashSet<Expression> Nominate(Expression expression)
        {
            _canditateNodes = new HashSet<Expression>();
            Visit(expression);
            return _canditateNodes;
        }


        public override Expression Visit(Expression node)
        {
            if (node == null) return node;

            var copyOfEvaluationState = _cannotBeEvaluated;

            _cannotBeEvaluated = false;// Initially, accept everthing can be evaluated.

            base.Visit(node);// Travers to bottom.

            //We are at the bottom,
            //if we can evalute the child, parent can also be checked for evaluation.
            //If we can't, no need to check the parent.
            if (!_cannotBeEvaluated)
            {
                if(_canBeEvaluatedFunc(node))
                    _canditateNodes.Add(node);
                else
                    _cannotBeEvaluated = true;//The node cannot be evaluated, so also the parent.
            }

            _cannotBeEvaluated |= copyOfEvaluationState;

            return node;
        }

    }
}