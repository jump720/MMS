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
using System.Threading.Tasks;

namespace MMS.Controllers.Seguridad
{
    public class RolesController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Roles
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var rol = await db.Roles.FindAsync(id);

            if (rol == null)
            {
                return HttpNotFound();
            }


            var objetos = await (from o in db.Objeto
                                 join ao in db.RolObjeto.Where(a => a.RolId == id) on o equals ao.Objeto into oaos
                                 where !o.ObjetoId.StartsWith("__")
                                 from aoa in oaos.DefaultIfEmpty()
                                 select new RolesViewModel.RolObjetosViewModel
                                 {
                                     ObjetoId = o.ObjetoId,
                                     Seleccionado = (aoa == null) ? false : aoa.RolObjetoActivo
                                 }).ToListAsync();

            var apps = await (from a in db.Aplicaciones
                              join ao in db.RolAplicaciones.Where(a => a.RolId == id) on a equals ao.Aplicacion into oaos
                              from aoa in oaos.DefaultIfEmpty()
                              select new RolesViewModel.AplicacionViewModel
                              {
                                  AplicacionId = a.Id,
                                  AplicacionNombre = a.Nombre,
                                  Seleccionado = (aoa.Rol == null) ? false : true
                              }).ToListAsync();

            return View(GetCrudMode().ToString(), new RolesViewModel
            {
                Rol = rol,
                Objetos = objetos,
                Apps = apps
            });
        }

        // GET: Roles/Details/5
        [AuthorizeAction]
        [FillPermission("Roles/Edit")]
        public async Task<ActionResult> Details(int id)
        {

        
            return await GetView(id);
        }

        // GET: Roles/Create
        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            //ViewBag.ObjetoList = db.Objeto.Where(o => !o.ObjetoId.StartsWith("__")).ToList();
            //ViewBag.RolObjetoList = new List<RolObjeto>();
            var apps = await db.Aplicaciones
                                      .Select(a => new RolesViewModel.AplicacionViewModel
                                      {
                                          AplicacionId = a.Id,
                                          AplicacionNombre = a.Nombre,
                                          Seleccionado = false
                                      }).ToListAsync();
            var objetos = await db.Objeto
                                    .Where(o => !o.ObjetoId.StartsWith("__"))
                                    .Select(o => new RolesViewModel.RolObjetosViewModel
                                    {
                                        ObjetoId = o.ObjetoId,
                                        Seleccionado = false
                                    }).ToListAsync();
            return View(new RolesViewModel { Objetos = objetos, Apps = apps });
        }

        // POST: Roles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        //[Bind(Include = "RolId,RolNombre")]
        public async Task<ActionResult> Create(RolesViewModel model)
        {
          
            if (ModelState.IsValid )
            {
                try
                {

                    db.Roles.Add(model.Rol);
                    db.SaveChanges();
                    await db.SaveChangesAsync();

                    //Objetos
                    foreach (var obj in model.Objetos)
                    {
                        if (obj.Seleccionado)
                        {
                            RolObjeto ao = new RolObjeto();
                            ao.RolId = model.Rol.RolId;
                            ao.ObjetoId = obj.ObjetoId;
                            ao.RolObjetoActivo = true;
                            db.RolObjeto.Add(ao);
                        }
                    }

                    //Apps
                    foreach (var app in model.Apps)
                    {
                        if (app.Seleccionado)
                        {
                            RolAplicacion ao = new RolAplicacion();
                            ao.AplicacionId = app.AplicacionId;
                            ao.RolId = model.Rol.RolId;
                            db.RolAplicaciones.Add(ao);
                        }
                    }

                    if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                    {
                        await db.SaveChangesAsync();
                        AddLog("", model.Rol.RolId, model);
                        return RedirectToAction("Index", GetReturnSearch());
                    }

                    //Guarda Objetos al rol
                    // guardaRolObjeto(RolObjetoList, rol.RolId);

                    return RedirectToAction("Index");

                }
                catch (Exception e)
                {
                    ViewBag.Error = e.ToString();
                }

            }

        

            return View(new RolesViewModel { Rol = model.Rol, Objetos = model.Objetos, Apps = model.Apps });
        }

        // GET: Roles/Edit/5
        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {

            return await GetView(id);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RolesViewModel model)
        {
    
          
            if (ModelState.IsValid )
            {
                try
                {
                    db.Entry(model.Rol).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    //Objetos
                    if (await DeleteRolObjetos(model.Rol.RolId))
                    {
                        foreach (var obj in model.Objetos)
                        {
                            if (obj.Seleccionado)
                            {
                                RolObjeto ao = new RolObjeto();
                                ao.RolId = model.Rol.RolId;
                                ao.ObjetoId = obj.ObjetoId;
                                ao.RolObjetoActivo = true;
                                db.RolObjeto.Add(ao);
                            }
                        }
                        await db.SaveChangesAsync();
                       
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error Deleting Detail (RolObjeto)");
                    }

                    //Apps
                    if (await DeleteRolAplicacion(model.Rol.RolId))
                    {
                        foreach (var app in model.Apps)
                        {
                            if (app.Seleccionado)
                            {
                                RolAplicacion ao = new RolAplicacion();
                                ao.AplicacionId = app.AplicacionId;
                                ao.RolId = model.Rol.RolId;
                                db.RolAplicaciones.Add(ao);
                            }
                        }
                        await db.SaveChangesAsync();
                        
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error Deleting Detail (RolAplicacion)");
                    }


                    AddLog("", model.Rol.RolId, model);
                    return RedirectToAction("Index", GetReturnSearch());

                    
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.ToString();
                }
            }

            
            return await GetView(model.Rol.RolId);
        }

        // GET: Roles/Delete/5
        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {


            return await GetView(id);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                /*Primero Eliminar registros de RolUsuario y RolObjetos*/
                //RolUsuario
                db.RolUsuario
                    .Where(r => r.RolId == id)
                    .ToList()
                    .ForEach(r => db.RolUsuario.Remove(r));

                await db.SaveChangesAsync();
                //RolObjetos
                await DeleteRolObjetos(id);
                //Rol APlicaciones
                await DeleteRolAplicacion(id);
                

                //Segundo se elimina registro de Rol
                Rol rol = await db.Roles.FindAsync(id);
                db.Roles.Remove(rol);
                await db.SaveChangesAsync();
                AddLog("", id, rol);
                return RedirectToAction("Index", GetReturnSearch());

            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
            }

            return RedirectToAction("Index", GetReturnSearch());
        }

        [Seguridad(isModal = true)]
        public async Task<ActionResult> RolAplicaciones(int id)
        {
            var rol = await db.Roles.FindAsync(id);
            var apps = await (from a in db.Aplicaciones
                              join r in db.RolAplicaciones.Where(a => a.RolId == id) on a equals r.Aplicacion into ars
                              from ar in ars.DefaultIfEmpty()
                              select new RolAplicacionViewModel.AplicacionView
                              {
                                  Id = a.Id,
                                  Nombre = a.Nombre,
                                  Seleccionado = (ar.Rol == null) ? false : true
                              }).ToListAsync();



            return PartialView("_RolAplicaciones", new RolAplicacionViewModel { rol = rol, aplicaciones = apps });
        }



        private async Task<bool> DeleteRolObjetos(int id)
        {
            bool result = true;
            try
            {
                var ro = await db.RolObjeto.Where(a => a.RolId == id).ToListAsync();
                if (ro.Count > 0)
                {
                    db.RolObjeto.RemoveRange(ro);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private async Task<bool> DeleteRolAplicacion(int id)
        {
            bool result = true;
            try
            {
                var ra = await db.RolAplicaciones.Where(a => a.RolId == id).ToListAsync();
                if (ra.Count > 0)
                {
                    db.RolAplicaciones.RemoveRange(ra);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        ////genera la lista con lo objetos asignados al usuario
        //private List<RolObjeto> addRolObjeto(FormCollection form, int RolId)
        //{
        //    List<RolObjeto> RolObjetoList = new List<RolObjeto>();

        //    for (int i = 2; i < form.Count; i++)
        //    {
        //        if (form.GetKey(i).ToString() == "RolNombre")
        //        {

        //        }
        //        else
        //        {
        //            RolObjeto rolobjeto = new RolObjeto();
        //            rolobjeto.RolId = RolId;
        //            rolobjeto.ObjetoId = form.GetKey(i).ToString();
        //            rolobjeto.RolObjetoActivo = true;
        //            RolObjetoList.Add(rolobjeto);
        //        }
        //    }

        //    return RolObjetoList;
        //}

        //private void guardaRolObjeto(List<RolObjeto> RolObjetoList, int RolId)
        //{
        //    db.RolObjeto
        //        .Where(r => r.RolId == RolId).ToList()
        //        .ForEach(r => db.RolObjeto.Remove(r));
        //    db.SaveChanges();

        //    foreach (var rolojeto in RolObjetoList)
        //    {
        //        rolojeto.RolId = RolId;
        //        db.RolObjeto.Add(rolojeto);
        //    }
        //    db.SaveChanges();
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
