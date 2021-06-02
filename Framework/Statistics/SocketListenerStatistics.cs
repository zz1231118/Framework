namespace Framework.Statistics
{
    /// <summary>
    /// SocketListener statistics
    /// </summary>
    public class SocketListenerStatistics
    {
        internal readonly Int32Counter InboundConnectionCounter = new Int32Counter(nameof(InboundConnectionCount));
        internal readonly Int32Counter CurrentConnectionCounter = new Int32Counter(nameof(CurrentConnectionCount));
        internal readonly Int32Counter RejectedConnectionCounter = new Int32Counter(nameof(RejectedConnectionCount));
        internal readonly Int32Counter ClosedConnectionCounter = new Int32Counter(nameof(ClosedConnectionCount));
        internal readonly Int32Counter SendConcurrencyCounter = new Int32Counter(nameof(SendConcurrencyCount));
        internal readonly Int32Counter ReceiveConcurrencyCounter = new Int32Counter(nameof(ReceiveConcurrencyCount));
        internal readonly Int64Counter SentBytesTotalCounter = new Int64Counter(nameof(SentBytesTotal));
        internal readonly Int64Counter ReceivedBytesTotalCounter = new Int64Counter(nameof(ReceivedBytesTotal));

        /// <summary>
        /// Inbound connection count
        /// </summary>
        public int InboundConnectionCount => InboundConnectionCounter.Value;

        /// <summary>
        /// Current connection count
        /// </summary>
        public int CurrentConnectionCount => CurrentConnectionCounter.Value;

        /// <summary>
        /// Rejected connection count
        /// </summary>
        public int RejectedConnectionCount => RejectedConnectionCounter.Value;

        /// <summary>
        /// Closed connection count
        /// </summary>
        public int ClosedConnectionCount => ClosedConnectionCounter.Value;

        /// <summary>
        /// Send concurrency count
        /// </summary>
        public int SendConcurrencyCount => SendConcurrencyCounter.Value;

        /// <summary>
        /// Receive concurrency count
        /// </summary>
        public int ReceiveConcurrencyCount => ReceiveConcurrencyCounter.Value;

        /// <summary>
        /// Total sent byte length
        /// </summary>
        public long SentBytesTotal => SentBytesTotalCounter.Value;

        /// <summary>
        /// Total received byte length
        /// </summary>
        public long ReceivedBytesTotal => ReceivedBytesTotalCounter.Value;
    }
}
