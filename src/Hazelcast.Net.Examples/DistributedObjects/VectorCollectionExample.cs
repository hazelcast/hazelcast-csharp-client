// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Models;
namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class VectorCollectionExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get distributed vector collection from cluster
            // Assume that the vector collection is configured on the cluster.
            // It has single index with 3 dimensions.
            await using var vectorCollection = await client.GetVectorCollectionAsync<string, HazelcastJsonValue>("vector-example");

            // Part 1: Add a vector to the collection

            // create a metadata object for the vector
            var bookMetaData = new HazelcastJsonValue(@"
                    {
                ""title"": ""Lord Of the Rings"",
                ""author"": ""J.R.R. Tolkien"",
                ""description"": ""The Lord of the Rings is an epic high-fantasy novel written by English author and scholar J. R. R. Tolkien. 
                The story began as a sequel to Tolkien's 1937 fantasy novel The Hobbit, but eventually developed into a much larger work. 
                Written in stages between 1937 and 1949, The Lord of the Rings is one of the best-selling novels ever written, with over 150 million copies sold."",
                ""year"": 1954
            }");

            // create a vector document with the metadata and the vector values
            var vectorDoc = VectorDocument<HazelcastJsonValue>
                .Of(bookMetaData, VectorValues.Of(new float[] { 0.1f, 0.2f, 0.3f }));


            // add the vector to the collection
            await vectorCollection.SetAsync("book-1", vectorDoc);


            // Part 2: Query the collection
            //Arrange the vector values
            var queryVector = VectorValues.Of(new float[] { 0.1f, 0.2f, 0.3f });

            var result = await vectorCollection.SearchAsync(queryVector,
                new VectorSearchOptions(includeVectors: true,
                    includeValue: true,
                    limit: 3
                ));

            // print the search results
            foreach (var entry in result.Results)
            {
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}, Distance: {entry.Score}, Vector: {entry.Vectors}");
            }

            /*
             OUTPUT:
            Key: book-1, Value:
            {
                "title": "Lord Of the Rings",
                "author": "J.R.R. Tolkien",
                "description": "The Lord of the Rings is an epic high-fantasy novel written by English author and scholar J. R. R. Tolkien.
                The story began as a sequel to Tolkien's 1937 fantasy novel The Hobbit, but eventually developed into a much larger work.
                Written in stages between 1937 and 1949, The Lord of the Rings is one of the best-selling novels ever written, with over 150 million copies sold.",
                "year": 1954
            }, Distance: 0.57, Vector: SingleVectorValues{vector=[0.1, 0.2, 0.3]}

            */

            // destroy the topic
            await client.DestroyAsync(vectorCollection);
        }
    }
}
