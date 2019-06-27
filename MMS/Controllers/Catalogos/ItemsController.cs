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
namespace MMS.Controllers.Catalogos
{
    public class ItemsController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: TipoDevoluciones
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {

            var item = await db.Item.FindAsync(id);

            if (item == null)
            {
                return HttpNotFound();
            }
            ViewData["MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            return PartialView(GetCrudMode().ToString(), item);
        }

        [AuthorizeAction]
        [FillPermission("Items/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            ViewData["MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Item model)
        {
            if (ModelState.IsValid)
            {
                var itemTemp = db.Item.Where(i => i.Codigo == model.Codigo).FirstOrDefault();
                if (itemTemp == null)
                {
                    db.Item.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Code already exists.");
                }
            }
            ViewData["MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int Id)
        {
            return await GetView(Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(Item model)
        {
            if (ModelState.IsValid)
            {
                var itemTemp = db.Item.Where(i => i.Codigo == model.Codigo).FirstOrDefault();
                if (itemTemp == null)
                {
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Code already exists.");
                }
            }
            ViewData["MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(int Id)
        {
            return await GetView(Id);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int Id)
        {
            var item = await db.Item.FindAsync(Id);
            try
            {
                db.Item.Remove(item);
                await db.SaveChangesAsync();
                AddLog("", item.Id, item);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(Id);
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