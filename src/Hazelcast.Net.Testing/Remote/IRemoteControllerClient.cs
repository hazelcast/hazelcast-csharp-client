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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Testing.Remote
{
    /// <summary>
    /// Defines a remote controller client.
    /// </summary>
    public interface IRemoteControllerClient
    {
        /// <summary>
        /// Pings the remote controller.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Whether the remote controller could be pinged.</returns>
        Task<bool> PingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Cleans the remote controller.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Whether the remote controller could be cleaned.</returns>
        Task<bool> CleanAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Exits the remote controller.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Whether the remote controller could be exited.</returns>
        Task<bool> ExitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new cluster.
        /// </summary>
        /// <param name="serverVersion">The Hazelcast server version.</param>
        /// <param name="serverConfiguration">The server Xml configuration.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The new cluster.</returns>
        Task<Cluster> CreateClusterAsync(string serverVersion, string serverConfiguration, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new cluster with name in the provided configuration.
        /// </summary>
        /// <param name="serverVersion">The Hazelcast server version.</param>
        /// <param name="serverConfiguration">The server Xml configuration.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns></returns>
        Task<Cluster> CreateClusterKeepClusterNameAsync(string serverVersion, string serverConfiguration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Starts a new member.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The new member.</returns>
        Task<Member> StartMemberAsync(string clusterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shuts a member down.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the member was properly shut down.</returns>
        Task<bool> ShutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates a member.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the member was properly terminated.</returns>
        Task<bool> TerminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Suspends a member.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the member was properly suspended.</returns>
        Task<bool> SuspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resumes a member.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the member was properly resumed.</returns>
        Task<bool> ResumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Shuts a cluster down.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the cluster was properly shut down.</returns>
        Task<bool> ShutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Terminates a cluster.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>Whether the cluster was properly terminated.</returns>
        Task<bool> TerminateClusterAsync(string clusterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Splits a member from a cluster.
        /// </summary>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The ??? cluster.</returns>
        Task<Cluster> SplitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Merges a member into a cluster.
        /// </summary>
        /// <param name="clusterId">The identifier of the target cluster.</param>
        /// <param name="memberId">The identifier of the member.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The ??? cluster.</returns>
        Task<Cluster> MergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a script on the controller.
        /// </summary>
        /// <param name="clusterId">The identifier of the cluster.</param>
        /// <param name="script">The body of the script.</param>
        /// <param name="lang">The language of the script.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The result of the script.</returns>
        Task<Response> ExecuteOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs the remote controller into the cloud.
        /// </summary>
        /// <param name="baseUrl">The cloud API url, e.g. https://uat.hazelcast.cloud.</param>
        /// <param name="apiKey">The API key provided by Hazelcast cloud.</param>
        /// <param name="apiSecret">The API secret provided by Hazelcast cloud.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Task LoginToCloudAsync(string baseUrl, string apiKey, string apiSecret, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs the remote controller into the cloud, using parameters from environment variables.
        /// <list type="bullet"> 
        ///   <item>  BASE_URL = The cloud API url, e.g. https://uat.hazelcast.cloud.</item>
        ///    <item> API_KEY  = The API key provided by Hazelcast cloud.</item>
        ///    <item> API_SECRET = The API secret provided by Hazelcast cloud. </item>
        /// </list>
        /// </summary>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        Task LoginToCloudAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates and starts a cluster in the cloud.
        /// </summary>
        /// <param name="hazelcastVersion">The Hazelcast version for members to run.</param>
        /// <param name="isTlsEnabled">Whether TLS is enabled on the cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The cloud cluster.</returns>
        Task<CloudCluster> CreateCloudClusterAsync(string hazelcastVersion, bool isTlsEnabled, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a cluster from the cloud.
        /// </summary>
        /// <param name="cloudClusterId">The identifier of the cloud cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The cloud cluster.</returns>
        Task<CloudCluster> GetCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Stops a cluster in the cloud.
        /// </summary>
        /// <param name="cloudClusterId">The identifier of the cloud cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The cloud cluster.</returns>
        Task<CloudCluster> StopCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Resumes a cluster in the cloud.
        /// </summary>
        /// <param name="cloudClusterId">The identifier of the cloud cluster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The cloud cluster.</returns>
        Task<CloudCluster> ResumeCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a cluster from the cloud.
        /// </summary>
        /// <param name="cloudClusterId">The identifier of the cloud luster.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>The cloud cluster.</returns>
        Task DeleteCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default);
    }
}
