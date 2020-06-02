using System;
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents authentication options.
    /// </summary>
    public class AuthenticationOptions
    {
        private string _authenticatorType;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions()
        {
            Authenticator = new ServiceFactory<IAuthenticator>();
            AuthenticatorArgs = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the authenticator factory.
        /// </summary>
        public ServiceFactory<IAuthenticator> Authenticator { get; private set; }

        /// <summary>
        /// Gets or sets the type of the authenticator.
        /// </summary>
        /// <remarks>
        /// <para>Returns the correct value only if it has been set via the same property. If the
        /// authenticator has been configured via code and the <see cref="Authenticator"/>
        /// property, the value returned by this property is unspecified.</para>
        /// </remarks>
        public string AuthenticatorType
        {
            get => _authenticatorType;

            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));

                _authenticatorType = value;

                Authenticator.Creator = () => Services.CreateInstance<IAuthenticator>(value, this);
            }
        }

        /// <summary>
        /// Gets the arguments for the authenticator.
        /// </summary>
        /// <remarks>
        /// <para>Arguments are used when creating an authenticator from its type as set
        /// via the <see cref="AuthenticatorType"/> property. They are ignored if the
        /// authenticator has been configured via code and the <see cref="Authenticator"/>
        /// property.</para>
        /// </remarks>
        public Dictionary<string, object> AuthenticatorArgs { get; private set; }

        /// <summary>
        /// Clone the options.
        /// </summary>
        public AuthenticationOptions Clone()
        {
            return new AuthenticationOptions()
            {
                _authenticatorType = _authenticatorType,
                Authenticator = Authenticator.Clone(),
                AuthenticatorArgs = new Dictionary<string, object>(AuthenticatorArgs),
            };
        }
    }
}
