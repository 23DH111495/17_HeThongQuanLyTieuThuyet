using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebNovel.Models;

namespace WebNovel.ViewModels
{
    public class NovelListViewModel
    {
        public List<NovelViewModel> Novels { get; set; } = new List<NovelViewModel>();
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public List<Author> Authors { get; set; } = new List<Author>();
        public string SearchTerm { get; set; }
        public int? SelectedGenreId { get; set; }
    }
}