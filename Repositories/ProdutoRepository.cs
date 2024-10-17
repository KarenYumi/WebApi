using Microsoft.EntityFrameworkCore;
using MinhaAPI.Contexto;
using MinhaAPI.Models;
using MinhaAPI.Pagination;
using X.PagedList;
using X.PagedList.EF;

namespace MinhaAPI.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context) 
        { 
            _context = context;
        }

        public async Task<List<Produto>> GetProdutosAsync() //retorna uma consulta que representa a seleção de todos os produtos da tabela no banco de dados
        {
            return await _context.Produtos.ToListAsync();
        }
        

        public Produto Create(Produto produto)
        {
            if(produto is null)
            {
                throw new ArgumentNullException("Produto é nulo");
            }
            _context.Produtos.Add(produto);
           // _context.SaveChanges();
            return produto; 

        }

        public bool Delete(int id)
        {
            var produto = _context.Produtos.Find(id);//find so pode ser usado se id for uma chave primaria
            if (produto is not null)
            {
                _context.Produtos.Remove(produto);
               // _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<Produto> GetProdutoAsync(int id)
        {
            var produto =_context.Produtos.FirstOrDefaultAsync(c => c.ProdutoId == id);
            if (produto is null)
                throw new InvalidCastException("Produto é nulo");
            return await produto;
        }

        public bool Update(Produto produto)
        {
            if (produto is null)
            {
                throw new ArgumentNullException("Produto é nulo");
            }
            if(_context.Produtos.Any(p=>p.ProdutoId == produto.ProdutoId))
            {
                _context.Produtos.Update(produto);
               // _context.SaveChanges();
                return true;
            }
            return false;
        }

        //Primeiro código da paginação antes de adicioanr o pagedlist

        //public IEnumerable<Produto> GetAll(ProdutosParameters produtosParams)
        //{
        //    return GetProdutos().OrderBy(p => p.Nome).Skip((produtosParams._pageNumber - 1)* produtosParams.PageSize).Take(produtosParams.PageSize).ToList();
        //    //calcula quantos itens deve ser pulada na coleção de dados antes de começar retornar os itens em take
        //}

        //mudei o código para ficar assincrono
        public async Task<IPagedList<Produto>> GetAllAsync(ProdutosParameters produtosParams)
        {
            
            var produtosOrdenados = _context.Produtos.OrderBy(p => p.ProdutoId).AsQueryable();

            var resultado = await produtosOrdenados.ToPagedListAsync(produtosParams._pageNumber, produtosParams.PageSize);
            return resultado;
        }

        public async Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams)
        {
            //var produtos = GetProdutosAsync().AsQueryable();//Queryable tem melhor desemplenho do q ienumerable
            //quando tem asqueryable necessita fazer isto:
            var produtos = await GetProdutosAsync();


            if (produtosFiltroParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroParams.PrecoCriterio))
            {
                if (produtosFiltroParams.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco > produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco).ToList(); // Materializa a lista
                }
                else if (produtosFiltroParams.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco < produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco).ToList(); // Materializa a lista
                }
                else if (produtosFiltroParams.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
                {
                    produtos = produtos.Where(p => p.Preco == produtosFiltroParams.Preco.Value).OrderBy(p => p.Preco).ToList(); // Materializa a lista
                }
            }
            var produtosFiltrados = await _context.Produtos.ToPagedListAsync( produtosFiltroParams._pageNumber,produtosFiltroParams.PageSize);
            return produtosFiltrados;
        }

        

        public async Task<List<Produto>> GetProdutosPorCategoriasAsync(int categoriaId)
        {
            var produtos = _context.Produtos.Where(p => p.CategoriaId == categoriaId).OrderBy(p => p.ProdutoId).AsQueryable();
            return await produtos.ToListAsync();
        }

        //public IEnumerable<Produto> GetProdutosPorCategoria(int id)
        //{
        //    return GetAll().Where(c => c.CategoriaId == id);
        //}

    }
}
