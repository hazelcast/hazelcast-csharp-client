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
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    internal class SslLayer
    {
        private readonly SslOptions _options;
        private readonly ILogger _logger;

        public SslLayer(SslOptions options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory?.CreateLogger<SslLayer>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async ValueTask<Stream> GetStreamAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            if (!_options.Enabled) return stream;

            var sslStream = new SslStream(stream, false, ValidateCertificate, null);

            var clientCertificates = GetClientCertificatesOrDefault();

            var targetHost = _options.CertificateName ?? ""; // TODO: uh?!

            try
            {
                await sslStream.AuthenticateAsClientAsync(targetHost, clientCertificates, _options.Protocol, _options.CheckCertificateRevocation).CfAwait();
            }
            catch (Exception e)
            {
                throw new ConnectionException("Failed to establish an SSL connection (see inner exception).", e);
            }

            _logger.LogInformation($"Established SSL connection, protocol {sslStream.SslProtocol}, {(sslStream.IsEncrypted?"":"not ")}encrypted, {(sslStream.IsMutuallyAuthenticated?"":"not ")}mutually authenticated");

            return sslStream;
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets the client certificate, or a default certificate.
        /// </summary>
        internal X509Certificate2Collection GetClientCertificatesOrDefault()
        {
            if (_options.CertificatePath == null)
                return null;

            var clientCertificates = new X509Certificate2Collection();
            try
            {
                clientCertificates.Import(_options.CertificatePath, _options.CertificatePassword, X509KeyStorageFlags.DefaultKeySet);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"Failed to load client certificate at \"{_options.CertificatePath}\".");
                throw;
            }

            return clientCertificates;
        }

        /// <summary>
        /// (internal for tests only)
        /// Validates a certificate.
        /// </summary>
        internal bool ValidateCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            if (policyErrors == SslPolicyErrors.None)
                return true;

            var validation = true;

            if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
            {
                if (_options.ValidateCertificateChain)
                {
                    _logger.LogWarning($"SSL certificate error: {policyErrors} (chain status: " +
                                       $" {string.Join(", ", chain.ChainStatus.Select(x => x.StatusInformation))}).");
                    validation = false;
                }
                else
                {
                    _logger.LogInformation("SSL certificate errors (chain validation) ignored by client configuration.");
                }
            }

            if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
            {
                if (_options.ValidateCertificateName)
                {
                    _logger.LogWarning($"SSL certificate error: {policyErrors}.");
                    validation = false;
                }
                else
                {
                    _logger.LogInformation("SSL certificate errors (name validation) ignored by client configuration.");
                }
            }

            if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
            {
                _logger.LogWarning($"SSL certificate error: {policyErrors}.");
                validation = false;
            }

            return validation;
        }
    }
}
