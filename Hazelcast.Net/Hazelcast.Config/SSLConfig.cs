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
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using static Hazelcast.Config.HazelcastProperties;

namespace Hazelcast.Config
{
    /// <summary>
    /// COnfiguration for client to server secure communication via SSL
    /// </summary>
    public class SSLConfig
    {
        private SslProtocols _sslProtocol = SslProtocols.Tls12;
        public bool Enabled { get; set; } = false;
        public bool ValidateCertificateChain { get; set; } = true;
        public bool ValidateCertificateName { get; set; } = false;
        public bool CheckCertificateRevocation { get; set; } = false;
        public string CertificateName { get; set; } = null;
        public string CertificateFilePath { get; set; } = null;
        public string CertificatePassword { get; set; } = null;

        public SslProtocols SslProtocol
        {
            get => _sslProtocol;
            set
            {
                if (value < SslProtocols.Tls)
                {
                    throw new ArgumentException("Invalid Tls configuration: SslProtocol. Allowed values: Tls, Tls11 or Tls12");
                }
                _sslProtocol = value;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SSLConfig");
            sb.Append("{ enabled=").Append(Enabled);
            sb.Append(", ValidateCertificateName=").Append(ValidateCertificateName);
            sb.Append(", ValidateCertificateChain=").Append(ValidateCertificateChain);
            sb.Append(", CheckCertificateRevocation=").Append(CheckCertificateRevocation);
            sb.Append(", CertificateName=").Append(CertificateName);
            sb.Append(", CertificateFilePath=").Append(CertificateFilePath);
            sb.Append('}');
            return sb.ToString();
        }
    }
}