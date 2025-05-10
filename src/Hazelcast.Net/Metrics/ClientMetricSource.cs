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
using System.Collections.Generic;
using System.Diagnostics;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Metrics
{
    // the source of metrics for the client
    internal class ClientMetricSource : IMetricSource
    {
        private readonly Cluster _cluster;
        private readonly ILogger _logger;

        public ClientMetricSource(Cluster cluster, ILoggerFactory loggerFactory)
        {
            _cluster = cluster;
            _logger = loggerFactory.CreateLogger<ClientMetricSource>();
        }

        public static class MetricDescriptors
        {
            public static readonly MetricDescriptor<long> LastStatisticsCollectionTime = MetricDescriptor.Create<long>("lastStatisticsCollectionTime");
            public static readonly MetricDescriptor<bool> Enterprise = MetricDescriptor.Create<bool>("enterprise");
            public static readonly MetricDescriptor<string> ClientType = MetricDescriptor.Create<string>("clientType");
            public static readonly MetricDescriptor<string> ClientVersion = MetricDescriptor.Create<string>("clientVersion");
            public static readonly MetricDescriptor<string> ClientName = MetricDescriptor.Create<string>("clientName");
            public static readonly MetricDescriptor<long> ClusterConnectionTimestamp = MetricDescriptor.Create<long>("clusterConnectionTimestamp");
            public static readonly MetricDescriptor<string> ClientAddress = MetricDescriptor.Create<string>("clientAddress");

            public static class Credentials
            {
                public static readonly MetricDescriptor<string> Principal = MetricDescriptor.Create<string>("credentials", "principal");
            }

            // ReSharper disable once InconsistentNaming
            public static class OS
            {
                public static readonly MetricDescriptor<long> CommittedVirtualMemorySize = MetricDescriptor.Create<long>("os", "committedVirtualMemorySize");
                public static readonly MetricDescriptor<long> FreePhysicalMemorySize = MetricDescriptor.Create<long>("os", "freePhysicalMemorySize");
                public static readonly MetricDescriptor<long> FreeSwapSpaceSize = MetricDescriptor.Create<long>("os", "freeSwapSpaceSize");
                public static readonly MetricDescriptor<long> MaxFileDescriptorCount = MetricDescriptor.Create<long>("os", "maxFileDescriptorCount");
                public static readonly MetricDescriptor<long> OpenFileDescriptorCount = MetricDescriptor.Create<long>("os", "openFileDescriptorCount");
                public static readonly MetricDescriptor<long> ProcessCpuTime = MetricDescriptor.Create<long>("os", "processCpuTime");
                public static readonly MetricDescriptor<double> SystemLoadAverage = MetricDescriptor.Create<double>("os", "systemLoadAverage");
                public static readonly MetricDescriptor<long> TotalPhysicalMemorySize = MetricDescriptor.Create<long>("os", "totalPhysicalMemorySize");
                public static readonly MetricDescriptor<long> TotalSwapSpaceSize = MetricDescriptor.Create<long>("os", "totalSwapSpaceSize");
            }

            public static class Runtime
            {
                public static readonly MetricDescriptor<int> AvailableProcessors = MetricDescriptor.Create<int>("runtime", "availableProcessors", MetricUnit.Count);
                public static readonly MetricDescriptor<long> FreeMemory = MetricDescriptor.Create<long>("runtime", "freeMemory", MetricUnit.Bytes);
                public static readonly MetricDescriptor<long> MaxMemory = MetricDescriptor.Create<long>("runtime", "maxMemory", MetricUnit.Bytes);
                public static readonly MetricDescriptor<long> TotalMemory = MetricDescriptor.Create<long>("runtime", "totalMemory", MetricUnit.Bytes);
                public static readonly MetricDescriptor<long> Uptime = MetricDescriptor.Create<long>("runtime", "uptime");
                public static readonly MetricDescriptor<long> UsedMemory = MetricDescriptor.Create<long>("runtime", "usedMemory", MetricUnit.Bytes);
            }
        }

        public IEnumerable<Metric> PublishMetrics()
        {
            // the Java client gets these from a random connection
            // we try to be more consistent and always pick the oldest active connection
            var connection = _cluster.Members.GetOldestConnection();
            if (connection == null)
            {
                _logger.IfDebug()?.LogDebug("Cannot send metrics, client is not connected.");
                yield break;
            }

            // MUST align with https://github.com/hazelcast/hazelcast/blob/master/hazelcast/src/main/java/com/hazelcast/internal/nio/ConnectionType.java
            const string clientType = "CSP";

            yield return MetricDescriptors.Enterprise.WithValue(false);
            yield return MetricDescriptors.ClientType.WithValue(clientType);
            yield return MetricDescriptors.ClientVersion.WithValue(ClientVersion.GetSemVerWithoutBuildingMetadata());
            yield return MetricDescriptors.ClientName.WithValue(_cluster.ClientName);
            yield return MetricDescriptors.ClusterConnectionTimestamp.WithValue(Clock.ToEpoch(connection.ConnectTime.UtcDateTime)); // TODO: ToEpoch supports DateTimeOffset
            yield return MetricDescriptors.ClientAddress.WithValue(connection.LocalEndPoint.Address.ToString());

            yield return MetricDescriptors.Credentials.Principal.WithValue(connection.Principal);

            yield return MetricDescriptors.OS.CommittedVirtualMemorySize.WithValue(Process.GetCurrentProcess().VirtualMemorySize64);
            yield return MetricDescriptors.OS.FreePhysicalMemorySize.WithoutValue();
            yield return MetricDescriptors.OS.FreeSwapSpaceSize.WithoutValue();
            yield return MetricDescriptors.OS.MaxFileDescriptorCount.WithoutValue();
            yield return MetricDescriptors.OS.OpenFileDescriptorCount.WithValue(Process.GetCurrentProcess().HandleCount);
            yield return MetricDescriptors.OS.ProcessCpuTime.WithValue((long)Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds * 1000000);
            yield return MetricDescriptors.OS.SystemLoadAverage.WithoutValue();
            yield return MetricDescriptors.OS.TotalPhysicalMemorySize.WithoutValue();
            yield return MetricDescriptors.OS.TotalSwapSpaceSize.WithoutValue();

            yield return MetricDescriptors.Runtime.AvailableProcessors.WithValue(Environment.ProcessorCount);
            yield return MetricDescriptors.Runtime.FreeMemory.WithoutValue();
            yield return MetricDescriptors.Runtime.MaxMemory.WithValue(Process.GetCurrentProcess().MaxWorkingSet.ToInt64());
            yield return MetricDescriptors.Runtime.TotalMemory.WithValue(Process.GetCurrentProcess().WorkingSet64);
            yield return MetricDescriptors.Runtime.Uptime.WithValue((long)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds);
            yield return MetricDescriptors.Runtime.UsedMemory.WithValue(Process.GetCurrentProcess().WorkingSet64);
        }
    }
}
