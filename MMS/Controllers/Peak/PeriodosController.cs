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
    public class PeriodosController : BaseController
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
            var periodo = await db.Periodo
                .Include(p => p.PeriodoRevisiones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (periodo == null)
                return HttpNotFound();

            return View(GetCrudMode().ToString(), new PeriodoViewModel
            {
                Periodo = periodo,
                Revisiones = periodo.PeriodoRevisiones.OrderBy(p => p.FechaIni).ToList()
            });
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
        public async Task<ActionResult> Create(PeriodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await db.Periodo.AnyAsync(p => model.Periodo.FechaIni <= p.FechaFin && model.Periodo.FechaFin >= p.FechaIni))
                    ModelState.AddModelError("", "Date Range is overlaping with other Period.");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    if (model.Revisiones != null)
                        model.Periodo.PeriodoRevisiones = model.Revisiones;

                    db.Periodo.Add(model.Periodo);
                    await db.SaveChangesAsync();
                    AddLog("", model.Periodo.Id, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }
            if (model.Revisiones == null)
                model.Revisiones = new List<PeriodoRevision>();

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
        public async Task<ActionResult> Edit(PeriodoViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await db.Periodo.AnyAsync(p => p.Id != model.Periodo.Id && model.Periodo.FechaIni <= p.FechaFin && model.Periodo.FechaFin >= p.FechaIni))
                    ModelState.AddModelError("", "Date Range is overlaping with other Period.");

                if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                {
                    db.Entry(model.Periodo).State = EntityState.Modified;

                    if (model.Revisiones != null)
                        foreach (var revision in model.Revisiones)
                        {
                            revision.PeriodoId = model.Periodo.Id;

                            if (revision.Id == 0)
                                db.PeriodoRevision.Add(revision);
                            else
                                db.Entry(revision).State = EntityState.Modified;
                        }

                    if (model.RevisionesId != null)
                    {
                        var revisionesRemove = await db.PeriodoRevision
                            .Where(pr => model.RevisionesId.Contains(pr.Id))
                            .ToListAsync();

                        db.PeriodoRevision.RemoveRange(revisionesRemove);
                    }

                    await db.SaveChangesAsync();
                    AddLog("", model.Periodo.Id, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
            }
            if (model.Revisiones == null)
                model.Revisiones = new List<PeriodoRevision>();

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
            var periodo = await db.Periodo.FindAsync(id);
            try
            {
                db.Periodo.Remove(periodo);
                await db.SaveChangesAsync();
                AddLog("", periodo.Id, periodo);

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
