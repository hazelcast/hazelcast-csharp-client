// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Hazelcast.Models;
using Hazelcast.Serialization;
using NUnit.Framework;
using NSubstitute;

[TestFixture]
public class PartitioningStrategyOptionsTests
{
    [Test]
    public void Constructor_WithNoArguments_SetsPartitioningStrategyClassToNull()
    {
        var options = new PartitioningStrategyOptions();

        Assert.IsNull(options.PartitioningStrategyClass);
    }

    [Test]
    public void Constructor_WithPartitioningStrategyOptionsArgument_CopiesPartitioningStrategyClass()
    {
        var originalOptions = new PartitioningStrategyOptions { PartitioningStrategyClass = "OriginalClass" };
        var copiedOptions = new PartitioningStrategyOptions(originalOptions);

        Assert.AreEqual("OriginalClass", copiedOptions.PartitioningStrategyClass);
    }

    [Test]
    public void Constructor_WithStringArgument_SetsPartitioningStrategyClass()
    {
        var options = new PartitioningStrategyOptions("TestClass");

        Assert.AreEqual("TestClass", options.PartitioningStrategyClass);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var options = new PartitioningStrategyOptions("TestClass");

        var result = options.ToString();

        StringAssert.Contains("partitioningStrategyClass='TestClass'", result);
        StringAssert.Contains("partitioningStrategy=<not-supported>", result);
    }

    [Test]
    public void WriteData_WritesPartitioningStrategyClass()
    {
        var options = new PartitioningStrategyOptions("TestClass");
        var output = Substitute.For<IObjectDataOutput>();

        options.WriteData(output);

        output.Received().WriteString("TestClass");
    }

    [Test]
    public void ReadData_ReadsPartitioningStrategyClass()
    {
        var options = new PartitioningStrategyOptions();
        var input = Substitute.For<IObjectDataInput>();
        input.ReadString().Returns("TestClass");

        options.ReadData(input);

        Assert.AreEqual("TestClass", options.PartitioningStrategyClass);
    }
}
