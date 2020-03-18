// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class HazelcastCloudDiscoveryTest
    {
        private const string JsonErrorResponse =
            "{\"application\":\"orchestrator-service\",\"message\":\"Cluster with token: DiscoveryToken not found.\"}";

        private const string JsonResponse = "[{\"private-address\":\"10.47.0.8\",\"public-address\":\"54.213.63.142:32298\"}," +
                                            "{\"private-address\":\"10.47.0.9\",\"public-address\":\"54.245.77.185:32298\"}," +
                                            "{\"private-address\":\"10.47.0.10\",\"public-address\":\"54.186.232.37:32298\"}]";

        private static readonly Dictionary<Address, Address> Addresses = new Dictionary<Address, Address>
        {
            {new Address("10.47.0.8", 32298), new Address("54.213.63.142", 32298)},
            {new Address("10.47.0.9", 32298), new Address("54.245.77.185", 32298)},
            {new Address("10.47.0.10", 32298), new Address("54.186.232.37", 32298)}
        };

        private const string DiscoveryToken = "DiscoveryToken";
        private const string DiscoveryTokenInvalid = "DiscoveryTokenInvalid";
        private const string LocalTestBaseUrl = "http://localhost:8001";

        private HttpListener _httpListener;

        private readonly Dictionary<string, string> _responses = new Dictionary<string, string>
        {
            { DiscoveryToken, JsonResponse }
        };

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(LocalTestBaseUrl + "/cluster/");
            _httpListener.Start();
        }

        [SetUp]
        public void Setup()
        {
            _httpListener.BeginGetContext(asyncResult =>
            {
                var listener = (HttpListener) asyncResult.AsyncState;
                var context = listener.EndGetContext(asyncResult);
                var request=context.Request;
                var token = request.Url.PathAndQuery.Substring("/cluster/discovery?token=".Length);

                var response = context.Response;

                byte[] buffer;
                string jsonText;
                if (_responses.TryGetValue(token, out jsonText))
                {
                    buffer = System.Text.Encoding.UTF8.GetBytes(jsonText);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    buffer = System.Text.Encoding.UTF8.GetBytes(JsonErrorResponse);
                }

                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }, _httpListener);
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _httpListener.Close();
        }

        [TearDown]
        public void Teardown()
        {
            Environment.SetEnvironmentVariable(HazelcastCloudDiscovery.CloudUrlBaseProperty, null);
        }

        [Test]
        public void TestHzCloudDiscovery()
        {
            var hzCloudDiscovery = new HazelcastCloudDiscovery(DiscoveryToken, int.MaxValue, LocalTestBaseUrl);
            var result = hzCloudDiscovery.DiscoverNodes();
            Assert.IsNotEmpty(result);
            foreach (var kv in result)
            {
                Assert.True(Addresses.ContainsKey(kv.Key) && Addresses[kv.Key].Equals(kv.Value));
            }
        }

        [Test]
        public void TestHzCloudDiscovery_invalidToken()
        {
            var hzCloudDiscovery = new HazelcastCloudDiscovery(DiscoveryTokenInvalid, int.MaxValue, LocalTestBaseUrl);
            var result = hzCloudDiscovery.DiscoverNodes();
            Assert.IsNull(result);
        }

        [Test]
        public void TestHzCloudAddressProvider()
        {
            Environment.SetEnvironmentVariable(HazelcastCloudDiscovery.CloudUrlBaseProperty, LocalTestBaseUrl);
            var cfg = new ClientConfig();
            cfg.GetNetworkConfig().GetCloudConfig().SetEnabled(true).SetDiscoveryToken(DiscoveryToken);

            var addressProvider = new AddressProvider(cfg);
            var providedAddresses = addressProvider.GetAddresses().ToList();
            Assert.IsNotEmpty(providedAddresses);
            foreach (var key in providedAddresses)
            {
                Assert.True(Addresses.ContainsKey(key));
                Assert.AreEqual(addressProvider.TranslateToPublic(key), Addresses[key]);
            }
        }

        [Test]
        public void TestParseResponse_DefaultPort()
        {
            const string jsonText = "[{\"private-address\":\"100.96.5.1\",\"public-address\":\"10.113.44.139:31115\"},"
                                   + "{\"private-address\":\"100.96.4.2\",\"public-address\":\"10.113.44.130:31115\"} ]";

            _responses["SomeToken"] = jsonText;

            var cloudDiscovery = new HazelcastCloudDiscovery("SomeToken", int.MaxValue, LocalTestBaseUrl);
            var nodes = cloudDiscovery.DiscoverNodes();
            Assert.IsNotNull(nodes);
            Assert.AreEqual(2, nodes.Count);

            foreach (var node in nodes)
                Console.WriteLine("{0} -> {1}", node.Key, node.Value);

            Address a1, a2;
            Assert.IsTrue(nodes.TryGetValue(new Address("100.96.5.1", 31115), out a1));
            Assert.IsTrue(a1.Equals(new Address("10.113.44.139", 31115)));
            Assert.IsTrue(nodes.TryGetValue(new Address("100.96.4.2", 31115), out a2));
            Assert.IsTrue(a2.Equals(new Address("10.113.44.130", 31115)));
        }

        [Test]
        public void TestParseResponse_DifferentPort()
        {
            const string jsonText = " [{\"private-address\":\"100.96.5.1:5701\",\"public-address\":\"10.113.44.139:31115\"},"
                                    + "{\"private-address\":\"100.96.4.2:5701\",\"public-address\":\"10.113.44.130:31115\"} ]";

            _responses["SomeToken"] = jsonText;

            var cloudDiscovery = new HazelcastCloudDiscovery("SomeToken", int.MaxValue, LocalTestBaseUrl);
            var nodes = cloudDiscovery.DiscoverNodes();
            Assert.IsNotNull(nodes);
            Assert.AreEqual(2, nodes.Count);

            foreach (var node in nodes)
                Console.WriteLine("{0} -> {1}", node.Key, node.Value);

            Address a1, a2;
            Assert.IsTrue(nodes.TryGetValue(new Address("100.96.5.1", 5701), out a1));
            Assert.IsTrue(a1.Equals(new Address("10.113.44.139", 31115)));
            Assert.IsTrue(nodes.TryGetValue(new Address("100.96.4.2", 5701), out a2));
            Assert.IsTrue(a2.Equals(new Address("10.113.44.130", 31115)));
        }
    }
}