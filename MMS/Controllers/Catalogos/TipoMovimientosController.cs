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
    public class TipoMovimientosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: TipoMovimientos
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string id)
        {
            var TipoMovimientos = await db.TipoMovimientos.FindAsync(id);
            if (TipoMovimientos == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), TipoMovimientos);
        }

        // GET: TipoMovimientos/Details/5
        [AuthorizeAction]
        public async Task<ActionResult> Details(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        // POST: TipoMovimientos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "TipoMovimientoID,TipoMovimientoDesc")] TipoMovimiento tipoMovimiento)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var tipoMovTemp = db.TipoMovimientos.Where(u => u.TipoMovimientoID == tipoMovimiento.TipoMovimientoID).FirstOrDefault();
        //            if (tipoMovTemp == null)
        //            {
        //                db.TipoMovimientos.Add(tipoMovimiento);
        //                db.SaveChanges();

        //                //Auditoria
        //                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //                Auditoria auditoria = new Auditoria();
        //                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //                auditoria.AuditoriaFecha = System.DateTime.Now;
        //                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //                auditoria.AuditoriaEvento = "Crear";
        //                auditoria.AuditoriaDesc = "Crea TipoMovimiento: " + tipoMovimiento.TipoMovimientoID;
        //                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //                seguridad.insertAuditoria(auditoria);
        //                //Auditoria

        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Moviminto " + tipoMovimiento.TipoMovimientoID + " a crear ya existe.";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }
        //    }
        //    return View(tipoMovimiento);
        //}

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }

        // POST: TipoMovimientos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "TipoMovimientoID,TipoMovimientoDesc")] TipoMovimiento tipoMovimiento)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            db.Entry(tipoMovimiento).State = EntityState.Modified;
        //            db.SaveChanges();

        //            //Auditoria
        //            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //            Auditoria auditoria = new Auditoria();
        //            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //            auditoria.AuditoriaFecha = System.DateTime.Now;
        //            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //            auditoria.AuditoriaEvento = "Modificar";
        //            auditoria.AuditoriaDesc = "Modificó TipoMovimiento: " + tipoMovimiento.TipoMovimientoID;
        //            auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //            seguridad.insertAuditoria(auditoria);
        //            //Auditoria

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception e)
        //        {
        //            var tipoMovTemp = db.TipoMovimientos.Where(u => u.TipoMovimientoID == tipoMovimiento.TipoMovimientoID).FirstOrDefault();
        //            if (tipoMovTemp == null)
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Movimiento " + tipoMovimiento.TipoMovimientoID + " ya no existe.";
        //            }
        //            else
        //            {
        //                ViewBag.error = e.ToString();
        //            }
        //        }
        //    }
        //    return View(tipoMovimiento);
        //}

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        // POST: TipoMovimientos/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string[] ids)
        //{
        //    try
        //    {
        //        TipoMovimiento tipoMovimiento = db.TipoMovimientos.Find(ids);
        //        db.TipoMovimientos.Remove(tipoMovimiento);
        //        db.SaveChanges();

        //        //Auditoria
        //        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //        Auditoria auditoria = new Auditoria();
        //        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //        auditoria.AuditoriaFecha = System.DateTime.Now;
        //        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //        auditoria.AuditoriaEvento = "Eliminar";
        //        auditoria.AuditoriaDesc = "Eliminó TipoMovimiento: " + tipoMovimiento.TipoMovimientoID;
        //        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //        seguridad.insertAuditoria(auditoria);
        //        //Auditoria
        //    }
        //    catch (Exception e)
        //    {
        //        var tipoMovimientos = db.TipoMovimientos.Find(ids);
        //        if (tipoMovimientos == null)
        //        {
        //            ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + ids;
        //        }
        //        else
        //        {
        //            ViewBag.Error = e.ToString();
        //        }
        //    }
        //    return RedirectToAction("Index");
        //}

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
