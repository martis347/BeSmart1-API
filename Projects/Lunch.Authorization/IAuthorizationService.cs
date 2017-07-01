
namespace Lunch.Authorization
{
    public interface IAuthorizationService
    {
        bool Authorize(string token);
        string GetToken();
    }
}