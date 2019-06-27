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
using MMS.Classes;
using API = MMS.ApiControllers.WCSS;
using System.IO;
using NPOI.XSSF.UserModel;
using Microsoft.Reporting.WebForms;

namespace MMS.Controllers.WCSS
{
    public class PQRSController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: PQRS
        [AuthorizeAction]
        [FillPermission("Devoluciones/Create", "Garantias/Create", "Novedades/Create", "PQRS/AsignarAnalista", "PQRS/AddComment", "PQRS/AddAnswer", "PQRS/Close", "PQRS/UsuarioStep", "Devoluciones/Edit", "Garantias/Edit", "Novedades/Edit")]
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeAction]
        public async Task<ActionResult> AsignarAnalista(TipoPQRS tipo, int DataId)
        {
            var data = new AsignarAnalistaViewModel();

            data.TipoPQRS = tipo;
            data.DataId = DataId;

            if (tipo == TipoPQRS.Devolucion)
            {
                var devolucion = await db.Devoluciones
                                        .Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (devolucion == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + devolucion.Asunto;
                data.AnalistaId = devolucion.AnalistaId;

            }
            else if (tipo == TipoPQRS.Garantia)
            {
                var garantia = await db.Garantias.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (garantia == null)
                    return HttpNotFound();

                data.Asunto = " Subject: " + garantia.Asunto;
                data.AnalistaId = garantia.AnalistaId;
            }
            else if (tipo == TipoPQRS.Novedad)
            {
                var novedad = await db.Novedad.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (novedad == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + novedad.Asunto;
                data.AnalistaId = novedad.AnalistaId;
            }
            else if (tipo == TipoPQRS.Recruitment)
            {
                var recruitment = await db.Recruitments.Where(d => d.RecruitmentId == DataId).FirstOrDefaultAsync();
                if (recruitment == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + recruitment.Appointment;
                data.AnalistaId = recruitment.AnalistaId;
            }
            else { return HttpNotFound(); }

            return PartialView("_AsignarAnalista", data);
        }

        [AuthorizeAction]
        [FillPermission("PQRS/AsignarAnalista", "PQRS/AddComment", "PQRS/AddAnswer", "PQRS/Close", "PQRS/UsuarioStep")]
        public async Task<ActionResult> Details(TipoPQRS TipoPQRS, int DataId, int RecordId)
        {
            var data = new PQRSTimeLineViewModel();
            if (TipoPQRS == TipoPQRS.Devolucion)
            {
                data.formato = await db.Devoluciones
                                    .Include(d => d.UsuarioCreacion)
                                    .Include(d => d.Analista)
                                    .Where(d => d.Id == DataId)
                                    .Select(d => new PQRSTimeLineViewModel.Formato
                                    {
                                        TipoPQRS = TipoPQRS,
                                        Id = d.Id,
                                        FechaCreacion = d.FechaCreacion,
                                        UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                        Estado = d.Estado,
                                        Prioridad = Prioridad.Baja,
                                        TipoPersona = TipoPersona.Otro,
                                        Persona = "",
                                        ClienteId = d.ClienteId,
                                        AnalistaId = d.Analista.UsuarioNombre,
                                        Asunto = d.Asunto,
                                        Observacion = d.Observacion,
                                        Destinatarios = d.Destinatarios,
                                        NroTracking = d.NroTracking
                                    })
                                    .FirstOrDefaultAsync();
                data.formatoArchivos = await db.DevolucionArchivos
                                             .Where(di => di.DevolucionId == DataId)
                                             .Select(da => new PQRSTimeLineViewModel.Comentarios.Archivos { Item = da.Order, FileName = da.FileName })
                                             .ToListAsync();
                data.formatoItems = await db.DevolucionItems
                                            .Include(di => di.MotivoPQRS)
                                            .Include(di => di.Items)
                                            .Where(di => di.DevolucionId == DataId && di.PQRSRecordId == RecordId)
                                            .Select(di => new PQRSTimeLineViewModel.FormatoItems
                                            {
                                                Id = di.Id,
                                                ItemId = di.ItemId,
                                                Cantidad = di.Cantidad,
                                                Precio = di.Precio,
                                                NroFactura = di.NroFactura,
                                                NroGuia = di.NroGuia,
                                                MotivoPQRSId = di.MotivoPQRSId,
                                                ComentarioAdicional = di.ComentarioAdicional,
                                                Estado = di.Estado,
                                                CantidadRecibida = di.CantidadRecibida,
                                                CantidadSubida = di.CantidadSubida,
                                                ComentarioEstadoMercancia = di.ComentarioEstadoMercancia,
                                                DocSoporte = di.DocSoporte,
                                                MotivoPQRS = di.MotivoPQRS,
                                                Items = di.Items
                                            })
                                            .ToListAsync();

            }
            else if (TipoPQRS == TipoPQRS.Garantia)
            {
                data.formato = await db.Garantias
                                        .Include(d => d.UsuarioCreacion)
                                        .Include(d => d.Analista)
                                        .Where(d => d.Id == DataId)
                                        .Select(d => new PQRSTimeLineViewModel.Formato
                                        {
                                            TipoPQRS = TipoPQRS,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = TipoPersona.Otro,
                                            Persona = "",
                                            ClienteId = d.ClienteId,
                                            AnalistaId = d.Analista.UsuarioNombre,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = d.Destinatarios,
                                            NroTracking = d.NroTracking
                                        }).FirstOrDefaultAsync();
                data.formatoArchivos = await db.GarantiaArchivos
                                           .Where(di => di.GarantiaId == DataId)
                                           .Select(da => new PQRSTimeLineViewModel.Comentarios.Archivos { Item = da.Order, FileName = da.FileName })
                                           .ToListAsync();
                data.formatoItems = await db.GarantiaItems
                                            .Include(di => di.MotivoPQRS)
                                            .Include(di => di.Items)
                                            .Where(di => di.GarantiaId == DataId && di.PQRSRecordId == RecordId)
                                            .Select(di => new PQRSTimeLineViewModel.FormatoItems
                                            {
                                                Id = di.Id,
                                                ItemId = di.ItemId,
                                                Cantidad = di.Cantidad,
                                                Precio = di.Precio,
                                                NroFactura = di.NroFactura,
                                                NroGuia = di.NroGuia,
                                                MotivoPQRSId = di.MotivoPQRSId,
                                                ComentarioAdicional = di.ComentarioAdicional,
                                                Estado = di.Estado,
                                                CantidadRecibida = di.CantidadRecibida,
                                                CantidadSubida = di.CantidadSubida,
                                                ComentarioEstadoMercancia = di.ComentarioEstadoMercancia,
                                                DocSoporte = di.DocSoporte,
                                                MotivoPQRS = di.MotivoPQRS,
                                                Items = di.Items
                                            })
                                            .ToListAsync();
            }
            else if (TipoPQRS == TipoPQRS.Novedad)
            {
                data.formato = await db.Novedad
                                        .Include(d => d.UsuarioCreacion)
                                        .Include(d => d.Analista)
                                        .Where(d => d.Id == DataId)
                                        .Select(d => new PQRSTimeLineViewModel.Formato
                                        {
                                            TipoPQRS = TipoPQRS,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = d.TipoPersona,
                                            Persona = d.Persona,
                                            ClienteId = d.ClienteId,
                                            AnalistaId = d.Analista.UsuarioNombre,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = d.Destinatarios,
                                            NroTracking = d.NroTracking
                                        }).FirstOrDefaultAsync();
                data.formatoArchivos = await db.NovedadArchivo
                                         .Where(di => di.NovedadId == DataId)
                                         .Select(da => new PQRSTimeLineViewModel.Comentarios.Archivos { Item = da.Order, FileName = da.FileName })
                                         .ToListAsync();
                data.formatoItems = await db.NovedadItem
                                            .Include(di => di.MotivoPQRS)
                                            .Include(di => di.Items)
                                            .Where(di => di.NovedadId == DataId && di.PQRSRecordId == RecordId)
                                            .Select(di => new PQRSTimeLineViewModel.FormatoItems
                                            {
                                                Id = di.Id,
                                                ItemId = di.ItemId,
                                                Cantidad = di.Cantidad,
                                                Precio = di.Precio,
                                                NroFactura = di.NroFactura,
                                                NroGuia = di.NroGuia,
                                                MotivoPQRSId = di.MotivoPQRSId,
                                                ComentarioAdicional = di.ComentarioAdicional,
                                                Estado = di.Estado,
                                                CantidadRecibida = di.CantidadRecibida,
                                                CantidadSubida = di.CantidadSubida,
                                                ComentarioEstadoMercancia = di.ComentarioEstadoMercancia,
                                                DocSoporte = di.DocSoporte,
                                                MotivoPQRS = di.MotivoPQRS,
                                                Items = di.Items
                                            })
                                            .ToListAsync();
            }
            //Lista de pasos.
            var PQRSRecords = await db.PQRSRecords
                                      .Include(p => p.PQRSRecordUsuarios)
                                      .Where(pr => pr.Id == RecordId).ToListAsync();
            data.PQRSRecords = PQRSRecords;



            //Lista de comentarios
            var PQRSRecordComentarios = await db.PQRSRecordComentarios
                                                .Include(pc => pc.Usuario)
                                                .Where(pc => pc.PQRSRecordId == RecordId).ToListAsync();
            List<PQRSTimeLineViewModel.Comentarios> Comentarios = new List<PQRSTimeLineViewModel.Comentarios>();
            foreach (var c in PQRSRecordComentarios)
            {
                PQRSTimeLineViewModel.Comentarios comentario = new PQRSTimeLineViewModel.Comentarios();
                comentario.PQRSRecordComentarios = c;
                comentario.PQRSRecordDocumentos = await db.PQRSRecordDocumentos
                                                            .Include(pd => pd.TipoDocSoporte)
                                                            .Where(pd => pd.PQRSRecordComentarioId == c.Id).ToListAsync();
                comentario.PQRSRecordArchivos = await db.PQRSRecordArchivos
                                                        .Where(pa => pa.PQRSRecordComentarioId == c.Id)
                                                        .Select(pa => new PQRSTimeLineViewModel.Comentarios.Archivos { Id = pa.PQRSRecordComentarioId, Item = pa.Item, FileName = pa.FileName }).ToListAsync();
                Comentarios.Add(comentario);
            }
            data.PQRSRecordComentarios = Comentarios;

            return View(data);
        }



        [AuthorizeAction]
        public async Task<ActionResult> AddComment(TipoPQRS TipoPQRS, int DataId, int RecordId)
        {


            var data = new AddCommentViewModel();

            data.TipoPQRS = TipoPQRS;
            data.PQRSRecordId = RecordId;

            if (TipoPQRS == TipoPQRS.Devolucion)
            {
                var devolucion = await db.Devoluciones
                                        .Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (devolucion == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + devolucion.Asunto;
                //data.AnalistaId = devolucion.AnalistaId;

            }
            else if (TipoPQRS == TipoPQRS.Garantia)
            {
                var garantia = await db.Garantias.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (garantia == null)
                    return HttpNotFound();

                data.Asunto = " Subject: " + garantia.Asunto;
                // data.AnalistaId = garantia.AnalistaId;
            }
            else if (TipoPQRS == TipoPQRS.Novedad)
            {
                var novedad = await db.Novedad.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (novedad == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + novedad.Asunto;
                // data.AnalistaId = novedad.AnalistaId;
            }
            else if (TipoPQRS == TipoPQRS.Recruitment)
            {
                var recruitment = await db.Recruitments.Where(d => d.RecruitmentId == DataId).FirstOrDefaultAsync();
                if (recruitment == null)
                    return HttpNotFound();


                data.Asunto = " Subject: " + recruitment.Appointment;
                // data.AnalistaId = novedad.AnalistaId;
            }
            else { return HttpNotFound(); }

            ViewBag.TipoDocSoporteId = await db.TipoDocSoporte.ToListAsync();

            return PartialView("_AddComment", data);
        }

        [HttpPost]
        [ValidateInput(false)]
        [AuthorizeAction]
        public async Task<JsonResult> AddComment(AddCommentViewModel model)
        {

            if (ModelState.IsValid)
            {
                // Crear Comment (primer paso Success)
                PQRSRecordComentario nPQRSComment = new PQRSRecordComentario();
                nPQRSComment.PQRSRecordId = model.PQRSRecordId;
                nPQRSComment.PQRSRecordOrder = model.PQRSRecordOrder; ;
                nPQRSComment.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                nPQRSComment.FechaCreacion = DateTime.Now;
                nPQRSComment.Comentario = model.Comment;
                nPQRSComment.TipoComentario = TipoComentario.Comment;
                db.PQRSRecordComentarios.Add(nPQRSComment);
                await db.SaveChangesAsync();
                AddLog("PQRS/AddComment", nPQRSComment.Id, nPQRSComment);
                if (model.PQRSRecordDocumentos != null)
                    await SaveDocuments(nPQRSComment.Id, model.PQRSRecordDocumentos);
                if (model.Files != null)
                    await UploadFiles(nPQRSComment.Id, model.Files);

                API.PQRSController ApiPQRS = new API.PQRSController();
                ApiPQRS.SendNotificationEmailTask(API.PQRSController.TipoNotificacion.Comment, model.TipoComentario, model.TipoPQRS, model.PQRSRecordId, model.PQRSRecordOrder, model.DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                //await Task.Run(() => ApiPQRS.SendNotificationEmailTask(API.PQRSController.TipoNotificacion.Comment, model.TipoComentario, model.TipoPQRS, model.PQRSRecordId, model.PQRSRecordOrder, model.DataId, nPQRSComment.Id));
            }


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeAction]
        public async Task<ActionResult> AddAnswer(TipoPQRS TipoPQRS, int DataId, int RecordId, int Order, TipoComentario TipoComentario)
        {


            //validar si ya dio respuesta en ese paso



            var data = new AddCommentViewModel();

            data.TipoPQRS = TipoPQRS;
            data.PQRSRecordId = RecordId;
            data.PQRSRecordOrder = Order;
            data.TipoComentario = TipoComentario;
            data.DataId = DataId;
            var PQRSRecord = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.Order == Order).FirstOrDefaultAsync();
            data.FlujoPQRSDescripcion = PQRSRecord.FlujoPQRSDescripcion;
            if (TipoPQRS == TipoPQRS.Devolucion)
            {
                var devolucion = await db.Devoluciones
                                        .Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (devolucion == null)
                    return HttpNotFound();


                data.Asunto = $"Step {PQRSRecord.Order}: " + PQRSRecord.FlujoPQRSNombre + " Reason: " + PQRSRecord.MotivoPQRSNombre + " Subject: " + devolucion.Asunto;
                //data.AnalistaId = devolucion.AnalistaId;

            }
            else if (TipoPQRS == TipoPQRS.Garantia)
            {
                var garantia = await db.Garantias.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (garantia == null)
                    return HttpNotFound();

                data.Asunto = $"Step {PQRSRecord.Order}:  " + PQRSRecord.FlujoPQRSNombre + " Reason: " + PQRSRecord.MotivoPQRSNombre + " Subject: " + garantia.Asunto;
                // data.AnalistaId = garantia.AnalistaId;
            }
            else if (TipoPQRS == TipoPQRS.Novedad)
            {
                var novedad = await db.Novedad.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                if (novedad == null)
                    return HttpNotFound();


                data.Asunto = $"Step {PQRSRecord.Order}:  " + PQRSRecord.FlujoPQRSNombre + " Reason: " + PQRSRecord.MotivoPQRSNombre + " Subject: " + novedad.Asunto;
                // data.AnalistaId = novedad.AnalistaId;
            }
            else if (TipoPQRS == TipoPQRS.Recruitment)
            {
                var recruitment = await db.Recruitments.Where(d => d.RecruitmentId == DataId).FirstOrDefaultAsync();
                if (recruitment == null)
                    return HttpNotFound();


                data.Asunto = $"Step {PQRSRecord.Order}:  " + PQRSRecord.FlujoPQRSNombre + " Reason: " + PQRSRecord.MotivoPQRSNombre + " Subject: " + recruitment.Appointment;
                // data.AnalistaId = novedad.AnalistaId;
            }
            else { return HttpNotFound(); }


            var Tareas = await db.PQRSRecordTareas.Where(pu => pu.PQRSRecordId == RecordId && pu.PQRSRecordOrder == Order).ToListAsync();
            data.Tareas = Tareas;
            //Condiciones
            int Idx = Order + 1;
            bool flagCondiciones = true;
            var CondicionesViewList = new List<AddCommentViewModel.CondicionesView>();
            while (flagCondiciones)
            {
                var Condiciones = await db.PQRSRecordCondiciones
                                          .Include(pc => pc.PQRSRecord)
                                          .Where(pu => pu.PQRSRecordId == RecordId && pu.PQRSRecordOrder == Idx).ToListAsync();

                if (Condiciones.Count > 0)
                {
                    var CondicionesView = new AddCommentViewModel.CondicionesView();
                    CondicionesView.Condiciones = Condiciones;
                    CondicionesView.PQRSRecord = Condiciones.FirstOrDefault().PQRSRecord;
                    CondicionesViewList.Add(CondicionesView);

                    Idx++;
                }
                else
                {
                    flagCondiciones = false;
                }
            }

            data.Condiciones = CondicionesViewList;


            ViewBag.TipoDocSoporteId = await db.TipoDocSoporte.ToListAsync();

            return PartialView("_AddAnswer", data);
        }

        [HttpPost]
        [ValidateInput(false)]
        [AuthorizeAction]
        public async Task<JsonResult> AddAnswer(AddCommentViewModel model)
        {
            var result = new AjaxResult();

            if (ModelState.IsValid)
            {

                #region ValidaCheckList
                //var tareas = await db.PQRSRecordTareas.Where(pt => pt.PQRSRecordId == model.PQRSRecordId && pt.PQRSRecordOrder == model.PQRSRecordOrder).ToListAsync();

                if (model.TipoComentario == TipoComentario.Approval)
                {
                    if (model.Tareas == null)
                        model.Tareas = new List<PQRSRecordTareas>();

                    bool flagTareas = true;
                    string msgTareas = "";
                    foreach (var t in model.Tareas)
                    {
                        if (t.Requerido && !t.Terminado)
                        {
                            flagTareas = false;
                            msgTareas += $" Task: '{t.Descripcion}' is required to advance" + System.Environment.NewLine;
                        }
                    }

                    if (!flagTareas)
                    {
                        return Json(result.False(msgTareas), JsonRequestBehavior.AllowGet);
                    }

                }
                #endregion

                // Crear Comment (primer paso Success)
                PQRSRecordComentario nPQRSComment = new PQRSRecordComentario();
                nPQRSComment.PQRSRecordId = model.PQRSRecordId;
                nPQRSComment.PQRSRecordOrder = model.PQRSRecordOrder;
                nPQRSComment.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                nPQRSComment.FechaCreacion = DateTime.Now;
                nPQRSComment.Comentario = model.Comment;
                nPQRSComment.TipoComentario = model.TipoComentario;
                db.PQRSRecordComentarios.Add(nPQRSComment);
                await db.SaveChangesAsync();
                AddLog("PQRS/AddAnswer", nPQRSComment.Id, nPQRSComment);
                if (model.PQRSRecordDocumentos != null)
                    await SaveDocuments(nPQRSComment.Id, model.PQRSRecordDocumentos);
                if (model.Files != null)
                    await UploadFiles(nPQRSComment.Id, model.Files);

                if (model.Tareas != null)
                    await SaveCheckList(model.Tareas);
                if (model.Condiciones != null)
                {
                    foreach (var c in model.Condiciones)
                    {
                        await SaveConditions(c.Condiciones);
                    }
                }


                //Users in step
                var usuarioStep = await db.PQRSRecordUsuarios
                                          .Where(pu => pu.PQRSRecordId == model.PQRSRecordId && pu.PQRSRecordOrder == model.PQRSRecordOrder && pu.UsuarioId == Seguridadcll.Usuario.UsuarioId)
                                          .FirstOrDefaultAsync();
                if (usuarioStep != null)
                {
                    if (model.TipoComentario == TipoComentario.Approval)
                    {
                        //usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Approved;
                        usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Done;
                    }
                    else if (model.TipoComentario == TipoComentario.Rejection)
                        //usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Rejected;
                        usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Returned;
                    else if (model.TipoComentario == TipoComentario.Close)
                        usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Closed;
                    db.Entry(usuarioStep).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                }

                API.PQRSController ApiPQRS = new API.PQRSController();
                if (model.TipoComentario == TipoComentario.Close)
                {
                    await ApiPQRS.CloseFlow(model.TipoComentario, model.PQRSRecordId, model.PQRSRecordOrder);
                    //Notificación Close Flow
                    ApiPQRS.SendNotificationEmailTask(API.PQRSController.TipoNotificacion.Close, model.TipoComentario, model.TipoPQRS, model.PQRSRecordId, model.PQRSRecordOrder, model.DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                }
                else
                {
                    await ApiPQRS.ChangeStep(model.TipoComentario, model.PQRSRecordId, model.PQRSRecordOrder, model.DataId, model.TipoPQRS);
                    //Notificación Current Step
                    ApiPQRS.SendNotificationEmailTask(API.PQRSController.TipoNotificacion.CurrentStep, model.TipoComentario, model.TipoPQRS, model.PQRSRecordId, model.PQRSRecordOrder, model.DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                }

                await ApiPQRS.CompleteFormat(model.TipoPQRS, model.DataId);


            }
            else
            {
                return Json(result.False("All fields are required."), JsonRequestBehavior.AllowGet);
            }


            return Json(result.True(), JsonRequestBehavior.AllowGet);
        }

        private async Task<bool> SaveDocuments(int Id, IEnumerable<PQRSRecordDocumento> Documents)
        {
            bool result = true;
            try
            {

                int item = 1;
                foreach (var d in Documents)
                {
                    if (d.NroDocumento != "" && d.NroDocumento != null)
                    {
                        PQRSRecordDocumento pd = new PQRSRecordDocumento();
                        pd.PQRSRecordComentarioId = Id;
                        pd.Item = item++;
                        pd.NroDocumento = d.NroDocumento;
                        pd.TipoDocSoporteId = d.TipoDocSoporteId;
                        db.PQRSRecordDocumentos.Add(pd);
                    }

                }

                if (db.PQRSRecordDocumentos.Local.Count > 0)
                    await db.SaveChangesAsync();
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private async Task<bool> SaveCheckList(IEnumerable<PQRSRecordTareas> Tareas)
        {
            var result = true;
            try
            {


                foreach (var t in Tareas)
                {
                    db.Entry(t).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("AddAnswer", t.Id, t);
                }

            }
            catch
            {
                result = false;
            }

            return result;


        }


        private async Task<bool> SaveConditions(IEnumerable<PQRSRecordCondiciones> Condiciones)
        {
            var result = true;
            try
            {
                foreach (var c in Condiciones)
                {
                    db.Entry(c).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("AddAnswer", c.Id, c);
                }

            }
            catch
            {
                result = false;
            }

            return result;


        }


        private async Task<bool> UploadFiles(int Id, IEnumerable<HttpPostedFileBase> Files)
        {


            int item = 1;
            foreach (var file in Files)
            {
                if (file != null && file.ContentLength > 0)
                {

                    PQRSRecordArchivo pa = new PQRSRecordArchivo();
                    pa.PQRSRecordComentarioId = Id;
                    pa.Item = item++;
                    pa.File = Fn.ConvertToByte(file);
                    pa.FileName = file.FileName;
                    pa.MediaType = file.ContentType;
                    db.PQRSRecordArchivos.Add(pa);
                }

            }


            if (db.PQRSRecordArchivos.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> DescargarArchivo(int c, int i)
        {
            var data = await db.PQRSRecordArchivos
                               .Where(pa => pa.PQRSRecordComentarioId == c && pa.Item == i)
                               .Select(pa => new { pa.File, pa.MediaType, pa.FileName })
                               .FirstOrDefaultAsync();

            if (data == null)
                return HttpNotFound();
            else
            {


                return File(data.File, data.MediaType, data.FileName);
            }

        }


        [AuthorizeAction]
        [FillPermission]
        public async Task<ActionResult> UsuarioStep(int RecordId, int Order)
        {

            var PQRSRecordUsuarios = new List<PQRSRecordUsuario>();

            PQRSRecordUsuarios = await db.PQRSRecordUsuarios
                           .Include(pu => pu.PQRSRecord)
                           .Include(pu => pu.Usuario)
                           .Where(pu => pu.PQRSRecordId == RecordId && pu.PQRSRecordOrder == Order)
                           .ToListAsync();

            var Tareas = await db.PQRSRecordTareas.Where(pu => pu.PQRSRecordId == RecordId && pu.PQRSRecordOrder == Order).ToListAsync();
            var Condiciones = await db.PQRSRecordCondiciones.Where(pu => pu.PQRSRecordId == RecordId && pu.PQRSRecordOrder == Order).ToListAsync();

            return PartialView("_UsuarioStep", new PQRSUsuarioStepViewModel { Usuarios = PQRSRecordUsuarios, Tareas = Tareas, Condiciones = Condiciones });
        }

        public async Task<ActionResult> ReportTimeLine(string archivo, TipoPQRS TipoPQRS, int DataId, int RecordId)
        {




            LocalReport lr = new LocalReport();
            string path = Path.Combine(Server.MapPath("~/Reports"), "WCSS/PQRSTimeLine.rdlc");
            if (System.IO.File.Exists(path))
                lr.ReportPath = path;
            else
                return View("Index");
            //var Actividads = new List<dynamic>();
            dynamic formato;//= new List<PQRSTimeLineViewModel.Formato>();
            dynamic formatoItems;
            if (TipoPQRS == TipoPQRS.Devolucion)
            {
                formato = await db.Devoluciones
                                    .Include(d => d.UsuarioCreacion)
                                    .Include(d => d.Analista)
                                    .Include(d => d.Cliente.ciudad.departamentos.paises)
                                    .Include(d => d.Cliente.canal)
                                    .Include(d => d.Cliente.usuario)
                                    .Where(d => d.Id == DataId)
                                    .Select(d => new
                                    {
                                        TipoPQRS = "Return",
                                        Id = d.Id,
                                        FechaCreacion = d.FechaCreacion,
                                        UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                        Estado = d.Estado,
                                        Prioridad = Prioridad.Baja,
                                        TipoPersona = TipoPersona.Otro,
                                        Persona = "",
                                        ClienteId = d.ClienteId,
                                        ClienteRazonSocial = d.Cliente.ClienteRazonSocial,
                                        Zona = d.Cliente.ciudad.departamentos.paises.PaisDesc + "-" + d.Cliente.ciudad.departamentos.DepartamentoDesc + "-" + d.Cliente.ciudad.CiudadDesc,
                                        Canal = d.Cliente.canal.CanalDesc,
                                        Vendedor = d.Cliente.VendedorId + "-" + d.Cliente.usuario.UsuarioNombre,
                                        AnalistaId = d.Analista.UsuarioNombre,
                                        Asunto = d.Asunto,
                                        Observacion = d.Observacion,
                                        Destinatarios = d.Destinatarios,
                                        NroTracking = d.NroTracking
                                    })
                                    .ToListAsync();

                formatoItems = await db.DevolucionItems
                                            .Include(di => di.MotivoPQRS)
                                            .Include(di => di.Items)
                                            .Where(di => di.DevolucionId == DataId && di.PQRSRecordId == RecordId)
                                            .Select(di => new
                                            {
                                                Producto = di.Items.Codigo + "-" + di.Items.Descripcion,
                                                Cantidad = di.Cantidad,
                                                Precio = di.Precio,
                                                NroFactura = di.NroFactura,
                                                NroGuia = di.NroGuia,
                                                MotivoPQRSId = di.MotivoPQRS.Nombre,
                                                Estado = di.Estado,
                                                CantidadRecibida = di.CantidadRecibida,
                                                CantidadSubida = di.CantidadSubida,
                                                DocSoporte = di.DocSoporte,
                                                MotivoPQRS = di.MotivoPQRS,
                                                Items = di.Items
                                            })
                                            .ToListAsync();

            }
            else if (TipoPQRS == TipoPQRS.Garantia)
            {
                formato = await db.Garantias
                                        .Include(d => d.UsuarioCreacion)
                                        .Include(d => d.Analista)
                                        .Where(d => d.Id == DataId)
                                        .Select(d => new PQRSTimeLineViewModel.Formato
                                        {
                                            TipoPQRS = TipoPQRS,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = TipoPersona.Otro,
                                            Persona = "",
                                            ClienteId = d.ClienteId,
                                            AnalistaId = d.Analista.UsuarioNombre,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = d.Destinatarios,
                                            NroTracking = d.NroTracking
                                        }).ToListAsync();

                //data.formatoItems = await db.GarantiaItems
                //                            .Include(di => di.MotivoPQRS)
                //                            .Include(di => di.Items)
                //                            .Where(di => di.GarantiaId == DataId && di.PQRSRecordId == RecordId)
                //                            .Select(di => new PQRSTimeLineViewModel.FormatoItems
                //                            {
                //                                Id = di.Id,
                //                                ItemId = di.ItemId,
                //                                Cantidad = di.Cantidad,
                //                                Precio = di.Precio,
                //                                NroFactura = di.NroFactura,
                //                                NroGuia = di.NroGuia,
                //                                MotivoPQRSId = di.MotivoPQRSId,
                //                                ComentarioAdicional = di.ComentarioAdicional,
                //                                Estado = di.Estado,
                //                                CantidadRecibida = di.CantidadRecibida,
                //                                CantidadSubida = di.CantidadSubida,
                //                                ComentarioEstadoMercancia = di.ComentarioEstadoMercancia,
                //                                DocSoporte = di.DocSoporte,
                //                                MotivoPQRS = di.MotivoPQRS,
                //                                Items = di.Items
                //                            })
                //                            .ToListAsync();
                formatoItems = new List<PQRSTimeLineViewModel.FormatoItems>();
            }
            else if (TipoPQRS == TipoPQRS.Novedad)
            {
                formato = await db.Novedad
                                        .Include(d => d.UsuarioCreacion)
                                        .Include(d => d.Analista)
                                        .Where(d => d.Id == DataId)
                                        .Select(d => new PQRSTimeLineViewModel.Formato
                                        {
                                            TipoPQRS = TipoPQRS,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = d.TipoPersona,
                                            Persona = d.Persona,
                                            ClienteId = d.ClienteId,
                                            AnalistaId = d.Analista.UsuarioNombre,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = d.Destinatarios,
                                            NroTracking = d.NroTracking
                                        }).ToListAsync();

                //data.formatoItems = await db.NovedadItem
                //                            .Include(di => di.MotivoPQRS)
                //                            .Include(di => di.Items)
                //                            .Where(di => di.NovedadId == DataId && di.PQRSRecordId == RecordId)
                //                            .Select(di => new PQRSTimeLineViewModel.FormatoItems
                //                            {
                //                                Id = di.Id,
                //                                ItemId = di.ItemId,
                //                                Cantidad = di.Cantidad,
                //                                Precio = di.Precio,
                //                                NroFactura = di.NroFactura,
                //                                NroGuia = di.NroGuia,
                //                                MotivoPQRSId = di.MotivoPQRSId,
                //                                ComentarioAdicional = di.ComentarioAdicional,
                //                                Estado = di.Estado,
                //                                CantidadRecibida = di.CantidadRecibida,
                //                                CantidadSubida = di.CantidadSubida,
                //                                ComentarioEstadoMercancia = di.ComentarioEstadoMercancia,
                //                                DocSoporte = di.DocSoporte,
                //                                MotivoPQRS = di.MotivoPQRS,
                //                                Items = di.Items
                //                            })
                //                            .ToListAsync();
                formatoItems = new List<PQRSTimeLineViewModel.FormatoItems>();
            }
            else
            {
                formato = new List<PQRSTimeLineViewModel.Formato>();
                formatoItems = new List<PQRSTimeLineViewModel.FormatoItems>();
            }

            var Comentarios = await db.Database.SqlQuery<ComentariosRptTimeLine>($"SELECT Titulo = isnull('Step: ' + p.FlujoPQRSNombre,'General Comment'), " +
                                                                    "Fecha = c.FechaCreacion, UsuarioId = U.UsuarioId + '-' + U.UsuarioNombre, " +
                                                                    "c.Comentario, " +
                                                                    "CASE c.TipoComentario " +
                                                                    "    WHEN 1 THEN '#4CAF50' " +
                                                                    "    WHEN 2 THEN '#F44336' " +
                                                                    "    WHEN 3 THEN '#9E9E9E' " +
                                                                    "    WHEN 4 THEN '#FFC107' " +
                                                                    "    ELSE '#9E9E9E' " +
                                                                    "END AS Color, " +
                                                                    "CASE c.TipoComentario " +
                                                                    "    WHEN 1 THEN 'Done' " +
                                                                    "    WHEN 2 THEN 'Returned' " +
                                                                    "    WHEN 3 THEN 'Comment' " +
                                                                    "    WHEN 4 THEN 'Closed' " +
                                                                    "    ELSE '' " +
                                                                    "END AS Tipo " +
                                                                    "FROM PQRSRecordComentario AS C " +
                                                                    "    INNER JOIN Usuario AS U ON C.UsuarioId = U.UsuarioId " +
                                                                    "    LEFT JOIN PQRSRecord AS P ON C.PQRSRecordId = P.Id AND C.PQRSRecordOrder = P.[Order] " +
                                                                    $"WHERE PQRSRecordId = {RecordId} ORDER BY FechaCreacion").ToListAsync();


            ReportDataSource formatoDS = new ReportDataSource("Formato", formato);
            ReportDataSource formatoItemsDS = new ReportDataSource("FormatoItems", formatoItems);
            ReportDataSource ComentariosDS = new ReportDataSource("Comentarios", Comentarios);
            lr.DataSources.Add(formatoDS);
            lr.DataSources.Add(formatoItemsDS);
            lr.DataSources.Add(ComentariosDS);

            string reportType = archivo;
            string mimeType;
            string encoding;
            string fileNameExtension;
            //string deviceInfo =

            //     "<DeviceInfo>" +
            //            "  <OutputFormat>" + archivo + "</OutputFormat>" +
            //            "  <PageWidth>8.5in</PageWidth>" +
            //            "  <PageHeight>11in</PageHeight>" +
            //            "  <MarginTop>0.5in</MarginTop>" +
            //            "  <MarginLeft>1in</MarginLeft>" +
            //            "  <MarginRight>1in</MarginRight>" +
            //            "  <MarginBottom>0.5in</MarginBottom>" +
            //            "</DeviceInfo>";
            string deviceInfo =
                 "<DeviceInfo>" +
                        "  <OutputFormat>" + archivo + "</OutputFormat>" +
                        "  <PageWidth>8.5in</PageWidth>" +
                        "  <PageHeight>11in</PageHeight>" +
                        "  <MarginTop>0.5in</MarginTop>" +
                        "  <MarginLeft>1in</MarginLeft>" +
                        "  <MarginRight>0.5in</MarginRight>" +
                        "  <MarginBottom>0in</MarginBottom>" +
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


        public class ComentariosRptTimeLine
        {
            public string Titulo { get; set; }
            public DateTime Fecha { get; set; }
            public string UsuarioId { get; set; }
            public string Comentario { get; set; }
            public string Color { get; set; }
            public string Tipo { get; set; }
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