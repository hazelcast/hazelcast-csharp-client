# Kerberos

The Hazelcast .NET client supports Kerberos as an authentication mechanism, on the Windows platform. Kerberos is not supported by the Hazelcast .NET Client on other platforms at the moment. The Hazelcast .NET Client must connect to a server that supports Kerberos authentication: Kerberos is supported by Hazelcast servers starting with version 4.1, as an Enterprise feature.

Kerberos authentication allows Windows clients to transparently authenticate, with permissions being managed through server-level integration to LDAP-based authorization.

## Client Configuration


Kerberos support for the Hazelcast .NET Client is provided in a separate [Hazelcast.Net.Win32](https://www.nuget.org/packages/Hazelcast.Net.Win32/) NuGet package, which needs to be installed alongside the main [Hazelcast.Net](https://www.nuget.org/packages/Hazelcast.Net/) package.

Kerberos authentication can be activated via the configuration file, or via code. In both cases, you will need to know the Service Principal Name (a.k.a. `spn` - "`hz/cluster1234`" in the examples below) corresponding to the Hazelcast cluster.

Configuration file:

```json
"hazelcast": {
    "authentication": {
        "kerberos": {
            "spn": "hz/cluster1234"
        }
    }
}
```

Code:

```csharp
HazelcastOptions options;
options.Authentication.ConfigureKerberosCredentials("hz/cluster1234");
```

In both cases, the Hazelcast .NET Client transparently negociates authentication with the server.

## Server Configuration

Server security configuration (starting with 4.1) is documented in the [Security](https://docs.hazelcast.com/imdg/latest/security/security.html) section of the main Hazelcast documentation, and Kerberos authentication is documented in the [Security Reams](https://docs.hazelcast.com/imdg/latest/security/security-realms.html#kerberos-authentication) sub-section.

The Kerberos support in Hazelcast has 2 configuration parts: identity and authentication. The identity part is responsible for retrieving the service ticket from Kerberos KDC (Key Distribution Center). The authentication part verifies the service tickets.

The following XML fragment can be used as an example of a working server configuration. However, it is recommended to read the completed documentation in order to fully understand the security aspects of Kerberos.

```xml
<realm name="kerberosRealm">
    <authentication>
        <kerberos>
            <security-realm>krb5Acceptor</security-realm>

            <!-- relax flags check because .NET tokens have too many things -->
            <relax-flags-check>true</relax-flags-check>

            <!-- permissions via LDAP -->
            <ldap>
                <!-- LDAP server -->
                <url>ldap://server19.hz.local/</url>

                <!-- LDAP auth -->
                <system-user-dn>CN=Administrateur,CN=Users,DC=hz,DC=local</system-user-dn>
                <system-user-password>******</system-user-password>

                <!-- no need to auth the user, it's been done already by Kerberos -->
                <skip-authentication>true</skip-authentication>

                <!-- find the user in AD (ensure UPN is set in AD!) -->
                <user-context>CN=Users,DC=hz,DC=local</user-context>
                <user-search-scope>subtree</user-search-scope>
                <user-filter>(userPrincipalName={login})</user-filter>

                <!-- map one attribute to a role -->
                <!--
                <role-mapping-mode>attribute</role-mapping-mode>
                <role-mapping-attribute>cn</role-mapping-attribute>
                -->

                <!-- map roles via groups -->
                <role-mapping-mode>reverse</role-mapping-mode>
                <role-context>CN=Users,DC=hz,DC=local</role-context>
                <role-search-scope>subtree</role-search-scope>
                <role-filter>(member={memberDN})</role-filter>
                <role-recursion-max-depth>4</role-recursion-max-depth>
                <role-name-attribute>cn</role-name-attribute>
            </ldap>
        </kerberos>
    </authentication>
</realm>
<realm name="krb5Acceptor">
    <authentication>
        <jaas>
            <login-module class-name="com.sun.security.auth.module.Krb5LoginModule" usage="REQUIRED">
                <properties>
                    <property name="isInitiator">false</property>
                    <property name="useTicketCache">false</property>
                    <property name="doNotPrompt">true</property>
                    <property name="useKeyTab">true</property>
                    <property name="storeKey">true</property>

                    <!-- the service principal -->
                    <property name="principal">hz/cluster1234@HZ.LOCAL</property>

                    <!-- on Windows, be sure to use the proper Windows paths with backslashes, not slashes! -->
                    <property name="keyTab">path\to\hzcluster1234.keytab</property>
                </properties>
            </login-module>
        </jaas>
    </authentication>
</realm>
```