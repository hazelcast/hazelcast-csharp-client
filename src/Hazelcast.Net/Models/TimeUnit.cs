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

using Hazelcast.Core;

namespace Hazelcast.Models;

/// <summary>
/// Defines a time unit.
/// </summary>
/// <remarks>
/// <para>Maps to <c>java.util.concurrent.TimeUnit</c>, we are using the same values.</para>
/// </remarks>
public enum TimeUnit : long
{
    /// <summary>
    /// Time unit representing one thousandth of a microsecond.
    /// </summary>
    [Enums.JavaName("NANOSECONDS")] Nanoseconds = 1,

    /// <summary>
    /// Time unit representing one thousandth of a millisecond.
    /// </summary>
    [Enums.JavaName("MICROSECONDS")] Microseconds = 1_000,

    /// <summary>
    /// Time unit representing one thousandth of a second.
    /// </summary>
    [Enums.JavaName("MILLISECONDS")] Milliseconds = 1_000_000,

    /// <summary>
    /// Time unit representing one second.
    /// </summary>
    [Enums.JavaName("SECONDS")] Seconds = 1_000_000_000,

    /// <summary>
    /// Time unit representing a minute i.e. sixty seconds.
    /// </summary>
    [Enums.JavaName("MINUTES")] Minutes = 60_000_000_000,

    /// <summary>
    /// Time unit representing an hour i.e. sixty minutes.
    /// </summary>
    [Enums.JavaName("HOURS")] Hours = 3600_000_000_000,

    /// <summary>
    /// Time unit representing a day i.e. twenty four hours.
    /// </summary>
    [Enums.JavaName("DAYS")] Days = 86400_000_000_000,
}
