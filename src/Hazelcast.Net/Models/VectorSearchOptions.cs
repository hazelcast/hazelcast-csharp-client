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
    /// Represents the options for performing a vector search.
    /// </summary>
    public class VectorSearchOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorSearchOptions"/> class with the specified options.
        /// </summary>
        /// <param name="includeValue">Whether to include values in the search results.</param>
        /// <param name="includeVectors">Whether to include vectors in the search results.</param>
        /// <param name="limit">The maximum number of results to return.</param>
        /// <param name="hints">Additional hints for the search.</param>
        public VectorSearchOptions(bool includeValue = default,
            bool includeVectors = default,
            int limit = default,
            IDictionary<string, string> hints = null)
        {
            IncludeValue = includeValue;
            IncludeVectors = includeVectors;
            Limit = limit;
            Hints = hints;
        }

        /// <summary>
        /// Gets a value indicating whether to include values in the search results.
        /// </summary>
        public bool IncludeValue { get; }

        /// <summary>
        /// Gets a value indicating whether to include vectors in the search results.
        /// </summary>
        public bool IncludeVectors { get; }

        /// <summary>
        /// Gets the maximum number of results to return.
        /// </summary>
        public int Limit { get; }

        /// <summary>
        /// Gets additional hints for the search.
        /// </summary>
        public IDictionary<string, string> Hints { get; }
    }
}
