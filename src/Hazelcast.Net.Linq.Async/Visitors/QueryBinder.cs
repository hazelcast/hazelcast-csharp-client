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
using Hazelcast.Linq.Expressions;

namespace Hazelcast.Linq.Visitors
{
    /// <summary>
    /// Traverses and prepares a SQLized expression tree to be traversed and converted to text based SQL statements.
    /// </summary>
    internal class QueryBinder : HzExpressionVisitor
    {
        private ColumnProjector _projector;
        private Dictionary<ParameterExpression, Expression> _map;
        private int _aliasCount;
        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public QueryBinder()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _projector = new ColumnProjector(p => p.NodeType == (ExpressionType)HzExpressionType.Column);
        }

        /// <summary>
        /// Get an SQLized tree and its projection bindings
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Expression Bind(Expression expression)
        {
            _map = new();
            return Visit(expression);
        }

        ///(Internal for tests)
        internal static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
                expression = ((UnaryExpression)expression).Operand;

            return expression;
        }

        /// <summary>
        /// Generates an alias for the field. (Internal for tests)
        /// </summary>
        /// <returns>alias</returns>
        internal string GetNextAlias()
        {
            return "m" + (_aliasCount++);//m for map
        }

        private static LambdaExpression GetLambda(Expression e)
        {
            e = StripQuotes(e);

            if (e.NodeType == ExpressionType.Constant)
#pragma warning disable CS8603 // Possible null reference return.
                return ((ConstantExpression)e).Value as LambdaExpression;
#pragma warning restore CS8603 // Possible null reference return.

#pragma warning disable CS8603 // Possible null reference return.
            return e as LambdaExpression;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public ProjectedColumns Project(Expression expression, string newAlias, params string[] existingAlias)
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
                        //the arguments respectivly->type of entry, source e, predicate
                        return BindWhere(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]));
                    case "Select":
                        return BindSelect(m.Type, m.Arguments[0], GetLambda(m.Arguments[1]));
                    case "Join":
                        return BindJoin(m.Type, m.Arguments[0], m.Arguments[1], GetLambda(m.Arguments[2]), GetLambda(m.Arguments[3]), GetLambda(m.Arguments[4]));
                }

                throw new NotSupportedException($"The method '{m.Method.Name}' is not supported.");
            }

            return base.VisitMethodCall(m);
        }

        private ProjectionExpression BindJoin(Type type, Expression outer, Expression inner, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression selector)
        {
            var outerProjection = (ProjectionExpression)Visit(outer);
            var innerProjection = (ProjectionExpression)Visit(inner);
            var visitedOuterKey = Visit(outerKey.Body);
            var visitedInnerKey = Visit(innerKey.Body);
            var visitedSelector = Visit(selector.Body);

            _map[outerKey.Parameters[0]] = outerProjection.Projector;
            _map[innerKey.Parameters[0]] = innerProjection.Projector;
            _map[selector.Parameters[0]] = outerProjection.Projector;
            _map[selector.Parameters[1]] = innerProjection.Projector;

            var alias = GetNextAlias();

            var joinExp = new JoinExpression(outerProjection.Source, innerProjection.Source, Expression.Equal(visitedOuterKey, visitedInnerKey), type);

            var projectedColumns = Project(visitedSelector, alias, outerProjection.Source.Alias, innerProjection.Source.Alias);

            return new ProjectionExpression(new SelectExpression(alias, type, projectedColumns.Columns, joinExp), projectedColumns.Projector, type);
        }

        /// <summary>
        /// Visit the expressions and bind columns and conditions with SQL equavilent expressions. 
        /// </summary>
        /// <param name="type">Type of the result entry</param>
        /// <param name="source">The source expression</param>
        /// <param name="predicate">Predicate of the condition</param>
        /// <returns>A Projection Expression</returns>
        private ProjectionExpression BindSelect(Type type, Expression source, LambdaExpression predicate)
        {
            var (projection, visitedPredicate) = VisitSourceAndPredicate(source, predicate);
            var alias = GetNextAlias();

            //Visit, nominate and arrange which columns to be selected out of fields of the map.
            //Note: SQL exp. are the custom ones defined by us under Hazelcast.Linq.Expressions.

            //Projected columns of the `Where` clause.
            var projectedColumns = Project(visitedPredicate, alias, GetExistingAlias(projection.Source));

            return new ProjectionExpression(new SelectExpression(alias, type, projectedColumns.Columns, projection.Source), projectedColumns.Projector, type);
        }

        private ProjectionExpression BindWhere(Type type, Expression source, LambdaExpression predicate)
        {
            //projection and expression(s) in the select clause.
            var (projection, visitedPredicate) = VisitSourceAndPredicate(source, predicate);
            var alias = GetNextAlias();

            //Projected columns of the `Select` clause.
            var projectedColumns = Project(projection.Projector, alias, GetExistingAlias(projection.Source));

            return new ProjectionExpression(new SelectExpression(alias, type, projectedColumns.Columns, projection.Source, visitedPredicate), projectedColumns.Projector, type);
        }

        private (ProjectionExpression, Expression) VisitSourceAndPredicate(Expression expression, LambdaExpression predicate)
        {
            var projection = (ProjectionExpression)Visit(expression);//DFS and project everything about the entry type          
            _map[predicate.Parameters[0]] = projection.Projector;//map predicate to the projector
            var predicateExp = Visit(predicate.Body);// Visit the body to handle inner expressions.
            return (projection, predicateExp);
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
                    throw new InvalidOperationException($"Invalid source node type '{source.NodeType}'");
            }
        }

        private bool IsMap(object? value)
        {
            return value is IQueryable q && q.Expression.NodeType == ExpressionType.Constant;
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
            return entryType.GetFields(bindingFlags).Cast<MemberInfo>().Concat(entryType.GetProperties(bindingFlags)).ToArray();
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

            var selectExp = new SelectExpression(selectAlias, entryType, columns.AsReadOnly(), new MapExpression(entryType, mapAlias, GetMapName(map)));
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
                case ExpressionType.MemberInit:
                    var initExp = (MemberInitExpression)visitedNode;

                    foreach (MemberAssignment assigment in initExp.Bindings)
                    {
                        if (assigment != null && MembersMatch(assigment.Member, node.Member))
                            return assigment.Expression;//Most probably a Column Expressions,
                                                        //already created at the Visit above.
                    }

                    break;

                case ExpressionType.New:

                    var newExp = (NewExpression)visitedNode;

                    if (newExp.Members == null) break;

                    for (var i = 0; i < newExp.Members.Count; i++)
                    {
                        if (MembersMatch(newExp.Members[i], node.Member))
                            return newExp.Arguments[i];
                    }

                    break;
            }

            return visitedNode == node.Expression ? node : MakeMemberAccess(visitedNode, node.Member);
        }

        /// <summary>
        /// Creates an expression that represents the member access of the field/property
        /// </summary>
        /// <param name="source">Expression</param>
        /// <param name="memberInfo">Member of the class</param>
        /// <returns>Property/Field expression of given field/property</returns>
        private Expression MakeMemberAccess(Expression source, MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;

            if (fieldInfo != null)
                return Expression.Field(source, fieldInfo);

            var propertyInfo = (PropertyInfo)memberInfo;
            return Expression.Property(source, propertyInfo);
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
