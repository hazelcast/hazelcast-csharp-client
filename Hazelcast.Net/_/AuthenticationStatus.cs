using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast
{
    internal enum AuthenticationStatus
    {
        Authenticated = 0,
        CredentialsFailed = 1,
        SerializationVersionMismatch = 2,
        NotAllowedInCluster = 3
    }
}
