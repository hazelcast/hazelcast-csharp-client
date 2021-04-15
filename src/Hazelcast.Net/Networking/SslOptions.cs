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

using System.Security.Authentication;
using System.Text;
using Hazelcast.Configuration;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the SSL options.
    /// </summary>
    public class SslOptions
    {
        // default is none, to let the system select the best option
        private SslProtocols _sslProtocol = SslProtocols.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="SslOptions"/> class.
        /// </summary>
        public SslOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SslOptions"/> class.
        /// </summary>
        private SslOptions(SslOptions other)
        {
            Enabled = other.Enabled;
            ValidateCertificateChain = other.ValidateCertificateChain;
            ValidateCertificateName = other.ValidateCertificateName;
            CheckCertificateRevocation = other.CheckCertificateRevocation;
            CertificateName = other.CertificateName;
            CertificatePath = other.CertificatePath;
            CertificatePassword = other.CertificatePassword;
            _sslProtocol = other._sslProtocol;
        }

        /// <summary>
        /// Whether to enable SSL.
        /// </summary>
        public bool Enabled { get; set; }

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
        /// <remarks>
        /// <para>The protocol must be a member of the <see cref="SslProtocols"/> enum,
        /// and currently only <c>Tls</c>, <c>Tls11</c> and <c>Tls12</c> are supported,
        /// though only the latest is recommended.</para>
        /// </remarks>
        public SslProtocols Protocol
        {
            get => _sslProtocol;
            set
            {
                // this would be nicer with a switch expression but dotCover (as of 2020.2.3) does no cover them
                switch (value)
                {
#pragma warning disable CA5397 // Do not use deprecated SslProtocols values - TODO: consider removing them?
                    case SslProtocols.Tls:
                    case SslProtocols.Tls11:
#pragma warning restore CA5397
#pragma warning disable CA5398 // Avoid hardcoded SslProtocols values - well, here, yes
                    case SslProtocols.Tls12:
#pragma warning restore CA5398
                        _sslProtocol = value;
                        break;
                    case SslProtocols.None:
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
            text.Append(Enabled ? "enabled" : "disabled");
            text.Append(", ValidateCertificateName=").Append(ValidateCertificateName ? "true" : "false");
            text.Append(", ValidateCertificateChain=").Append(ValidateCertificateChain ? "true" : "false");
            text.Append(", CheckCertificateRevocation=").Append(CheckCertificateRevocation ? "true" : "false");
            text.Append(", CertificateName='").Append(CertificateName);
            text.Append("', CertificatePath='").Append(CertificatePath);
            text.Append("'}");
            return text.ToString();
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SslOptions Clone() => new SslOptions(this);
    }
}
