# Failover (Blue/Green)
Failover is a cluster backup feature. In case of connected cluster is not reachable anymore, client will use the reconnection strategy and try to reconnect to current one. If connection still cannot be established, client will try to connect next configured cluster. After failover is occurred successfully, client state will be changed from `Disconnected` to `ClusterChanged` then `Connected`. Also, client can be black listed from cluster, that will force client to failover to next configured cluster.

---

Initialization of a failover client.

```csharp
var options = new HazelcastFailoverOptionsBuilder()
                .With(fo =>
                {
                    fo.TryCount = 2;//<1>
                    
                    fo.Clients.Add(new HazelcastOptionsBuilder().With(opt => {//<2>
                        opt.ClusterName = "blue";
                        opt.Networking.Addresses.Add("CLUSTER_ADDRESS");
                        opt.Networking.RoutingMode = RoutingModes.AllMembers;
                    }).Build());
                    
                    fo.Clients.Add(new HazelcastOptionsBuilder().With(opt => {//<3>
                        opt.ClusterName = "green";
                        opt.Networking.Addresses.Add("CLUSTER_ADDRESS");
                    }).Build());

                })
                .With(args)
                .Build();

await using var client = await HazelcastClientFactory.StartNewFailoverClientAsync(options); //<4>

```

**(1)** Sets max try count. Try count is maximum number of going over clusters.

Let's assume both clusters are down, and we just initialize the client. Client will do;
 - First try to `blue`, than it will fail.
 - Try `green`, than it will fail.
 - Try `blue`, than it will fail.
 - Try `green`, than it will shutdown since try count is exhausted.

> Note: At each step, reconnection strategy will be used. Client won't change the cluster without retry the current one. Failover will be occurred as a last option. 

**(2)** Client configuration for the first cluster to be connected. Note that first `HazelcastOptionsBuilder` is able to configure to everything on the client, such as `HeartBeat`, `LoadBalancer` etc. This part is still `HazelcastOptions`. See details for client options [Options](configuration/options.md).

**(3)** Alternative cluster options. Alternative options are only for connection and authentication. Alternative client options cannot override other fields. General configuration should be done in first client options which is step **1**. For example, here **3**, we cannot change the `RoutingMode` for each cluster. Also, for server side, provided cluster must have **same partition count**. Client cannot failover to a cluster with different partition count.

**(4)** Initialize the client with failover feature. **Failover client uses different options builder and different factory method.**

---

Alternatively, options can be also done in `appsettings.json`.

```json
{      
  "hazelcast-failover": { // "hazelcast-failover" is a different section. Please don't confuse with "hazelcast" section. You should either use "hazelcast-failover" or "hazelcast".
      "tryCount": 2,
      "clients":[
          {
            // name of the cluster
            "clusterName": "blue",
            // networking options
            "networking": {
                // cluster addresses
                "addresses": [
                    "CLUSTER_ADDRESS"
                ]
            }
          },

          ///Alternative cluster options, only for connection and authentication options.
          {
            // name of the cluster
            "clusterName": "green",
            // networking options
            "networking": {
                // alternative cluster, in this case it's cloud with ssl.
                "cloud": {
                    "enabled": true,
                    "discoveryToken": "token"                    
                }, 
                "ssl": {
                    "enabled": true,
                    "validateCertificateChain": false,
                    "validateCertificateName": true,
                    "checkCertificateRevocation": true,
                    "certificateName": "cert",
                    "certificatePath": "path",
                    "certificatePassword": "password",
                    "protocol": "tls11"
                },
            }
        }
      ]  
  }
}
```
For details reading from alternative resources [see details](configuration/sources.md)

As it is seen, `HazelcastFailoverOptions` is a list of `HazelcastOptions` basically. You can find more details about `HazelcastOptions` [here](configuration/options.md).


More about Blue/Green feature [see details](https://docs.hazelcast.com/hazelcast/latest/getting-started/blue-green).