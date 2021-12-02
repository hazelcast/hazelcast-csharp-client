// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
            public static readonly MetricDescriptor<long> LastStatisticsCollectionTime = new MetricDescriptor<long>("lastStatisticsCollectionTime");
            public static readonly MetricDescriptor<bool> Enterprise = new MetricDescriptor<bool>("enterprise");
            public static readonly MetricDescriptor<string> ClientType = new MetricDescriptor<string>("clientType");
            public static readonly MetricDescriptor<string> ClientVersion = new MetricDescriptor<string>("clientVersion");
            public static readonly MetricDescriptor<string> ClientName = new MetricDescriptor<string>("clientName");
            public static readonly MetricDescriptor<long> ClusterConnectionTimestamp = new MetricDescriptor<long>("clusterConnectionTimestamp");
            public static readonly MetricDescriptor<string> ClientAddress = new MetricDescriptor<string>("clientAddress");

            public static class Credentials
            {
                public static readonly MetricDescriptor<string> Principal = new MetricDescriptor<string>("credentials", "principal");
            }

            // ReSharper disable once InconsistentNaming
            public static class OS
            {
                public static readonly MetricDescriptor<long> CommittedVirtualMemorySize = new MetricDescriptor<long>("os", "committedVirtualMemorySize");
                public static readonly MetricDescriptor<long> FreePhysicalMemorySize = new MetricDescriptor<long>("os", "freePhysicalMemorySize");
                public static readonly MetricDescriptor<long> FreeSwapSpaceSize = new MetricDescriptor<long>("os", "freeSwapSpaceSize");
                public static readonly MetricDescriptor<long> MaxFileDescriptorCount = new MetricDescriptor<long>("os", "maxFileDescriptorCount");
                public static readonly MetricDescriptor<long> OpenFileDescriptorCount = new MetricDescriptor<long>("os", "openFileDescriptorCount");
                public static readonly MetricDescriptor<long> ProcessCpuTime = new MetricDescriptor<long>("os", "processCpuTime");
                public static readonly MetricDescriptor<double> SystemLoadAverage = new MetricDescriptor<double>("os", "systemLoadAverage");
                public static readonly MetricDescriptor<long> TotalPhysicalMemorySize = new MetricDescriptor<long>("os", "totalPhysicalMemorySize");
                public static readonly MetricDescriptor<long> TotalSwapSpaceSize = new MetricDescriptor<long>("os", "totalSwapSpaceSize");
            }

            public static class Runtime
            {
                public static readonly MetricDescriptor<int> AvailableProcessors = new MetricDescriptor<int>("runtime", "availableProcessors");
                public static readonly MetricDescriptor<long> FreeMemory = new MetricDescriptor<long>("runtime", "freeMemory");
                public static readonly MetricDescriptor<long> MaxMemory = new MetricDescriptor<long>("runtime", "maxMemory");
                public static readonly MetricDescriptor<long> TotalMemory = new MetricDescriptor<long>("runtimes", "totalMemory");
                public static readonly MetricDescriptor<long> Uptime = new MetricDescriptor<long>("runtime", "uptime");
                public static readonly MetricDescriptor<long> UsedMemory = new MetricDescriptor<long>("runtime", "usedMemory");
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

            yield return MetricDescriptors.Enterprise.WithValue(false);
            yield return MetricDescriptors.ClientType.WithValue("CSHARP");
            yield return MetricDescriptors.ClientVersion.WithValue(ClientVersion.Version);
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
