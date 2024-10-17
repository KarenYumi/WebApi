
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MinhaAPI.Contexto;
using MinhaAPI.DTOs;
using MinhaAPI.DTOs.Mappings;
using MinhaAPI.Filters;
using MinhaAPI.Models;
using MinhaAPI.Pagination;
using MinhaAPI.Repositories;
using Newtonsoft.Json;
using System.Data;
using X.PagedList;

namespace APICatalogo.Controllers;

//[Authorize(Roles = "Admin")]
[ApiController]
[Route("[controller]")]
/*[ApiExplorerSettings(IgnoreApi = true)]*/ // n mostra na web
//[EnableRateLimiting("fixedwindow")] // necessita do nome da política //será aplicada globalmente pelo código globalLimiter
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _uow;//tira o repositorio e coloca esse
    //private readonly ICategoriaRepository _repository; //retiramos o AppDbContext e substituimos 
    private readonly IConfiguration _configuration;
    private readonly ILogger<CategoriasController> _logger;


    public CategoriasController(//ICategoriaRepository repository
                                IUnitOfWork uow, IConfiguration configuration, ILogger<CategoriasController> logger)
    {
         // _repository = repository;
        _uow = uow;
        _configuration = configuration;
        _logger = logger;
        
    }
    //AQUI NÓS APRENDEMOS A COLOCAR O FILTRO PARA REDUZIR A REPETIÇÃO DO CÓDIGO (TRY E O CATCH)


    ////Pare esse código funcionar necessita adicionar builder.Services.AddControllers().AddJsonOptions(options=> options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles); no Progrma.cs
    //[HttpGet ("LerArquivoConfigurado")]
    //public string GetValores()
    //{
    //    var valor1 = _configuration["chave1"];
    //    var valor2 = _configuration["chave2"];

    //    var secao1= _configuration["secao1:chave1"];

    //        return $"Chave1 = {valor1} \nChave2= {valor2} \nSeção1 => Chave2 = {secao1}";
    //}

    //    [HttpGet("produtos")]
    //    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoriasProdutos()
    //    {
    //        try
    //        {
    //            _logger.LogInformation("============ Get/Categorias/produtos==============");
    //            return await _context.Categorias.Include(p => p.Produtos).ToListAsync();
    //        }
    //        catch (Exception)
    //        {
    //            return StatusCode(StatusCodes.Status500InternalServerError,
    //                "Ocorreu um problema ao tratar a sua solicitação.");
    //        }
    //    }


    //    [HttpGet]
    //    [ServiceFilter(typeof(ApiLoggingFilter))]
    //    public async Task<ActionResult<IEnumerable<Categoria>>> Get()
    //    {
    //        try
    //        {
    //            return await _context.Categorias.AsNoTracking().ToListAsync();
    //        }
    //        catch (Exception)
    //        {
    //            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação");
    //        }

    //    }

    //    [HttpGet("{id:int}", Name = "ObterCategoria")]
    //    public ActionResult<Categoria> Get(int id)
    //    {
    //        //throw new Exception("Exceção ao retorno a categoria pelo Id");// usamos essa linha para testar o middleware
    //        //throw new ArgumentException("Exceção ao retorno a categoria pelo Id");
    //        try
    //        {
    //            var categoria = _context.Categorias.FirstOrDefault(p => p.CategoriaId == id);

    //            if (categoria == null)
    //            {
    //                _logger.LogWarning("=======================================");
    //                _logger.LogWarning($"Categoria com id= {id} não encontrada...");
    //                _logger.LogWarning("=======================================");
    //                return NotFound($"Categoria com id= {id} não encontrada...");
    //            }
    //            return Ok(categoria);

    //        }
    //        catch (Exception)
    //        {

    //            _logger.LogError("=======================================================================");
    //            _logger.LogError($"{StatusCodes.Status500InternalServerError} - Ocorreu um problema ao tratar a sua solicitação");
    //            _logger.LogError("=======================================================================");

    //            return StatusCode(StatusCodes.Status500InternalServerError,
    //                       "Ocorreu um problema ao tratar a sua solicitação.");

    //        }

    //    }

    //    [HttpPost]
    //    public ActionResult Post(Categoria categoria)
    //    {
    //        try
    //        {
    //            if (categoria is null)
    //                return BadRequest();

    //            _context.Categorias.Add(categoria);
    //            _context.SaveChanges();

    //            return new CreatedAtRouteResult("ObterCategoria",
    //                new { id = categoria.CategoriaId }, categoria);
    //        }
    //        catch (Exception)
    //        {

    //            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação");
    //        }

    //    }

    //    [HttpPut("{id:int}")]
    //    public ActionResult Put(int id, Categoria categoria)
    //    {
    //        try
    //        {
    //            if (id != categoria.CategoriaId)
    //            {
    //                return BadRequest();
    //            }
    //            _context.Entry(categoria).State = EntityState.Modified;
    //            _context.SaveChanges();
    //            return Ok(categoria);
    //        }
    //        catch (Exception)
    //        {

    //            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação");
    //        }

    //    }

    //    [HttpDelete("{id:int}")]
    //    public ActionResult Delete(int id)
    //    {
    //        try
    //        {
    //            var categoria = _context.Categorias.FirstOrDefault(p => p.CategoriaId == id);

    //            if (categoria == null)
    //            {
    //                return NotFound("Categoria não encontrada...");
    //            }
    //            _context.Categorias.Remove(categoria);
    //            _context.SaveChanges();
    //            return Ok(categoria);
    //        }
    //        catch (Exception)
    //        {

    //            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um problema ao tratar a sua solicitação");
    //        }

    //    }
    //}




    //MUDAMOS O _CONTEXT PARA O _REPOSITORY POIS MOVEMOS A
    //LÓGICA DE ACESSO A DADOS QUE ESTAVA NO CONTROLADOR O REPOSITORIO



    //    [HttpGet]
    //    public async Task<ActionResult<IEnumerable<Categoria>>> Get()
    //    {
    //        return await _context.Categorias.AsNoTracking().ToListAsync();
    //    }

    //    [HttpGet("{id:int}", Name = "ObterCategoria")]
    //    public ActionResult<Categoria> Get(int id)
    //    {
    //        var categoria = _context.Categorias.FirstOrDefault(p => p.CategoriaId == id);

    //        if (categoria == null)
    //        {
    //            _logger.LogWarning($"Categoria com id= {id} não encontrada...");
    //            return NotFound($"Categoria com id= {id} não encontrada...");
    //        }
    //        return Ok(categoria);
    //    }

    //    [HttpPost]
    //    public ActionResult Post(Categoria categoria)
    //    {
    //        if (categoria is null)
    //        {
    //            _logger.LogWarning($"Dados inválidos...");
    //            return BadRequest("Dados inválidos");
    //        }

    //        _context.Categorias.Add(categoria);
    //        _context.SaveChanges();

    //        return new CreatedAtRouteResult("ObterCategoria", new { id = categoria.CategoriaId }, categoria);
    //    }

    //    [HttpPut("{id:int}")]
    //    public ActionResult Put(int id, Categoria categoria)
    //    {
    //        if (id != categoria.CategoriaId)
    //        {
    //            _logger.LogWarning($"Dados inválidos...");
    //            return BadRequest("Dados inválidos");
    //        }

    //        _context.Entry(categoria).State = EntityState.Modified;
    //        _context.SaveChanges();
    //        return Ok(categoria);
    //    }

    //    [HttpDelete("{id:int}")]
    //    public ActionResult Delete(int id)
    //    {
    //        var categoria = _context.Categorias.FirstOrDefault(p => p.CategoriaId == id);

    //        if (categoria == null)
    //        {
    //            _logger.LogWarning($"Categoria com id={id} não encontrada...");
    //            return NotFound($"Categoria com id={id} não encontrada...");
    //        }

    //        _context.Categorias.Remove(categoria);
    //        _context.SaveChanges();
    //        return Ok(categoria);
    //    }
    //}

    //mudei todos os _repository para _uow e mudei os .SaveChages para .Commit
    /// <summary>
    /// Obtém uma lista de objetos categoria
    /// </summary>
    /// <returns>Uma Lista de objetos categorias</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias =await _uow.CategoriaRepository.GetCategoirasAsync();
        if (categorias is null)
        {
            return NotFound("Não Existem categorias");
        }
        //var categoriasDto = new List<CategoriaDTO>();
        //foreach(var categoria in categorias)
        //{
        //    var categoriaDto = new CategoriaDTO() 
        //    {
        //        CategoriaId = categoria.CategoriaId,                                                  DTO MANUAL
        //        Nome = categoria.Nome,
        //        ImagemUrl = categoria.ImagemUrl
        //    };
        //    categoriasDto.Add(categoriaDto);
        //}
        
        var categoriasDto = categorias.ToCategoriaDTOList();           //DTO Extensão
        return Ok(categoriasDto);
    }



    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var categoria = await _uow.CategoriaRepository.GetCategoriaAsync(id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id= {id} não encontrada...");
            return NotFound($"Categoria com id= {id} não encontrada...");
        }
        //var categoriaDto = new CategoriaDTO() // categoriaDto é uma instancia de CategoriaDTO e é atribuido os valores a baixo em categoria Dto
        //{
        //    CategoriaId = categoria.CategoriaId,
        //    Nome = categoria.Nome,
        //    ImagemUrl = categoria.ImagemUrl
        //};
        var categoriaDto = categoria.ToCategoriaDTO();                  //DTO Extensão
        return Ok(categoriaDto);
    }



    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }
        //var categoria = new Categoria()
        //{
        //    CategoriaId = categoriaDto.CategoriaId,                                                     DTO MANUAL
        //    Nome = categoriaDto.Nome,
        //    ImagemUrl = categoriaDto.ImagemUrl
        //};
        var categoria = categoriaDto.ToCategoria();                       //DTO Extensão
        var categoriaCriada = _uow.CategoriaRepository.Create(categoria);
        await _uow.CommitAsync();

        //var novacategoriaDto = new CategoriaDTO() 
        //{
        //    CategoriaId = categoriaCriada.CategoriaId,                                                  DTO MANUAL
        //    Nome = categoriaCriada.Nome,
        //    ImagemUrl = categoriaCriada.ImagemUrl
        //};
        var novacategoriaDto = categoria.ToCategoriaDTO();                //DTO Extensão
        return new CreatedAtRouteResult("ObterCategoria", new { id = novacategoriaDto.CategoriaId }, novacategoriaDto);
    }



    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }
        //var categoria = new Categoria()
        //{
        //    CategoriaId = categoriaDto.CategoriaId,                                                  DTO MANUAL
        //    Nome = categoriaDto.Nome,
        //    ImagemUrl = categoriaDto.ImagemUrl
        //};
        var categoria = categoriaDto.ToCategoria();                //DTO Extensão
        var categoriaAtualizada = _uow.CategoriaRepository.Create(categoria);
        await _uow.CommitAsync();

        //var categoriaAtualizadaDto = new CategoriaDTO()
        //{
        //    CategoriaId = categoriaAtualizada.CategoriaId,                                                  DTO MANUAL
        //    Nome = categoriaAtualizada.Nome,
        //    ImagemUrl = categoriaAtualizada.ImagemUrl
        //};
        var categoriaAtualizadaDto = categoria.ToCategoriaDTO();                //DTO Extensão
        return Ok(categoriaAtualizadaDto);
    }


    [Authorize(AuthenticationSchemes = "Bearer", Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = _uow.CategoriaRepository.Delete(id);

        if (categoria is null)
        {
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaExcluida = _uow.CategoriaRepository.Delete(id);
        await _uow.CommitAsync();
        //var categoriaExcluidaAtualizadaDto = new CategoriaDTO()
        //{
        //    CategoriaId = categoriaExcluida.CategoriaId,                                                   DTO MANUAL
        //    Nome = categoriaExcluida.Nome, 
        //    ImagemUrl = categoriaExcluida.ImagemUrl
        //};
        var categoriaExcluidaAtualizadaDto = categoria.ToCategoriaDTO();              //DTO Extensão
        return Ok(categoriaExcluidaAtualizadaDto);
    }



    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters) //retorna uma lista de produtos
    {
        var resultados = await _uow.CategoriaRepository.GetAllAsync(categoriasParameters);
        return ObterCategorias(resultados);

    }


    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IPagedList<Categoria>>> GetCategoriasFiltradas( [FromQuery] CategoriasFiltroNome categoriasFiltro)
    {
        var categoriasFiltradas = await _uow.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);

        //  return ObterCategorias(categoriasFiltradas);
        return Ok(categoriasFiltradas);

    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
    {
        //var metadata = new
        //{
        //    categorias.TotalCount,
        //    categorias.PageSize,
        //    categorias.CurrentPage,                só vai mudar o nome                      
        //    categorias.TotalPages,
        //    categorias.HasNext,
        //    categorias.HasPrevious
        //};

        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));
        var categoriasDto = categorias.ToCategoriaDTOList();
        return Ok(categoriasDto);
    }



}
