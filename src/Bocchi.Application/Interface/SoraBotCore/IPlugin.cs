using Volo.Abp.DependencyInjection;

namespace Bocchi.SoraBotCore;

public interface IPlugin : ITransientDependency
{
    public uint Priority { get; }
}