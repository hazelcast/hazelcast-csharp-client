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
            Protocol = other.Protocol;
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

        // notes on TLS 1.3 support
        //
        // the SslProtocols.Tls13 value was introduced with .NET 5.0, it is not defined
        // in netstandard 2.0 nor 2.1, but then it was defined for .NET Framework 4.8 (not 4.6.2).
        // in order to properly validate the value we'd need to create a dedicated net48 build
        // of the client, and then we'd lose all the netstandard features. in the end, this
        // validation is becoming quite complex and is probably useless. from now on, no validation.
        //
        // note that the value being defined does *not* mean that the OS will support it

        /// <summary>
        /// Gets or sets the SSL protocol.
        /// </summary>
        /// <remarks>
        /// <para>The value is passed directly to the underlying <see cref="System.Net.Security.SslStream"/>
        /// when authenticating the client. It is recommended to leave the value set to <see cref="SslProtocols.None"/>
        /// in order to let the operating system choose the best option. Alternatively, use one of TLS versions
        /// (1.1, 1.2 or 1.3 where available). Note that not all operating systems support all versions.</para>
        /// </remarks>
        public SslProtocols Protocol { get; set; } = SslProtocols.None;

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
