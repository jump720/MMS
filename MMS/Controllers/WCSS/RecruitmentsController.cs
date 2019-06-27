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
    public class RecruitmentsController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: Recruitments
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

            var recruitment = await db.Recruitments
                         .FindAsync(id);

            if (recruitment == null)
            {
                return HttpNotFound();
            }

            ViewBag.UsuarioIdSubstitute = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", recruitment.UsuarioIdSubstitute);
            ViewBag.AreaManagerID = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", recruitment.AreaManagerID);
            ViewBag.HumanResourcesID = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", recruitment.HumanResourcesID);
            ViewBag.ImmediateBossID = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", recruitment.ImmediateBossID);
            ViewBag.CentroCostoID = new SelectList(await db.CentroCostos.ToListAsync(), "CentroCostoID", "CentroCostoDesc", recruitment.CentroCostoID);
            ViewBag.ProposedCostCenterID = new SelectList(await db.CentroCostos.ToListAsync(), "CentroCostoID", "CentroCostoDesc", recruitment.ProposedCostCenterID);
            ViewBag.DepartmentId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre", recruitment.DepartmentId);
            ViewBag.ProposedDepartmentId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre", recruitment.ProposedDepartmentId);       

            ViewBag.Sector = new SelectList(Fn.EnumToIEnumarable<Sectors>().ToList(), "Value", "Name", recruitment.Sector);
            ViewBag.Position = new SelectList(Fn.EnumToIEnumarable<Positions>().ToList(), "Value", "Name", recruitment.Position);
            ViewBag.ContractType = new SelectList(Fn.EnumToIEnumarable<ContractTypes>().ToList(), "Value", "Name", recruitment.ContractType);
            ViewBag.Budget = new SelectList(Fn.EnumToIEnumarable<Budgets>().ToList(), "Value", "Name", recruitment.Budget);
            ViewBag.ResignationReason = new SelectList(Fn.EnumToIEnumarable<ResignationReasons>().ToList(), "Value", "Name", recruitment.ResignationReason);
            ViewBag.PreviousNotice = new SelectList(Fn.EnumToIEnumarable<PreviousNotices>().ToList(), "Value", "Name", recruitment.PreviousNotice);
            ViewBag.ExperienceTime = new SelectList(Fn.EnumToIEnumarable<ExperienceTimes>().ToList(), "Value", "Name", recruitment.ExperienceTime);
            ViewBag.Type = new SelectList(Fn.EnumToIEnumarable<Types>().ToList(), "Value", "Name", recruitment.Type);

            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Recruitment && m.Activo == true).ToListAsync();
            ViewBag.CausaPQRS = await db.CausaPQRS.Where(m => m.TipoPQRS == TipoPQRS.Recruitment).ToListAsync();

            return View(GetCrudMode().ToString(), recruitment);
        }



        [AuthorizeAction]
        [FillPermission("Recruitments/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            ViewBag.UsuarioIdSubstitute = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
            ViewBag.AreaManagerID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
            ViewBag.HumanResourcesID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
            ViewBag.ImmediateBossID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
            ViewBag.CentroCostoID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc");
            ViewBag.ProposedCostCenterID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc");
            ViewBag.DepartmentId = new SelectList(db.Area, "Id", "Nombre");
            ViewBag.ProposedDepartmentId = new SelectList(db.Area, "Id", "Nombre");

            var sectors = from Sectors d in Enum.GetValues(typeof(Sectors))
                          select new { ID = (int)d, Name = d.ToString() };

            var positions = from Positions d in Enum.GetValues(typeof(Positions))
                            select new { ID = (int)d, Name = d.ToString() };

            var contractTypes = from ContractTypes d in Enum.GetValues(typeof(ContractTypes))
                                select new { ID = (int)d, Name = d.ToString() };

            var budgets = from Budgets d in Enum.GetValues(typeof(Budgets))
                          select new { ID = (int)d, Name = d.ToString() };

            var resignationReasons = from ResignationReasons d in Enum.GetValues(typeof(ResignationReasons))
                                     select new { ID = (int)d, Name = d.ToString() };

            var previousNotices = from PreviousNotices d in Enum.GetValues(typeof(PreviousNotices))
                                  select new { ID = (int)d, Name = d.ToString() };

            var experienceTimes = from ExperienceTimes d in Enum.GetValues(typeof(ExperienceTimes))
                                  select new { ID = (int)d, Name = d.ToString() };

            var types = from Types d in Enum.GetValues(typeof(Types))
                        select new { ID = (int)d, Name = d.ToString() };

            ViewBag.Sector = new SelectList(sectors, "ID", "Name");
            ViewBag.Position = new SelectList(positions, "ID", "Name");
            ViewBag.ContractType = new SelectList(contractTypes, "ID", "Name");
            ViewBag.Budget = new SelectList(budgets, "ID", "Name");
            ViewBag.ResignationReason = new SelectList(resignationReasons, "ID", "Name");
            ViewBag.PreviousNotice = new SelectList(previousNotices, "ID", "Name");
            ViewBag.ExperienceTime = new SelectList(experienceTimes, "ID", "Name");
            ViewBag.Type = new SelectList(types, "ID", "Name");

            ViewBag.UsuarioId = Seguridadcll.Usuario.UsuarioId;

            return View();
        }



        [HttpPost]
        [AuthorizeAction]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Recruitment model)
        {
            if (ModelState.IsValid)
            {
                model.Estado = EstadoFormatoPQRS.Open;
                model.CreationDate = DateTime.Now;

                db.Recruitments.Add(model);
                await db.SaveChangesAsync();

                AddLog("", model.RecruitmentId, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            ViewBag.UsuarioIdSubstitute = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", model.UsuarioIdSubstitute);
            ViewBag.AreaManagerID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", model.AreaManagerID);
            ViewBag.HumanResourcesID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", model.HumanResourcesID);
            ViewBag.ImmediateBossID = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", model.ImmediateBossID);
            ViewBag.CentroCostoID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc", model.CentroCostoID);
            ViewBag.ProposedCostCenterID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc", model.ProposedCostCenterID);
            ViewBag.DepartmentId = new SelectList(db.Area, "Id", "Nombre", model.DepartmentId);
            ViewBag.ProposedDepartmentId = new SelectList(db.Area, "Id", "Nombre", model.ProposedDepartmentId);

            var sectors = from Sectors d in Enum.GetValues(typeof(Sectors))
                          select new { ID = (int)d, Name = d.ToString() };

            var positions = from Positions d in Enum.GetValues(typeof(Positions))
                            select new { ID = (int)d, Name = d.ToString() };

            var contractTypes = from ContractTypes d in Enum.GetValues(typeof(ContractTypes))
                                select new { ID = (int)d, Name = d.ToString() };

            var budgets = from Budgets d in Enum.GetValues(typeof(Budgets))
                          select new { ID = (int)d, Name = d.ToString() };

            var resignationReasons = from ResignationReasons d in Enum.GetValues(typeof(ResignationReasons))
                                     select new { ID = (int)d, Name = d.ToString() };

            var previousNotices = from PreviousNotices d in Enum.GetValues(typeof(PreviousNotices))
                                  select new { ID = (int)d, Name = d.ToString() };

            var experienceTimes = from ExperienceTimes d in Enum.GetValues(typeof(ExperienceTimes))
                                  select new { ID = (int)d, Name = d.ToString() };

            ViewBag.Sector = new SelectList(sectors, "ID", "Name", model.Sector);
            ViewBag.Position = new SelectList(positions, "ID", "Name", model.Position);
            ViewBag.ContractType = new SelectList(contractTypes, "ID", "Name", model.ContractType);
            ViewBag.Budget = new SelectList(budgets, "ID", "Name", model.Budget);
            ViewBag.ResignationReason = new SelectList(resignationReasons, "ID", "Name", model.ResignationReason);
            ViewBag.PreviousNotice = new SelectList(previousNotices, "ID", "Name", model.PreviousNotice);
            ViewBag.ExperienceTime = new SelectList(experienceTimes, "ID", "Name", model.ExperienceTime);

            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            ViewBag.UsuarioNombre = seguridadcll.Usuario.UsuarioId + " - " + seguridadcll.Usuario.UsuarioNombre;
            ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
            ViewBag.MotivoPQRS = await db.MotivosPQRS.Where(m => m.TipoPQRS == TipoPQRS.Recruitment && m.Activo == true).ToListAsync();

            return View(model);

        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }


        [AuthorizeAction]
        public ActionResult Panel()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Recruitment model)
        {
            if (ModelState.IsValid)
            {
                //Guardar Cabecera
                db.Entry(model).State = EntityState.Modified;
                await db.SaveChangesAsync();

                AddLog("", model.RecruitmentId, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            return await GetView(model.RecruitmentId);
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
            var recruitment = await db.Recruitments.FindAsync(id);

            try
            {
                if (recruitment != null)
                {
                    recruitment.Estado = EstadoFormatoPQRS.Deleted;

                    db.Entry(recruitment).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", id, recruitment);
                    return RedirectToAction("Index", GetReturnSearch());
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return await GetView(id);
        }

        [AuthorizeAction]
        [FillPermission("PQRS/AsignarAnalista", "PQRS/AddComment", "PQRS/AddAnswer", "PQRS/Close", "PQRS/UsuarioStep")]
        public async Task<ActionResult> DetailsPanel(TipoPQRS TipoPQRS, int DataId, int RecordId)
        {
            var data = new PQRSTimeLineViewModel();
            if (TipoPQRS == TipoPQRS.Recruitment)
            {
                data.formato = await db.Recruitments
                                    .Include(d => d.UsuarioCreacion)
                                    .Include(d => d.Analista)
                                    .Where(d => d.RecruitmentId == DataId && d.PQRSRecordId == RecordId)
                                    .Select(d => new PQRSTimeLineViewModel.Formato
                                    {
                                        TipoPQRS = TipoPQRS,
                                        Id = d.RecruitmentId,
                                        FechaCreacion = d.CreationDate,
                                        UsuarioIdCreacion = d.UsuarioCreacion.UsuarioNombre,
                                        Estado = d.Estado,
                                        Prioridad = Prioridad.Baja,
                                        TipoPersona = TipoPersona.Otro,
                                        Persona = "",
                                        ClienteId = "",
                                        AnalistaId = d.Analista.UsuarioNombre,
                                        Asunto = d.Appointment,
                                        Observacion = d.Appointment,
                                        Destinatarios = d.Appointment,
                                        NroTracking = d.NroTracking
                                    })
                                    .FirstOrDefaultAsync();

                data.formatoArchivos = new List<PQRSTimeLineViewModel.Comentarios.Archivos>();
                data.formatoItems = new List<PQRSTimeLineViewModel.FormatoItems>();
                //data.formatoArchivos = await db.DevolucionArchivos
                //                             .Where(di => di.DevolucionId == DataId)
                //                             .Select(da => new PQRSTimeLineViewModel.Comentarios.Archivos { Item = da.Order, FileName = da.FileName })
                //                             .ToListAsync();
                //data.formatoItems = await db.DevolucionItems
                //                            .Include(di => di.MotivoPQRS)
                //                            .Include(di => di.Items)
                //                            .Where(di => di.DevolucionId == DataId && di.PQRSRecordId == RecordId)
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

    }

  
}