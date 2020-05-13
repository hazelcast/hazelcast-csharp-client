using System;
using NuGet.Versioning;
using NUnit.Framework;

// A SetUpFixture outside of any namespace provides SetUp and TearDown for the entire assembly.

/*
[SetUpFixture]
// ReSharper disable once CheckNamespace
public class GlobalFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Console.WriteLine("ONE TIME BLAH");
        ServerConditionAttribute.ServerVersion = NuGetVersion.Parse("4.1");
    }
}*/