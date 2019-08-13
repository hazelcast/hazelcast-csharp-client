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
using System.Diagnostics;
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Client
{
    internal class Statistics
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(Statistics));

        private static readonly string DllVersion = VersionUtil.GetDllVersion();

        private const string NearCacheCategoryPrefix = "nc.";

        private const string SinceVersionString = "3.9";
        private static readonly int SinceVersion = VersionUtil.ParseServerVersion(SinceVersionString);

        private const char StatSeparator = ',';
        private const char KeyValueSeparator = '=';
        private const char EscapeChar = '\\';
        private const string EmptyStatValue = "";

        private readonly HazelcastClient _client;
        private readonly bool _enabled;
        private readonly TimeSpan _period;
        private CancellationTokenSource _heartbeatToken;

        private readonly AtomicBoolean _live = new AtomicBoolean(false);

        private readonly IDictionary<string, Func<string>> _gauges = new Dictionary<string, Func<string>>();

        private volatile Address _ownerAddress;

        public Statistics(HazelcastClient client)
        {
            _client = client;
            _enabled = EnvironmentUtil.ReadBool("hazelcast.client.statistics.enabled") ?? false;
            _period = TimeSpan.FromSeconds(EnvironmentUtil.ReadInt("hazelcast.client.statistics.period.seconds") ?? 3);
        }

        public void Start()
        {
            if (!_enabled || !_live.CompareAndSet(false, true))
            {
                return;
            }

            RegisterMetrics();

            _heartbeatToken = new CancellationTokenSource();

            _client.GetClientExecutionService().ScheduleWithFixedDelay(PeriodicStatisticsSendTask, _period, _period, _heartbeatToken.Token);

            Logger.Info($"Client statistics is enabled with period {_period} seconds.");
        }

        public void Destroy()
        {
            if (!_live.CompareAndSet(true, false))
            {
                return;
            }

            _heartbeatToken.Cancel();
        }

        private ClientConnection GetOwnerConnection()
        {
            var ownerAddress = _client.GetClientClusterService().GetOwnerConnectionAddress();
            var clientConnection = _client.GetConnectionManager().GetConnection(ownerAddress);
            if (clientConnection == null)
            {
                return null;
            }

            if (clientConnection.ConnectedServerVersionInt >= SinceVersion)
            {
                return clientConnection;
            }

            // do not print too many logs if connected to an old version server
            if (_ownerAddress == null || !_ownerAddress.Equals(ownerAddress))
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest($"Client statistics can not be sent to server {ownerAddress} since, " +
                                  $"connected owner server version is less than the minimum supported server version {SinceVersionString}");
                }
            }

            // cache the last connected server address for decreasing the log prints
            _ownerAddress = ownerAddress;
            return null;
        }

        private void PeriodicStatisticsSendTask()
        {
            var ownerConnection = GetOwnerConnection();
            if (null == ownerConnection)
            {
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Can not send client statistics to the server. No owner connection.");
                }

                return;
            }

            var stats = new StringBuilder();
            try
            {
                FillMetrics(stats, ownerConnection);
                AddNearCacheStats(stats);
            }
            catch (Exception e)
            {
                Logger.Finest("Can not collect client statistics.", e);
            }

            SendStatsToOwner(stats);
        }

        private void SendStatsToOwner(StringBuilder stats)
        {
            var request = ClientStatisticsCodec.EncodeRequest(stats.ToString());
            try
            {
                _client.GetInvocationService().InvokeOnTarget(request, _ownerAddress);
            }
            catch (Exception e)
            {
                // suppress exception, do not print too many messages
                if (Logger.IsFinestEnabled())
                {
                    Logger.Finest("Could not send stats ", e);
                }
            }
        }

        private void RegisterMetrics()
        {
            RegisterGauge("os.committedVirtualMemorySize",
                () => Process.GetCurrentProcess().VirtualMemorySize64.ToString());
            RegisterGauge("os.freePhysicalMemorySize", () => EmptyStatValue);
            RegisterGauge("os.freeSwapSpaceSize", () => EmptyStatValue);
            RegisterGauge("os.maxFileDescriptorCount", () => EmptyStatValue);
            RegisterGauge("os.openFileDescriptorCount", () => Process.GetCurrentProcess().HandleCount.ToString());
            RegisterGauge("os.processCpuTime", () =>
            {
                var totalMilliSec = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
                return ((long) totalMilliSec * 1000000).ToString();
            });
            RegisterGauge("os.systemLoadAverage", () => EmptyStatValue); //double value
            RegisterGauge("os.totalPhysicalMemorySize", () => EmptyStatValue);
            RegisterGauge("os.totalSwapSpaceSize", () => EmptyStatValue);

            RegisterGauge("runtime.availableProcessors", () => Environment.ProcessorCount.ToString());
            RegisterGauge("runtime.freeMemory", () => EmptyStatValue);
            RegisterGauge("runtime.maxMemory", () => Process.GetCurrentProcess().MaxWorkingSet.ToInt64().ToString());
            RegisterGauge("runtime.totalMemory", () => Process.GetCurrentProcess().WorkingSet64.ToString());
            RegisterGauge("runtime.uptime",
                () => ((long) (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds).ToString());
            RegisterGauge("runtime.usedMemory", () => Process.GetCurrentProcess().WorkingSet64.ToString());
            RegisterGauge("executionService.userExecutorQueueSize", () => EmptyStatValue);
        }

        private void RegisterGauge(string gaugeName, Func<string> gaugeFunc)
        {
            try
            {
                //try a gauge function read, we will register it if it succeed . 
                gaugeFunc();
                _gauges.Add(gaugeName, gaugeFunc);
            }
            catch (Exception e)
            {
                Logger.Warning(
                    string.Format("Could not collect data for gauge {0} , it won't be registered", gaugeName), e);
                _gauges.Add(gaugeName, () => EmptyStatValue);
            }
        }

        private void FillMetrics(StringBuilder stats, ClientConnection ownerConnection)
        {
            AddStat(stats, "lastStatisticsCollectionTime", Clock.CurrentTimeMillis());
            AddStat(stats, "enterprise", "false");
            AddStat(stats, "clientType", "CSHARP");
            AddStat(stats, "clientVersion", DllVersion);
            AddStat(stats, "clusterConnectionTimestamp", ownerConnection.ConnectionStartTime);
            AddStat(stats, "clientAddress", ownerConnection.GetLocalSocketAddress());
            AddStat(stats, "clientName", _client.GetName());

            var credentials = _client.GetConnectionManager().LastCredentials;
            AddStat(stats, "credentials.principal", credentials.GetPrincipal());

            foreach (var pair in _gauges)
            {
                var gaugeName = pair.Key;
                var gaugeValueFunc = pair.Value;
                try
                {
                    var value = gaugeValueFunc();
                    AddStat(stats, gaugeName, value);
                }
                catch (Exception e)
                {
                    if (Logger.IsFinestEnabled())
                    {
                        Logger.Finest(string.Format("Could not collect data for gauge {0}", gaugeName), e);
                    }
                }
            }
        }

        private void AddNearCacheStats(StringBuilder stats)
        {
            foreach (var nearCache in _client.GetNearCacheManager().GetAllNearCaches())
            {
                var nearCacheNameWithPrefix = GetNameWithPrefix(nearCache.Name);
                nearCacheNameWithPrefix.Append('.');
                var nearCacheStats = nearCache.NearCacheStatistics;
                var prefix = nearCacheNameWithPrefix.ToString();
                AddStat(stats, prefix, "creationTime", nearCacheStats.CreationTime);
                AddStat(stats, prefix, "evictions", nearCacheStats.Evictions);
                AddStat(stats, prefix, "hits", nearCacheStats.Hits);
                AddEmptyStat(stats, prefix, "lastPersistenceDuration");
                AddEmptyStat(stats, prefix, "lastPersistenceKeyCount");
                AddEmptyStat(stats, prefix, "lastPersistenceTime");
                AddEmptyStat(stats, prefix, "lastPersistenceWrittenBytes");
                AddStat(stats, prefix, "misses", nearCacheStats.Misses);
                AddStat(stats, prefix, "ownedEntryCount", nearCacheStats.OwnedEntryCount);
                AddStat(stats, prefix, "expirations", nearCacheStats.Expirations);
                AddEmptyStat(stats, prefix, "ownedEntryMemoryCost");
                AddEmptyStat(stats, prefix, "lastPersistenceFailure");
            }
        }

        private static void AddEmptyStat(StringBuilder stats, string keyPrefix, string name)
        {
            AddStat(stats, keyPrefix, name, EmptyStatValue);
        }

        private static void AddStat<T>(StringBuilder stats, string name, T value)
        {
            AddStat(stats, null, name, value);
        }

        private static void AddStat<T>(StringBuilder stats, string keyPrefix, string name, T value)
        {
            if (stats.Length != 0)
            {
                stats.Append(StatSeparator);
            }

            if (null != keyPrefix)
            {
                stats.Append(keyPrefix);
            }

            stats.Append(name).Append(KeyValueSeparator).Append(value);
        }

        private static StringBuilder GetNameWithPrefix(string name)
        {
            var escapedName = new StringBuilder(NearCacheCategoryPrefix);
            var prefixLen = NearCacheCategoryPrefix.Length;
            escapedName.Append(name);
            if (escapedName[prefixLen] == '/')
            {
                escapedName.Remove(prefixLen, 1);
            }

            EscapeSpecialCharacters(escapedName, prefixLen);
            return escapedName;
        }

        private static void EscapeSpecialCharacters(StringBuilder buffer, int start)
        {
            for (var i = start; i < buffer.Length; ++i)
            {
                char c = buffer[i];
                if (c == '=' || c == '.' || c == ',' || c == EscapeChar)
                {
                    buffer.Insert(i, EscapeChar);
                    ++i;
                }
            }
        }
    }
}