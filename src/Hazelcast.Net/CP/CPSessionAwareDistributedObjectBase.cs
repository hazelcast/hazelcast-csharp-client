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

using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;

namespace Hazelcast.CP;

/// <summary>
/// Provides a base class to session-aware CP distributed objects.
/// </summary>
internal abstract class CPSessionAwareDistributedObjectBase : CPDistributedObjectBase
{
    protected CPSessionAwareDistributedObjectBase(string serviceName, string name, CPGroupId groupId, Cluster cluster, CPSessionManager sessionManager)
        : base(serviceName, name, groupId, cluster)
    {
        SessionManager = sessionManager;
    }

    protected CPSessionManager SessionManager { get; }

    protected Task<long> AcquireSessionAsync() => SessionManager.AcquireSessionAsync(CPGroupId);

    protected Task<long> AcquireSessionAsync(int count) => SessionManager.AcquireSessionAsync(CPGroupId, count);

    protected void ReleaseSession(long sessionId) => SessionManager.ReleaseSession(CPGroupId, sessionId);

    protected void ReleaseSession(long sessionId, int count) => SessionManager.ReleaseSession(CPGroupId, sessionId, count);

    protected void InvalidateSession(long sessionId) => SessionManager.InvalidateSession(CPGroupId, sessionId);

    protected long GetSession() => SessionManager.GetSessionId(CPGroupId);

    protected ValueTask<long> GetOrCreateUniqueThreadIdAsync(long localThreadId) => SessionManager.GetOrCreateUniqueThreadIdAsync(CPGroupId, localThreadId);

}
