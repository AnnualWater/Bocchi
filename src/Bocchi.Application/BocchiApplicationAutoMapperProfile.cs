using AutoMapper;
using Bocchi.ComicSubscription;
using Bocchi.PluginSwitch;

namespace Bocchi;

public class BocchiApplicationAutoMapperProfile : Profile
{
    public BocchiApplicationAutoMapperProfile()
    {
        CreateMap<PluginSwitchEntity, PluginSwitchItemDto>();
        CreateMap<ComicSubscriptionEntity, ComicSubscriptionDto>();
    }
}