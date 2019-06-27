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
    public class CiudadController : ApiBaseController
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

                int count = await db.Ciudad
                    .Where(c => c.CiudadDesc.Contains(search) || c.departamentos.DepartamentoDesc.Contains(search) || c.departamentos.paises.PaisDesc.Contains(search))
                    .CountAsync();

                var data = await db.Ciudad
                    .Where(c => c.CiudadDesc.Contains(search) || c.departamentos.DepartamentoDesc.Contains(search) || c.departamentos.paises.PaisDesc.Contains(search))
                    .Select(c => new { c.CiudadID, c.DepartamentoID, c.PaisID, c.CiudadDesc, c.departamentos.DepartamentoDesc, c.departamentos.paises.PaisDesc })
                    .OrderBy(c => c.CiudadID)
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
