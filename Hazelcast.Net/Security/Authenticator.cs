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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.Security
{
    /// <summary>
    /// Represents the default <see cref="IAuthenticator"/>.
    /// </summary>
    public class Authenticator : IAuthenticator
    {
        private static string _clientVersion;
        private readonly HazelcastConfiguration _configuration;
        private readonly ISerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        public Authenticator(HazelcastConfiguration configuration, ISerializationService serializationService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _serializationService = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
            XConsole.Configure(this, config => config.SetIndent(4).SetPrefix("AUTH"));
        }

        /// <inheritdoc />
        public async ValueTask<AuthenticationResult> AuthenticateAsync(Client client, Guid clusterClientId, string clusterClientName)
        {
            var info = await TryAuthenticateAsync(client, clusterClientId, clusterClientName);
            if (info != null) return info;

            var credentialsFactory = _configuration.Security.CredentialsFactory;
            if (credentialsFactory is IResettableCredentialsFactory resettableCredentialsFactory)
            {
                resettableCredentialsFactory.Reset();

                // try again
                info = await TryAuthenticateAsync(client, clusterClientId, clusterClientName);
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

        private async ValueTask<AuthenticationResult> TryAuthenticateAsync(Client client, Guid clientId, string clientName)
        {
            // TODO accept parameters etc

            const string clientType = "CSP"; // CSharp

            var clusterName = _configuration.ClusterName;
            var serializationVersion = _serializationService.GetVersion();
            var clientVersion = ClientVersion;
            var labels = _configuration.Labels;

            var credentialsFactory = _configuration.Security.CredentialsFactory;
            var credentials = credentialsFactory.NewCredentials();

            ClientMessage requestMessage;
            switch (credentials)
            {
                case IPasswordCredentials passwordCredentials:
                    requestMessage = ClientAuthenticationCodec.EncodeRequest(clusterName, passwordCredentials.Name, passwordCredentials.Password, clientId, clientType, serializationVersion, clientVersion, clientName, labels);
                    break;

                case ITokenCredentials tokenCredentials:
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, tokenCredentials.Token, clientId, clientType, serializationVersion, clientVersion, clientName, labels);
                    break;

                default:
                    var bytes = _serializationService.ToData(credentials).ToByteArray();
                    requestMessage = ClientAuthenticationCustomCodec.EncodeRequest(clusterName, bytes, clientId, clientType, serializationVersion, clientVersion, clientName, labels);
                    break;
            }

            XConsole.WriteLine(this, "Send auth request");
            var responseMessage = await client.SendAsync(requestMessage);
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