using System.Threading.Tasks;

namespace Bocchi.SoraBotCore.NoPasswordToken;

public interface INoPasswordTokenService
{
    public Task<string> GetLoginToken(long userId);
    public Task<string> GetUserId(string token);
}