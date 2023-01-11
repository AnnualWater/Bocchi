using System;
using Volo.Abp.Application.Dtos;

namespace Bocchi.ComicSubscription;

public class ComicSubscriptionDto : EntityDto<Guid>
{
    /// <summary>
    /// 订阅人TencentId
    /// </summary>
    public long CreateUserId { get; set; }

    /// <summary>
    /// 群号
    /// </summary>
    public long GroupId { get; set; }

    /// <summary>
    /// 番剧ID
    /// </summary>
    public string ComicId { get; set; }

    /// <summary>
    /// 番剧名
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 当前集数
    /// </summary>
    public string Episode { get; set; }

    /// <summary>
    /// 更新信息
    /// </summary>
    public string UpdateInfo { get; set; }

    /// <summary>
    /// 最新集URL
    /// </summary>
    public string NewLink { get; set; }

    /// <summary>
    ///  订阅类型
    /// </summary>
    public ScheduledType ScheduledType { get; set; }

    /// <summary>
    /// 已经完结
    /// </summary>
    public bool IsEnd { get; set; }
}