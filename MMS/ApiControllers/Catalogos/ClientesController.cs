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

namespace MMS.ApiControllers.Catalogos
{
    public class ClientesController : ApiBaseController
    {
        private MMSContext db = new MMSContext();  
        //Mudança no DataTable
        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.Clientes
                    .Where(c => c.ClienteNit.Contains(search) 
                    || c.ClienteRazonSocial.Contains(search) 
                    || c.ciudad.CiudadDesc.Contains(search) 
                    || c.usuario.UsuarioNombre.Contains(search) 
                    || c.canal.CanalDesc.Contains(search) 
                    || c.ColeccionPIV.Nombre.Contains(search) 
                    || c.CNPJ.Contains(search)
                    || c.MatrizFilial.Contains(search))
                    .CountAsync();

                var data = await db.Clientes
                    .Where(c => c.ClienteNit.Contains(search) 
                    || c.ClienteRazonSocial.Contains(search) 
                    || c.ciudad.CiudadDesc.Contains(search) 
                    || c.usuario.UsuarioNombre.Contains(search) 
                    || c.canal.CanalDesc.Contains(search) 
                    || c.ColeccionPIV.Nombre.Contains(search) 
                    || c.CNPJ.Contains(search)
                    || c.MatrizFilial.Contains(search))

                    .Select(c => new {
                        c.ClienteID,
                        c.ClienteNit,
                        c.ClienteRazonSocial,
                        c.ciudad.CiudadDesc,
                        c.usuario.UsuarioNombre,
                        c.canal.CanalDesc,
                        c.ColeccionPIV.Nombre,
                        c.CNPJ,
                        c.MatrizFilial})
                    .OrderBy(c => c.ClienteID)
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
