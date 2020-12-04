// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;

namespace Hazelcast.Query
{
    /// <summary>
    ///     SQL Predicate
    /// </summary>
    internal class SqlPredicate : IPredicate, IIdentifiedDataSerializable
    {
        private string _sql;

        public SqlPredicate()
        { }

        public SqlPredicate(string sql)
        {
            _sql = sql;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_sql);
        }

        public void ReadData(IObjectDataInput input)
        {
            _sql = input.ReadUTF();
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => PredicateDataSerializerHook.SqlPredicate;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is SqlPredicate other && Equals(this, other);
        }

        private static bool Equals(SqlPredicate left, SqlPredicate right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._sql == right._sql;
        }

        public override int GetHashCode()
        {

            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _sql != null ? _sql.GetHashCode(StringComparison.Ordinal) : 0;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override string ToString()
        {
            return "SQL('" + _sql + "')";
        }
    }
}
