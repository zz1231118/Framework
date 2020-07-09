namespace Framework.Security
{
    public interface IServerCredentials
    {
        string AccessKey { get; }

        bool Authenticate(IAuthorization authorization);
    }
}
