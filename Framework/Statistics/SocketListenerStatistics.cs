namespace Framework.Statistics
{
    /// <summary>
    /// SocketListener statistics
    /// </summary>
    public class SocketListenerStatistics
    {
        internal Int32CounterStatistic inboundConnectionCounter = new Int32CounterStatistic(nameof(InboundConnectionCount));
        internal Int32CounterStatistic currentConnectionCounter = new Int32CounterStatistic(nameof(CurrentConnectionCount));
        internal Int32CounterStatistic rejectedConnectionCounter = new Int32CounterStatistic(nameof(RejectedConnectionCount));
        internal Int32CounterStatistic closedConnectionCounter = new Int32CounterStatistic(nameof(ClosedConnectionCount));
        internal Int32CounterStatistic sendConcurrencyCounter = new Int32CounterStatistic(nameof(SendConcurrencyCount));
        internal Int32CounterStatistic recvConcurrencyCounter = new Int32CounterStatistic(nameof(ReceiveConcurrencyCount));

        /// <summary>
        /// Inbound connection count
        /// </summary>
        public int InboundConnectionCount => inboundConnectionCounter.Value;

        /// <summary>
        /// Current connection count
        /// </summary>
        public int CurrentConnectionCount => currentConnectionCounter.Value;

        /// <summary>
        /// Rejected connection count
        /// </summary>
        public int RejectedConnectionCount => rejectedConnectionCounter.Value;

        /// <summary>
        /// Closed connection count
        /// </summary>
        public int ClosedConnectionCount => closedConnectionCounter.Value;

        /// <summary>
        /// Send concurrency count
        /// </summary>
        public int SendConcurrencyCount => sendConcurrencyCounter.Value;

        /// <summary>
        /// Receive concurrency count
        /// </summary>
        public int ReceiveConcurrencyCount => recvConcurrencyCounter.Value;
    }
}
