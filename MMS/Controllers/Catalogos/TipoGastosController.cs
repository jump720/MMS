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

namespace MMS.Controllers.Catalogos
{
    public class TipoGastosController : BaseController
    {
        private MMSContext db = new MMSContext();

        
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {

            return View();
        }

        private async Task<ActionResult> GetView(string id)
        {
            var TipoGastos = await db.TipoGastos.FindAsync(id);
            if (TipoGastos == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), TipoGastos);
        }

        // GET: TipoGastos/Details/5

        [AuthorizeAction]
        public async Task<ActionResult> Details(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        // GET: TipoGastos/Create
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

       
        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }




        // GET: TipoGastos/Delete/5
        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        // POST: TipoGastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [Seguridad]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string[] ids)
        {
            try
            {
                TipoGasto tipoGasto = db.TipoGastos.Find(ids);
                db.TipoGastos.Remove(tipoGasto);
                db.SaveChanges();

                //Auditoria
                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                Auditoria auditoria = new Auditoria();
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                auditoria.AuditoriaFecha = System.DateTime.Now;
                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                auditoria.AuditoriaEvento = "Eliminar";
                auditoria.AuditoriaDesc = "Eliminó TipoGasto: " + tipoGasto.TipoGastoID;
                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

                seguridad.insertAuditoria(auditoria);
                //Auditoria
            }
            catch (Exception e)
            {
                var tipoGastos = db.TipoGastos.Find(ids);
                if (tipoGastos == null)
                {
                    ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + ids;
                }
                else
                {
                    ViewBag.Error = e.ToString();
                }
            }
            return RedirectToAction("Index");
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
