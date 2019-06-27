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
    public class DevolucionesController : BaseController
    {

        private MMSContext db = new MMSContext();
        // GET: Devoluciones

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

            var devolucion = await db.Devoluciones
                                     .FindAsync(id);

            if (devolucion == null)
            {
                return HttpNotFound();
            }


            

            var items = await db.DevolucionItems
                    .Include(di => di.Items)
                    .Include(di => di.MotivoPQRS)
                    .Where(di => di.DevolucionId == id).ToListAsync();



            var archivos = await db.DevolucionArchivos
                                   .Where(di => di.DevolucionId == id)
                                   .Select(da => new DevolucionViewModel.Archivos { Order = da.Order, FileName = da.FileName })
                                   .ToListAsync();

            if (GetCrudMode() == Fn.CrudMode.Edit || GetCrudMode() == Fn.CrudMode.Delete)
            {
                if (devolucion.Estado == EstadoFormatoPQRS.Deleted || devolucion.Estado == EstadoFormatoPQRS.Completed)
                {
                    return RedirectToAction("Index", GetReturnSearch());
                }

                foreach (var item in items)
                {

                    if (devolucion.Estado == EstadoFormatoPQRS.In_Process)
                    {
                        var pqrs = await db.PQRSRecords
                                       .Where(p => p.Id == item.PQRSRecordId && p.PasoActual == true && p.FlujoPQRSTipoPaso == TipoPaso.LlenarFormato && p.TipoPQRS == TipoPQRS.Devolucion)
                                       .FirstOrDefaultAsync();

                        if (pqrs == null)
                            ViewData["has_pqrs_" + item.DevolucionId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                        else
                        {
                            var usuarioPQRS = await db.PQRSRecordUsuarios
                                                      .Where(pu => pu.PQRSRecordId == pqrs.Id && pu.PQRSRecordOrder == pqrs.Order && pu.UsuarioId == seguridadcll.Usuario.UsuarioId)
                                                      .FirstOrDefaultAsync();
                            if (usuarioPQRS == null)
                                ViewData["has_pqrs_" + item.DevolucionId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                            else
                                ViewData["has_pqrs_" + item.DevolucionId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";
                        }
                    }
                    else
                        ViewData["has_pqrs_" + item.DevolucionId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";

                }


            }

            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Devolucion && m.Activo == true).ToListAsync();
            ViewBag.CausaPQRS = await db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Devolucion).ToListAsync();

            return View(GetCrudMode().ToString(), new DevolucionViewModel
            {
                Devolucion = devolucion,
                Items = items,
                DevolucionArchivos = archivos
            });
        }


        [AuthorizeAction]
        [FillPermission("Devoluciones/Edit")]
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
            ViewBag.MotivoPQRS = db.MotivosPQRS
                                    .Where(m => m.TipoPQRS == TipoPQRS.Devolucion && m.Activo == true)
                                    .ToList();
            ViewBag.CausaPQRS = db.CausaPQRS
                                .Where(m => m.TipoPQRS == TipoPQRS.Devolucion)
                                .ToList();
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DevolucionViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            //var f = await UploadFiles(0, Files);

            if (ModelState.IsValid)
            {
                //Save Return
                model.Devolucion.Estado = EstadoFormatoPQRS.Open;
                model.Devolucion.FechaCreacion = DateTime.Now;
                db.Devoluciones.Add(model.Devolucion);
                await db.SaveChangesAsync();

                //Genera HASHNroTracking
                string HASHNroTracking = Fn.HASH("D" + model.Devolucion.Id);
                HASHNroTracking = "D" + model.Devolucion.Id + HASHNroTracking;
                model.Devolucion.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();

                db.Entry(model.Devolucion).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Save Items
                if (model.Items != null)
                {
                    int i = 1;
                    foreach (var item in model.Items)
                    {
                        item.DevolucionId = model.Devolucion.Id;
                        item.Id = i++;

                        db.DevolucionItems.Add(item);

                    }

                    await db.SaveChangesAsync();

                }

                //Save Files
                if (Files != null)
                    await UploadFiles(model.Devolucion.Id, Files, 1);

                //Generar HASH y actualiza Formato


                AddLog("", model.Devolucion.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Devolucion && m.Activo == true).ToListAsync();
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
        public async Task<ActionResult> Edit(DevolucionViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            if (ModelState.IsValid)
            {
                //Guardar Cabecera
                db.Entry(model.Devolucion).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //si es null ponerle el valor en 1


                //int idx = await db.Database.SqlQuery<int>($"SELECT ISNULL(MAX(Id),0) FROM DevolucionItem WHERE DevolucionId = {model.Devolucion.Id} ").FirstOrDefaultAsync();

                //Elimina los items Delete
                if (await DeleteItems(model.Devolucion.Id, model.ItemsDelete, true))
                {
                    int idx = 0;
                    var currentItems = await db.DevolucionItems.Where(i => i.DevolucionId == model.Devolucion.Id).ToListAsync();
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
                                item.DevolucionId = model.Devolucion.Id;
                                item.Id = ++idx;
                                db.DevolucionItems.Add(item);

                            }
                        }

                    await db.SaveChangesAsync();



                    //Archivos
                    var currentFiles = await db.DevolucionArchivos
                                               .Where(da => da.DevolucionId == model.Devolucion.Id)
                                               .ToListAsync();


                    if (model.DevolucionArchivos != null)
                    {
                        var FilesId = model.DevolucionArchivos.Select(a => a.Order).ToArray();


                        var itemsToDelete = currentFiles.Where(a => !FilesId.Contains(a.Order)).ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.DevolucionArchivos.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var itemsToDelete = currentFiles.ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.DevolucionArchivos.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }


                    //Save Files
                    int order = 1;
                    if (currentFiles.Count > 0)
                        order = currentFiles.Select(i => i.Order).Max() + 1;

                    if (Files != null)
                        await UploadFiles(model.Devolucion.Id, Files, order);



                    AddLog("", model.Devolucion.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());

                }
                else
                {
                    ModelState.AddModelError("", "Error Deleting Detail (DevolucionItems)");
                }

            }

            return await GetView(model.Devolucion.Id);
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
            var devolucion = await db.Devoluciones.FindAsync(id);

            try
            {
                if (devolucion != null)
                {
                    //if (await DeleteItems(id, null, false) && await DeleteFiles(id))
                    //{
                    //    db.Devoluciones.Remove(devolucion);
                    //    await db.SaveChangesAsync();
                    //    AddLog("", id, devolucion);
                    //    return RedirectToAction("Index", GetReturnSearch());
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Error Deleting Detail (DevolucionItems)");
                    //}

                    devolucion.Estado = EstadoFormatoPQRS.Deleted;

                    db.Entry(devolucion).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", id, devolucion);
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

                    DevolucionArchivo da = new DevolucionArchivo();
                    da.DevolucionId = Id;
                    da.Order = order++;
                    da.File = Fn.ConvertToByte(file);
                    da.FileName = file.FileName;
                    da.MediaType = file.ContentType;
                    db.DevolucionArchivos.Add(da);

                }

            }


            if (db.DevolucionArchivos.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarArchivo(int d, int o)
        {
            var data = await db.DevolucionArchivos
                               .Where(da => da.DevolucionId == d && da.Order == o)
                               .Select(da => new { da.File, da.MediaType, da.FileName })
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
                var di = await db.DevolucionItems.Where(i => i.DevolucionId == id).ToListAsync();
                if (Items == null && !Partial)
                    db.DevolucionItems.RemoveRange(di);
                else if (Items != null)
                    foreach (int i in Items)
                    {
                        db.DevolucionItems.Remove(di.Where(it => it.Id == i).FirstOrDefault());
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
                var di = await db.DevolucionItems.Where(i => i.DevolucionId == id).ToListAsync();
                if (di.Count > 0)
                {
                    db.DevolucionItems.RemoveRange(di);
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