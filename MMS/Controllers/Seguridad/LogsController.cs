using MMS.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using System.Data.Entity;

namespace MMS.Controllers.Seguridad
{
    public class LogsController : BaseController
    {
        // GET: Logs
        [AuthorizeAction]
        [FillPermission("Logs/Eventos")]
        public ActionResult Index()
        {
            return View();
        }




        [AuthorizeAction]
        public async Task<ActionResult> Eventos()
        {
            using (var db = new MMSContext())
            {
                string[] eventosOmitidos = { "Logs/Events" };
                return PartialView("_Eventos", await db.Evento.Where(e => !eventosOmitidos.Contains(e.Nombre)).OrderBy(e => e.Id).ToListAsync());
            }
        }

        [AuthorizeAction]
        public ActionResult Tracking()
        {
            return PartialView("_Tracking");
        }
    }
}