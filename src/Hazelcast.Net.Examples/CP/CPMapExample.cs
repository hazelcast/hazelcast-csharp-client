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

using System;
using System.Threading.Tasks;

namespace Hazelcast.Examples.CP;

public class CPMapExample
{
    public static async Task Main(string[] args)
    {
        var options = new HazelcastOptionsBuilder()
            .With(args)
            .WithConsoleLogger()
            .Build();

        // create an Hazelcast client and connect to a enterprise server running on localhost
        // note that that server should be properly configured for CP with at least 3 members
        await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

        // Get a CPMap under "myGroup" Raft group.
        var map = await client.CPSubsystem.GetMapAsync<int, string>("myMap@myGroup");

        var (key, val) = (1, "my-value");
        
        // Set a value  
        // Note: Set does not return back the old value that is associated with the key. If you require the previous value,
        // consider using PutAsync.
        await map.SetAsync(key, val);

        // Get value that is    map to the key.
        // If key does not exist, the return value will be null. However, we know that the key-value pair exists, and
        // ignore the possible null warning.
        var currentVal = await map.GetAsync(key)!;

        Console.WriteLine($"Key: {key}, Expected Value: {val}, Actual Value:{currentVal}");

        // Let's change the value of the key by using CompareAndSetAsync
        // The expected value will be compared to current value which is associated to given key. 
        // If they are equal, new value will be set.
        var newValue = "my-new-value";
        var isSet = await map.CompareAndSetAsync(key, currentVal, newValue);

        Console.WriteLine($"Key: {key}, Expected Value: {currentVal}, New Value:{newValue}, Is set successfully done:{isSet}");


        // Assume that we do not need the map anymore. So, it is better to destroy the map on the cluster and release the resources.
        // Note that Hazelcast does NOT do garbage collection on CPMap.
        await map.DestroyAsync();
    }
}
