﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hazelcast.Tests {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Hazelcast.Tests.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string Empty {
            get {
                return ResourceManager.GetString("Empty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///    // accepting comments?
        ///  }
        ///}.
        /// </summary>
        internal static string EmptyWithComments {
            get {
                return ResourceManager.GetString("EmptyWithComments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    &quot;clientName&quot;: &quot;client&quot;,
        ///    &quot;clusterName&quot;: &quot;cluster&quot;,
        ///    &quot;asyncStart&quot;: true,
        ///
        ///    &quot;labels&quot;: [
        ///      &quot;label_1&quot;,
        ///      &quot;label_2&quot;
        ///    ],
        ///
        ///    &quot;subscribers&quot;: [
        ///      { &quot;typeName&quot;: &quot;Hazelcast.Tests.ConfigurationTests+TestSubscriber, Hazelcast.Net.Tests&quot; }
        ///    ],
        ///
        ///    &quot;logging&quot;: {},
        ///
        ///    // core options
        ///    &quot;core&quot;: {
        ///
        ///      // clock options
        ///      &quot;clock&quot;: {
        ///
        ///        // clock offset
        ///        &quot;offset&quot;: 1000
        ///      }
        ///    },
        ///
        ///    // messaging options
        ///    &quot;messaging [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string HazelcastOptions {
            get {
                return ResourceManager.GetString("HazelcastOptions", resourceCulture);
            }
        }
    }
}
