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
namespace Hazelcast.Models
{
    /// <summary>
    /// Represents a base class for collection of vector values.
    /// </summary>
    public abstract class VectorValues
    {
        internal VectorValues() { }
        
        /// <summary>
        /// Creates a new instance of the <see cref="VectorValues"/> class with a single vector.
        /// </summary>
        /// <param name="vector">The vector values.</param>
        /// <returns>A new instance of the <see cref="SingleVectorValues"/> class containing the specified vector.</returns>
        public static VectorValues Of(float[] vector)
        {
            return new SingleVectorValues(vector);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="VectorValues"/> class with multiple vectors indexed by names.
        /// </summary>
        /// <param name="indexToVectors">An array of tuples containing index names and their corresponding vector values.</param>
        /// <returns>A new instance of the <see cref="MultiVectorValues"/> class containing the specified indexed vectors.</returns>
        public static VectorValues Of(params (string indexName, float[] vector)[] indexToVectors)
        {
            var indexNameToVector = new Dictionary<string, float[]>();
            foreach (var (indexName, vector) in indexToVectors)
            {
                indexNameToVector[indexName] = vector;
            }
            return new MultiVectorValues(indexNameToVector);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="VectorValues"/> class with multiple vectors indexed by names.
        /// </summary>
        /// <param name="indexNameToVector">A dictionary containing index names and their corresponding vector values.</param>
        /// <returns>A new instance of the <see cref="MultiVectorValues"/> class containing the specified indexed vectors.</returns>
        public static VectorValues Of(IDictionary<string, float[]> indexNameToVector)
        {
            return new MultiVectorValues(indexNameToVector);
        }
    }
}
