using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MMS.Filters;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MMS.Controllers
{
    public class HomeController : Controller
    {
        [Seguridad]
        public ActionResult Index()
        {

            return RedirectToAction("Index", "Test");
            //return View();
        }

        [Seguridad]
        public ActionResult Home()
        {
            return View();
        }

        public ActionResult NotAccess()
        {
            ViewBag.UsuarioId = TempData["UsuarioId"];
            ViewBag.ObjetoId = TempData["ObjetoId"];
            return View();
        }

        public ActionResult _NotAccess()
        {
            //ViewBag.UsuarioId = TempData["UsuarioId"];
            //ViewBag.ObjetoId = TempData["ObjetoId"];
            return PartialView();
        }

    }
}