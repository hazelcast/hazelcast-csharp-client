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
using System.Reflection;
using System.Text;
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    internal class QueryBinder : ExpressionVisitor
    {
        private ColumnProjector _projector;
        private Dictionary<ParameterExpression, Expression> _map;
        private int _aliasCount;

        public QueryBinder()
        {
            _projector = new ColumnProjector(p => p.NodeType == (ExpressionType)HzExpressionType.Column);
        }

        public Expression Bind(Expression expression)
        {
            _map = new Dictionary<ParameterExpression, Expression>();
            return Visit(expression);
        }

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
                expression = ((UnaryExpression)expression).Operand;

            return expression;
        }

        /// <summary>
        /// Generates an alias for the field.
        /// </summary>
        /// <returns>alias</returns>
        private string GetNextAlias()
        {
            return "t" + (_aliasCount++);
        }

        public ProjectedColumns Project(Expression expression, string newAlias, string existingAlias)
        {
            return _projector.Project(expression, newAlias, existingAlias);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) || m.Method.DeclaringType == typeof(Enumerable))
            {
                switch (m.Method.Name)
                {
                    case "Where":
                        //type of entry, source, predicate
                        return this.BindWhere(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                }

                throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
            }

            return base.VisitMethodCall(m);
        }

        private Expression BindWhere(Type type, Expression source, LambdaExpression predicate)
        {
            var projection = (ProjectionExpression)Visit(source); //DFS            
            _map[predicate.Parameters[0]] = projection.Projector;//map field (ex. AlbumId:int) to projector
            var where = Visit(predicate.Body); // Visit the body to handle inner expressions.
            var alias = GetNextAlias();

            //Visit, nominate and replace with SQL equvalients nodes on the 'projection' expression.
            //Note: SQL equivalients are the custom ones defined by us under Hazelcast.Linq.Expressions.
            var pc = _projector.Project(projection.Projector, alias, GetExistingAlias(projection.Source));

            return new ProjectionExpression(new SelectExpression(alias, pc.Columns, projection.Source, where, type), pc.Projector, type);
        }

        private static string GetExistingAlias(Expression source)
        {
            switch ((HzExpressionType)source.NodeType)
            {
                case HzExpressionType.Select:
                    return ((SelectExpression)source).Alias;
                case HzExpressionType.Map:
                    return ((MapExpression)source).Alias;
                default:
                    throw new InvalidOperationException(string.Format("Invalid source node type '{0}'", source.NodeType));
            }
        }

        private bool IsMap(object value)
        {
            var q = value as IQueryable;
            return q != null && q.Expression.NodeType == ExpressionType.Constant;
        }

        private string GetMapName(object map)
        {
            var mapQuery = (IQueryable)map;
            var rowType = mapQuery.ElementType;
            return rowType.Name;
        }

        private string GetColumnName(MemberInfo member)
        {
            return member.Name;
        }

        private Type GetColumnType(MemberInfo member)
        {
            var finfo = member as FieldInfo;

            if (finfo != null)
                return finfo.FieldType;

            var pinfo = (PropertyInfo)member;
            return pinfo.PropertyType;
        }

        /// <summary>
        /// Gets members of the entry type.
        /// </summary>
        /// <param name="entryType">The type of the object that will be queried from the map.</param>
        /// <returns>List of fields</returns>
        private IEnumerable<MemberInfo> GetMappedMembers(Type entryType)
        {
            return entryType.GetFields().Cast<MemberInfo>();
        }

        /// <summary>
        /// Creates an projection that holds all fields with binding of the entry type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Projection of the entry type</returns>
        private ProjectionExpression GetMapProjection(object obj)
        {
            var map = (IQueryable)obj;//map means HMap as a data source

            var mapAlias = GetNextAlias();
            var selectAlias = GetNextAlias();

            var bindings = new List<MemberBinding>();
            var columns = new List<ColumnDefinition>();

            foreach (var mi in GetMappedMembers(map.ElementType))
            {
                var columnName = GetColumnName(mi);
                var columnType = GetColumnType(mi);

                bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName, columns.Count)));
                columns.Add(new ColumnDefinition(columnName, new ColumnExpression(columnType, mapAlias, columnName, columns.Count)));
            }

            var projector = Expression.MemberInit(Expression.New(map.ElementType), bindings);
            var entryType = typeof(IEnumerable<>).MakeGenericType(map.ElementType);

            var selectExp = new SelectExpression(selectAlias, columns.AsReadOnly(), new MapExpression(entryType, mapAlias, GetMapName(map)), null, entryType);
            return new ProjectionExpression(selectExp, projector, entryType);
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            return IsMap(node.Value) ? (Expression)GetMapProjection(node.Value) : (Expression)node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _map.TryGetValue(node, out var exp) ? exp : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var visitedNode = Visit(node.Expression);

            switch (visitedNode.NodeType)
            {
                //Get only Member that matches within the expression. 
                case ExpressionType.MemberInit:
                    var initExp = (MemberInitExpression)visitedNode;

                    foreach (MemberAssignment assigment in initExp.Bindings)
                    {
                        if (assigment != null && MembersMatch(assigment.Member, node.Member))
                            return assigment.Expression;
                    }

                    break;

                case ExpressionType.New:

                    var newExp = (NewExpression)visitedNode;

                    if (newExp.Members == null) break;

                    for (int i = 0; i < newExp.Members.Count; i++)
                    {
                        if (MembersMatch(newExp.Members[i], node.Member))
                            return newExp.Arguments[i];
                    }

                    break;

            }

            return visitedNode;
        }


        private bool MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
                return true;

            if (a is MethodInfo && b is PropertyInfo infoB)
                return a == infoB.GetGetMethod();

            else if (a is PropertyInfo infoA && b is MethodInfo)
                return infoA.GetGetMethod() == b;

            return false;
        }
    }
}
