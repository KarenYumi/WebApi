using AutoMapper;
using MinhaAPI.Models;

namespace MinhaAPI.DTOs.Mappings;

public class ProdutoDTOMappingAUTO : Profile
{
    public ProdutoDTOMappingAUTO() // definimos o mapeamento
    {
        CreateMap<Produto, ProdutoDTO>().ReverseMap();
        CreateMap<Categoria, CategoriaDTO>().ReverseMap();
        CreateMap<Produto, ProdutoDTOUpdateRequest>().ReverseMap();
        CreateMap<Produto, ProdutoDTOUpdateResponse>().ReverseMap();
    }
}
