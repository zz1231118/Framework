namespace Framework.Security
{
    public interface IAuthorization
    {
        string Account { get; }

        long Timestamp { get; }

        string Token { get; }
    }
}
