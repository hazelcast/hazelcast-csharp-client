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