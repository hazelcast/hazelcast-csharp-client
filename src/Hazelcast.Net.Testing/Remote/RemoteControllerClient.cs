// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift.Protocol;

namespace Hazelcast.Testing.Remote
{
    /// <summary>
    /// Represents a remote controller client.
    /// </summary>
    public class RemoteControllerClient : RemoteController.Client, IRemoteControllerClient
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        private RemoteControllerClient(TProtocol protocol)
            : base(protocol)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        private RemoteControllerClient(TProtocol inputProtocol, TProtocol outputProtocol)
            : base(inputProtocol, outputProtocol)
        { }

        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol protocol)
            => new RemoteControllerClient(protocol);

        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol inputProtocol, TProtocol outputProtocol)
            => new RemoteControllerClient(inputProtocol, outputProtocol);

        private async Task<T> WithLock<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken).CfAwait();
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                    return await action(cancellationToken).CfAwait();
                else
                    return default;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc />
        public Task<bool> PingAsync(CancellationToken cancellationToken = default)
            => WithLock(pingAsync, cancellationToken);

#if NETFRAMEWORK
        private Task<bool> pingAsync(CancellationToken cancellationToken)
            => Task.FromResult(ping());
#endif

        /// <inheritdoc />
        public Task<bool> CleanAsync(CancellationToken cancellationToken = default)
            => WithLock(cleanAsync, cancellationToken);

#if NETFRAMEWORK
        private Task<bool> cleanAsync(CancellationToken cancellationToken)
            => Task.FromResult(clean());
#endif

        /// <inheritdoc />
        public async Task<bool> ExitAsync(CancellationToken cancellationToken = default)
        {
            var result = await WithLock(exitAsync, cancellationToken).CfAwait();
            InputProtocol?.Transport?.Close();
            return result;
        }

#if NETFRAMEWORK
        private Task<bool> exitAsync(CancellationToken cancellationToken)
            => Task.FromResult(exit());
#endif

        /// <inheritdoc />
        public Task<Cluster> CreateClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
            => WithLock(token => createClusterAsync(hzVersion, xmlconfig, token), cancellationToken);

#if NETFRAMEWORK
        private Task<Cluster> createClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken)
            => Task.FromResult(createCluster(hzVersion, xmlconfig));
#endif

        /// <inheritdoc />
        public Task<Member> StartMemberAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => startMemberAsync(clusterId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<Member> startMemberAsync(string clusterId, CancellationToken cancellationToken)
            => Task.FromResult(startMember(clusterId));
#endif

        /// <inheritdoc />
        public Task<bool> ShutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownMemberAsync(clusterId, memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> shutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken)
            => Task.FromResult(shutdownMember(clusterId, memberId));
#endif

        /// <inheritdoc />
        public Task<bool> TerminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateMemberAsync(clusterId, memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> terminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken)
            => Task.FromResult(terminateMember(clusterId, memberId));
#endif

        /// <inheritdoc />
        public Task<bool> SuspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => suspendMemberAsync(clusterId, memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> suspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken)
            => Task.FromResult(suspendMember(clusterId, memberId));
#endif

        /// <inheritdoc />
        public Task<bool> ResumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => resumeMemberAsync(clusterId, memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> resumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken)
            => Task.FromResult(resumeMember(clusterId, memberId));
#endif

        /// <inheritdoc />
        public Task<bool> ShutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownClusterAsync(clusterId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> shutdownClusterAsync(string clusterId, CancellationToken cancellationToken)
            => Task.FromResult(shutdownCluster(clusterId));
#endif

        /// <inheritdoc />
        public Task<bool> TerminateClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateClusterAsync(clusterId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<bool> terminateClusterAsync(string clusterId, CancellationToken cancellationToken)
            => Task.FromResult(terminateCluster(clusterId));
#endif

        /// <inheritdoc />
        public Task<Cluster> SplitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => splitMemberFromClusterAsync(memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<Cluster> splitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken)
            => Task.FromResult(splitMemberFromCluster(memberId));
#endif

        /// <inheritdoc />
        public Task<Cluster> MergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => mergeMemberToClusterAsync(clusterId, memberId, token), cancellationToken);

#if NETFRAMEWORK
        private Task<Cluster> mergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken)
            => Task.FromResult(mergeMemberToCluster(clusterId, memberId));
#endif

        /// <inheritdoc />
        public Task<Response> ExecuteOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default)
            => WithLock(token => executeOnControllerAsync(clusterId, script, lang, token), cancellationToken);

#if NETFRAMEWORK
        private Task<Response> executeOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken)
            => Task.FromResult(executeOnController(clusterId, script, lang));
#endif
    }
}
