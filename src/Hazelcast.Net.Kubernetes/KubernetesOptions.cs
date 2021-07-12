// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hazelcast.Kubernetes
{
    // FIXME
    //
    // how can we extend our configuration system to provide these options?
    // in tests we have this "extra binding" thing but I'm not sure that's how it should work?
    // FIGURE IT OUT at .NET level
    //
    // and, in the Java project, how is Kubernetes injected into the original behavior?
    // config.getNetworkConfig().getJoin().getKubernetesConfig() <<< how is this working?!
    //
    // in a DI system, 
    //  ContainerExample = we still manually bind the configuration, right?
    //                     or would the binding still takes place auto-magically?
    //  HostedExample = we also add the various hazelcast-specific sources - on top of the existing ones I guess
    //
    // but what about the *binding* itself?
    // we use our own internal static ConfigurationBinder
    // but I cannot find out where it's invoked, and how?!
    // -> figure it out and EXPLAIN + how it works with hosted etc

    public class KubernetesOptions
    {
        // a sample option
        public string Name { get; set; }
    }
}
