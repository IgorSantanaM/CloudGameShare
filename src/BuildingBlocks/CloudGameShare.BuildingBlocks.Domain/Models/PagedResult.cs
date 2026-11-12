using System;
using System.Collections.Generic;
using System.Text;

namespace CloudGameShare.BuildingBlocks.Domain.Models
{
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

    }
}
