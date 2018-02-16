// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Config
{
    /// <summary>
    /// COnfiguration for client to server secure communication via SSL
    /// </summary>
    public class SSLConfig
    {
        /// <summary>
        /// Certifate Name; CN part of the Certificate Subject.
        /// </summary>
        public const string CertificateName = "CertificateServerName";

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

        public SSLConfig SetProperties(Dictionary<string, string> properites)
        {
            _properties = properites;
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
            _properties.Add(name, value);
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