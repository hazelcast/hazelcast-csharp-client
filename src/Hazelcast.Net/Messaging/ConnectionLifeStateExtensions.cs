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

using System;

namespace Hazelcast.Messaging;

/// <summary>
/// Provides extension methods for the <see cref="ConnectionLifeState"/> enum.
/// </summary>
internal static class ConnectionLifeStateExtensions
{
    /// <summary>
    /// Gets the worse out of two states.
    /// </summary>
    /// <param name="state">This state.</param>
    /// <param name="other">The other state.</param>
    /// <returns>Gets the worse out of the two states.</returns>
    public static ConnectionLifeState Worse(this ConnectionLifeState state, ConnectionLifeState other)
        => (ConnectionLifeState)Math.Min((byte)state, (byte)other);
}
