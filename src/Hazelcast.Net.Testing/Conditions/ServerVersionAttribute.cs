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

using System;
using NuGet.Versioning;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Specifies that a class or a method overrides the server version.
    /// </summary>
    /// <remarks>
    /// <para>This attributes takes precedence over all other methods of defining the
    /// server version.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServerVersionAttribute :  Attribute
    {
        /// <summary>
        /// Specifies that a class or a method overrides the server version.
        /// </summary>
        /// <param name="version">The server version.</param>
        public ServerVersionAttribute(string version)
        {
            if (!NuGetVersion.TryParse(version, out var v))
                throw new ArgumentException("Invalid version.", nameof(version));

            Version = v;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        public NuGetVersion Version { get; }
    }
}
