using System;
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides clustering extension methods for the <see cref="Services.ServiceGetter"/> class.
    /// </summary>
    public static class ServicesClusteringExtensions
    {
        /// <summary>
        /// Gets the <see cref="IAuthenticator"/>.
        /// </summary>
        /// <param name="_">The services getter.</param>
        /// <returns>The <see cref="IAuthenticator"/>.</returns>
        public static IAuthenticator Authenticator(this Services.ServiceGetter _)
            => Services.TryGetInstance<IAuthenticator>() ?? throw new InvalidOperationException();
    }
}
