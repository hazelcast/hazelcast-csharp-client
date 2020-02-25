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

using Hazelcast.Security;

namespace Hazelcast.Config
{
    // FIXME not sure about this, the token is not configured
    // remove this + StaticCredentialsFactory + SimpleTokenCredentials?
    public class TokenIdentityConfig : IIdentityConfig
    {
        private byte[] Token { get; set; }
        private TokenEncoding Encoding { get; set; }

        public object Clone()
        {
            return new TokenIdentityConfig{Token = Token};
        }

        public ICredentialsFactory AsCredentialsFactory()
        {
            return new StaticCredentialsFactory(new SimpleTokenCredentials(Token));
        }
    }

    public enum TokenEncoding
    {
        None,
        Base64
    }
}