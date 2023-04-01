using AutoMapper;
using Bocchi.ComicSubscription;
using Bocchi.OpenAi;
using Bocchi.PluginSwitch;

namespace Bocchi;

public class BocchiApplicationAutoMapperProfile : Profile
{
    public BocchiApplicationAutoMapperProfile()
    {
        CreateMap<PluginSwitchEntity, PluginSwitchItemDto>();
        CreateMap<ComicSubscriptionEntity, ComicSubscriptionDto>();
        CreateMap<ChatMessageDto, ChatMessage>();
    }
}