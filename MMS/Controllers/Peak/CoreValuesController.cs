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

namespace MMS.Controllers.Peak
{
    public class CoreValuesController : BaseController
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
            var coreValue = await db.CoreValue.FindAsync(id);
            if (coreValue == null)
                return HttpNotFound();

            return View(GetCrudMode().ToString(), coreValue);
        }

        [AuthorizeAction]
        [FillPermission("Reglas/Edit")]
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
        public async Task<ActionResult> Create(CoreValue coreValue)
        {
            if (ModelState.IsValid)
            {
                db.CoreValue.Add(coreValue);
                await db.SaveChangesAsync();
                AddLog("", coreValue.Id, coreValue);

                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(coreValue);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(CoreValue coreValue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(coreValue).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", coreValue.Id, coreValue);

                return RedirectToAction("Index", GetReturnSearch());
            }
            return View(coreValue);
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
            var coreValue = await db.CoreValue.FindAsync(id);
            try
            {
                db.CoreValue.Remove(coreValue);
                await db.SaveChangesAsync();
                AddLog("", coreValue.Id, coreValue);

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
