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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    internal class SslLayer
    {
        private static readonly bool IsSslProtocolsNoneSupported = DetermineSslProtocolsNoneSupport();

        private readonly SslOptions _options;
        private readonly ILogger _logger;

        static SslLayer()
        {
            HConsole.Configure(consoleOptions => consoleOptions.Configure<SslLayer>().SetPrefix("SSL"));
        }

        private static bool DetermineSslProtocolsNoneSupport()
        {
            // see source code for System.Net.Security.SslState, this is how the SslState determines
            // whether SslProtocols.None is accepted, and throws if it is not supported - we need this
            // because it is supported with framework 4.7+ but not 4.6.2.
            //
            // https://referencesource.microsoft.com/#System/net/System/Net/SecureProtocols/_SslState.cs,5d0d274f6285d5dd

            var p = typeof(ServicePointManager).GetProperty("DisableSystemDefaultTlsVersions", BindingFlags.Static | BindingFlags.NonPublic);
            return p == null || !(bool)p.GetValue(null);
        }

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

            // if targetHost does not match the server certificate name then a RemoteCertificateNameMismatch error will
            // be reported, which can be ignored with options.ValidateCertificateName being false. If it is true, then
            // options.CertificateName *must* be set to the server certificate name.

            var targetHost = _options.CertificateName ?? "";
            _logger.LogDebug("TargetHost: {TargetHost}", targetHost);

            // _options.Protocol is 'None' by default
            //
            // as per https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls
            //
            //  "We recommend that you do not specify the TLS version. Configure your code to let the OS decide on the TLS
            //  version. When your app lets the OS choose the TLS version, it automatically takes advantage of new protocols
            //  added in the future, such as TLS 1.3 + the OS blocks protocols that are discovered not to be secure."
            //
            //  "SslStream, using .NET Framework 4.7 and later versions, defaults to the OS choosing the best security
            //  protocol and version. To get the default OS best choice, if possible, don't use the method overloads of
            //  SslStream that take an explicit SslProtocols parameter. Otherwise, pass SslProtocols.None."
            //
            // AuthenticateAsClientAsync:
            //
            //  "Starting with .NET Framework 4.7, this method authenticates using None, which allows the operating system
            //  to choose the best protocol to use, and to block protocols that are not secure. In .NET Framework 4.6 (and
            //  .NET Framework 4.5 with the latest security patches installed), the allowed TLS/SSL protocols versions are
            //  1.2, 1.1, and 1.0 (unless you disable strong cryptography by editing the Windows Registry)."

            var protocol = _options.Protocol;
            if (!IsSslProtocolsNoneSupported && protocol == SslProtocols.None)
            {
                _logger.LogInformation("Configured protocol 'None' is not supported, falling back to 'Tls12'.");
#pragma warning disable CA5398 // Avoid hardcoded SslProtocols values
                protocol = SslProtocols.Tls12;
#pragma warning restore CA5398
            }

            try
            {
                await sslStream.AuthenticateAsClientAsync(targetHost, clientCertificates, protocol, _options.CheckCertificateRevocation).CfAwait();
            }
            catch (Exception e)
            {
                throw new ConnectionException("Failed to establish an SSL connection (see inner exception).", e);
            }

            _logger.LogInformation($"Established SSL connection, protocol {sslStream.SslProtocol}, {(sslStream.IsEncrypted ? "" : "not ")}encrypted, {(sslStream.IsMutuallyAuthenticated ? "" : "not ")}mutually authenticated");

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
                _logger.IfWarning()?.LogWarning(e, "Failed to load client certificate at \"{CertificatePath}\".", _options.CertificatePath);
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
                    _logger.IfWarning()?.LogWarning("SSL certificate error: {PolicyErrors} (chain status: {StatusInformations}).", policyErrors, string.Join(", ", chain.ChainStatus.Select(x => x.StatusInformation)));
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
                    var name = "";
                    try
                    {
                        name = $" (cert name: '{cert.Subject}')";
                    }
                    catch { /* bah */ }
                    _logger.IfWarning()?.LogWarning($"SSL certificate error: {PolicyErrors}{Name}.", policyErrors, name);
                    validation = false;
                }
                else
                {
                    _logger.LogInformation("SSL certificate errors (name validation) ignored by client configuration.");
                }
            }

            if (policyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
            {
                _logger.IfWarning()?.LogWarning("SSL certificate error: {PolicyErrors}.", policyErrors);
                validation = false;
            }

            return validation;
        }
    }
}
