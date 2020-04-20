using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Serialization
{
    internal interface IInputStream
    {
        int Available();

        void Close();

        void Mark(int readlimit);

        bool MarkSupported();
        int Read();

        int Read(byte[] b);

        int Read(byte[] b, int off, int len);

        void Reset();

        long Skip(long n);
    }
}
