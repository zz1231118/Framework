namespace Framework.Security
{
    public interface IClientCredentials
    {
        string Account { get; }

        string AccessKey { get; }

        IAuthorization Create();
    }
}