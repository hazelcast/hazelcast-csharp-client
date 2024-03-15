// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Testing.Networking
{
    /// <summary>
    /// Represents a duplex stream.
    /// </summary>
    public class DuplexStream : Stream
    {
        private readonly Stream _input, _output;
        private readonly Action _onDisposed;
        private volatile bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplexStream"/> class.
        /// </summary>
        /// <param name="input">The input pipe, to read from.</param>
        /// <param name="output">The output pipe, to write to.</param>
        /// <param name="onDisposed">An action to run when disposed.</param>
        public DuplexStream(Pipe input, Pipe output, Action onDisposed)
        {
            _input = input.Reader.AsStream();
            _output = output.Writer.AsStream();
            _onDisposed = onDisposed;
        }

        private void EnsureIsOpen()
        {
            if (_disposed) throw new ObjectDisposedException("Stream is closed.");
        }

        /// <inheritdoc />
        public override void Flush()
        { }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        /// <inheritdoc />
        public override void SetLength(long value)
            => throw new NotSupportedException();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureIsOpen();
            return _input.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureIsOpen();
            return _input.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureIsOpen();
            _output.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureIsOpen();
            return _output.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !_disposed)
                {
                    _disposed = true;
                    _onDisposed();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
