using MinhaAPI.Models;
using MinhaAPI.Pagination;
using X.PagedList;

namespace MinhaAPI.Repositories
{
    public interface IProdutoRepository
    {
        //aleterei eles para assíncronas
        Task<IQueryable<Produto>> GetProdutosAsync();
        Task<Produto> GetProdutoAsync(int id);
        Produto Create(Produto produto);
        bool Update(Produto produto);
        bool Delete(int id);

        //IEnumerable<Produto> GetAll(ProdutosParameters produtosParams); //assinatura de método 
        Task<IPagedList<Produto>> GetAllAsync(ProdutosParameters produtosParams);

        Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams);
        Task<List<Produto>> GetProdutosPorCategoriasAsync(int categoriaId);
    }
}
