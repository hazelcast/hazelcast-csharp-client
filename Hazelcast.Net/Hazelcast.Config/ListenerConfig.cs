using Hazelcast.Core;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>
    ///     Contains the configuration for an
    ///     <see cref="IEventListener">IEventListener</see>
    ///     . The configuration contains either the classname
    ///     of the IEventListener implementation, or the actual IEventListener instance.
    /// </summary>
    public class ListenerConfig
    {
        private string className;
        private IEventListener implementation;

        /// <summary>Creates a ListenerConfig without className/implementation.</summary>
        /// <remarks>Creates a ListenerConfig without className/implementation.</remarks>
        public ListenerConfig()
        {
        }

        /// <summary>Creates a ListenerConfig with the given className.</summary>
        /// <remarks>Creates a ListenerConfig with the given className.</remarks>
        /// <param name="className">the name of the IEventListener class.</param>
        /// <exception cref="System.ArgumentException">if className is null or an empty String.</exception>
        public ListenerConfig(string className)
        {
            SetClassName(className);
        }

        /// <summary>Creates a ListenerConfig with the given implementation.</summary>
        /// <remarks>Creates a ListenerConfig with the given implementation.</remarks>
        /// <param name="implementation">the implementation to use as IEventListener.</param>
        /// <exception cref="System.ArgumentException">if the implementation is null.</exception>
        public ListenerConfig(IEventListener implementation)
        {
            this.implementation = ValidationUtil.IsNotNull(implementation, "implementation");
        }

        /// <summary>Returns the name of the class of the IEventListener.</summary>
        /// <remarks>Returns the name of the class of the IEventListener. If no class is specified, null is returned.</remarks>
        /// <returns>the class name of the IEventListener.</returns>
        /// <seealso cref="SetClassName(string)">SetClassName(string)</seealso>
        public virtual string GetClassName()
        {
            return className;
        }

        /// <summary>Returns the IEventListener implementation.</summary>
        /// <remarks>Returns the IEventListener implementation. If none has been specified, null is returned.</remarks>
        /// <returns>the IEventListener implementation.</returns>
        /// <seealso cref="SetImplementation(IEventListener)" />
        public virtual IEventListener GetImplementation()
        {
            return implementation;
        }

        public virtual bool IsIncludeValue()
        {
            return true;
        }

        public virtual bool IsLocal()
        {
            return false;
        }

        /// <summary>Sets the class name of the IEventListener.</summary>
        /// <remarks>
        ///     Sets the class name of the IEventListener.
        ///     If a implementation was set, it will be removed.
        /// </remarks>
        /// <param name="className">the name of the class of the IEventListener.</param>
        /// <returns>the updated ListenerConfig.</returns>
        /// <exception cref="System.ArgumentException">if className is null or an empty String.</exception>
        /// <seealso cref="SetImplementation(IEventListener)" />
        /// <seealso cref="GetClassName()">GetClassName()</seealso>
        public ListenerConfig SetClassName(string className)
        {
            this.className = ValidationUtil.HasText(className, "className");
            implementation = null;
            return this;
        }

        /// <summary>Sets the IEventListener implementation.</summary>
        /// <remarks>
        ///     Sets the IEventListener implementation.
        ///     If a className was set, it will be removed.
        /// </remarks>
        /// <param name="implementation">the IEventListener implementation.</param>
        /// <returns>the updated ListenerConfig.</returns>
        /// <exception cref="System.ArgumentException">the implementation is null.</exception>
        /// <seealso cref="SetClassName(string)">SetClassName(string)</seealso>
        /// <seealso cref="GetImplementation()">GetImplementation()</seealso>
        public virtual ListenerConfig SetImplementation(IEventListener implementation)
        {
            this.implementation = ValidationUtil.IsNotNull(implementation, "implementation");
            className = null;
            return this;
        }

        public override string ToString()
        {
            return "ListenerConfig [className=" + className + ", implementation=" + implementation + ", includeValue=" +
                   IsIncludeValue() + ", local=" + IsLocal() + "]";
        }
    }
}