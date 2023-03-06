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
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Hazelcast.Linq.Expressions
{
    /// <summary>
    /// Represents fields in the expression tree that are converted to a column for projection.
    /// </summary>
    internal class ProjectedColumns
    {
        /// <summary>
        /// Sqlized expression tree for projection.
        /// </summary>
        public Expression Projector { get;}

        /// <summary>
        /// List of all columns in the tree <see cref="Projector"/>.
        /// </summary>
        public ReadOnlyCollection<ColumnDefinition> Columns { get;}

        public ProjectedColumns(Expression projector, ReadOnlyCollection<ColumnDefinition> columns)
        {
            Projector = projector ?? throw new ArgumentNullException(nameof(projector));
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }
    }
}
