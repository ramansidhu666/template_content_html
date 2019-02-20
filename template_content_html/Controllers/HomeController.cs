using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HomeHelp.Services;
namespace HomeHelp.Controllers
{
    public class HomeController : Controller
    {
        //public ICompanyService _CompanyService { get; set; }
        //public HomeController(ICompanyService CompanyService)
        //{
        //    this._CompanyService = CompanyService;
        //}
        public ActionResult Index()
        {
            ViewBag.Title = "Welcome";
            //ViewBag.IsPartial = "Y";
            //ViewBag.HideSideContactUs = "F";
            //ViewBag.ModelPopup = "";
            //Session["ModelPopup"] = "";
            return View();
        }

        //public ActionResult Company()
        //{
        //    Session["CompanyID"] = 0; //Set Company Id to 0, so company will be choose from Choose Company View
        //    Session["CompanyName"] = "";//Set Company Name Blank
        //    Session["LogoPath"] = ""; //Set Logo
        //    ViewBag.CompanyList =_CompanyService.GetCompanies().OrderBy(x => x.CompanyName);

        //    return View();
        //}

        public ActionResult WelcomeHome()
        {
            ViewBag.Title = "Welcome";
            return View();
        }


        public ActionResult About()
        {
            ViewBag.HideSideContactUs = "F";
            ViewBag.Title = "About Us";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}