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

namespace Hazelcast.Projections
{
    /// <summary>
    /// Creates <see cref="IProjection"/> instances.
    /// </summary>
    public static class Project
    {
        /// <summary>
        /// Projects a single attribute.
        /// </summary>
        /// <param name="attributePath">The attribute.</param>
        /// <returns>A projection.</returns>
        public static IProjection SingleAttribute(string attributePath)
            => new SingleAttributeProjection(attributePath);

        /// <summary>
        /// Projects multiple attributes.
        /// </summary>
        /// <returns>A projection.</returns>
        public static IProjection MultipleAttribute()
            => new MultiAttributeProjection();
    }
}
