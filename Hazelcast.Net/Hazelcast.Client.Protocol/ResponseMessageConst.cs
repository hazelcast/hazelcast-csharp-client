namespace Hazelcast.Client.Protocol
{
    /// <summary>Message type ids of responses in client protocol.</summary>
    /// <remarks>
    ///     Message type ids of responses in client protocol. They also used to bind a request to a response inside Request
    ///     annotation.
    /// </remarks>
    public sealed class ResponseMessageConst
    {
        public const int Void = 100;
        public const int Boolean = 101;
        public const int Integer = 102;
        public const int Long = 103;
        public const int String = 104;
        public const int Data = 105;
        public const int ListData = 106;
        public const int Authentication = 107;
        public const int Partitions = 108;
        public const int Exception = 109;
        public const int DistributedObject = 110;
        public const int EntryView = 111;
        public const int JobProcessInfo = 112;
        public const int SetData = 113;
        public const int SetEntry = 114;
    }
}