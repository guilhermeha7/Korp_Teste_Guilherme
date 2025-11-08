using AutoMapper;
using FaturamentoAPI.Models;

namespace FaturamentoAPI.DTOs
{
    public class NotaFiscalMappingProfile : Profile
    {
        public NotaFiscalMappingProfile() // <- precisa do construtor
        {
            CreateMap<ItemNotaFiscalInputDto, ItemNotaFiscal>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.NotaFiscalId, opt => opt.Ignore())
            .ForMember(dest => dest.NotaFiscal, opt => opt.Ignore());

            CreateMap<NotaFiscalInputDto, NotaFiscal>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CriadoEm, opt => opt.Ignore())
                // mapeia a lista de itens usando o mapping acima
                .ForMember(dest => dest.Itens, opt => opt.MapFrom(src => src.Itens));
        }
    }
}
