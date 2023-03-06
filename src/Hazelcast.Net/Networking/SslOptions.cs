// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Security.Cryptography.X509Certificates;
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
        {
#if NETSTANDARD2_0
            KeyStorageFlags = X509KeyStorageFlags.DefaultKeySet;
#else
            // DefaultKeySet causes error: System.ComponentModel.Win32Exception (0x8009030D):
            // "The credentials supplied to the package were not recognized..." on .NET 7 whereas
            // this works - however it is not available on netstandard2.0.
            KeyStorageFlags = X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet;
#endif
        }

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
            KeyStorageFlags = other.KeyStorageFlags;
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
        /// Gets or sets the X509 key storage flags.
        /// </summary>
        internal X509KeyStorageFlags KeyStorageFlags { get; set; }

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
#pragma warning disable IDE0072
                // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
                _sslProtocol = value switch
                {
                    SslProtocols.None => value,
#pragma warning disable CA5397 // Do not use deprecated SslProtocols values - but, we still support them
#if NET7_0_OR_GREATER
#pragma warning disable SYSLIB0039 // Required for .NET 7
#endif
                    SslProtocols.Tls => value,
                    SslProtocols.Tls11 => value,
#pragma warning restore CA5397
#if NET7_0_OR_GREATER
#pragma warning restore SYSLIB0039
#endif
#pragma warning disable CA5398 // Avoid hardcoded SslProtocols values - well, here, yes
                    SslProtocols.Tls12 => value,
#pragma warning restore CA5398
                    _ => throw new ConfigurationException("Invalid value. Value must be None, Tls, Tls11 or Tls12.")
                };
#pragma warning restore IDE0072
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
            text.Append("', KeyStorageFlags='").Append(KeyStorageFlags);
            text.Append("'}");
            return text.ToString();
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SslOptions Clone() => new(this);
    }
}
