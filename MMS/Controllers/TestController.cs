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
    public class TestController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.DominioWeb = Seguridadcll.Configuracion.ConfigDominioWeb;
            return View(Seguridadcll.Aplicaciones);
        }

        public ActionResult Unauthorized()
        {
            ViewBag.ObjetoId = Request.Params["objetoId"];
            ViewBag.Layout = "~/Views/Shared/_LayoutTT.cshtml";
            if (Request.IsAjaxRequest())
            {
                ViewBag.Layout = null;
                return PartialView();
            }
            else
                return View();
        }
    }
}