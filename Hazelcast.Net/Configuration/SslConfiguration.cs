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

using System.Security.Authentication;
using System.Text;
using Hazelcast.Exceptions;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Represents the SSL configuration.
    /// </summary>
    public class SslConfiguration
    {
        private SslProtocols _sslProtocol = SslProtocols.Tls12;

        /// <summary>
        /// Whether to enable SSL.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Whether to validate the certificate chain.
        /// </summary>
        public bool ValidateCertificateChain { get; set; } = true;

        /// <summary>
        /// Whether to validate the certificate name.
        /// </summary>
        public bool ValidateCertificateName { get; set; }

        /// <summary>
        /// Whether to check for certificate revocation.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }

        /// <summary>
        /// Gets or sets the name of the certificate.
        /// </summary>
        public string CertificateName { get; set; }

        /// <summary>
        /// Gets or sets the full path to the certificate file.
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// Gets or sets the password for the certificate file.
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets the SSL protocol.
        /// </summary>
        public SslProtocols SslProtocol
        {
            get => _sslProtocol;
            set
            {
                switch (value)
                {
                    case SslProtocols.Tls:
                    case SslProtocols.Tls11:
                    case SslProtocols.Tls12:
                        _sslProtocol = value;
                        break;
                    default:
                        throw new ConfigurationException("Invalid value. Value must be Tls, Tls11 or Tls12.");
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append("SslConfig {");
            text.Append(IsEnabled ? "enabled" : "disabled");
            text.Append(", ValidateCertificateName=").Append(ValidateCertificateName ? "true" : "false");
            text.Append(", ValidateCertificateChain=").Append(ValidateCertificateChain ? "true" : "false");
            text.Append(", CheckCertificateRevocation=").Append(CheckCertificateRevocation ? "true" : "false");
            text.Append(", CertificateName='").Append(CertificateName);
            text.Append("', CertificatePath='").Append(CertificatePath);
            text.Append("'}");
            return text.ToString();
        }
    }
}