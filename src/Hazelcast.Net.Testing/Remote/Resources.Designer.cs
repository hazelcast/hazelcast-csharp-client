﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hazelcast.Testing.Remote {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Hazelcast.Testing.Remote.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;hazelcast xmlns=&quot;http://www.hazelcast.com/schema/config&quot;
        ///           xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;
        ///           xsi:schemaLocation=&quot;http://www.hazelcast.com/schema/config
        ///           http://www.hazelcast.com/schema/config/hazelcast-config-4.0.xsd&quot;&gt;
        ///
        ///  &lt;properties&gt;
        ///    &lt;property name=&quot;hazelcast.map.invalidation.batch.enabled&quot;&gt;false&lt;/property&gt;
        ///    &lt;property name=&quot;hazelcast.cache.invalidation.batch.size&quot;&gt;10&lt;/property&gt;
        ///    &lt;property name=&quot;haze [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string hazelcast {
            get {
                return ResourceManager.GetString("hazelcast", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;UTF-8&quot;?&gt;
        ///&lt;hazelcast xmlns=&quot;http://www.hazelcast.com/schema/config&quot;
        ///           xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;
        ///           xsi:schemaLocation=&quot;http://www.hazelcast.com/schema/config
        ///           http://www.hazelcast.com/schema/config/hazelcast-config-5.0.xsd&quot;&gt;
        ///  &lt;jet enabled=&quot;true&quot;&gt;&lt;/jet&gt;
        ///&lt;/hazelcast&gt;.
        /// </summary>
        internal static string jet_enabled {
            get {
                return ResourceManager.GetString("jet_enabled", resourceCulture);
            }
        }
    }
}
