using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using MMS.Classes;
using System.IO;

namespace MMS.Controllers.Catalogos.Ubicacion
{
    public class DepartamentoController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string DepartamentoID, string PaisID)
        {
            var departamento = db.Departamento.Include(d => d.paises).Where(d => d.DepartamentoID == DepartamentoID && d.PaisID == PaisID).FirstOrDefault();
            //var departamento = await db.Departamento.FindAsync(DepartamentoID, PaisID);
            if (departamento == null)
                return HttpNotFound();

            ViewData["PaisID"] = new SelectList(await db.Pais.Select(d => new { d.PaisID, d.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");

            return PartialView(GetCrudMode().ToString(), departamento);
        }

        [AuthorizeAction]
        [FillPermission("Departamento/Edit")]
        public async Task<ActionResult> Details(string DepartamentoID, string PaisID)
        {
            return await GetView(DepartamentoID, PaisID);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(d => new { d.PaisID, d.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Departamento model)
        {
            if (ModelState.IsValid)
            {
                var dptoTemp = db.Departamento.Where(d => d.DepartamentoID == model.DepartamentoID && d.PaisID == model.PaisID).FirstOrDefault();
                if (dptoTemp == null)
                {
                    db.Departamento.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.DepartamentoID, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, The Department already exists.");
                }
            }
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string DepartamentoID, string PaisID)
        {
            return await GetView(DepartamentoID, PaisID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(Departamento model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.DepartamentoID, model);

                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string DepartamentoID, string PaisID)
        {
            return await GetView(DepartamentoID, PaisID);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string DepartamentoID, string PaisID)
        {
            var departamento = await db.Departamento.FindAsync(DepartamentoID, PaisID);
            try
            {
                db.Departamento.Remove(departamento);
                await db.SaveChangesAsync();
                AddLog("", departamento.DepartamentoID, departamento);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(DepartamentoID, PaisID);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //[Seguridad]
        //// GET: Departamento
        //public ActionResult Index()
        //{
        //    var departamento = db.Departamento.Include(d => d.paises);
        //    return View(departamento.ToList());
        //}

        //// GET: Departamento/Details/5
        //[Seguridad]
        //public ActionResult Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Departamento departamento = db.Departamento.Find(id);
        //    if (departamento == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(departamento);
        //}

        //[Seguridad]
        //// GET: Departamento/Create
        //public ActionResult Create()
        //{
        //    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc");
        //    return View();
        //}

        //// POST: Departamento/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "DepartamentoID,PaisID,DepartamentoDesc")] Departamento departamento)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var depTemp = db.Departamento.Where(u => u.DepartamentoID == departamento.DepartamentoID && u.PaisID == departamento.PaisID).FirstOrDefault();
        //            if (depTemp == null)
        //            {
        //                db.Departamento.Add(departamento);
        //                db.SaveChanges();

        //                //Auditoria
        //                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //                Auditoria auditoria = new Auditoria();
        //                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //                auditoria.AuditoriaFecha = System.DateTime.Now;
        //                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //                auditoria.AuditoriaEvento = "Crear";
        //                auditoria.AuditoriaDesc = "Crea Departamento: " + departamento.DepartamentoID + " en Pais: " + departamento.PaisID;
        //                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //                seguridad.insertAuditoria(auditoria);
        //                //Auditoria

        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.error = "Advertencia, el Departamento " + departamento.DepartamentoID + " a crear ya existe.";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }
        //    }
        //    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc", departamento.PaisID);
        //    return View(departamento);
        //}

        //[Seguridad]
        //// GET: Departamento/Edit/5
        //public ActionResult Edit(string id, string idF1)
        //{
        //    if (id == null || idF1 == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Departamento departamento = db.Departamento.Include(d => d.paises).Where(u => u.DepartamentoID == id && u.PaisID == idF1).FirstOrDefault();
        //    if (departamento == null)
        //    {
        //        //return HttpNotFound();    
        //        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
        //    }
        //    return View(departamento);
        //}

        //// POST: Departamento/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "DepartamentoID,PaisID,DepartamentoDesc")] Departamento departamento)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            db.Entry(departamento).State = EntityState.Modified;
        //            db.SaveChanges();

        //            //Auditoria
        //            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //            Auditoria auditoria = new Auditoria();
        //            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //            auditoria.AuditoriaFecha = System.DateTime.Now;
        //            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //            auditoria.AuditoriaEvento = "Modificar";
        //            auditoria.AuditoriaDesc = "Modificó Departamento: " + departamento.DepartamentoID + " del Pais: " + departamento.PaisID;
        //            auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //            seguridad.insertAuditoria(auditoria);
        //            //Auditoria

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception e)
        //        {
        //            var departamentoTemp = db.Departamento.Where(u => u.DepartamentoID == departamento.DepartamentoID).FirstOrDefault();
        //            if (departamentoTemp == null)
        //            {
        //                ViewBag.error = "Advertencia, el Departamento " + departamento.DepartamentoID + " ya no existe.";
        //            }
        //            else
        //            {
        //                ViewBag.error = e.ToString();
        //            }
        //        }
        //    }
        //    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc", departamento.PaisID);
        //    return View(departamento);
        //}

        //[Seguridad(isModal = true)]
        //// GET: Departamento/Delete/5
        //public ActionResult Delete(string[] ids)
        //{
        //    //POSISIÓN [0] Es el ID del departamento y [1] es la del País.
        //    string idDep = ids[0];
        //    string idPais = ids[1];
        //    if (idDep == null || idPais == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Departamento departamento = db.Departamento.Include(d => d.paises).Where(u => u.DepartamentoID == idDep && u.paises.PaisID == idPais).FirstOrDefault();
        //    if (departamento == null)
        //    {
        //        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idDep;
        //    }
        //    else
        //    {
        //        var ciudades = from d in db.Ciudad
        //                       where d.DepartamentoID == departamento.DepartamentoID
        //                       select d;
        //        Ciudad ciudad = ciudades.Include(d => d.departamentos).FirstOrDefault();

        //        if (ciudad == null)
        //        {
        //            return View(departamento);
        //        }
        //        else
        //        {
        //            return RedirectToAction("DeleteAlert", "Departamento", new { idDep = departamento.DepartamentoID, idPais = departamento.paises.PaisID });
        //        }
        //    }
        //    return View(departamento);
        //}

        //// GET: Pais
        //public ActionResult DeleteAlert(string idDep, string idPais)
        //{
        //    Departamento departamento = db.Departamento.Where(u => u.DepartamentoID == idDep && u.paises.PaisID == idPais).FirstOrDefault();

        //    return View(departamento);
        //}

        //// POST: Departamento/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string[] ids)
        //{
        //    //POSISIÓN [0] Es el ID del departamento y [1] es la del País.
        //    string idDep = ids[0];
        //    string idPais = ids[1];
        //    try
        //    {
        //        Departamento departamento = db.Departamento.Where(w => w.DepartamentoID == idDep && w.paises.PaisID == idPais).FirstOrDefault();
        //        db.Departamento.Remove(departamento);
        //        db.SaveChanges();

        //        //Auditoria
        //        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //        Auditoria auditoria = new Auditoria();
        //        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //        auditoria.AuditoriaFecha = System.DateTime.Now;
        //        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //        auditoria.AuditoriaEvento = "Eliminar";
        //        auditoria.AuditoriaDesc = "Eliminó Departamento: " + departamento.DepartamentoID + " del Pais: " + departamento.PaisID;
        //        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //        seguridad.insertAuditoria(auditoria);
        //        //Auditoria
        //    }
        //    catch (Exception e)
        //    {
        //        var dptoTemp = db.Departamento.Where(w => w.DepartamentoID == idDep && w.paises.PaisID == idPais).FirstOrDefault();
        //        if (dptoTemp == null)
        //        {
        //            ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idDep;
        //        }
        //        else
        //        {
        //            ViewBag.Error = e.ToString();
        //        }
        //    }
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
