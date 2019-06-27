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

namespace MMS.Controllers.PerfectStore
{
    public class PlantillasController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Plantillas
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var plantilla = await db.Plantilla.FindAsync(id);

            if (plantilla == null)
                return HttpNotFound();

            var plantillaItems = await db.PlantillaItem
                .Include(pi => pi.Item)
                .Include(pi => pi.Item.Marca)
                .Where(pi => pi.PlantillaId == id)
                .ToListAsync();

            return View(GetCrudMode().ToString(), new PlantillaViewModel
            {
                Plantilla = plantilla,
                PlantillaItems = plantillaItems
            });
        }

        // GET: Plantillas/Details/5
        [AuthorizeAction]
        [FillPermission("Plantillas/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        // GET: Plantillas/Create
        [AuthorizeAction]
        public ActionResult Create()
        {
            return View(new PlantillaViewModel { Plantilla = new Plantilla { Activa = true } });
        }

        // POST: Plantillas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PlantillaViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Plantilla.PlantillaItems = model.PlantillaItems;
                db.Plantilla.Add(model.Plantilla);
                await db.SaveChangesAsync();
                AddLog("", model.Plantilla.Id, model);

                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(model);
        }

        // GET: Plantillas/Edit/5
        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        // POST: Plantillas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(PlantillaViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.PlantillaItems == null)
                    model.PlantillaItems = new List<PlantillaItem>();

                foreach (var pi in model.PlantillaItems)
                    pi.PlantillaId = model.Plantilla.Id;

                var currentItems = await db.PlantillaItem
                    .Where(pi => pi.PlantillaId == model.Plantilla.Id)
                    .ToListAsync();

                var currentItemsId = currentItems.Select(pi => pi.ItemId).ToArray();
                var itemsId = model.PlantillaItems.Select(pi => pi.ItemId).ToArray();

                var itemsToAdd = model.PlantillaItems.Where(pi => !currentItemsId.Contains(pi.ItemId)).ToList();
                var itemsToDelete = currentItems.Where(pi => !itemsId.Contains(pi.ItemId)).ToList();

                if (itemsToDelete.Count > 0)
                    db.PlantillaItem.RemoveRange(itemsToDelete);

                if (itemsToAdd.Count > 0)
                    db.PlantillaItem.AddRange(itemsToAdd);

                db.Entry(model.Plantilla).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.Plantilla.Id, model);

                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(model);
        }

        // GET: Plantillas/Delete/5
        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }

        // POST: Plantillas/Delete/5
        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var plantilla = await db.Plantilla.FindAsync(id);
            try
            {
                db.Plantilla.Remove(plantilla);
                await db.SaveChangesAsync();
                AddLog("", plantilla.Id, plantilla);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            var plantillaItems = await db.PlantillaItem
                .Include(pi => pi.Item)
                .Include(pi => pi.Item.Marca)
                .Where(pi => pi.PlantillaId == id)
                .ToListAsync();

            return View(new PlantillaViewModel { Plantilla = plantilla, PlantillaItems = plantillaItems });
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
