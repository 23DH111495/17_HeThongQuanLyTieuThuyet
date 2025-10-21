using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class ChapterNavigationInfo
    {
        public int NovelId { get; set; }
        public int CurrentChapterNumber { get; set; }
        public int? PreviousChapterNumber { get; set; }
        public int? NextChapterNumber { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int CurrentPosition { get; set; }
        public int TotalChapters { get; set; }
    }
}