using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.Catalogos
{
    public class PaisController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpGet]
        public async Task<IHttpActionResult> Departamentos(string id)
        {
            try
            {
                var departamentos = await db.Departamento.OrderBy(d => d.DepartamentoDesc)
                    .Where(d => d.PaisID == id)
                    .Select(d => new
                    {
                        Id = d.DepartamentoID,
                        Nombre = d.DepartamentoDesc
                    }).ToListAsync();

                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.Pais
                    .Where(p => p.PaisID.Contains(search) || p.PaisDesc.Contains(search))
                    .CountAsync();

                var data = await db.Pais
                    .Where(p => p.PaisID.Contains(search) || p.PaisDesc.Contains(search))
                    .Select(p => new { p.PaisID, p.PaisDesc })
                    .OrderBy(p => p.PaisID)
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


        [HttpGet]
        public async Task<IHttpActionResult> BuscarPais(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Ok(new List<Pais>());

                return Ok(await db.Pais
                   .Where(p => (p.PaisID.Contains(id)) && p.PaisID.Contains(id))
                    .Select(p => new
                    {
                        p.PaisID,
                        p.PaisDesc
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
        public async Task<IHttpActionResult> GetPais(string id)
        {
            try
            {

                return Ok(await db.Pais
                   .Where(p => p.PaisID == id)
                    .Select(p => new
                    {
                        p.PaisID,
                        p.PaisDesc
                    })
                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
