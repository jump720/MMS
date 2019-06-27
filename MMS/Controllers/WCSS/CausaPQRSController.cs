using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;


namespace MMS.Controllers.WCSS
{
    public class CausaPQRSController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: CausaPQRS
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {

            var motivo = await db.CausaPQRS.FindAsync(id);

            if (motivo == null)
            {
                return HttpNotFound();
            }


            return View(GetCrudMode().ToString(), motivo);
        }

        [AuthorizeAction]
        [FillPermission("CausaPQRS/Edit")]
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
        public async Task<ActionResult> Create(CausaPQRS model)
        {

            if (ModelState.IsValid)
            {
                if (model.TipoPQRS != 0)
                {

                    db.CausaPQRS.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Please select type PQRS");
                }

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
        public async Task<ActionResult> Edit(CausaPQRS model)
        {

            if (ModelState.IsValid)
            {
                if (model.TipoPQRS != 0)
                {
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    AddLog("", model.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Please select type PQRS");
                }
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
            var garantia = await db.GarantiaItems.Where(di => di.CausaPQRSId == id).ToListAsync();
            if (garantia.Count > 0)
            {
                ModelState.AddModelError("", "La Causa de PQRS no puede ser eliminado, ya se encuentra relacionada con una Garantia");
            }


            var devolucion = await db.DevolucionItems.Where(di => di.CausaPQRSId == id).ToListAsync();
            if (devolucion.Count > 0)
            {
                ModelState.AddModelError("", "La Causa de PQRS no puede ser eliminado, ya se encuentra relacionada con una Devolución");
            }

            var novedad = await db.NovedadItem.Where(di => di.CausaPQRSId == id).ToListAsync();
            if (novedad.Count > 0)
            {
                ModelState.AddModelError("", "La Causa de PQRS no puede ser eliminado, ya se encuentra relacionada con una Novedad");
            }

            if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
            {
                var causa = await db.CausaPQRS.FindAsync(id);
                if (causa != null)
                {

                    db.CausaPQRS.Remove(causa);
                    await db.SaveChangesAsync();
                    AddLog("", id, causa);
                    return RedirectToAction("Index", GetReturnSearch());

                }
                else
                {
                    ModelState.AddModelError("", "Causa PQRS no encontrada");
                }
            }

            return await GetView(id);

        }
    }
}