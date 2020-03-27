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

using Hazelcast.Security;

namespace Hazelcast.Config
{
    public class KerberosIdentityConfig : IIdentityConfig
    {
        private readonly string _spn;
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;

        public KerberosIdentityConfig(string spn, string username = null, string password = null, string domain = null)
        {
            _spn = spn;
            _username = username;
            _password = password;
            _domain = domain;
        }

        public object Clone()
        {
            return new KerberosIdentityConfig(_spn, _username, _password, _domain);
        }

        public ICredentialsFactory AsCredentialsFactory()
        {
            return _username == null && _password == null && _domain == null
                ? new KerberosCredentialsFactory(_spn)
                : new KerberosCredentialsFactory(_spn, _username, _password, _domain);
        }
    }
}