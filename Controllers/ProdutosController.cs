using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MinhaAPI.Contexto;
using MinhaAPI.DTOs;
using MinhaAPI.Models;
using MinhaAPI.Pagination;
using MinhaAPI.Repositories;
using Newtonsoft.Json;
using System.Security.Principal;
using X.PagedList;

namespace MinhaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    //[ApiExplorerSettings(IgnoreApi = true)] // n mostra na web
    public class ProdutosController : ControllerBase
    {
       // private readonly IProdutoRepository _repository; //mudei o AppDbContext para o repositorio
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProdutosController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }


        //Tirei todos os try and cacth e mudei o context para reposiotort alterando seu contexto

        //Método Action:

        //Apenas mostra todos os itens
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get() 
        {
            try
            {
                var produtos = await _uow.ProdutoRepository.GetProdutosAsync();

                if (produtos is null)
                    return NotFound();

                var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
                return Ok(produtosDto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        ////Permite pesquisar o item 
        //[HttpGet("{id:int:min(1)}", Name = "ObterProduto")]
        //public async Task<ActionResult<Produto>> Get(int id, [BindRequired] string nome)
        //{
        //    try
        //    {
        //        var nomeProduto = nome;
        //        var produto = await _context.Produtos.Take(10).FirstOrDefaultAsync(p => p.ProdutoId == id);
        //        if (produto is null)
        //        {
        //            return NotFound("Produto não encontrado");
        //        }
        //        return produto;
        //    }
        //    catch (Exception)
        //    {

        //        return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação");
        //    }

        //}
        //Permite pesquisar o item 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{id:int:min(1)}", Name = "ObterProduto")]
        public async Task<ActionResult<ProdutoDTO>> Get(int id)
        {
            if (id == null || id <= 0)
            {
                return BadRequest("ID de produto inválido");
            }

            var produto = await _uow.ProdutoRepository.GetProdutoAsync(id);
            if (produto is null)
            {
                return NotFound("Produto não encontrado...");
            }
            var produtoDto = _mapper.Map<ProdutoDTO>(produto);
            return Ok(produtoDto);

        }



        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetProdutosPorCategoria")]
        public async Task<ActionResult<ProdutoDTO>> GetProdutosPorCategoria(int categoriaId)
        {
            // Chama o método do repositório ou serviço para buscar os produtos pela categoria
            var produtos = await _uow.ProdutoRepository.GetProdutosPorCategoriasAsync(categoriaId);

            // Verifica se não há produtos para a categoria especificada
            if (produtos == null || produtos.Count == 0)
            {
                return NotFound(); // Retorna 404 se nenhum produto for encontrado
            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtosDto); // Retorna 200 com a lista de produtos
        }





        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters) //retorna uma lista de produtos
        {
            var produtos = await _uow.ProdutoRepository.GetAllAsync(produtosParameters);
            return ObterProdutos(produtos);

        }


        //Retorna os produtos por preços e paginados
        [HttpGet("filter/preco/pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco
                                                                                   produtosFilterParameters)
        {
            var produtos = await _uow.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFilterParameters);
            return ObterProdutos(produtos);
        }

        //cria um método chamado ObterProdutos, não necessitando repetir o código, só colocar no return, como no anterior
        private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos)
        {
            var metadata = new
            {
                produtos.Count,
                produtos.PageSize,
                produtos.PageCount,     //muda os nomes por conta de que mudamos o PagedList que tinhamos criado para o pacote IPagedList
                produtos.TotalItemCount,
                produtos.HasNextPage,
                produtos.HasPreviousPage
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }



        //Permite adicionar um Produto novo
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "UserOnly")]
        public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
        {
            if (produtoDto is null)
                return BadRequest();
            var produto = _mapper.Map<Produto>(produtoDto);//usando o AutoMapper

            var novoProduto = _uow.ProdutoRepository.Create(produto);
            await _uow.CommitAsync();

            var novoProdutoDto =_mapper.Map<ProdutoDTO>(novoProduto);//usando o AutoMapper
            return new CreatedAtRouteResult("ObterProduto", new { id = novoProduto.ProdutoId }, novoProdutoDto);
        }



        [HttpPatch("{id}/UpdatePartial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO) //ActionResult vai retornar o <> e usa o jasonpath e a <entrada> 
        {
            if(patchProdutoDTO is null || id<=0)
                return BadRequest();

            var produto = await _uow.ProdutoRepository.GetProdutoAsync(id); //aqui usaria o lambda, mas como meu GetProduto não é bool pra retornar verdade ent coloquei apenas o id
            if (produto == null)
                return NotFound();

            var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);

            patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState); //Qualquer erro ou problema será gravado no ModelState
            if(!ModelState.IsValid || TryValidateModel(produtoUpdateRequest))//
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(produtoUpdateRequest, produto);
            _uow.ProdutoRepository.Update(produto);
            await _uow.CommitAsync();

            return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto));

        }




        //Permite alterar os dados de um produto já postado
        [HttpPut ("{id:int}")]//{id} é um parâmetro, mas serve como os anteriores a rota de acesso é api/produtos/{id}
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "UserOnly")]
        public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto) 
        {
            
            if (id != produtoDto.ProdutoId)
               return BadRequest();
            var produto = _mapper.Map<Produto>(produtoDto); //usando o AutoMapper
            bool atualizado = _uow.ProdutoRepository.Update(produto);
            await _uow.CommitAsync();
            

            if (atualizado)
            {
                var produtoAtualizado = _mapper.Map<ProdutoDTO>(produto); //usando o AutoMapper
                return Ok(produtoAtualizado);
            }
            else
            {
                return BadRequest("Produto não encontrado...");
            }
        }
        


        //Exclue um produto
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "UserOnly")]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id) 
        {
            // Buscar o produto antes de deletar
            var produto = await _uow.ProdutoRepository.GetProdutoAsync(id);
            if (produto == null)
            {
                return NotFound("Produto não encontrado...");
            }

            // Realizar a exclusão
            bool deletado = _uow.ProdutoRepository.Delete(id);
            await _uow.CommitAsync();

            if (deletado)
            {
                // Mapear o produto deletado para ProdutoDTO
                var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(produto);
                return Ok(produtoDeletadoDto);
            }
            else
            {
                return BadRequest("Erro ao excluir o produto.");
            }

        }

        




    }
}
