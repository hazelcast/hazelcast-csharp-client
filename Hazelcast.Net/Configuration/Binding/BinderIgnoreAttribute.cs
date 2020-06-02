using System;

namespace Hazelcast.Configuration.Binding
{
    [AttributeUsage(AttributeTargets.Property)]
    public class BinderIgnoreAttribute : Attribute
    {
        public BinderIgnoreAttribute(bool ignore = true)
        {
            Ignore = ignore;
        }

        public bool Ignore { get; }
    }
}
