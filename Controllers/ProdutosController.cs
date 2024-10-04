using AutoMapper;
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
    public class ProdutosController : ControllerBase
    {
       // private readonly IProdutoRepository _repository; //mudei o AppDbContext para o repositorio
       private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ProdutosController(IProdutoRepository repository, IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        //Tirei todos os try and cacth e mudei o context para reposiotort alterando seu contexto

        //Método Action:

        //Apenas mostra todos os itens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get() 
        {
            
            var produtos = await _uow.ProdutoRepository.GetProdutosAsync();
            if (produtos is null)
            {
                return NotFound();
            }
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos); //usando o AutoMapper
            return Ok(produtosDto);
            

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

        [HttpGet("{id:int:min(1)}", Name = "ObterProduto")]
        public async Task<ActionResult<ProdutoDTO>> Get(int id)
        {
            var produto = await _uow.ProdutoRepository.GetProdutoAsync(id);
            if (produto is null)
            {
                return NotFound("Produto não encontrado");
            }
            var produtoDto = _mapper.Map<ProdutoDTO>(produto); //usando o AutoMapper
            return Ok(produtoDto);
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
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO) //ActionResult vai retornar o <> e usa o jasonpath e a <entrada> 
        {
            if(patchProdutoDTO is null || id<=0)
                return BadRequest();

            var produto = await _uow.ProdutoRepository.GetProdutoAsync(id); //aqui usaria o lambda, mas como meu GetProduto não é bool pra retornar verdade ent coloquei apenas o id
            if (produto is null)
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
        public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto) 
        {
            
            if (id != produtoDto.ProdutoId)
               return BadRequest();
            var produto = _mapper.Map<Produto>(produtoDto); //usando o AutoMapper
            bool atualizado = _uow.ProdutoRepository.Update(produto);
            await _uow.CommitAsync();
            var produtoAtualizado = _mapper.Map<ProdutoDTO>(atualizado); //usando o AutoMapper

            if (atualizado)
            {
                return Ok(produto);
            }
            else
            {
                return StatusCode(500, $"Falha ao atualizar o produto de id = {id}");
            }
        }





        //Exclue um produto
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ProdutoDTO>> Delete(int id) 
        {
            bool deletado = _uow.ProdutoRepository.Delete(id);
            await _uow.CommitAsync();

            var produtoDeletadoDto = _mapper.Map<ProdutoDTO>(deletado);//usando o AutoMapper

            if (deletado)
            {
                return Ok(produtoDeletadoDto);
            }
            else
            {
                return StatusCode(500, $"Falha ao excluir o produto de id = {id}");
            }


        }




    }
}
