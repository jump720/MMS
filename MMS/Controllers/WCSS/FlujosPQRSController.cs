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
    public class FlujosPQRSController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: FlujosPQRS
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {

            var flujo = await db.FlujosPQRS
                                .Include(f => f.MotivoPQRS)
                                .Where(f => f.MotivoPQRSId == id)
                                .OrderBy(f => f.Order)
                                .ToListAsync();

            if (flujo.Count == 0)
            {
                return HttpNotFound();
            }

            return View(GetCrudMode().ToString(), new FlujoPQRSViewModel { Flujo = flujo });
        }


        [AuthorizeAction]
        [FillPermission("FlujosPQRS/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> CreateStep(int MotivoPQRSId)
        {
            var motivo = await db.MotivosPQRS.FindAsync(MotivoPQRSId);
            if (motivo == null)
                return HttpNotFound();

            ViewBag.MotivoNombre = motivo.Nombre;
            return PartialView("_CreateStep", new FlujoPQRS { MotivoPQRSId = motivo.Id });
        }

        [AuthorizeAction]
        public async Task<ActionResult> EditStep(int MotivoPQRSId, int Id)
        {

            var motivo = await db.MotivosPQRS.FindAsync(MotivoPQRSId);
            if (motivo == null)
                return HttpNotFound();

            var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId && f.Id == Id).FirstOrDefaultAsync();
            if (flujo == null)
                return HttpNotFound();

            ViewBag.MotivoNombre = motivo.Nombre;
            return PartialView("_EditStep", flujo);
        }

        [AuthorizeAction]
        public async Task<ActionResult> DeleteStep(int MotivoPQRSId, int Id)
        {

            var motivo = await db.MotivosPQRS.FindAsync(MotivoPQRSId);
            if (motivo == null)
                return HttpNotFound();

            var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId && f.Id == Id).FirstOrDefaultAsync();
            if (flujo == null)
                return HttpNotFound();

            ViewBag.MotivoNombre = motivo.Nombre;
            return PartialView("_DeleteStep", flujo);
        }


        [AuthorizeAction]
        public async Task<ActionResult> UsuariosStep(int MotivoPQRSId, int Id)
        {



            var flujo = await db.FlujosPQRS
                                .Where(f => f.MotivoPQRSId == MotivoPQRSId && f.Id == Id).FirstOrDefaultAsync();
            if (flujo == null)
                return HttpNotFound();

            string sqlquery = $"select isnull(uf.MotivoPQRSId,{MotivoPQRSId}) as MotivoPQRSId ,isnull(uf.FlujoPQRSId,{Id}) as FlujoPQRSId,u.UsuarioId,u.UsuarioNombre, case when uf.UsuarioId is null then convert(bit,0) else convert(bit,1) end as [check] from Usuario as u left join UsuarioFlujoPQRS as uf " +
                               $"on u.UsuarioId = uf.UsuarioId and uf.MotivoPQRSId = {MotivoPQRSId} and uf.FlujoPQRSId = {Id} " +
                               $"where u.Usuarioactivo = 1";
            List<UsuarioFlujoPQRSViewModel.UsuariosStep> usuarioList = await db.Database.SqlQuery<UsuarioFlujoPQRSViewModel.UsuariosStep>(sqlquery).ToListAsync();

            ViewBag.EtapaNombre = flujo.Nombre;
            return PartialView("_UsuariosStep", new UsuarioFlujoPQRSViewModel { UsuariosStepList = usuarioList });
        }


        [AuthorizeAction]
        public async Task<ActionResult> ConfigStep(int MotivoPQRSId, int Id)
        {
            var Flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId && f.Id == Id).FirstOrDefaultAsync();

            var Tareas = await db.FlujoPQRSTareas.Where(ft => ft.MotivoPQRSId == MotivoPQRSId && ft.FlujoPQRSId == Id)
                        .ToListAsync();

            var Condiciones = await db.FlujoPQRSCondiciones.Where(ft => ft.MotivoPQRSId == MotivoPQRSId && ft.FlujoPQRSId == Id)
                        .ToListAsync();


            return PartialView("_ConfigStep", new FlujoPQRSConfigViewModel { Flujo = Flujo, Tareas = Tareas, Condiciones = Condiciones });
        }


        //[AllowAnonymous]
        //public async Task<bool> CreaFlujoMotivoPQRS()
        //{
        //    bool result = true;
        //    var motivos = await db.MotivosPQRS.ToListAsync();

        //    try
        //    {
        //        foreach (var motivo in motivos)
        //        {
        //            var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == motivo.Id).FirstOrDefaultAsync();
        //            if (flujo == null)
        //            {
        //                for (int i = 1; i <= 3; i++)
        //                {
        //                    FlujoPQRS nFlujo = new FlujoPQRS();

        //                    nFlujo.Id = i;
        //                    nFlujo.MotivoPQRSId = motivo.Id;
        //                    nFlujo.Order = i;
        //                    if (i == 1)
        //                    {
        //                        nFlujo.TipoPaso = TipoPaso.LlenarFormato;
        //                        nFlujo.Nombre = "Creación de formato";
        //                    }
        //                    else if (i == 2)
        //                    {
        //                        nFlujo.TipoPaso = TipoPaso.General;
        //                        nFlujo.Nombre = "Asignación de Analista";
        //                    }
        //                    else if (i == 3)
        //                    {
        //                        nFlujo.TipoPaso = TipoPaso.General;
        //                        nFlujo.Nombre = "Cierre o Solución";
        //                    }

        //                    db.FlujosPQRS.Add(nFlujo);

        //                }//for (int i = 1; i <= 3; i++)
        //                await db.SaveChangesAsync();
        //            }//if (flujo == null)
        //        }//foreach (var motivo in motivos)
        //    }
        //    catch
        //    {
        //        result = false;
        //    }


        //    return result;
        //}
    }
}