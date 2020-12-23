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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Networking
{
    // TODO: better test the web service part

    internal class CloudDiscovery
    {
        // internal const string CloudUrlBaseProperty = "hazelcast.client.cloud.url";
        // internal const string CloudUrlBase = "https://coordinator.hazelcast.cloud";
        private const string CloudUrlPath = "/cluster/discovery?token=";
        private const string RegexErrorStr = "(?<=message\":\").*?(?=\")";
        private const string RegexPrivateStr = "(?<=private-address\":\").*?(?=\")";
        private const string RegexPublicStr = "(?<=public-address\":\").*?(?=\")";

        private readonly ILogger _logger;
        private readonly Uri _endpointUrl;
        private readonly int _connectionTimeoutMilliseconds;
        private readonly int _defaultPort;
        private static string _response;

        internal CloudDiscovery(string discoveryToken, int connectionTimeoutMilliseconds, Uri cloudBaseUrl, int defaultPort, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(discoveryToken)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(discoveryToken));
            if (cloudBaseUrl == null) throw new ArgumentNullException(nameof(cloudBaseUrl));

            _defaultPort = defaultPort;
            _endpointUrl = new Uri(cloudBaseUrl, CloudUrlPath + discoveryToken);
            _connectionTimeoutMilliseconds = connectionTimeoutMilliseconds;
            _logger = loggerFactory?.CreateLogger<CloudDiscovery>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// (internal for tests only) Sets the constant response for tests.
        /// </summary>
        internal static void SetResponse(string response)
        {
            _response = response;
        }


        [ExcludeFromCodeCoverage] // not testing the web connection
        public IDictionary<NetworkAddress, NetworkAddress> Scan()
        {
            if (!string.IsNullOrWhiteSpace(_response)) return ParseResponse(_response);

            try
            {
                // TODO: is this the best way to do an http request?
                var httpWebRequest = (HttpWebRequest) WebRequest.Create(_endpointUrl);
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Timeout = _connectionTimeoutMilliseconds;
                httpWebRequest.ReadWriteTimeout = _connectionTimeoutMilliseconds;
                httpWebRequest.Headers.Set("Accept-Charset", "UTF-8");
                var resp = ReadFromResponse(httpWebRequest.GetResponse());
                return ParseResponse(resp);
            }
            catch (WebException we)
            {
                var resp = ReadFromResponse(we.Response);
                var errorMsg = ParseErrorResponse(resp);
                _logger.LogWarning($"Hazelcast cloud discovery error: '{errorMsg}'.");
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Hazelcast cloud discovery failed.");
            }
            return null;
        }

        [ExcludeFromCodeCoverage] // not testing the web connection
        private static string ReadFromResponse(WebResponse webResponse)
        {
            using var responseStream = webResponse.GetResponseStream();
            using var sr = new StreamReader(responseStream, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        private Dictionary<NetworkAddress, NetworkAddress> ParseResponse(string jsonResult)
        {
            var regexPrivate = new Regex(RegexPrivateStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regexPublic = new Regex(RegexPublicStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchesPrivate = regexPrivate.Matches(jsonResult);
            var matchesPublic = regexPublic.Matches(jsonResult);

            var privateToPublicAddresses = new Dictionary<NetworkAddress, NetworkAddress>();
            for (var i = 0; i < matchesPrivate.Count; i++)
            {
                var privateAddressStr = matchesPrivate[i].Value;
                var publicAddressStr = matchesPublic[i].Value;

                var publicAddress = NetworkAddress.Parse(publicAddressStr);
                if (publicAddress.Port == 0)
                    publicAddress = publicAddress.WithPort(_defaultPort);

                var privateAddress = NetworkAddress.Parse(privateAddressStr);
                if (privateAddress.Port == 0)
                    privateAddress = privateAddress.WithPort(publicAddress.Port);

                privateToPublicAddresses.Add(privateAddress, publicAddress);
            }
            return privateToPublicAddresses;
        }

        [ExcludeFromCodeCoverage] // not testing the web connection
        private static string ParseErrorResponse(string jsonResult)
        {
            var regexError = new Regex(RegexErrorStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchesError = regexError.Match(jsonResult);
            return matchesError.Value;
        }
    }
}
