using Microsoft.EntityFrameworkCore;
using MinhaAPI.Contexto;
using MinhaAPI.Models;
using MinhaAPI.Pagination;

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

        public async Task<PagedList<Categoria>> GetAllAsync(CategoriasParameters categoriasParams)
        {
            //var categorias = GetCategoiras().OrderBy(p => p.CategoriaId).AsQueryable();
            var categorias = await GetCategoirasAsync();
            var categoriasOrdenadas = categorias.OrderBy(p => p.CategoriaId).AsQueryable();
            var categoriasOrdenados = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParams._pageNumber, categoriasParams.PageSize);
            return categoriasOrdenados;
        }

        public async Task<PagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams)
        {
            //var categorias = GetCategoiras().AsQueryable();
            var categorias = await GetCategoirasAsync();
            if (!string.IsNullOrEmpty(categoriasParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome.Contains(categoriasParams.Nome));
            }

            var categoriasFiltradas = PagedList<Categoria>.ToPagedList(categorias.AsQueryable(),
                                       categoriasParams._pageNumber, categoriasParams.PageSize);

            return categoriasFiltradas;
        }
    }
}
