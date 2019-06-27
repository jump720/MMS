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
