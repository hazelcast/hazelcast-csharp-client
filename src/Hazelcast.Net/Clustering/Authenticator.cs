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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.TempCodecs;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering;

/// <summary>
/// Authenticates client connections.
/// </summary>
internal class Authenticator
{
    public static string ClusterVersionKey = "clusterVersion";
    private static string _clientVersion; // static cache (immutable value)
    private readonly AuthenticationOptions _options;
    private readonly SerializationService _serializationService;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Authenticator"/> class.
    /// </summary>
    public Authenticator(AuthenticationOptions options, SerializationService serializationService, ILoggerFactory loggerFactory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        _logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger<Authenticator>();

        HConsole.Configure(x => x.Configure<Authenticator>().SetIndent(4).SetPrefix("AUTH"));
    }

    /// <summary>
    /// Authenticates the client connection.
    /// </summary>
    /// <param name="client">The client to authenticate.</param>
    /// <param name="clusterName">The cluster name, as assigned by the client.</param>
    /// <param name="clusterClientId">The cluster unique identifier, as assigned by the client.</param>
    /// <param name="clusterClientName">The cluster client name, as assigned by the client.</param>
    /// <param name="labels">The client labels.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that will complete when the client is authenticated.</returns>
    public async ValueTask<AuthenticationResult> AuthenticateAsync(MemberConnection client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, byte routingMode, CancellationToken cancellationToken)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));

        // gets the credentials factory and don't dispose it
        // if there is none, create the default one and dispose it
        var credentialsFactory = _options.CredentialsFactory.Service;
        using var temp = credentialsFactory != null ? null : new DefaultCredentialsFactory();
        credentialsFactory ??= temp;

        _logger.IfDebug()?.LogDebug("Authenticate with {CredentialsFactoryType}", credentialsFactory.GetType().Name);

        var result = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, routingMode, cancellationToken).CfAwait();
        if (result != null) return result;

        // result is null, credentials failed but we may want to retry
        if (credentialsFactory is IResettableCredentialsFactory resettableCredentialsFactory)
        {
            resettableCredentialsFactory.Reset();

            // try again
            result = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, routingMode, cancellationToken).CfAwait();
            if (result != null) return result;
        }

        // nah, no chance
        throw new AuthenticationException("Invalid credentials.");
    }

    /// <summary>
    /// Authenticates a TPC connection.
    /// </summary>
    public async ValueTask<bool> AuthenticateTpcAsync(MemberConnection client, ClientMessageConnection connection, Guid clientId, byte[] token, CancellationToken cancellationToken)
    {
        var message = TpcClientChannelAuthenticationCodec.EncodeRequest(clientId, token);
        message.InvocationFlags |= InvocationFlags.InvokeWhenNotConnected;
        _ = await client.SendAsync(message, connection, cancellationToken).CfAwait();
        return true; // response is empty
    }

    private static string ClientVersion
    {
        get
        {
            if (_clientVersion != null) return _clientVersion;
            var version = typeof(Authenticator).Assembly.GetName().Version;
            _clientVersion = version.Major + "." + version.Minor;
            if (version.Build > 0) _clientVersion += "." + version.Build;
            return _clientVersion;
        }
    }

    // tries to authenticate
    // returns a result if successful
    // returns null if failed due to credentials (may want to retry)
    // throws if anything else went wrong
    private async ValueTask<AuthenticationResult> TryAuthenticateAsync(MemberConnection client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, ICredentialsFactory credentialsFactory, byte routingMode, CancellationToken cancellationToken)
    {
        const string clientType = "CSP"; // CSharp

        var serializationVersion = _serializationService.GetVersion();
        var clientVersion = ClientVersion;
        var credentials = credentialsFactory.NewCredentials();

        ClientMessage requestMessage;
        switch (credentials)
        {
            case IPasswordCredentials passwordCredentials:
                requestMessage = _options.TpcEnabled
                    ? TpcClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name, passwordCredentials.Password,
                        clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels)
                    : ClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name, passwordCredentials.Password,
                        clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels, routingMode);
                break;

            case ITokenCredentials tokenCredentials:
                requestMessage = _options.TpcEnabled
                    ? TpcClientAuthenticationCustomCodec.EncodeRequest(clusterName, tokenCredentials.GetToken(), clusterClientId, clientType,
                        serializationVersion, clientVersion, clusterClientName, labels)
                    : ClientAuthenticationCustomCodec.EncodeRequest(clusterName, tokenCredentials.GetToken(), clusterClientId, clientType,
                        serializationVersion, clientVersion, clusterClientName, labels, routingMode);
                break;

            default:
                var bytes = _serializationService.ToData(credentials, withSchemas: true).ToByteArray();
                requestMessage = _options.TpcEnabled
                    ? TpcClientAuthenticationCustomCodec.EncodeRequest(clusterName, bytes, clusterClientId, clientType, serializationVersion,
                        clientVersion, clusterClientName, labels)
                    : ClientAuthenticationCustomCodec.EncodeRequest(clusterName, bytes, clusterClientId, clientType, serializationVersion,
                        clientVersion, clusterClientName, labels, routingMode);
                break;
        }

        cancellationToken.ThrowIfCancellationRequested();

        requestMessage.InvocationFlags |= InvocationFlags.InvokeWhenNotConnected; // is part of the connection phase
        HConsole.WriteLine(this, "Send auth request");
        var responseMessage = await client.SendAsync(requestMessage).CfAwait();
        HConsole.WriteLine(this, "Rcvd auth response");
        var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);
        HConsole.WriteLine(this, "Auth response is: " + (AuthenticationStatus) response.Status);

        return (AuthenticationStatus) response.Status switch
        {
            AuthenticationStatus.Authenticated
                => new AuthenticationResult(response.ClusterId,
                    response.MemberUuid,
                    response.Address,
                    response.ServerHazelcastVersion,
                    response.FailoverSupported,
                    response.PartitionCount,
                    response.SerializationVersion,
                    credentials.Name,
                    response.TpcPorts,
                    response.TpcToken,
                    ParsePartitionMemberGroups(response), // Doesn't throw
                    ParseClusterVersion(response) // Doesn't throw
                ),

            AuthenticationStatus.CredentialsFailed
                => null, // could want to retry

            AuthenticationStatus.NotAllowedInCluster
                => throw new ClientNotAllowedInClusterException("Client is not allowed in cluster."),

            AuthenticationStatus.SerializationVersionMismatch
                => throw new AuthenticationException("Serialization mismatch."),

            _ => throw new AuthenticationException($"Received unsupported status code {response.Status}.")
        };
    }

    /// <summary>
    /// Parse the cluster version. Returns null if the version is invalid. Doesn't throw.
    /// </summary>
    /// <param name="version">string cluster version</param>
    /// <returns>ClusterVersion</returns>
    private ClusterVersion ParseClusterVersion(ClientAuthenticationCodec.ResponseParameters response)
    {
        if (!response.IsKeyValuePairsExists)
            return new ClusterVersion(ClusterVersion.Unknown, ClusterVersion.Unknown);
        
        var clusterVersion = "";
        try
        {
            var isClusterVersionExists = response.KeyValuePairs.TryGetValue(ClusterVersionKey, out clusterVersion);
            if (response.IsKeyValuePairsExists && isClusterVersionExists)
            {
                return ClusterVersion.Parse(clusterVersion);
            }
        }
        catch (FormatException e)
        {
            _logger.IfDebug()?.LogDebug("Failed to parse cluster version [{Version}]: {Exception}", clusterVersion, e.Message);
        }

        return new ClusterVersion(ClusterVersion.Unknown, ClusterVersion.Unknown);
    }

    /// <summary>
    /// Parse members group. (internal for testing purposes. Doesn't throw.
    /// </summary>
    internal MemberGroups ParsePartitionMemberGroups(ClientAuthenticationCodec.ResponseParameters response)
    {
        var emptyMemberGroups = new MemberGroups(new List<IList<Guid>>(0), 0, response.ClusterId, response.MemberUuid);

        if (!response.IsKeyValuePairsExists)
            return emptyMemberGroups;
        
        var isContainsPartitionGroups =  response.KeyValuePairs.TryGetValue(MemberPartitionGroup.PartitionGroupJsonField, out var jsonMessage);

        if ( isContainsPartitionGroups && string.IsNullOrEmpty(jsonMessage))
        {
            return emptyMemberGroups;
        }

        var partitionGroups = new List<IList<Guid>>();
        var version = MemberPartitionGroup.InvalidVersion;
        try
        {
            // Why is data json string?
            var jsonObject = JsonNode.Parse(response.KeyValuePairs[MemberPartitionGroup.PartitionGroupRootJsonField]);

            var isVersionValid = int.TryParse(jsonObject.AsObject()[MemberPartitionGroup.VersionJsonField].ToString(), out var versionString);
            version = isVersionValid ? versionString : MemberPartitionGroup.InvalidVersion;

            foreach (var memberIds in jsonObject[MemberPartitionGroup.PartitionGroupJsonField].AsArray())
            {
                var group = new HashSet<Guid>();
                foreach (var member in memberIds.AsArray())
                {
                    group.Add(Guid.Parse(member.ToString()));
                }
                partitionGroups.Add(group.ToList());
            }
        }
        catch (Exception e)
        {
            _logger.IfDebug()?.LogDebug("Failed to parse partition groups [{JsonMessage}]: {Message}. ", jsonMessage, e.Message);
        }

        return new MemberGroups(partitionGroups, version, response.ClusterId, response.MemberUuid);
    }
}
