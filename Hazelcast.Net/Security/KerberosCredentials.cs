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

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements <see cref="ICredentials"/> for the Kerberos protocol.
    /// </summary>
    public class KerberosCredentials : TokenCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentials"/> class with a Kerberos token.
        /// </summary>
        /// <param name="token">The Kerberos token.</param>
        public KerberosCredentials(byte[] token)
            : base(token)
        { }

        /// <inheritdoc />
        public override string ToString()
            => $"KerberosCredentials (Token, {Token.Length} bytes)";
    }
}