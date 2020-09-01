# Kerberos

The Hazelcast .NET client supports Kerberos as an authentication mechanism, on the Windows platform. Kerberos is not supported on other platforms at the moment. This can allow Windows client to transparently authenticate, with permissions being managed at Active Directory level.

Kerberos support requires the Enterprise version of the server, release 4.1 or above.

## Client Configuration

Kerberos support for Windows is provided in a separate [Hazelcast.Net.Win32](https://www.nuget.org/packages/Hazelcast.Net.Win32/) NuGet package, which needs to be installed alongside the main [Hazelcast.Net](https://www.nuget.org/packages/Hazelcast.Net/) package.

Kerberos authentication can be activated via the configuration file, or via code. In both cases, you will need to know the Service Principal Name (a.k.a. `spn` - "`hz/cluster1234`" in the examples below) corresponding to the Hazelcast cluster.

Configuration file:

```json
"hazelcast": {
    "authentication": {
        "credentialsFactory": {
            "typeName": "Hazelcast.Security.KerberosCredentialsFactory, Hazelcast.Net.Win32",
            "args": {
                "spn": "hz/cluster1234"
            }
        }
    }
}
```

Code:

```csharp
var client = HazelcastClientFactory.CreateClient(options => {
    options.Authentication.ConfigureKerberosCredentials("hz/cluster1234");
});
```

## Server Configuration

Until there is a 4.1 release, see [this page](https://docs.hazelcast.org/docs/latest-dev/manual/html-single/index.html#kerberos-authentication-type) for documentation.

(to be completed)