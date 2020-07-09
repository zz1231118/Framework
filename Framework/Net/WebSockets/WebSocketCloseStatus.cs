namespace Framework.Net.WebSockets
{
    public enum WebSocketCloseStatus : byte
    {
        None,
        Closed,
        Timeout,
        InvalidMessage,
        InternalServerError,
    }
}
