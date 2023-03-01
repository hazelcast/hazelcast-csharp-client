// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Configuration.Binding
{
    /// <summary>
    /// Options class used by the configuration binder.
    /// </summary>
    internal class BinderOptions
    {
        /// <summary>
        /// When false (the default), the binder will only attempt to set public properties.
        /// If true, the binder will attempt to set all non read-only properties.
        /// </summary>
        public bool BindNonPublicProperties { get; set; }
    }
}
