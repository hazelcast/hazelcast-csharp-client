using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hazelcast.Client.Spi
{
    public interface IFuture<T>
    {
        Exception Exception { get; }
        T Result { get; }
        T GetResult(int miliseconds);
        bool Wait(int miliseconds);
        bool Wait();
        Task<T> ToTask();
    }
}
