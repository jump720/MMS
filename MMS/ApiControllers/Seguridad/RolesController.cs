using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MMS.Filters;
using MMS.Models;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;

namespace MMS.ApiControllers.Seguridad
{
    public class RolesController : ApiBaseController
    {

        private MMSContext db = new MMSContext();


        


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.Roles
                    .Where(a => a.RolNombre.Contains(search) || a.RolId.ToString().Contains(search))
                    .CountAsync();

                var data = await db.Roles
                    .Where(r => r.RolNombre.Contains(search) || r.RolId.ToString().Contains(search))
                    .Select(r => new { r.RolId, r.RolNombre })
                    .OrderBy(r => r.RolId)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

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
        }

        [HttpPost]
        public async Task<IHttpActionResult> RolAplicaciones(RolAplicacionViewModel model)
        {
            try
            {
                if (await DeleteRolAplicaciones(model.rol.RolId))

                    foreach (var apps in model.aplicaciones)
                    {
                        if (apps.Seleccionado)
                            db.RolAplicaciones.Add(new RolAplicacion { RolId = model.rol.RolId, AplicacionId = apps.Id });
                    }

                await db.SaveChangesAsync();


                //Inserta Auditoria
                Controllers.Seguridad.Seguridad seguridad = new Controllers.Seguridad.Seguridad();
                Auditoria auditoria = new Auditoria();
                Seguridadcll seguridadcll = (Seguridadcll)HttpContext.Current.Session["seguridad"];

                auditoria.AuditoriaFecha = System.DateTime.Now;
                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                auditoria.AuditoriaEvento = "Add_Apps";
                auditoria.AuditoriaDesc = "Add_Apps : " + model.rol.RolId;
                auditoria.ObjetoId = "Roles/RolAplicaciones";

                seguridad.insertAuditoria(auditoria);
                //Inserta Auditoria

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        private async Task<bool> DeleteRolAplicaciones(int id)
        {
            bool result = true;
            try
            {
                var ra = await db.RolAplicaciones.Where(r => r.RolId == id).ToListAsync();
                db.RolAplicaciones.RemoveRange(ra);
                await db.SaveChangesAsync();
            }
            catch
            {
                result = false;
            }

            return result;
        }

    }
}
