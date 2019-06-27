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
    public class TipoActividadesController : BaseController
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
            var TipoActividades = await db.TipoActividades.FindAsync(id);
            if (TipoActividades == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), TipoActividades);
        }

        // GET: TipoActividades/Details/5
        [AuthorizeAction]
        public async Task<ActionResult> Details(string id)
        {
            return await GetView(id);
        }

        // GET: TipoActividades/Create
        [AuthorizeAction]
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        // POST: TipoActividades/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "TipoActividadID,TipoActividadDesc")] TipoActividad tipoActividad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var tipoActTemp = db.TipoActividades.Where(u => u.TipoActividadID == tipoActividad.TipoActividadID).FirstOrDefault();
        //            if (tipoActTemp == null)
        //            {
        //                db.TipoActividades.Add(tipoActividad);
        //                db.SaveChanges();

        //                //Auditoria
        //                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //                Auditoria auditoria = new Auditoria();
        //                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //                auditoria.AuditoriaFecha = System.DateTime.Now;
        //                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //                auditoria.AuditoriaEvento = "Crear";
        //                auditoria.AuditoriaDesc = "Crea TipoActividad: " + tipoActividad.TipoActividadID;
        //                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //                seguridad.insertAuditoria(auditoria);
        //                //Auditoria

        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Actividad " + tipoActividad.TipoActividadID + " a crear ya existe.";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }
        //    }

        //    return View(tipoActividad);
        //}


        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }


        // POST: TipoActividades/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "TipoActividadID,TipoActividadDesc")] TipoActividad tipoActividad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            db.Entry(tipoActividad).State = EntityState.Modified;
        //            db.SaveChanges();

        //            //Auditoria
        //            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //            Auditoria auditoria = new Auditoria();
        //            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //            auditoria.AuditoriaFecha = System.DateTime.Now;
        //            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //            auditoria.AuditoriaEvento = "Modificar";
        //            auditoria.AuditoriaDesc = "Modificó TipoActividad: " + tipoActividad.TipoActividadID;
        //            auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //            seguridad.insertAuditoria(auditoria);
        //            //Auditoria

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception e)
        //        {
        //            var tipoActTemp = db.TipoActividades.Where(u => u.TipoActividadID == tipoActividad.TipoActividadID).FirstOrDefault();
        //            if (tipoActTemp == null)
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Actividad " + tipoActividad.TipoActividadID + " ya no existe.";
        //            }
        //            else
        //            {
        //                ViewBag.error = e.ToString();
        //            }
        //        }
        //    }
        //    return View(tipoActividad);
        //}

        
        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        // GET: TipoActividades/DeleteAlert/
        //public ActionResult DeleteAlert(string ids)
        //{
        //    TipoActividad tipoActividad = db.TipoActividades.Find(ids);

        //    return View(tipoActividad);
        //}

        // POST: TipoActividades/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string[] ids)
        //{
        //    try
        //    {
        //        TipoActividad tipoActividad = db.TipoActividades.Find(ids);
        //        db.TipoActividades.Remove(tipoActividad);
        //        db.SaveChanges();

        //        //Auditoria
        //        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //        Auditoria auditoria = new Auditoria();
        //        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //        auditoria.AuditoriaFecha = System.DateTime.Now;
        //        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //        auditoria.AuditoriaEvento = "Eliminar";
        //        auditoria.AuditoriaDesc = "Eliminó TipoActividad: " + tipoActividad.TipoActividadID;
        //        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //        seguridad.insertAuditoria(auditoria);
        //        //Auditoria
        //    }
        //    catch (Exception e)
        //    {
        //        var tipoActTemp = db.TipoActividades.Find(ids);
        //        if (tipoActTemp == null)
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
