using AutoMapper;
using Microsoft.Xrm.Sdk;

namespace Socio.Api.MappProfiles
{
    public class AlquilerProfile : Profile
    {
        public AlquilerProfile()
        {
            CreateMap<Entity, Model.Dto.Alquiler>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Biblioteca, opt => opt.MapFrom(src =>src.GetAttributeValue<EntityReference>("dn_biblioteca").Name))
                .ForMember(dest => dest.Libro, opt => opt.MapFrom(src => src.GetAttributeValue<EntityReference>("dn_libro").Name))
                .ForMember(dest => dest.Socio, opt => opt.MapFrom(src => src.GetAttributeValue<EntityReference>("dn_socio").Name))
                .ForMember(dest => dest.BibliotecaGuid, opt => opt.MapFrom(src => src.GetAttributeValue<EntityReference>("dn_biblioteca").Id))
                .ForMember(dest => dest.LibroGuid, opt => opt.MapFrom(src => src.GetAttributeValue<EntityReference>("dn_libro").Id))
                .ForMember(dest => dest.SocioGuid, opt => opt.MapFrom(src => src.GetAttributeValue<EntityReference>("dn_socio").Id))
                .ForMember(dest => dest.Desde, opt => opt.MapFrom(src => src.GetAttributeValue<DateTime>("dn_desde") ))
                .ForMember(dest => dest.Hasta, opt => opt.MapFrom(src => src.GetAttributeValue<DateTime>("dn_hasta")));
        }
    }
}
