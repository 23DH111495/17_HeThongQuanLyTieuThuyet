using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Novel> SliderNovels { get; set; }
        // Add other properties you might need for the home page
        public HomeIndexViewModel()
        {
            SliderNovels = new List<Novel>();
        }
    }
}