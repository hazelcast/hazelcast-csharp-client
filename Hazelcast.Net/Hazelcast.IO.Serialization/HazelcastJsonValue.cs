// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System;

namespace Hazelcast.IO.Serialization
{
    /// <summary>
    ///  Class for serializing/deserializing Hazelcast JSON types
    /// </summary>
    public class HazelcastJsonValue
    {
        private string _jsonString;

        private HazelcastJsonValue(string json)
        {
            _jsonString = json;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((HazelcastJsonValue)obj);
        }

        public override int GetHashCode()
        {
            return (_jsonString != null ? _jsonString.GetHashCode() : 0);
        }

        /// <summary>
        /// This method returns a Json representation of the object
        /// </summary>
        /// <returns>Json representation</returns>
        public override string ToString()
        {
            return _jsonString;
        }

        protected bool Equals(HazelcastJsonValue other)
        {
            return string.Equals(_jsonString, other._jsonString);
        }

        /// <summary>
        /// Create a HazelcastJsonValue from a string.
        /// This method does not the validity of the underlying Json string.
        /// Invalid Json strings may cause wrong results in queries.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static HazelcastJsonValue FromString(string jsonString)
        {
            ValidationUtil.CheckNotNull(jsonString, ValidationUtil.NULL_VALUE_IS_NOT_ALLOWED);
            return new HazelcastJsonValue(jsonString);
        }
    }
}