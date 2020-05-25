// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Security;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the default <see cref="IAuthenticator"/>.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        private static string _clientVersion;
        private readonly SecurityConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        public Authenticator(SecurityConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            XConsole.Configure(this, config => config.SetIndent(4).SetPrefix("AUTH"));
        }

        /// <inheritdoc />
        public async ValueTask<AuthenticationResult> AuthenticateAsync(Client client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, ISerializationService serializationService, CancellationToken cancellationToken)
        {
            var credentialsFactory = _configuration.CredentialsFactory.Create();

            var info = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, serializationService, cancellationToken);
            if (info != null) return info;

            if (credentialsFactory is IResettableCredentialsFactory resettableCredentialsFactory)
            {
                resettableCredentialsFactory.Reset();

                // try again
                info = await TryAuthenticateAsync(client, clusterName, clusterClientId, clusterClientName, labels, credentialsFactory, serializationService, cancellationToken);
                if (info != null) return info;
            }

            // but maybe we want to capture an exception here?
            throw new Exception("Failed to authenticate.");
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

        private async ValueTask<AuthenticationResult> TryAuthenticateAsync(Client client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, ICredentialsFactory credentialsFactory, ISerializationService serializationService, CancellationToken cancellationToken)
        {
            const string clientType = "CSP"; // CSharp

            var serializationVersion = serializationService.GetVersion();
            var clientVersion = ClientVersion;

            var credentials = credentialsFactory.NewCredentials();

            ClientMessage requestMessage;
            switch (credentials)
            {
                case IPasswordCredentials passwordCredentials:
                    requestMessage = ClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name, passwordCredentials.Password, clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;

                case ITokenCredentials tokenCredentials:
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, tokenCredentials.Token, clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;

                default:
                    var bytes = serializationService.ToData(credentials).ToByteArray();
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, bytes, clusterClientId, clientType, serializationVersion, clientVersion, clusterClientName, labels);
                    break;
            }

            cancellationToken.ThrowIfCancellationRequested();

            XConsole.WriteLine(this, "Send auth request");
            var responseMessage = await client.SendAsync(requestMessage, cancellationToken);
            XConsole.WriteLine(this, "Rcvd auth response");
            var response = ClientAuthenticationCodec.DecodeResponse(responseMessage);

            switch ((AuthenticationStatus) response.Status)
            {
                case AuthenticationStatus.Authenticated:
                    break;
                case AuthenticationStatus.CredentialsFailed:
                case AuthenticationStatus.NotAllowedInCluster:
                case AuthenticationStatus.SerializationVersionMismatch:
                    return null;
                default:
                    throw new NotSupportedException();
            }

            return new AuthenticationResult(response.ClusterId, response.MemberUuid, response.Address, response.ServerHazelcastVersion, response.FailoverSupported, response.PartitionCount, response.SerializationVersion);
        }
    }
}