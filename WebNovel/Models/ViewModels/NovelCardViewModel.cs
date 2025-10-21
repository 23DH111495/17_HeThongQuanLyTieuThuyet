using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    // File: Models/NovelCardViewModel.cs

    public class NovelCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; } // Thêm Slug để tạo link thân thiện SEO
        public string Status { get; set; }
        public string CoverImageUrl { get; set; }

        // Sửa kiểu dữ liệu cho chính xác với model Novel
        public long BookmarkCount { get; set; } // Chuyển từ int sang long
        public decimal AverageRating { get; set; } // Chuyển từ double sang decimal

        public int TotalChapters { get; set; }
        public string GenreName { get; set; }
        public string AuthorName { get; set; } // Thêm tên tác giả
    }
}