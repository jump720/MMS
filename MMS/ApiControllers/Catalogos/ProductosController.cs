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
    public class ProductosController : ApiBaseController
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

                int count = await db.Producto
                    .Where(p => 
                    p.CodigoAutoLog.Contains(search) || 
                    p.Grupo.Contains(search) ||
                    p.Departamento.Contains(search) ||
                    p.ProductoDesc.Contains(search) || 
                    p.tipoProducto.TipoProductoDesc.Contains(search))
                    .CountAsync();

                var data = await db.Producto
                     .Where(p => 
                     p.CodigoAutoLog.Contains(search) || 
                     p.ProductoDesc.Contains(search) ||
                     p.Departamento.Contains(search) ||
                     p.Grupo.Contains(search) ||
                     p.tipoProducto.TipoProductoDesc.Contains(search))

                    .Select(p => new {
                        p.ProductoId,
                        p.CodigoAutoLog,
                        p.TipoProductoID,
                        p.Grupo,
                        p.ProductoDesc,
                        p.Departamento,
                        p.SaldoProducto,
                        ValorUnitario = p.ProductoPrecio,
                        ValorTotal = p.ValorTotal,
                        p.tipoProducto.TipoProductoDesc })
                    .OrderBy(p => p.CodigoAutoLog)
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
        public async Task<IHttpActionResult> Create(Producto producto)
        {
            try
            {
                    db.Producto.Add(producto);
                    await db.SaveChangesAsync();
                    AddLog("", producto.CodigoAutoLog, producto);
                    return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Producto producto)
        {
            try
            {
                    db.Entry(producto).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", producto.CodigoAutoLog, producto);

                    return Ok(true);
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
                var producto = await db.Producto.FindAsync(id);
                if (producto == null)
                    return NotFound();

                db.Producto.Remove(producto);
                await db.SaveChangesAsync();
                AddLog("", producto.CodigoAutoLog, producto);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        public async Task<IHttpActionResult> BuscarProductoPOP(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Item>());

                return Ok(await db.Producto                    
                    .Where(i => (i.CodigoAutoLog.Contains(q) || i.ProductoDesc.Contains(q)))
                    .Select(i => new { i.CodigoAutoLog, i.ProductoDesc })
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
        public async Task<IHttpActionResult> GetProductoPOP(string id)
        {
            try
            {
                return Ok(await db.Producto                    
                    .Where(i => i.ProductoId == id)
                    .Select(i => new { i.ProductoId, i.ProductoDesc })
                    .FirstOrDefaultAsync()
                    );
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
