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


#if !NETSTANDARD

using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;

namespace Hazelcast.Security
{
    class KerberosCredentials : TokenCredentials
    {
        readonly string _spn;
        readonly TimeSpan _tokenTimeout;
        volatile byte[] _token;

        public KerberosCredentials(string spn, TimeSpan tokenTimeout)
        {
            _spn = spn;
            _tokenTimeout = tokenTimeout;
        }

        public override byte[] Token
        {
            get
            {
                if (_token == null)
                {
                    _token = GetToken();
                }

                return _token;
            }
        }

        public override void Refresh()
        {
            _token = GetToken();
        }

        private byte[] GetToken()
        {
            var provider = new KerberosSecurityTokenProvider(_spn); // no user passed
            var token = provider.GetToken(_tokenTimeout) as KerberosRequestorSecurityToken;
            var request = token.GetRequest(); // request contains ticket bytes
            return request;
        }
    }
}
#endif