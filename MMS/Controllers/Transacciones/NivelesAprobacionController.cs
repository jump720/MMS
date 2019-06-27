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

namespace MMS.Controllers.Transacciones
{
    public class NivelesAprobacionController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var na = await db.NivelesAprobacion.FindAsync(id);

            if (na == null)
                return HttpNotFound();

            ViewBag.CanalID = new SelectList(db.Canales.OrderBy(c => c.CanalDesc).ToList(), "CanalID", "CanalDesc", na.CanalID);
            ViewBag.PlantaID = new SelectList(db.Plantas.OrderBy(p => p.PlantaDesc).ToList(), "PlantaID", "PlantaDesc", na.PlantaID);
            ViewBag.UsuarioId = new SelectList(new List<Usuario>());

            return View(GetCrudMode().ToString(), na);
        }

        [AuthorizeAction]
        [FillPermission("NivelesAprobacion/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            ViewBag.CanalID = new SelectList(db.Canales.OrderBy(c => c.CanalDesc).ToList(), "CanalID", "CanalDesc");
            ViewBag.PlantaID = new SelectList(db.Plantas.OrderBy(p => p.PlantaDesc).ToList(), "PlantaID", "PlantaDesc");
            ViewBag.UsuarioId = new SelectList(new List<Usuario>());
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NivelesAprobacion nivelAprobacion)
        {
            if (ModelState.IsValid)
            {
                if (await db.NivelesAprobacion.AnyAsync(na => na.CanalID == nivelAprobacion.CanalID && na.PlantaID == nivelAprobacion.PlantaID && na.Orden == nivelAprobacion.Orden))
                    ModelState.AddModelError("", "Ya existe un un nivel de aprobación con la misma (Planta, Canal, Orden)");

                if (await db.NivelesAprobacion.AnyAsync(na => na.CanalID == nivelAprobacion.CanalID && na.PlantaID == nivelAprobacion.PlantaID && na.UsuarioId == nivelAprobacion.UsuarioId))
                    ModelState.AddModelError("", "Ya existe un un nivel de aprobación con la misma (Planta, Canal, Usuario)");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    db.NivelesAprobacion.Add(nivelAprobacion);
                    await db.SaveChangesAsync();
                    AddLog("", nivelAprobacion.Id, nivelAprobacion);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }

            ViewBag.CanalID = new SelectList(db.Canales.OrderBy(c => c.CanalDesc).ToList(), "CanalID", "CanalDesc", nivelAprobacion.CanalID);
            ViewBag.PlantaID = new SelectList(db.Plantas.OrderBy(p => p.PlantaDesc).ToList(), "PlantaID", "PlantaDesc", nivelAprobacion.PlantaID);
            ViewBag.UsuarioId = new SelectList(new List<Usuario>());
            return View(nivelAprobacion);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(NivelesAprobacion nivelAprobacion)
        {
            if (ModelState.IsValid)
            {
                if (await db.NivelesAprobacion.AnyAsync(na => na.CanalID == nivelAprobacion.CanalID && na.PlantaID == nivelAprobacion.PlantaID && na.Orden == nivelAprobacion.Orden && na.Id != nivelAprobacion.Id))
                    ModelState.AddModelError("", "Ya existe un un nivel de aprobación con la misma (Planta, Canal, Orden)");

                if (await db.NivelesAprobacion.AnyAsync(na => na.CanalID == nivelAprobacion.CanalID && na.PlantaID == nivelAprobacion.PlantaID && na.UsuarioId == nivelAprobacion.UsuarioId && na.Id != nivelAprobacion.Id))
                    ModelState.AddModelError("", "Ya existe un un nivel de aprobación con la misma (Planta, Canal, Usuario)");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    db.Entry(nivelAprobacion).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", nivelAprobacion.Id, nivelAprobacion);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }

            ViewBag.CanalID = new SelectList(db.Canales.OrderBy(c => c.CanalDesc).ToList(), "CanalID", "CanalDesc", nivelAprobacion.CanalID);
            ViewBag.PlantaID = new SelectList(db.Plantas.OrderBy(p => p.PlantaDesc).ToList(), "PlantaID", "PlantaDesc", nivelAprobacion.PlantaID);
            ViewBag.UsuarioId = new SelectList(new List<Usuario>());
            return View(nivelAprobacion);
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
            var na = await db.NivelesAprobacion.FindAsync(id);
            try
            {
                db.NivelesAprobacion.Remove(na);
                await db.SaveChangesAsync();
                AddLog("", na.Id, na);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(na);
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
