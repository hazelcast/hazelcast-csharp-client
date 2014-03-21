using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Base
{
    /// <summary>
    /// Base client request, All Request must extend this class
    /// </summary>
    public abstract class ClientRequest:IPortable
    {
        int callId = -1;

        public int CallId
        {
            get { return callId; }
            set { callId = value; }
        }

        public virtual bool Sticky
        {
            get { return false; }
        }

        public abstract int GetFactoryId();
        public abstract int GetClassId();

        void IPortable.WritePortable(IPortableWriter writer)
        {
            BaseWritePortable(writer);
            WritePortable(writer);
        }

        protected virtual void BaseWritePortable(IPortableWriter writer)
        {
            writer.WriteInt("cId", callId);
        }
        
        public abstract void WritePortable(IPortableWriter writer);

        public void ReadPortable(IPortableReader reader)
        {
            
        }

    }


    internal interface IRemoveRequest
    {
        string RegistrationId
        {
            //get;
            set;
        }
    }
}
