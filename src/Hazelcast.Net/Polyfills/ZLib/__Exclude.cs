
#if NET6_0_OR_GREATER

// ensure that Hazelcast.Polyfills.ZLib is *not* compiled for .NET 6+
#error Hazelcast.Polyfills.ZLib code should not build for this target framework!

#endif