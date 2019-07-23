# Table of Contents

* [Introduction](#introduction)
* [1. Getting Started](#1-getting-started)
  * [1.1. Requirements](#11-requirements)
  * [1.2. Working with Hazelcast IMDG Clusters](#12-working-with-hazelcast-imdg-clusters)
    * [1.2.1. Setting Up a Hazelcast IMDG Cluster](#121-setting-up-a-hazelcast-imdg-cluster)
      * [1.2.1.1. Running Standalone JARs](#1211-running-standalone-jars)
      * [1.2.1.2. Adding User Library to CLASSPATH](#1212-adding-user-library-to-classpath)
  * [1.3. Downloading and Installing](#13-downloading-and-installing)
  * [1.4. Basic Configuration](#14-basic-configuration)
    * [1.4.1. Configuring Hazelcast IMDG](#141-configuring-hazelcast-imdg)
    * [1.4.2. Configuring Hazelcast .NET Client](#142-configuring-hazelcast-net-client)
        * [1.4.2.1 Group Settings](#1421-group-settings)
        * [1.4.2.2 Network Settings](#1422-network-settings)
  * [1.5. Basic Usage](#15-basic-usage)
  * [1.6. Code Samples](#16-code-samples)
* [2. Features](#2-features)
* [3. Configuration Overview](#3-configuration-overview)
  * [3.1. Configuration Options](#31-configuration-options)
    * [3.1.1. Programmatic Configuration](#311-programmatic-configuration)
    * [3.1.2. Declarative Configuration (XML)](#312-declarative-configuration-xml)
* [4. Serialization](#4-serialization)
  * [4.1. IdentifiedDataSerializable Serialization](#41-identifieddataserializable-serialization)
  * [4.2. Portable Serialization](#42-portable-serialization)
  * [4.3. Custom Serialization](#43-custom-serialization)
  * [4.4. JSON Serialization](#44-json-serialization)
  * [4.5. Global Serialization](#45-global-serialization)
* [5. Setting Up Client Network](#5-setting-up-client-network)
  * [5.1. Providing Member Addresses](#51-providing-member-addresses)
  * [5.2. Setting Smart Routing](#52-setting-smart-routing)
  * [5.3. Enabling Redo Operation](#53-enabling-redo-operation)
  * [5.4. Setting Connection Timeout](#54-setting-connection-timeout)
  * [5.5. Setting Connection Attempt Limit](#55-setting-connection-attempt-limit)
  * [5.6. Setting Connection Attempt Period](#56-setting-connection-attempt-period)
  * [5.7. Enabling Client TLS/SSL](#57-enabling-client-tlsssl)
  * [5.8. Enabling Hazelcast Cloud Discovery](#58-enabling-hazelcast-cloud-discovery)
* [6. Securing Client Connection](#6-securing-client-connection)
  * [6.1. TLS/SSL](#61-tlsssl)
    * [6.1.1. TLS/SSL for Hazelcast Members](#611-tlsssl-for-hazelcast-members)
    * [6.1.2. TLS/SSL for Hazelcast .NET Clients](#612-tlsssl-for-hazelcast-net-clients)
    * [6.1.3. Mutual Authentication](#613-mutual-authentication)
* [7. Using .NET Client with Hazelcast IMDG](#7-using-net-client-with-hazelcast-imdg)
  * [7.1. .NET Client API Overview](#71-net-client-api-overview)
  * [7.2. .NET Client Operation Modes](#72-net-client-operation-modes)
      * [7.2.1. Smart Client](#721-smart-client)
      * [7.2.2. Unisocket Client](#722-unisocket-client)
  * [7.3. Handling Failures](#73-handling-failures)
    * [7.3.1. Handling Client Connection Failure](#731-handling-client-connection-failure)
    * [7.3.2. Handling Retry-able Operation Failure](#732-handling-retry-able-operation-failure)
  * [7.4. Using Distributed Data Structures](#74-using-distributed-data-structures)
    * [7.4.1. Using Map](#741-using-map)
    * [7.4.2. Using MultiMap](#742-using-multimap)
    * [7.4.3. Using Replicated Map](#743-using-replicated-map)
    * [7.4.4. Using Queue](#744-using-queue)
    * [7.4.5. Using Set](#745-using-set)
    * [7.4.6. Using List](#746-using-list)
    * [7.4.7. Using Ringbuffer](#747-using-ringbuffer)
    * [7.4.8. Using Lock](#748-using-lock)
    * [7.4.9. Using Atomic Long](#749-using-atomic-long)
    * [7.4.10. Using Semaphore](#7410-using-semaphore)
  * [7.5. Distributed Events](#75-distributed-events)
    * [7.5.1. Cluster Events](#751-cluster-events)
      * [7.5.1.1. Listening for Member Events](#7511-listening-for-member-events)
      * [7.5.1.2. Listening for Distributed Object Events](#7512-listening-for-distributed-object-events)
      * [7.5.1.3. Listening for Lifecycle Events](#7513-listening-for-lifecycle-events)
    * [7.5.2. Distributed Data Structure Events](#752-distributed-data-structure-events)
      * [7.5.2.1. Listening for Map Events](#7521-listening-for-map-events)
  * [7.6. Distributed Computing](#76-distributed-computing)
    * [7.6.1. Using EntryProcessor](#761-using-entryprocessor)
  * [7.7. Distributed Query](#77-distributed-query)
    * [7.7.1. How Distributed Query Works](#771-how-distributed-query-works)
      * [7.7.1.1. Employee Map Query Example](#7711-employee-map-query-example)
      * [7.7.1.2. Querying by Combining Predicates with AND, OR, NOT](#7712-querying-by-combining-predicates-with-and-or-not)
      * [7.7.1.3. Querying with SQL](#7713-querying-with-sql)
	  * [7.7.1.4. Querying with JSON Strings](#7714-querying-with-json-strings)
      * [7.7.1.5. Filtering with Paging Predicates](#7715-filtering-with-paging-predicates)
    * [7.7.2. Fast-Aggregations](#772-fast-aggregations)
  * [7.8. Monitoring and Logging](#78-monitoring-and-logging)
    * [7.8.1. Enabling Client Statistics](#781-enabling-client-statistics)
    * [7.8.2. Logging Configuration](#782-logging-configuration)
* [8. Development and Testing](#8-development-and-testing)
  * [8.1. Building and Using Client From Sources](#81-building-and-using-client-from-sources)
  * [8.2. Testing](#82-testing)
* [9. Getting Help](#9-getting-help)
* [10. Contributing](#10-contributing)
* [11. License](#11-license)
* [12. Copyright](#12-copyright)

# Introduction

This document provides information about the .NET client for [Hazelcast](https://hazelcast.org/). This client uses Hazelcast's [Open Client Protocol](https://hazelcast.org/documentation/#open-binary) and works with Hazelcast IMDG 3.6 and higher versions.

### Resources

See the following for more information on .NET and Hazelcast IMDG:

* Hazelcast IMDG [website](https://hazelcast.org/)
* Hazelcast IMDG [Reference Manual](https://hazelcast.org/documentation/#imdg)
* About [.NET](https://www.microsoft.com/net)

### Release Notes

See the [Releases](https://github.com/hazelcast/hazelcast-csharp-client/releases) page of this repository.


# 1. Getting Started

This chapter provides information on how to get started with your Hazelcast .NET client. It outlines the requirements, installation 
and configuration of the client, setting up a cluster, and provides a simple application that uses a distributed map in .NET client.

## 1.1. Requirements

- Windows, Linux or MacOS
- .NET Framework 4.0 or newer, .NET core 2.0 or newer
- Java 6 or newer
- Hazelcast IMDG 3.6 or newer
- Latest Hazelcast .NET client

## 1.2. Working with Hazelcast IMDG Clusters

Hazelcast .NET client requires a working Hazelcast IMDG cluster to run. This cluster handles storage and manipulation of the user data.
Clients are a way to connect to the Hazelcast IMDG cluster and access such data.

Hazelcast IMDG cluster consists of one or more cluster members. These members generally run on multiple virtual or physical machines
and are connected to each other via network. Any data put on the cluster is partitioned to multiple members transparent to the user.
It is therefore very easy to scale the system by adding new members as the data grows. Hazelcast IMDG cluster also offers resilience. Should
any hardware or software problem causes a crash to any member, the data on that member is recovered from backups and the cluster
continues to operate without any downtime. Hazelcast clients are an easy way to connect to a Hazelcast IMDG cluster and perform tasks on
distributed data structures that live on the cluster.

In order to use Hazelcast .NET client, we first need to setup a Hazelcast IMDG cluster.

### 1.2.1. Setting Up a Hazelcast IMDG Cluster

There are following options to start a Hazelcast IMDG cluster easily:

* You can run standalone members by downloading and running JAR files from the website.
* You can embed members to your Java projects.

We are going to download JARs from the website and run a standalone member for this guide.

#### 1.2.1.1. Running Standalone JARs

Follow the instructions below to create a Hazelcast IMDG cluster:

1. Go to Hazelcast's [download page](https://hazelcast.org/download/) and download either the `.zip` or `.tar` distribution of Hazelcast IMDG.
2. Decompress the contents into any directory that you
want to run members from.
3. Change into the directory that you decompressed the Hazelcast content and then into the `bin` directory.
4. Use either `start.sh` or `start.bat` depending on your operating system. Once you run the start script, you should see the Hazelcast IMDG logs in the terminal.

You should see a log similar to the following, which means that your 1-member cluster is ready to be used:

```
INFO: [192.168.0.3]:5701 [dev] [3.10.4]

Members {size:1, ver:1} [
	Member [192.168.0.3]:5701 - 65dac4d1-2559-44bb-ba2e-ca41c56eedd6 this
]

Sep 06, 2018 10:50:23 AM com.hazelcast.core.LifecycleService
INFO: [192.168.0.3]:5701 [dev] [3.10.4] [192.168.0.3]:5701 is STARTED
```

#### 1.2.1.2. Adding User Library to CLASSPATH

When you want to use features such as querying and language interoperability, you might need to add your own Java classes to the Hazelcast member in order to use them from your .NET client. This can be done by adding your own compiled code to the `CLASSPATH`. To do this, compile your code with the `CLASSPATH` and add the compiled files to the `user-lib` directory in the extracted `hazelcast-<version>.zip` (or `tar`). Then, you can start your Hazelcast member by using the start scripts in the `bin` directory. The start scripts will automatically add your compiled classes to the `CLASSPATH`.

Note that if you are adding an `IdentifiedDataSerializable` or a `Portable` class, you need to add its factory too. Then, you should configure the factory in the `hazelcast.xml` configuration file. This file resides in the `bin` directory where you extracted the `hazelcast-<version>.zip` (or `tar`).

The following is an example configuration when you are adding an `IdentifiedDataSerializable` class:

```xml
<hazelcast>
     ...
     <serialization>
        <data-serializable-factories>
            <data-serializable-factory factory-id=<identified-factory-id>>
                IdentifiedFactoryClassName
            </data-serializable-factory>
        </data-serializable-factories>
    </serialization>
    ...
</hazelcast>
```
If you want to add a `Portable` class, you should use `<portable-factories>` instead of `<data-serializable-factories>` in the above configuration.

See the [Hazelcast IMDG Reference Manual](http://docs.hazelcast.org/docs/latest/manual/html-single/index.html#getting-started) for more information on setting up the clusters.

## 1.3. Downloading and Installing

Hazelcast .NET client is on [NuGet Repo](https://www.nuget.org/packages/Hazelcast.Net/). Just add `hazelcast-client` as a dependency to your .NET project and you are good to go.

```
PM> Install-Package Hazelcast.Net
```

## 1.4. Basic Configuration

If you are using Hazelcast IMDG and .NET Client on the same computer, generally the default configuration should be fine. This is great for
trying out the client. However, if you run the client on a different computer than any of the cluster members, you may
need to do some simple configurations such as specifying the member addresses.

The Hazelcast IMDG members and clients have their own configuration options. You may need to reflect some of the member side configurations on the client side to properly connect to the cluster.

This section describes the most common configuration elements to get you started in no time.
It discusses some member side configuration options to ease the understanding of Hazelcast's ecosystem. Then, the client side configuration options
regarding the cluster connection are discussed. The configurations for the Hazelcast IMDG data structures that can be used in the .NET client are discussed in the following sections.

See the [Hazelcast IMDG Reference Manual](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html) and [Configuration Overview section](#3-configuration-overview) for more information.

### 1.4.1. Configuring Hazelcast IMDG

Hazelcast IMDG aims to run out-of-the-box for most common scenarios. However if you have limitations on your network such as multicast being disabled,
you may have to configure your Hazelcast IMDG members so that they can find each other on the network. Also, since most of the distributed data structures are configurable, you may want to configure them according to your needs. We will show you the basics about network configuration here.

You can use the following options to configure Hazelcast IMDG:

* Using the `hazelcast.xml` configuration file.
* Programmatically configuring the member before starting it from the Java code.

Since we use standalone servers, we will use the `hazelcast.xml` file to configure our cluster members.

When you download and unzip `hazelcast-<version>.zip` (or `tar`), you see the `hazelcast.xml` in the `bin` directory. When a Hazelcast member starts, it looks for the `hazelcast.xml` file to load the configuration from. A sample `hazelcast.xml` is shown below.

```xml
<hazelcast>
    <group>
        <name>dev</name>
        <password>dev-pass</password>
    </group>
    <network>
        <port auto-increment="true" port-count="100">5701</port>
        <join>
            <multicast enabled="true">
                <multicast-group>224.2.2.3</multicast-group>
                <multicast-port>54327</multicast-port>
            </multicast>
            <tcp-ip enabled="false">
                <interface>127.0.0.1</interface>
                <member-list>
                    <member>127.0.0.1</member>
                </member-list>
            </tcp-ip>
        </join>
        <ssl enabled="false"/>
    </network>
    <partition-group enabled="false"/>
    <map name="default">
        <backup-count>1</backup-count>
    </map>
</hazelcast>
```

We will go over some important configuration elements in the rest of this section.

- `<group>`: Specifies which cluster this member belongs to. A member connects only to the other members that are in the same group as
itself. As shown in the above configuration sample, there are `<name>` and `<password>` tags under the `<group>` element with some pre-configured values. You may give your clusters different names so that they can
live in the same network without disturbing each other. Note that the cluster name should be the same across all members and clients that belong
 to the same cluster. The `<password>` tag is not in use since Hazelcast 3.9. It is there for backward compatibility
purposes. You can remove or leave it as it is if you use Hazelcast 3.9 or later.
- `<network>`
    - `<port>`: Specifies the port number to be used by the member when it starts. Its default value is 5701. You can specify another port number, and if
     you set `auto-increment` to `true`, then Hazelcast will try the subsequent ports until it finds an available port or the `port-count` is reached.
    - `<join>`: Specifies the strategies to be used by the member to find other cluster members. Choose which strategy you want to
    use by setting its `enabled` attribute to `true` and the others to `false`.
        - `<multicast>`: Members find each other by sending multicast requests to the specified address and port. It is very useful if IP addresses
        of the members are not static.
        - `<tcp>`: This strategy uses a pre-configured list of known members to find an already existing cluster. It is enough for a member to
        find only one cluster member to connect to the cluster. The rest of the member list is automatically retrieved from that member. We recommend
        putting multiple known member addresses there to avoid disconnectivity should one of the members in the list is unavailable at the time
        of connection.

These configuration elements are enough for most connection scenarios. Now we will move onto the configuration of the .NET client.

### 1.4.2. Configuring Hazelcast .NET Client

There are two ways to configure a Hazelcast .NET client:

* Programmatically
* Declaratively (XML)

This section describes some network configuration settings to cover common use cases in connecting the client to a cluster. See the [Configuration Overview section](#3-configuration-overview)
and the following sections for information about detailed network configurations and/or additional features of Hazelcast .NET client configuration.

An easy way to configure your Hazelcast .NET client is to create a `ClientConfig` object and set the appropriate options. Then you can
supply this object to your client at the startup. This is the programmatic configuration approach.

Another way to configure your client is to provide a `hazelcast-client.xml` file. This approach is similar to `hazelcast.xml` approach
in configuring the member. Note that Hazelcast .NET client shares the same XML configuration with Java client.

Once you added `Hazelcast.Net` library to your .NET project, you may follow any of programmatic or declarative configuration approaches.
We will provide both ways for each configuration option in this section. Pick one way and stick to it.

**Programmatic configuration**

You need to create a `ClientConfig` object and adjust its properties. Then you can pass this object to the client when starting it.

```c#
var cfg = new ClientConfig();
var client = HazelcastClient.NewHazelcastClient(cfg);
```

**Declarative configuration**

Hazelcast .NET client looks for a `hazelcast-client.xml` in the current working directory unless you provide a configuration object
at the startup. If you intend to configure your client using a configuration file, then place a `hazelcast-client.xml` in the directory
of your application's entry point.

If you prefer to keep your `hazelcast-client.xml` file somewhere else, you can profile the file path when you create the client.
```c#
var client = HazelcastClient.NewHazelcastClient("your_xml_configuration_file_path");
```

You can also override the environment variable `hazelcast.client.config` with the location of your config file. In this case,
the client uses the configuration file specified in the environment variable.

For the structure of `hazelcast-client.xml`, take a look at [hazelcast-client-full.xml](Hazelcast.Test/Resources/hazelcast-client-full.xml). You
can use only the relevant parts of the file in your `hazelcast-client.xml` and remove the rest. Default configuration is used for any
part that you do not explicitly set in `hazelcast-client.xml`.

---

If you run the Hazelcast IMDG members in a different server than the client, you most probably have configured the members' ports and cluster
names as explained in the previous section. If you did, then you need to make certain changes to the network settings of your client.

#### 1.4.2.1 Group Settings

You need to provide the group name of the cluster, if it is defined on the server side, to which you want the client to connect.

**Programmatic Configuration:**

```c#
var cfg = new ClientConfig();
cfg.GetGroupConfig().SetName("group name of your cluster").SetPassword("group password");
```

**Declarative Configuration:**

```xml
<group>
  <name>group name of you cluster</name>
  <password>group password</password>
</group>
```

> **NOTE: If you have a Hazelcast IMDG release older than 3.11, you need to provide also a group password along with the group name.**

#### 1.4.2.2 Network Settings

You need to provide the IP address and port of at least one member in your cluster so the client can find it.

**Programmatic Configuration:**

```c#
var cfg = new ClientConfig();
cfg.GetNetworkConfig().AddAddress("some-ip-address:port");
```

**Declarative Configuration:**

```xml
<network>
  <cluster-members>
    <address>127.0.0.1</address>
    <address>127.0.0.2</address>
  </cluster-members>
</network>  
```

## 1.5. Basic Usage

Now that we have a working cluster and we know how to configure both our cluster and client, we can run a simple program to use a
distributed map in the .NET client.

The following example first creates a programmatic configuration object. Then, it starts a client.

```c#
var cfg = new ClientConfig();
// We create a config for illustrative purposes.
// We do not adjust this config. Therefore it has default settings.

var client = HazelcastClient.NewHazelcastClient(cfg);
Console.WriteLine("Local address : {0}", Client.GetLocalEndpoint().GetSocketAddress());
```

Congratulations! You just started a Hazelcast .NET client.

**Using a Map**

Let's manipulate a distributed map on a cluster using the client.

**Employees.cs**
```c#
var client = HazelcastClient.NewHazelcastClient();

var personnelMap = client.GetMap("personnelMap");
personnelMap.Put("Alice", "IT");
personnelMap.Put("Bob", "IT");
personnelMap.Put("Clark", "IT");
Console.WriteLine("Added IT personnel. Logging all known personnel");
foreach(var entry in personnelMap.entrySet())
{
    Console.WriteLine("{0} is in {1} department", entry.Key, entry.Value );
}
client.Shutdown();
```
**Output**
```
Added IT personnel. Logging all known personnel
Alice is in IT department
Clark is in IT department
Bob is in IT department
```

You see this example puts all IT personnel into a cluster-wide `personnelMap` and then prints all known personnel.

**Sales.cs**
```c#
var client = HazelcastClient.NewHazelcastClient();

var personnelMap = client.GetMap("personnelMap");
personnelMap.Put("Denise", "Sales");
personnelMap.Put("Erwing", "Sales");
personnelMap.Put("Faith", "Sales");
Console.WriteLine("Added Sales personnel. Logging all known personnel");
foreach(var entry in personnelMap.entrySet())
{
    Console.WriteLine("{0} is in {1} department", entry.Key, entry.Value );
}
client.Shutdown();
```
**Output**
```
Added Sales personnel. Logging all known personnel
Denise is in Sales department
Erwing is in Sales department
Faith is in Sales department
Alice is in IT department
Clark is in IT department
Bob is in IT department

```

You will see this time we add only the sales employees but we get the list all known employees including the ones in IT.
That is because our map lives in the cluster and no matter which client we use, we can access the whole map.

## 1.6. Code Samples

See the Hazelcast .NET [code samples](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/Hazelcast.Examples) for more examples.

# 2. Features

Hazelcast .NET client supports the following data structures and features:

* Map
* Queue
* Set
* List
* MultiMap
* Replicated Map
* Ringbuffer
* Lock
* Semaphore
* Atomic Long
* Atomic Reference
* Id Generator
* CRDT PN Counter
* CountDownLatch
* Event Listeners
* Entry Processor
* Transactional Map, MultiMap, Queue, List and Set
* Query (Predicates)
* Paging Predicate
* Partition predicate
* Built-in Predicates
* Listener with Predicate
* Projections
* Fast Aggregations
* Near Cache Support
* Eventual Consistency Control
* Declarative Configuration (XML)
* Programmatic Configuration
* Fail Fast on Invalid Configuration
* SSL Support (requires Enterprise server)
* Authorization
* Smart Client
* Unisocket Client
* Lifecycle Service
* HeartBeat Monitor
* Hazelcast Cloud Discovery
* IdentifiedDataSerializable Serialization
* Portable Serialization
* Custom Serialization
* JSON Serialization
* Global Serialization

# 3. Configuration Overview

This chapter describes the options to configure your .NET client.

## 3.1. Configuration Options

You can configure the Hazelcast .NET client declaratively (XML) or programmatically (API).

### 3.1.1. Programmatic Configuration

For programmatic configuration of the Hazelcast .NET client, just instantiate a `ClientConfig` object and configure the
desired aspects. An example is shown below.

```c#
var cfg = new ClientConfig();
cfg.GetNetworkConfig().AddAddress("127.0.0.1:5701");
var client = HazelcastClient.NewHazelcastClient(cfg);
```

See the `ClientConfig` class documentation at [Hazelcast .NET client API Docs](http://docs.hazelcast.org/docs/clients/net/current/) for details.

### 3.1.2. Declarative Configuration (XML)

If the client is not supplied with a programmatic configuration at the time of initialization, it will look for a configuration file named `hazelcast-client.xml`.
If this file exists, then the configuration is loaded from it. Otherwise, the client will start with the default configuration.
The following are the places that the client looks for a `hazelcast-client.xml` in the given order:

1. API configuration: The client first looks for configuration file path provided to the API call `HazelcastClient.NewHazelcastClient(xml_cfg)`
2. Environment variable: The client looks for the environment variable `hazelcast.client.config`. If it exists, the client looks for the configuration file in the specified location.
3. Current working directory: If there is no environment variable set, the client tries to load `hazelcast-client.xml` from the current working directory.
4. Default configuration: If all of the above methods fail, the client starts with the default configuration.


Following is a sample XML configuration file:

```xml
<hazelcast-client>

  <group>
    <name>dev</name>
    <password>dev-pass</password>
  </group>

  <network>
    <cluster-members>
      <address>127.0.0.1:5701</address>
      <address>127.0.0.2:5701</address>
    </cluster-members>
    <smart-routing>true</smart-routing>
    <redo-operation>true</redo-operation>
    <connection-timeout>60000</connection-timeout>
    <connection-attempt-period>3000</connection-attempt-period>
    <connection-attempt-limit>2</connection-attempt-limit>
  </network>

</hazelcast-client>
```

# 4. Serialization

Serialization is the process of converting an object into a stream of bytes to store the object in memory, a file or database, or transmit it through network.
Its main purpose is to save the state of an object in order to be able to recreate it when needed. The reverse process is called deserialization.
Hazelcast offers you its own native serialization methods. You will see these methods throughout the chapter.

**Default Types**

Hazelcast serializes all your objects before sending them to the server. The built-in primitive types are serialized natively and you cannot override this behavior.
The following table is the conversion of types for Java server side.

| .NET    | Java      |
|---------|-----------|
| bool    | Boolean   |
| byte    | Byte      |
| char    | Character |
| short   | Short     |
| int     | Integer   |
| long    | Long      |
| float   | Float     |
| double  | Double    |
| string  | String    |

and

|        .NET                |       Java              |
|----------------------------|-------------------------|
| DateTime                   | java.util.Date          |
| System.Numeric.BigInteger  | java.math.BigInteger    |


Arrays of the above types can be serialized as `boolean[]`, `byte[]`, `short[]`, `int[]`, `long[]`, `float[]`, `double[]`,  `char[]` and `string[]` for Java server side, respectively.

**Serialization Priority**

When Hazelcast .NET client serializes an object into IData:

1. It first checks whether the object is null.

2. If the above check fails, then Hazelcast checks if it is an instance of `Hazelcast.IO.Serialization.IIdentifiedDataSerializable`.

3. If the above check fails, then Hazelcast checks if it is an instance of `Hazelcast.IO.Serialization.IPortable`.

4. If the above check fails, then Hazelcast checks if it is an instance of one of the default types (see above default types).

5. If the above check fails, then Hazelcast looks for a user-specified Custom Serializer, i.e., an implementation of `IByteArraySerializer<T>` or `IStreamSerializer<T>`. Custom serializer is searched using the input Object’s Class and its parent class up to Object. If parent class search fails, all interfaces implemented by the class are also checked.

6. If the above check fails, then Hazelcast checks if it is Serializable ( `Type.IsSerializable` ) and a Global Serializer is not registered with CLR serialization Override feature.

7. If the above check fails, Hazelcast will use the registered Global Serializer if one exists.

## 4.1. IdentifiedDataSerializable Serialization

For a faster serialization of objects, Hazelcast recommends to implement the `IdentifiedDataSerializable` interface. The following is an example of an object implementing this interface:

```c#
public class Employee : IIdentifiedDataSerializable
{
    private const int ClassId = 100;

    public int Id { get; set; }
    public string Name { get; set; }

    public void ReadData(IObjectDataInput input)
    {
        Id = input.ReadInt();
        Name = input.ReadUTF();
    }

    public void WriteData(IObjectDataOutput output)
    {
        output.WriteInt(Id);
        output.WriteUTF(Name);
    }

    public int GetFactoryId()
    {
        return SampleDataSerializableFactory.FactoryId;
    }

    public int GetId()
    {
        return ClassId;
    }
}
```


IdentifiedDataSerializable uses `GetClassId()` and `GetFactoryId()` to reconstitute the object. To complete the implementation `IDataSerializableFactory` should also be implemented and registered into `SerializationConfig`. The factory's responsibility is to return an instance of the right `IIdentifiedDataSerializable` object, given the class id.

A sample `IDataSerializableFactory` could be implemented as following:

```c#
public class SampleDataSerializableFactory : IDataSerializableFactory
{
    public const int FactoryId = 1000;

    public IIdentifiedDataSerializable Create(int typeId)
    {
        if (typeId == 100) return new Employee();
        return null;
    }
}
```

The last step is to register the `IDataSerializableFactory` to the `SerializationConfig`.

**Programmatic Configuration:**
```c#
var clientConfig = new ClientConfig();
clientConfig.GetSerializationConfig()
    .AddDataSerializableFactory(SampleDataSerializableFactory.FactoryId,
    new Hazelcast.Examples.SampleDataSerializableFactory());
```

**Declarative Configuration:**

```xml
  <serialization>
    <data-serializable-factories>
      <data-serializable-factory factory-id="1000">Hazelcast.Examples.SampleDataSerializableFactory</data-serializable-factory>
    </data-serializable-factories>
  </serialization>
```

Note that the ID that is passed to the `SerializationConfig` is same as the `factoryId` that `Employee` object returns.

## 4.2. Portable Serialization

As an alternative to the existing serialization methods, Hazelcast offers portable serialization. To use it, you need to implement the `IPortable` interface. Portable serialization has the following advantages:

- Supporting multiversion of the same object type.
- Fetching individual fields without having to rely on the reflection.
- Querying and indexing support without deserialization and/or reflection.

In order to support these features, a serialized `IPortable` object contains meta information like the version and concrete location of the each field in the binary data. This way Hazelcast is able to navigate in the binary data and deserialize only the required field without actually deserializing the whole object which improves the query performance.

With multiversion support, you can have two members where each of them having different versions of the same object, and Hazelcast will store both meta information and use the correct one to serialize and deserialize portable objects depending on the member. This is very helpful when you are doing a rolling upgrade without shutting down the cluster.

Also note that portable serialization is totally language independent and is used as the binary protocol between Hazelcast server and clients.

A sample portable implementation of a `Customer` class looks like the following:

```c#
public class Customer : IPortable
{
    public const int ClassId = 1;

    public string Name { get; set; }
    public int Id { get; set; }
    public DateTime LastOrder { get; set; }

    public int GetFactoryId()
    {
        return SamplePortableFactory.FactoryId;
    }

    public int GetClassId()
    {
        return ClassId;
    }

    public void WritePortable(IPortableWriter writer)
    {
        writer.WriteInt("id", Id);
        writer.WriteUTF("name", Name);
        writer.WriteLong("lastOrder", LastOrder.ToFileTimeUtc());
    }

    public void ReadPortable(IPortableReader reader)
    {
        Id = reader.ReadInt("id");
        Name = reader.ReadUTF("name");
        LastOrder = DateTime.FromFileTimeUtc(reader.ReadLong("lastOrder"));
    }
}
```

Similar to `IIdentifiedDataSerializable`, a Portable object must provide `classId` and `factoryId`. The factory object will be used to create the Portable object given the classId.

A sample `IPortableFactory` could be implemented as following:

```c#
public class SamplePortableFactory : IPortableFactory
{
    public const int FactoryId = 1;

    public IPortable Create(int classId)
    {
        if (classId == Customer.ClassId)  return new Customer();
        return null;
    }
}
```

The last step is to register the `IPortableFactory` to the `SerializationConfig`.

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetSerializationConfig()
    .AddPortableFactory(SamplePortableFactory.FactoryId,
    new Hazelcast.Examples.SamplePortableFactory());
```

**Declarative Configuration:**

```xml
<serialization>
    <portable-factories>
      <portable-factory factory-id="1">Hazelcast.Examples.SamplePortableFactory</portable-factory>
    </portable-factories>
</serialization>

```

Note that the ID that is passed to the `SerializationConfig` is same as the `factoryId` that `Customer` object returns.

## 4.3. Custom Serialization

Hazelcast lets you plug a custom serializer to be used for serialization of objects.

Let's say you have a class `CustomSerializableType` and you would like to customize the serialization, since you may want to use an external serializer for only one class.

```c#
public class CustomSerializableType
{
    public string Value { get; set; }
}
```

Let's say your custom `CustomSerializer` will serialize `CustomSerializableType`.

```c#
public class CustomSerializer : IStreamSerializer<CustomSerializableType>
{
    public int GetTypeId()
    {
        return 10;
    }

    public void Destroy()
    {
    }

    public void Write(IObjectDataOutput output, CustomSerializableType t)
    {
        var array = Encoding.UTF8.GetBytes(t.Value);
        output.WriteInt(array.Length);
        output.Write(array);
    }

    public CustomSerializableType Read(IObjectDataInput input)
    {
        var len = input.ReadInt();
        var array = new byte[len];
        input.ReadFully(array);
        return new CustomSerializableType {Value = Encoding.UTF8.GetString(array)};
    }
}
```

Note that the serializer `id` must be unique as Hazelcast will use it to lookup the `CustomSerializer` while it deserializes the object.
Now the last required step is to register the `CustomSerializer` to the configuration.

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetSerializationConfig()
    .AddSerializerConfig(new SerializerConfig()
        .SetImplementation(new CustomSerializer())
        .SetTypeClass(typeof(CustomSerializableType)));
```

**Declarative Configuration:**

```xml
<serialization>
    <serializers>
      <serializer type-class="Hazelcast.Examples.CustomSerializableType">Hazelcast.Examples.CustomSerializer</serializer>
    </serializers>
</serialization>

```

From now on, Hazelcast will use `CustomSerializer` to serialize `CustomSerializableType` objects.

## 4.4. JSON Serialization

You can use the JSON formatted strings as objects in Hazelcast cluster. Starting with Hazelcast IMDG 3.12, the JSON serialization is one of the formerly supported serialization methods. Creating JSON objects in the cluster does not require any server side coding and hence you can just send a JSON formatted string object to the cluster and query these objects by fields.

In order to use JSON serialization, you should use the `HazelcastJsonValue` object for the key or value. Here is an example IMap usage:

```c#
    var config = new ClientConfig();
    var client = HazelcastClient.NewHazelcastClient(config);
    var map = client.GetMap<string, HazelcastJsonValue>("map");
```

We constructed a map in the cluster which has `string` as the key and `HazelcastJsonValue` as the value. `HazelcastJsonValue` is a simple wrapper and identifier for the JSON formatted strings. You can get the JSON string from the `HazelcastJsonValue` object by using the `ToString()` method. 

You can construct a `HazelcastJsonValue` using `HazelcastJsonValue(string jsonString)` constructor. In case `json` parameter is null it will throw `NullReferenceException` exception. No JSON parsing is performed but it is your responsibility to provide correctly formatted JSON strings. The client will not validate the string, and it will send it to the cluster as it is. If you submit incorrectly formatted JSON strings and, later, if you query those objects, it is highly possible that you will get formatting errors since the server will fail to deserialize or find the query fields.

Here is an example of how you can construct a `HazelcastJsonValue` and put to the map:

```c#
    map.Put("item1", new HazelcastJsonValue("{ \"age\": 4 }"));
    map.Put("item2", new HazelcastJsonValue("{ \"age\": 20 }"));
```

You can query JSON objects in the cluster using the `Predicate`s of your choice. An example JSON query for querying the values whose age is less than 6 is shown below:

```c#
    // Get the objects whose age is less than 6
    var result = map.Values(Predicates.IsLessThan("age", 6));

    Console.WriteLine("Retrieved " + result.Count + " values whose age is less than 6.");
    Console.WriteLine("Entry is: " + result.First().ToString());
```

## 4.5. Global Serialization

The global serializer is identical to custom serializers from the implementation perspective.
The global serializer is registered as a fallback serializer to handle all other objects if a serializer cannot be located for them.

By default, the global serializer does not handle .NET Serializable instances. However, you can configure it to be responsible for those instances.

A custom serializer should be registered for a specific class type. The global serializer will handle all class types if all the
steps in searching for a serializer fail as described in Serialization Interface Types.

**Use cases**

- Third party serialization frameworks can be integrated using the global serializer.

- For your custom objects, you can implement a single serializer to handle all of them.

A sample global serializer that integrates with a third party serializer is shown below.

```c#
public class GlobalSerializer : IStreamSerializer<object>
{
    public int GetTypeId()
    {
        return 20;
    }

    public void Destroy()
    {
    }

    public void Write(IObjectDataOutput output, object obj)
    {
        out.write(MyFavoriteSerializer.serialize(obj))
    }

    public object Read(IObjectDataInput input)
    {
        return MyFavoriteSerializer.deserialize(input);
    }
}
```

You should register the global serializer in the configuration.

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetSerializationConfig().SetGlobalSerializerConfig(
    new GlobalSerializerConfig().SetImplementation(new GlobalSerializer())
```

**Declarative Configuration:**

```xml
<serialization>
    <serializers>
      <global-serializer>Hazelcast.Examples.GlobalSerializer</global-serializer>
    </serializers>
</serialization>
```

# 5. Setting Up Client Network

All network related configuration of Hazelcast .NET client is performed via the `network` element in the declarative configuration file,
or in the object `ClientNetworkConfig` when using programmatic configuration. Let's first give the examples for these two approaches. Then we will look at its sub-elements and attributes.

**Declarative Configuration**

Here is an example of configuring network for .NET Client declaratively.

```xml
  <network>
    <cluster-members>
      <address>127.0.0.1</address>
      <address>127.0.0.2</address>
    </cluster-members>
    <smart-routing>true</smart-routing>
    <redo-operation>true</redo-operation>
    <connection-timeout>6000</connection-timeout>
    <connection-attempt-period>5000</connection-attempt-period>
    <connection-attempt-limit>5</connection-attempt-limit>
  </network>
```

**Programmatic Configuration**

Here is an example of configuring network for .NET Client programmatically.

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig()
    .AddAddress('10.1.1.21', '10.1.1.22:5703')
    .SetSmartRouting(true)
    .SetRedoOperation(true)
    .SetConnectionTimeout(6000)
    .SetConnectionAttemptPeriod(5000)
    .SetConnectionAttemptLimit(5)
```

## 5.1. Providing Member Addresses

Address list is the initial list of cluster addresses which the client will connect to. The client uses this
list to find an alive member. Although it may be enough to give only one address of a member in the cluster
(since all members communicate with each other), it is recommended that you give the addresses for all the members.

**Declarative Configuration:**

```xml
<network>
  <cluster-members>
    <address>127.0.0.1</address>
    <address>127.0.0.2</address>
  </cluster-members>
</network>  
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig()
    .AddAddress('10.1.1.21', '10.1.1.22:5703');
```

If the port part is omitted, then 5701, 5702 and 5703 will be tried in a random order.

You can specify multiple addresses with or without the port information as seen above. The provided list is shuffled and tried in a random order. Its default value is `localhost`.

## 5.2. Setting Smart Routing

Smart routing defines whether the client mode is smart or unisocket. See the [.NET Client Operation Modes section](#72-net-client-operation-modes)
for the description of smart and unisocket modes.

The following are example configurations.

**Declarative Configuration:**

```xml
<network>
    <smart-routing>true</smart-routing>
</network>  

```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().SetSmartRouting(true);
```

Its default value is `true` (smart client mode).

## 5.3. Enabling Redo Operation

It enables/disables redo-able operations. While sending the requests to the related members, the operations can fail due to various reasons. Read-only operations are retried by default. If you want to enable retry for the other operations, you can set the `redoOperation` to `true`.

**Declarative Configuration:**

```xml
<network>
    <redo-operation>true</redo-operation>
</network>  
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().SetRedoOperation(true);
```

Its default value is `false` (disabled).

## 5.4. Setting Connection Timeout

Connection timeout is the timeout value in milliseconds for the members to accept the client connection requests.
If the member does not respond within the timeout, the client will retry to connect as many as `ClientNetworkConfig.GetConnectionAttemptPeriod()` times.

The following are the example configurations.


**Declarative Configuration:**

```xml
<network>
    <connection-timeout>6000</connection-timeout>
</network>
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().SetConnectionTimeout(6000);
```

Its default value is `5000` milliseconds.

## 5.5. Setting Connection Attempt Limit

While the client is trying to connect initially to one of the members in the `ClientNetworkConfig.addresses`, that member might not be available at that moment.
Instead of giving up, throwing an error and stopping the client, the client will retry as many as `ClientNetworkConfig.GetConnectionAttemptLimit()` times.
This is also the case when the previously established connection between the client and that member goes down.

The following are example configurations.

**Declarative Configuration:**

```xml
<network>
    <connection-attempt-limit>5</connection-attempt-limit>
</network>
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().SetConnectionAttemptLimit(5);
```

Its default value is `2`.

## 5.6. Setting Connection Attempt Period

Connection attempt period is the duration in milliseconds between the connection attempts defined by `ClientNetworkConfig.GetConnectionAttemptLimit()`.

The following are example configurations.

**Declarative Configuration:**

```xml
<network>
    <connection-attempt-period>5000</connection-attempt-period>
</network>
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetNetworkConfig().SetConnectionAttemptPeriod(5000);
```

Its default value is `3000` milliseconds.

## 5.7. Enabling Client TLS/SSL

You can use TLS/SSL to secure the connection between the clients and members. If you want to enable TLS/SSL
for the client-cluster connection, you should set an SSL configuration. Please see [TLS/SSL section](#61-tlsssl).

## 5.8. Enabling Hazelcast Cloud Discovery

The purpose of Hazelcast Cloud Discovery is to provide the clients to use IP addresses provided by `hazelcast orchestrator`.
To enable Hazelcast Cloud Discovery, specify a token for the `discoveryToken` field and set the `enabled` field to `true`.

The following are example configurations.

**Declarative Configuration:**

```xml
<hazelcast-client>
  <group>
    <name>hazel</name>
    <password>cast</password>
  </group>
  <network>
    <hazelcast-cloud enabled="true">
        <discovery-token>EXAMPLE_TOKEN</discovery-token>
    </hazelcast-cloud>
  </network>
</hazelcast-client>
```

**Programmatic Configuration:**

```c#
var clientConfig = new ClientConfig();
clientConfig.GetGroupConfig()
    .SetName("hazel")
    .SetPassword("cast");

clientConfig.GetNetworkConfig().GetCloudConfig()
    .SetEnabled(true)
    .SetDiscoveryToken("EXAMPLE_TOKEN");
```

To be able to connect to the provided IP addresses, you should use secure TLS/SSL connection between the client and members.
Therefore, you should set an SSL configuration as described in the previous section.

# 6. Securing Client Connection

This chapter describes the security features of Hazelcast .NET client. These include using TLS/SSL for connections between members and between clients and members, and mutual authentication. These security features require **Hazelcast IMDG Enterprise** edition.

### 6.1. TLS/SSL

One of the offers of Hazelcast is the TLS/SSL protocol which you can use to establish an encrypted communication across your cluster with key stores and trust stores.

* A Java `keyStore` is a file that includes a private key and a public certificate.
* A Java `trustStore` is a file that includes a list of certificates trusted by your application which is named as  "certificate authority". 

You should set `keyStore` and `trustStore` before starting the members. See the next section on setting `keyStore` and `trustStore` on the server side.

#### 6.1.1. TLS/SSL for Hazelcast Members

Hazelcast allows you to encrypt socket level communication between Hazelcast members and between Hazelcast clients and members, for end to end encryption. To use it, see the [TLS/SSL for Hazelcast Members section](http://docs.hazelcast.org/docs/latest/manual/html-single/index.html#tls-ssl-for-hazelcast-members).

#### 6.1.2. TLS/SSL for Hazelcast .NET Clients

TLS/SSL for the Hazelcast .NET client can be configured using the `SSLConfig` class. Let's first give an example of a sample configuration and then go over the configuration options one by one:

```c#
    var clientConfig = new ClientConfig();
    var sslConfig = clientConfig.GetNetworkConfig().GetSSLConfig();
    sslConfig.SetEnabled(true);
    sslConfig.SetProperty(SSLConfig.ValidateCertificateChain, "true");
    sslConfig.SetProperty(SSLConfig.CheckCertificateRevocation, "false");
    sslConfig.SetProperty(SSLConfig.ValidateCertificateName, "false");
    sslConfig.SetProperty(SSLConfig.CertificateName, "CN or SAN of server certificate");
    sslConfig.SetProperty(SSLConfig.CertificateFilePath, "client pfx file path");
    sslConfig.SetProperty(SSLConfig.CertificatePassword, "client pfx password");
    sslConfig.SetProperty(SSLConfig.SslProtocol, "tls");
```

##### Enabling TLS/SSL

TLS/SSL for the Hazelcast .NET client can be enabled/disabled using the `enabled` option. When this option is set to `true`, TLS/SSL will be configured with respect to the other `SSLConfig` options. 
Setting this option to `false` will result in discarding the other `SSLConfig` options.

The following is an example configuration:

```c#
clientConfig.GetNetworkConfig().GetSSLConfig().SetEnabled(true)
```

Default value is `false` (disabled). 

##### Certificate Chain validation

Remote SSL certificate chain validation can be enabled/disabled using the `SSLConfig.ValidateCertificateChain` option. It is enabled by default. If you need to bypass certificate validation for some reason, you can disable it as follows:  

```c#
sslConfig.SetProperty(SSLConfig.ValidateCertificateChain, "false");
```

Validation is done by .NET and delegated to OS, and you need to make sure your server certificate is trusted by your OS.
Please refer to [this blog](https://blogs.msdn.microsoft.com/webdev/2017/11/29/configuring-https-in-asp-net-core-across-different-platforms/) for information on how to configure your OS to trust your server certificates.

##### Certificate Name Validation

Server certificate CN or SAN field can be validated against a value you set into configuration. This option is disabled by default.

An example usage is shown below:

```c#
sslConfig.SetProperty(SSLConfig.ValidateCertificateName, "true");
sslConfig.SetProperty(SSLConfig.CertificateName, "hazelcast.org");
```

##### TLS/SSL Protocol

You can configure the TLS/SSL protocol using the `SSLConfig.SslProtocol` option. Valid options are string values of `System.Security.Authentication.SslProtocols` Enum. 
Depending on your .Net Framework/Net core version, below values are valid:

* **None**    : Allows the operating system to choose the best protocol to use. 
* **Ssl2**    : SSL 2.0 Protocol. *RFC 6176 prohibits the usage of SSL 2.0.* 
* **Ssl3**    : SSL 3.0 Protocol. *RFC 7568 prohibits the usage of SSL 3.0.*
* **Tls**     : TLS 1.0 Protocol described in RFC 2246.
* **Tls11**   : TLS 1.1 Protocol described in RFC 4346. (.Net Framework 4.5+)
* **Tls12**   : TLS 1.2 Protocol described in RFC 5246. (.Net Framework 4.5+)


#### 6.1.3. Mutual Authentication

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

You can see the details of setting mutual authentication on the server side in the [Mutual Authentication section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#mutual-authentication) of the Hazelcast IMDG Reference Manual.

On the client side, you have to provide the client certificate and its password if there is one. Here is how you do it:

```c#
sslConfig.SetProperty(SSLConfig.CertificateFilePath, "client pfx file path");
sslConfig.SetProperty(SSLConfig.CertificatePassword, "client pfx password");
```

The provided certificate file should be a PFX file that has private and public keys. The file path should be set with `SSLConfig.CertificateFilePath`.
If you choose to set a password to it, you need to provide it to the configuration using the `SSLConfig.CertificatePassword` option.


# 7. Using .NET Client with Hazelcast IMDG

This chapter provides information on how you can use Hazelcast IMDG's data structures in the .NET client, after giving some basic
information including an overview to the client API, operation modes of the client and how it handles the failures.

## 7.1. .NET Client API Overview

Most of the .NET API are synchronous methods. The failures are communicated via exceptions. There are also asynchronous versions of some of the API. The asynchronous API uses the Task<T> result.

If you are ready to go, let's start to use Hazelcast .NET client.

The first step is the configuration. You can configure the .NET client declaratively or programmatically. We will use the programmatic approach throughout this chapter.
See the [Programmatic Configuration section](#311-programmatic-configuration) for details.

The following is an example on how to create a `ClientConfig` object and configure it programmatically:

```c#
    var clientConfig = new ClientConfig();
    clientConfig.GetGroupConfig().setName("dev");
    clientConfig.GetNetworkConfig().AddAddress("10.90.0.2:5701", "10.90.0.2:5702");
```

The second step is initializing the `HazelcastClient` to be connected to the cluster:

```c#
var client = HazelcastClient.NewHazelcastClient(clientConfig);
// some operation
```

**This client object is your gateway to access all the Hazelcast distributed objects.**

Let's create a map and populate it with some data, as shown below.

```c#
    // Get the Distributed Map from Cluster.
    var map = client.GetMap<string, string>("my-distributed-map");
    //Standard Put and Get.
    map.Put("key", "value");
    map.Get("key");
    //Concurrent Map methods, optimistic updating
    map.PutIfAbsent("somekey", "somevalue");
    map.Replace("key", "value", "newvalue");
 ```

As the final step, if you are done with your client, you can shut it down as shown below. This will release all the used resources and close connections to the cluster.

```c#
    // Shutdown this Hazelcast Client
    client.shutdown();
```

## 7.2. .NET Client Operation Modes

The client has two operation modes because of the distributed nature of the data and cluster: smart and unisocket.

### 7.2.1. Smart Client

In the smart mode, the clients connect to each cluster member. Since each data partition uses the well known and consistent hashing algorithm,
each client can send an operation to the relevant cluster member, which increases the overall throughput and efficiency. Smart mode is the default mode.

### 7.2.2. Unisocket Client

For some cases, the clients can be required to connect to a single member instead of each member in the cluster. Firewalls, security or some custom networking issues can be the reason for these cases.

In the unisocket client mode, the client will only connect to one of the configured addresses. This single member will behave as a gateway to the other members.
For any operation requested from the client, it will redirect the request to the relevant member and return the response back to the client returned from this member.

## 7.3. Handling Failures

There are two main failure cases you should be aware of. Below sections explain these and the configurations you can perform to achieve proper behavior.

### 7.3.1. Handling Client Connection Failure

While the client is trying to connect initially to one of the members in the `ClientNetworkConfig.GetAddresses()`, all the members might not be available.
Instead of giving up, throwing an error and stopping the client, the client will retry as many as `connectionAttemptLimit` times.

You can configure `connectionAttemptLimit` for the number of times you want the client to retry connecting. See the [Setting Connection Attempt Limit section](#55-setting-connection-attempt-limit).

The client executes each operation through the already established connection to the cluster. If this connection(s) disconnects or drops, the client will try to reconnect as configured.

### 7.3.2. Handling Retry-able Operation Failure

While sending the requests to the related members, the operations can fail due to various reasons. Read-only operations are retried by default.
If you want to enable retrying for the other operations, you can set the `redoOperation` to `true`. See the [Enabling Redo Operation section](#53-enabling-redo-operation).

You can set a timeout for retrying the operations sent to a member. This can be provided by using the environment variable `hazelcast.client.invocation.timeout.seconds`.
The client will retry an operation within this given period, of course, if it is a read-only operation or you enabled the `redoOperation` as stated in the above paragraph.
This timeout value is important when there is a failure resulted by either of the following causes:

* Member throws an exception.
* Connection between the client and member is closed.
* Client’s heartbeat requests are timed out.

When a connection problem occurs, an operation is retried if it is certain that it has not run on the member yet or if it is idempotent
such as a read-only operation, i.e., retrying does not have a side effect. If it is not certain whether the operation has run on the member,
then the non-idempotent operations are not retried. However, as explained in the first paragraph of this section, you can force all
the client operations to be retried (`redoOperation`) when there is a connection failure between the client and member.
But in this case, you should know that some operations may run multiple times causing conflicts. For example, assume that your client
sent a `queue.offer` operation to the member and then the connection is lost. Since there will be no response for this operation,
you will not know whether it has run on the member or not. If you enabled `redoOperation`, it means this operation may run again,
which may cause two instances of the same object in the queue.


## 7.4. Using Distributed Data Structures

Most of the distributed data structures are supported by the .NET client. In this chapter, you will learn how to use these distributed data structures.

### 7.4.1. Using Map

Hazelcast Map (`IMap`) is a distributed map. Through the .NET client, you can perform operations like reading and writing from/to a
Hazelcast Map with the well known get and put methods. For details, see the [Map section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#map) in the Hazelcast IMDG Reference Manual.

A Map usage example is shown below.

```c#
    // Get the Distributed Map from Cluster.
    var map = hz.GetMap<string, string>("my-distributed-map");
    //Standard Put and Get.
    map.Put("key", "value");
    map.Get("key");
    //Concurrent Map methods, optimistic updating
    map.PutIfAbsent("somekey", "somevalue");
    map.Replace("key", "value", "newvalue");
```
### 7.4.2. Using MultiMap

Hazelcast `MultiMap` is a distributed and specialized map where you can store multiple values under a single key. For details,
see the [MultiMap section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#multimap) in the Hazelcast IMDG Reference Manual.

A MultiMap usage example is shown below.

```c#
    // Get the Distributed MultiMap from Cluster.
    var multiMap = hz.GetMultiMap<string, string>("my-distributed-multimap");
    // Put values in the map against the same key
    multiMap.Put("my-key", "value1");
    multiMap.Put("my-key", "value2");
    multiMap.Put("my-key", "value3");
    // Print out all the values for associated with key called "my-key"
    var values = multiMap.Get("my-key");
    foreach (var item in values)
    {
        Console.WriteLine(item);
    }

    // remove specific key/value pair
    multiMap.Remove("my-key", "value2");
```

### 7.4.3. Using Replicated Map

Hazelcast `ReplicatedMap` is a distributed key-value data structure where the data is replicated to all members in the cluster.
It provides full replication of entries to all members for high speed access. For details,
see the [Replicated Map section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#replicated-map) in the Hazelcast IMDG Reference Manual.

A Replicated Map usage example is shown below.

```c#
    // Get a Replicated Map called "my-replicated-map"
    var map = hz.GetReplicatedMap<string, string>("my-replicated-map");
    // Put and Get a value from the Replicated Map
    var replacedValue = map.Put("key", "value"); // key/value replicated to all members
    Console.WriteLine("replacedValue = " + replacedValue); // Will be null as its first update
    var value = map.Get("key"); // the value is retrieved from a random member in the cluster
    Console.WriteLine("value for key = " + value);
```

### 7.4.4. Using Queue

Hazelcast Queue (`IQueue`) is a distributed queue which enables all cluster members to interact with it. For details,
see the [Queue section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#queue) in the Hazelcast IMDG Reference Manual.

A Queue usage example is shown below.

```c#
    var queue = hz.GetQueue<string>("my-distributed-queue");
    // Offer a String into the Distributed Queue
    queue.Offer("item");
    // Poll the Distributed Queue and return the String
    queue.Poll();
    //Timed blocking Operations
    queue.Offer("anotheritem", 500, TimeUnit.Milliseconds);
    queue.Poll(5, TimeUnit.Seconds);
    //Indefinitely blocking Operations
    queue.Put("yetanotheritem");
    Console.WriteLine(queue.Take());
```

### 7.4.5. Using Set

Hazelcast Set (`ISet`) is a distributed set which does not allow duplicate elements. For details,
see the [Set section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#set) in the Hazelcast IMDG Reference Manual.

A Set usage example is shown below.

```c#
    var set = hz.GetSet<string>("my-distributed-set");
    // Add items to the set with duplicates
    set.Add("item1");
    set.Add("item1");
    set.Add("item2");
    set.Add("item2");
    set.Add("item2");
    set.Add("item3");
    // Get the items. Note that there are no duplicates.
    foreach (var item in set)
    {
        Console.WriteLine(item);
    }
```

### 7.4.6. Using List

Hazelcast List (`IList`) is a distributed list which allows duplicate elements and preserves the order of elements. For details,
see the [List section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#list) in the Hazelcast IMDG Reference Manual.

A List usage example is shown below.

```c#
    // Get the Distributed List from Cluster.
    var list = hz.GetList<string>("my-distributed-list");
    // Add elements to the list
    list.Add("item1");
    list.Add("item2");

    // Remove the first element
    Console.WriteLine("Removed: " + list.Remove(0));
    // There is only one element left
    Console.WriteLine("Current size is " + list.Size());
    // Clear the list
    list.Clear();
```

### 7.4.7. Using Ringbuffer

Hazelcast `Ringbuffer` is a replicated but not partitioned data structure that stores its data in a ring-like structure.
You can think of it as a circular array with a given capacity. Each Ringbuffer has a tail and a head. The tail is where the items
are added and the head is where the items are overwritten or expired. You can reach each element in a Ringbuffer using a sequence ID,
which is mapped to the elements between the head and tail (inclusive) of the Ringbuffer. For details,
see the [Ringbuffer section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#ringbuffer) in the Hazelcast IMDG Reference Manual.

A Ringbuffer usage example is shown below.

```c#
    var rb = hz.GetRingbuffer<long>("rb");

    // add two items into ring buffer
    rb.Add(100);
    rb.Add(200);

    // we start from the oldest item.
    // if you want to start from the next item, call rb.tailSequence()+1
    var sequence = rb.HeadSequence();
    Console.WriteLine(rb.ReadOne(sequence));
    sequence += 1;
    Console.WriteLine(rb.ReadOne(sequence));
```

### 7.4.8 Using Lock

Hazelcast Lock (`ILock`) is a distributed lock implementation. You can synchronize Hazelcast members and clients using a Lock.
For details, see the [Lock section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#lock) in the Hazelcast IMDG Reference Manual.

A Lock usage example is shown below.

```c#
    // Get a distributed lock called "my-distributed-lock"
    var lck = hz.GetLock("my-distributed-lock");
    // Now create a lock and execute some guarded code.
    lck.Lock();
    try
    {
        //do something here
    }
    finally
    {
        lck.Unlock();
    }
```

### 7.4.9 Using Atomic Long

Hazelcast Atomic Long (`IAtomicLong`) is the distributed long which offers most of the operations such as `get`, `set`, `getAndSet`, `compareAndSet` and `incrementAndGet`.
For details, see the [Atomic Long section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#iatomiclong) in the Hazelcast IMDG Reference Manual.

An Atomic Long usage example is shown below.

```c#
    // Get an Atomic Counter, we'll call it "counter"
    var counter = hz.GetAtomicLong("counter");
    // Add and Get the "counter"
    counter.AddAndGet(3); // value is now 3
    // Display the "counter" value
    Console.WriteLine("counter: " + counter.Get());
```

### 7.4.10 Using Semaphore

Hazelcast Semaphore (`ISemaphore`) is a distributed semaphore implementation. For details, see the [Semaphore section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#isemaphore) in the Hazelcast IMDG Reference Manual.

A Semaphore usage example is shown below.

```c#
    var semaphore = hz.GetISemaphore("semaphore");
    semaphore.Init(10);
    semaphore.Acquire(5);
    Console.WriteLine( "Number of available permits: " + semaphore.availablePermits());
```

## 7.5. Distributed Events

This chapter explains when various events are fired and describes how you can add event listeners on a Hazelcast .NET client. These events can be categorized as cluster and distributed data structure events.

### 7.5.1. Cluster Events

You can add event listeners to a Hazelcast .NET client. You can configure the following listeners to listen to the events on the client side:

* Membership Listener: Notifies when a member joins to/leaves the cluster, or when an attribute is changed in a member.
* Distributed Object Listener: Notifies when a distributed object is created or destroyed throughout the cluster.
* Lifecycle Listener: Notifies when the client is starting, started, shutting down and shutdown.

#### 7.5.1.1. Listening for Member Events

You can register the following types of member events.

* `memberAdded`: A new member is added to the cluster.
* `memberRemoved`: An existing member leaves the cluster.
* `memberAttributeChanged`: An attribute of a member is changed. See the [Defining Member Attributes section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#defining-member-attributes) in the Hazelcast IMDG Reference Manual to learn about member attributes.

You can use the `ICluster` ( `HazelcastClient.GetCluster()` ) interface to register for the membership listeners. There are two types of listeners: `IInitialMembershipListener` and `IMembershipListener`.
The difference is that `IInitialMembershipListener` also gets notified when the client connects to the cluster and retrieves the whole membership list.
You need to implement one of these two interfaces and register an instance of the listener to the cluster.

The following example demonstrates both initial and regular membership listener registrations.

```c#
public class MyInitialMemberListener : IInitialMembershipListener
{
    public void Init(InitialMembershipEvent membershipEvent)
    {
        var members = membershipEvent.GetMembers();
        Console.WriteLine("The following are the initial members in the cluster:");
        foreach (var member in members)
        {
            Console.WriteLine(member);
        }
    }

    public void MemberAdded(MembershipEvent membershipEvent)
    {
        Console.WriteLine("[MyInitialMemberListener.MemberAdded] New member joined:");
        Console.Write(membershipEvent.GetMember());
    }

    public void MemberRemoved(MembershipEvent membershipEvent)
    {
        Console.WriteLine("[MyInitialMemberListener.MemberRemoved] Member left:");
        Console.Write(membershipEvent.GetMember());
    }

    public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
    {
        Console.WriteLine("[MyInitialMemberListener.MemberAttributeChanged] Member attribute: {0} changed. Value: {1}  for member: {2}",
            memberAttributeEvent.GetKey(), memberAttributeEvent.GetValue(), memberAttributeEvent.GetMember());
    }
}


public class MyMemberListener : IMembershipListener
{
    public void MemberAdded(MembershipEvent membershipEvent)
    {
        Console.WriteLine("[MyMemberListener.MemberAdded] New member joined:");
        Console.Write(membershipEvent.GetMember());
    }

    public void MemberRemoved(MembershipEvent membershipEvent)
    {
        Console.WriteLine("[MyMemberListener.MemberRemoved] Member left:");
        Console.Write(membershipEvent.GetMember());
    }

    public void MemberAttributeChanged(MemberAttributeEvent memberAttributeEvent)
    {
        Console.WriteLine("[MyMemberListener.MemberAttributeChanged] Member attribute: {0} changed. Value: {1}  for member: {2}",
            memberAttributeEvent.GetKey(), memberAttributeEvent.GetValue(), memberAttributeEvent.GetMember());
    }
}
```

You can register these listener implementations using the following code.

```c#
//register the listeners
var hz = HazelcastClient.NewHazelcastClient(config);

hz.GetCluster().AddMembershipListener(new MyMemberListener());
hz.GetCluster().AddMembershipListener(new MyInitialMemberListener());
```

The `memberAttributeChanged` has its own type of event named as `MemberAttributeEvent`. When there is an attribute change on the member, this event is fired.


#### 7.5.1.2. Listening for Distributed Object Events

`IDistributedObjectListener` interface implementations can be registered for listening distributed object events.
The events for distributed objects are invoked when they are created or destroyed in the cluster.
The `DistributedObjectEvent` parameter of the listener interface methods has following getters:

* `GetServiceName()`: Service name of the distributed object.
* `GetObjectName()`: Name of the distributed object.
* `GetEventType()`: Type of the invoked event. It can be `created` or `destroyed`.

The following is an example of adding a `IDistributedObjectListener`.

```c#
public class MyDistributedObjectListener : IDistributedObjectListener
{
    public void DistributedObjectCreated(DistributedObjectEvent e)
    {
        Console.WriteLine("[CREATED] EventType: {0}, ObjectName: {1}, ServiceName: {2}", e.GetEventType(), e.GetObjectName(), e.GetServiceName() );
    }

    public void DistributedObjectDestroyed(DistributedObjectEvent e)
    {
        Console.WriteLine("[DESTROYED] EventType: {0}, ObjectName: {1}, ServiceName: {2}", e.GetEventType(), e.GetObjectName(), e.GetServiceName() );
    }
}

//register the listener
client.AddDistributedObjectListener(new MyDistributedObjectListener());
```

#### 7.5.1.3. Listening for Lifecycle Events

The `LifecycleListener` interface notifies for the following events:

* `Starting`: The client is starting.
* `Started`: The client has started.
* `ShuttingDown`: The client is shutting down.
* `Shutdown`: The client’s shutdown has completed.
* `ClientConnected`: The client is connected to cluster
* `ClientDisconnected`: The client is disconnected from cluster

The following is an example of the `LifecycleListener` that is added to the `ClientConfig`.

```c#
public class MyLifecycleListener : ILifecycleListener
{
    public void StateChanged(LifecycleEvent lifecycleEvent)
    {
        Console.WriteLine(lifecycleEvent);
    }
}

var clientConfig = new ClientConfig();
clientConfig.AddListenerConfig(new ListenerConfig(new MyLifecycleListener()));
```

You can alternatively add it to the lifecycle service as follows:

```c#
var registrationID = client.GetLifecycleService().AddLifecycleListener(new MyLifecycleListener());

//....

//Unregister it when you want to stop listening
client.GetLifecycleService().RemoveLifecycleListener(registrationID);
```

### 7.5.2. Distributed Data Structure Events

You can add event listeners to the distributed data structures.

#### 7.5.2.1. Listening for Map Events

You can listen to map-wide or entry-based events using the listeners provided by the Hazelcast’s eventing framework. To listen to these events, implement a `MapListener` sub-interface.


See the following example.

```c#
public class MyEntryAddedListener<K, V> : EntryAddedListener<K, V>
{
    public void EntryAdded(EntryEvent<K, V> entryEvent)
    {
        Console.WriteLine(entryEvent);
    }
}

//Add listener
map.AddEntryListener(new MyEntryAddedListener<string, string>());
```

A map-wide event is fired as a result of a map-wide operation. For example, `IMap.Clear()` or `IMap.EvictAll()`. An entry-based event is fired after the operations that affect a specific entry. For example, `IMap.remove()` or `IMap.evict()`.

See the following example.

```c#
public class MyMapClearedListener : MapClearedListener
{
    public void MapCleared(MapEvent mapEvent)
    {
        Console.WriteLine(mapEvent);
    }
}

//Add listener
map.AddEntryListener(new MyMapClearedListener());
```

## 7.6. Distributed Computing

This chapter explains how you can use Hazelcast IMDG's entry processor implementation in the .NET client.

### 7.6.1. Using EntryProcessor

Hazelcast supports entry processing. An entry processor is a function that executes your code on a map entry in an atomic way.

An entry processor is a good option if you perform bulk processing on an `IMap`. Usually you perform a loop of keys -- executing `IMap.Get(key)`, mutating the value and finally putting the entry back in the map using `IMap.Put(key,value)`. If you perform this process from a client or from a member where the keys do not exist, you effectively perform two network hops for each update: the first to retrieve the data and the second to update the mutated value.

If you are doing the process described above, you should consider using entry processors. An entry processor executes a read and updates upon the member where the data resides. This eliminates the costly network hops described above.

> **NOTE: Entry processor is meant to process a single entry per call. Processing multiple entries and data structures in an entry processor is not supported as it may result in deadlocks on the server side.**

Hazelcast sends the entry processor to each cluster member and these members apply it to the map entries. Therefore, if you add more members, your processing completes faster.

### 7.6.2. Processing Entries

The `IMap` interface provides the following functions for entry processing:

* `executeOnKey` processes an entry mapped by a key.
* `executeOnKeys` processes entries mapped by a list of keys.
* `executeOnEntries` can process all entries in a map with a defined predicate. Predicate is optional.

In the .NET client, an `EntryProcessor` should be `IIdentifiedDataSerializable` or `IPortable` because the server should be able to deserialize it to process.

The following is an example for `EntryProcessor` which is `IIdentifiedDataSerializable`.

```c#
public class IdentifiedEntryProcessor : IEntryProcessor, IIdentifiedDataSerializable
{
    internal const int ClassId = 1;

    private string value;

    public IdentifiedEntryProcessor(string value = null)
    {
        this.value = value;
    }

    public void ReadData(IObjectDataInput input)
    {
        value = input.ReadUTF();
    }

    public void WriteData(IObjectDataOutput output)
    {
        output.WriteUTF(value);
    }

    public int GetFactoryId()
    {
        return IdentifiedFactory.FactoryId;
    }

    public int GetId()
    {
        return ClassId;
    }
}
```

Now, you need to make sure that the Hazelcast member recognizes the entry processor. For this, you need to implement the Java equivalent of your entry processor and its factory, and create your own compiled class or JAR files. For adding your own compiled class or JAR files to the server's `CLASSPATH`, see the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

The following is the Java equivalent of the entry processor in .NET client given above:

```java
import com.hazelcast.map.AbstractEntryProcessor;
import com.hazelcast.nio.ObjectDataInput;
import com.hazelcast.nio.ObjectDataOutput;
import com.hazelcast.nio.serialization.IdentifiedDataSerializable;
import java.io.IOException;
import java.util.Map;

public class IdentifiedEntryProcessor extends AbstractEntryProcessor<String, String> implements IdentifiedDataSerializable {
     static final int CLASS_ID = 1;
     private String value;

    public IdentifiedEntryProcessor() {
    }

     @Override
    public int getFactoryId() {
        return IdentifiedFactory.FACTORY_ID;
    }

     @Override
    public int getId() {
        return CLASS_ID;
    }

     @Override
    public void writeData(ObjectDataOutput out) throws IOException {
        out.writeUTF(value);
    }

     @Override
    public void readData(ObjectDataInput in) throws IOException {
        value = in.readUTF();
    }

     @Override
    public Object process(Map.Entry<String, String> entry) {
        entry.setValue(value);
        return value;
    }
}
```

You can implement the above processor’s factory as follows:

```java
import com.hazelcast.nio.serialization.DataSerializableFactory;
import com.hazelcast.nio.serialization.IdentifiedDataSerializable;

public class IdentifiedFactory implements DataSerializableFactory {
    public static final int FACTORY_ID = 5;

     @Override
    public IdentifiedDataSerializable create(int typeId) {
        if (typeId == IdentifiedEntryProcessor.CLASS_ID) {
            return new IdentifiedEntryProcessor();
        }
        return null;
    }
}
```

Now you need to configure the `hazelcast.xml` to add your factory as shown below.

```xml
<hazelcast>
    <serialization>
        <data-serializable-factories>
            <data-serializable-factory factory-id="5">
                IdentifiedFactory
            </data-serializable-factory>
        </data-serializable-factories>
    </serialization>
</hazelcast>
```

The code that runs on the entries is implemented in Java on the server side. The client side entry processor is used to specify which entry processor should be called. For more details about the Java implementation of the entry processor, see the [Entry Processor section](https://docs.hazelcast.org/docs/latest/manual/html-single/index.html#entry-processor) in the Hazelcast IMDG Reference Manual.

After the above implementations and configuration are done and you start the server where your library is added to its `CLASSPATH`, you can use the entry processor in the `IMap` functions. See the following example.

```c#
map.ExecuteOnKey("key", new IdentifiedEntryProcessor("processed"));
Console.WriteLine("Value for key is : {0}", map.Get("key"));
//Output:
//Value for key is : processed
```

## 7.7. Distributed Query

Hazelcast partitions your data and spreads it across cluster of members. You can iterate over the map entries and look for certain entries (specified by predicates) you are interested in. However, this is not very efficient because you will have to bring the entire entry set and iterate locally. Instead, Hazelcast allows you to run distributed queries on your distributed map.

### 7.7.1. How Distributed Query Works

1. The requested predicate is sent to each member in the cluster.
2. Each member looks at its own local entries and filters them according to the predicate. At this stage, key-value pairs of the entries are deserialized and then passed to the predicate.
3. The predicate requester merges all the results coming from each member into a single set.

Distributed query is highly scalable. If you add new members to the cluster, the partition count for each member is reduced and thus the time spent by each member on iterating its entries is reduced. In addition, the pool of partition threads evaluates the entries concurrently in each member, and the network traffic is also reduced since only filtered data is sent to the requester.

**Predicates Class Operators**

There are many built-in `IPredicate` implementations for your query requirements. Some of them are explained below.

* `TruePredicate`: This predicate returns true and hence includes all the entries on the response.
* `FalsePredicate`: This predicate returns false and hence filters out all the entries in the response.
* `EqualPredicate`: Checks if the result of an expression is equal to a given value.
* `NotEqualPredicate`: Checks if the result of an expression is not equal to a given value.
* `InstanceOfPredicate`: Checks if the result of an expression has a certain type.
* `LikePredicate`: Checks if the result of an expression matches some string pattern. `%` (percentage sign) is the placeholder for many characters, `_` (underscore) is placeholder for only one character.
* `ILikePredicate`: Same as LikePredicate. Checks if the result of an expression matches some string pattern. `%` (percentage sign) is the placeholder for many characters, `_` (underscore) is placeholder for only one character.
* `GreaterLessPredicate`: Checks if the result of an expression is greater equal or less equal than a certain value.
* `BetweenPredicate`: Checks if the result of an expression is between two values (start and end values are inclusive).
* `InPredicate`: Checks if the result of an expression is an element of a certain list.
* `NotPredicate`: Negates a provided predicate result.
* `RegexPredicate`: Checks if the result of an expression matches some regular expression.
* `SqlPredicate`: Query using SQL syntax.

**Simplifying with Predicates**

You can simplify the predicate usage with the `Predicates` class, which offers simpler predicate building. 
Please see the below example code.

```c#
var users = hz.GetMap<string, User>("users");
// Add some users to the Distributed Map

// Create a Predicate from a String (a SQL like Where clause)
var sqlQuery = Predicates.Sql("active AND age BETWEEN 18 AND 21)");

// Creating the same Predicate as above but with a builder
var criteriaQuery = Predicates.And(
    Predicates.IsEqual("active", true),
    Predicates.IsBetween("age", 18, 21)
);
// Get result collections using the two different Predicates
var result1 = users.Values(sqlQuery);
var result2 = users.Values(criteriaQuery);
```

#### 7.7.1.1. Employee Map Query Example

Assume that you have an `employee` map containing the values of `Employee`, as coded below.

```c#
public class Employee : IPortable
{
    public const int TypeId = 100;

    public string Name { get; set; }
    public int Age { get; set; }
    public bool Active { get; set; }
    public double Salary { get; set; }

    public int GetClassId()
    {
        return TypeId;
    }

    public int GetFactoryId()
    {
        return ExampleDataSerializableFactory.FactoryId;
    }

    public void ReadPortable(IPortableReader reader)
    {
        Name = reader.ReadUTF("name");
        Age = reader.ReadInt("age");
        Active = reader.ReadBoolean("active");
        Salary = reader.ReadDouble("salary");
    }

    public void WritePortable(IPortableWriter writer)
    {
        writer.WriteUTF("name", Name);
        writer.WriteInt("age", Age);
        writer.WriteBoolean("active", Active);
        writer.WriteDouble("salary", Salary);
    }
}

```

Note that `Employee` is implementing `IPortable`. As portable types are not deserialized on the server side for querying, you don't need to implement its Java equivalent on the server side.

For the non-portable types, you need to implement its Java equivalent and its serializable factory on the server side for server to reconstitute the objects from binary formats. 
In this case before starting the server, you need to compile the `Employee` and related factory classes with server's `CLASSPATH` and add them to the `user-lib` directory in the extracted `hazelcast-<version>.zip` (or `tar`). See the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

> **NOTE: Querying with `IPortable` interface is faster as compared to `IdentifiedDataSerializable`.**

#### 7.7.1.2. Querying by Combining Predicates with AND, OR, NOT

You can combine predicates by using the `and`, `or` and `not` operators, as shown in the below example.

```c#
var criteriaQuery = Predicates.And(
    Predicates.IsEqual("active", true),
    Predicates.IsLessThan("age", 30)
);
var result2 = map.Values(criteriaQuery);
```

In the above example code, `predicate` verifies whether the entry is active and its `age` value is less than 30. 
This method sends the predicate to all cluster members and merges the results coming from them.

> **NOTE: Predicates can also be applied to `keySet` and `entrySet` of the Hazelcast IMDG's distributed map.**

#### 7.7.1.3. Querying with SQL

`SqlPredicate` takes the regular SQL `where` clause. See the following example:

```c#
var map = hazelcastInstance.GetMap<string, Employee>( "employee" );
var employees = map.Values( new SqlPredicate( "active AND age < 30" ) );
```

##### Supported SQL Syntax

**AND/OR:** `<expression> AND <expression> AND <expression>…`

- `active AND age > 30`
- `active = false OR age = 45 OR name = 'Joe'`
- `active AND ( age > 20 OR salary < 60000 )`

**Equality:** `=, !=, <, ⇐, >, >=`

- `<expression> = value`
- `age <= 30`
- `name = 'Joe'`
- `salary != 50000`

**BETWEEN:** `<attribute> [NOT] BETWEEN <value1> AND <value2>`

- `age BETWEEN 20 AND 33 ( same as age >= 20 AND age ⇐ 33 )`
- `age NOT BETWEEN 30 AND 40 ( same as age < 30 OR age > 40 )`

**IN:** `<attribute> [NOT] IN (val1, val2,…)`

- `age IN ( 20, 30, 40 )`
- `age NOT IN ( 60, 70 )`
- `active AND ( salary >= 50000 OR ( age NOT BETWEEN 20 AND 30 ) )`
- `age IN ( 20, 30, 40 ) AND salary BETWEEN ( 50000, 80000 )`

**LIKE:** `<attribute> [NOT] LIKE 'expression'`

The `%` (percentage sign) is the placeholder for multiple characters, an `_` (underscore) is the placeholder for only one character.

- `name LIKE 'Jo%'` (true for 'Joe', 'Josh', 'Joseph' etc.)
- `name LIKE 'Jo_'` (true for 'Joe'; false for 'Josh')
- `name NOT LIKE 'Jo_'` (true for 'Josh'; false for 'Joe')
- `name LIKE 'J_s%'` (true for 'Josh', 'Joseph'; false 'John', 'Joe')

**ILIKE:** `<attribute> [NOT] ILIKE 'expression'`

ILIKE is similar to the LIKE predicate but in a case-insensitive manner.

- `name ILIKE 'Jo%'` (true for 'Joe', 'joe', 'jOe','Josh','joSH', etc.)
- `name ILIKE 'Jo_'` (true for 'Joe' or 'jOE'; false for 'Josh')

**REGEX:** `<attribute> [NOT] REGEX 'expression'`

- `name REGEX 'abc-.*'` (true for 'abc-123'; false for 'abx-123')

##### Querying Examples with Predicates

You can use the `__key` attribute to perform a predicated search for the entry keys. See the following example:

```c#
var employeeMap = client.getMap<string, Employee>("employees");
employeeMap.Put("Alice", new Employee {Name= "Alice", Age= "35"});
employeeMap.Put("Andy",  new Employee {Name= "Andy", Age= "37"});
employeeMap.Put("Bob",   new Employee {Name= "Bob", Age= "22"});
// ...
var predicate = new SqlPredicate("__key like A%");
var startingWithA = employeeMap.Values(predicate);
```

You can also use the helper class `Predicates` mentioned earlier. Here is an example:

```c#
//continued from previous example
var predicate = Predicates.Key().IsLike("A%");;
var startingWithA = personMap.values(predicate);
```

It is also possible to use a complex object as key and make query on key fields.

```c#
var employeeMap = client.getMap<Employee, int'>("employees");
employeeMap.Put(new Employee {Name= "Alice", Age= "35"}, 1);
employeeMap.Put(new Employee {Name= "Andy", Age= "37"}, 2);
employeeMap.Put(new Employee {Name= "Bob", Age= "22"}, 3);
// ...
var predicate = Predicates.Key("name").IsLike("A%");//identical to sql predicate:"__key#name LIKE A%"
var startingWithA = employeeMap.Values(predicate);
```

You can use the `this` attribute to perform a predicated search for entry values. See the following example:

```c#
//continued from previous example
var predicate=Predicates.IsGreaterThan("this", 2);
var result = employeeMap.Values(predicate);
//result will include only Bob
```

#### 7.7.1.4. Querying with JSON Strings

You can query JSON strings stored inside your Hazelcast clusters. To query the JSON string,
you first need to create a `HazelcastJsonValue` from the JSON string using the `HazelcastJsonValue(string jsonString)` constructor.
You can use ``HazelcastJsonValue``s both as keys and values in the distributed data structures. 
Then, it is possible to query these objects using the Hazelcast query methods explained in this section.

```c#
    var person1 = new HazelcastJsonValue("{ \"age\": 35 }");
    var person2 = new HazelcastJsonValue("{ \"age\": 24 }");
    var person3 = new HazelcastJsonValue("{ \"age\": 17 }");

    var idPersonMap = client.GetMap<int, HazelcastJsonValue>("jsonValues");

    idPersonMap.Put(1, person1);
    idPersonMap.Put(2, person2);
    idPersonMap.Put(3, person3);

    var peopleUnder21 = idPersonMap.Values(Predicates.IsLessThan("age", 21));
```

When running the queries, Hazelcast treats values extracted from the JSON documents as Java types so they
can be compared with the query attribute. JSON specification defines five primitive types to be used in the JSON
documents: `number`,`string`, `true`, `false` and `null`. The `string`, `true/false` and `null` types are treated
as `String`, `boolean` and `null`, respectively. We treat the extracted `number` values as ``long``s if they
can be represented by a `long`. Otherwise, ``number``s are treated as ``double``s.

It is possible to query nested attributes and arrays in the JSON documents. The query syntax is the same
as querying other Hazelcast objects using the ``Predicate``s.

```c#
/**
 * Sample JSON object
 *
 * {
 *     "departmentId": 1,
 *     "room": "alpha",
 *     "people": [
 *         {
 *             "name": "Peter",
 *             "age": 26,
 *             "salary": 50000
 *         },
 *         {
 *             "name": "Jonah",
 *             "age": 50,
 *             "salary": 140000
 *         }
 *     ]
 * }
 *
 *
 * The following query finds all the departments that have a person named "Peter" working in them.
 */
 
var departmentWithPeter = departments.Values(Predicates.IsEqual("people[any].name", "Peter"));

```

`HazelcastJsonValue` is a lightweight wrapper around your JSON strings. It is used merely as a way to indicate
that the contained string should be treated as a valid JSON value. Hazelcast does not check the validity of JSON
strings put into to the maps. Putting an invalid JSON string into a map is permissible. However, in that case
whether such an entry is going to be returned or not from a query is not defined.

#### 7.7.1.5. Filtering with Paging Predicates

The .NET client provides paging for defined predicates. With its `PagingPredicate` class, you can get a list of keys, values or entries page 
by page by filtering them with predicates and giving the size of the pages. Also, you can sort the entries by specifying comparators.

```c#
var map = hazelcastInstance.GetMap<int, Student>( "students" );
var greaterEqual = Predicates.IsGreaterThanOrEqual( "age", 18 );
var pagingPredicate = new PagingPredicate(pageSize:5, predicate:greaterEqual);
// Retrieve the first page
var values = map.Values( pagingPredicate );
//...
// Set up next page
pagingPredicate.NextPage();
// Retrieve next page
var values = map.Values( pagingPredicate );

//...
```

If you want to sort the result before paging, you need to specify a comparator object that implements the `System.Collections.Generic.IComparer<KeyValuePair<object, object>>` interface. 
Also, this comparator class should implement' one of `IIdentifiedDataSerializable` or `IPortable`. After implementing this class in .NET, 
you need to implement the Java equivalent of it and its factory. The Java equivalent of the comparator should implement `java.util.Comparator`. 
Note that the `Compare` function of `Comparator` on the Java side is the equivalent of the `sort` function of `Comparator` on the .NET side. 
When you implement the `Comparator` and its factory, you can add them to the `CLASSPATH` of the server side.  
See the [Adding User Library to CLASSPATH section](#1212-adding-user-library-to-classpath).

Also, you can access a specific page more easily with the help of the `Page` property. This way, if you make a query for the 100th page, 
for example, it will get all 100 pages at once instead of reaching the 100th page one by one using the `NextPage` function.

### 7.7.2. Fast-Aggregations

Fast-Aggregations feature provides some aggregate functions, such as `sum`, `average`, `max`, and `min`, on top of Hazelcast `IMap` entries. 
Their performance is perfect since they run in parallel for each partition and are highly optimized for speed and low memory consumption.

The `Aggregators` class provides a wide variety of built-in aggregators. The full list is presented below:

* count
* bigInteger sum/avg/min/max
* double sum/avg/min/max
* integer sum/avg/min/max
* long sum/avg/min/max
* number avg
* fixedPointSum, floatingPointSum

You can use these aggregators with the `IMap.Aggregate(aggregator)` and `IMap.Aggregate(aggregator, predicate)` methods.

## 7.8. Monitoring and Logging

### 7.8.1. Enabling Client Statistics

You can enable the client statistics before starting your clients. There are environment variables related to client statistics:

- `hazelcast.client.statistics.enabled`: If set to `true`, it enables collecting the client statistics and sending them to the cluster. When it is `true` you can monitor the clients that are connected to your Hazelcast cluster, using Hazelcast Management Center. Its default value is `false`.

- `hazelcast.client.statistics.period.seconds`: Period in seconds the client statistics are collected and sent to the cluster. Its default value is `3`.

> **NOTE: These two variables need to be configured on Member, too. They are identical on member and client.**

### 7.8.2. Logging Configuration

You can configure logging type and level for your client. There are two environment variables for this.

- `hazelcast.logging.type` : Configures the logging type. One of values `console`, `trace` . if left empty, all logging will be disabled. `console` option will log to `Console`. `trace` option will use `System.Diagnostics.Trace` to log.
- `hazelcast.logging.level` : Configures the logging level. You can configure the log verbosity by setting one of enum values of `Hazelcast.Logging.LogLevel`

Example:

```c#
Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
Environment.SetEnvironmentVariable("hazelcast.logging.level", "finest");
```

Above configuration set logging type to console and the highest verbosity level `finest`.


# 8. Development and Testing

Hazelcast .NET client is developed using C#. If you want to help with bug fixes, develop new features or
tweak the implementation to your application's needs, you can follow the steps in this section.

## 8.1. Building and Using Client From Sources

You can build the source by calling the batch file `build.bat`.

**Strong name generation**

Hazelcast assemblies are signed using a [strong name key](https://msdn.microsoft.com/en-us/library/wd40t7ad.aspx). 

To be able to build the project, a public key is already configured in the codebase. If you want to build your own assemblies, you will need to create your own strong name key.

This can be done using the sn.exe tool which ships with .NET framework.

```
sn -k hazelcast.snk
```

Furthermore, you will need to update `Hazelcast.Net/Properties/AssemblyInfo.cs` with the new public key. 

```
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(@"Hazelcast.Test, PublicKey=00240000049e....b3")]
```

To get the new public key, use the following commands:

```
sn -p hazelcast.snk hazelcast.key
sn -tp hazelcast.key
```

## 8.2. Testing

In order to test Hazelcast .NET client locally, you will need the following:

* Java 6 or newer
* Maven

All the tests use NUnit, and require a `hazelcast.jar` and JVM to run the hazelcast instance. 
The script `build.bat` will attempt to download `hazelcast.jar` for the latest snapshot from Maven Central and will run the tests using the downloaded JAR. 

# 9. Getting Help

You can use the following channels for your questions and development/usage issues:

* This repository by opening an issue.
* Our Google Groups directory: https://groups.google.com/forum/#!forum/hazelcast
* Stack Overflow: https://stackoverflow.com/questions/tagged/hazelcast

# 10. Contributing

Besides your development contributions as explained in the [Development and Testing chapter](#8-development-and-testing) above, 
you can always open a pull request on this repository for your other requests such as documentation changes. 

Please complete the [Hazelcast Contributor Agreement](https://hazelcast.atlassian.net/wiki/display/COM/Hazelcast+Contributor+Agreement).

For an enhancement or larger feature, create a GitHub issue first to discuss.


# 11. License

Hazelcast is available under the [Apache 2 License](https://github.com/hazelcast/hazelcast-csharp-client/blob/master/LICENSE). 
Please see the [Licensing section](https://docs.hazelcast.org/docs/latest-dev/manual/html-single/index.html#licensing) for more information.

# 12. Copyright

Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.

Visit [www.hazelcast.com](http://www.hazelcast.com) for more information.
