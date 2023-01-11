using System.Collections.Generic;
using System.Threading.Tasks;
using Sora.Entities.Base;

namespace Bocchi.SoraBotCore;

public interface ISoraBotService
{
    public Task RunService();
    public IEnumerable<long> GetAllTencentId();

    public SoraApi GetApi(long tencentId);

    public IEnumerable<SoraApi> GetAllApi();
}