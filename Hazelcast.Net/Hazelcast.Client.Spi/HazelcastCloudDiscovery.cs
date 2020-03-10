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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Spi
{
    internal class HazelcastCloudDiscovery
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(HazelcastCloudDiscovery));

        internal const string CloudUrlBaseProperty = "hazelcast.client.cloud.url";
        internal const string CloudUrlBase = "https://coordinator.hazelcast.cloud";
        private const string CloudUrlPath = "/cluster/discovery?token=";
        private const string RegexErrorStr = "(?<=message\":\").*?(?=\")";
        private const string RegexPrivateStr = "(?<=private-address\":\").*?(?=\")";
        private const string RegexPublicStr = "(?<=public-address\":\").*?(?=\")";

        private readonly string _endpointUrl;
        private readonly int _connectionTimeoutInMillis;

        internal HazelcastCloudDiscovery(string discoveryToken, int connectionTimeoutInMillis, string cloudBaseUrl)
        {
            _endpointUrl = cloudBaseUrl + CloudUrlPath + discoveryToken;
            _connectionTimeoutInMillis = connectionTimeoutInMillis;
        }

        public IDictionary<Address, Address> DiscoverNodes()
        {
            try
            {
                var uri = new Uri(_endpointUrl);
                var httpWebRequest = (HttpWebRequest) HttpWebRequest.Create(uri);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Timeout = _connectionTimeoutInMillis;
                httpWebRequest.ReadWriteTimeout = _connectionTimeoutInMillis;
                httpWebRequest.Headers.Set("Accept-Charset", "UTF-8");
                var resp = ReadFromResponse(httpWebRequest.GetResponse());
                return ParseResponse(resp);
            }
            catch (WebException we)
            {
                var resp = ReadFromResponse(we.Response);
                var errorMsg = ParseErrorResponse(resp);
                Logger.Warning("Hazelcast cloud discovery error: " + errorMsg);
            }
            catch (Exception e)
            {
                Logger.Warning("Hazelcast cloud discovery failed", e);
            }
            return null;
        }

        private string ReadFromResponse(WebResponse webResponse)
        {
            var responseStream = webResponse.GetResponseStream();
            var sr = new StreamReader(responseStream, Encoding.UTF8);
            var resp = sr.ReadToEnd();
            responseStream.Close();
            webResponse.Close();
            return resp;
        }
        
        private Dictionary<Address, Address> ParseResponse(string jsonResult)
        {
            var regexPrivate = new Regex(RegexPrivateStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regexPublic = new Regex(RegexPublicStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchesPrivate = regexPrivate.Matches(jsonResult);
            var matchesPublic = regexPublic.Matches(jsonResult);

            var privateToPublicAddresses = new Dictionary<Address, Address>();
            for (int i = 0; i < matchesPrivate.Count; i++)
            {
                var privateAddressStr = matchesPrivate[i].Value;
                var publicAddressStr = matchesPublic[i].Value;

                var publicAddress = AddressUtil.ParseSocketAddress(publicAddressStr);
                privateToPublicAddresses.Add(new Address(privateAddressStr, publicAddress.Port), publicAddress);
            }
            return privateToPublicAddresses;
        }

        private string ParseErrorResponse(string jsonResult)
        {
            var regexError = new Regex(RegexErrorStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchesError = regexError.Match(jsonResult);
            return matchesError.Value;
        }
    }
}