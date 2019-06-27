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

namespace MMS.Controllers.PIV
{
    public class ReglasController : BaseController
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
            var regla = await db.Regla.FindAsync(id);

            if (regla == null)
                return HttpNotFound();

            var model = new ReglaViewModel
            {
                Regla = regla,
                Tipo = regla.ItemId != null ? ReglaViewModel.TipoRegla.Item : ReglaViewModel.TipoRegla.Brand
            };

            ViewBag.Tipo = new SelectList(Fn.EnumToIEnumarable<ReglaViewModel.TipoRegla>().ToList(), "Value", "Name", model.Tipo);
            ViewData["Regla.ItemId"] = new SelectList(new List<string>(), "", "");

            if (model.Tipo == ReglaViewModel.TipoRegla.Brand)
                ViewData["Regla.MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre", regla.MarcaId);
            else
                ViewData["Regla.MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");

            return View(GetCrudMode().ToString(), model);
        }

        [AuthorizeAction]
        [FillPermission("Reglas/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            ViewBag.Tipo = new SelectList(Fn.EnumToIEnumarable<ReglaViewModel.TipoRegla>().ToList(), "Value", "Name");
            ViewData["Regla.ItemId"] = new SelectList(new List<string>(), "", "");
            ViewData["Regla.MarcaId"] = new SelectList(await db.Marca.Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ReglaViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Tipo == ReglaViewModel.TipoRegla.Item)
                    model.Regla.MarcaId = null;
                else
                    model.Regla.ItemId = null;

                db.Regla.Add(model.Regla);
                await db.SaveChangesAsync();
                AddLog("", model.Regla.Id, model.Regla);

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
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(ReglaViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Tipo == ReglaViewModel.TipoRegla.Item)
                    model.Regla.MarcaId = null;
                else
                    model.Regla.ItemId = null;

                db.Entry(model.Regla).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.Regla.Id, model.Regla);

                return RedirectToAction("Index", GetReturnSearch());
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
            var regla = await db.Regla.FindAsync(id);
            try
            {
                db.Regla.Remove(regla);
                await db.SaveChangesAsync();
                AddLog("", regla.Id, regla);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(id);
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
