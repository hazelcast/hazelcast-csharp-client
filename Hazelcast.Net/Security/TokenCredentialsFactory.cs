﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.Security
{
    /// <summary>
    /// Provides an implementation of <see cref="ICredentialsFactory"/> that returns a static token <see cref="ICredentials"/>.
    /// </summary>
    public class TokenCredentialsFactory : StaticCredentialsFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCredentialsFactory"/>.
        /// </summary>
        /// <param name="token">A token.</param>
        public TokenCredentialsFactory(byte[] token)
            : base(new TokenCredentials(token))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCredentialsFactory"/>.
        /// </summary>
        /// <param name="token">A token.</param>
        public TokenCredentialsFactory(string token)
            : this(Encoding.UTF8.GetBytes(token))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCredentialsFactory"/>.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public TokenCredentialsFactory(IReadOnlyDictionary<string, string> args)
            : this(args.GetStringValue("token"))
        { }
    }
}
