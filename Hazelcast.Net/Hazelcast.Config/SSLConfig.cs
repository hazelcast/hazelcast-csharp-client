// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Security.Authentication;
using System.Text;

namespace Hazelcast.Config
{
    /// <summary>
    /// COnfiguration for client to server secure communication via SSL
    /// </summary>
    public class SSLConfig
    {
        /// <summary>
        /// Certificate Name to be validated against SAN field of the remote certificate, if not present then the CN part of the Certificate Subject.
        /// </summary>
        public const string CertificateName = "CertificateServerName";

        /// <summary>
        /// Certificate File path.
        /// </summary>
        public const string CertificateFilePath = "CertificateFilePath";

        /// <summary>
        /// Password need to import the certificates.
        /// </summary>
        public const string CertificatePassword = "CertificatePassword";

        /// <summary>
        /// SSL/TLS protocol. string value of enum type <see cref="System.Security.Authentication.SslProtocols"/>
        /// </summary>
        public const string SslProtocol = "SslProtocol";

       /// <summary>
        /// specifies whether the certificate revocation list is checked during authentication.
        /// </summary>
        public const string CheckCertificateRevocation = "CheckCertificateRevocation";

        /// <summary>
        /// The property is used to configure ssl to enable certificate chain validation.
        /// </summary>
        public const string ValidateCertificateChain = "ValidateCertificateChain";

        /// <summary>
        /// The property is used to configure ssl to enable Certificate name validation
        /// </summary>
        public const string ValidateCertificateName = "ValidateCertificateName";

        private bool _enabled;

        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        public SSLConfig()
        {
            SetProperty(ValidateCertificateChain, true.ToString());
        }

        public bool IsEnabled()
        {
            return _enabled;
        }

        public SSLConfig SetEnabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        public virtual Dictionary<string, string> GetProperties()
        {
            return _properties;
        }

        public SSLConfig SetProperties(Dictionary<string, string> properties)
        {
            _properties = properties;
            return this;
        }

        public virtual string GetProperty(string name)
        {
            string value;
            _properties.TryGetValue(name, out value);
            return value;
        }

        public virtual SSLConfig SetProperty(string name, string value)
        {
            _properties[name] = value;
            return this;
        }

        internal bool IsValidateCertificateChain()
        {
            var prop = GetProperty(ValidateCertificateChain);
            return AbstractXmlConfigHelper.CheckTrue(prop);
        }

        internal bool IsValidateCertificateName()
        {
            var prop = GetProperty(ValidateCertificateName);
            return AbstractXmlConfigHelper.CheckTrue(prop);
        }

        internal string GetCertificateName()
        {
            return GetProperty(CertificateName);
        }

        internal string GetCertificateFilePath()
        {
            return GetProperty(CertificateFilePath);
        }

        internal string GetCertificatePassword()
        {
            return GetProperty(CertificatePassword);
        }

        internal SslProtocols GetSslProtocol()
        {
            var sslProtocol = GetProperty(SslProtocol);
            if (sslProtocol == null)
            {
#if !NETSTANDARD
                return SslProtocols.Tls;
#else
                return SslProtocols.None;
#endif
            }

            if (Enum.TryParse(sslProtocol, true, out SslProtocols result))
            {
                return result;
            }

            var allNames = string.Join(", ", Enum.GetNames(typeof(SslProtocols)));
            throw new ArgumentException($"Invalid value of the SslProtocol: '{sslProtocol}'. You should use one of SslProtocol enum values: {allNames}");
        }

        internal bool IsCheckCertificateRevocation()
        {
            var prop = GetProperty(CheckCertificateRevocation);
            return AbstractXmlConfigHelper.CheckTrue(prop);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SSLConfig");
            sb.Append("{ enabled=").Append(_enabled);
            sb.Append(", properties=").Append(_properties);
            sb.Append('}');
            return sb.ToString();
        }
    }
}