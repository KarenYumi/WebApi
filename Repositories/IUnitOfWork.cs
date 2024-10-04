namespace MinhaAPI.Repositories
{
    public interface IUnitOfWork
    {
        //pode usar  os repositorios genéricos, poréma apenas os métodos não específicos  
        IProdutoRepository ProdutoRepository { get; }
        ICategoriaRepository CategoriaRepository { get; }

        // void Commit();//confirma todas as alterações pendentes, iagual ao SaveChanges
        Task CommitAsync();

    }
}
