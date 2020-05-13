﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Data;
using Hazelcast.Logging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Security
{
    /// <summary>
    /// Represents the default <see cref="IAuthenticator"/>.
    /// </summary>
    public class Authenticator : IAuthenticator
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="Authenticator"/> class.
        /// </summary>
        public Authenticator()
        {
            XConsole.Configure(this, config => config.SetIndent(4).SetPrefix("AUTH"));
        }

        /// <inheritdoc />
        public async ValueTask<AuthenticationResult> AuthenticateAsync(Client client)
        {
            var info = await TryAuthenticateAsync(client);
            // but maybe we want to capture an exception here?
            if (info == null) throw new Exception("Failed to authenticated.");
            return info;
        }

        private async ValueTask<AuthenticationResult> TryAuthenticateAsync(Client client)
        {
            // TODO accept parameters etc

            // RC assigns a GUID but the default cluster name is 'dev' - else, cant connect
            var clusterName = "dev";
            var username = (string)null; // null
            var password = (string)null; // null
            var clientId = Guid.NewGuid();
            var clientType = "CSP"; // CSharp
            var serializationVersion = (byte)0x01;
            var clientVersion = "4.0";
            var clientName = "hz.client_0";
            var labels = new HashSet<string>();

            var requestMessage = ClientAuthenticationCodec.EncodeRequest(clusterName, username, password, clientId, clientType, serializationVersion, clientVersion, clientName, labels);
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