using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Hazelcast.Client.Connection;
using Hazelcast.Client.Request.Base;
using Hazelcast.Config;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    //internal class PacketManager:IDisposable
    //{
    //    private int _readerThreadCount;
    //    private int _writerThreadCount;


    //    private ClientConnectionManager _connectionManager;

    //    private bool _active = false;

    //    public PacketManager(ClientConnectionManager connectionManager,int readerThreadCount=2, int writerThreadCount=2)
    //    {
    //        this._connectionManager = connectionManager;
    //        this._readerThreadCount = readerThreadCount;
    //        this._writerThreadCount = writerThreadCount;
    //    }

    //    public void Start()
    //    {
    //        //Start reader threads

    //        //Start writer threads

    //        _active = true;



    //    }

    //    public void Stop()
    //    {
    //        _active = true;
            
    //        //stop writer threads
    //        //Stop reader threads
            
    //    }

    //    public void Send(ClientRequest clientRequest, Address address)
    //    {

            
    //    }

    //    public void ProcessSend()
    //    {
    //        ClientConnection clientConnection = _connectionManager.NextConnection();

    //        clientConnection.ProcessSend();
    //    }

    //    public void ProcessReceive()
    //    {
            
    //    }

     

    //    public void Dispose()
    //    {
            
    //    }
    //}
}
