using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;
using MMS.Classes;
namespace MMS.ApiControllers.PerfectStore
{
    public class VisitasWebController : ApiBaseController
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

                //string fecha = form["_fecha"];
                string nombreestablecimiento = form["_nombreestablecimiento"];
                string tipoindustriaid = form["_tipoindustriaid"];
                string usuarioid = form["_usuarioid"];
                string direccion = form["_direccion"];
                string ubicacion = form["_ubicacion"];
                string administrador = form["_administrador"];
                string tipovisitaid = form["_tipovisitaid"];

                var countQuery = db.Visita.Include(v => v.Usuario)
                                          .Include(v => v.Ciudad.departamentos.paises)
                                          .Include(v => v.TipoVisita)
                                          .Include(v => v.TipoIndustria)
                                          .Where(v => v.UsuarioId == Seguridadcll.Usuario.UsuarioId)
                                          .Select(v => new { v.Id, v.Fecha, v.NombreEstablecimiento, TipoIndustriaId = v.TipoIndustria.Nombre, UsuarioId = v.Usuario.UsuarioNombre, v.Direccion, Ubicacion = v.Ciudad.departamentos.paises.PaisDesc + "-" + v.Ciudad.departamentos.DepartamentoDesc + "-" + v.Ciudad.CiudadDesc, v.Administrador, TipoVisitaId = v.TipoVisita.Nombre, v.Completada });
                var dataQuery = db.Visita.Include(v => v.Usuario)
                                          .Include(v => v.Ciudad.departamentos.paises)
                                          .Include(v => v.TipoVisita)
                                          .Include(v => v.TipoIndustria)
                                          .Where(v => v.UsuarioId == Seguridadcll.Usuario.UsuarioId)
                                          .Select(v => new { v.Id, v.Fecha, v.NombreEstablecimiento, TipoIndustriaId = v.TipoIndustria.Nombre, UsuarioId = v.Usuario.UsuarioNombre, v.Direccion, Ubicacion = v.Ciudad.departamentos.paises.PaisDesc + "-" + v.Ciudad.departamentos.DepartamentoDesc + "-" + v.Ciudad.CiudadDesc, v.Administrador, TipoVisitaId = v.TipoVisita.Nombre, v.Completada });


                //if (!string.IsNullOrWhiteSpace(fecha))
                //{
                //    string value = fecha.Trim();
                //    countQuery = countQuery.Where(id => id.Fecha.ToShortDateString().Contains(value));
                //    dataQuery = dataQuery.Where(id => id.Fecha.ToShortDateString().Contains(value));
                //}

                if (!string.IsNullOrWhiteSpace(nombreestablecimiento))
                {
                    string value = nombreestablecimiento.Trim();
                    countQuery = countQuery.Where(id => id.NombreEstablecimiento.Contains(value));
                    dataQuery = dataQuery.Where(id => id.NombreEstablecimiento.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(tipoindustriaid))
                {
                    string value = tipoindustriaid.Trim();
                    countQuery = countQuery.Where(id => id.TipoIndustriaId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.TipoIndustriaId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(usuarioid))
                {
                    string value = usuarioid.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(direccion))
                {
                    string value = direccion.Trim();
                    countQuery = countQuery.Where(id => id.Direccion.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Direccion.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    string value = ubicacion.Trim();
                    countQuery = countQuery.Where(id => id.Ubicacion.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Ubicacion.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(tipovisitaid))
                {
                    string value = tipovisitaid.Trim();
                    countQuery = countQuery.Where(id => id.TipoVisitaId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.TipoVisitaId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(administrador))
                {
                    string value = administrador.Trim();
                    countQuery = countQuery.Where(id => id.Administrador.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Administrador.Contains(value));
                }

                int count = await countQuery.CountAsync();

                var data = (await dataQuery
                    .OrderByDescending(a => a.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync())
                    .Select(v => new { v.Id, Fecha = v.Fecha.ToShortDateString(), v.NombreEstablecimiento, v.TipoIndustriaId, v.UsuarioId, v.Direccion, v.Ubicacion, v.Administrador, v.TipoVisitaId,v.Completada }).ToList();



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

        [HttpGet]
        public IHttpActionResult GetDV(string nit)
        {
            var result = new AjaxResult();

            var dv = Fn.GetDv(nit);
            return Ok(result.True("",dv));
        }

        [HttpPost]
        public async Task<IHttpActionResult> BuscarEstablecimiento(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);

                //string fecha = form["_fecha"];
                string nombreestablecimiento = form["_nombreestablecimiento"];
                string usuarioid = form["_usuarioid"];
                string direccion = form["_direccion"];
                string ubicacion = form["_ubicacion"];

                var countQuery = db.Visita.Include(v => v.Usuario)
                                          .Include(v => v.Ciudad.departamentos.paises)
                                          .Include(v => v.TipoVisita)
                                          .Include(v => v.TipoIndustria)
                                          .Select(v => new { v.Id, v.Fecha, v.NombreEstablecimiento, TipoIndustriaId = v.TipoIndustria.Nombre, UsuarioId = v.Usuario.UsuarioNombre, v.Direccion, Ubicacion = v.Ciudad.departamentos.paises.PaisDesc + "-" + v.Ciudad.departamentos.DepartamentoDesc + "-" + v.Ciudad.CiudadDesc, v.Administrador, TipoVisitaId = v.TipoVisita.Nombre });
                var dataQuery = db.Visita.Include(v => v.Usuario)
                                          .Include(v => v.Ciudad.departamentos.paises)
                                          .Include(v => v.TipoVisita)
                                          .Include(v => v.TipoIndustria)
                                          .Select(v => new { v.Id, v.Fecha, v.NombreEstablecimiento, TipoIndustriaId = v.TipoIndustria.Nombre, UsuarioId = v.Usuario.UsuarioNombre, v.Direccion, Ubicacion = v.Ciudad.departamentos.paises.PaisDesc + "-" + v.Ciudad.departamentos.DepartamentoDesc + "-" + v.Ciudad.CiudadDesc, v.Administrador, TipoVisitaId = v.TipoVisita.Nombre });


              if (!string.IsNullOrWhiteSpace(nombreestablecimiento))
                {
                    string value = nombreestablecimiento.Trim();
                    countQuery = countQuery.Where(id => id.NombreEstablecimiento.Contains(value));
                    dataQuery = dataQuery.Where(id => id.NombreEstablecimiento.Contains(value));
                }               

                if (!string.IsNullOrWhiteSpace(usuarioid))
                {
                    string value = usuarioid.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(direccion))
                {
                    string value = direccion.Trim();
                    countQuery = countQuery.Where(id => id.Direccion.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Direccion.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    string value = ubicacion.Trim();
                    countQuery = countQuery.Where(id => id.Ubicacion.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Ubicacion.Contains(value));
                }

          
                int count = await countQuery.Distinct().CountAsync();

                var data = (await dataQuery.Distinct()
                    .OrderByDescending(a => a.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync())
                    .Select(v => new { v.Id, Fecha = v.Fecha.ToShortDateString(), v.NombreEstablecimiento, v.TipoIndustriaId, v.UsuarioId, v.Direccion, v.Ubicacion, v.Administrador, v.TipoVisitaId }).ToList();



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


        public async Task<IHttpActionResult> SetComplete(int id)
        {
            var result = new AjaxResult();

            var visita = await db.Visita.FindAsync(id);

            if (visita == null)
                return Ok(result.False("Warning, Visit not found"));

            if (!visita.Completada)
            {

                

                visita.Completada = true;
                db.Entry(visita).State = EntityState.Modified;
                await db.SaveChangesAsync();

                #region SendMail
                var usuarioJefe = await db.Usuarios.Include(u => u.UsuarioPadre).Where(u => u.UsuarioId == visita.UsuarioId).FirstOrDefaultAsync();


                List<Mails> mails = new List<Mails>();
                mails.Add(new Mails { to = usuarioJefe.UsuarioPadre.UsuarioCorreo, toName = usuarioJefe.UsuarioPadre.UsuarioNombre });
                //mails.Add(new Mails { to = "carlos.delgado@apextoolgroup.com", toName = "KIKE" });


                string subject = "AIS - ";
                subject += $"Visit [{visita.Id} - {visita.NombreEstablecimiento}] has been completed )";
                string msg = $"The user <b>{usuarioJefe.UsuarioNombre}</b> completed the visit";
                string action = "Details";

                msg += $"<br /><br /><a style='color:#22BCE5' href={{url}}/Visitas/{action}/{visita.Id}>Click here to view the Visit.</a>";

                string appLink = Seguridadcll.Aplicacion.Link;
                foreach (var m in mails)
                {
                    Task.Run(() => Fn.SendHtmlEmail(m.to, m.toName, subject, msg, appLink));
                }

                #endregion
            }

            return Ok(result.True("the visit has been completed"));
        }

        private class Mails
        {
            public string to { get; set; }
            public string toName { get; set; }
        }

    }
}
