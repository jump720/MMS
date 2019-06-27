using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;
using MMS.Classes;
namespace MMS.ApiControllers.MMS
{
    public class VentasxClientesController : ApiBaseController
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





                var clientes = Seguridadcll.ClienteList.Select(c => c.ClienteID).ToArray();

                int count = await db.VentasxCliente                    
                    .Include(p => p.cliente)
                    .Where(a =>  clientes.Contains(a.ClienteID) && (a.VentasxClienteAno.ToString().Contains(search) || a.VentasxClienteMes.ToString().Contains(search)
                                                                    || (a.ClienteID + a.cliente.ClienteRazonSocial).Contains(search)))
                    .Select(p => new
                    {
                        p.ClienteID,
                        p.cliente.ClienteRazonSocial,
                        p.VentasxClienteAno,
                        p.VentasxClienteMes,                       
                        p.VentasxClienteVenta
                    })
                    .Distinct()
                    .CountAsync();



                //Pegar a variavel Sum ( na próxima vista//
                var sum =
                     "SELECT SUM(VentasxClienteMes) FROM VentasxCliente";



                var data = await db.VentasxCliente
                    .Include(p => p.cliente)
                    .Where(a => clientes.Contains(a.ClienteID) && (a.VentasxClienteAno.ToString().Contains(search) || a.VentasxClienteMes.ToString().Contains(search)
                                                                    || (a.ClienteID + a.cliente.ClienteRazonSocial).Contains(search)))
                    .Select(p => new
                    {
                        p.ClienteID,
                        p.cliente.ClienteRazonSocial,
                        p.VentasxClienteAno,
                        p.VentasxClienteMes,
                        p.VentasxClienteVenta
                    }) 
                    .Distinct()
                    .OrderBy(a => a.ClienteID)
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

    }
}
