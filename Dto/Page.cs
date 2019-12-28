using System.Collections.Generic;

namespace UrbanSisters.Dto
{
    public class Page<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
    }
}