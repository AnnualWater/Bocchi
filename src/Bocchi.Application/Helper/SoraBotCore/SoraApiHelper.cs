using System.Collections.Generic;
using System.Threading.Tasks;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Enumeration.ApiType;

namespace Bocchi.SoraBotCore;

public static class SoraApiHelper
{
    public static async Task<bool> TrySendPrivateMessage(this IEnumerable<SoraApi> soraApis,long userId,MessageBody messageBody)
    {
        foreach (var soraApi in soraApis)
        {
            var (apiStatus, messageId) = await soraApi.SendPrivateMessage(userId,messageBody);
            if (apiStatus.RetCode==ApiStatusType.Ok)
            {
                return true;
            }
        }

        return false;
    }
    public static async Task<bool> TrySendGroupMessage(this IEnumerable<SoraApi> soraApis,long groupId,MessageBody messageBody)
    {
        foreach (var soraApi in soraApis)
        {
            var (apiStatus, messageId) = await soraApi.SendGroupMessage(groupId,messageBody);
            if (apiStatus.RetCode==ApiStatusType.Ok)
            {
                return true;
            }
        }

        return false;
    }
}