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
    public class ColeccionesPIVController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        [FillPermission("ColeccionesPIV/Items", "ColeccionesPIV/Asesores")]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var coleccion = await db.ColeccionPIV.FindAsync(id);
            if (coleccion == null)
                return HttpNotFound();

            var clientes = await db.Clientes
                .Include(c => c.ciudad)
                .Include(c => c.ciudad.departamentos)
                .Include(c => c.ciudad.departamentos.paises)
                .Include(c => c.canal)
                .Where(c => c.ColeccionPIVId == id)
                .ToListAsync();

            return View(GetCrudMode().ToString(), new ColeccionPIVViewModel
            {
                ColeccionPIV = coleccion,
                Clientes = clientes
            });
        }

        [AuthorizeAction]
        [FillPermission("ColeccionesPIV/Edit")]
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
        public async Task<ActionResult> Create(ColeccionPIVViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ClientesId != null)
                {
                    var clientes = await db.Clientes.Where(c => model.ClientesId.Contains(c.ClienteID)).ToListAsync();
                    foreach (var cliente in clientes)
                    {
                        cliente.ColeccionPIVId = model.ColeccionPIV.Id;
                        db.Entry(cliente).State = EntityState.Modified;
                    }
                }

                db.ColeccionPIV.Add(model.ColeccionPIV);
                await db.SaveChangesAsync();
                AddLog("", model.ColeccionPIV.Id, model);

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
        public async Task<ActionResult> Edit(ColeccionPIVViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.ClientesId == null)
                    model.ClientesId = new List<string>();

                var currentClientes = await db.Clientes
                    .Where(cc => cc.ColeccionPIVId == model.ColeccionPIV.Id)
                    .ToListAsync();

                var currentClientesId = currentClientes.Select(cc => cc.ClienteID).ToArray();
                var clientesIdToAdd = model.ClientesId.Where(id => !currentClientesId.Contains(id)).ToArray();

                var clientesToDelete = currentClientes.Where(c => !model.ClientesId.Contains(c.ClienteID)).ToList();
                var clientesToAdd = await db.Clientes.Where(c => clientesIdToAdd.Contains(c.ClienteID)).ToListAsync();

                foreach (var cliente in clientesToDelete)
                {
                    cliente.ColeccionPIVId = null;
                    db.Entry(cliente).State = EntityState.Modified;
                }

                foreach (var cliente in clientesToAdd)
                {
                    cliente.ColeccionPIVId = model.ColeccionPIV.Id;
                    db.Entry(cliente).State = EntityState.Modified;
                }

                db.Entry(model.ColeccionPIV).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.ColeccionPIV.Id, model);

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
            var coleccion = await db.ColeccionPIV.FindAsync(id);
            try
            {
                var clientesToUnselect = await db.Clientes.Where(c => c.ColeccionPIVId == id).ToListAsync();
                foreach (var cliente in clientesToUnselect)
                {
                    cliente.ColeccionPIVId = null;
                    db.Entry(cliente).State = EntityState.Modified;
                }

                db.ColeccionPIV.Remove(coleccion);
                await db.SaveChangesAsync();
                AddLog("", coleccion.Id, coleccion);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            var clientes = await db.Clientes
                .Include(c => c.ciudad)
                .Include(c => c.ciudad.departamentos)
                .Include(c => c.ciudad.departamentos.paises)
                .Include(c => c.canal)
                .Where(c => c.ColeccionPIVId == id)
                .ToListAsync();

            return View(new ColeccionPIVViewModel { ColeccionPIV = coleccion, Clientes = clientes });
        }

        [AuthorizeAction]
        [FillPermission("ColeccionesPIV/CreateItem", "ColeccionesPIV/EditItem", "ColeccionesPIV/DetailsItem", "ColeccionesPIV/DeleteItem")]
        public async Task<ActionResult> Items(int id)
        {
            var coleccion = await db.ColeccionPIV.FindAsync(id);
            if (coleccion == null)
                return HttpNotFound();

            return View(coleccion);
        }

        private async Task<ActionResult> GetViewItem(int id)
        {
            var collectionItem = await db.ColeccionPIVItem.FindAsync(id);
            if (collectionItem == null)
                return HttpNotFound();

            var coleccion = await db.ColeccionPIV.FindAsync(collectionItem.ColeccionPIVId);
            if (coleccion == null)
                return HttpNotFound();

            ViewBag.ColeccionPIVNombre = coleccion.Nombre;
            return PartialView($"_{GetCrudMode().ToString()}Item", collectionItem);
        }

        [AuthorizeAction]
        public async Task<ActionResult> CreateItem(int coleccionPIVId)
        {
            var coleccion = await db.ColeccionPIV.FindAsync(coleccionPIVId);
            if (coleccion == null)
                return HttpNotFound();

            ViewBag.ColeccionPIVNombre = coleccion.Nombre;
            return PartialView("_CreateItem", new ColeccionPIVItem { ColeccionPIVId = coleccion.Id });
        }

        [AuthorizeAction]
        public async Task<ActionResult> EditItem(int id)
        {
            return await GetViewItem(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> DetailsItem(int id)
        {
            return await GetViewItem(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> DeleteItem(int id)
        {
            return await GetViewItem(id);
        }

        [AuthorizeAction]
        [FillPermission("ColeccionesPIV/CreateAsesor", "ColeccionesPIV/EditAsesor", "ColeccionesPIV/DetailsAsesor", "ColeccionesPIV/DeleteAsesor")]
        public async Task<ActionResult> Asesores(int id)
        {
            var coleccion = await db.ColeccionPIV.FindAsync(id);
            if (coleccion == null)
                return HttpNotFound();

            return View(coleccion);
        }

        private async Task<ActionResult> GetViewAsesor(int id)
        {
            var asesor = await db.Asesor.FindAsync(id);
            if (asesor == null)
                return HttpNotFound();

            var coleccion = await db.ColeccionPIV.FindAsync(asesor.ColeccionPIVId);
            if (coleccion == null)
                return HttpNotFound();

            ViewBag.ColeccionPIV = coleccion;
            ViewBag.PaisId = new SelectList(await db.Pais.OrderBy(p => p.PaisDesc).ToListAsync(), "PaisID", "PaisDesc", asesor.PaisId);
            ViewBag.DepartamentoId = new SelectList(await db.Departamento.Where(d => d.PaisID == asesor.PaisId).OrderBy(d => d.DepartamentoDesc).ToListAsync(), "DepartamentoID", "DepartamentoDesc", asesor.DepartamentoId);
            ViewBag.CiudadId = new SelectList(await db.Ciudad.Where(c => c.PaisID == asesor.PaisId && c.DepartamentoID == asesor.DepartamentoId).OrderBy(c => c.CiudadDesc).ToListAsync(), "CiudadID", "CiudadDesc", asesor.CiudadId);

            return PartialView($"_{GetCrudMode().ToString()}Asesor", asesor);
        }

        [AuthorizeAction]
        public async Task<ActionResult> CreateAsesor(int coleccionPIVId)
        {
            var coleccion = await db.ColeccionPIV.FindAsync(coleccionPIVId);
            if (coleccion == null)
                return HttpNotFound();

            ViewBag.ColeccionPIV = coleccion;
            ViewBag.PaisId = new SelectList(await db.Pais.OrderBy(p => p.PaisDesc).ToListAsync(), "PaisID", "PaisDesc");
            ViewBag.DepartamentoId = new SelectList(new List<string>(), "");
            ViewBag.CiudadId = new SelectList(new List<string>(), "");

            return PartialView("_CreateAsesor", new Asesor { ColeccionPIVId = coleccion.Id });
        }

        [AuthorizeAction]
        public async Task<ActionResult> DetailsAsesor(int id)
        {
            return await GetViewAsesor(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> EditAsesor(int id)
        {
            return await GetViewAsesor(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> DeleteAsesor(int id)
        {
            return await GetViewAsesor(id);
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
