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
    public class NovedadesController : BaseController
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

            var novedad = await db.Novedad
                                     .FindAsync(id);

            if (novedad == null)
            {
                return HttpNotFound();
            }


            var items = await db.NovedadItem
                                .Include(gi => gi.Items)
                                .Include(gi => gi.MotivoPQRS)
                                .Where(di => di.NovedadId == id).ToListAsync();

            var archivos = await db.NovedadArchivo
                                   .Where(ga => ga.NovedadId == id)
                                   .Select(ga => new NovedadViewModel.Archivos { Order = ga.Order, FileName = ga.FileName })
                                   .ToListAsync();


            if (GetCrudMode() == Fn.CrudMode.Edit || GetCrudMode() == Fn.CrudMode.Delete)
            {
                if (novedad.Estado == EstadoFormatoPQRS.Deleted || novedad.Estado == EstadoFormatoPQRS.Completed)
                {
                    return RedirectToAction("Index", GetReturnSearch());
                }

                foreach (var item in items)
                {

                    if (novedad.Estado == EstadoFormatoPQRS.In_Process)
                    {
                        var pqrs = await db.PQRSRecords
                                       .Where(p => p.Id == item.PQRSRecordId && p.PasoActual == true && p.FlujoPQRSTipoPaso == TipoPaso.LlenarFormato && p.TipoPQRS == TipoPQRS.Novedad)
                                       .FirstOrDefaultAsync();

                        if (pqrs == null)
                            ViewData["has_pqrs_" + item.NovedadId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                        else
                        {
                            var usuarioPQRS = await db.PQRSRecordUsuarios
                                                      .Where(pu => pu.PQRSRecordId == pqrs.Id && pu.PQRSRecordOrder == pqrs.Order && pu.UsuarioId == seguridadcll.Usuario.UsuarioId)
                                                      .FirstOrDefaultAsync();
                            if (usuarioPQRS == null)
                                ViewData["has_pqrs_" + item.NovedadId + "_" + item.Id + "_" + item.MotivoPQRSId] = "hide";
                            else
                                ViewData["has_pqrs_" + item.NovedadId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";
                        }
                    }
                    else
                        ViewData["has_pqrs_" + item.NovedadId + "_" + item.Id + "_" + item.MotivoPQRSId] = "";

                }
            }

            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Novedad && m.Activo == true).ToListAsync();
            ViewBag.CausaPQRS = await db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Novedad).ToListAsync();
            return View(GetCrudMode().ToString(), new NovedadViewModel
            {
                Novedad = novedad,
                Items = items,
                NovedadArchivos = archivos
            });
        }


        [AuthorizeAction]
        [FillPermission("Novedades/Edit")]
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
            ViewBag.MotivoPQRS = db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Novedad && m.Activo == true).ToList();
            ViewBag.CausaPQRS = db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Novedad).ToList();
            return View();
        }



        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(NovedadViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            //var f = await UploadFiles(0, Files);

            if (ModelState.IsValid)
            {
                //Save News
                model.Novedad.Estado = EstadoFormatoPQRS.Open;
                model.Novedad.FechaCreacion = DateTime.Now;
                db.Novedad.Add(model.Novedad);
                await db.SaveChangesAsync();


                //Genera HASHNroTracking
                string HASHNroTracking = Fn.HASH("N" + model.Novedad.Id);
                HASHNroTracking = "N" + model.Novedad.Id + HASHNroTracking;
                model.Novedad.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();

                db.Entry(model.Novedad).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //Save Items
                if (model.Items != null)
                {
                    int i = 1;
                    foreach (var item in model.Items)
                    {
                        item.NovedadId = model.Novedad.Id;
                        item.Id = i++;

                        db.NovedadItem.Add(item);

                    }

                    await db.SaveChangesAsync();

                }

                //Save Files
                if (Files != null)
                    await UploadFiles(model.Novedad.Id, Files, 1);

                AddLog("", model.Novedad.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Novedad && m.Activo == true).ToListAsync();
            return View(model);

        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(NovedadViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            if (ModelState.IsValid)
            {
                //Guardar Cabecera
                db.Entry(model.Novedad).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //si es null ponerle el valor en 1


                //int idx = await db.Database.SqlQuery<int>($"SELECT ISNULL(MAX(Id),0) FROM DevolucionItem WHERE DevolucionId = {model.Devolucion.Id} ").FirstOrDefaultAsync();

                //Elimina los items Delete
                if (await DeleteItems(model.Novedad.Id, model.ItemsDelete, true))
                {
                    int idx = 0;
                    var currentItems = await db.NovedadItem.Where(i => i.NovedadId == model.Novedad.Id).ToListAsync();
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
                                item.NovedadId = model.Novedad.Id;
                                item.Id = ++idx;
                                db.NovedadItem.Add(item);

                            }
                        }


                    await db.SaveChangesAsync();


                    //Archivos
                    var currentFiles = await db.NovedadArchivo
                                               .Where(da => da.NovedadId == model.Novedad.Id)
                                               .ToListAsync();


                    if (model.NovedadArchivos != null)
                    {
                        var FilesId = model.NovedadArchivos.Select(a => a.Order).ToArray();


                        var itemsToDelete = currentFiles.Where(a => !FilesId.Contains(a.Order)).ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.NovedadArchivo.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var itemsToDelete = currentFiles.ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.NovedadArchivo.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }


                    //Save Files
                    int order = 1;
                    if (currentFiles.Count > 0)
                        order = currentFiles.Select(i => i.Order).Max() + 1;

                    if (Files != null)
                        await UploadFiles(model.Novedad.Id, Files, order);


                    AddLog("", model.Novedad.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());

                }
                else
                {
                    ModelState.AddModelError("", "Error Deleting Detail (NovedadItems)");
                }

            }

            return await GetView(model.Novedad.Id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }


        private async Task<bool> UploadFiles(int Id, IEnumerable<HttpPostedFileBase> Files, int porder)
        {

            int order = porder;
            foreach (var file in Files)
            {
                if (file != null && file.ContentLength > 0)
                {

                    NovedadArchivo na = new NovedadArchivo();
                    na.NovedadId = Id;
                    na.Order = order++;
                    na.File = Fn.ConvertToByte(file);
                    na.FileName = file.FileName;
                    na.MediaType = file.ContentType;

                    db.NovedadArchivo.Add(na);
                }


            }

            if (db.NovedadArchivo.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }


        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var novedad = await db.Novedad.FindAsync(id);

            try
            {
                if (novedad != null)
                {
                    //if (await DeleteItems(id, null, false) && await DeleteFiles(id))
                    //{
                    //    db.Novedad.Remove(novedad);
                    //    await db.SaveChangesAsync();
                    //    AddLog("", id, novedad);
                    //    return RedirectToAction("Index", GetReturnSearch());
                    //}
                    //else
                    //{
                    //    ModelState.AddModelError("", "Error Deleting Detail (NovedadItems)");
                    //}


                    novedad.Estado = EstadoFormatoPQRS.Deleted;
                    db.Entry(novedad).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", id, novedad);
                    return RedirectToAction("Index", GetReturnSearch());
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return await GetView(id);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarArchivo(int d, int o)
        {
            var data = await db.NovedadArchivo
                               .Where(na => na.NovedadId == d && na.Order == o)
                               .Select(na => new { na.File, na.MediaType, na.FileName })
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
                var ni = await db.NovedadItem.Where(i => i.NovedadId == id).ToListAsync();
                if (Items == null && !Partial)
                    db.NovedadItem.RemoveRange(ni);
                else if (Items != null)
                    foreach (int i in Items)
                    {
                        db.NovedadItem.Remove(ni.Where(it => it.Id == i).FirstOrDefault());
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
                var na = await db.NovedadArchivo.Where(a => a.NovedadId == id).ToListAsync();
                if (na.Count > 0)
                {
                    db.NovedadArchivo.RemoveRange(na);
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