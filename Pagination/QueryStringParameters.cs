namespace MinhaAPI.Pagination
{
    public abstract class QueryStringParameters //classe abstrata não pode criar objetos apartir dela, é utilizada como uma classe base para outras bases
    {
        const int maxPageSize = 50;
        public int _pageNumber { get; set; } = 1; //numero da página e o inicial vai ser o 1
        private int _pageSize = maxPageSize; //controla o tamanho da página
        public int PageSize { get { return _pageSize; } set { _pageSize = (value > maxPageSize) ? maxPageSize : value; } } //no set estamos falando se o value é maior será definido como maxpagesize
    }
}
