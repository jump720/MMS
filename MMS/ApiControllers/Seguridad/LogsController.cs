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

namespace MMS.ApiControllers.Seguridad
{
    public class LogsController : ApiBaseController
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

                string fecha = form["_fecha"];
                string usuario = form["_usuario"];
                string eventoId = form["_evento"];
                string key = form["_key"];
                string cliente = form["_cliente"];

                string sqlWhere = "";

                if (fecha.Trim() != "")
                    sqlWhere += "Fecha LIKE '%" + fecha + "%' AND ";

                if (usuario.Trim() != "")
                    sqlWhere += "Usuario LIKE '%" + usuario + "%' AND ";

                if (eventoId.Trim() != "")
                    sqlWhere += "EventoId = " + eventoId + " AND ";

                if (key.Trim() != "")
                    sqlWhere += "[Key] LIKE '%" + key + "%' AND ";

                if (cliente.Trim() != "")
                    sqlWhere += "Cliente LIKE '%" + cliente + "%' AND ";

                if (sqlWhere.Trim() != "")
                    sqlWhere = "WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");

                string sqlCount =
                    "SELECT COUNT(Id) FROM ( " +
                    "SELECT A.Id, CONVERT(nvarchar, A.Fecha, 103) + ' ' + CONVERT(nvarchar, A.Fecha, 108) AS Fecha, A.Usuario, A.EventoId, A.[Key], A.Cliente " +
                    "FROM dbo.Log AS A " +
                    ") AS T " +
                    sqlWhere;

                string sqlData =
                    "SELECT * FROM ( " +
                    "SELECT ROW_NUMBER() OVER (ORDER BY Id DESC) AS Seq, * FROM ( " +
                    "SELECT A.Id, CONVERT(nvarchar, A.Fecha, 103) + ' ' + CONVERT(nvarchar, A.Fecha, 108) AS Fecha, A.Usuario, A.EventoId, E.Descripcion AS Evento, A.[Key], A.Cliente, CASE WHEN A.Data IS NULL THEN CONVERT(BIT, 0) ELSE CONVERT(BIT, 1) END AS HasData " +
                    "FROM dbo.Log AS A " +
                    "INNER JOIN dbo.Evento AS E ON A.EventoId=E.Id " +
                    ") AS T " +
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
        }


        public async Task<IHttpActionResult> Tracking(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                var countQuery = db.Log.Include(l => l.Evento).Select(l => new { l.Id, l.Fecha, l.Usuario, l.EventoId, Evento = l.Evento.Descripcion, l.Key, l.Cliente });
                var dataQuery = db.Log.Include(l => l.Evento).Select(l => new { l.Id, l.Fecha, l.Usuario, l.EventoId, Evento = l.Evento.Descripcion, l.Key, l.Cliente });


                if (!string.IsNullOrWhiteSpace(search))
                {
                    string value = search.Trim();
                    countQuery = countQuery.Where(l => l.Id.ToString().Contains(value) || l.Usuario.Contains(value)
                                                    || l.EventoId.ToString().Contains(value) || l.Evento.Contains(value) || l.Cliente.Contains(value));
                    dataQuery = dataQuery.Where(l => l.Id.ToString().Contains(value) || l.Usuario.Contains(value)
                                                    || l.EventoId.ToString().Contains(value) || l.Evento.Contains(value) || l.Cliente.Contains(value));
                }

                int count = await countQuery.CountAsync();


                var data = (await dataQuery
                                .OrderByDescending(l => l.Id)
                                .Skip(displayStart).Take(displayLength).ToListAsync())
                            .Select(l => new { l.Id, Fecha = l.Fecha.ToShortDateString(), l.Usuario, l.EventoId, Evento = l.Evento, l.Key, l.Cliente }).ToList();


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
        [ApiAuthorizeAction("Logs/Index")]
        public async Task<IHttpActionResult> ViewData(int id)
        {
            try
            {
                return Ok(await db.Log.Where(l => l.Id == id).Select(l => l.Data).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> BuscarEventos(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Evento>());

                return Ok(await db.Evento.Where(e => e.Descripcion.Contains(q)).ToListAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> EventoDescripcion(int id)
        {
            try
            {
                return Ok(await db.Evento.Where(e => e.Id == id).Select(e => e.Descripcion).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Eventos(List<Evento> eventos)
        {
            try
            {
                foreach (var evento in eventos)
                    db.Entry(evento).State = EntityState.Modified;

                await db.SaveChangesAsync();

                AddLog("", "", new { EventosActivos = eventos.Where(e => e.Activo).Select(e => new { e.Id, e.Nombre, e.Descripcion }) });

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
