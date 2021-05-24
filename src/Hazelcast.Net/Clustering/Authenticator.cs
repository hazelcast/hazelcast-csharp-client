﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Authenticates client connections.
    /// </summary>
    internal class Authenticator
    {
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
        public async ValueTask<AuthenticationResult> AuthenticateAsync(MemberConnection client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, CancellationToken cancellationToken)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            // gets the credentials factory and don't dispose it
            // if there is none, create the default one and dispose it
            var credentialsFactory = _options.CredentialsFactory.Service;
            using var temp = credentialsFactory != null ? null : new DefaultCredentialsFactory();
            credentialsFactory ??= temp;

            _logger.LogDebug("Authenticate with {CredentialsFactoryType}", credentialsFactory.GetType().Name);

            var result = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, cancellationToken).CfAwait();
            if (result != null) return result;

            // result is null, credentials failed but we may want to retry
            if (credentialsFactory is IResettableCredentialsFactory resettableCredentialsFactory)
            {
                resettableCredentialsFactory.Reset();

                // try again
                result = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, cancellationToken).CfAwait();
                if (result != null) return result;
            }

            // nah, no chance
            throw new AuthenticationException("Invalid credentials.");
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
        private async ValueTask<AuthenticationResult> TryAuthenticateAsync(MemberConnection client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, ICredentialsFactory credentialsFactory, CancellationToken cancellationToken)
        {
            const string clientType = "CSP"; // CSharp

            var serializationVersion = _serializationService.GetVersion();
            var clientVersion = ClientVersion;
            var credentials = credentialsFactory.NewCredentials();

            ClientMessage requestMessage;
            switch (credentials)
            {
                case IPasswordCredentials passwordCredentials:
                    requestMessage = ClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name, passwordCredentials.Password, clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;

                case ITokenCredentials tokenCredentials:
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, tokenCredentials.GetToken(), clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;

                default:
                    var bytes = _serializationService.ToData(credentials).ToByteArray();
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, bytes, clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;
            }

            cancellationToken.ThrowIfCancellationRequested();

            HConsole.WriteLine(this, "Send auth request");
            var responseMessage = await client.SendAsync(requestMessage).CfAwait();
            HConsole.WriteLine(this, "Rcvd auth response");
            var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);
            HConsole.WriteLine(this, "Auth response is: " + (AuthenticationStatus) response.Status);

            return (AuthenticationStatus) response.Status switch
            {
                AuthenticationStatus.Authenticated
                    => new AuthenticationResult(response.ClusterId, response.MemberUuid, response.Address, response.ServerHazelcastVersion, response.FailoverSupported, response.PartitionCount, response.SerializationVersion, credentials.Name),

                AuthenticationStatus.CredentialsFailed
                    => null, // could want to retry

                AuthenticationStatus.NotAllowedInCluster
                    => throw new AuthenticationException("Client is not allowed in cluster."),

                AuthenticationStatus.SerializationVersionMismatch
                    => throw new AuthenticationException("Serialization mismatch."),

                _ => throw new AuthenticationException($"Received unsupported status code {response.Status}.")
            };
        }
    }
}
