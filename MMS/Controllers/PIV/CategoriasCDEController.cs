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

namespace MMS.Controllers.PIV
{
    public class CategoriasCDEController : BaseController
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
            var categoria = await db.CategoriaCDE.FindAsync(id);

            if (categoria == null)
                return HttpNotFound();

            return View(GetCrudMode().ToString(), categoria);
        }

        [AuthorizeAction]
        [FillPermission("CategoriasCDE/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoriaCDE categoria)
        {
            if (ModelState.IsValid)
            {
                if (await db.CategoriaCDE.AnyAsync(c => c.LiquidacionId == null && categoria.ValorMinimo <= c.ValorMaximo && categoria.ValorMaximo >= c.ValorMinimo))
                    ModelState.AddModelError("", "Given values are overlaping with other COE Category values");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    db.CategoriaCDE.Add(categoria);
                    await db.SaveChangesAsync();
                    AddLog("", categoria.Id, categoria);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }
            return View(categoria);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(CategoriaCDE categoria)
        {
            if (ModelState.IsValid)
            {
                if (await db.CategoriaCDE.AnyAsync(c => c.LiquidacionId == null && c.Id != categoria.Id && categoria.ValorMinimo <= c.ValorMaximo && categoria.ValorMaximo >= c.ValorMinimo))
                    ModelState.AddModelError("", "Given values are overlaping with other COE Category values");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    db.Entry(categoria).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", categoria.Id, categoria);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }
            return View(categoria);
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
            var categoria = await db.CategoriaCDE.FindAsync(id);
            try
            {
                db.CategoriaCDE.Remove(categoria);
                await db.SaveChangesAsync();
                AddLog("", categoria.Id, categoria);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(categoria);
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
