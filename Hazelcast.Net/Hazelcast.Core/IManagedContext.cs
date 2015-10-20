/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Core
{
    /// <summary>Container managed context, such as Spring, Guice and etc.</summary>
    /// <remarks>Container managed context, such as Spring, Guice and etc.</remarks>
    public interface IManagedContext
    {
        /// <summary>Initialize the given object instance.</summary>
        /// <remarks>
        ///     Initialize the given object instance.
        ///     This is intended for repopulating select fields and methods for deserialized instances.
        ///     It is also possible to proxy the object, e.g. with AOP proxies.
        /// </remarks>
        /// <param name="obj">Object to initialize</param>
        /// <returns>the initialized object to use</returns>
        object Initialize(object obj);
    }
}