using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class Heartbeat : IAsyncDisposable
    {
        private readonly HeartbeatOptions _options;
        private readonly TimeSpan _period;
        private readonly TimeSpan _timeout;

        private readonly Cluster _cluster;
        private readonly ILogger _logger;

        private int _active;
        private Task _heartbeating;
        private CancellationTokenSource _cancellation;

        public Heartbeat(Cluster cluster, HeartbeatOptions options, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory?.CreateLogger<Heartbeat>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _period = TimeSpan.FromMilliseconds(options.PeriodMilliseconds);
            _timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _active, 1, 0) == 1)
                throw new InvalidOperationException("Already active.");

            _cancellation = new CancellationTokenSource();
            _heartbeating ??= LoopAsync(_cancellation.LinkedWith(cancellationToken).Token);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _active, 0, 1) == 0)
                return;

            _cancellation.Cancel();

            try
            {
                await _heartbeating;
                _heartbeating = null;
                _cancellation = null;
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            catch (Exception e)
            {
                // unexpected
                _logger.LogWarning(e, "Heartbeat has thrown an exception.");
            }
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(_period, cancellationToken);
                try
                {
                    await RunAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    // RunAsync should *not* throw
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var clients = _cluster.SnapshotClients();
            var now = DateTime.UtcNow;

            _logger.LogDebug("Run heartbeat");

            var tasks = clients
                .Where(x => x.Active)
                .Select(x => RunAsync(x, now, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks);
        }

        private async Task RunAsync(Client client, DateTime now, CancellationToken cancellationToken)
        {
            // must ensure that timeout > interval ?!

            // make sure we read from the client at least every timeout
            // which is greater than the interval, so we *should* have
            // read from the last ping, if nothing else, so no read means
            // that the client is kinda dead - kill it for real
            if (now - client.LastReadTime > _timeout)
            {
                await KillClient(client);
                return;
            }

            // make sure we write to the client at least every interval
            // this should trigger a read when we receive the response
            if (now - client.LastWriteTime > _period)
            {
                _logger.LogDebug("Ping client {ClientId}", client.Id);

                var requestMessage = ClientPingCodec.EncodeRequest();

                // cannot wait forever on a ping
                var timeout = TimeSpan.Zero.AsCancellationTokenSource(_options.PingTimeoutMilliseconds);
                var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

                try
                {
                    var responseMessage = await _cluster.SendToClientAsync(requestMessage, client, cancellation.Token)
                        .OrTimeout(timeout)
                        .ThenDispose(cancellation);

                    // just to be sure everything is ok
                    _ = ClientPingCodec.DecodeResponse(responseMessage);
                }
                catch (TimeoutException)
                {
                    await KillClient(client);
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
            }
        }

        private async Task KillClient(Client client)
        {
            // dead already?
            if (!client.Active) return;

            // kill
            _logger.LogWarning("Heartbeat timeout for client {ClientId}, stopping.", client.Id);
            await client.DieAsync(); // does not throw

            // FIXME: should pass a reason?
            //connection.Close(reason, new TargetDisconnectedException($"Heartbeat timed out to connection {connection}"));
        }
    }
}
