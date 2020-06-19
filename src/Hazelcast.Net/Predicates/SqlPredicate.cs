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
using System.Text;
using Hazelcast.Serialization;

namespace Hazelcast.Predicates
{
    /// <summary>
    ///     SQL Predicate
    /// </summary>
    public class SqlPredicate : IPredicate
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
            if (output == null) throw new ArgumentNullException(nameof(output));
            output.WriteUtf(_sql);
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            _sql = input.ReadUtf();
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.SqlPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is SqlPredicate other && Equals(this, other);
        }

        private static bool Equals(SqlPredicate obj1, SqlPredicate obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (obj1 is null) return false;

            return obj1._sql == obj2._sql;
        }

        public override int GetHashCode()
        {

            // ReSharper disable NonReadonlyMemberInGetHashCode
            return _sql != null ? _sql.GetHashCode(StringComparison.Ordinal) : 0;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(_sql);
            return builder.ToString();
        }
    }
}
