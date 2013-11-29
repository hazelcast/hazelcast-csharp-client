using System;
using System.IO;
using System.Threading;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Client.Spi;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Client.Connection
{
    public class HeartBeatChecker
    {
        private static readonly ILogger logger = Logger.GetLogger(typeof (HeartBeatChecker));

        private readonly int _connectionTimeout;

        private readonly IClientExecutionService _executionService;

        private readonly Data ping;

        public HeartBeatChecker(int timeout, ISerializationService serializationService,
            IClientExecutionService executionService)
        {
            _connectionTimeout = timeout;
            _executionService = executionService;
            ping = serializationService.ToData(new ClientPingRequest());
        }

        public virtual bool CheckHeartBeat(IConnection connection)
        {
            if ((Clock.CurrentTimeMillis() - connection.GetLastReadTime()) > _connectionTimeout/2)
            {
                var done = new ManualResetEvent(false);

                _executionService.Submit(delegate
                {
                    try
                    {
                        connection.Write(ping);
                        connection.Read();
                        done.Set();
                    }
                    catch (IOException e)
                    {
                        logger.Severe("Error during heartbeat check!", e);
                    }
                });
                try
                {
                    return done.WaitOne(TimeSpan.FromSeconds(5));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
    }
}