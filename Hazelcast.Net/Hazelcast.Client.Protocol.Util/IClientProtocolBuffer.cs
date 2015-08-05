namespace Hazelcast.Client.Protocol.Util
{
    /// <summary>Interface for buffer to be used in client protocol.</summary>
    /// <remarks>
    ///     Interface for buffer to be used in client protocol.
    ///     Implemented by
    ///     <see cref="SafeBuffer" />
    ///     and
    ///     <see cref="UnsafeBuffer" />
    /// </remarks>
    public interface IClientProtocolBuffer
    {
        /// <summary>Attach a view to a byte[] for providing direct access.</summary>
        /// <param name="buffer">to which the view is attached.</param>
        void Wrap(byte[] buffer);

        /// <summary>Get the underlying byte[] if one exists.</summary>
        /// <returns>the underlying byte[] if one exists.</returns>
        byte[] ByteArray();

        /// <summary>Get the capacity of the underlying buffer.</summary>
        /// <returns>the capacity of the underlying buffer in bytes.</returns>
        int Capacity();

        /// <summary>Get the value at a given index.</summary>
        /// <param name="index">in bytes from which to get.</param>
        /// <returns>the value for at a given index</returns>
        long GetLong(int index);

        /// <summary>Get the value at a given index.</summary>
        /// <param name="index">in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        int GetInt(int index);

        /// <summary>Get the value at a given index.</summary>
        /// <param name="index">in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        short GetShort(int index);

        /// <summary>Get the value at a given index.</summary>
        /// <param name="index">in bytes from which to get.</param>
        /// <returns>the value at a given index.</returns>
        byte GetByte(int index);

        /// <summary>Get from the underlying buffer into a supplied byte array.</summary>
        /// <remarks>
        ///     Get from the underlying buffer into a supplied byte array.
        ///     This method will try to fill the supplied byte array.
        /// </remarks>
        /// <param name="index">in the underlying buffer to start from.</param>
        /// <param name="dst">into which the dst will be copied.</param>
        void GetBytes(int index, byte[] dst);

        /// <summary>Get bytes from the underlying buffer into a supplied byte array.</summary>
        /// <param name="index">in the underlying buffer to start from.</param>
        /// <param name="dst">into which the bytes will be copied.</param>
        /// <param name="offset">in the supplied buffer to start the copy</param>
        /// <param name="length">of the supplied buffer to use.</param>
        void GetBytes(int index, byte[] dst, int offset, int length);

        /// <summary>Get part of String from bytes encoded in UTF-8 format without a length prefix.</summary>
        /// <param name="offset">at which the String begins.</param>
        /// <param name="length">of the String in bytes to decode.</param>
        /// <returns>the String as represented by the UTF-8 encoded bytes.</returns>
        string GetStringUtf8(int offset, int length);

        /// <summary>Put a value at a given index.</summary>
        /// <param name="index">The index in bytes where the value is put.</param>
        /// <param name="value">The value to put at the given index.</param>
        void PutLong(int index, long value);

        /// <summary>Put a value at a given index.</summary>
        /// <param name="index">The index in bytes where the value is put.</param>
        /// <param name="value">The value put at the given index.</param>
        void PutInt(int index, int value);

        /// <summary>Put a value to a given index.</summary>
        /// <param name="index">The index in bytes where the value is put.</param>
        /// <param name="value">The value put at the given index.</param>
        void PutShort(int index, short value);

        /// <summary>Put a value to a given index.</summary>
        /// <param name="index">The index in bytes where the value is put.</param>
        /// <param name="value">The value put at the given index.</param>
        void PutByte(int index, byte value);

        /// <summary>Put an array of src into the underlying buffer.</summary>
        /// <param name="index">The index in the underlying buffer from which to start the array.</param>
        /// <param name="src">The array to be copied into the underlying buffer.</param>
        void PutBytes(int index, byte[] src);

        /// <summary>Put an array into the underlying buffer.</summary>
        /// <param name="index">The index in the underlying buffer from which to start the array.</param>
        /// <param name="src">The array to be copied into the underlying buffer.</param>
        /// <param name="offset">The offset in the supplied buffer at which to begin the copy.</param>
        /// <param name="length">The length of the supplied buffer to copy.</param>
        void PutBytes(int index, byte[] src, int offset, int length);

        /// <summary>Encode a String as UTF-8 bytes to the buffer with a length prefix.</summary>
        /// <param name="index">The index at which the String should be encoded.</param>
        /// <param name="value">The value of the String to be encoded.</param>
        /// <returns>The number of bytes put to the buffer.</returns>
        int PutStringUtf8(int index, string value);

        /// <summary>Encode a String as UTF-8 bytes the buffer with a length prefix with a maximum encoded size check.</summary>
        /// <param name="index">The index at which the String should be encoded.</param>
        /// <param name="value">The value of the String to be encoded.</param>
        /// <param name="maxEncodedSize">The maximum encoded size to be checked before writing to the buffer.</param>
        /// <returns>The number of bytes put to the buffer.</returns>
        /// <exception cref="System.ArgumentException">if the encoded bytes are greater than maxEncodedSize.</exception>
        int PutStringUtf8(int index, string value, int maxEncodedSize);
    }
}