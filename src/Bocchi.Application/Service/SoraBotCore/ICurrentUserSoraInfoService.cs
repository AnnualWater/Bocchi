using System.Threading.Tasks;
using Sora.Entities.Info;

namespace Bocchi.SoraBotCore;

public interface ICurrentUserSoraInfoService
{
    public Task<UserInfo?> GetUserInfo();
    public Task<long?> GetUserTencentId();
    public Task<GroupMemberInfo> GetGroupMemberInfo(long groupId);
}