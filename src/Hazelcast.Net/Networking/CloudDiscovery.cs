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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Hazelcast.Core;
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

        private readonly ILogger _logger;
        private readonly Uri _endpointUrl;
        private readonly int _connectionTimeoutMilliseconds;
        private readonly int _defaultPort;
        private static string _response;
        private readonly JsonSerializerOptions jsonOptions = new System.Text.Json.JsonSerializerOptions { AllowTrailingCommas = true };

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
        public CloudInfo Scan()
        {
            if (!string.IsNullOrWhiteSpace(_response)) return ParseResponse(_response);

            try
            {
                // TODO: It needs to be refactored. We cannot make it async now due to refactoring of
                // the callers of the Scan. The caller chain should be all async. The method can be
                // refactored as async and using HttpClient/HttpClientFactory.
#pragma warning disable SYSLIB0014
#pragma warning disable SYSLIB0000
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(_endpointUrl);
#pragma warning restore SYSLIB0000
#pragma warning restore SYSLIB0014
                httpWebRequest.Method = WebRequestMethods.Http.Get;
                httpWebRequest.Timeout = _connectionTimeoutMilliseconds;
                httpWebRequest.ReadWriteTimeout = _connectionTimeoutMilliseconds;
                httpWebRequest.Headers.Set("Accept-Charset", "UTF-8");
                var resp = ReadFromResponse(httpWebRequest.GetResponse());
                _logger.IfDebug()?.LogDebug("CloudInfo: " + resp);
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

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CloudMemberInfo
        {
            [System.Text.Json.Serialization.JsonPropertyName("private-address")]
            public string PrivateAddress { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("public-address")]
            public string PublicAddress { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("tpc-ports")]
            public CloudTpcInfo[] TpcPorts { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CloudTpcInfo
        {
            [System.Text.Json.Serialization.JsonPropertyName("private-port")]
            public int PrivatePort { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("public-port")]
            public int PublicPort { get; set; }
        }

        private CloudInfo ParseResponse(string jsonResult)
        {            
            var result = System.Text.Json.JsonSerializer.Deserialize<CloudMemberInfo[]>(jsonResult, jsonOptions);

            var cloudInfo = new CloudInfo();

            foreach (var cloudMemberInfo in result)
            {
                var publicAddress = NetworkAddress.Parse(cloudMemberInfo.PublicAddress);
                if (publicAddress.Port == 0) publicAddress = publicAddress.WithPort(_defaultPort);

                var privateAddress = NetworkAddress.Parse(cloudMemberInfo.PrivateAddress);
                if (privateAddress.Port == 0) privateAddress = privateAddress.WithPort(publicAddress.Port);

                cloudInfo.PrivateToPublicAddresses[privateAddress] = publicAddress;
                cloudInfo.ClassicAddresses.Add(publicAddress);

                if (cloudMemberInfo.TpcPorts != null)
                {
                    foreach (var tpcPortInfo in cloudMemberInfo.TpcPorts)
                    {
                        var privateTpcAddress = privateAddress.WithPort(tpcPortInfo.PrivatePort);
                        var publicTpcAddress = publicAddress.WithPort(tpcPortInfo.PublicPort);
                        cloudInfo.PrivateToPublicAddresses[privateTpcAddress] = publicTpcAddress;
                    }
                }
            }

            return cloudInfo;
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
