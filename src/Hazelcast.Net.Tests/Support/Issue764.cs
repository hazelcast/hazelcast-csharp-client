// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Hazelcast.Query;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Remote;

namespace Hazelcast.Tests.Support;


[TestFixture]
public class Issue764 : SingleMemberClientRemoteTestBase
{
    protected override HazelcastOptions CreateHazelcastOptions()
    {
        var options = base.CreateHazelcastOptions();
        options.Serialization.Compact.AddSerializer(new TradeSerializer());
        return options;
    }

    [Test]
    public async Task Reproduce()
    {
        var serverVersion = ServerVersion.GetVersion("5.0");
        Console.WriteLine($"Server Version: {serverVersion}");

        var clusterName = Client.ClusterName;
        var mapName = CreateUniqueName();

        using var run = new JavaRun()
            .WithLib($"hazelcast-{serverVersion}.jar")
            .WithSourceText("ActionType", JavaCode.ActionType)
            .WithSourceText("Trade", JavaCode.Trade)
            .WithSourceText("TradeSerializer", JavaCode.TradeSerializer)
            .WithSourceText("Main", JavaCode.Main
                .Replace("%%CLUSTERNAME%%", clusterName)
                .Replace("%%MAPNAME%%", mapName));

        Console.WriteLine("Compile Java code...");
        run.Compile();

        Console.WriteLine("Execute Java code...");
        var output = run.Execute("Main");

        Console.WriteLine("Output:");
        Console.WriteLine(output);
        Console.WriteLine("--");
        Console.WriteLine();

        await using var map = await Client.GetMapAsync<string, Trade>(mapName);

        Console.WriteLine("Read from .NET...");
        var trade = await map.GetAsync("trade1");
        Assert.That(trade, Is.Not.Null);
        Console.WriteLine($"TRADE: {trade.Id} {trade.Action} {trade.SourceTradeId}");
        Console.WriteLine();

        Console.WriteLine("Query from .NET with predicate...");
        var result = await map.GetValuesAsync(Predicates.Sql("id=1234"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        foreach (var resultTrade in result)
        {
            Assert.That(resultTrade, Is.Not.Null);
            Console.WriteLine($"TRADE: {resultTrade.Id} {resultTrade.Action} {resultTrade.SourceTradeId}");
        }
        Console.WriteLine();

        // NOTE! in the predicate, the casing of fields matters
        result = await map.GetValuesAsync(Predicates.Sql("ID=1234"));
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0)); // no result with 'ID', has to be 'id'
    }
}

public /*file*/ static class JavaCode
{
    public const string TradeSerializer = @"
import com.hazelcast.nio.serialization.compact.*;

public /*static*/ class TradeSerializer implements CompactSerializer<Trade> {
    @Override
    public Trade read(CompactReader reader) {
        Trade t = new Trade();
        t.setId(reader.readInt32(""ID""));
        t.setAction(ActionType.valueOf(reader.readString(""action"")));
        t.setSourceTradeId(reader.readString(""sourceTradeId""));
        return t;
    }
    
    @Override
    public void write(CompactWriter writer, Trade object) {
        writer.writeInt32(""ID"", object.getId());
        writer.writeString(""action"", object.getAction().toString());
        writer.writeString(""sourceTradeId"", object.getSourceTradeId());
    }
    
    @Override
    public String getTypeName() {
        return ""Trade"";
    }
    
    @Override
    public Class<Trade> getCompactClass() {
        return Trade.class;
    }
}
";

    public const string Trade = @"
public class Trade {
    private int id;
    private ActionType action;
    private String sourceTradeId;
    public void setId(int value) {
        id = value;
    }
    public void setAction(ActionType value) {
        action = value;
    }
    public void setSourceTradeId(String value) {
        sourceTradeId = value;
    }
    public int getId() {
        return id;
    }
    public ActionType getAction() {
        return action;
    }
    public String getSourceTradeId() {
        return sourceTradeId;
    }
}
";

    public const string ActionType = @"
public enum ActionType {
    Foo,
    Bar
}
";

    public const string Main = @"
import java.io.IOException;
import com.hazelcast.client.config.ClientConfig;
import com.hazelcast.core.HazelcastInstance;
import com.hazelcast.client.HazelcastClient;
import com.hazelcast.map.IMap;

public class Main {
    public static void main(String[] args) throws IOException {

        System.out.println(""JAVA: begin"");

        ClientConfig config = new ClientConfig();
        config.setClusterName(""%%CLUSTERNAME%%"");
        config.getNetworkConfig().addAddress(""127.0.0.1:5701"");
        HazelcastInstance client = HazelcastClient.newHazelcastClient(config);

        IMap<String, Trade> map = client.getMap(""%%MAPNAME%%"");

        Trade trade = new Trade();
        trade.setId(1234);
        trade.setAction(ActionType.Foo);
        trade.setSourceTradeId(""woot"");

        map.set(""trade1"", trade);

        client.shutdown();

        System.out.println(""JAVA: end"");
    }
}
";
}

public /*file*/ class Trade
{
    public int Id { get; set; }
    public string Action { get; set; }
    public string SourceTradeId { get; set; }
}

public /*file*/ class TradeSerializer : ICompactSerializer<Trade>
{
    public string TypeName => "Trade";

    public Trade Read(ICompactReader reader)
    {
        return new Trade
        {
            Id = reader.ReadInt32("id"),
            Action = reader.ReadString("action"),
            SourceTradeId = reader.ReadString("sourceTradeId")
        };
    }

    public void Write(ICompactWriter writer, Trade value)
    {
        writer.WriteInt32("id", value.Id);
        writer.WriteString("action", value.Action);
        writer.WriteString("sourceTradeId", value.SourceTradeId);
    }
}
