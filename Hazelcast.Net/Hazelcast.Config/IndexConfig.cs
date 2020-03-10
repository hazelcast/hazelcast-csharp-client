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

using System.Collections.Generic;
using static Hazelcast.Util.IndexUtil;

namespace Hazelcast.Config
{
    /// <summary>
    /// Configuration of an index.
    /// </summary>
    /// <remarks>
    /// Hazelcast support two types of indexes: sorted index and hash index.
    /// Sorted indexes could be used with equality and range predicates and have logarithmic search time.
    /// Hash indexes could be used with equality predicates and have constant search time assuming the hash
    /// function of the indexed field disperses the elements properly.
    /// <p>
    /// Index could be created on one or more attributes.
    /// </remarks>
    /// <seealso cref="Hazelcast.Config.IndexType"/>
    public class IndexConfig
    {
        public static readonly IndexType DefaultType = IndexType.Sorted;

        /// <summary>
        /// Name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the index.
        /// </summary>
        public IndexType Type { get; set; } = DefaultType;

        /// <summary>
        /// Indexed attributes.
        /// </summary>
        public IList<string> Attributes { get; set; } = new List<string>();
        
        public BitmapIndexOptions BitmapIndexOptions{ get; set; } = new BitmapIndexOptions();

        /// <summary>
        /// Adds an index attribute with the given.
        /// </summary>
        /// <param name="attribute">Attribute name.</param>
        /// <returns>This instance for chaining.</returns>
        public IndexConfig AddAttribute(string attribute)
        {
            ValidateAttribute(this, attribute);
            if (Attributes == null)
            {
                Attributes = new List<string>();
            }
            Attributes.Add(attribute);
            return this;
        }

        public override string ToString()
        {
            return $"IndexConfig[Name={Name}, IndexType= {Type}, Attributes={string.Join(",", Attributes)}]";
        }
    }
}