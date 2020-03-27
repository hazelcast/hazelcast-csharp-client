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
using Hazelcast.Security;

namespace Hazelcast.Config
{
    public class TokenIdentityConfig : IIdentityConfig
    {
        private byte[] Token { get; set; }
        private TokenEncoding Encoding { get; set; }

        public TokenIdentityConfig(byte[] Token, TokenEncoding Encoding = TokenEncoding.None)
        {
            
        }
        public TokenIdentityConfig(string encodedToken, TokenEncoding encoding)
        {
            Encoding = encoding;
            switch (encoding)
            {
                case TokenEncoding.None:
                    Token = System.Text.Encoding.ASCII.GetBytes(encodedToken);
                    break;
                case TokenEncoding.Base64:
                    Token = System.Convert.FromBase64String(encodedToken);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported Encoding:{encoding}");
            }
        }

        public object Clone()
        {
            return new TokenIdentityConfig(Token, Encoding);
        }

        public ICredentialsFactory AsCredentialsFactory()
        {
            return new StaticCredentialsFactory(new TokenCredentials(Token));
        }
    }

    public enum TokenEncoding
    {
        None,
        Base64
    }
}