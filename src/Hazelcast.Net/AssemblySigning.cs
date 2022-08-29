// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Hazelcast
{
    /// <summary>
    /// Provides constants for assembly signing.
    /// </summary>
    internal static class AssemblySigning
    {
        /// <summary>
        /// Gets the Hazelcast assembly signing public key.
        /// </summary>
        /// <remarks>
        /// <para>This key is used in <c>AssemblyInfo.cs</c> to expose the internals
        /// of the <c>Hazelcast.Net</c> assembly to other assemblies such as tests,
        /// via <see cref="InternalsVisibleToAttribute"/> attributes.</para>
        /// </remarks>
        internal const string PublicKey = "00240000048000009400000006020000002400005253413100040000010001004d81045a994968" +
                                          "ac643918d7bbce405b2473471d8de6aed6bbffc0fe1874bfcabf3c0b437c6c5293a589bdcbe884" +
                                          "c6d86934069b35deaf5ab2e770cbff41a20dd4014bb53e481c30bd3ead29437b02dec5916a717a" +
                                          "4a2b4fd353e81238b89ae09e5ba0ab615c5fef7937aabab4e240c3dffe2b948047769eeb07f674" +
                                          "589d0bb3";

        // note: The public key token is represented by the last 8 bytes of the SHA-1
        // hash of the public key under which the application is signed.

        internal static string PublicKeyToken { get; } = GetKeyToken(PublicKey);

        /// <summary>
        /// Gets the key token corresponding to a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The key token.</returns>
        internal static string GetKeyToken(string key)
        {
            var bytes = new byte[key.Length / 2];
            for (var i = 0; i < key.Length / 2; i++)
#pragma warning disable CA1305 // Specify IFormatProvider - no
                bytes[i] = byte.Parse(key.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
#pragma warning restore CA1305 // Specify IFormatProvider
#pragma warning disable CA5350 // Do Not Use Weak Cryptographic Algorithms - well, that's what PublicKeyToken uses
            using var csp = SHA1.Create();
#pragma warning restore CA5350 // Do Not Use Weak Cryptographic Algorithms
            var hash = csp.ComputeHash(bytes);
            var text = new StringBuilder();
            for (var i = 0; i < 8; i++)
                //token[i] = hash[^(i + 1)];
                text.Append($"{hash[^(i + 1)]:x2}");
            return text.ToString();
        }
    }
}
