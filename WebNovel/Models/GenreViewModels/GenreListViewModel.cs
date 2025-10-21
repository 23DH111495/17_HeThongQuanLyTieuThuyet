using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebNovel.Models.ViewModels;

namespace WebNovel.Models.GenreViewModels
{
    public class GenreListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // Tác giả
        public AuthorViewModel Author { get; set; }

        // Ảnh bìa
        public string CoverImageUrl { get; set; }
        public byte[] CoverImage { get; set; }
        public string CoverImageContentType { get; set; }
        public bool HasCoverImage => CoverImage != null && CoverImage.Length > 0;

        // Thể loại
        public List<string> Genres { get; set; } = new List<string>();

        // Dùng để sắp xếp theo mức độ trùng khớp khi lọc thể loại
        public int MatchCount { get; set; } = 0;

        //  Hashtags
        public virtual ICollection<NovelTag> NovelTags { get; set; }
        public List<string> Hashtags { get; set; } = new List<string>();
        public int HashtagMatchCount { get; set; } = 0;
    }
}