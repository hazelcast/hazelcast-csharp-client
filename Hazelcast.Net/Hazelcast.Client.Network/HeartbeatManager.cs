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
using System.Threading;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Network
{
    internal class HeartbeatManager
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(HeartbeatManager));

        private readonly HazelcastClient _client;
        private readonly ConnectionManager _connectionManager;
        public TimeSpan HeartbeatTimeout { get; }
        private readonly TimeSpan _heartbeatInterval;
        private readonly CancellationTokenSource _heartbeatToken = new CancellationTokenSource();

        public HeartbeatManager(HazelcastClient client, ConnectionManager connectionManager)
        {
            _client = client;
            _connectionManager = connectionManager;

            const int defaultHeartbeatInterval = 5000;
            const int defaultHeartbeatTimeout = 60000;

            var heartbeatTimeoutMillis = EnvironmentUtil.ReadInt("hazelcast.client.heartbeat.timeout") ?? defaultHeartbeatTimeout;
            var heartbeatIntervalMillis =
                EnvironmentUtil.ReadInt("hazelcast.client.heartbeat.interval") ?? defaultHeartbeatInterval;

            HeartbeatTimeout = TimeSpan.FromMilliseconds(heartbeatTimeoutMillis);
            _heartbeatInterval = TimeSpan.FromMilliseconds(heartbeatIntervalMillis);
        }

        public void Start()
        {
            //start Heartbeat
            _client.ExecutionService.ScheduleWithFixedDelay(Heartbeat, _heartbeatInterval, _heartbeatInterval,
                _heartbeatToken.Token);
        }


        private void Heartbeat()
        {
            if (!_connectionManager.IsALive) return;

            var now = DateTime.Now;
            foreach (var connection in _connectionManager.ActiveConnections)
            {
                CheckConnection(now, connection);
            }
        }

        private void CheckConnection(DateTime now, Connection connection)
        {
            if (!connection.IsAlive)
            {
                return;
            }

            if (now - connection.LastRead > HeartbeatTimeout)
            {
                if (connection.IsAlive)
                {
                    Logger.Warning("Heartbeat failed over the connection: " + connection);
                    OnHeartbeatStopped(connection, "Heartbeat timed out");
                }
            }

            if (now - connection.LastWrite > _heartbeatInterval)
            {
                if (Logger.IsFinestEnabled)
                {
                    Logger.Finest("Sending heartbeat to " + connection);
                }
                var request = ClientPingCodec.EncodeRequest();
                _client.InvocationService.InvokeOnConnection(request, connection);
            }
        }

        private void OnHeartbeatStopped(Connection connection, string reason)
        {
            connection.Close(reason, 
                new TargetDisconnectedException($"Heartbeat timed out to connection {connection}"));
        }

        public void Shutdown()
        {
            _heartbeatToken.Cancel();
        }
    }
}