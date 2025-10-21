using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class ChapterListViewModel
    {
        public Novel Novel { get; set; }
        public List<Chapter> Chapters { get; set; }
    }
}