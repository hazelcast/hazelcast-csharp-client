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

using static Hazelcast.Util.ValidationUtil;

namespace Hazelcast.Security
{
    public class SimpleTokenCredentials : ITokenCredentials
    {
        private readonly byte[] _token;

        public SimpleTokenCredentials(byte[] token)
        {
            CheckNotNull(token, "Token has to be provided.");
            _token = token;
        }

        public string Name => Token == null ? "<empty>" : "<token>";

        public byte[] Token
        {
            get
            {
                if (_token == null) return null;
                var dst = new byte[_token.Length];
                System.Buffer.BlockCopy(_token, 0, dst, 0, _token.Length);
                return dst;
            }
        }
    }
}