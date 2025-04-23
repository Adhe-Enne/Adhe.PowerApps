namespace Core.Abstractions
{
    public interface IAuthService
    {
        Task<string> GetCrmToken(bool fromEnv = false, bool UseExternalUrl = false);
        //Task SetCredentials();
        //Task<Model.Credential> GetCredentials(bool fromEnv = false);
    }
}
