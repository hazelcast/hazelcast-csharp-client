using System;
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
