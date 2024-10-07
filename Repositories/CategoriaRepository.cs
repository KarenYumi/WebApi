using Microsoft.EntityFrameworkCore;
using MinhaAPI.Contexto;
using MinhaAPI.DTOs;
using MinhaAPI.Models;
using MinhaAPI.Pagination;
using X.PagedList;
using X.PagedList.EF;

namespace MinhaAPI.Repositories //AQUI TEM APENAS A LÓGICA DE ACESSO A DADOS    
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;
        //tirei o savechanges pq ele já faz isso no unit of work
        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public Categoria Create(Categoria categoria)
        {
            if (categoria is null)
                throw new ArgumentNullException(nameof(categoria));
            
            _context.Categorias.Add(categoria);//inclui na memória
            //_context.SaveChanges();//inclui no banco de dados
            return categoria;
        }

        public Categoria Delete(int id)
        {
            var categoria= _context.Categorias.Find(id);//encontramos a categoria que quermos excluir pelo find id
            if(categoria is null)
            {
                throw new ArgumentException(nameof(categoria));
            }
            _context.Categorias.Remove(categoria);
            //_context.SaveChanges();
            return categoria;
        }

        public async Task<IEnumerable<Categoria>> GetCategoirasAsync()
        {
            return  await _context.Categorias.ToListAsync();//retorna o objeto categorias
        }

        public async Task<Categoria> GetCategoriaAsync(int id)
        {
           return await _context.Categorias.FirstOrDefaultAsync(c => c.CategoriaId == id); //FirstorDefault retorna o primeiro ou null
            //Acessa as propriedades da categoria e tem que ser == (igual) ao id
        }

        public Categoria Update(Categoria categoria)
        {
            if (categoria is null)
                throw new ArgumentNullException(nameof(categoria));
            
            _context.Entry(categoria).State = EntityState.Modified;
           // _context.SaveChanges();
            return categoria;

        }

        public async Task<IPagedList<Categoria>> GetAllAsync(CategoriasParameters categoriasParams)
        {
            ////var categorias = GetCategoiras().OrderBy(p => p.CategoriaId).AsQueryable();

            //var categorias = await GetCategoirasAsync();
            ////orderBy sincrono
            //var categoriasOrdenadas = categorias.OrderBy(p => p.CategoriaId).AsQueryable();
            ////var resultado = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParams._pageNumber, categoriasParams.PageSize);
            //var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParams._pageNumber, categoriasParams.PageSize);

            //return resultado;

            var categorias = _context.Categorias.OrderBy(c => c.CategoriaId);

            // Retorna a lista paginada de forma assíncrona
            var resultado = await categorias.ToPagedListAsync(categoriasParams._pageNumber, categoriasParams.PageSize);
            return resultado;
        }

        public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
        {
            //FORMA QUEE O FÁBIO ENSINOU, MAS ACHEI CONFUSO E MUDEI PRA FORMA DE BAIXO
            //IQueryable<Categoria> ct;
            //var stringDePesquisa = categoriasParams.Nome ?? string.Empty;
            //ct = from c in _context.Categorias //puxamos direto do DB pois ele vem como IQueryable direto, do que fazer oq fez no de cima onde puca pelo GetCategoirasAsync e esse formato de escrever o código se chama expression language
            //     where c.Nome.Contains(stringDePesquisa)
            //     select c;
            //return await ct.ToPagedListAsync(categoriasParams._pageNumber, categoriasParams.PageSize);


            var stringDePesquisa = categoriasParams.Nome ?? string.Empty;

            // Usa métodos de extensão LINQ para aplicar o filtro
            var categoria = _context.Categorias.Where(c => c.Nome.Contains(stringDePesquisa));

            // Retorna a lista paginada de forma assíncrona
            return await categoria.ToPagedListAsync(categoriasParams._pageNumber, categoriasParams.PageSize);
        }
    }
}
