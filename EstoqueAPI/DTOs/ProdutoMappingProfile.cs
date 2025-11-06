using AutoMapper;
using EstoqueAPI.Models;

namespace EstoqueAPI.DTOs
{
    namespace EstoqueAPI.Profiles
    {
        public class ProdutoMappingProfile : Profile
        {
            public ProdutoMappingProfile() // <- precisa do construtor
            {
                CreateMap<ProdutoInputDto, Produto>();
            }
        }
    }
}