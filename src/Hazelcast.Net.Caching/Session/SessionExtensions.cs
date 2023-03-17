// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.AspNetCore.Http;

namespace Hazelcast.Caching.Session;

/// <summary>
/// This class wraps ISession extensions in the Microsoft.AspNetCore.Http.Extensions with LoadAsync call.
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Gets a string value from <see cref="ISession"/>.
    /// </summary>
    /// <remarks>It loads the session as async. It is strongly advised to use async extensions over Hazelcast distributed
    /// cache implementation.</remarks>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    /// /// <param name="token">Cancellation Token</param>
    public static async Task<string?> GetStringAsync(this ISession session, string key, CancellationToken token = default)
    {
        await session.LoadAsync(token).CfAwait();
        return session.GetString(key);
    }

    /// <summary>
    /// Gets an int value from <see cref="ISession"/>.
    /// <remarks>It loads the session as async. It is strongly advised to use async extensions over Hazelcast distributed
    /// cache implementation.</remarks>
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    /// <param name="token">Cancellation Token</param>
    public static async Task<int?> GetInt32Async(this ISession session, string key, CancellationToken token = default)
    {
        await session.LoadAsync(token).CfAwait();
        return session.GetInt32(key);
    }
    
    /// <summary>
    /// Gets a byte-array value from <see cref="ISession"/>.
    /// <remarks>It loads the session as async. It is strongly advised to use async extensions over Hazelcast distributed
    /// cache implementation.</remarks>
    /// </summary>
    /// <param name="session">The <see cref="ISession"/>.</param>
    /// <param name="key">The key to read.</param>
    /// <param name="token">Cancellation Token</param>
    public static async Task<byte[]?> GetAsync(this ISession session, string key, CancellationToken token = default)
    {
        await session.LoadAsync(token).CfAwait();
        return session.Get(key);
    }
}
