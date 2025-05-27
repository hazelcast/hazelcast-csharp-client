﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Linq.Visitors;

namespace Hazelcast.Linq.Evaluation
{
    /// <summary>
    /// Travers the tree bottom up to find nodes that can be evaluated partially.
    /// </summary>
    internal class ExpressionNominator : HzExpressionVisitor
    {
        private bool _cannotBeEvaluated;
        private readonly Func<Expression, bool> _canBeEvaluatedFunc;
        private readonly HashSet<Expression> _canditateNodes;

        public ExpressionNominator(Func<Expression, bool> canBeEvaluated)
        {
            _canBeEvaluatedFunc = canBeEvaluated;
            _canditateNodes = new();
        }

        /// <summary>
        /// Travers and find nodes that be evaluated on the tree.
        /// </summary>
        /// <param name="expression">Root of the tree</param>
        /// <returns>Subtrees that can be evaluated partially.</returns>
        public HashSet<Expression> Nominate(Expression expression)
        {
            _canditateNodes.Clear();
            Visit(expression);
            return _canditateNodes;
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override Expression Visit(Expression node)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (node == null) return node;
#pragma warning restore CS8603 // Possible null reference return.

            var copyOfEvaluationState = _cannotBeEvaluated;

            _cannotBeEvaluated = false;// Initially, accept everything can be evaluated.

            base.Visit(node);// Travers to bottom.

            //We are at the bottom,
            //if we can evaluate the child, parent can also be checked for evaluation.
            //If we can't, no need to check the parent.
            if (!_cannotBeEvaluated)
            {
                if (_canBeEvaluatedFunc(node))
                    _canditateNodes.Add(node);
                else
                    _cannotBeEvaluated = true;//The node cannot be evaluated, so also the parent.
            }

            _cannotBeEvaluated |= copyOfEvaluationState;

            return node;
        }

    }
}
