namespace Bocchi.SoraBotCore;

public interface IOnSoraMessageAttribute
{
    /// <summary>
    /// 方法优先级
    /// </summary>
    public uint Priority { get; init; }
}