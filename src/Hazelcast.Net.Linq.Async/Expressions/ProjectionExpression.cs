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
using System.Linq.Expressions;

namespace Hazelcast.Linq.Expressions
{
    /// <summary>
    /// Expression helps to reconstruct the result object
    /// </summary>
    internal class ProjectionExpression : Expression
    {
        /// <summary>
        /// SQL Select statement of the expression.
        /// </summary>
        public SelectExpression Source { get; }

        /// <summary>
        /// Expression that holds the element/data type and its bindings. It's useful while constructing the result objects
        /// </summary>
        public Expression Projector { get; }
        /// <inheritdoc/>
        public override ExpressionType NodeType { get; }
        /// <inheritdoc/>
        public override Type Type { get; }

        public ProjectionExpression(SelectExpression source, Expression projector, Type type)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Projector = projector ?? throw new ArgumentNullException(nameof(projector));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            NodeType = (ExpressionType)HzExpressionType.Projection;
        }
    }
}
