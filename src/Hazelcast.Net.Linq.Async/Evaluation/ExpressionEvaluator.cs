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

using System;
using System.Linq.Expressions;

namespace Hazelcast.Linq.Evaluation
{
    /// <summary>
    /// Travers and evaluates the expression tree.
    /// </summary>
    internal static class ExpressionEvaluator
    {
        /// <summary>
        /// Travers and evaluates the expression tree.
        /// </summary>
        /// <param name="expression">Root node</param>
        /// <param name="canBeEvaluatedFunc">A function decides that the node can be evaluated or not.</param>
        /// <returns>A new Tree which is evaluated partially.</returns>
        public static Expression EvaluatePartially(Expression expression, Func<Expression, bool> canBeEvaluatedFunc)
        {
            return new SubtreeEvaluator(new ExpressionNominator(canBeEvaluatedFunc).Nominate(expression))
                .Visit(expression);
        }

        /// <summary>
        /// Travers and evaluates <see cref="ExpressionType.Parameter"/> nodes on the expression tree.
        /// </summary>
        /// <param name="expression">Root node</param>
        /// <returns>A new Tree which is evaluated partially.</returns>
        public static Expression EvaluatePartially(Expression expression)
        {
            return new SubtreeEvaluator(new ExpressionNominator((node) => node.NodeType != ExpressionType.Parameter).Nominate(expression))
                .Visit(expression);
        }
    }
}
