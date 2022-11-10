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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Hazelcast.Linq.Evaluation;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Traverse and creates column definitations for projection.
    /// </summary>
    internal class ColumnProjector : HzExpressionVisitor
    {
        public ExpressionNominator Nominator { get; }

        private Dictionary<ColumnExpression, ColumnExpression> _mapOfColumns;
        private List<ColumnDefinition> _columns;
        private HashSet<string> _columnNames;
        private HashSet<Expression> _candidates;
        /// <summary>
        /// Alias of the current level
        /// </summary>
        private IReadOnlyList<string> _existingAlias;
        /// <summary>
        /// Alias of the outer level of the query
        /// </summary>
        private string _newAlias;
        private int _columnIndex;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ColumnProjector(Func<Expression, bool> canBeColumn)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (canBeColumn == null) throw new ArgumentNullException(nameof(canBeColumn));

            Nominator = new ExpressionNominator(canBeColumn);
            _existingAlias = Enumerable.Empty<string>().ToList();
            _newAlias = String.Empty;
        }

        /// <summary>
        /// Arranges columns declarations for the select expression
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="newAlias">Alias of the outer level of the query</param>
        /// <param name="existingAlias">Alias of the current level</param>
        /// <returns></returns>
        public ProjectedColumns Project(Expression exp, string newAlias, params string[] existingAlias)
        {
            _mapOfColumns = new();
            _columns = new();
            _columnNames = new();
            _newAlias = newAlias;
            _existingAlias = existingAlias;
            _candidates = Nominator.Nominate(exp);

            return new ProjectedColumns(Visit(exp), _columns.AsReadOnly());
        }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override Expression Visit(Expression node)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            //Skip if not nominated to be evaluated
            if (!_candidates.Contains(node))
                return base.Visit(node);

            if (node.NodeType == (ExpressionType)HzExpressionType.Column)
            {
                var column = (ColumnExpression)node;

                //The column is already defined on the tree
                if (_mapOfColumns.TryGetValue(column, out ColumnExpression mappedColumn))
                    return mappedColumn;

                //Maps are overlaped though column didn't match above.
                if (_existingAlias.Contains(column.Alias))
                {
                    var name = CreateUniqueColumnName(column.Name);
                    _columns.Add(new ColumnDefinition(name, column));
                    mappedColumn = new ColumnExpression(column.Type, _newAlias, name, _columns.Count);
                    _columnNames.Add(column.Name);
                    _mapOfColumns[column] = mappedColumn;
                    return mappedColumn;
                }

                return column;
            }
            else
            {
                //new column definition
                var name = GetNextColumnName();
                _columns.Add(new ColumnDefinition(name, node));
                return new ColumnExpression(node.Type, _newAlias, name, _columns.Count);
            }
        }

        private string GetNextColumnName()
        {
            return CreateUniqueColumnName("c" + (_columnIndex++));
        }

        private string CreateUniqueColumnName(string columnName)
        {
            var baseName = columnName;
            var suffix = 1;

            while (_columnNames.Contains(columnName))
                columnName = baseName + "_" + (suffix++);

            return columnName;
        }
    }
}
