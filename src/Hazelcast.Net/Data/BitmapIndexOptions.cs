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

namespace Hazelcast.Data
{
    /// <summary>
    /// Configures indexing options for <see cref="IndexType.Bitmap"/> indexes.
    /// </summary>
    public class BitmapIndexOptions
    {
        /// <summary>
        /// Gets or sets the unique key.
        /// </summary>
        public string UniqueKey { get; set; } = Predicates.Query.KeyName;

        /// <summary>
        /// Gets or sets the <see cref="UniqueKeyTransformation"/> which will be
        /// applied to the <see cref="UniqueKey"/> value.
        /// </summary>
        public UniqueKeyTransformation UniqueKeyTransformation { get; set; } = UniqueKeyTransformation.Object;
    }
}
