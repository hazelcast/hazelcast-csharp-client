using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
#if NETSTANDARD2_0
    /// <summary>
    /// Provides a mechanism for releasing unmanaged resources asynchronously.
    /// </summary>
    /// <remarks>
    /// <para>This class is a substitute for the true IAsyncDisposable which is
    /// only available in C# 8 and netstandard 2.1. Though it allows to asynchronously
    /// dispose resources, asynchronous using and other C# 8 features are of
    /// course not supported in netstandard 2.0.</para>
    /// </remarks>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        ValueTask DisposeAsync();
    }
#endif
}
