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
                if (request.Url.PathAndQuery.Equals("/cluster/discovery?token=" + DiscoveryToken))
                {
                    var response = context.Response;
                    var buffer = System.Text.Encoding.UTF8.GetBytes(JsonResponse);
    
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                else
                {
                    var response = context.Response;
                    response.StatusCode = (int) HttpStatusCode.NotFound;
                    var buffer = System.Text.Encoding.UTF8.GetBytes(JsonErrorResponse);
    
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();

                }
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
    }
}