// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Map
{
    public interface IMapInterceptor
    {
        /// <summary>Called after get(..) operation is completed.</summary>
        /// <remarks>
        ///     Called after get(..) operation is completed.
        ///     <p />
        /// </remarks>
        /// <param name="value">the value returned as the result of get(..) operation</param>
        void AfterGet(object value);

        /// <summary>Called after put(..) operation is completed.</summary>
        /// <remarks>
        ///     Called after put(..) operation is completed.
        ///     <p />
        /// </remarks>
        /// <param name="value">the value returned as the result of put(..) operation</param>
        void AfterPut(object value);

        /// <summary>Called after remove(..) operation is completed.</summary>
        /// <remarks>
        ///     Called after remove(..) operation is completed.
        ///     <p />
        /// </remarks>
        /// <param name="value">the value returned as the result of remove(..) operation</param>
        void AfterRemove(object value);

        /// <summary>Intercept get operation before returning value.</summary>
        /// <remarks>
        ///     Intercept get operation before returning value.
        ///     Return another object to change the return value of get(..)
        ///     Returning null will cause the get(..) operation return original value, namely return null if you do not want to
        ///     change anything.
        ///     <p />
        /// </remarks>
        /// <param name="value">the original value to be returned as the result of get(..) operation</param>
        /// <returns>the new value that will be returned by get(..) operation</returns>
        object InterceptGet(object value);

        /// <summary>Intercept put operation before modifying map data.</summary>
        /// <remarks>
        ///     Intercept put operation before modifying map data.
        ///     Return the object to be put into the map.
        ///     Returning null will cause the put(..) operation to operate as expected, namely no interception.
        ///     Throwing an exception will cancel the put operation.
        ///     <p />
        /// </remarks>
        /// <param name="oldValue">the value currently in map</param>
        /// <param name="newValue">the new value to be put</param>
        /// <returns>new value after intercept operation</returns>
        object InterceptPut(object oldValue, object newValue);

        /// <summary>Intercept remove operation before removing the data.</summary>
        /// <remarks>
        ///     Intercept remove operation before removing the data.
        ///     Return the object to be returned as the result of remove operation.
        ///     Throwing an exception will cancel the remove operation.
        ///     <p />
        /// </remarks>
        /// <param name="removedValue">the existing value to be removed</param>
        /// <returns>the value to be returned as the result of remove operation</returns>
        object InterceptRemove(object removedValue);
    }
}