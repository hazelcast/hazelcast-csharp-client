using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IMultiExecutionCallback allows to get notified when an execution is completed on each member
    ///     which task is submitted to.
    /// </summary>
    /// <remarks>
    ///     IMultiExecutionCallback allows to get notified when an execution is completed on each member
    ///     which task is submitted to. After all executions are completed on all submitted members,
    ///     <see cref="OnComplete(System.Collections.Generic.IDictionary{K, V})">
    ///         OnComplete(System.Collections.Generic.IDictionary
    ///         &lt;K, V&gt;)
    ///     </see>
    ///     method is called with map of all results.
    /// </remarks>
    /// <seealso cref="IExecutorService">IExecutorService</seealso>
    /// <seealso cref="IIExecutionCallback{V}">IExecutionCallback&lt;V&gt;</seealso>
    public interface IMultiExecutionCallback
    {
        /// <summary>Called when an execution is completed on a member.</summary>
        /// <remarks>Called when an execution is completed on a member.</remarks>
        /// <param name="member">member which task is submitted to.</param>
        /// <param name="value">result of execution</param>
        void OnResponse(IMember member, object value);

        /// <summary>Called after all executions are completed.</summary>
        /// <remarks>Called after all executions are completed.</remarks>
        /// <param name="values">map of IMember-Response pairs</param>
        void OnComplete(IDictionary<IMember, object> values);
    }
}