// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using Hazelcast.Serialization;
namespace Hazelcast.Models
{
    /// <summary>
    /// Represents a collection of vector values indexed by names.
    /// </summary>
    public class MultiVectorValues : VectorValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiVectorValues"/> class with the specified indexed vectors.
        /// </summary>
        /// <param name="indexNameToVector">A dictionary containing index names and their corresponding vector values.</param>
        public MultiVectorValues(IDictionary<string, float[]> indexNameToVector)
        {
            IndexNameToVector = indexNameToVector;
        }

        /// <summary>
        /// Gets the dictionary containing index names and their corresponding vector values.
        /// </summary>
        public IDictionary<string, float[]> IndexNameToVector { get; }

        public override string ToString()
        {
            var val = IndexNameToVector == null
                ? "null"
                : $"{{ {string.Join(", ", IndexNameToVector.Select(entry => $"Index:{entry.Key}, Vectors:{string.Join(", ", entry.Value)}"))} }}";
            
            return $"MultiVectorValues{{{val}}}";
        }

    }
}
