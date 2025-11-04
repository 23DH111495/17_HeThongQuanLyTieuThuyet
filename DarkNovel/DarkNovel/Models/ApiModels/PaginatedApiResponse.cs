using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DarkNovel.Models.ApiModels
{
    public class PaginatedApiResponse<T> : ApiResponse<T>
    {
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}