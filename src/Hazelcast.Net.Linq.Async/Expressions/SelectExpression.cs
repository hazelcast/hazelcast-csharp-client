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
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Hazelcast.Linq.Expressions
{
    /// <summary>
    /// Expression equalivent of a SQL Select statement.
    /// </summary>
    internal class SelectExpression : Expression
    {
        public string Alias { get; }
        public ReadOnlyCollection<ColumnDefinition> Columns { get; }
        public Expression From { get; }
        public Expression? Where { get; }

        /// <inheritdoc/>
        public override ExpressionType NodeType { get; }
        /// <inheritdoc/>
        public override Type Type { get; }

        public SelectExpression(string alias, Type type, ReadOnlyCollection<ColumnDefinition> columns, Expression from, Expression? where = null)
        {
            Alias = alias;
            Columns = columns;
            From = from;
            Where = where;
            Type = type;
            NodeType = (ExpressionType)HzExpressionType.Select;
        }
    }
}