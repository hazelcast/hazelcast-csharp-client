// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Networking;
using Hazelcast.Testing.TestServer;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class SocketConnectionTests
    {
        [Test]
        public async Task Exceptions()
        {
            var endpoint = NetworkAddress.Parse("127.0.0.1:5701").IPEndPoint;
            var options = new NetworkingOptions();

            await using var connection = new ClientSocketConnection(Guid.NewGuid(), endpoint, options, new SslOptions(), new NullLoggerFactory(), 3);

            // OnReceiveMessageBytes is missing
            Assert.ThrowsAsync<InvalidOperationException>(async () => await connection.ConnectAsync(default));

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            connection.OnReceiveMessageBytes = (x, y) => true;

            // OnReceivePrefixBytes is missing
            Assert.ThrowsAsync<InvalidOperationException>(async () => await connection.ConnectAsync(default));

            await using var connection2 = new ClientSocketConnection(Guid.NewGuid(), endpoint, options, new SslOptions(), new NullLoggerFactory())
            {
                OnReceiveMessageBytes = (x, y) => true
            };

            await connection2.ConnectAsync(default);

            Assert.Throws<InvalidOperationException>(() => connection2.OnReceiveMessageBytes = (x, y) => true);
            Assert.Throws<InvalidOperationException>(() => connection2.OnReceivePrefixBytes = (x, y) => new ValueTask());
            Assert.Throws<InvalidOperationException>(() => connection2.OnShutdown = x => default);
        }

        [Test]
        public async Task ConnectAsyncSuccess()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");
            var options = new NetworkingOptions();

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            // OnReceiveMessageBytes = (x, y) => true
            Func<SocketConnectionBase, IBufferReference<ReadOnlySequence<byte>>, bool> onReceiveMessageBytes
                = (x, y) => true;

            await using var socket = new ClientSocketConnection(Guid.NewGuid(), endpoint, options, new SslOptions(), new NullLoggerFactory())
            {
                OnReceiveMessageBytes = onReceiveMessageBytes
            };

            Assert.That(socket.OnReceiveMessageBytes, Is.SameAs(onReceiveMessageBytes));

            Func<SocketConnectionBase, ReadOnlySequence<byte>, ValueTask> onReceivePrefixBytes
                = (s, b) => default;
            socket.OnReceivePrefixBytes = onReceivePrefixBytes;
            Assert.That(socket.OnReceivePrefixBytes, Is.SameAs(onReceivePrefixBytes));

            Func<SocketConnectionBase, ValueTask> onShutdown = s => default;
            socket.OnShutdown = onShutdown;
            Assert.That(socket.OnShutdown, Is.SameAs(onShutdown));

            await socket.ConnectAsync(default);
            // connected!

            var remoteEndpoint = socket.RemoteEndPoint;
            Assert.That(remoteEndpoint.ToString(), Does.StartWith("127.0.0.1:"));

            // once connected, there are things we can't do anymore

            Assert.Throws<InvalidOperationException>(() =>
            {
                socket.OnReceiveMessageBytes = (s, b) => default;
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                socket.OnReceivePrefixBytes = (s, b) => default;
            });
        }

        [Test]
        public async Task ConnectAsyncSuccessDontLinger()
        {
            var endpoint = IPEndPointEx.Parse("127.0.0.1:11000");
            var options = new NetworkingOptions();
            options.Socket.LingerSeconds = 0;

            using var server = new SocketListener(endpoint, SocketListenerMode.AcceptOnce);

            await using var socket = new ClientSocketConnection(Guid.NewGuid(), endpoint, options, new SslOptions(), new NullLoggerFactory())
            {
                OnReceiveMessageBytes = (x, y) => true
            };
            await socket.ConnectAsync(default);
            // connected!
        }

        [Test]
        public async Task SendReceive()
        {
            //using var console = HConsoleForTest();

            var now = DateTime.Now;
            await Task.Delay(100);

            var address = NetworkAddress.Parse("127.0.0.1").WithTestEndPointPort();

            await using var server = new Server(address)
                .HandleFallback(async request =>
                {
                    await Task.Delay(200).CfAwait();
                    await request.Connection.SendAsync(new ClientMessage(new Frame(new byte[64]))).CfAwait();
                });

            await server.StartAsync();

            await using var socket = new ClientSocketConnection(Guid.NewGuid(), address.IPEndPoint, new NetworkingOptions(), new SslOptions(), NullLoggerFactory.Instance);
            var m = new ClientMessageConnection(socket, NullLoggerFactory.Instance);
            var s = new SemaphoreSlim(0, 1);
            m.OnReceiveMessage += (_, _) =>
            {
                s.Release();
            };
            await socket.ConnectAsync(default);

            await Task.Delay(100);
            await socket.SendAsync(MemberConnection.ClientProtocolInitBytes, MemberConnection.ClientProtocolInitBytes.Length).CfAwait();
            await m.SendAsync(new ClientMessage(new Frame(new byte[64], (FrameFlags) ClientMessageFlags.Unfragmented))).CfAwait();
            await s.WaitAsync(TimeSpan.FromSeconds(10)).CfAwait(); // wait for reply

            Assert.That(socket.CreateTime, Is.GreaterThan(now));
            Assert.That(socket.LastWriteTime, Is.GreaterThan(socket.CreateTime));
            Assert.That(socket.LastReadTime, Is.GreaterThan(socket.LastWriteTime));
        }

        [Test]
        public void ReadPipeState()
        {
            var state = new SocketConnectionBase.ReadPipeState();

            var e = new Exception("bang");
            state.CaptureExceptionAndFail(e);

            state.CaptureExceptionAndFail(new Exception("another"));

            Assert.That(state.Failed);
            Assert.That(state.Exception, Is.Not.Null);
            Assert.That(state.Exception.SourceException, Is.SameAs(e));
        }

        [Test]
        public async Task MoreExceptions()
        {
            HConsole.Configure(options => options.ConfigureDefaults(this));

            // note: a MemoryStream does not block, so connection immediately closes
            // use a pipe stream instead
            var pipe = new MemoryPipe();

            var s = new TestSocketConnection(new StreamWrapper(new MemoryStream()) { OnWrite = StreamWrapper.Throw })
            {
                OnReceiveMessageBytes = (s, r) => false
            };

            // not active => false
            var sent = await s.SendAsync(new byte[16], 16);
            Assert.That(sent, Is.False);

            s = new TestSocketConnection(new StreamWrapper(pipe.Stream1) { OnWrite = StreamWrapper.Throw })
            {
                OnReceiveMessageBytes = (s, r) => false
            };
            await s.ConnectAsync();

            // test stream in test socket throws on WriteAsync => we get 'false'
            sent = await s.SendAsync(new byte[16], 16);
            Assert.That(sent, Is.False);

            s = new TestSocketConnection(new StreamWrapper(pipe.Stream1))
            {
                OnReceiveMessageBytes = (s, r) => false
            };
            await s.ConnectAsync();

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await s.WritePipeThrowsArgumentNull1());
            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await s.WritePipeThrowsArgumentNull2());
            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await s.ReadPipeThrowsArgumentNull());

            var down = 0;
            s = new TestSocketConnection(new StreamWrapper(pipe.Stream1) { OnRead = StreamWrapper.Throw })
            {
                OnReceiveMessageBytes = (s, r) => false,
                OnShutdown = s =>
                {
                    down++;
                    return default;
                }
            };
            await s.ConnectAsync();

            // throwing when reading will bring the connection down eventually
            await AssertEx.SucceedsEventually(() => Assert.That(down, Is.GreaterThan(0)), 4000, 200);
        }

        [Test]
        public async Task CompletedWritePipe()
        {
            //using var _ = EnableHConsoleForTest();

            var pipe = new MemoryPipe();
            var count = 0L;

            var s = new TestSocketConnection(pipe.Stream1)
            {
                OnReceiveMessageBytes = (s, r) =>
                {
                    count += r.Buffer.Length;
                    var slice = r.Buffer.Slice(r.Buffer.Start, r.Buffer.Length);
                    r.Buffer = r.Buffer.Slice(slice.End);
                    return false;
                }
            };

            HConsole.WriteLine(this, "Connect socket.");
            await s.ConnectAsync();

            HConsole.WriteLine(this, "Write to stream.");
            await pipe.Stream2.WriteAsync(new byte[16], 0, 16);

            for (var i = 0; i < 10 && count != 16; i++)
                await Task.Delay(200);

            Assert.That(count, Is.EqualTo(16));

            // code below will make sure we hit 'if (result.IsCompleted)' in WritePipeAsync

            HConsole.WriteLine(this, "Break, and write to stream.");
            await s.Pipe.Reader.CompleteAsync();
            await pipe.Stream2.WriteAsync(new byte[16], 0, 16);

            await Task.Delay(200);
            HConsole.WriteLine(this, "Dispose.");
            await s.DisposeAsync();
        }

        [Test]
        public async Task ReadPipeLoop_1()
        {
            //using var _ = EnableHConsoleForTest();

            var pipe = new MemoryPipe();
            var count = 0L;
            var down = 0;

            var s = new TestSocketConnection(pipe.Stream1, 4)
            {
                OnReceiveMessageBytes = (s, r) =>
                {
                    count += r.Buffer.Length;
                    var slice = r.Buffer.Slice(r.Buffer.Start, r.Buffer.Length);
                    r.Buffer = r.Buffer.Slice(slice.End);
                    return false;
                },
                OnReceivePrefixBytes = (s, r) => throw new Exception("bang"),
                OnShutdown = s =>
                {
                    down++;
                    return default;
                }
            };

            HConsole.WriteLine(this, "Connect socket.");
            await s.ConnectAsync();

            // send some bytes (but not enough)
            await pipe.Stream2.WriteAsync(new byte[2], 0, 2);

            // give it some time else the 2+2 bytes become 4 bytes
            await Task.Delay(500);

            // send enough bytes (will throw)
            await pipe.Stream2.WriteAsync(new byte[2], 0, 2);

            // throwing brings the connection down eventually
            await AssertEx.SucceedsEventually(() => Assert.That(down, Is.GreaterThan(0)), 4000, 200);
        }

        [Test]
        public async Task ReadPipeLoop_2()
        {
            //using var _ = EnableHConsoleForTest();

            var pipe = new MemoryPipe();
            var down = 0;

            var s = new TestSocketConnection(pipe.Stream1)
            {
                OnReceiveMessageBytes = (s, r) => throw new Exception("bang"),
                OnShutdown = s =>
                {
                    down++;
                    return default;
                }
            };

            HConsole.WriteLine(this, "Connect socket.");
            await s.ConnectAsync();

            // send some bytes (will throw)
            await pipe.Stream2.WriteAsync(new byte[2], 0, 2);

            // throwing brings the connection down eventually
            await AssertEx.SucceedsEventually(() => Assert.That(down, Is.GreaterThan(0)), 4000, 200);
        }

        [Test]
        [KnownIssue(0, "Breaks on GitHub Actions")]
        public async Task ReadPipeLoop_3()
        {
            //using var _ = EnableHConsoleForTest();

            var pipe = new MemoryPipe();
            var down = 0;

            TestSocketConnection s = null;
            s = new TestSocketConnection(pipe.Stream1)
            {
                OnReceiveMessageBytes = (sc, r) =>
                {
                    s.Pipe.Writer.Complete();
                    return false;
                },
                OnShutdown = sc =>
                {
                    down++;
                    return default;
                }
            };

            HConsole.WriteLine(this, "Connect socket.");
            await s.ConnectAsync();

            // send some bytes
            await pipe.Stream2.WriteAsync(new byte[2], 0, 2);

            // throwing brings the connection down eventually
            await AssertEx.SucceedsEventually(() => Assert.That(down, Is.GreaterThan(0)), 30000, 200);
        }

        [Test]
        public async Task DisposeExceptions()
        {
            var pipe = new MemoryPipe();

            TestSocketConnection s = null;
            s = new TestSocketConnection(new StreamWrapper(pipe.Stream1) { OnDispose = w =>
            {
                s.StreamReadCancellationTokenSource.Dispose();
                StreamWrapper.Throw(w);
            }})
            {
                OnReceiveMessageBytes = (sc, r) => false
            };

            // disposing the socket connection should not throw even though disposing the inner stream throws
            await s.ConnectAsync();

            // and because the stream wrapper's been disposed once it won't be disposed again so this is safe
            await s.DisposeAsync();
        }

        [Test]
        public void OkToDisposeCancellationSourceAgain()
        {
            var cancellation = new CancellationTokenSource();
            cancellation.Dispose();
            cancellation.Dispose();
        }

        private class StreamWrapper : Stream
        {
            private readonly Stream _stream;

            public StreamWrapper(Stream stream)
            {
                _stream = stream;
            }

            public Action<StreamWrapper> OnWrite { get; set; }

            public Action<StreamWrapper> OnRead { get; set; }

            public Action<StreamWrapper> OnDispose { get; set; }

            public bool Disposed { get; set; }

            public static Action<StreamWrapper> Throw { get; } = _ => throw new Exception("bang");

            public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

            public override void SetLength(long value) => _stream.SetLength(value);

            public override int Read(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override long Position
            {
                get => _stream.Position;
                set => _stream.Position = value;
            }

            public override void Flush() => _stream.Flush();

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Yield();
                OnRead?.Invoke(this);
                return await _stream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Yield();
                OnWrite?.Invoke(this);
                await _stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (Disposed) return;
                Disposed = true;

                try
                {
                    OnDispose?.Invoke(this);
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        private class TestSocketConnection : SocketConnectionBase
        {
            private readonly Stream _stream;

            public TestSocketConnection(Stream stream, int prefixLength = 0)
                : base(Guid.NewGuid(), prefixLength)
            {
                _stream = stream;
            }

            public ValueTask ConnectAsync()
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EnsureCanOpenPipe();
                OpenPipe(socket, _stream);
                return default;
            }

            public new System.IO.Pipelines.Pipe Pipe => base.Pipe;

            public new CancellationTokenSource StreamReadCancellationTokenSource => base.StreamReadCancellationTokenSource;

            public async Task WritePipeThrowsArgumentNull1() => await WritePipeAsync(null, null);
            public async Task WritePipeThrowsArgumentNull2() => await WritePipeAsync(new MemoryStream(), null);
            public async Task ReadPipeThrowsArgumentNull() => await ReadPipeAsync(null);
        }
    }
}
