
using System;//ok
using System.Collections.Generic;//ok
using System.Data;//ok
using System.Data.Entity;//ok
using System.Linq;//ok
using System.Net;
using System.Web;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System.Net.Mail;

//using System.IO;
//using LinqToExcel;
//using LinqToExcel.Attributes;

namespace MMS.ApiControllers.WCSS
{
    public class PQRSController : ApiBaseController
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

                string clienteIds = string.Join(" , ", Seguridadcll.ClienteList.ToList().Select(e => "'" + e.ClienteID + "'"));

                if (clienteIds.Trim() != "")
                    sqlWhere += "ClienteId  in (" + clienteIds + ") AND ";
                else
                    sqlWhere += " 0 = 1  AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");


                string sqlCount = "select COUNT(Id)  from PQRSView" + sqlWhere;




                string sqlData = "SELECT * FROM (select Row_number() OVER( ORDER BY id DESC) AS Seq, * from PQRSView " + sqlWhere +
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
        public async Task<IHttpActionResult> AsignarAnalista(AsignarAnalistaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var flujoPQRS = new List<PQRSMotivoViewModel>();
                    if (model.TipoPQRS == TipoPQRS.Devolucion)
                    {
                        var devolucion = await db.Devoluciones.Where(d => d.Id == model.DataId).FirstOrDefaultAsync();



                        devolucion.AnalistaId = model.AnalistaId;
                        db.Entry(devolucion).State = EntityState.Modified;
                        AddLog("Devoluciones/Edit", devolucion.Id, devolucion);
                        await db.SaveChangesAsync();

                        //Lista de Flujos
                        flujoPQRS = await db.DevolucionItems
                                                .Where(di => di.DevolucionId == model.DataId)
                                                .Select(di => new PQRSMotivoViewModel { DataId = di.DevolucionId, PQRSRecordId = di.PQRSRecordId, MotivoPQRSId = di.MotivoPQRSId })
                                                .Distinct()
                                                .ToListAsync();


                    }
                    else if (model.TipoPQRS == TipoPQRS.Garantia)
                    {
                        var garantia = await db.Garantias.Where(d => d.Id == model.DataId).FirstOrDefaultAsync();


                        garantia.AnalistaId = model.AnalistaId;
                        db.Entry(garantia).State = EntityState.Modified;
                        AddLog("Garantias/Edit", garantia.Id, garantia);
                        await db.SaveChangesAsync();

                        //Lista de Flujos
                        flujoPQRS = await db.GarantiaItems
                                                .Where(di => di.GarantiaId == model.DataId)
                                                .Select(di => new PQRSMotivoViewModel { DataId = di.GarantiaId, PQRSRecordId = di.PQRSRecordId, MotivoPQRSId = di.MotivoPQRSId })
                                                .Distinct()
                                                .ToListAsync();

                    }
                    else if (model.TipoPQRS == TipoPQRS.Novedad)
                    {
                        var novedad = await db.Novedad.Where(d => d.Id == model.DataId).FirstOrDefaultAsync();


                        novedad.AnalistaId = model.AnalistaId;
                        db.Entry(novedad).State = EntityState.Modified;
                        AddLog("Novedades/Edit", novedad.Id, novedad);
                        await db.SaveChangesAsync();

                        //Lista de Flujos
                        flujoPQRS = await db.NovedadItem
                                                .Where(di => di.NovedadId == model.DataId)
                                                .Select(di => new PQRSMotivoViewModel { DataId = di.NovedadId, PQRSRecordId = di.PQRSRecordId, MotivoPQRSId = di.MotivoPQRSId })
                                                .Distinct()
                                                .ToListAsync();
                    }
                    else if (model.TipoPQRS == TipoPQRS.Recruitment)
                    {
                        var recruitment = await db.Recruitments.Where(d => d.RecruitmentId == model.DataId).FirstOrDefaultAsync();


                        recruitment.AnalistaId = model.AnalistaId;
                        db.Entry(recruitment).State = EntityState.Modified;
                        AddLog("Recruitments/Edit", recruitment.RecruitmentId, recruitment);
                        await db.SaveChangesAsync();

                        //Lista de Flujos
                        flujoPQRS = await db.Recruitments
                                                .Where(RE => RE.RecruitmentId == model.DataId)
                                                .Select(di => new PQRSMotivoViewModel { DataId = di.RecruitmentId, PQRSRecordId = di.PQRSRecordId, MotivoPQRSId = 29 /*di.MotivoPQRSId */})
                                                .Distinct()
                                                .ToListAsync();
                    }


                    foreach (var f in flujoPQRS)
                    {
                        var pqrsrecord = await db.PQRSRecords.Where(p => p.Id == f.PQRSRecordId && p.PasoActual == true && p.TipoPQRS == model.TipoPQRS).FirstOrDefaultAsync();

                        // Crear Comment (primer paso Success)
                        var analista = await db.Usuarios.Where(u => u.UsuarioId == model.AnalistaId).FirstOrDefaultAsync();

                        PQRSRecordComentario nPQRSComment = new PQRSRecordComentario();
                        nPQRSComment.PQRSRecordId = pqrsrecord.Id;
                        nPQRSComment.PQRSRecordOrder = (pqrsrecord.Order == 2) ? pqrsrecord.Order : 0;
                        nPQRSComment.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                        nPQRSComment.FechaCreacion = DateTime.Now;
                        nPQRSComment.Comentario = $"PQRS Asignado a :  ({model.AnalistaId}) {analista.UsuarioNombre}    Comment:   {model.Comment}";
                        nPQRSComment.TipoComentario = TipoComentario.Approval;
                        db.PQRSRecordComentarios.Add(nPQRSComment);
                        await db.SaveChangesAsync();
                        AddLog("PQRS/AddComment", nPQRSComment.Id, nPQRSComment);

                        if (pqrsrecord.Order == 2)
                        {
                            await ChangeStep(TipoComentario.Approval, pqrsrecord.Id, pqrsrecord.Order, model.DataId, model.TipoPQRS);
                        }
                        ////Users in step
                        //var usuarioStep = await db.PQRSRecordUsuarios
                        //                          .Where(pu => pu.PQRSRecordId == pqrsrecord.Id && pu.PQRSRecordOrder == pqrsrecord.Order && pu.UsuarioId == Seguridadcll.Usuario.UsuarioId)
                        //                          .FirstOrDefaultAsync();
                        //if (usuarioStep != null)
                        //{
                        //    usuarioStep.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Approved;
                        //    db.Entry(usuarioStep).State = EntityState.Modified;
                        //    await db.SaveChangesAsync();
                        //}//if (usuarioStep != null)

                        //Task.Run(() => SendNotificationEmailTask(TipoNotificacion.Assign, TipoComentario.Approval, model.TipoPQRS, pqrsrecord.Id, pqrsrecord.Order, model.DataId, nPQRSComment.Id, usuario, AppLink));
                        SendNotificationEmailTask(TipoNotificacion.Assign, TipoComentario.Approval, model.TipoPQRS, pqrsrecord.Id, pqrsrecord.Order, model.DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);

                    }


                }
                catch
                {
                    return InternalServerError();
                }
                return Ok(true);
            }

            return Ok(false);
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> CreaPQRSRecord(TipoPQRS TipoPQRS, int id)
        {

            var dataDist = new List<PQRSMotivoViewModel>();

            //Tipo devolución (flujo a crear)
            if (TipoPQRS == TipoPQRS.Devolucion)
            {
                //devoluciones agrupadas por motivo 
                dataDist = await db.DevolucionItems
                                            .Include(di => di.MotivoPQRS)
                                            .Where(di => di.DevolucionId == id && di.MotivoPQRS.Activo == true && (di.PQRSRecordId == null || di.PQRSRecordId == 0))
                                            .Select(di => new PQRSMotivoViewModel() { DataId = di.DevolucionId, MotivoPQRSId = di.MotivoPQRSId, PQRSRecordId = di.PQRSRecordId })
                                            .Distinct().ToListAsync();



            }
            else if (TipoPQRS == TipoPQRS.Garantia)
            {
                //Garantias agrupadas por motivo 
                dataDist = await db.GarantiaItems
                                    .Include(gi => gi.MotivoPQRS)
                                    .Where(gi => gi.GarantiaId == id && gi.MotivoPQRS.Activo == true && (gi.PQRSRecordId == null || gi.PQRSRecordId == 0))
                                    .Select(di => new PQRSMotivoViewModel() { DataId = di.GarantiaId, MotivoPQRSId = di.MotivoPQRSId, PQRSRecordId = di.PQRSRecordId })
                                    .Distinct().ToListAsync();
            }
            else if (TipoPQRS == TipoPQRS.Novedad)
            {
                //Garantias agrupadas por motivo 
                dataDist = await db.NovedadItem
                                    .Include(ni => ni.MotivoPQRS)
                                    .Where(ni => ni.NovedadId == id && ni.MotivoPQRS.Activo == true && (ni.PQRSRecordId == null || ni.PQRSRecordId == 0))
                                    .Select(di => new PQRSMotivoViewModel() { DataId = di.NovedadId, MotivoPQRSId = di.MotivoPQRSId, PQRSRecordId = di.PQRSRecordId })
                                    .Distinct().ToListAsync();
            }
            else if (TipoPQRS == TipoPQRS.Recruitment)
            {
                dataDist = await db.Recruitments
                                    .Where(r => r.RecruitmentId == id)
                                    .Select(RE => new PQRSMotivoViewModel() { DataId = RE.RecruitmentId, MotivoPQRSId = 29, PQRSRecordId = RE.RecruitmentId })
                                    .Distinct().ToListAsync();
            }
            //Esta Fazendo ^

            try{
                if (dataDist.Count == 0)
                    return Ok(false);
                
                //Consecutivo del flujo
                int idx = 1;

                //Recorre data
                foreach (var item in dataDist)
                {
                    var flujo = await db.FlujosPQRS
                                        .Include(f => f.MotivoPQRS)
                                        .Where(f => f.MotivoPQRSId == item.MotivoPQRSId).ToListAsync();

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
                        //await db.SaveChangesAsync();

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
                                if (TipoPQRS == TipoPQRS.Devolucion)
                                    usuarioVendedor = await db.Devoluciones.Include(d => d.Cliente).Where(d => d.Id == id).Select(d => d.Cliente.VendedorId).FirstOrDefaultAsync();
                                else if (TipoPQRS == TipoPQRS.Garantia)
                                    usuarioVendedor = await db.Garantias.Include(d => d.Cliente).Where(d => d.Id == id).Select(d => d.Cliente.VendedorId).FirstOrDefaultAsync();
                                else if (TipoPQRS == TipoPQRS.Novedad)
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
                        }//foreach(var uf in flujoUsuarios)

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

                        if (f.Id == 2)
                            SendNotificationEmailTask(TipoNotificacion.NextStep, TipoComentario.Approval, TipoPQRS, nPQRS.Id, nPQRS.Order, dataDist.FirstOrDefault().DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);

                        if (f.Id == 4)
                            SendNotificationEmailTask(TipoNotificacion.NextStep, TipoComentario.Approval, TipoPQRS, nPQRS.Id, nPQRS.Order, dataDist.FirstOrDefault().DataId, nPQRSComment.Id, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);



                    }//foreach (var f in flujo)

                    if (TipoPQRS == TipoPQRS.Devolucion)
                    {
                        //Actualiza Items con motivo (PQRSRecordID)
                        var itemsdev = await db.DevolucionItems.Where(di => di.DevolucionId == item.DataId && di.MotivoPQRSId == item.MotivoPQRSId).ToListAsync();
                        foreach (var itemD in itemsdev)
                        {
                            itemD.PQRSRecordId = idx;
                            db.Entry(itemD).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            AddLog("", itemD.ItemId, itemD);
                        }
                    }
                    else if (TipoPQRS == TipoPQRS.Garantia)
                    {
                        //Actualiza Items con motivo (PQRSRecordID)
                        var itemsGaran = await db.GarantiaItems.Where(di => di.GarantiaId == item.DataId && di.MotivoPQRSId == item.MotivoPQRSId).ToListAsync();
                        foreach (var itemG in itemsGaran)
                        {
                            itemG.PQRSRecordId = idx;
                            db.Entry(itemG).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            AddLog("", itemG.ItemId, itemG);
                        }
                    }
                    else if (TipoPQRS == TipoPQRS.Novedad)
                    {
                        //Actualiza Items con motivo (PQRSRecordID)
                        var itemsNov = await db.NovedadItem.Where(di => di.NovedadId == item.DataId && di.MotivoPQRSId == item.MotivoPQRSId).ToListAsync();
                        foreach (var itemN in itemsNov)
                        {
                            itemN.PQRSRecordId = idx;
                            db.Entry(itemN).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            AddLog("", itemN.ItemId, itemN);
                        }
                    }
                }//foreach(var item in dataDist)

                if (TipoPQRS == TipoPQRS.Devolucion)
                {
                    //Actualiza estado Devolución
                    var devolucion = await db.Devoluciones.Where(d => d.Id == id).FirstOrDefaultAsync();
                    devolucion.Estado = EstadoFormatoPQRS.In_Process;

                    if (devolucion.NroTracking == "" || devolucion.NroTracking == null)
                    {
                        string HASHNroTracking = Fn.HASH("D" + devolucion.Id);
                        HASHNroTracking = "D" + devolucion.Id + HASHNroTracking;
                        devolucion.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();
                    }

                    db.Entry(devolucion).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", devolucion.Id, devolucion);


                    SendNotificationEmailTask(TipoNotificacion.CreateFormat, TipoComentario.Comment, TipoPQRS, 0, 0, devolucion.Id, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);

                }
                else if (TipoPQRS == TipoPQRS.Garantia)
                {
                    //Actualiza estado Garantia
                    var garantia = await db.Garantias.Where(d => d.Id == id).FirstOrDefaultAsync();
                    garantia.Estado = EstadoFormatoPQRS.In_Process;

                    if (garantia.NroTracking == "" || garantia.NroTracking == null)
                    {
                        string HASHNroTracking = Fn.HASH("G" + garantia.Id);
                        HASHNroTracking = "G" + garantia.Id + HASHNroTracking;
                        garantia.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();
                    }

                    db.Entry(garantia).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", garantia.Id, garantia);

                    SendNotificationEmailTask(TipoNotificacion.CreateFormat, TipoComentario.Comment, TipoPQRS, 0, 0, garantia.Id, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                }
                else if (TipoPQRS == TipoPQRS.Novedad)
                {
                    //Actualiza estado Novedad
                    var novedad = await db.Novedad.Where(d => d.Id == id).FirstOrDefaultAsync();
                    novedad.Estado = EstadoFormatoPQRS.In_Process;

                    if (novedad.NroTracking == "" || novedad.NroTracking == null)
                    {
                        string HASHNroTracking = Fn.HASH("N" + novedad.Id);
                        HASHNroTracking = "N" + novedad.Id + HASHNroTracking;
                        novedad.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();
                    }

                    db.Entry(novedad).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", novedad.Id, novedad);

                    SendNotificationEmailTask(TipoNotificacion.CreateFormat, TipoComentario.Comment, TipoPQRS, 0, 0, novedad.Id, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                }
                else if (TipoPQRS == TipoPQRS.Recruitment)
                {
                    //Actualiza estado Garantia
                    var recruitment = await db.Recruitments.Where(d => d.RecruitmentId == id).FirstOrDefaultAsync();
                    recruitment.Estado = EstadoFormatoPQRS.In_Process;

                    recruitment.PQRSRecordId = idx;

                    if (recruitment.NroTracking == "" || recruitment.NroTracking == null)
                    {
                        string HASHNroTracking = Fn.HASH("RE" + recruitment.RecruitmentId);
                        HASHNroTracking = "RE" + recruitment.RecruitmentId + HASHNroTracking;
                        recruitment.NroTracking = HASHNroTracking.Substring(0, 10).ToUpper();
                    }

                    db.Entry(recruitment).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", recruitment.RecruitmentId, recruitment);

                    SendNotificationEmailTask(TipoNotificacion.CreateFormat, TipoComentario.Comment, TipoPQRS, 0, 0, recruitment.RecruitmentId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }


            return Ok(true);
        }

        [NonAction]
        public async Task<bool> ChangeStep(TipoComentario TipoComentario, int RecordId, int Order, int DataId, TipoPQRS TipoPQRS)
        {
            bool result = true;
            try
            {
                //Paso actual
                var currentStep = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.Order == Order && pr.TipoPQRS == TipoPQRS).FirstOrDefaultAsync();

                //Si el paso es de aprobación revisar (todos approve)
                bool flagApprove = false;


                int OrderNew = 0;
                if (TipoComentario == TipoComentario.Approval)
                {
                    //currentStep.EstadoStep = EstadoStep.Approved;
                    currentStep.EstadoStep = EstadoStep.Done;
                    currentStep.PasoActual = false;
                    OrderNew = Order + 1;

                    //if (currentStep.FlujoPQRSTipoPaso == TipoPaso.Aprobar)
                    //{
                    var usuariosStep = await db.PQRSRecordUsuarios.Where(pu => pu.PQRSRecordId == currentStep.Id && pu.PQRSRecordOrder == currentStep.Order).ToListAsync();
                    foreach (var u in usuariosStep)
                    {
                        //if (u.EstadoUsuarioFlujoPQRS != EstadoUsuarioFlujoPQRS.Approved)
                        if (u.EstadoUsuarioFlujoPQRS == EstadoUsuarioFlujoPQRS.Done || (currentStep.Order == 2))
                        {
                            flagApprove = true;
                        }
                    }//foreach (var u in usuariosStep)
                     // }//if (currentStep.FlujoPQRSTipoPaso == TipoPaso.Aprobar)

                }//if (TipoComentario == TipoComentario.Approval)
                else if (TipoComentario == TipoComentario.Rejection)
                {
                    //currentStep.EstadoStep = EstadoStep.Rejected;
                    currentStep.EstadoStep = EstadoStep.Returned;
                    currentStep.PasoActual = false;
                    OrderNew = Order - 1;

                    flagApprove = true;
                }//else if (TipoComentario == TipoComentario.Rejection)

                if (flagApprove)
                {
                    //Paso actual se guarda sin ser el actual
                    db.Entry(currentStep).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    bool flagWhile = true;
                    // loop para recorrer los pasos siguientes (sea aumentanto el order o disminuyento)
                    while (flagWhile)
                    {
                        var newStep = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.Order == OrderNew && pr.TipoPQRS == TipoPQRS).FirstOrDefaultAsync();
                        if (newStep != null)
                        {
                            #region NewStepFull
                            TipoNotificacion vTipoNotificacion = TipoNotificacion.NextStep;
                            if (TipoComentario == TipoComentario.Approval)
                            {
                                //obtiene las condiciones
                                var condiciones = await db.PQRSRecordCondiciones.Where(pc => pc.PQRSRecordId == RecordId && pc.PQRSRecordOrder == OrderNew).ToListAsync();
                                if (condiciones.Count > 0)
                                {
                                    
                                    bool flagSaltaPaso = true;
                                    //Analiza condiciones
                                    foreach (var c in condiciones)
                                    {
                                        if (c.TipoCondicion == TipoCondicion.Value)
                                        {
                                            if (c.CondicionesValor == CondicionesValor.Equal)
                                                flagSaltaPaso = (c.RespValor != c.Valor) ? flagSaltaPaso : false;
                                            else if (c.CondicionesValor == CondicionesValor.Higher)
                                                flagSaltaPaso = (c.Valor > c.RespValor) ? flagSaltaPaso : false;
                                            else if (c.CondicionesValor == CondicionesValor.HigherEqual)
                                                flagSaltaPaso = (c.Valor >= c.RespValor) ? flagSaltaPaso : false;
                                            else if (c.CondicionesValor == CondicionesValor.Less)
                                                flagSaltaPaso = (c.Valor < c.RespValor) ? flagSaltaPaso : false;
                                            else if (c.CondicionesValor == CondicionesValor.LessEqual)
                                                flagSaltaPaso = (c.Valor <= c.RespValor) ? flagSaltaPaso : false;

                                        }
                                        else if (c.TipoCondicion == TipoCondicion.YesNo)
                                        {
                                            flagSaltaPaso = (c.SiNo != c.RespSiNo) ? flagSaltaPaso : false;
                                        }
                                    }//foreach (var c in condiciones)

                                    if (flagSaltaPaso)
                                    {
                                        newStep.EstadoStep = EstadoStep.DoesNotApply;
                                        newStep.PasoActual = false;
                                        vTipoNotificacion = TipoNotificacion.DoesNotApply;
                                    }
                                    else
                                    {
                                        newStep.EstadoStep = EstadoStep.In_Process;
                                        newStep.PasoActual = true;
                                        //solo se recorre hasta este paso
                                        flagWhile = false;
                                    }
                                                                        //Next Step Mail
                                    //SendNotificationEmailTask(vTipoNotificacion, TipoComentario, TipoPQRS, currentStep.Id, currentStep.Order, DataId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);

                                }//if (condiciones.Count > 0)
                                else
                                {
                                    newStep.EstadoStep = EstadoStep.In_Process;
                                    newStep.PasoActual = true;
                                    //solo se recorre hasta este paso
                                    flagWhile = false;
                                }//if (condiciones.Count > 0)

                                OrderNew++;//sigue recorriendo si flagWhile == true;
                            }// if (TipoComentario == TipoComentario.Approval)
                            else if (TipoComentario == TipoComentario.Rejection)
                            {
                                if (newStep.EstadoStep == EstadoStep.DoesNotApply)
                                {
                                    newStep.EstadoStep = EstadoStep.Pending;
                                    newStep.PasoActual = false;
                                }
                                else
                                {
                                    newStep.EstadoStep = EstadoStep.In_Process;
                                    newStep.PasoActual = true;
                                    //solo se recorre hasta este paso
                                    flagWhile = false;
                                }//if (newStep.EstadoStep == EstadoStep.DoesNotApply)
                                OrderNew--;
                            }// else if (TipoComentario == TipoComentario.Rejection)


                            db.Entry(newStep).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            //Reinicia respuesta de usuarios en el paso.
                            var usuariosNewStep = await db.PQRSRecordUsuarios.Where(pu => pu.PQRSRecordId == newStep.Id && pu.PQRSRecordOrder == newStep.Order).ToListAsync();
                            foreach (var u in usuariosNewStep)
                            {
                                u.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Unanswered;
                                db.Entry(u).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                            }//foreach (var u in usuariosNewStep)

                            SendNotificationEmailTask(vTipoNotificacion, TipoComentario, TipoPQRS, currentStep.Id, currentStep.Order, DataId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                            #endregion NewStepFull
                        }else
                        {
                            #region NewStepNull


                            if (TipoComentario == TipoComentario.Approval)
                            {
                                //Revisar si todos los flujos estan completo y cerrar el formato(devolucion, garantia o novedad)
                                bool flagComplete = true;
                                var steps = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.TipoPQRS == TipoPQRS).ToListAsync();
                                foreach (var s in steps)
                                {
                                    //if (s.EstadoStep != EstadoStep.Approved)
                                    if (s.EstadoStep != EstadoStep.Done)
                                    {
                                        flagComplete = false;
                                    }
                                }//foreach (var s in steps)

                                if (flagComplete)
                                {
                                    var LastStep = steps.LastOrDefault();
                                    LastStep.EstadoStep = EstadoStep.Completed;
                                    LastStep.PasoActual = true;
                                    db.Entry(LastStep).State = EntityState.Modified;
                                    await db.SaveChangesAsync();
                                    //Complete Flow
                                    SendNotificationEmailTask(TipoNotificacion.Complete, TipoComentario, TipoPQRS, currentStep.Id, currentStep.Order, DataId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                                }
                            }// if (TipoComentario == TipoComentario.Approval)
                            flagWhile = false;
                            #endregion NewStepNull

                        }
                    }// while (flagWhile)




                    /*if (newStep != null)
                    {
                        //Paso actual se guarda sin ser el actual
                        db.Entry(currentStep).State = EntityState.Modified;
                        await db.SaveChangesAsync();



                        newStep.EstadoStep = EstadoStep.In_Process;
                        newStep.PasoActual = true;
                        db.Entry(newStep).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        //Reinicia respuesta de usuarios en el paso.
                        var usuariosNewStep = await db.PQRSRecordUsuarios.Where(pu => pu.PQRSRecordId == newStep.Id && pu.PQRSRecordOrder == newStep.Order).ToListAsync();
                        foreach (var u in usuariosNewStep)
                        {
                            u.EstadoUsuarioFlujoPQRS = EstadoUsuarioFlujoPQRS.Unanswered;
                            db.Entry(u).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }//foreach (var u in usuariosNewStep)
                        //Next Step Mail
                        SendNotificationEmailTask(TipoNotificacion.NextStep, TipoComentario, TipoPQRS, currentStep.Id, currentStep.Order, DataId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                    }
                    else
                    {
                        //Paso actual se guarda queda como actual
                        currentStep.PasoActual = true;
                        db.Entry(currentStep).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        if (TipoComentario == TipoComentario.Approval)
                        {
                            //Revisar si todos los flujos estan completo y cerrar el formato(devolucion, garantia o novedad)
                            bool flagComplete = true;
                            var steps = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.TipoPQRS == TipoPQRS).ToListAsync();
                            foreach (var s in steps)
                            {
                                //if (s.EstadoStep != EstadoStep.Approved)
                                if (s.EstadoStep != EstadoStep.Done)
                                {
                                    flagComplete = false;
                                }
                            }//foreach (var s in steps)

                            if (flagComplete)
                            {
                                var LastStep = steps.LastOrDefault();
                                LastStep.EstadoStep = EstadoStep.Completed;
                                LastStep.PasoActual = true;
                                db.Entry(LastStep).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                                //Complete Flow
                                SendNotificationEmailTask(TipoNotificacion.Complete, TipoComentario, TipoPQRS, currentStep.Id, currentStep.Order, DataId, 0, Seguridadcll.Usuario.UsuarioNombre, Seguridadcll.Aplicacion.Link);
                            }
                        }// if (TipoComentario == TipoComentario.Approval)
                    }//if (newStep != null)*/
                }// if (flagApprove) {



            }
            catch
            {
                result = false;
            }

            return result;
        }

        public async Task<bool> CloseFlow(TipoComentario TipoComentario, int RecordId, int Order)
        {
            bool result = true;
            try
            {
                if (TipoComentario == TipoComentario.Close)
                {
                    //Paso actual
                    var currentStep = await db.PQRSRecords.Where(pr => pr.Id == RecordId && pr.Order == Order).FirstOrDefaultAsync();
                    currentStep.PasoActual = false;
                    await db.SaveChangesAsync();



                    //Ultimo Paso
                    var lastStep = await db.PQRSRecords.Where(pr => pr.Id == RecordId).OrderByDescending(pr => pr.Order).FirstOrDefaultAsync();
                    lastStep.EstadoStep = EstadoStep.Closed;
                    lastStep.PasoActual = true;
                    await db.SaveChangesAsync();

                }
            }
            catch
            {
                result = false;
            }



            return result;
        }

        public async Task<bool> CompleteFormat(TipoPQRS TipoPQRS, int DataId)
        {
            bool result = true;

            try
            {
                List<int?> PQRSId = new List<int?>();
                if (TipoPQRS == TipoPQRS.Devolucion)
                {
                    PQRSId = await db.DevolucionItems.Where(di => di.DevolucionId == DataId).Select(di => di.PQRSRecordId).ToListAsync() ?? new List<int?>();
                }
                else if (TipoPQRS == TipoPQRS.Garantia)
                {
                    PQRSId = await db.GarantiaItems.Where(di => di.GarantiaId == DataId).Select(di => di.PQRSRecordId).ToListAsync() ?? new List<int?>();
                }
                else if (TipoPQRS == TipoPQRS.Novedad)
                {
                    PQRSId = await db.NovedadItem.Where(di => di.NovedadId == DataId).Select(di => di.PQRSRecordId).ToListAsync() ?? new List<int?>();
                }//if (TipoPQRS == TipoPQRS.Devolucion)

                bool flagComplete = true;
                foreach (var p in PQRSId)
                {
                    //Ultimo Paso
                    var lastStep = await db.PQRSRecords.Where(pr => pr.Id == p).OrderByDescending(pr => pr.Order).FirstOrDefaultAsync();
                    if (lastStep.EstadoStep != EstadoStep.Completed && lastStep.EstadoStep != EstadoStep.Closed)
                        flagComplete = false;

                }//foreach (var p in PQRSId)

                if (flagComplete)
                {
                    if (TipoPQRS == TipoPQRS.Devolucion)
                    {
                        var devolucion = await db.Devoluciones.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                        if (devolucion != null)
                        {
                            devolucion.Estado = EstadoFormatoPQRS.Completed;
                            db.Entry(devolucion).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                    else if (TipoPQRS == TipoPQRS.Garantia)
                    {
                        var garantia = await db.Garantias.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                        if (garantia != null)
                        {
                            garantia.Estado = EstadoFormatoPQRS.Completed;
                            db.Entry(garantia).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }
                    else if (TipoPQRS == TipoPQRS.Novedad)
                    {
                        var novedad = await db.Novedad.Where(d => d.Id == DataId).FirstOrDefaultAsync();
                        if (novedad != null)
                        {
                            novedad.Estado = EstadoFormatoPQRS.Completed;
                            db.Entry(novedad).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                    }//if (TipoPQRS == TipoPQRS.Devolucion)

                }//if (flagComplete)
            }
            catch
            {
                result = false;
            }

            return result;
        }


        public void SendNotificationEmailTask(TipoNotificacion tipoNotificacion, TipoComentario tipoComentario, TipoPQRS tipoPQRS,
                                               int RecordId, int Order, int DataId, int CommentId, string UsuarioNombre, string AppLink)
        {
            //return;

            var formato = new Formato();
            try
            {
                if (tipoPQRS == TipoPQRS.Devolucion)
                {
                    formato = db.Devoluciones.Include(d => d.UsuarioCreacion).Include(d => d.Cliente).Include(d => d.Analista).Where(d => d.Id == DataId)
                                        .Select(d => new Formato
                                        {
                                            TipoPQRS = tipoPQRS,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            UsuarioCreacionCorreo = d.UsuarioCreacion.UsuarioCorreo,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = TipoPersona.Otro,
                                            Persona = "",
                                            ClienteId = d.ClienteId,
                                            ClienteRazonSocial = d.Cliente.ClienteRazonSocial,
                                            AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                            AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = (d.Destinatarios == null) ? "" : d.Destinatarios,
                                            NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                        }).FirstOrDefault();


                }
                else if (tipoPQRS == TipoPQRS.Garantia)
                {
                    formato = db.Garantias.Include(d => d.UsuarioCreacion).Include(d => d.Cliente).Include(d => d.Analista).Where(d => d.Id == DataId)
                                            .Select(d => new Formato
                                            {
                                                TipoPQRS = tipoPQRS,
                                                Id = d.Id,
                                                FechaCreacion = d.FechaCreacion,
                                                UsuarioCreacion = d.UsuarioCreacion.UsuarioNombre,
                                                UsuarioCreacionCorreo = d.UsuarioCreacion.UsuarioCorreo,
                                                Estado = d.Estado,
                                                Prioridad = Prioridad.Baja,
                                                TipoPersona = TipoPersona.Otro,
                                                Persona = "",
                                                ClienteId = d.ClienteId,
                                                ClienteRazonSocial = d.Cliente.ClienteRazonSocial,
                                                AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                                AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                                Asunto = d.Asunto,
                                                Observacion = d.Observacion,
                                                Destinatarios = (d.Destinatarios == null) ? "" : d.Destinatarios,
                                                NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                            }).FirstOrDefault();

                }
                else if (tipoPQRS == TipoPQRS.Novedad)
                {
                    formato = db.Novedad.Include(d => d.UsuarioCreacion).Include(d => d.Cliente).Include(d => d.Analista).Where(d => d.Id == DataId)
                                            .Select(d => new Formato
                                            {
                                                TipoPQRS = tipoPQRS,
                                                Id = d.Id,
                                                FechaCreacion = d.FechaCreacion,
                                                UsuarioCreacion = d.UsuarioCreacion.UsuarioNombre,
                                                UsuarioCreacionCorreo = d.UsuarioCreacion.UsuarioCorreo,
                                                Estado = d.Estado,
                                                Prioridad = Prioridad.Baja,
                                                TipoPersona = d.TipoPersona,
                                                Persona = d.Persona,
                                                ClienteId = d.ClienteId,
                                                ClienteRazonSocial = d.Cliente.ClienteRazonSocial,
                                                AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                                AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                                Asunto = d.Asunto,
                                                Observacion = d.Observacion,
                                                Destinatarios = (d.Destinatarios == null) ? "" : d.Destinatarios,
                                                NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                            }).FirstOrDefault();
                }
                else if (tipoPQRS == TipoPQRS.Recruitment)
                {
                    formato = db.Recruitments.Include(d => d.UsuarioCreacion).Include(d => d.RecruitmentId).Include(d => d.Analista).Where(d => d.RecruitmentId == DataId)
                                            .Select(d => new Formato
                                            {
                                                TipoPQRS = tipoPQRS,
                                                Id = d.RecruitmentId,
                                                UsuarioCreacion = d.UsuarioCreacion.UsuarioNombre,
                                                UsuarioCreacionCorreo = d.UsuarioCreacion.UsuarioCorreo,
                                                Estado = d.Estado,
                                                Prioridad = Prioridad.Baja,
                                                AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                                AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                                NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                            }).FirstOrDefault();
                }
                List<Mails> mails = new List<Mails>();

                //Persona que creo el PQRS
                mails.Add(new Mails { to = formato.UsuarioCreacionCorreo, toName = formato.UsuarioCreacion });

                //Analista del PQSRa
                if (formato.AnalistaCorreo != "" && formato.AnalistaCorreo != null)
                    mails.Add(new Mails { to = formato.AnalistaCorreo, toName = formato.AnalistaId });

                string subject = "AIS - ";
                subject += $"[{formato.NroTracking}]: {formato.Asunto}  ({tipoNotificacion.ToString()})";
                string msg = $"The user <b>{UsuarioNombre}</b> ";
                string action = "Details";


                bool flagDestinatarios = false;
                //Primer paso ( se crear el ticket de PQRS)
                if (tipoNotificacion == TipoNotificacion.CreateFormat)
                {
                    msg += $"has created a new PQRS({formato.TipoPQRS.ToString()})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";
                    flagDestinatarios = true;
                }
                else if (tipoNotificacion == TipoNotificacion.Assign)
                {
                    msg += $"has assigned a new CS analyst ({formato.AnalistaId})";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    msg += $"<b>Reason</b>: {step.MotivoPQRSNombre}<br />";
                    var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                    msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                    msg += $"<b>Comment</b>: {comment.Comentario}<br />";
                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;
                }
                else if (tipoNotificacion == TipoNotificacion.Comment)
                {
                    msg += $"has added a new comment";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";

                    var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                    msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                    msg += $"<b>Comment</b>: {comment.Comentario}<br />";


                }
                else if (tipoNotificacion == TipoNotificacion.CurrentStep)
                {
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
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
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

                }
                else if (tipoNotificacion == TipoNotificacion.NextStep)
                {
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefaultAsync();
                    msg = $"You have pending an action to do in system, please log in here to continue";
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
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

                }
                else if (tipoNotificacion == TipoNotificacion.Close)
                {
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    msg = $"has closed  <b>Reason PQRS:</b> {step.MotivoPQRSNombre}";
                    var comment = db.PQRSRecordComentarios.Where(c => c.Id == CommentId).FirstOrDefault();
                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";
                    msg += $"<b>Date Comment</b>: {comment.FechaCreacion}<br />";
                    msg += $"<b>Comment</b>: {comment.Comentario}<br />";
                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;

                }
                else if (tipoNotificacion == TipoNotificacion.Complete)
                {
                    var step = db.PQRSRecords.Where(r => r.Id == RecordId && r.Order == Order).FirstOrDefault();
                    msg += $"has completed  ";

                    msg += $"<br /><br /><b>PQRS Information</b><br /><br />";
                    msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                    msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                    msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                    msg += $"<b>Note</b>: {formato.Observacion}<br />";

                    flagDestinatarios = step.EnviaCorreoDestinatarios ?? false;

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
                    Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, msg, AppLink));
                }
            }
            catch{
                return;
            }
            // await Task.Run(() => Fn.SendHtmlEmail(to, toName, subject, msg));
        }

        [HttpGet]
        [AllowAnonymous]
        public string PQRSNotification()
        {


            //var config = await db.Configuracion.FirstOrDefaultAsync();
            string sqlData = $"SELECT DISTINCT TOP 6 PQRS.Id,PQRS.PQRSRecordId,PQRS.PQRSRecordOrder, C.[Days] " +
                             "FROM PQRSView AS PQRS " +
                             "  INNER JOIN (SELECT DISTINCT PQRSRecordId, MIN(DATEDIFF(dd, FechaCreacion, GETDATE())) as [Days] " +
                             "              FROM PQRSRecordComentario " +
                             "              GROUP BY PQRSRecordId) AS C " +
                             "     ON PQRS.PQRSRecordId = C.PQRSRecordId AND C.[Days] >= 3 " +
                             "WHERE PQRS.TipoPQRS = 'New' AND PQRS.Estado = 'In Process'";
            var pqrsNew = db.Database.SqlQuery<PQRSPanel>(sqlData).ToList();
            var formato = new Formato();
            foreach (var pqrs in pqrsNew)
            {
                formato = db.Novedad.Include(d => d.UsuarioCreacion).Include(d => d.Cliente).Include(d => d.Analista).Where(d => d.Id == pqrs.Id)
                                        .Select(d => new Formato
                                        {
                                            TipoPQRS = TipoPQRS.Novedad,
                                            Id = d.Id,
                                            FechaCreacion = d.FechaCreacion,
                                            UsuarioCreacion = d.UsuarioCreacion.UsuarioNombre,
                                            UsuarioCreacionCorreo = d.UsuarioCreacion.UsuarioCorreo,
                                            Estado = d.Estado,
                                            Prioridad = Prioridad.Baja,
                                            TipoPersona = d.TipoPersona,
                                            Persona = d.Persona,
                                            ClienteId = d.ClienteId,
                                            ClienteRazonSocial = d.Cliente.ClienteRazonSocial,
                                            AnalistaId = (d.Analista == null) ? "" : d.Analista.UsuarioNombre,
                                            AnalistaCorreo = (d.Analista == null) ? "" : d.Analista.UsuarioCorreo,
                                            Asunto = d.Asunto,
                                            Observacion = d.Observacion,
                                            Destinatarios = (d.Destinatarios == null) ? "" : d.Destinatarios,
                                            NroTracking = (d.NroTracking == null) ? "" : d.NroTracking
                                        }).FirstOrDefault();

                List<Mails> mails = new List<Mails>();

                ////Persona que creo el PQRS
                //mails.Add(new Mails { to = formato.UsuarioCreacionCorreo, toName = formato.UsuarioCreacion });

                //Analista del PQSR
                if (formato.AnalistaCorreo != "" && formato.AnalistaCorreo != null)
                    mails.Add(new Mails { to = formato.AnalistaCorreo, toName = formato.AnalistaId });

                string subject = "AIS - ";
                subject += $"[{formato.NroTracking}]: {formato.Asunto}  (Reminder)";
                string action = "Details";



                var step = db.PQRSRecords.Where(r => r.Id == pqrs.PQRSRecordId && r.Order == pqrs.PQRSRecordOrder).FirstOrDefault();
                string msg = $"No se ha registrado actividad en esta solicitud en los últimos {pqrs.Days} días, " +
                             $"el paso actual es {step.FlujoPQRSNombre}.Por favor realizar la acción actual para mantener actualizada la solicitud.";

                msg += $"<br/><br/> We have not detect any activity on this request over the last {pqrs.Days} days, " +
                      $"the current step is {step.FlujoPQRSNombre}. Please perform the current action in order to keep the request updated.";


                msg += $"<br/><br /><b>PQRS Information</b><br /><br />";
                msg += $"<b>Customer</b>: {formato.ClienteId} - {formato.ClienteRazonSocial}<br />";
                msg += $"<b>Date</b>: {formato.FechaCreacion}<br />";
                msg += $"<b>Priority</b>: {formato.Prioridad.ToString()}<br />";
                msg += $"<b>Note</b>: {formato.Observacion}<br />";
                msg += $"<b>Reason</b>: {step.MotivoPQRSNombre}<br />";

                var userStep = db.PQRSRecordUsuarios
                                        .Include(ru => ru.Usuario)
                                        .Where(ru => ru.PQRSRecordId == pqrs.PQRSRecordId && ru.PQRSRecordOrder == pqrs.PQRSRecordOrder).ToList();
                foreach (var u in userStep)
                {
                    mails.Add(new Mails { to = u.Usuario.UsuarioCorreo, toName = u.Usuario.UsuarioNombre });
                }

                //mails.Add(new Mails { to = "carlos.delgado@apextoolgroup.com", toName = "Carlos Delgado" });
                //mails.Add(new Mails { to = "juan.palomino@apextoolgroup.com", toName = "Juan Palomino" });

                msg += $"<br /><br /><a style='color:#22BCE5' href={{url}}/PQRS/{action}?TipoPQRS={TipoPQRS.Novedad}&DataId={pqrs.Id}&RecordId={pqrs.PQRSRecordId}>Click here to view the PQRS.</a>";


                foreach (var m in mails)
                {

#if DEBUG
                    Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, msg, "dev"));

#else
                        Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, msg, "ais"));
#endif

                }

            }
            //Task.Run(() => Fn.SendHtmlEmail("carlos.delgado@apextoolgroup.com", "Carlos", "Joda Manual", "Manual Joda", "apextoolgroup.com.co"));

            return "Enviado";
        }

        //[HttpPost]
        //[ApiAuthorizeAction("PQRS/Upload")]
        //public async Task<IHttpActionResult> Upload(TypeUpLoad typeUpLoad, TipoPQRS tipoPQRS, int Id, int MotivoPQRSId)
        //{
        //    string filePath = "";

        //    try
        //    {
        //        var result = new AjaxResult();

        //        var httpRequest = HttpContext.Current.Request;
        //        if (httpRequest.Files.Count != 1)
        //            return BadRequest("File not given");

        //        var postedFile = httpRequest.Files[0];
        //        var date = DateTime.Now;
        //        filePath = HttpContext.Current.Server.MapPath($"~/UploadFolder/Return_Items_{Seguridadcll.Usuario.UsuarioId}_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}_{Path.GetExtension(postedFile.FileName)}");
        //        postedFile.SaveAs(filePath);

        //        var excel = new ExcelQueryFactory(filePath);

        //        if (!excel.GetWorksheetNames().Any(s => s == "Formato"))
        //            return Ok(result.False("Incorrect given File"));

        //        var rows = (from r in excel.WorksheetRange<PQRSItemRow>("B6", "L16384", "Formato")
        //                    select r).ToList();

        //        var allErrors = new List<string>();
        //        var items = new List<PQRSTimeLineViewModel.FormatoItems>();
        //        var errors = new List<string>();
        //        for (int i = 0; i < rows.Count; i++)
        //        {
        //            var row = rows[i];

        //            if (string.IsNullOrWhiteSpace(row.Product) && string.IsNullOrWhiteSpace(row.Invoice) && row.Qty == 0 && row.Price == 0)
        //                continue; // omitir linea en blanco



        //            int line = i + 7;


        //            var motivoPQRS = await db.MotivosPQRS.Where(m => m.Id == MotivoPQRSId).FirstOrDefaultAsync();

        //            var producto = await db.Item.Where(item => item.Codigo.Trim().ToLower() == row.Product.Trim().ToLower()).FirstOrDefaultAsync();
        //            if (producto == null)
        //            {
        //                AddValidationError(errors, $"Field {FileFields.Product} is not exist", line);
        //            }
        //            else
        //            {
        //                int ModUnidadEmpaque = row.Qty % (producto.UnidadEmpaque ?? 1);
        //                if (ModUnidadEmpaque != 0)
        //                    AddValidationError(errors, $"Field {FileFields.Qty} must be a multiple of the unit packing {producto.UnidadEmpaque}", line);
        //            }

        //            if (row.Qty == 0)
        //                AddValidationError(errors, $"Field {FileFields.Qty} is required", line);

        //            if (row.Price == 0)
        //                AddValidationError(errors, $"Field {FileFields.Price} is required", line);
        //            if (string.IsNullOrWhiteSpace(row.Invoice))
        //                AddValidationError(errors, $"Field {FileFields.Invoice} is required", line);


        //            if (typeUpLoad == TypeUpLoad.FillFormat)
        //            {
        //                if (string.IsNullOrWhiteSpace(row.Status))
        //                    AddValidationError(errors, $"Field {FileFields.Status} is required", line);

        //                if (string.IsNullOrWhiteSpace(row.SupportDocument))
        //                    AddValidationError(errors, $"Field {FileFields.SupportDocument} is required", line);

        //                if (row.ReceivedQty == 0)
        //                    AddValidationError(errors, $"Field {FileFields.ReceivedQty} is required", line);
        //                if (row.LoadedQty == 0)
        //                    AddValidationError(errors, $"Field {FileFields.LoadedQty} is required", line);
        //            }

        //            if (errors.Count == 0)
        //            {
        //                //if (typeUpLoad == TypeUpLoad.Insert)
        //                //{
        //                var item = new PQRSTimeLineViewModel.FormatoItems();
        //                item.Items = producto;
        //                item.ItemId = producto.Id;
        //                item.Cantidad = row.Qty;
        //                item.Precio = row.Price;
        //                item.NroFactura = row.Invoice;
        //                item.NroGuia = row.TrackingNumber;
        //                item.MotivoPQRSId = MotivoPQRSId;
        //                item.MotivoPQRS = motivoPQRS;
        //                item.ComentarioAdicional = row.Comments;
        //                item.Estado = (row.Status == "Approved") ? EstadoFormatoItemPQRS.Approved : EstadoFormatoItemPQRS.Rejected;
        //                item.CantidadRecibida = row.ReceivedQty;
        //                item.CantidadSubida = row.LoadedQty;
        //                item.DocSoporte = row.SupportDocument;
        //                item.ComentarioEstadoMercancia = row.StatusComment;

        //                items.Add(item);
        //                //}else if (typeUpLoad == TypeUpLoad.Update)
        //                //{

        //                //}
        //            }
        //            else
        //                allErrors.AddRange(errors);


        //        }


        //        if (allErrors.Count > 0)
        //            return Ok(result.False("validation", allErrors));



        //        return Ok(result.True("success", items));

        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //    finally
        //    {
        //        if (File.Exists(filePath))
        //            File.Delete(filePath);
        //    }
        //    //return BadRequest("Empty process");
        //}


        //private void AddValidationError(List<string> errors, string error, int line)
        //{
        //    errors.Add($"Line {line}: {error}.");
        //}

        //Class and Enums

        static class FileFields
        {
            public const string Product = "PRODUCT";
            public const string Qty = "QTY";
            public const string Price = "PRICE";
            public const string Invoice = "INVOICE";
            public const string TrackingNumber = "TRACKING NUMBER";
            public const string Comments = "COMMENTS";
            public const string Status = "STATUS";
            public const string SupportDocument = "SUPPORT DOCUMENT";
            public const string ReceivedQty = "RECEIVED QTY";
            public const string LoadedQty = "LOADED QTY";
            public const string StatusComment = "STATUS COMMENT";
        }

        //class PQRSItemRow
        //{
        //    [ExcelColumn(FileFields.Product)]
        //    public string Product { get; set; }

        //    [ExcelColumn(FileFields.Qty)]
        //    public int Qty { get; set; }

        //    [ExcelColumn(FileFields.Price)]
        //    public decimal Price { get; set; }

        //    [ExcelColumn(FileFields.Invoice)]
        //    public string Invoice { get; set; }

        //    [ExcelColumn(FileFields.TrackingNumber)]
        //    public string TrackingNumber { get; set; }

        //    [ExcelColumn(FileFields.Comments)]
        //    public string Comments { get; set; }

        //    [ExcelColumn(FileFields.Status)]
        //    public string Status { get; set; }

        //    [ExcelColumn(FileFields.SupportDocument)]
        //    public string SupportDocument { get; set; }

        //    [ExcelColumn(FileFields.ReceivedQty)]
        //    public int ReceivedQty { get; set; }

        //    [ExcelColumn(FileFields.LoadedQty)]
        //    public int LoadedQty { get; set; }

        //    [ExcelColumn(FileFields.StatusComment)]
        //    public string StatusComment { get; set; }
        //}

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

        private class Formato
        {
            public TipoPQRS TipoPQRS { get; set; }
            public int Id { get; set; }
            public DateTime FechaCreacion { get; set; }
            public string UsuarioCreacion { get; set; }
            public string UsuarioCreacionCorreo { get; set; }
            public EstadoFormatoPQRS Estado { get; set; }
            public Prioridad Prioridad { get; set; }
            public TipoPersona TipoPersona { get; set; }
            public string Persona { get; set; }
            public string ClienteId { get; set; }
            public string ClienteRazonSocial { get; set; }
            public string AnalistaId { get; set; }
            public string AnalistaCorreo { get; set; }
            public string Asunto { get; set; }
            public string Observacion { get; set; }
            public string Destinatarios { get; set; }
            public string NroTracking { get; set; }

        }

        private class Mails
        {
            public string to { get; set; }
            public string toName { get; set; }
        }

        public enum TypeUpLoad
        {
            Insert = 1,
            Update = 2,
            FillFormat = 3
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
