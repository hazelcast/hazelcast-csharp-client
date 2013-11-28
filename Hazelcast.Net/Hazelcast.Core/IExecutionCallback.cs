using System;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IExecutionCallback allows to asynchronously get notified when the execution is completed,
    ///     either successfully or with error.
    /// </summary>
    /// <remarks>
    ///     IExecutionCallback allows to asynchronously get notified when the execution is completed,
    ///     either successfully or with error.
    /// </remarks>
    /// <seealso cref="IExecutorService.Submit{T}(Hazelcast.Net.Ext.Callable{V}, IExecutionCallback{V})">
    ///     IExecutorService.Submit
    ///     &lt;T&gt;(Hazelcast.Net.Ext.Callable&lt;V&gt;, IExecutionCallback&lt;V&gt;)
    /// </seealso>
    /// <seealso cref="IExecutorService.SubmitToMember{T}(Hazelcast.Net.Ext.Callable{V}, IMember, IExecutionCallback{V})">
    ///     IExecutorService.SubmitToMember
    ///     &lt;T&gt;(Hazelcast.Net.Ext.Callable&lt;V&gt;, IMember, IExecutionCallback&lt;V&gt;)
    /// </seealso>
    /// <seealso cref="IExecutorService.SubmitToKeyOwner{T}(Hazelcast.Net.Ext.Callable{V}, object, IExecutionCallback{V})">
    ///     IExecutorService.SubmitToKeyOwner
    ///     &lt;T&gt;(Hazelcast.Net.Ext.Callable&lt;V&gt;, object, IExecutionCallback&lt;V&gt;)
    /// </seealso>
    /// <?></?>
    public interface IExecutionCallback<V>
    {
        /// <summary>Called when an execution is completed successfully.</summary>
        /// <remarks>Called when an execution is completed successfully.</remarks>
        /// <param name="response">result of execution</param>
        void OnResponse(V response);

        /// <summary>Called when an execution is completed with an error.</summary>
        /// <remarks>Called when an execution is completed with an error.</remarks>
        /// <param name="t">exception thrown</param>
        void OnFailure(Exception t);
    }
}