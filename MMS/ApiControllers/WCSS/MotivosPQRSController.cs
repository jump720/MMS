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

namespace MMS.ApiControllers.WCSS
{
    public class MotivosPQRSController : ApiBaseController
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

                string tipopqrs = form["_tipopqrs"];

                var countQuery = db.MotivosPQRS
                    .Where(m => m.Nombre.Contains(search) || m.Id.ToString().Contains(search) || m.TipoPQRS.ToString().Contains(search));


                var dataQuery = db.MotivosPQRS
                    .Where(m => m.Nombre.Contains(search) || m.Id.ToString().Contains(search) || m.TipoPQRS.ToString().Contains(search));


                if (!string.IsNullOrWhiteSpace(tipopqrs))
                {
                    int value = int.Parse(tipopqrs);
                    countQuery = countQuery.Where(q => q.TipoPQRS == (TipoPQRS)value);
                    dataQuery = dataQuery.Where(q => q.TipoPQRS == (TipoPQRS)value);
                }

                int count = await countQuery.CountAsync();

                var data = await dataQuery
                    .Select(m => new
                    {
                        m.Id,
                        TipoPQRS = (m.TipoPQRS == TipoPQRS.Devolucion) ? "Return" : (m.TipoPQRS == TipoPQRS.Garantia) ? "Guarantee" : (m.TipoPQRS == TipoPQRS.Recruitment) ? "Recruitment" : "New" ,
                        m.Nombre,
                        m.Activo
                    })
                    .OrderBy(m => m.Id).Skip(displayStart).Take(displayLength).ToListAsync();

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
        public async Task<IHttpActionResult> BuscarMotivoPQRS(string q, TipoPQRS t)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<MotivoPQRS>());

                return Ok(await db.MotivosPQRS
                   .Where(i => (i.Nombre.Contains(q)) && i.TipoPQRS == t && i.Activo == true)
                    .Select(i => new
                    {
                        i.Id,
                        Nombre = i.Id + " - " + i.Nombre
                    })
                    .Take(50)
                    .ToListAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetMotivoPQRS(int id)
        {
            try
            {

                return Ok(await db.MotivosPQRS
                   .Where(i => i.Id == id)
                    .Select(i => new
                    {
                        i.Id,
                        Nombre = i.Id + " - " + i.Nombre
                    })
                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> MotivoPQRSList(TipoPQRS tipoPQRS)
        {
            var motivo = await db.MotivosPQRS.Where(m => m.TipoPQRS == tipoPQRS && m.Activo == true)
                                .Select(m => new { MotivoString = m.Id.ToString() + m.Nombre })
                                .ToListAsync();

            return Ok(motivo);
        }
    }
}
