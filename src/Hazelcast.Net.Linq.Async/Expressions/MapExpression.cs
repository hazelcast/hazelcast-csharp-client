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
using System.Linq.Expressions;

namespace Hazelcast.Linq.Expressions
{
    /// <summary>
    /// Custom expression that holds information about the Hazelcast Map as expression.
    /// </summary>
    internal class MapExpression : Expression
    {
        /// <summary>
        /// Name of the map that will be used in SQL query
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Alias of the Map that will be used in SQL query.
        /// </summary>
        public string Alias { get; }

        /// <inheritdoc />
        public override ExpressionType NodeType { get; }

        /// <inheritdoc />
        public override Type Type { get; }

        public MapExpression(Type type, string name, string alias)
        {
            Name = name ?? throw new ArgumentNullException(nameof(type));
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            NodeType = (ExpressionType)HzExpressionType.Map;//This is a custom expression and type
        }
    }
}
