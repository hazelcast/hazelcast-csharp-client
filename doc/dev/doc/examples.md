# Examples

The [Hazelcast.Net.Examples](https://github.com/hazelcast/hazelcast-csharp-client/tree/master/src/Hazelcast.Net.Examples) project (only provided in source form) provides a range of examples that demonstrate how to use the Hazelcast.Net client.

Building the complete Hazelcast.Net solution builds the example project. 

## Running Examples

Examples can then be executed through the `hz.[sh|ps1]` script:

```powershell
PS> ./hz.ps1 run-example Client.SimpleExample
```

By default, this runs the `netcoreapp3.1` Release version of the example. The `hz.[sh|ps1]` script provides options (see the [Building](contrib/building.md) page for details) that can be used to change these. For instance, the following command runs the `net462` Debug version of the example:

```powershell
PS> ./hz.ps1 run-example -c Debug -f net462 Client.SimpleExample
```

The example name (here, `Client.SimpleExample`) is the short name of the class: if the full class name is `Hazelcast.Examples.Namespace.SomeExample` then the short name is `Namespace.SomeExample`. The `Example` suffix can be ommited, so `Client.Simple` would work too. And, it is possible to use a tilde character to run examples with name matching the argument. For instance `~Client` would run all examples with a short name containing `Client`.

Additional arguments are passed to the example as command-line arguments, and therefore can be used to configure Hazelcast. For instance, if the server runs on `192.168.42.42:5757`, the example above can be launched with:

```powershell
PS> ./hz.ps1 run-example Client.SimpleExample --- --hazelcast.networking.addresses.0=192.168.42.42:5757
```

Note: the `---` separator tells the `hz.[sh|ps]` script that the trailing arguments are not arguments for the script, but for the example.

## Reusing Examples

Each example is proposed as a standalone class that implements a static `Main` method. Therefore, each example can be copied and executed directly in a new project. Note that the `HazelcastOptionsBuilderExtensions` may also need to be copied.