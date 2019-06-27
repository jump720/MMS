using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using MMS.Classes;
using System.Threading.Tasks;

namespace MMS.Controllers.Transacciones
{
    public class VentasxClientesController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            //List<Cliente> clientesList = (List<Cliente>)seguridadcll.ClienteList;
            //var ventasxCliente = (from c in clientesList
            //                         join v in db.VentasxCliente.Include(v => v.cliente)
            //                                      on c.ClienteID equals v.ClienteID
            //                         select v).ToList();

            //return View(ventasxCliente.ToList());
            return View();
        }


        // Para enviar la lista resultante de meses con ventas al AJAX
        public JsonResult obtenerMesesVentas(string cliente = null, int ano = 0)
        {
            var result = new[] { new { mes = 0, valor = (decimal?)0} }.ToList();
            try
            {
                List<VentasxCliente> ventas = new List<VentasxCliente>();
                if (cliente != null && ano != 0)
                {
                    ventas = db.VentasxCliente.Where(p => p.ClienteID == cliente && p.VentasxClienteAno == ano).ToList();
                    result = null;
                    var resultTemp = (from r in ventas
                                      select new
                                      {
                                          mes = r.VentasxClienteMes,
                                          valor = r.VentasxClienteVenta
                                      }).ToList();
                    result = resultTemp;
                }
                else
                {
                    result = new[] { new { mes = 0, valor = (decimal?)0 } }.ToList(); ;
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
