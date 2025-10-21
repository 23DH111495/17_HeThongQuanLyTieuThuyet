using System.Web.Mvc;

namespace WebNovel.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Admin_NovelDetails",
                "Admin/NovelDetails_Manager/{action}/{id}",
                new { controller = "NovelDetails_Manager", action = "Novel_Details", id = UrlParameter.Optional },
                new[] { "WebNovel.Areas.Admin.Controllers.Novel_ManagerController" }
            );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}