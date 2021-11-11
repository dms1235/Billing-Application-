using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.Common.Entities
{
    public class GridParameters
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public string SortBy { get; set; }
        public bool IsAscending { get; set; }
        public string Culture { get; set; }
    }

    public class GridParameters<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public T SearchText { get; set; }
        public string SortBy { get; set; }
        public bool IsAscending { get; set; }
        public string Culture { get; set; }
    }
}
