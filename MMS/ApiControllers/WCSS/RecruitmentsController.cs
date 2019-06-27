using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MMS.Models;
using MMS.Filters;
using System.Net.Http.Formatting;
using MMS.Classes;
using System.Threading.Tasks;

namespace MMS.ApiControllers.WCSS
{
    public class RecruitmentsController : ApiBaseController
    {


        private MMSContext db = new MMSContext();

        [HttpPost]
        [ApiAuthorizeAction]
        public IHttpActionResult Index(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];

                string id = form["_id"];
                string type = form["_type"];
                string appointment = form["_appointment"];
                string department = form["_department"];
                string centroCosto = form["_centroCosto"];
                string startDate = form["_startDate"];
                string estado = form["_estado"];
               
                string sqlWhere = "";

                if (id.Trim() != "")
                    sqlWhere += "RecruitmentId = " + id + " AND ";

                if (type.Trim() != "")
                    sqlWhere += "Type LIKE '%" + type + "%' AND ";

                if (appointment.Trim() != "")
                    sqlWhere += "Appointment LIKE '%" + appointment + "%' AND ";

                if (department.Trim() != "")
                    sqlWhere += "DepartmentId LIKE '%" + department + "%' AND ";

                if (centroCosto.Trim() != "")
                    sqlWhere += "CentroCostoID LIKE '%" + centroCosto + "%' AND ";

                if (startDate.Trim() != "")
                    sqlWhere += "StartDate LIKE '%" + startDate + "%' AND ";

                if (estado.Trim() != "")
                    sqlWhere += "Estado  LIKE '%" + estado + "%' AND ";

                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");

                string sqlCount =
                    "SELECT COUNT(Id) FROM ( " +
                    "SELECT D.RecruitmentId As Id, D.Type, D.Appointment, A.Nombre AS Department, C.CentroCostoDesc AS CentroCosto, " +
                    "    CONVERT(nvarchar, D.StartDate, 103) +' ' + CONVERT(nvarchar, D.StartDate, 108) AS StartDate," +
                    "   case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Recruitment AS D " +
                    "INNER JOIN Area AS A ON D.DepartmentId = A.Id " +
                    "LEFT JOIN CentroCosto AS C ON D.CentroCostoID = C.CentroCostoID " +
                    ") as T" +
                   sqlWhere;

                string sqlData =
                    "SELECT * FROM (" +
                    "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS Seq, * FROM ( " +
                    "SELECT D.RecruitmentId As Id, D.Type, D.Appointment, A.Nombre AS Department, C.CentroCostoDesc AS CentroCosto, " +
                    "    CONVERT(nvarchar, D.StartDate, 103) +' ' + CONVERT(nvarchar, D.StartDate, 108) AS StartDate," +
                    "   case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Recruitment AS D " +
                    "INNER JOIN Area AS A ON D.DepartmentId = A.Id " +
                    "LEFT JOIN CentroCosto AS C ON D.CentroCostoID = C.CentroCostoID " +
                    ") as T" +
                    sqlWhere +
                    $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";

                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data
                });

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // return InternalServerError("");
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public IHttpActionResult Panel(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];

                //string tipopqrs = form["_tipopqrs"];
                string nrotracking = form["_nrotracking"];
                string pqrsrecordid = form["_pqrsrecordid"];
                string motivopqrsnombre = form["_motivopqrsnombre"];
                string asunto = form["_asunto"];
                //string persona = form["_persona"];
                string cliente = form["_cliente"];
                string prioridad = form["_prioridad"];
                string analista = form["_analista"];
                string fechacreacion = form["_fechacreacion"];
                string estado = form["_estado"];


                string sqlWhere = "";


                if (nrotracking.Trim() != "")
                    sqlWhere += "NroTracking LIKE '%" + nrotracking + "%' AND ";

                if (asunto.Trim() != "")
                    sqlWhere += "Asunto LIKE '%" + asunto + "%' AND ";

                if (pqrsrecordid.Trim() != "")
                    sqlWhere += "PQRSRecordId LIKE '%" + pqrsrecordid + "%' AND ";

                if (motivopqrsnombre.Trim() != "")
                    sqlWhere += "(TipoPQRS + MotivoPQRSNombre) LIKE '%" + motivopqrsnombre + "%' AND ";

                if (cliente.Trim() != "")
                    sqlWhere += "ClienteId in (" + cliente + ") AND ";

                if (prioridad.Trim() != "")
                    sqlWhere += "Prioridad LIKE '%" + prioridad + "%' AND ";

                if (analista.Trim() != "")
                    sqlWhere += "AnalistaId in (" + analista + ") AND ";

                if (fechacreacion.Trim() != "")
                    sqlWhere += "FechaCreacion LIKE '%" + fechacreacion + "%' AND ";


                if (estado.Trim() != "")
                    sqlWhere += "Estado  LIKE '%" + estado + "%' AND ";

                //string clienteIds = string.Join(" , ", Seguridadcll.ClienteList.ToList().Select(e => "'" + e.ClienteID + "'"));

                //if (clienteIds.Trim() != "")
                //    sqlWhere += "ClienteId  in (" + clienteIds + ") AND ";
                //else
                //    sqlWhere += " 0 = 1  AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");


                string sqlCount = "select COUNT(Id)  from RecruitmentsView" + sqlWhere;




                string sqlData = "SELECT * FROM (select Row_number() OVER( ORDER BY id DESC) AS Seq, * from RecruitmentsView " + sqlWhere +
                $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";


                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = data
                });

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            // return InternalServerError("");
        }

        public async Task<IHttpActionResult> CreaPQRSRecord(TipoPQRS TipoPQRS, int id){
            var dataDist = new List<PQRSMotivoViewModel>();

            //Tipo Recrutamento
            if (TipoPQRS == TipoPQRS.Recruitment)
                dataDist = await db.Recruitments.Where(r => r.RecruitmentId == id).
                    Select(RE => new PQRSMotivoViewModel() { DataId = RE.RecruitmentId, MotivoPQRSId = 29 , PQRSRecordId = RE.RecruitmentId }).
                    Distinct().ToListAsync();

            try
            {
                if (dataDist.Count == 0)
                {
                    return Ok(false);
                }
                //Consecutivo del flujo
                int idx = 1;

                //Recorre data
                foreach (var item in dataDist)
                {
                    var flujo = await db.FlujosPQRS.Include(f => f.MotivoPQRS).Where(f => f.MotivoPQRSId == item.MotivoPQRSId).ToListAsync();

                    idx = 1;
                    idx = await db.Database.SqlQuery<int>("select isnull(max(id),0) from PQRSRecord").FirstOrDefaultAsync() + 1;
                    int CommentId = 0;

                    foreach (var f in flujo)
                    {
                        //1.  Crea record
                        PQRSRecord nPQRS = new PQRSRecord();
                        nPQRS.Id = idx;
                        nPQRS.Order = f.Order;
                        PQRSRecordComentario nPQRSComment = new PQRSRecordComentario();
                        if (f.Id == 1)
                        {
                            //nPQRS.EstadoStep = EstadoStep.Approved;
                            nPQRS.EstadoStep = EstadoStep.Done;
                            nPQRS.PasoActual = false;
                            //Crear Comment (primer paso Success)
                            nPQRSComment.PQRSRecordId = nPQRS.Id;
                            nPQRSComment.PQRSRecordOrder = nPQRS.Order;
                            nPQRSComment.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                            nPQRSComment.FechaCreacion = DateTime.Now;
                            nPQRSComment.Comentario = f.Nombre + " Formato:" + f.MotivoPQRS.TipoPQRS.ToString() + " Motivo:" + f.MotivoPQRS.Nombre;
                            nPQRSComment.TipoComentario = TipoComentario.Approval;
                            db.PQRSRecordComentarios.Add(nPQRSComment);
                            // await db.SaveChangesAsync();
                            AddLog("", nPQRS.Id, nPQRSComment);
                        }
                        else if (f.Id == 2)
                        {
                            nPQRS.EstadoStep = EstadoStep.In_Process;
                            nPQRS.PasoActual = true;
                        }
                        else
                        {
                            nPQRS.EstadoStep = EstadoStep.Pending;
                            nPQRS.PasoActual = false;
                        }

                        nPQRS.MotivoPQRSId = f.MotivoPQRSId;
                        nPQRS.TipoPQRS = TipoPQRS;
                        nPQRS.MotivoPQRSNombre = f.MotivoPQRS.Nombre;
                        nPQRS.FlujoPQRSNombre = f.Nombre;
                        nPQRS.FlujoPQRSTipoPaso = f.TipoPaso;
                        nPQRS.EnviaCorreoDestinatarios = f.EnviaCorreoDestinatarios;
                        nPQRS.FlujoPQRSDescripcion = f.Descripcion;
                        db.PQRSRecords.Add(nPQRS);
                        await db.SaveChangesAsync();

                        if (nPQRSComment != null)
                            CommentId = nPQRSComment.Id;

                        AddLog("", nPQRS.Id, nPQRS);
                        //crea record usuarios
                        var flujoUsuarios = await db.UsuarioFlujoPQRS.Where(uf => uf.MotivoPQRSId == f.MotivoPQRSId && uf.FlujoPQRSId == f.Id).ToListAsync();
                        foreach (var uf in flujoUsuarios)
                        {
                            PQRSRecordUsuario nPQRSUser = new PQRSRecordUsuario();
                            if (uf.UsuarioId == Seguridadcll.Configuracion.UsuarioVendedorPQRS)
                            {
                                string usuarioVendedor = "";
                                usuarioVendedor = await db.Novedad.Include(d => d.Cliente).Where(d => d.Id == id).Select(d => d.Cliente.VendedorId).FirstOrDefaultAsync();

                                nPQRSUser.UsuarioId = usuarioVendedor;
                            }
                            else
                            {
                                nPQRSUser.UsuarioId = uf.UsuarioId;
                            }

                            nPQRSUser.PQRSRecordId = nPQRS.Id;
                            nPQRSUser.PQRSRecordOrder = nPQRS.Order;

                            nPQRSUser.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Unanswered;

                            db.PQRSRecordUsuarios.Add(nPQRSUser);
                            await db.SaveChangesAsync();
                            AddLog("", nPQRSUser.UsuarioId, nPQRSUser);
                        }

                        //Tareas
                        var Tareas = await db.FlujoPQRSTareas.Where(uf => uf.MotivoPQRSId == f.MotivoPQRSId && uf.FlujoPQRSId == f.Id).ToListAsync();
                        foreach (var t in Tareas)
                        {
                            PQRSRecordTareas nTarea = new PQRSRecordTareas();
                            //nTarea.Id = t.Id;
                            nTarea.PQRSRecordId = nPQRS.Id;
                            nTarea.PQRSRecordOrder = nPQRS.Order;
                            nTarea.Descripcion = t.Descripcion;
                            nTarea.Requerido = t.Requerido;
                            nTarea.Terminado = false;

                            db.PQRSRecordTareas.Add(nTarea);
                            await db.SaveChangesAsync();
                            AddLog("", nTarea.Id, nTarea);
                        }


                        //Condiciones
                        var Condiciones = await db.FlujoPQRSCondiciones.Where(uf => uf.MotivoPQRSId == f.MotivoPQRSId && uf.FlujoPQRSId == f.Id).ToListAsync();
                        foreach (var c in Condiciones)
                        {
                            PQRSRecordCondiciones nCondicion = new PQRSRecordCondiciones();
                            nCondicion.Id = c.Id;
                            nCondicion.PQRSRecordId = nPQRS.Id;
                            nCondicion.PQRSRecordOrder = nPQRS.Order;
                            nCondicion.Descripcion = c.Descripcion;
                            nCondicion.TipoCondicion = c.TipoCondicion;
                            nCondicion.CondicionesValor = c.CondicionesValor;
                            nCondicion.Valor = c.Valor;
                            nCondicion.SiNo = c.SiNo;
                            nCondicion.RespValor = 0;
                            nCondicion.RespSiNo = false;

                            db.PQRSRecordCondiciones.Add(nCondicion);
                            await db.SaveChangesAsync();
                            AddLog("", nCondicion.Id, nCondicion);
                        }


                        //

                        if (f.Id == 4)
                           SendNotificationEmailTask(TipoNotificacion.NextStep, TipoComentario.Approval, TipoPQRS, nPQRS.Id, nPQRS.Order, dataDist.FirstOrDefault().DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                    }

                    if (TipoPQRS == TipoPQRS.Recruitment)
                    {
                        var itemsdev = await db.Recruitments.Where(di => di.RecruitmentId == item.DataId && 29 == item.MotivoPQRSId).ToListAsync();
                        foreach (var itemD in itemsdev)
                        {
                            itemD.PQRSRecordId = idx;
                            db.Entry(itemD).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            AddLog("", itemD.RecruitmentId, itemD);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(true);
        }

        public void SendNotificationEmailTask(TipoNotificacion tipoNotificacion, TipoComentario tipoComentario, TipoPQRS tipoPQRS,int RecordId, int Order, int DataId, int CommentId, string UsuarioNombre, string AppLink){
            var formato = new Formato();
            try
            {
                    formato = db.Recruitments.Include(d => d.UsuarioCreacion).Include(d => d.Analista).Where(d => d.RecruitmentId == DataId)
                                        .Select(d => new Formato{
                                            TipoPQRS = tipoPQRS,
                                            Id = d.RecruitmentId,
                                            AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                            AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                            NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                        }).FirstOrDefault();

                List<Mails> mails = new List<Mails>();

                string subject = "AIS - ";
                subject += $"[{formato.NroTracking}]: {formato.Asunto}  ({tipoNotificacion.ToString()})";
                string msg = $"The user <b>{UsuarioNombre}</b> ";
                string action = "Details";


                bool flagDestinatarios = false;
                //Primer paso ( se crear el ticket de PQRS)
                if (tipoNotificacion == TipoNotificacion.CreateFormat)
                {
                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                    flagDestinatarios = true;
                }
                else if (tipoNotificacion == TipoNotificacion.Assign)
                {
                    /* msg += $"has assigned a new CS analyst ({formato.AnalistaId})";
                     msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                     msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                     msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                     msg += $"<b>Note</b>: {formato.Observacion}<br />";
                     var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                     msg += $"<b>Reason</b>: {step.MotivoPQRSNombre}<br />";
                     var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                     msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                     msg += $"<b>Comment</b>: {comment.Comentario}<br />";
                     flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;*/

                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                }
                else if (tipoNotificacion == TipoNotificacion.Comment)
                {
                    /* msg += $"has added a new comment";

                     msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                     msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                     msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                     msg += $"<b>Note</b>: {formato.Observacion}<br />";

                     var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                     msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                     msg += $"<b>Comment</b>: {comment.Comentario}<br />";*/

                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";


                }
                else if (tipoNotificacion == TipoNotificacion.CurrentStep)
                {
                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                    /*
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    if (tipoComentario == TipoComentario.Approval)
                    {
                        msg += $"has approved <b>Step:</b> {step.FlujoPQRSNombre} <b>Reason PQRS:</b> {step.MotivoPQRSNombre}";
                    }
                    else if (tipoComentario == TipoComentario.Rejection)
                    {
                        msg += $"has rejected <b>Step:</b> {step.FlujoPQRSNombre} <b>Reason PQRS:</b> {step.MotivoPQRSNombre}";
                    }


                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";
                    msg += $"<b>Reason</b>: {step.MotivoPQRSNombre}<br />";
                    var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                    msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                    msg += $"<b>Comment</b>: {comment.Comentario}<br />";
                    var userStep = db.PQRSRecordUsuarios
                                            .Include(ru => ru.Usuario)
                                            .Where(ru => ru.PQRSRecordId == RecordId && ru.PQRSRecordOrder == Order).ToList();
                    foreach (var u in userStep)
                    {
                        mails.Add(new Mails { to = u.Usuario.UsuarioCorreo, toName = u.Usuario.UsuarioNombre });
                    }
                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;
                    */
                }
                else if (tipoNotificacion == TipoNotificacion.NextStep)
                {
                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                    /*
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefaultAsync();
                    msg = $"You have pending an action to do in system, please log in here to continue";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";

                    var userStep = db.PQRSRecordUsuarios
                                            .Include(ru => ru.Usuario)
                                            .Where(ru => ru.PQRSRecordId == RecordId && ru.PQRSRecordOrder == Order).ToList();
                    mails.Clear();
                    foreach (var u in userStep)
                    {
                        mails.Add(new Mails { to = u.Usuario.UsuarioCorreo, toName = u.Usuario.UsuarioNombre });
                    }
                    flagDestinatarios = false;
                    */
                }
                else if (tipoNotificacion == TipoNotificacion.Close)
                {
                    /*
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    msg = $"has closed  <b>Reason PQRS:</b> {step.MotivoPQRSNombre}";
                    var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";
                    msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                    msg += $"<b>Comment</b>: {comment.Comentario}<br />";
                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;
                    */
                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                }
                else if (tipoNotificacion == TipoNotificacion.Complete)
                {
                    /*
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    msg += $"has completed  ";

                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";

                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;
                    */
                    msg += $"has created a new Recruitment({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Testecorreo</b>: {formato.AnalistaCorreo}<br />";
                    msg += $"<b>Testeanalista</b>: {formato.AnalistaId}<br />";
                    msg += $"<b>Traking</b>: {formato.NroTracking}<br />";
                    msg += $"<b>Recruitment</b>: {formato.Id}<br />";
                }

                //Destinatario del formato
                if (flagDestinatarios)
                {
                    var destinatarios = formato.Destinatarios.Split(',');
                    foreach (var d in destinatarios)
                    {
                        if (d != null && d != "")
                            mails.Add(new Mails { to = d, toName = d });
                    }
                }

                //if (tipoNotificacion != TipoNotificacion.CreateFormat)
                msg += $"<br /><br /><a style='color:#22BCE5' href={{url}}/PQRS/{action}?TipoPQRS={tipoPQRS}&DataId={DataId}&RecordId={RecordId}>Click here to view the PQRS.</a>";

                foreach (var m in mails)
                {
                    Task.Run(() => Fn.SendHtmlEmail("cayo.diebe@apextoolgroup.com", "Cayo Diebe", subject, msg, AppLink));
                    //Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, msg, AppLink));
                }
            }
            catch
            {
                return;
            }

            // await Task.Run(() => Fn.SendHtmlEmail(to, toName, subject, msg));
        }

        private class Formato  // corregir con nombres de cambios correctos
        {
            public TipoPQRS TipoPQRS { get; set; }
            public int Id { get; set; }
            public DateTime FechaCreacion { get; set; }
            public EstadoFormatoPQRS Estado { get; set; }
            public Prioridad Prioridad { get; set; }
            public TipoPersona TipoPersona { get; set; }
            public string Persona { get; set; }
            public string AnalistaId { get; set; }
            public string AnalistaCorreo { get; set; }
            public string Asunto { get; set; }
            public string Observacion { get; set; }
            public string Destinatarios { get; set; }
            public string NroTracking { get; set; }

            //Remove Usuario Creacion/UsuarioCreacionCorreo, Cliente
        }

        private class Mails
        {
            public string to { get; set; }
            public string toName { get; set; }
        }

        public enum TipoNotificacion
        {
            CreateFormat = 1,
            //Comentario General
            Comment = 2,
            //Notificación de atención a los usuario del paso actual
            CurrentStep = 3,
            //Notificación de atención a los usuario del siguiente paso
            NextStep = 4,
            //Notificación asignación de Analista.
            Assign = 5,
            //Notificación Cierre
            Close = 6,
            //Notificación Complete
            Complete = 7,

            DoesNotApply = 8

        }

    }
}
