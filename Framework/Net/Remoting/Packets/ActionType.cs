namespace Framework.Net.Remoting.Packets
{
    public enum ActionType : byte
    {
        Heartbeat,
        Error,
        Validate,
        Request,
        Response,
    }
}
