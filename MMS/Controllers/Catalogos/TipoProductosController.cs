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
    public class TipoProductosController : BaseController
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
            var TipoProductos = await db.TipoProductos.FindAsync(id);
            if (TipoProductos == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), TipoProductos);
        }

        // GET: TipoProductos/Details/5
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
        // POST: TipoProductos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "TipoProductoID,TipoProductoDesc")] TipoProducto tipoProducto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var tipoProdTemp = db.TipoProductos.Where(u => u.TipoProductoID == tipoProducto.TipoProductoID).FirstOrDefault();
        //            if (tipoProdTemp == null)
        //            {
        //                db.TipoProductos.Add(tipoProducto);
        //                db.SaveChanges();

        //                //Auditoria
        //                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //                Auditoria auditoria = new Auditoria();
        //                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //                auditoria.AuditoriaFecha = System.DateTime.Now;
        //                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //                auditoria.AuditoriaEvento = "Crear";
        //                auditoria.AuditoriaDesc = "Crea TipoProducto: " + tipoProducto.TipoProductoID;
        //                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //                seguridad.insertAuditoria(auditoria);
        //                //Auditoria

        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Producto " + tipoProducto.TipoProductoID + " a crear ya existe.";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }
        //    }

        //    return View(tipoProducto);
        //}

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }

        // POST: TipoProductos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "TipoProductoID,TipoProductoDesc")] TipoProducto tipoProducto)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            db.Entry(tipoProducto).State = EntityState.Modified;
        //            db.SaveChanges();

        //            //Auditoria
        //            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //            Auditoria auditoria = new Auditoria();
        //            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //            auditoria.AuditoriaFecha = System.DateTime.Now;
        //            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //            auditoria.AuditoriaEvento = "Modificar";
        //            auditoria.AuditoriaDesc = "Modificó TipoProducto: " + tipoProducto.TipoProductoID;
        //            auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //            seguridad.insertAuditoria(auditoria);
        //            //Auditoria

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception e)
        //        {
        //            var tipoProdTemp = db.TipoProductos.Where(u => u.TipoProductoID == tipoProducto.TipoProductoID).FirstOrDefault();
        //            if (tipoProdTemp == null)
        //            {
        //                ViewBag.error = "Advertencia, el Tipo de Producto " + tipoProducto.TipoProductoID + " ya no existe.";
        //            }
        //            else
        //            {
        //                ViewBag.error = e.ToString();
        //            }
        //        }
        //    }
        //    return View(tipoProducto);
        //}

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        // GET: TipoProductos/DeleteAlert/
        public ActionResult DeleteAlert(string ids)
        {
            TipoProducto tipoProducto = db.TipoProductos.Find(ids);

            return View(tipoProducto);
        }

        // POST: TipoProductos/Delete/5
        [HttpPost, ActionName("Delete")]
        [Seguridad]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string[] ids)
        {
            try
            {
                TipoProducto tipoProducto = db.TipoProductos.Find(ids);
                db.TipoProductos.Remove(tipoProducto);
                db.SaveChanges();

                //Auditoria
                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                Auditoria auditoria = new Auditoria();
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                auditoria.AuditoriaFecha = System.DateTime.Now;
                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                auditoria.AuditoriaEvento = "Eliminar";
                auditoria.AuditoriaDesc = "Eliminó TipoProducto: " + tipoProducto.TipoProductoID;
                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

                seguridad.insertAuditoria(auditoria);
                //Auditoria
            }
            catch (Exception e)
            {
                var tipoProdTemp = db.TipoProductos.Find(ids);
                if (tipoProdTemp == null)
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
