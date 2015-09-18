using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    internal class MemberAttributeChange 
    {
        private string key;
        private MemberAttributeOperationType operationType;
        private string uuid;
        private string value;

        public MemberAttributeChange()
        {
        }

        public string Value
        {
            get { return value; }
        }

        public string Uuid
        {
            get { return uuid; }
        }

        public MemberAttributeOperationType OperationType
        {
            get { return operationType; }
        }

        public string Key
        {
            get { return key; }
        }


        public MemberAttributeChange(string uuid, MemberAttributeOperationType operationType
            , string key, string value)
        {
            this.uuid = uuid;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }
    }
}