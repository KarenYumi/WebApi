using MinhaAPI.Contexto;

namespace MinhaAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private IProdutoRepository? _produtoRepo;

        private ICategoriaRepository? _categoriaRepo;


        public AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IProdutoRepository ProdutoRepository
        { //pega uma instancia de IProdutoRepository e quando invocar uma instância de repositório se não tiver uma pronta ele cria uma nova e passa a instância do contexto
            get
            {
                return _produtoRepo = _produtoRepo ?? new ProdutoRepository(_context); //?? é o operador de coalescência nula, vê se não existe e cria um novo
            }
        }
        public ICategoriaRepository CategoriaRepository
        { 
            get
            {
                return _categoriaRepo = _categoriaRepo ?? new CategoriaRepository(_context); //?? é o operador de coalescência nula, vê se não existe e cria um novo
            }
        }

        public async Task CommitAsync()
        {
            _context.SaveChangesAsync();
        }
        //responsavel para liberar recursos associados ao contexto do banco de dados ao _context
        public void Dispose()
        {
            _context.Dispose();
        }   
    }
}
