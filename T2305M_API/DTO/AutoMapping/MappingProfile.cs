using AutoMapper;
using T2305M_API.DTO.Notification;
using T2305M_API.Entities;

namespace T2305M_API.DTO.AutoMapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateBasicNotificationDTO, Entities.Notification>();
            CreateMap<Entities.Notification, GetBasicNotificationDTO>();
        }
    }
}
