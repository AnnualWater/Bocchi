namespace Bocchi.SoraBotCore;

public abstract class Plugin : IPlugin
{
    public abstract uint Priority { get; }
}