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

namespace Hazelcast.Client.Connection
{
    //internal class SSLSocketFactory //: ISocketFactory
    //{
        //private readonly Properties properties;

        //private readonly SSLContextFactory sslContextFactory;

        //private volatile bool initialized = false;

        //public SSLSocketFactory()
        //{
        //    // TODO: add SSLConfig to client config.
        //    sslContextFactory = new BasicSSLContextFactory();
        //    properties = new Properties();
        //}

        //public SSLSocketFactory(Properties properties)
        //{
        //    this.properties = properties != null ? properties : new Properties();
        //    sslContextFactory = new BasicSSLContextFactory();
        //}

        //public SSLSocketFactory(SSLContextFactory sslContextFactory, Properties properties)
        //{
        //    if (sslContextFactory == null)
        //    {
        //        throw new ArgumentNullException("SSLContextFactory is required!");
        //    }
        //    this.sslContextFactory = sslContextFactory;
        //    this.properties = properties != null ? properties : new Properties();
        //}

        ///// <exception cref="System.IO.IOException"></exception>
        //public virtual SSLSocket CreateSocket()
        //{
        //    if (!initialized)
        //    {
        //        lock (this)
        //        {
        //            if (!initialized)
        //            {
        //                try
        //                {
        //                    sslContextFactory.Init(properties);
        //                }
        //                catch (Exception e)
        //                {
        //                    throw ExceptionUtil.Rethrow<IOException>(e);
        //                }
        //                initialized = true;
        //            }
        //        }
        //    }
        //    SSLContext sslContext = sslContextFactory.GetSSLContext();
        //    Hazelcast.Net.Ext.SSLSocketFactory factory = sslContext.GetSocketFactory();
        //    return (SSLSocket)factory.CreateSocket();
        //}
    //}
}