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

using Hazelcast.Util;

namespace Hazelcast.Core
{
    /// <summary>
    /// HazelcastJsonValue is a wrapper for Json formatted strings. It is
    /// preferred to store HazelcastJsonValue instead of Strings for Json
    /// formatted strings. Users can run predicates/aggregations and use
    /// indexes on the attributes of the underlying Json strings.
    /// </summary>
    /// <remarks>
    /// HazelcastJsonValue is queried using Hazelcast's querying language.
    /// See <see cref="Predicates"/>.
    /// <br/>
    /// In terms of querying, numbers in Json strings are treated as either
    /// <c>long</c> or <c>double</c>. strings, booleans and null are
    /// treated as their .net counterparts.
    /// <br/>
    /// HazelcastJsonValue keeps given string as it is. Strings are not
    /// checked for being valid. Ill-formatted json strings may cause false
    /// positive or false negative results in queries. <c>null</c> string
    /// is not allowed.
    /// </remarks>
    public sealed class HazelcastJsonValue
    {
        private readonly string _jsonString;

        /// <summary>
        /// Creates a HazelcastJsonValue from given string.
        /// </summary>
        /// <param name="jsonString">a non null Json string</param>
        /// <exception cref="System.NullReferenceException">if jsonString param is null</exception>
        public HazelcastJsonValue(string jsonString)
        {
            ValidationUtil.CheckNotNull(jsonString, ValidationUtil.NullJsonStringIsNotAllowed);
            _jsonString = jsonString;
        }

        /// <summary>
        /// Returns unaltered string that was used to create this object.
        /// </summary>
        /// <returns>original string</returns>
        public override string ToString()
        {
            return _jsonString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (ReferenceEquals(obj, null) || GetType() != obj.GetType())
            {
                return false;
            }
            var jsonValue = (HazelcastJsonValue)obj;
            return _jsonString != null ? _jsonString.Equals(jsonValue._jsonString) : jsonValue._jsonString == null;
        }

        public override int GetHashCode()
        {
            return _jsonString != null ? _jsonString.GetHashCode() : 0;
        }
    }
}