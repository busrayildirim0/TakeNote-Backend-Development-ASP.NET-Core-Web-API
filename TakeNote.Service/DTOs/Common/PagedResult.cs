namespace TakeNote.Service.DTOs
{
    // Generic (<T>) bir yapı kuruyoruz ki hem Notlar hem de başka şeyler için kullanabilelim.
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}