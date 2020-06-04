using System.Collections.Generic;
using System.Text;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an option for an injected instance.
    /// </summary>
    internal class InjectionOptions
    {
        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string TypeName { get; set;}

        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        public Dictionary<string, string> Args { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append(GetType().Name);
            text.Append(" typeName: '");
            text.Append(TypeName ?? "<null>");
            text.Append("'");
            
            ToString(text);

            if (Args != null)
            {
                foreach (var (argKey, argValue) in Args)
                {
                    text.Append(", ");
                    text.Append(argKey);
                    text.Append(": '");
                    text.Append(argValue ?? "<null>");
                    text.Append("'");
                }
            }

            return text.ToString();
        }
        
        protected virtual void ToString(StringBuilder text)
        { }
    }
}
