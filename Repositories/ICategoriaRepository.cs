using MinhaAPI.Models;
using MinhaAPI.Pagination;
using X.PagedList;

namespace MinhaAPI.Repositories
{
    public interface ICategoriaRepository //interface tem que ser público
    {
        //IEnumerable<Categoria> GetCategoiras(); //retorna uma lista de todas as categorias
        // Categoria GetCategoria(int id); //parametro de consulta pelo id
        Task<IEnumerable<Categoria>> GetCategoirasAsync(); 
        Task<Categoria> GetCategoriaAsync(int id);
        Categoria Create(Categoria categoria); //retorna uma categoria que vai ser criada
        Categoria Update(Categoria categoria); //categoiras que iremos editar
        Categoria Delete(int id);

        Task<IPagedList<Categoria>> GetAllAsync(CategoriasParameters categoriasParams);
        Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasParams);
    }
}
