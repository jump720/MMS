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
namespace MMS.ApiControllers.Catalogos
{
    public class TipoProductosController : ApiBaseController
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

                int count = await db.TipoProductos
                    .Where(ta => ta.TipoProductoID.Contains(search) || ta.TipoProductoDesc.Contains(search))
                    .CountAsync();

                var data = await db.TipoProductos
                    .Where(ta => ta.TipoProductoID.Contains(search) || ta.TipoProductoDesc.Contains(search))
                    .Select(ta => new { ta.TipoProductoID, ta.TipoProductoDesc })
                    .OrderBy(ta => ta.TipoProductoID)
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
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Create(TipoProducto tipoProducto)
        {
            try
            {
                var result = new AjaxResult();

                var tipoProductoTemp = await db.TipoProductos.Where(u => u.TipoProductoID == tipoProducto.TipoProductoID).FirstOrDefaultAsync();
                if (tipoProductoTemp == null)
                {
                    db.TipoProductos.Add(tipoProducto);
                    await db.SaveChangesAsync();
                    AddLog("", tipoProducto.TipoProductoID, tipoProducto);
                }
                else
                {
                    return Ok(result.False("Type of product already exists"));
                }


                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(TipoProducto tipoProducto)
        {
            try
            {
                var result = new AjaxResult();

                db.Entry(tipoProducto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", tipoProducto.TipoProductoID, tipoProducto);

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                var result = new AjaxResult();
                var tipoProductos = await db.TipoProductos.FindAsync(id);
                if (tipoProductos == null)
                    return NotFound();

                var productos = await db.Producto.Where(g => g.TipoProductoID == id).ToListAsync();

                if (productos.Count == 0)
                {
                    db.TipoProductos.Remove(tipoProductos);
                    await db.SaveChangesAsync();
                    AddLog("", tipoProductos.TipoProductoID, tipoProductos);
                }
                else
                {
                    return Ok(result.False("Type of product is related with some records"));
                }


                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
