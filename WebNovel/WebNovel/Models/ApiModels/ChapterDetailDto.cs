using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ApiModels
{
    public class ChapterDetailDto
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int WordCount { get; set; }
        public DateTime PublishDate { get; set; }
        public bool IsPremium { get; set; }
        public int UnlockPrice { get; set; }
        public string PreviewContent { get; set; } // For locked chapters
    }
}