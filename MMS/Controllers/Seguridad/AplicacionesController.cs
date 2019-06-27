using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;

namespace MMS.Controllers.Seguridad
{
    public class AplicacionesController : BaseController
    {

        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            ViewBag.DominioWeb = Seguridadcll.Configuracion.ConfigDominioWeb;
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {
            var aplicacion = await db.Aplicaciones.FindAsync(id);
            if (aplicacion == null)
                return HttpNotFound();

            var objetos = await (from o in db.Objeto
                                 join ao in db.AplicacionObjetos.Where(a => a.AplicacionId == id) on o equals ao.Objeto into oaos
                                 where o.ObjetoMenu //&& !o.ObjetoId.StartsWith("__")
                                 from aoa in oaos.DefaultIfEmpty()
                                 select new AplicacionesViewModel.AplicacionObjetosViewModel
                                 {
                                     ObjetoId = o.ObjetoId,
                                     Seleccionado = (aoa.Aplicacion == null) ? false : true
                                 }).ToListAsync();

            return View(GetCrudMode().ToString(), new AplicacionesViewModel
            {
                Aplicacion = aplicacion,
                Objetos = objetos
            });
        }

        [AuthorizeAction]
        [FillPermission("Aplicaciones/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            var objetos = await db.Objeto
                .Where(o => o.ObjetoMenu) //&& !o.ObjetoId.StartsWith("__"))
                .Select(o => new AplicacionesViewModel.AplicacionObjetosViewModel
                {
                    ObjetoId = o.ObjetoId,
                    Seleccionado = false
                }).ToListAsync();

            return View(new AplicacionesViewModel { Objetos = objetos });
        }


        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AplicacionesViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Aplicacion.AplicacionObjetos = new List<AplicacionObjeto>();

                foreach (var obj in model.Objetos)
                    if (obj.Seleccionado)
                        model.Aplicacion.AplicacionObjetos.Add(new AplicacionObjeto()
                        {
                            ObjetoId = obj.ObjetoId
                        });

                db.Aplicaciones.Add(model.Aplicacion);
                await db.SaveChangesAsync();
                AddLog("", model.Aplicacion.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AplicacionesViewModel model)
        {

            if (ModelState.IsValid)
            {
                //Save Aplicación table
                db.Entry(model.Aplicacion).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //Detele Detail (AplicacionObjetos)
                if (await DeleteAplicacionObjetos(model.Aplicacion.Id))
                {
                    foreach (var obj in model.Objetos)
                    {
                        if (obj.Seleccionado)
                        {
                            var ao = new AplicacionObjeto();
                            ao.AplicacionId = model.Aplicacion.Id;
                            ao.ObjetoId = obj.ObjetoId;
                            db.AplicacionObjetos.Add(ao);
                        }
                    }
                    await db.SaveChangesAsync();
                    AddLog("", model.Aplicacion.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error Deleting Detail (AplicacionObjetos)");
                }

                //Save Detail (AplicacionObjetos)

            }
            return View(model);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var rolApp = await db.RolAplicaciones.Where(ra => ra.AplicacionId == id).ToListAsync();
                if (rolApp.Count <= 0)
                {
                    var aplicacion = await db.Aplicaciones.FindAsync(id);
                    db.Aplicaciones.Remove(aplicacion);
                    await db.SaveChangesAsync();
                    AddLog("", id, aplicacion);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "This App is related to a role");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return await GetView(id);
        }

        private async Task<bool> DeleteAplicacionObjetos(int id)
        {
            bool result = true;
            try
            {
                var ao = await db.AplicacionObjetos.Where(a => a.AplicacionId == id).ToListAsync();
                if (ao.Count > 0)
                {
                    db.AplicacionObjetos.RemoveRange(ao);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

    }
}