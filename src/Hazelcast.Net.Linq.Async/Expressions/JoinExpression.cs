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
using System.Linq.Expressions;

namespace Hazelcast.Linq.Expressions
{
    internal class JoinExpression : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public Expression JoinCondition { get; }

        /// <inheritdoc />
        public override ExpressionType NodeType => (ExpressionType)HzExpressionType.Join;

        /// <inheritdoc />
        public override Type Type { get; }

        public JoinExpression(Expression left, Expression right, Expression joinCondition, Type type)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            JoinCondition = joinCondition ?? throw new ArgumentNullException(nameof(joinCondition));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}
