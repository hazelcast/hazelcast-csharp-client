// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.IO;
using System.Runtime.CompilerServices;
using Hazelcast.Client;
using NUnit.Framework;
using PublicApiGenerator;

public class ApiApprovals
{
#if NETFRAMEWORK
    [Test]
    public void ApproveApi()
    {
        var publicApi = ApiGenerator.GeneratePublicApi(typeof(HazelcastClient).Assembly);
        Approve(publicApi, scenario: "netframework");
    }
#endif

#if NETCOREAPP
        [Test]
        public void ApproveApi()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(HazelcastClient).Assembly);
            Approve(publicApi, scenario: "netcoreapp");
        }
#endif

    static void Approve(string publicApi, string scenario, [CallerFilePath] string filePath = null)
    {
        var directory = Path.GetDirectoryName(filePath);
        var file = Path.Combine(directory, $"API_{scenario}.txt");
        if (File.Exists(file) == false)
        {
            File.WriteAllText(file, publicApi);
            return;
        }

        var approved = File.ReadAllText(file);
        Assert.AreEqual(approved, publicApi);
    }
}
