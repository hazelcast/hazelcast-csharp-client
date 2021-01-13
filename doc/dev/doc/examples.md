# Examples

The [Hazelcast.Net.Examples](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples) project (only provided in source form) provides a range of examples that demonstrate how to use the Hazelcast.Net client.

Building the complete Hazelcast.Net solution builds the example project. 

## Running Examples

Examples can then be executed with:

```
cd src/Hazelcast.Net/Examples/bin/<configuration>/<target>
./hx.exe <name>
```

Where:

* `<configuration>` is the configuration that was built, either `Debug` or `Release`.
* `<target>` is the target .NET, either `net462` for .NET Framework 4.6.2, or `netcoreapp2.1` or `netcoreapp3.1` for the corresponding .NET Core versions.
* `<name>` is the short name of the example class. If the full class name is `Hazelcast.Examples.Namespace.SomeExample` then the short name is `Namespace.SomeExample`. Note that the `Example` suffix can be ommited.

For instance, this runs the .NET Framework 4.6.2, Release build, client SimpleExample:

```
cd src/Hazelcast.Net/Examples/bin/Release/net462
./hx.exe Client.Simple
```

Additional arguments are passed to the example as command-line arguments, and therefore can be used to configure Hazelcast. For instance, if the server runs on `192.168.42.42:5757`, the example above can be launched with:

```
./hx.exe Client.SimpleExample --hazelcast.networking.addresses.0=192.168.42.42:5757
```

## Advanced Running

The example launcher (`hx.exe`) can identify the example to run via a regular expression. The expression is provided by prefixing the `<name>` argument with a `~` character, and then the launcher runs every example whose type `FullName` property matches the expression.

For instance, this runs every example with a name containing "CustomSerializer".

```
./hx.exe "~CustomSerializer" 
```

## Reusing Examples

Each example is proposed as a standalone class that implements a static `Main` method. Therefore, each example can be copied and executed directly in a new project. Note that the `HazelcastOptionsBuilderExtensions` may also need to be copied.