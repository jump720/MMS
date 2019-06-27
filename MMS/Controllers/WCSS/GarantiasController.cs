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
using System.IO;
using MMS.Classes;

namespace MMS.Controllers.WCSS
{
    public class GarantiasController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: Garantias
        [AuthorizeAction]
        [FillPermission("PQRS/CreaPQRSRecord")]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;

            var garantia = await db.Garantias
                                     .FindAsync(id);

            if (garantia == null)
            {
                return HttpNotFound();
            }


            var items = await db.GarantiaItems
                                .Include(gi => gi.Items)
                                .Include(gi => gi.MotivoPQRS)
                                .Where(di => di.GarantiaId == id).ToListAsync();

            var archivos = await db.GarantiaArchivos
                                   .Where(ga => ga.GarantiaId == id)
                                   .Select(ga => new GarantiaViewModel.Archivos { Order = ga.Order, FileName = ga.FileName })
                                   .ToListAsync();

            if (GetCrudMode() == Fn.CrudMode.Edit || GetCrudMode() == Fn.CrudMode.Delete)
            {
                if (garantia.Estado == EstadoFormatoPQRS.Deleted || garantia.Estado == EstadoFormatoPQRS.Completed)
                {
                    return RedirectToAction("Index", GetReturnSearch());
                }

                foreach (var item in items)
                {

                    if (garantia.Estado == EstadoFormatoPQRS.In_Process)
                    {
                        var pqrs = await db.PQRSRecords
                                       .Where(p => p.Id == item.PQRSRecordId && p.PasoActual == true && p.FlujoPQRSTipoPaso == TipoPaso.LlenarFormato && p.TipoPQRS == TipoPQRS.Garantia)
                                       .FirstOrDefaultAsync();

                        if (pqrs == null)
                            ViewData["has_pqrs_" + item.GarantiaId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                        else
                        {
                            var usuarioPQRS = await db.PQRSRecordUsuarios
                                                      .Where(pu => pu.PQRSRecordId == pqrs.Id && pu.PQRSRecordOrder == pqrs.Order && pu.UsuarioId == seguridadcll.Usuario.UsuarioId)
                                                      .FirstOrDefaultAsync();
                            if (usuarioPQRS == null)
                                ViewData["has_pqrs_" + item.GarantiaId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                            else
                                ViewData["has_pqrs_" + item.GarantiaId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";
                        }
                    }
                    else
                        ViewData["has_pqrs_" + item.GarantiaId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";

                }
            }

            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Garantia && m.Activo == true).ToListAsync();
            ViewBag.CausaPQRS = await db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Garantia).ToListAsync();

            return View(GetCrudMode().ToString(), new GarantiaViewModel
            {
                Garantia = garantia,
                Items = items,
                GarantiaArchivos = archivos
            });
        }



        [AuthorizeAction]
        [FillPermission("Garantias/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
            ViewBag.MotivoPQRS = db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Garantia && m.Activo == true).ToList();
            ViewBag.CausaPQRS = db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Garantia).ToList();
            return View();
        }



        [HttpPost]
        [AuthorizeAction]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(GarantiaViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            //var f = await UploadFiles(0, Files);

            if (ModelState.IsValid)
            {
                //Save Return
                model.Garantia.Estado = EstadoFormatoPQRS.Open;
                model.Garantia.FechaCreacion = DateTime.Now;
                db.Garantias.Add(model.Garantia);
                await db.SaveChangesAsync();


                //Genera HASHNroTracking
                string HASHNroTracking = Fn.HASH("G" + model.Garantia.Id);
                HASHNroTracking = "G" + model.Garantia.Id + HASHNroTracking;
                model.Garantia.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();

                db.Entry(model.Garantia).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Save Items
                if (model.Items != null)
                {
                    int i = 1;
                    foreach (var item in model.Items)
                    {
                        item.GarantiaId = model.Garantia.Id;
                        item.Id = i++;

                        db.GarantiaItems.Add(item);

                    }

                    await db.SaveChangesAsync();

                }

                //Save Files
                if (Files != null)
                    await UploadFiles(model.Garantia.Id, Files, 1);

                AddLog("", model.Garantia.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Garantia && m.Activo == true).ToListAsync();

            return View(model);

        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }


        [HttpPost]
        [ValidateInput(false)]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(GarantiaViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            if (ModelState.IsValid)
            {
                //Guardar Cabecera
                db.Entry(model.Garantia).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //si es null ponerle el valor en 1


                //int idx = await db.Database.SqlQuery<int>($"SELECT ISNULL(MAX(Id),0) FROM DevolucionItem WHERE DevolucionId = {model.Devolucion.Id} ").FirstOrDefaultAsync();

                //Elimina los items Delete
                if (await DeleteItems(model.Garantia.Id, model.ItemsDelete, true))
                {
                    int idx = 0;
                    var currentItems = await db.GarantiaItems.Where(i => i.GarantiaId == model.Garantia.Id).ToListAsync();
                    if (currentItems.Count > 0)
                        idx = currentItems.Select(i => i.Id).Max();
                    //Actualiza  los items actuales
                    foreach (var item in currentItems)
                    {
                        var modelItem = model.Items.Where(i => i.Id == item.Id).FirstOrDefault();
                        if (modelItem != null)
                        {
                            item.ItemId = modelItem.ItemId;
                            item.Cantidad = modelItem.Cantidad;
                            item.Precio = modelItem.Precio;
                            item.NroFactura = modelItem.NroFactura;
                            item.NroGuia = modelItem.NroGuia;
                            item.MotivoPQRSId = modelItem.MotivoPQRSId;
                            item.CausaPQRSId = ((modelItem.CausaPQRSId ?? 0) != 0) ? modelItem.CausaPQRSId : null;
                            item.Estado = modelItem.Estado;
                            item.ComentarioAdicional = modelItem.ComentarioAdicional;
                            item.CantidadRecibida = modelItem.CantidadRecibida;
                            item.CantidadSubida = modelItem.CantidadSubida;
                            item.ComentarioEstadoMercancia = modelItem.ComentarioEstadoMercancia;
                            item.DocSoporte = modelItem.DocSoporte;
                            item.PrecioAsumido = modelItem.PrecioAsumido;
                            db.Entry(item).State = EntityState.Modified;
                        }

                    }
                  
                    //Agregar los nuevos items
                    if (model.Items != null)
                        foreach (var item in model.Items.Where(i => i.Id == 0))
                        {
                            if (item.Id == 0)
                            {
                                item.GarantiaId = model.Garantia.Id;
                                item.Id = ++idx;
                                db.GarantiaItems.Add(item);

                            }
                        }


                    await db.SaveChangesAsync();


                    //Archivos
                    var currentFiles = await db.GarantiaArchivos
                                               .Where(da => da.GarantiaId == model.Garantia.Id)
                                               .ToListAsync();


                    if (model.GarantiaArchivos != null)
                    {
                        var FilesId = model.GarantiaArchivos.Select(a => a.Order).ToArray();


                        var itemsToDelete = currentFiles.Where(a => !FilesId.Contains(a.Order)).ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.GarantiaArchivos.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }else
                    {
                        var itemsToDelete = currentFiles.ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.GarantiaArchivos.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }


                    //Save Files
                    int order = 1;
                    if (currentFiles.Count > 0)
                        order = currentFiles.Select(i => i.Order).Max() + 1;

                    if (Files != null)
                        await UploadFiles(model.Garantia.Id, Files, order);


                    AddLog("", model.Garantia.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());

                }
                else
                {
                    ModelState.AddModelError("", "Error Deleting Detail (GarantiaItems)");
                }

            }

            return await GetView(model.Garantia.Id);
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
            var garantia = await db.Garantias.FindAsync(id);

            try
            {
                if (garantia != null)
                {
                    //if (await DeleteItems(id, null, false) && await DeleteFiles(id))
                    //{
                    //    db.Garantias.Remove(garantia);
                    //    await db.SaveChangesAsync();
                    //    AddLog("", id, garantia);
                    //    return RedirectToAction("Index", GetReturnSearch());
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Error Deleting Detail (DevolucionItems)");
                    //}
                    garantia.Estado = EstadoFormatoPQRS.Deleted;

                    db.Entry(garantia).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", id, garantia);
                    return RedirectToAction("Index", GetReturnSearch());
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return await GetView(id);
        }

        private async Task<bool> UploadFiles(int Id, IEnumerable<HttpPostedFileBase> Files, int porder)
        {
            
            int order = porder;
            foreach (var file in Files)
            {
                if (file != null && file.ContentLength > 0)
                {

                    GarantiaArchivo ga = new GarantiaArchivo();
                    ga.GarantiaId = Id;
                    ga.Order = order++;
                    ga.File = Fn.ConvertToByte(file);
                    ga.FileName = file.FileName;
                    ga.MediaType = file.ContentType;

                    db.GarantiaArchivos.Add(ga);
                }


            }

            if (db.GarantiaArchivos.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarArchivo(int d, int o)
        {
            var data = await db.GarantiaArchivos
                               .Where(ga => ga.GarantiaId == d && ga.Order == o)
                               .Select(ga => new { ga.File, ga.MediaType, ga.FileName })
                               .FirstOrDefaultAsync();

            if (data == null)
                return HttpNotFound();
            else
            {


                return File(data.File, data.MediaType, data.FileName);
            }

        }

        

        private async Task<bool> DeleteItems(int id, List<int> Items = null, bool Partial = false)
        {
            bool result = true;
            try
            {
                var gi = await db.GarantiaItems.Where(i => i.GarantiaId == id).ToListAsync();
                if (Items == null && !Partial)
                    db.GarantiaItems.RemoveRange(gi);
                else if (Items != null)
                    foreach (int i in Items)
                    {
                        db.GarantiaItems.Remove(gi.Where(it => it.Id == i).FirstOrDefault());
                    }

                await db.SaveChangesAsync();
            }
            catch
            {
                result = false;
            }

            return result;
        }


        private async Task<bool> DeleteFiles(int id)
        {
            bool result = true;
            try
            {
                var gi = await db.GarantiaArchivos.Where(i => i.GarantiaId == id).ToListAsync();
                if (gi.Count > 0)
                {
                    db.GarantiaArchivos.RemoveRange(gi);
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