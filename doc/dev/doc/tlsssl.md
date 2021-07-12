# TLS/SSL

This page describes the TLS/SSL security features of Hazelcast .NET client, for connections between members and between clients and members, and mutual authentication. These security features require **Hazelcast IMDG Enterprise** edition.

One of the offers of Hazelcast is the TLS/SSL protocol which you can use to establish an encrypted communication across your cluster with key stores and trust stores.

* A Java `keyStore` is a file that includes a private key and a public certificate.
* A Java `trustStore` is a file that includes a list of certificates trusted by your application which is named as  "certificate authority". 

You should set `keyStore` and `trustStore` before starting the members. See the next section on setting `keyStore` and `trustStore` on the server side.

## TLS/SSL for Hazelcast Members

Hazelcast allows you to encrypt socket level communication between Hazelcast members and between Hazelcast clients and members, for end to end encryption. To use it, see the [TLS/SSL for Hazelcast Members section](https://docs.hazelcast.com/imdg/latest/security/tls-ssl.html#tlsssl-for-hazelcast-members).

## TLS/SSL for Hazelcast .NET Clients

TLS/SSL for the Hazelcast .NET client can be configured using the `SslOptions` class. Let's first give an example of a sample configuration and then go over the configuration options one by one:

```csharp
    var hazelcastOptions = new HazelcastOptionsBuilder().Build();
    var sslOptions = hazelcastOptions.Networking.Ssl;

    sslOptions.Enabled = true;
    sslOptions.ValidateCertificateChain = true;
    sslOptions.ValidateCertificateName = false;
    sslOptions.CheckCertificateRevocation = false;
    sslOptions.CertificateName = "CN or SAN of server certificate";
    sslOptions.CertificatePath = "client pfx file path";
    sslOptions.CertificatePassword = "client pfx password";
    sslOptions.SslProtocol = SslProtocols.Tls12;
```

Of course these can also be configured via command-line options or environment variables, or via the Hazelcast configuration file. See the [configuration](configuration.md) page for details.

### Enabling TLS/SSL

TLS/SSL for the Hazelcast .NET client can be enabled/disabled using the `Enabled` option. When this option is set to `true`, TLS/SSL will be configured with respect to the other `SslOptions` options. Setting this option to `false` will result in discarding the other `SslOptions` options.

Default value is `false` (disabled). 

### Certificate Chain validation

Remote SSL certificate chain validation can be enabled/disabled using the `SslOptions.ValidateCertificateChain` option. It is enabled by default. If you need to bypass certificate validation for some reason, you can disable it as follows by setting the value to `false`. 

Validation is done by .NET and delegated to OS, and you need to make sure your server certificate is trusted by your OS.
Please refer to [this blog](https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/) for information on how to configure your OS to trust your server certificates.

### Certificate Name Validation

Server certificate CN or SAN field can be validated against a value you set into configuration. This option is disabled by default. You can enable it by setting `SslOptions.ValidateCertificateName` to `true` and providing a name with `SslOptions.CertificateName`.

### TLS/SSL Protocol

You can configure the TLS/SSL protocol using the `SslOptions.Protocol` option. Valid options are string values of `System.Security.Authentication.SslProtocols` Enum. Depending on your .Net Framework/Net core version, below values are valid:

* **None**    : Allows the operating system to choose the best protocol to use. 
* **Ssl2**    : SSL 2.0 Protocol. *RFC 6176 prohibits the usage of SSL 2.0.* 
* **Ssl3**    : SSL 3.0 Protocol. *RFC 7568 prohibits the usage of SSL 3.0.*
* **Tls**     : TLS 1.0 Protocol described in RFC 2246. *deprecated.*
* **Tls11**   : TLS 1.1 Protocol described in RFC 4346. *deprecated.*
* **Tls12**   : TLS 1.2 Protocol described in RFC 5246. *deprecated.*

## Mutual Authentication

As explained above, Hazelcast members have key stores used to identify themselves (to other members) and Hazelcast clients have trust stores used to define which members they can trust.

Using mutual authentication, the clients also have their key stores and members have their trust stores so that the members can know which clients they can trust.

To enable mutual authentication, firstly, you need to set the following property on the server side in the `hazelcast.xml` file:

```xml
<network>
    <ssl enabled="true">
        <properties>
            <property name="javax.net.ssl.mutualAuthentication">REQUIRED</property>
        </properties>
    </ssl>
</network>
```

You can see the details of setting mutual authentication on the server side in the [Mutual Authentication section](https://docs.hazelcast.com/imdg/latest/security/tls-ssl.html#mutual-authentication) of the Hazelcast IMDG Reference Manual.

On the client side, you have to provide the client certificate and its password if there is one. Here is how you do it:

```csharp
sslOptions.CertificatePath = "client pfx file path";
sslOptions.CertificatePassword = "client pfx password";
```

The provided certificate file should be a PFX file that has private and public keys. The file path should be set with `SslOptions.CertificatePath`.
If you choose to set a password to it, you need to provide it to the configuration using the `SslOptions.CertificatePassword` option.
