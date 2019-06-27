using LinqToExcel;
using LinqToExcel.Attributes;
using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.Catalogos.Ubicacion
{
    public class DepartamentoController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpGet]
        public async Task<IHttpActionResult> Ciudades(string id, string paisId)
        {
            try
            {
                var ciudades = await db.Ciudad.OrderBy(c => c.CiudadDesc)
                    .Where(c => c.PaisID == paisId && c.DepartamentoID == id)
                    .Select(c => new
                    {
                        Id = c.CiudadID,
                        Nombre = c.CiudadDesc
                    }).ToListAsync();

                return Ok(ciudades);
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

                int count = await db.Departamento
                    .Where(d => d.DepartamentoDesc.Contains(search) || d.paises.PaisDesc.Contains(search))
                    .CountAsync();

                var data = await db.Departamento
                    .Where(d => d.DepartamentoDesc.Contains(search) || d.paises.PaisDesc.Contains(search))
                    .Select(d => new { d.DepartamentoID, d.PaisID, d.DepartamentoDesc, d.paises.PaisDesc})
                    .OrderBy(d => d.DepartamentoID)
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
