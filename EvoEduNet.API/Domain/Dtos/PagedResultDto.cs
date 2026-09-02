using System;
using System.Collections.Generic;

namespace EvoEduNet.API.Domain.Dtos
{
    public class PagedResultDto<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
        public IEnumerable<T> Items { get; set; }

        public PagedResultDto()
        {
            Items = new List<T>();
        }

        public PagedResultDto(IEnumerable<T> items, int total, int page, int pageSize)
        {
            Items = items;
            Total = total;
            Page = page;
            PageSize = pageSize;
        }
    }
}
