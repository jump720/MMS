using Microsoft.Reporting.WebForms;
using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MMS.Controllers.Transacciones
{
    public class ActividadesController : BaseController
    {


        //Variable de Contexto de la Base de Datos
        private MMSContext db = new MMSContext();

        [AuthorizeAction("Actividades/Index")]
        [FillPermission("Actividades/Approve", "Actividades/CambiaEstadoActividad", "Actividades/Report", "Actividades/Duplicar")]
        public ActionResult Index()
        {

            return View("IndexN");
        }

        private async Task<ActionResult> GetView(int id)
        {
            var actividad = await db.Actividad.FindAsync(id);
            var actividadItems = await db.ActividadItem
                .Select(i => new ActividadViewModel.ActividadItemViewModel
                {
                    ActividadId = i.ActividadId,
                    ActividadItemId = i.ActividadItemId,
                    ActividadItemCantidad = i.ActividadItemCantidad,
                    ActividadItemProducto = i.ActividadItemProducto,
                    ActividadItemPrecio = i.ActividadItemPrecio,
                    ActividadItemDescripcion = i.ActividadItemDescripcion,
                    ProductoId = i.ProductoId,
                    CentroCostoID = i.CentroCostoID,
                    delete = false
                })
                .Where(i => i.ActividadId == id).ToListAsync();

            if (actividad.ActividadEstado != EstadosActividades.Abierto && actividad.ActividadEstado != EstadosActividades.Rechazado && GetCrudMode() != Fn.CrudMode.Details)
            {
                return RedirectToAction("Details", "Actividades", new { id });
            }

            ViewData["Actividad.TipoActividadID"] = new SelectList(await db.TipoActividades.ToListAsync(), "TipoActividadId", "TipoActividadDesc", actividad.TipoActividadID);
            ViewData["Actividad.CanalID"] = new SelectList(await db.Canales.ToListAsync(), "CanalID", "CanalDesc", actividad.CanalID);
            ViewData["Actividad.MarcaID"] = new SelectList(await db.Marca.ToListAsync(), "Id", "Nombre");
            ViewBag.CentroCosto = await db.CentroCostos.ToListAsync();
            //ViewBag.EstadoCierre = new SelectList(Fn.EnumToIEnumarable<EstadoCierreActividad>(), "Value", "Name");
            var archivos = await db.ActividadArchivo
                              .Where(di => di.ActividadId == id)
                              .Select(da => new ActividadViewModel.Archivo { Order = da.Order, FileName = da.FileName })
                              .ToListAsync();

            var ActividadAut = await db.ActividadAutorizacion.Include(aa => aa.usuario).Where(aa => aa.ActividadId == id).ToListAsync();

            return View(GetCrudMode().ToString() + "N", new ActividadViewModel
            {
                Actividad = actividad,
                Items = actividadItems,
                Archivos = archivos,
                ActividadAutorizaciones = ActividadAut
            });


        }

        // GET: Actividades/Details/5
        [AuthorizeAction]
        [FillPermission("Actividades/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {

            ViewData["Actividad.TipoActividadID"] = new SelectList(await db.TipoActividades.ToListAsync(), "TipoActividadId", "TipoActividadDesc");
            ViewData["Actividad.CanalID"] = new SelectList(await db.Canales.ToListAsync(), "CanalID", "CanalDesc");
            ViewData["Actividad.MarcaID"] = new SelectList(await db.Marca.ToListAsync(), "Id", "Nombre");
            ViewBag.CentroCosto = await db.CentroCostos.ToListAsync();


            return View("CreateN", new ActividadViewModel
            {
                Actividad = new Actividad
                {
                    ActividadId = 0,
                    UsuarioIdElabora = Seguridadcll.Usuario.UsuarioId,
                    ActividadEstado = EstadosActividades.Abierto,
                    ActividadFecha = DateTime.Now,
                    ActividadFechaDesde = DateTime.Now.AddDays(1),
                    ActividadFechaHasta = DateTime.Now.AddDays(2),
                    ActividadFechaMod = DateTime.Now,
                    ActividadFechaAprob = DateTime.Now
                },
                Items = new List<ActividadViewModel.ActividadItemViewModel>()
            });
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ActividadViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            bool flagAdd = true;
            //Validar Cantidad de Actividades para el mismo cliente en el mismo mes.(Fecha inicio y fin de la actividad)
            //var actvidadesMes = await db.Actividad.Where(a => a.ClienteID == model.Actividad.ClienteID &&
            //                                            a.ActividadFechaDesde.Month == model.Actividad.ActividadFechaDesde.Month && a.ActividadFechaDesde.Year == model.Actividad.ActividadFechaDesde.Year).ToListAsync();

            //if (actvidadesMes.Count > 5)
            //{
            //    ModelState.AddModelError("", "Error, Cliente:" + model.Actividad.ClienteID + " con mas de dos actividades en el mes " + model.Actividad.ActividadFechaDesde.Month.ToString() + " de la actividad");
            //    flagAdd = false;
            //}
            try
            {
                if (ModelState.IsValid && flagAdd)
                {
                    db.Actividad.Add(model.Actividad);
                    await db.SaveChangesAsync();
                    AddLog("", model.Actividad.ActividadId, model.Actividad);
                    foreach (var item in model.Items)
                    {
                        if (!item.delete)
                        {
                            ActividadItem NewItem = new ActividadItem();
                            NewItem.ActividadId = model.Actividad.ActividadId;
                            NewItem.ActividadItemId = item.ActividadItemId;
                            NewItem.ActividadItemCantidad = item.ActividadItemCantidad;
                            NewItem.ActividadItemProducto = item.ActividadItemProducto;
                            NewItem.ActividadItemPrecio = Decimal.Parse(item.ActividadItemPrecio.ToString().Replace(".", ","));
                            NewItem.ActividadItemDescripcion = item.ActividadItemDescripcion;
                            NewItem.ProductoId = item.ProductoId;
                            NewItem.CentroCostoID = item.CentroCostoID;

                            db.ActividadItem.Add(NewItem);
                        }
                    }
                    await db.SaveChangesAsync();

                    //Save Files
                    if (Files != null)
                        await UploadFiles(model.Actividad.ActividadId, Files, 1);

                    //AddLog("", model.Actividad.ActividadId, model.Items);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Modelo no valido");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.ToString());
            }

            ViewData["Actividad.TipoActividadID"] = new SelectList(await db.TipoActividades.ToListAsync(), "TipoActividadId", "TipoActividadDesc", model.Actividad.TipoActividadID);
            ViewData["Actividad.CanalID"] = new SelectList(await db.Canales.ToListAsync(), "CanalID", "CanalDesc", model.Actividad.CanalID);
            ViewData["Actividad.MarcaID"] = new SelectList(await db.Marca.ToListAsync(), "Id", "Nombre");
            ViewBag.CentroCosto = await db.CentroCostos.ToListAsync();


            return View("CreateN", new ActividadViewModel
            {
                Actividad = model.Actividad,
                Items = model.Items
            });
        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ActividadViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {
            bool flagAdd = true;
            //Validar Cantidad de Actividades para el mismo cliente en el mismo mes.(Fecha inicio y fin de la actividad)
            //var actvidadesMes = await db.Actividad.Where(a => a.ClienteID == model.Actividad.ClienteID && a.ActividadId != model.Actividad.ActividadId &&
            //                                            a.ActividadFechaDesde.Month == model.Actividad.ActividadFechaDesde.Month && a.ActividadFechaDesde.Year == model.Actividad.ActividadFechaDesde.Year).ToListAsync();

            //if (actvidadesMes.Count > 5)
            //{
            //    ModelState.AddModelError("", "Error, Cliente:" + model.Actividad.ClienteID + " con mas de dos actividades en el mes " + model.Actividad.ActividadFechaDesde.Month.ToString() + " de la actividad");
            //    flagAdd = false;
            //}

            try
            {
                if (ModelState.IsValid && flagAdd)
                {
                    db.Entry(model.Actividad).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", model.Actividad.ActividadId, model.Actividad);

                    foreach (var item in model.Items)
                    {
                        if (item.delete)
                        {
                            var itemDelete = await db.ActividadItem.Where(ai => ai.ActividadId == model.Actividad.ActividadId && ai.ActividadItemId == item.ActividadItemId).FirstOrDefaultAsync();

                            if (itemDelete != null)
                            {
                                db.ActividadItem.Remove(itemDelete);
                            }
                        }
                        else
                        {
                            if (item.ActividadItemId == 0)
                            {
                                ActividadItem NewItem = new ActividadItem();
                                NewItem.ActividadId = model.Actividad.ActividadId;
                                NewItem.ActividadItemId = item.ActividadItemId;
                                NewItem.ActividadItemCantidad = item.ActividadItemCantidad;
                                NewItem.ActividadItemProducto = item.ActividadItemProducto;
                                NewItem.ActividadItemPrecio = Decimal.Parse(item.ActividadItemPrecio.ToString().Replace(".", ","));
                                NewItem.ActividadItemDescripcion = item.ActividadItemDescripcion;
                                NewItem.ProductoId = item.ProductoId;
                                NewItem.CentroCostoID = item.CentroCostoID;

                                db.ActividadItem.Add(NewItem);
                            }
                            else
                            {
                                var itemEdit = await db.ActividadItem.Where(ai => ai.ActividadId == model.Actividad.ActividadId && ai.ActividadItemId == item.ActividadItemId).FirstOrDefaultAsync();
                                itemEdit.ActividadId = model.Actividad.ActividadId;
                                itemEdit.ActividadItemId = item.ActividadItemId;
                                itemEdit.ActividadItemCantidad = item.ActividadItemCantidad;
                                itemEdit.ActividadItemProducto = item.ActividadItemProducto;
                                itemEdit.ActividadItemPrecio = Decimal.Parse(item.ActividadItemPrecio.ToString().Replace(".", ","));
                                itemEdit.ActividadItemDescripcion = item.ActividadItemDescripcion;
                                itemEdit.ProductoId = item.ProductoId;
                                itemEdit.CentroCostoID = item.CentroCostoID;
                                db.Entry(itemEdit).State = EntityState.Modified;
                            }
                        }
                    }
                    await db.SaveChangesAsync();

                    //Archivos
                    var currentFiles = await db.ActividadArchivo
                                               .Where(da => da.ActividadId == model.Actividad.ActividadId)
                                               .ToListAsync();

                    if (model.Archivos != null)
                    {
                        var FilesId = model.Archivos.Select(a => a.Order).ToArray();


                        var itemsToDelete = currentFiles.Where(a => !FilesId.Contains(a.Order)).ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.ActividadArchivo.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        var itemsToDelete = currentFiles.ToList();

                        if (itemsToDelete.Count > 0)
                        {
                            db.ActividadArchivo.RemoveRange(itemsToDelete);
                            await db.SaveChangesAsync();
                        }
                    }


                    //Save Files
                    int order = 1;
                    if (currentFiles.Count > 0)
                        order = currentFiles.Select(i => i.Order).Max() + 1;

                    if (Files != null)
                        await UploadFiles(model.Actividad.ActividadId, Files, order);

                    //AddLog("", model.Actividad.ActividadId, model.Items);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Modelo no valido");
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", e.ToString());
            }

            return await GetView(model.Actividad.ActividadId);
        }


        // GET: Actividades/Delete/5
        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }

        // POST: Actividades/Delete/5
        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var actividad = await db.Actividad.FindAsync(id);
            bool flagDelete = EliminaItemsActividad(actividad.ActividadId);
            bool flagAutoriza = EliminaAutorizadoresActividad(actividad.ActividadId);
            if (flagDelete && flagAutoriza)
            {
                db.Actividad.Remove(actividad);
                await db.SaveChangesAsync();

                //Archivos
                var currentFiles = await db.ActividadArchivo
                                           .Where(da => da.ActividadId == id)
                                           .ToListAsync();

                var itemsToDelete = currentFiles.ToList();

                if (itemsToDelete.Count > 0)
                {
                    db.ActividadArchivo.RemoveRange(itemsToDelete);
                    await db.SaveChangesAsync();
                }
                AddLog("", actividad.ActividadId, actividad);
                return RedirectToAction("Index", GetReturnSearch());
            }
            else
            {
                ModelState.AddModelError("", "Error, Eliminando Items o Autorizadores");
                return await GetView(id);
            }

        }



        public ActionResult _ConfirmarEliminar(int i = 0)
        {
            ViewBag.indice = i;
            return PartialView();
        }

        private bool AddValidateItemsACt(FormCollection form, int ActividadId, int Mode)
        {
            //Add Items(ACtividadItems)
            bool flagAdd = true;
            List<ActividadItem> ActividadItemList;
            try
            {
                ActividadItemList = addItemsActividades(form, ActividadId, Mode);
                flagAdd = true;
            }
            catch (Exception e)
            {
                ActividadItemList = new List<ActividadItem>();
                ViewBag.error = "Error al agregar items: " + e.ToString();
                flagAdd = false;
            }
            return flagAdd;
        }

        //genera la lista con lo roles asignados al usuario
        private List<ActividadItem> addItemsActividades(FormCollection form, int ActividadId, int Mode)
        {
            List<ActividadItem> ActividadItemList = new List<ActividadItem>();
            decimal? MontoAut = 0;

            int ln_rows = int.Parse(form["ActividadUltimoItem"].ToString());
            // string prueba= form["ActividadItemDesc" + 0].ToString() ;

            //Seguridadcll cll_seguridad = (Seguridadcll)Session["seguridad"];
            //int ln_item = 1;
            for (int row = 0; row <= ln_rows; row++)
            {
                bool flagExists = form.AllKeys.Contains("ActividadItemId" + row);
                if (flagExists)
                {
                    if (!string.IsNullOrEmpty(form["ActividadItemId" + row].ToString()))
                    {
                        ActividadItem actividadItem = new ActividadItem();
                        actividadItem.ActividadId = ActividadId;
                        // actividadItem.ActividadItemId = int.Parse(form["ActividadItemId" + row].ToString());
                        actividadItem.ProductoId = (!string.IsNullOrEmpty(form["ActividadItemProducto" + row].ToString())) ? form["ActividadItemProducto" + row].ToString() : null;
                        actividadItem.ActividadItemCantidad = (!string.IsNullOrEmpty(form["ActividadItemCantidad" + row].ToString())) ? int.Parse(form["ActividadItemCantidad" + row].ToString()) : 0;
                        actividadItem.ActividadItemPrecio = (!string.IsNullOrEmpty(form["ActividadItemPrecio" + row].ToString())) ? int.Parse(form["ActividadItemPrecio" + row].ToString()) : 0;
                        actividadItem.ActividadItemProducto = (!string.IsNullOrEmpty(form["ActividadItemProducto" + row].ToString())) ? form["ActividadItemProducto" + row].ToString() : null;
                        //actividadItem.ActividadItemDescripcion = "Verificar Descripción";
                        actividadItem.ActividadItemDescripcion = (!string.IsNullOrEmpty(form["ActividadItemDesc" + row].ToString())) ? form["ActividadItemDesc" + row].ToString() : null;
                        actividadItem.CentroCostoID = (!string.IsNullOrEmpty(form["ActividadItemCC" + row].ToString())) ? form["ActividadItemCC" + row].ToString() : null;

                        if (actividadItem.ActividadItemCantidad > 0 && Mode == 1)
                        {
                            ActividadItemList.Add(actividadItem);
                            db.ActividadItem.Add(actividadItem);
                            MontoAut += actividadItem.ActividadItemPrecio * actividadItem.ActividadItemCantidad;
                        }
                    }
                }


            }


            return ActividadItemList;
        }

        [AuthorizeAction]
        [FillPermission("Actividades/Details", "Actividades/ConfirmarApprove")]
        public ActionResult Approve()
        {
            return View("ApproveN");
        }


        // GET: Actividades/ConfirmarApprove/5
        [AuthorizeAction]
        public async Task<ActionResult> ConfirmarApprove(int ActividadId = 0, int tipoAutorizacionId = 0)
        {
            //POSISIÓN [0] Es el ID de la Actividad, [1] el tipo de Autorizacion.
            int idActividad = ActividadId;
            int tipoAutorizacion = tipoAutorizacionId;
            if (idActividad == 0 || tipoAutorizacion == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<ActividadAutorizacion> autorizaciones = new List<ActividadAutorizacion>();
            autorizaciones = await db.ActividadAutorizacion.Include(a => a.Actividad).Where(a => a.ActividadId == idActividad).ToListAsync();
            if (autorizaciones == null)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idActividad;
            }
            else
            {
                //Para conocer el nombre del Usuario que elabora la Actividad.
                Actividad actividadAAutorizar = await db.Actividad.Include(c => c.cliente).Where(a => a.ActividadId == idActividad).FirstOrDefaultAsync();
                ViewBag.UsuarioElabora = db.Usuarios.Where(u => u.UsuarioId == actividadAAutorizar.UsuarioIdElabora).First().UsuarioNombre;
                //Para conocer el estado, si es Rechazado por un Usuario no permite realizar ninguna acción.
                bool rechazado = autorizaciones.Any(u => u.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Rechazado);
                if (!rechazado && (EstadoAutorizaActividad)tipoAutorizacion == EstadoAutorizaActividad.Autorizado)
                {
                    ViewBag.TipoAutorizacion = EstadoAutorizaActividad.Autorizado;
                }
                else if (!rechazado && (EstadoAutorizaActividad)tipoAutorizacion == EstadoAutorizaActividad.Rechazado)
                {
                    ViewBag.TipoAutorizacion = EstadoAutorizaActividad.Rechazado;
                }
                else
                {
                    ViewBag.Error = "La actividad ha sido rechazada por otro usuario.";
                }
            }
            return PartialView(autorizaciones.FirstOrDefault());
        }

        // POST: Actividades/Approve/5
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve(ActividadAutorizacion actividadAutorizacion, FormCollection form)
        {


            //Lista de los autorizadores o aprobadores de la actividad
            var todasAutorizaciones = db.ActividadAutorizacion
                                        .Include(a => a.usuario)
                                        .Include(a => a.Actividad)
                                        .Include(a => a.Actividad.cliente)
                                        .Where(a => a.ActividadId == actividadAutorizacion.ActividadId)
                                        .ToList();

            //Autorización usuario Actual
            ActividadAutorizacion autorizacionUsuarioActual = todasAutorizaciones
                                                                .Where(a => a.UsuarioIdAutoriza == Seguridadcll.Usuario.UsuarioId)
                                                                .FirstOrDefault();

            //Actividad a autorizar
            var actividad = db.Actividad.Include(a => a.ActividadItemList).Where(a => a.ActividadId == actividadAutorizacion.ActividadId).FirstOrDefault();

            //Obtener el Correo del Elaborador.
            var usuarioId = todasAutorizaciones.FirstOrDefault().Actividad.UsuarioIdElabora.ToLower();
            var elaborador = (from u in db.Usuarios
                              where (u.UsuarioId.ToLower() == usuarioId)
                              select new { u.UsuarioId, u.UsuarioCorreo, u.UsuarioNombre }).FirstOrDefault();

            //Obtener el Correo del Vendedor asignado al cliente.
            var VendedorId = todasAutorizaciones.FirstOrDefault().Actividad.cliente.VendedorId;
            var Vendedor = (from u in db.Usuarios
                            where (u.UsuarioId.ToLower() == VendedorId)
                            select new { u.UsuarioId, u.UsuarioCorreo, u.UsuarioNombre }).FirstOrDefault();

            //Obtener valores del Formulario.
            var pTipoAutorizacion = form.GetValues("TipoAutorizacion");
            var pMotivoAutorizacion = form.GetValues("ActividadAutorizacionMotivo");

            bool FlagAutorizacion = false;
            try
            {
                if (autorizacionUsuarioActual == null)
                {
                    ViewBag.Error = "Advertencia, Registro no encontrado o Inválido " + actividadAutorizacion.ActividadId;
                }
                else
                {
                    //Cambiar a la fecha actual.
                    autorizacionUsuarioActual.ActividadAutorizacionFecha = DateTime.Now;

                    //Variables para el envío de correos.
                    List<Mails> mails = new List<Mails>();
                    //bool flagAdd = false;
                    //string to = "";
                    string subject = "";
                    //Obtiene valor de la actividad
                    decimal valor = actividad.ActividadItemList.Sum(a => (a.ActividadItemPrecio * a.ActividadItemCantidad)) ?? 0;
                    string valorTotal = String.Format("{0:C}", valor);

                    //Cambiar a Autorizado (autorizo el aprobador)
                    if (pTipoAutorizacion[0] == EstadoAutorizaActividad.Autorizado.ToString())
                    {
                        ////Si esta en estado PendienteTrade -> Cambia a estado Pendiente(envia correo a los otros aprobadores)
                        //if (todasAutorizaciones.Where(a => a.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Por_Autorizar).Count() == 1)
                        //{
                        //    //Pendiente
                        //    await CambiaEstadoActividad(actividad.ActividadId, 2);

                        //    //Actualiza lista de Aprobadores
                        //    todasAutorizaciones = db.ActividadAutorizacion
                        //                            .Include(a => a.usuario)
                        //                            .Include(a => a.Actividad.cliente)
                        //                            .Where(a => a.ActividadId == actividadAutorizacion.ActividadId)
                        //                            .ToList();
                        //}//if (actividad.ActividadEstado == EstadosActividades.PendienteTrade && todasAutorizaciones.Where(a => a.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Por_Autorizar && (a.usuario.UsuarioAprobadorPrincipal ?? false)).Count() == 1)
                        
                        //Aprueba el actual
                        autorizacionUsuarioActual.ActividadAutorizacionAutoriza = EstadoAutorizaActividad.Autorizado;
                        autorizacionUsuarioActual.ActividadAutorizacionMotivo = pMotivoAutorizacion[0];
                        db.Entry(autorizacionUsuarioActual).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        //Obtiene el siguiente en aprobar
                        ActividadAutorizacion nextAprobador = await db.ActividadAutorizacion
                            .Where(aa => aa.ActividadId == actividadAutorizacion.ActividadId && aa.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Pendiente 
                            && aa.Orden == db.ActividadAutorizacion
                            .Where(at => at.ActividadId == actividadAutorizacion.ActividadId && at.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Pendiente).Min(at =>at.Orden))
                            .FirstOrDefaultAsync();
                        //si existe el siguiente
                        if (nextAprobador != null)
                        {
                            nextAprobador.ActividadAutorizacionAutoriza = EstadoAutorizaActividad.Por_Autorizar;
                            db.Entry(nextAprobador).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        //Si es la última autorización( no existe siguiente)
                        if (nextAprobador == null)
                        {
                            subject = "MMS-Notificación de Actividad " + autorizacionUsuarioActual.Actividad.ActividadId;

                            //Mensaje para la autorización de las actividades.
                            var mensaje = "<style>";
                            mensaje += "table {";
                            mensaje += "border-collapse: collapse;";
                            mensaje += "}";
                            mensaje += "td, th {";
                            mensaje += "border: 1px solid #dddddd;";
                            mensaje += "text-align: left;";
                            mensaje += "padding: 8px;";
                            mensaje += "}";
                            mensaje += "th {";
                            mensaje += "background-color: #0091ea;";
                            mensaje += "color: white;";
                            mensaje += "}";
                            mensaje += "</style>";
                            mensaje += "<br>Se le informa que la siguiente actividad ha sido Autorizada:<br><br> ";
                            mensaje += "<table><thead><tr>";
                            mensaje += "<th>ID</th>";
                            mensaje += "<th>Título</th>";
                            mensaje += "<th>Cliente</th>";
                            mensaje += "<th>Valor Total</th>";
                            mensaje += "</tr></thead><tbody><tr>";
                            mensaje += "<td>" + autorizacionUsuarioActual.Actividad.ActividadId + "</td>";
                            mensaje += "<td>" + autorizacionUsuarioActual.Actividad.ActividadTitulo + "</td>";
                            mensaje += "<td>(" + autorizacionUsuarioActual.Actividad.ClienteID + ") " + autorizacionUsuarioActual.Actividad.cliente.ClienteRazonSocial + "</td>";
                            mensaje += "<td> " + valorTotal + "</td>";
                            mensaje += "</tr></tbody></table>";
                            mensaje += "<br><p>";
                            mensaje += "<h3>Motivo de Autorización: <h3></p>";
                            mensaje += "<p>" + pMotivoAutorizacion[0] + "</p>";
                            mensaje += "<br><p>";
                            mensaje += "<a href='http://mms.apextoolgroup.com.co/" + RouteData.Values["controller"].ToString() + "/Index'>CLICK AQUÍ</a>";
                            mensaje += " para ver las Actividades.</p>";



                            //Crea el Gasto, Orden y Movimiento
                            if (actividad != null)
                            {
                                await CambiaEstadoActividad(actividad.ActividadId, (int)EstadosActividades.Autorizado);
                                FlagAutorizacion = true;
                                if (FlagAutorizacion)
                                {
                                    bool flagGasto = CreaGastoXActividad(actividad.ActividadId);
                                    OrdenesController ordCtrl = new OrdenesController();
                                    ordCtrl.CreaOrdenXActividad(actividad.ActividadId, (Seguridadcll)HttpContext.Session["seguridad"]);
                                    //return RedirectToAction("CreaOrdenXActividad", "Ordenes", new { actividadId = actividad.ActividadId });
                                }//if (FlagAutorizacion)
                            }//if (actividad != null)

                            //Enviar mensaje al Creador de la Actividad.
                            //to = elaborador.UsuarioCorreo;
                            //flagAdd = Fn.SendEmail(to, subject, mensaje);

                            mails.Add(new Mails { to = elaborador.UsuarioCorreo, toName = elaborador.UsuarioNombre });

                            //Enviar mensaje al Dueño del cliente
                            //to = Vendedor.UsuarioCorreo;
                            //flagAdd = Fn.SendEmail(to, subject, mensaje);

                            mails.Add(new Mails { to = Vendedor.UsuarioCorreo, toName = Vendedor.UsuarioNombre });

                            foreach (var m in mails)
                            {
                                //quitar comentario si ya funciona
                                await Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, mensaje, Seguridadcll.Aplicacion.Link));
                            }


                        }//if (nextAprobador == null)

                    }//if (pTipoAutorizacion[0] == EstadoAutorizaActividad.Autorizado.ToString())

                    //Cambiar a Rechazado Y el motivo.
                    if (pTipoAutorizacion[0] == EstadoAutorizaActividad.Rechazado.ToString())
                    {
                        subject = "MMS-Notificación de Actividad " + autorizacionUsuarioActual.Actividad.ActividadId;

                        //Mensaje para la autorización de las actividades.
                        var mensaje = "<style>";
                        mensaje += "table {";
                        mensaje += "border-collapse: collapse;";
                        mensaje += "}";
                        mensaje += "td, th {";
                        mensaje += "border: 1px solid #dddddd;";
                        mensaje += "text-align: left;";
                        mensaje += "padding: 8px;";
                        mensaje += "}";
                        mensaje += "th {";
                        mensaje += "background-color: #0091ea;";
                        mensaje += "color: white;";
                        mensaje += "}";
                        mensaje += "</style>";
                        mensaje += "<br>Se le informa que el usuario: <b>" + autorizacionUsuarioActual.usuario.UsuarioNombre + "</b> ha Rechazado la siguiente actividad:<br><br> ";
                        mensaje += "<table><thead><tr>";
                        mensaje += "<th>ID</th>";
                        mensaje += "<th>Título</th>";
                        mensaje += "<th>Cliente</th>";
                        mensaje += "<th>Valor Total</th>";
                        mensaje += "</tr></thead><tbody><tr>";
                        mensaje += "<td>" + autorizacionUsuarioActual.Actividad.ActividadId + "</td>";
                        mensaje += "<td>" + autorizacionUsuarioActual.Actividad.ActividadTitulo + "</td>";
                        mensaje += "<td>(" + autorizacionUsuarioActual.Actividad.ClienteID + ") " + autorizacionUsuarioActual.Actividad.cliente.ClienteRazonSocial + "</td>";
                        mensaje += "<td> " + valorTotal + "</td>";
                        mensaje += "</tr></tbody></table>";
                        mensaje += "<br><p>";
                        mensaje += "<h3>Motivo del Rechazo: <h3></p>";
                        mensaje += "<p>" + pMotivoAutorizacion[0] + "</p>";
                        mensaje += "<br><p>";
                        mensaje += "<a href='http://mms.apextoolgroup.com.co/" + RouteData.Values["controller"].ToString() + "/Index'>CLICK AQUÍ</a>";
                        mensaje += " para ver las Actividades.</p>";

                        //Para enviar los mensajes a cada Autorizador.
                        for (int i = 0; i < todasAutorizaciones.Count; i++)
                        {
                            if (todasAutorizaciones[i].UsuarioIdAutoriza != autorizacionUsuarioActual.UsuarioIdAutoriza)
                            {
                                mails.Add(new Mails { to = todasAutorizaciones[i].usuario.UsuarioCorreo, toName = todasAutorizaciones[i].usuario.UsuarioNombre });
                                //to = todasAutorizaciones[i].usuario.UsuarioCorreo;
                                //flagAdd = Fn.SendEmail(to, subject, mensaje);
                            }
                        }

                        //Enviar mensaje al creador de la actividad.
                        //to = elaborador.UsuarioCorreo;
                        //flagAdd = Fn.SendEmail(to, subject, mensaje);

                        mails.Add(new Mails { to = elaborador.UsuarioCorreo, toName = elaborador.UsuarioNombre });

                        foreach (var m in mails)
                        {
                            //quitar comentario si ya funciona
                            await Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, mensaje, Seguridadcll.Aplicacion.Link));
                        }

                        autorizacionUsuarioActual.ActividadAutorizacionAutoriza = EstadoAutorizaActividad.Rechazado;
                        autorizacionUsuarioActual.ActividadAutorizacionMotivo = pMotivoAutorizacion[0];
                        db.Entry(autorizacionUsuarioActual).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                        if (actividad != null)
                        {
                            await CambiaEstadoActividad(actividad.ActividadId, (int)EstadosActividades.Rechazado);

                        }//if (actividad != null)

                     
                    }//if (pTipoAutorizacion[0] == EstadoAutorizaActividad.Rechazado.ToString())
                    
                }
            }
            catch (Exception e)
            {
                if (autorizacionUsuarioActual == null)
                {
                    ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + actividadAutorizacion.ActividadId;
                }
                else
                {
                    ViewBag.Error = e.ToString();
                }
            }


            return RedirectToAction("Approve", RouteData.Values["controller"].ToString());
        }


        private void addAutorizadoresActividades(int ActividadId)
        {
            var actividadAutorizaciones = db.ActividadAutorizacion.Where(a => a.ActividadId == ActividadId).ToList();
            var actividad = db.Actividad.Where(a => a.ActividadId == ActividadId).FirstOrDefault();
            var usuarioElabora = db.Usuarios.Where(u => u.UsuarioId == actividad.UsuarioIdElabora).FirstOrDefault();

            db.ActividadAutorizacion.RemoveRange(actividadAutorizaciones);
            db.SaveChanges();

            //List<Usuario> usuariosList = new List<Usuario>();
            List<RolUsuario> roles;
            int Orden = 0;
            #region Nivel 1 Aprobación (Jefe del vendedor)
            var rolVendedor = db.Configuracion.Select(c => c.ConfigTipoUsuVendedor).FirstOrDefault();
            roles = db.RolUsuario.Where(ru => ru.UsuarioId == usuarioElabora.UsuarioId).ToList();
            if (roles.Where(r => r.RolId == rolVendedor).Count() == 1)
            {
                var usuario = db.Usuarios.Where(u => u.UsuarioId == usuarioElabora.UsuarioPadreId).FirstOrDefault();
                if (usuario != null)
                {
                    ActividadAutorizacion Autorizacion = new ActividadAutorizacion();
                    Autorizacion.ActividadId = ActividadId;
                    Autorizacion.ActividadAutorizacionFecha = System.DateTime.Now;
                    Autorizacion.ActividadAutorizacionAutoriza = EstadoAutorizaActividad.Por_Autorizar;
                    Autorizacion.Orden = Orden;
                    Autorizacion.AprobacionDesc = "Aprobación Jefe";
                    Autorizacion.UsuarioIdAutoriza = usuario.UsuarioId;

                    db.ActividadAutorizacion.Add(Autorizacion);

                    Orden = 1;
                }

            }
            #endregion
            var nivelesA = db.NivelesAprobacion.Where(na => na.PlantaID == actividad.PlantaID && na.CanalID == actividad.CanalID).ToList();

            foreach (var n in nivelesA)
            {
                ActividadAutorizacion Autorizacion = new ActividadAutorizacion();
                Autorizacion.ActividadId = ActividadId;
                Autorizacion.ActividadAutorizacionFecha = System.DateTime.Now;
                Autorizacion.ActividadAutorizacionAutoriza = (Orden == 1) ? EstadoAutorizaActividad.Pendiente : EstadoAutorizaActividad.Por_Autorizar;
                Autorizacion.Orden = n.Orden;
                Autorizacion.AprobacionDesc = n.Descripcion;
                Autorizacion.UsuarioIdAutoriza = n.UsuarioId;

                db.ActividadAutorizacion.Add(Autorizacion);
                Orden = 1;
            }

            db.SaveChanges();
        }


        // GET: Actividades/Edit/5
        public PartialViewResult Upload()
        {
            return PartialView();
        }

        public ActionResult UploadAction()
        {

            string Key1 = "";
            //  string Key1 = Request.QueryString["ActividadKeyFiles"];

            bool isSavedSuccessfully = true;
            string fName = "";
            string key = System.Web.Security.Membership.GeneratePassword(50, 0);
            string regExp = "[\\W_]";
            key = System.Text.RegularExpressions.Regex.Replace(key, regExp, "");
            key = key.ToUpper();
            key = key.Substring(1, 12);

            if (!string.IsNullOrEmpty(Key1))
            {
                key = Key1;
            }
            else
            {
                //   Request.QueryString["ActividadKeyFiles"] = key;
            }

            var path = "";
            var fileName1 = "";

            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    fName = file.FileName;
                    if (file != null && file.ContentLength > 0)
                    {
                        path = "";
                        fileName1 = "";
                        path = Path.Combine(Server.MapPath("~/UploadFolder/" + key + "/"));
                        string pathString = System.IO.Path.Combine(path.ToString());
                        fileName1 = Path.GetFileName(file.FileName);
                        bool isExists = System.IO.Directory.Exists(pathString);
                        if (!isExists) System.IO.Directory.CreateDirectory(pathString);
                        var uploadpath = string.Format("{0}\\{1}", pathString, file.FileName);
                        file.SaveAs(uploadpath);



                        //return JavaScript("AddAnexos(" + path + ", + " + fileName1 + "," +  DateTime.Today.ToString("u").Substring(0, 10) + ")" );
                        ;
                    }
                }
            }
            catch// (Exception ex)
            {
                isSavedSuccessfully = false;
            }

            if (isSavedSuccessfully)
            {
                return Json(new
                {
                    Message = fName

                },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    Message = "Error in saving file"
                }, JsonRequestBehavior.AllowGet);
            }


            //return View();
        }

        public ActionResult GetProductos()
        {
            List<Producto> productoList = new List<Producto>();

            productoList = null;
            productoList = db.Producto.ToList();

            var result = (from p in productoList
                          select new
                          {
                              id = p.ProductoId,
                              name = p.ProductoDesc,
                              price = p.ProductoPrecio
                          }).ToList();


            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetVentaxCliente(string clienteID)
        {

            int month = DateTime.Today.Month;
            int year = DateTime.Today.Year;
            switch (month)
            {
                case 1:
                    month = 12;
                    year = year - 1;
                    break;
                default:
                    month = month - 1;
                    break;
            }



            var result = db.VentasxCliente
                            .Where(c => c.VentasxClienteMes == month)
                            .Where(c => c.ClienteID == clienteID)
                            .Where(c => c.VentasxClienteAno == year).ToList();

            var presupuesto = (from r in result
                               select new
                               {
                                   ano = r.VentasxClienteAno,
                                   mes = r.VentasxClienteMes,
                                   venta = r.VentasxClienteVenta
                               }).ToList();


            return Json(presupuesto, JsonRequestBehavior.AllowGet);
        }



        [AuthorizeAction]
        public ActionResult Report(string archivo, string filtro = "")
        {

            string filter = "";
            //string title_report = "";

            int ActividadId = int.Parse(filtro);
            filter = filtro.ToLower();
            //title_report = "Reporte de Actividad";
            //Seguridadcll cll_seguridad = (Seguridadcll)Session["seguridad"];
            filter = filtro.ToLower();
            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Reports"), "Transacciones/RptActividades.rdlc");
            if (System.IO.File.Exists(path))
                lr.ReportPath = path;
            else
                return View("Index");
            //var Actividads = new List<dynamic>();
            var Actividad = db.Actividad.Include(a => a.cliente.ciudad).Include(a => a.tipoActividad).Include(a => a.canal).Where(a => a.ActividadId == ActividadId)
                           .Select(a => new
                           {
                               a.ActividadId,
                               a.ActividadFecha,
                               a.ActividadFechaDesde,
                               a.ActividadFechaHasta,
                               a.ActividadTitulo,
                               a.ClienteID,
                               a.cliente.ClienteRazonSocial,
                               a.cliente.ciudad.CiudadDesc,
                               ActividadEstado = a.ActividadEstado.ToString(),
                               a.tipoActividad.TipoActividadDesc,
                               a.ActividadMetaV,
                               a.ActividadMetaE,
                               a.canal.CanalDesc,
                               a.ActividadDesc,
                               a.ActividadObjetivo,
                               a.UsuarioIdElabora,
                               a.ActividadLugarEnvioPOP,
                               a.CumplimientoTotal,
                               a.CumplimientoPorcentaje,
                               a.Resultado,
                               EstadoCierre = a.EstadoCierre.ToString(),
                               a.PlantaID,
                               a.ActividadCuenta
                           })
                           .ToList();

            dynamic ActividadItemList;
            ActividadItemList = db.ActividadItem.Where(a => a.ActividadId == ActividadId).ToList();

            dynamic OrdenItemList;
            OrdenItemList = db.OrdenItems.Include(o => o.orden).Include(o => o.producto).Where(o => o.orden.ActividadId == ActividadId && o.orden.OrdenEstado != EstadoOrden.Eliminado)
                            .Select(o => new
                            {
                                o.OrdenId,
                                o.ProductoId,
                                o.producto.ProductoDesc,
                                o.OrdenItemsVlr,
                                o.OrdenItemsCantConfirmada,
                                o.orden.OrdenNroGuia,
                                o.orden.OrdenFechaDespacho
                            })
                            .ToList();

            dynamic GastoList;
            GastoList = db.Gasto.Include(o => o.producto).Where(o => o.ActividadId == ActividadId && o.GastoEstado != EstadoGasto.Eliminado)
                            .Select(o => new
                            {
                                o.GastoId,
                                o.ProductoId,
                                o.producto.ProductoDesc,
                                o.GastoValor,
                                o.GastoCant,
                                o.GastoFactura,
                                GastoEstado = o.GastoEstado.ToString()
                            })
                            .ToList();

            var ActividadAprobadoresList = db.ActividadAutorizacion.Include(a => a.usuario).Where(a => a.ActividadId == ActividadId)
                                            .Select(a => new { a.UsuarioIdAutoriza, a.usuario.UsuarioNombre })
                                            .ToList();

            var usuarioId = Actividad.FirstOrDefault().UsuarioIdElabora.ToLower();
            var elaborador = (from u in db.Usuarios
                              where (u.UsuarioId.ToLower() == usuarioId)
                              select new { u.UsuarioId, u.UsuarioNombre }).ToList();

            ReportDataSource actividadDS = new ReportDataSource("ActividadDS", Actividad);
            ReportDataSource actividadItemDS = new ReportDataSource("ActividadItemDS", ActividadItemList);
            ReportDataSource OrdenItemDS = new ReportDataSource("OrdenItemDS", OrdenItemList);
            ReportDataSource gastoDS = new ReportDataSource("GastoDS", GastoList);
            ReportDataSource usuarioDS = new ReportDataSource("UsuarioDS", elaborador);
            ReportDataSource aprobadorDS = new ReportDataSource("AprobadorDS", ActividadAprobadoresList);
            lr.DataSources.Add(actividadDS);
            lr.DataSources.Add(actividadItemDS);
            lr.DataSources.Add(OrdenItemDS);
            lr.DataSources.Add(gastoDS);
            lr.DataSources.Add(usuarioDS);
            lr.DataSources.Add(aprobadorDS);

            string reportType = archivo;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo =

                 "<DeviceInfo>" +
                        "  <OutputFormat>" + archivo + "</OutputFormat>" +
                        "  <PageWidth>8.5in</PageWidth>" +
                        "  <PageHeight>11in</PageHeight>" +
                        "  <MarginTop>0.5in</MarginTop>" +
                        "  <MarginLeft>1in</MarginLeft>" +
                        "  <MarginRight>1in</MarginRight>" +
                        "  <MarginBottom>0.5in</MarginBottom>" +
                        "</DeviceInfo>";
            Warning[] warnings;
            string[] streams;
            byte[] renderedBytes;
            renderedBytes = lr.Render(
                reportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            return File(renderedBytes, mimeType);
        }

        /// <summary>
        /// Metodo que elimina el detalle de una actividad
        /// </summary>
        /// <param name="ActividadId"></param>
        private bool EliminaItemsActividad(int ActividadId)
        {
            bool result = false;
            try
            {
                List<ActividadItem> ActividadItemList = db.ActividadItem
                                                       .Where(ai => ai.ActividadId == ActividadId)
                                                       .ToList();

                if (ActividadItemList.Count > 0)
                {
                    ActividadItemList.ForEach(ai => db.ActividadItem.Remove(ai));
                    db.SaveChanges();

                }
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;

        }

        /// <summary>
        /// Metodo que elimina los autorizadores de una actividad
        /// </summary>
        /// <param name="ActividadId"></param>
        private bool EliminaAutorizadoresActividad(int ActividadId)
        {
            bool result = false;
            try
            {
                List<ActividadAutorizacion> ActividadItemList = db.ActividadAutorizacion
                                                       .Where(ai => ai.ActividadId == ActividadId)
                                                       .ToList();

                if (ActividadItemList.Count > 0)
                {
                    ActividadItemList.ForEach(ai => db.ActividadAutorizacion.Remove(ai));
                    db.SaveChanges();

                }
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;

        }

        [AuthorizeAction]
        public async Task<dynamic> CambiaEstadoActividad(int ActividadId, int Estado)
        {
            bool result = false;

            try
            {
                var actividad = await db.Actividad.Where(a => a.ActividadId == ActividadId).FirstOrDefaultAsync();
                EstadosActividades EstadoActual = actividad.ActividadEstado;
                if (actividad != null)
                {
                    actividad.ActividadEstado = (EstadosActividades)Estado;

                    if ((EstadosActividades)Estado == EstadosActividades.Despachado)
                    {
                        //Guarda cambios
                        db.Entry(actividad).State = EntityState.Modified;
                        db.SaveChanges();

                        AfectaPrespuestoXActividad(actividad.ActividadId);
                        SendDespachado(actividad.ActividadId);
                    }
                    //else if ((EstadosActividades)Estado == EstadosActividades.PendienteTrade)
                    else if ((EstadosActividades)Estado == EstadosActividades.Pendiente)
                    {
                        actividad.ActividadFechaAprob = DateTime.Today;
                        //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];//Seguridad

                        //Conocer los roles que tienen permiso de actividad express.
                        var rolActExpress = await db.RolObjeto.Where(ro => ro.Objeto.ObjetoId == "actividades/CreateExpress").Select(ro => ro.RolId).ToListAsync();
                        //Conocer los roles del Usuario de la Actividad.
                        var rolUser = await db.RolUsuario.Where(ru => ru.UsuarioId == Seguridadcll.Usuario.UsuarioId && rolActExpress.Contains(ru.RolId)).Select(ru => ru.RolId).ToListAsync();

                        bool Express = (rolUser.Count >= 1) ? true : false;

                        if (Express)
                        {
                            actividad.ActividadEstado = EstadosActividades.Autorizado;
                            //Guarda cambios
                            db.Entry(actividad).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            //Crea el Gasto, Orden y Movimiento
                            bool flagGasto = CreaGastoXActividad(actividad.ActividadId);
                            OrdenesController ordCtrl = new OrdenesController();
                            ordCtrl.CreaOrdenXActividad(actividad.ActividadId, Seguridadcll);
                        }
                        else
                        {
                            db.Entry(actividad).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            //Agrega al aprobador principal como un aprobador normal y utilizar interfaz de rechazo
                            if (EstadoActual == EstadosActividades.Abierto || EstadoActual == EstadosActividades.Rechazado)
                                addAutorizadoresActividades(actividad.ActividadId);//crea a los autorizadores segun el nivel

                           
                            var autorizaciones = await db.ActividadAutorizacion.Where(aa => aa.ActividadId == actividad.ActividadId).ToListAsync();
                            //si no tiene autorizadores no envia correo y lo pasa autorizado
                            if (autorizaciones.Count <= 0)
                            {

                                actividad.ActividadEstado = EstadosActividades.Autorizado;
                                //Guarda cambios
                                db.Entry(actividad).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                                //Crea el Gasto, Orden y Movimiento
                                bool flagGasto = CreaGastoXActividad(actividad.ActividadId);
                                OrdenesController ordCtrl = new OrdenesController();
                                ordCtrl.CreaOrdenXActividad(actividad.ActividadId, Seguridadcll);
                            }
                            else
                            {
                                ////Enviar Correo para autorización de trade y Jefe si es el caso
                                //sendToAutorizadorTrade(actividad.ActividadId);
                                sendToAutorizadores(actividad.ActividadId);
                            }                           
                        }
                    }
                    else if ((EstadosActividades)Estado == EstadosActividades.Rechazado)
                    {
                        //Enviar Correo de rechazo 
                        db.Entry(actividad).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        db.Entry(actividad).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }


                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch
            {
                result = false;
            }


            return result;
        }

        //Enviar mensajes a los Autorizadores Cuando el estado pasa a Despachado.
        private void SendDespachado(int ActividadId)
        {
#if DEBUG
            return;
#else
            try
            {
                //bool flagAdd = false;
                List<Mails> mails = new List<Mails>();
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                Actividad actividadAAutorizar = db.Actividad.Include(c => c.cliente).Where(a => a.ActividadId == ActividadId).FirstOrDefault();
                var usuarioId = actividadAAutorizar.UsuarioIdElabora.ToLower();

                var usuarioElabora = (from u in db.Usuarios
                                      where (u.UsuarioId.ToLower() == usuarioId)
                                      select new { u.UsuarioId, u.UsuarioCorreo, u.UsuarioNombre }).First();

                string subject = "MMS-Notificación de Actividad despachada: " + actividadAAutorizar.ActividadId;

                decimal valor = actividadAAutorizar.ActividadItemList.Sum(a => (a.ActividadItemPrecio * a.ActividadItemCantidad)) ?? 0;

                string valorTotal = String.Format("{0:C}", valor);
                //Mensaje del despacho de la Actividad.
                var mensaje = "<style>";
                mensaje += "table {";
                mensaje += "border-collapse: collapse;";
                mensaje += "}";
                mensaje += "td, th {";
                mensaje += "border: 1px solid #dddddd;";
                mensaje += "text-align: left;";
                mensaje += "padding: 8px;";
                mensaje += "}";
                mensaje += "th {";
                mensaje += "background-color: #0091ea;";
                mensaje += "color: white;";
                mensaje += "}";
                mensaje += "</style>";
                mensaje += "<br>La siguiente actividad ha sido despachada:<br><br> ";
                mensaje += "<table><thead><tr>";
                mensaje += "<th>ID</th>";
                mensaje += "<th>Título</th>";
                mensaje += "<th>Cliente</th>";
                mensaje += "<th>Valor Total</th>";
                mensaje += "</tr></thead><tbody><tr>";
                mensaje += "<td>" + actividadAAutorizar.ActividadId + "</td>";
                mensaje += "<td>" + actividadAAutorizar.ActividadTitulo + "</td>";
                mensaje += "<td>(" + actividadAAutorizar.ClienteID + ") " + actividadAAutorizar.cliente.ClienteRazonSocial + "</td>";
                mensaje += "<td> " + valorTotal + "</td>";
                mensaje += "</tr></tbody></table>";
                mensaje += "<br><p>";

                var orden = db.Orden.Where(o => o.ActividadId == actividadAAutorizar.ActividadId).FirstOrDefault();
                if (orden != null)
                {
                    mensaje += "<a href='http://wel.apextoolgroup.com.co/magnum/pedidos/formCoordinadora?No_Guia=" + orden.OrdenNroGuia + "'>CLICK AQUÍ</a>";
                    mensaje += " para ver el seguimiento de la guia</p>";
                }
                else
                {
                    mensaje += "<a href='http://mms.apextoolgroup.com.co/Actividades" + "'>CLICK AQUÍ</a>";
                    mensaje += " Para ver todas la actividades</p>";
                }

                mails.Add(new Mails { to = usuarioElabora.UsuarioCorreo, toName = usuarioElabora.UsuarioNombre });

                foreach (var m in mails)
                {
                    Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, mensaje, seguridadcll.Aplicacion.Link));
                }

                //Enviar mensaje al creador de la actividad.
                //to = usuarioElabora.UsuarioCorreo;
                //flagAdd = Fn.SendEmail(to, subject, mensaje);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
#endif
        }

        //Enviar mensajes a los Autorizadores Cuando el estado pasa a Pendiente.
        private void sendToAutorizadores(int ActividadId)
        {
#if DEBUG
            return;
#else
            try
            {
                List<Mails> mails = new List<Mails>();
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                Actividad actividadAAutorizar = db.Actividad.Include(c => c.cliente).Where(a => a.ActividadId == ActividadId).FirstOrDefault();
                var usuarioId = actividadAAutorizar.UsuarioIdElabora.ToLower();

                var usuarioElabora = (from u in db.Usuarios
                                      where (u.UsuarioId.ToLower() == usuarioId)
                                      select new { u.UsuarioId, u.UsuarioNombre }).First();

                string subject = "MMS-Notificación de Actividad " + actividadAAutorizar.ActividadId;

                decimal valor = actividadAAutorizar.ActividadItemList.Sum(a => (a.ActividadItemPrecio * a.ActividadItemCantidad)) ?? 0;

                string valorTotal = String.Format("{0:C}", valor);

                //Mensaje para la autorización de las actividades.
                var mensaje = "<style>";
                mensaje += "table {";
                mensaje += "border-collapse: collapse;";
                mensaje += "}";
                mensaje += "td, th {";
                mensaje += "border: 1px solid #dddddd;";
                mensaje += "text-align: left;";
                mensaje += "padding: 8px;";
                mensaje += "}";
                mensaje += "th {";
                mensaje += "background-color: #0091ea;";
                mensaje += "color: white;";
                mensaje += "}";
                mensaje += "</style>";
                mensaje += "<br>El Usuario <b>" + usuarioElabora.UsuarioNombre + "</b> ha creado la siguiente actividad:<br><br> ";
                mensaje += "<table><thead><tr>";
                mensaje += "<th>ID</th>";
                mensaje += "<th>Título</th>";
                mensaje += "<th>Cliente</th>";
                mensaje += "<th>Valor Total</th>";
                mensaje += "</tr></thead><tbody><tr>";
                mensaje += "<td>" + actividadAAutorizar.ActividadId + "</td>";
                mensaje += "<td>" + actividadAAutorizar.ActividadTitulo + "</td>";
                mensaje += "<td>(" + actividadAAutorizar.ClienteID + ") " + actividadAAutorizar.cliente.ClienteRazonSocial + "</td>";
                mensaje += "<td> " + valorTotal + "</td>";
                mensaje += "</tr></tbody></table>";
                mensaje += "<br><p>";
                mensaje += "<a href='http://mms.apextoolgroup.com.co/" + RouteData.Values["controller"].ToString() + "/Approve'>CLICK AQUÍ</a>";
                mensaje += " para ingresar a la autorización de esta actividad.</p>";

                //bool flagAdd = false;

                var ActividadAprobadoresList = db.ActividadAutorizacion.Include(a => a.usuario).Where(a => a.ActividadId == ActividadId && a.ActividadAutorizacionAutoriza == EstadoAutorizaActividad.Por_Autorizar)
                                                .Select(a => new { a.UsuarioIdAutoriza, a.usuario.UsuarioNombre, a.usuario.UsuarioCorreo, a.usuario.UsuarioAprobadorPrincipal })
                                                .ToList();

                //Para enviar los mensajes a cada Autorizador.
                for (int i = 0; i < ActividadAprobadoresList.Count; i++)
                {
                    if (((ActividadAprobadoresList[i].UsuarioAprobadorPrincipal) ?? false) == false)
                    {
                        mails.Add(new Mails { to = ActividadAprobadoresList[i].UsuarioCorreo, toName = ActividadAprobadoresList[i].UsuarioNombre });
                        //to = ActividadAprobadoresList[i].UsuarioCorreo;
                        //flagAdd = Fn.SendEmail(to, subject, mensaje);
                    }
                }
                foreach (var m in mails)
                {
                    Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, mensaje, seguridadcll.Aplicacion.Link));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
#endif
        }


        //Enviar mensaje al autorizador principal Cuando el estado pasa a Pendiente por trade.
        private void sendToAutorizadorTrade(int ActividadId)
        {
#if DEBUG
            return;

#else
            try
            {
                List<Mails> mails = new List<Mails>();
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                Actividad actividad = db.Actividad
                                        .Include(c => c.ActividadItemList)
                                        .Include(c => c.cliente).Where(a => a.ActividadId == ActividadId).FirstOrDefault();

                if (actividad != null)
                {
                    var usuarioId = actividad.UsuarioIdElabora.ToLower();

                    var usuarioElabora = (from u in db.Usuarios
                                          where (u.UsuarioId.ToLower() == usuarioId)
                                          select new { u.UsuarioId, u.UsuarioNombre, u.UsuarioPadreId }).First();

                    string subject = "MMS-Notificación Actividad " + actividad.ActividadId;

                    decimal valor = actividad.ActividadItemList.Sum(a => (a.ActividadItemPrecio * a.ActividadItemCantidad)) ?? 0;

                    string valorTotal = String.Format("{0:C}", valor);

                    var mensaje = "<style>";
                    mensaje += "table {";
                    mensaje += "border-collapse: collapse;";
                    mensaje += "}";
                    mensaje += "td, th {";
                    mensaje += "border: 1px solid #dddddd;";
                    mensaje += "text-align: left;";
                    mensaje += "padding: 8px;";
                    mensaje += "}";
                    mensaje += "th {";
                    mensaje += "background-color: #0091ea;";
                    mensaje += "color: white;";
                    mensaje += "}";
                    mensaje += "</style>";
                    mensaje += "<br>El Usuario <b>" + usuarioElabora.UsuarioNombre + "</b> ha creado la siguiente actividad:<br><br> ";
                    mensaje += "<table><thead><tr>";
                    mensaje += "<th>ID</th>";
                    mensaje += "<th>Título</th>";
                    mensaje += "<th>Cliente</th>";
                    mensaje += "<th>Valor Total</th>";
                    mensaje += "</tr></thead><tbody><tr>";
                    mensaje += "<td>" + actividad.ActividadId + "</td>";
                    mensaje += "<td>" + actividad.ActividadTitulo + "</td>";
                    mensaje += "<td>(" + actividad.ClienteID + ") " + actividad.cliente.ClienteRazonSocial + "</td>";
                    mensaje += "<td> " + valorTotal + "</td>";
                    mensaje += "</tr></tbody></table>";
                    mensaje += "<br><p>";
                    mensaje += "<a href='http://mms.apextoolgroup.com.co/" + RouteData.Values["controller"].ToString() + "/Approve'>CLICK AQUÍ</a>";
                    mensaje += " para ingresar a la autorización de Trade</p>";


                    //bool flagSend = false;

                    //var usuariosAprobadorPrincipal = db.Usuarios.Where(u => u.UsuarioAprobadorPrincipal == true).ToList();

                    var vendedor = db.Configuracion.Select(c => c.ConfigTipoUsuVendedor).FirstOrDefault();
                    List<Usuario> usuariosList;
                    List<RolUsuario> rolesList;

                    rolesList = db.RolUsuario.Where(ru => ru.UsuarioId == usuarioElabora.UsuarioId).ToList();

                    if (rolesList.Where(r => r.RolId == vendedor).Count() == 1)
                    {
                        usuariosList = db.Usuarios.Where(u => u.UsuarioAprobadorPrincipal == true || u.UsuarioId == usuarioElabora.UsuarioPadreId).ToList();
                    }
                    else
                    {
                        usuariosList = db.Usuarios.Where(u => u.UsuarioAprobadorPrincipal == true).ToList();
                    }

                    foreach (var u in usuariosList)
                    {

                        if (u != null && u.UsuarioCorreo != "")
                            mails.Add(new Mails { to = u.UsuarioCorreo, toName = u.UsuarioNombre });

                        //to = u.UsuarioCorreo;
                        //flagSend = Fn.SendEmail(to, subject, mensaje);
                    }//foreach (var u in usuariosAprobadorPrincipal)

                    foreach (var m in mails)
                    {
                        Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, mensaje, seguridadcll.Aplicacion.Link));
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
#endif
        }

        /// <summary>
        /// Crea gastos de la actividad Autorizada solo productos no inventariables
        /// </summary>
        /// <param name="ActividadId"></param>
        /// <returns></returns>
        private bool CreaGastoXActividad(int ActividadId = 0)
        {
            bool result = false;

            int NroGasto = 0;
            var actividad = db.Actividad.Where(a => a.ActividadId == ActividadId).FirstOrDefault();
            //valida que exista la actividad
            if (actividad != null)
            {
                //valida que el estado sea autorizado
                if (actividad.ActividadEstado == EstadosActividades.Autorizado)
                {

                    var GastoTemp = db.Gasto
                                        .Where(g => g.ActividadId == ActividadId && g.GastoEstado != EstadoGasto.Eliminado)
                                        .FirstOrDefault();

                    if (GastoTemp == null)
                    {
                        var actividadItems = db.ActividadItem.Where(ai => ai.ActividadId == ActividadId).ToList();

                        //Obtiene numero del Gasto(ultimo de la tabla + 1)
                        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                        NroGasto = seguridad.generaConsecutivo("Gastos");


                        //Crea El gasto 
                        int Linea = 1;
                        foreach (var item in actividadItems)
                        {
                            var producto = db.Producto.Where(p => p.ProductoId == item.ProductoId).FirstOrDefault();
                            if (producto.TipoProductoID != "1")
                            {
                                Gasto gasto = new Gasto();
                                gasto.GastoId = NroGasto;
                                gasto.ActividadId = actividad.ActividadId;
                                gasto.TipoGastoID = "1";//Corregir (crear campo por default en configuración)
                                gasto.GastoFecha = DateTime.Today;
                                gasto.GastoFechaMod = DateTime.Today;
                                gasto.GastoEstado = EstadoGasto.Abierta;
                                gasto.GastoComentario = actividad.ActividadDesc;
                                gasto.GastoLinea = Linea;
                                gasto.ProductoId = item.ProductoId;
                                gasto.GastoValor = item.ActividadItemPrecio ?? 0;
                                gasto.GastoCant = item.ActividadItemCantidad;
                                gasto.GastoFactura = "Nro Actividad: " + ActividadId;
                                gasto.CentroCostoID = item.CentroCostoID;

                                db.Gasto.Add(gasto);

                                Linea++;
                            }


                        }//foreach (var item in actividadItems)
                        db.SaveChanges();
                        result = true;

                        #region auditoria
                        AddLog("Actividades/CreaGastoXActividad", NroGasto, actividad);
                        //Auditoria auditoria = new Auditoria();
                        //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                        //auditoria.AuditoriaFecha = System.DateTime.Now;
                        //auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                        //auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                        //auditoria.AuditoriaEvento = "Create";
                        //auditoria.AuditoriaDesc = "Se Creo el Gasto: " + NroGasto;
                        //auditoria.ObjetoId = "Actividades/CreaGastoXActividad";

                        //seguridad.insertAuditoria(auditoria);
                        #endregion auditoria

                    }//if (GastoTemp == null)
                }//if (actividad.ActividadEstado == EstadosActividades.Autorizado)
            }//if (actividad != null)

            return result;
        }

        public void AfectaPrespuestoXActividad(int ActividadId = 0)
        {
            var actividad = db.Actividad.Where(a => a.ActividadId == ActividadId &&
                                                    a.ActividadEstado == EstadosActividades.Despachado)
                                                    .FirstOrDefault();
            decimal ValorGasto = 0;
            if (actividad != null)
            {
                //Buscar Prespuesto a Afectar    
                DateTime Date = actividad.ActividadFecha;

                int Year = Date.Year;
                int Month = Date.Month;
                int quartile = 0;

                if (Month >= 1 && Month <= 3)//Q1
                {
                    quartile = 1;
                }
                else if (Month >= 4 && Month <= 6)//Q2
                {
                    quartile = 2;
                }
                else if (Month >= 7 && Month <= 9)//Q3
                {
                    quartile = 3;
                }
                else if (Month >= 10 && Month <= 12)//Q4
                {
                    quartile = 4;
                }//if (Month >= 1 && Month <= 3)

                //Busca registro de gastos (productos no inventariables)
                var gastoList = db.Gasto.Where(g => g.ActividadId == ActividadId && (g.GastoEstado == EstadoGasto.Ejecutado || g.GastoEstado == EstadoGasto.Pagado)).ToList();
                var centroCostosGasto = gastoList.Select(g => g.CentroCostoID).Distinct();

                foreach (var item in centroCostosGasto)
                {

                    var gastoListTemp = gastoList.Where(g => g.CentroCostoID == item).ToList();
                    gastoListTemp.ForEach(g => ValorGasto += (g.GastoValor * g.GastoCant));

                    var prespuesto = db.PresupuestoVendedor.Where(p => p.PlantaID == actividad.PlantaID && p.CanalID == actividad.CanalID &&
                                                                   p.PresupuestoVendedorAno == Year &&
                                                                   p.CentroCostoID == item).FirstOrDefault();

                    if (prespuesto != null)
                    {
                        prespuesto.PresupuestoGasto += ValorGasto;
                        db.Entry(prespuesto).State = EntityState.Modified;
                        db.SaveChanges();
                    }//if (prespuesto != null)

                    ValorGasto = 0;
                }

                //Busca registro de ordenes (Productos Inventariables)
                var orden = db.Orden.Where(o => o.ActividadId == ActividadId && o.OrdenEstado == EstadoOrden.Despachado).FirstOrDefault();

                if (orden != null)
                {
                    var ordenItemsList = db.OrdenItems.Where(oi => oi.OrdenId == orden.OrdenId).ToList();
                    var centroCostosOrden = ordenItemsList.Select(oi => oi.CentroCostoID).Distinct();

                    foreach (var item in centroCostosOrden)
                    {

                        var ordenItemsListTemp = ordenItemsList.Where(oi => oi.CentroCostoID == item).ToList();
                        ordenItemsListTemp.ForEach(oi => ValorGasto += (oi.OrdenItemsVlr * oi.OrdenItemsCantConfirmada));

                        var prespuesto = db.PresupuestoVendedor.Where(p => p.PlantaID == actividad.PlantaID && p.CanalID == actividad.CanalID &&
                                                                       p.PresupuestoVendedorAno == Year &&
                                                                       p.CentroCostoID == item).FirstOrDefault();

                        if (prespuesto != null)
                        {
                            prespuesto.PresupuestoGasto += ValorGasto;
                            db.Entry(prespuesto).State = EntityState.Modified;
                            db.SaveChanges();
                        }//if (prespuesto != null)

                        ValorGasto = 0;
                    }

                }//if (orden != null)

            }//if (actividad != null)
        }



        private class Mails
        {
            public string to { get; set; }
            public string toName { get; set; }
        }

        [AuthorizeAction]
        public async Task<ActionResult> ActividadCierre(int id)
        {
            var actividad = await db.Actividad.Include(a => a.ActividadItemList)
                .Include(a => a.cliente)
                .Where(a => a.ActividadId == id).FirstOrDefaultAsync();


            ViewBag.EstadoCierre = new SelectList(Fn.EnumToIEnumarable<EstadoCierreActividad>(), "Value", "Name");
            ViewBag.MetaCierre = new SelectList(Fn.EnumToIEnumarable<MetaCierreActividad>(), "Value", "Name");

            var archivos = await db.ActividadArchivo
                            .Where(di => di.ActividadId == id)
                            .Select(da => new ActividadCierreViewModel.Archivo { Order = da.Order, FileName = da.FileName })
                            .ToListAsync();

            return PartialView("_ActividadCierre", new ActividadCierreViewModel
            {
                ActividadId = actividad.ActividadId,
                ActividadTitulo = actividad.ActividadTitulo,
                ActividadEstado = actividad.ActividadEstado.ToString(),
                ClienteID = actividad.ClienteID + " - " + actividad.cliente.ClienteRazonSocial,
                ActividadMetaV = actividad.ActividadMetaV ?? 0,
                ActividadMetaE = actividad.ActividadMetaE ?? 0,
                ActividadTotal = actividad.ActividadItemList.Sum(a => (a.ActividadItemPrecio * a.ActividadItemCantidad)) ?? 0,
                Archivos = archivos
            }
            );
        }


        private async Task<bool> UploadFiles(int Id, IEnumerable<HttpPostedFileBase> Files, int porder)
        {


            int order = porder;
            foreach (var file in Files)
            {
                if (file != null && file.ContentLength > 0)
                {

                    ActividadArchivo da = new ActividadArchivo();
                    da.ActividadId = Id;
                    da.Order = order++;
                    da.File = Fn.ConvertToByte(file);
                    da.FileName = file.FileName;
                    da.MediaType = file.ContentType;
                    db.ActividadArchivo.Add(da);

                }

            }


            if (db.ActividadArchivo.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarArchivo(int d, int o)
        {
            var data = await db.ActividadArchivo
                               .Where(da => da.ActividadId == d && da.Order == o)
                               .Select(da => new { da.File, da.MediaType, da.FileName })
                               .FirstOrDefaultAsync();

            if (data == null)
                return HttpNotFound();
            else
            {
                return File(data.File, data.MediaType, data.FileName);
            }
        }

        private async Task<bool> DeleteFiles(int id)
        {
            bool result = true;
            try
            {
                var gi = await db.ActividadArchivo.Where(i => i.ActividadId == id).ToListAsync();
                if (gi.Count > 0)
                {
                    db.ActividadArchivo.RemoveRange(gi);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }

        [HttpPost]
        [AuthorizeAction]
        public async Task<ActionResult> ActividadCierre(ActividadCierreViewModel model, IEnumerable<HttpPostedFileBase> Files)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    var actividad = await db.Actividad.Where(a => a.ActividadId == model.ActividadId).FirstOrDefaultAsync();
                    if (actividad != null)
                    {
                        actividad.ActividadEstado = EstadosActividades.Cerrado;
                        actividad.EstadoCierre = model.EstadoCierre;
                        actividad.CumplimientoPorcentaje = model.CumplimientoPorcentaje;
                        actividad.CumplimientoTotal = model.CumplimientoTotal;
                        actividad.Resultado = model.Resultado;
                        actividad.MetaCierre = model.MetaCierre;
                        db.Entry(actividad).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        //Archivos
                        var currentFiles = await db.ActividadArchivo
                                                   .Where(da => da.ActividadId == model.ActividadId)
                                                   .ToListAsync();
                        //Save Files
                        int order = 1;
                        if (currentFiles.Count > 0)
                            order = currentFiles.Select(i => i.Order).Max() + 1;

                        if (Files != null)
                            await UploadFiles(model.ActividadId, Files, order);

                        AddLog("", model.ActividadId, model);
                        return RedirectToAction("Index", GetReturnSearch());
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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

