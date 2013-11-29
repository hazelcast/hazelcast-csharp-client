using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal abstract class PortableContextAwareInputStream : InputStream
    {
        private int classId;
        private int factoryId;

        private int version;

        public abstract int Read();
        public abstract int Read(byte[] b);
        public abstract int Read(byte[] b, int off, int len);
        public abstract long Skip(long n);
        public abstract int Available();
        public abstract void Close();
        public abstract void Mark(int readlimit);
        public abstract void Reset();
        public abstract bool MarkSupported();

        internal int GetFactoryId()
        {
            return factoryId;
        }

        internal void SetFactoryId(int factoryId)
        {
            this.factoryId = factoryId;
        }

        internal int GetClassId()
        {
            return classId;
        }

        internal void SetClassId(int classId)
        {
            this.classId = classId;
        }

        internal int GetVersion()
        {
            return version;
        }

        internal void SetVersion(int version)
        {
            this.version = version;
        }

        internal void SetClassDefinition(IClassDefinition cd)
        {
            factoryId = cd != null ? cd.GetFactoryId() : 0;
            classId = cd != null ? cd.GetClassId() : -1;
            version = cd != null ? cd.GetVersion() : -1;
        }
    }
}