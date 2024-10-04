using MinhaAPI.Models;
using MinhaAPI.Pagination;

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
        Task<PagedList<Produto>> GetAllAsync(ProdutosParameters produtosParams);

        Task<PagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroParams);
    }
}
