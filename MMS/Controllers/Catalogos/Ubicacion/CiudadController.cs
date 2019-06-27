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
    public class CiudadController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string CiudadID, string DepartamentoID, string PaisID)
        {
            var ciudad = db.Ciudad.Include(c => c.departamentos.paises).Where(c => c.CiudadID == CiudadID && c.DepartamentoID == DepartamentoID && c.PaisID == PaisID).FirstOrDefault();
            if (ciudad == null)
                return HttpNotFound();

            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");

            return PartialView(GetCrudMode().ToString(), ciudad);
        }

        [AuthorizeAction]
        [FillPermission("Departamento/Edit")]
        public async Task<ActionResult> Details(string CiudadID, string DepartamentoID, string PaisID)
        {
            return await GetView(CiudadID, DepartamentoID, PaisID);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Ciudad model)
        {
            if (ModelState.IsValid)
            {
                var ciudadTemp = db.Ciudad.Where(c => c.CiudadID == model.CiudadID && c.DepartamentoID == model.DepartamentoID && c.PaisID == model.PaisID).FirstOrDefault();
                if (ciudadTemp == null)
                {
                    db.Ciudad.Add(model);
                await db.SaveChangesAsync();
                AddLog("", model.CiudadID, model);

                return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, The City already exists.");
                }
            }
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string CiudadID, string DepartamentoID, string PaisID)
        {
            return await GetView(CiudadID, DepartamentoID, PaisID);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(Ciudad model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.CiudadID, model);

                return RedirectToAction("Index", GetReturnSearch());
            }
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string CiudadID, string DepartamentoID, string PaisID)
        {
            return await GetView(CiudadID, DepartamentoID, PaisID);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string CiudadID, string DepartamentoID, string PaisID)
        {
            var ciudad = await db.Ciudad.FindAsync(CiudadID, DepartamentoID, PaisID);
            try
            {
                db.Ciudad.Remove(ciudad);
                await db.SaveChangesAsync();
                AddLog("", ciudad.CiudadID, ciudad);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(CiudadID, DepartamentoID, PaisID);
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
        //// GET: Ciudad
        //public ActionResult Index()
        //{
        //    var ciudad = db.Ciudad.Include(c => c.departamentos.paises);
        //    return View(ciudad.ToList());
        //}

        //// GET: Ciudad/Details/5
        //[Seguridad]
        //public ActionResult Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ciudad ciudad = db.Ciudad.Find(id);
        //    if (ciudad == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(ciudad);
        //}

        //[Seguridad]
        //// GET: Ciudad/Create
        //public ActionResult Create()
        //{
        //    List<Pais> lstPais = db.Pais.ToList();
        //    lstPais.Insert(0, new Pais { PaisID = "0", PaisDesc = "Seleccione un País" });
        //    List<Departamento> lstDepartamento = new List<Departamento>();
        //    ViewBag.PaisID = new SelectList(lstPais, "PaisID", "PaisDesc");
        //    ViewBag.DepartamentoID = new SelectList(lstDepartamento, "DepartamentoID", "PaisID", "DepartamentoDesc");
        //    return View();
        //}

        // Para enviar la lista resultante al AJAX
        public JsonResult GetDropDownInCascada(string id)
        {
            List<Departamento> departamentos = new List<Departamento>();
            if (id != null)
            {
                departamentos = null;
                departamentos = db.Departamento.Where(p => p.PaisID == id).ToList();

            }
            var result = (from r in departamentos
                          select new
                          {
                              id = r.DepartamentoID,
                              name = r.DepartamentoDesc
                          }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //// POST: Ciudad/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "CiudadID,DepartamentoID,PaisID,CiudadDesc")] Ciudad ciudad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var ciudadTemp = db.Ciudad.Where(u => u.CiudadID == ciudad.CiudadID && u.DepartamentoID == ciudad.DepartamentoID && u.PaisID == ciudad.PaisID).FirstOrDefault();
        //            if (ciudadTemp == null)
        //            {
        //                db.Ciudad.Add(ciudad);
        //                db.SaveChanges();

        //                //Auditoria
        //                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //                Auditoria auditoria = new Auditoria();
        //                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //                auditoria.AuditoriaFecha = System.DateTime.Now;
        //                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //                auditoria.AuditoriaEvento = "Crear";
        //                auditoria.AuditoriaDesc = "Crea Ciudad: " + ciudad.CiudadID + " con Departamento: " + ciudad.DepartamentoID + " y Pais: " + ciudad.PaisID;
        //                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //                seguridad.insertAuditoria(auditoria);
        //                //Auditoria

        //                return RedirectToAction("Index");
        //            }
        //            else
        //            {
        //                ViewBag.error = "Advertencia, La Ciudad " + ciudad.CiudadID + " a crear ya existe.";
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }
        //    }
        //    ViewBag.DepartamentoID = new SelectList(db.Departamento, "DepartamentoID", "DepartamentoDesc", ciudad.DepartamentoID);
        //    return View(ciudad);
        //}

        //[Seguridad]
        //// GET: Ciudad/Edit/5
        //public ActionResult Edit(string id, string idF1, string idF2)
        //{
        //    if (id == null || idF1 == null || idF2 == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ciudad ciudad = db.Ciudad.Include(c => c.departamentos.paises).Where(u => u.CiudadID == id && u.PaisID == idF1 && u.DepartamentoID == idF2).FirstOrDefault();
        //    if (ciudad == null)
        //    {
        //        //return HttpNotFound();    
        //        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
        //    }
        //    return View(ciudad);
        //}

        //// POST: Ciudad/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "CiudadID,DepartamentoID,PaisID,CiudadDesc")] Ciudad ciudad)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            db.Entry(ciudad).State = EntityState.Modified;
        //            db.SaveChanges();

        //            //Auditoria
        //            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //            Auditoria auditoria = new Auditoria();
        //            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //            auditoria.AuditoriaFecha = System.DateTime.Now;
        //            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //            auditoria.AuditoriaEvento = "Modificar";
        //            auditoria.AuditoriaDesc = "Modificó Ciudad: " + ciudad.CiudadID + " del Departamento: " + ciudad.DepartamentoID + " del Pais: " + ciudad.PaisID;
        //            auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //            seguridad.insertAuditoria(auditoria);
        //            //Auditoria

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception e)
        //        {
        //            var ciudadTemp = db.Ciudad.Where(u => u.CiudadID == ciudad.CiudadID).FirstOrDefault();
        //            if (ciudadTemp == null)
        //            {
        //                ViewBag.error = "Advertencia, la Ciudad " + ciudad.CiudadID + " ya no existe.";
        //            }
        //            else
        //            {
        //                ViewBag.error = e.ToString();
        //            }
        //        }
        //    }
        //    ViewBag.DepartamentoID = new SelectList(db.Departamento, "DepartamentoID", "DepartamentoDesc", ciudad.DepartamentoID);
        //    return View(ciudad);
        //}

        //[Seguridad(isModal = true)]
        //// GET: Ciudad/Delete/5
        //public ActionResult Delete(string[] ids)
        //{
        //    //POSISIÓN [0] Es el ID de la Ciudad, [1] el del País y [2] del Departamento.
        //    string idCiudad = ids[0];
        //    string idPais = ids[1];
        //    string idDep = ids[2];
        //    if (idDep == null || idPais == null || idCiudad == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ciudad ciudades = db.Ciudad.Include(d => d.departamentos.paises).Where(u => u.CiudadID == idCiudad && u.departamentos.PaisID == idPais && u.DepartamentoID == idDep).FirstOrDefault();
        //    if (ciudades == null)
        //    {
        //        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idDep;
        //    }
        //    else
        //    {
        //        var clientes = from d in db.Clientes
        //                       where d.CiudadID == ciudades.CiudadID
        //                       select d;
        //        Cliente cliente = clientes.Include(d => d.ciudad).FirstOrDefault();

        //        if (cliente == null)
        //        {
        //            return View(ciudades);
        //        }
        //        else
        //        {
        //            return RedirectToAction("DeleteAlert", "Ciudad", new { idCiudad = ciudades.CiudadID, idPais = ciudades.departamentos.paises.PaisID, idDep = ciudades.departamentos.DepartamentoID });
        //        }
        //    }
        //    return View(ciudades);
        //}

        //// GET: Pais
        //public ActionResult DeleteAlert(string idCiudad, string idPais, string idDep)
        //{
        //    Ciudad ciudades = db.Ciudad.Where(u => u.CiudadID == idCiudad && u.departamentos.PaisID == idPais && u.DepartamentoID == idDep).FirstOrDefault();

        //    return View(ciudades);
        //}

        //// POST: Ciudad/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string[] ids)
        //{
        //    //POSISIÓN [0] Es el ID de la Ciudad, [1] el del País y [2] del Departamento.
        //    string idCiudad = ids[0];
        //    string idPais = ids[1];
        //    string idDep = ids[2];
        //    try
        //    {
        //        Ciudad ciudades = db.Ciudad.Where(u => u.CiudadID == idCiudad && u.departamentos.PaisID == idPais && u.DepartamentoID == idDep).FirstOrDefault();
        //        db.Ciudad.Remove(ciudades);
        //        db.SaveChanges();

        //        //Auditoria
        //        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //        Auditoria auditoria = new Auditoria();
        //        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //        auditoria.AuditoriaFecha = System.DateTime.Now;
        //        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //        auditoria.AuditoriaEvento = "Eliminar";
        //        auditoria.AuditoriaDesc = "Eliminó Ciudad: " + ciudades.CiudadID + " del Departamento: " + ciudades.DepartamentoID + " del Pais: " + ciudades.PaisID;
        //        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

        //        seguridad.insertAuditoria(auditoria);
        //        //Auditoria
        //    }
        //    catch (Exception e)
        //    {
        //        var ciudadTemp = db.Ciudad.Where(u => u.CiudadID == idCiudad && u.departamentos.PaisID == idPais && u.DepartamentoID == idDep).FirstOrDefault();
        //        if (ciudadTemp == null)
        //        {
        //            ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idCiudad;
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
