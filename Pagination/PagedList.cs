namespace MinhaAPI.Pagination
{
    public class PagedList<T> : List<T> where T : class
    {
        public int CurrentPage { get;private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize); //total de paginas dividindo o total de itens pelo tamanho da página

            AddRange(items);
        }
        //é static para n ter que criar uma classe para usar o ToPagedList
        public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)//recebe uma fonte de dados IQueryable, pode ser consultado diretamente
        {
            var count = source.Count();//consulta o numero total que temos
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();//busca o elemnteo na pagina atual
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
