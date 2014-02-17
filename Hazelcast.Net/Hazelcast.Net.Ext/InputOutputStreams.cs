namespace Hazelcast.Net.Ext
{
    internal interface InputStream
    {
        int Read();

        int Read(byte[] b);

        int Read(byte[] b, int off, int len);

        long Skip(long n);

        int Available();

        void Close();

        void Mark(int readlimit);

        void Reset();

        bool MarkSupported();
    }

    internal interface OutputStream
    {
        void Write(int b);

        void Write(byte[] b);

        void Write(byte[] b, int off, int len);

        void Flush();

        void Close();
    }
}