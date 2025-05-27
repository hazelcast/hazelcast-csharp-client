// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;

namespace Hazelcast.Models;

/// <summary>
/// Defines the expiry policy type.
/// </summary>
public enum ExpiryPolicyType
{
    /// <summary>
    /// Expiry policy type for the CreatedExpiryPolicy.
    /// </summary>
    [Enums.JavaName("CREATED")] Created = 0,

    /// <summary>
    /// Expiry policy type for the ModifiedExpiryPolicy.
    /// </summary>
    [Enums.JavaName("MODIFIED")] Modified = 1,

    /// <summary>
    /// Expiry policy type for the AccessedExpiryPolicy.
    /// </summary>
    [Enums.JavaName("ACCESSED")] Accessed = 2,

    /// <summary>
    /// Expiry policy type for the TouchedExpiryPolicy.
    /// </summary>
    [Enums.JavaName("TOUCHED")] Touched = 3,

    /// <summary>
    /// Expiry policy type for the EternalExpiryPolicy.
    /// </summary>
    [Enums.JavaName("ETERNAL")] Eternal = 4
}
