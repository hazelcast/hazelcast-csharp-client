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
        ///    // all ClusterOptions
        ///
        ///    &quot;cluster&quot;: {
        ///      &quot;eventSubscribers&quot;: [
        ///        &quot;Hazelcast.Tests.ConfigurationTests+TestSubscriber, Hazelcast.Net.Tests&quot;
        ///      ]
        ///    } 
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string ClusterOptions {
            get {
                return ResourceManager.GetString("ClusterOptions", resourceCulture);
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
        ///    // all HazelcastOptions top-level properties
        ///
        ///    &quot;clusterName&quot;: &quot;testClusterName&quot;,
        ///
        ///    &quot;clientName&quot;: &quot;testClientName&quot;,
        ///
        ///    &quot;properties&quot;: {
        ///      &quot;aKey&quot;: &quot;aValue&quot;,
        ///      &quot;anotherKey&quot;: &quot;anotherValue&quot;
        ///    },
        ///
        ///    &quot;labels&quot;: [
        ///      &quot;label1&quot;,
        ///      &quot;label2&quot;
        ///    ],
        ///
        ///    &quot;asyncStart&quot;: true
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string HazelcastOptions {
            get {
                return ResourceManager.GetString("HazelcastOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    // all LoadBalancingOptions
        ///
        ///    &quot;loadBalancing&quot;: {
        ///
        ///      &quot;loadBalancerType&quot;: &quot;Hazelcast.Clustering.LoadBalancing.RandomLoadBalancer, Hazelcast.Net&quot;,
        ///
        ///      &quot;loadBalancerArgs&quot;: {
        ///
        ///        &quot;arg1&quot;: &quot;value1&quot;,
        ///        &quot;arg2&quot;: &quot;value2&quot;
        ///
        ///      }
        ///
        ///    }
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string LoadBalancingOptions {
            get {
                return ResourceManager.GetString("LoadBalancingOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    &quot;logging&quot;: { }
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string LoggingOptions {
            get {
                return ResourceManager.GetString("LoggingOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    // all NearCacheOptions
        ///
        ///    &quot;nearCache&quot;: {
        ///
        ///      &quot;default&quot;: {
        ///
        ///        &quot;name&quot;: &quot;defaultName&quot;,
        ///        &quot;evictionPolicy&quot;: &quot;lru&quot;,
        ///        &quot;inMemoryFormat&quot;: &quot;binary&quot;,
        ///        &quot;maxIdleSeconds&quot;: 666,
        ///        &quot;maxSize&quot;: 667,
        ///        &quot;TimeToLiveSeconds&quot;: 668,
        ///        &quot;invalidateOnChange&quot;: true
        ///      },
        ///
        ///      &quot;other&quot;: {
        ///
        ///        &quot;name&quot;: &quot;otherName&quot;,
        ///        &quot;evictionPolicy&quot;: &quot;lfu&quot;,
        ///        &quot;inMemoryFormat&quot;: &quot;object&quot;,
        ///        &quot;maxIdleSeconds&quot;: 166,
        ///        &quot;maxSize&quot;: [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NearCacheOptions {
            get {
                return ResourceManager.GetString("NearCacheOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    // all NetworkingOptions
        ///
        ///    &quot;networking&quot;: {
        ///
        ///      &quot;addresses&quot;: [
        ///        &quot;localhost&quot;,
        ///        &quot;otherhost&quot;
        ///      ],
        ///
        ///      &quot;smartRouting&quot;: false,
        ///
        ///      &quot;redoOperation&quot;: false,
        ///
        ///      &quot;connectionTimeoutMilliseconds&quot;: 666,
        ///
        ///      &quot;reconnectMode&quot;: &quot;doNotReconnect&quot;,
        ///
        ///      &quot;ssl&quot;: {},
        ///
        ///      &quot;cloud&quot;: {},
        ///
        ///      &quot;socket&quot;: {},
        ///
        ///      &quot;outboundPorts&quot;: [],
        ///
        ///      &quot;socketInterceptor&quot;: {},
        ///
        ///      &quot;connectionRetry&quot;: {}
        ///
        ///    } 
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string NetworkingOptions {
            get {
                return ResourceManager.GetString("NetworkingOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    // all SecurityOptions
        ///
        ///    &quot;security&quot;: {
        ///
        ///      &quot;credentialsFactoryType&quot;: &quot;Hazelcast.Security.DefaultCredentialsFactory, Hazelcast.Net&quot;,
        ///
        ///      &quot;credentialsFactoryArgs&quot;: {
        ///        &quot;arg1&quot;: &quot;value1&quot;,
        ///        &quot;arg2&quot;: &quot;value2&quot;
        ///      },
        ///
        ///      &quot;authenticatorType&quot;: &quot;Hazelcast.Clustering.Authenticator, Hazelcast.Net&quot;,
        ///
        ///      &quot;authenticatorArgs&quot;: {
        ///        &quot;arg3&quot;: &quot;value3&quot;,
        ///        &quot;arg4&quot;: &quot;value4&quot; 
        ///      } 
        ///      
        ///    } 
        ///
        ///  }
        ///}.
        /// </summary>
        internal static string SecurityOptions {
            get {
                return ResourceManager.GetString("SecurityOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;hazelcast&quot;: {
        ///
        ///    &quot;serialization&quot;: {
        ///
        ///      &quot;endianness&quot;: &quot;LittleEndian&quot;,
        ///      &quot;portableVersion&quot;: 42,
        ///      &quot;checkClassDefinitionErrors&quot;: false,
        ///
        ///      &quot;portableFactories&quot;: [
        ///        {
        ///          &quot;id&quot;: 666,
        ///          &quot;factoryType&quot;: &quot;Hazelcast.Tests.ConfigurationTests+TestPortableFactory, Hazelcast.Net.Tests&quot;
        ///        }
        ///      ],
        ///
        ///      &quot;dataSerializableFactories&quot;: [
        ///        {
        ///          &quot;id&quot;: 667,
        ///          &quot;factoryType&quot;: &quot;Hazelcast.Tests.ConfigurationTests+TestDataSerializableFactory, [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SerializationOptions {
            get {
                return ResourceManager.GetString("SerializationOptions", resourceCulture);
            }
        }
    }
}
