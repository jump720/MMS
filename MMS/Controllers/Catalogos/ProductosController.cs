using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using MMS.Classes;
using System.IO;

namespace MMS.Controllers.Catalogos
{
    public class ProductosController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string ProductoId)
        {
            var producto = await db.Producto.FindAsync(ProductoId);
            if (producto == null)
                return HttpNotFound();

            ViewData["TipoProductoID"] = new SelectList(await db.TipoProductos.Select(t => new { t.TipoProductoID, t.TipoProductoDesc }).ToListAsync(), "TipoProductoID", "TipoProductoDesc");
            return PartialView("_" + GetCrudMode().ToString(), producto);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Details(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            ViewData["TipoProductoID"] = new SelectList(await db.TipoProductos.Select(t => new { t.TipoProductoID, t.TipoProductoDesc }).ToListAsync(), "TipoProductoID", "TipoProductoDesc");
            return PartialView("_Create");
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        [Seguridad(isModal = true)]
        public ActionResult _Productos(bool ValidaInventario = false)
        {
            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            var Config = seguridadcll.Configuracion;

            string ConfigTipoMovEntrada = Config.ConfigTipoMovEntrada;
            string ConfigTipoMovAjEntrada = Config.ConfigTipoMovAjEntrada;

            List<Producto> productoList = new List<Producto>();

            if (ValidaInventario == false)
            {
                productoList = db.Producto.ToList();
            }
            else if (ValidaInventario == true)
            {
                //db.Producto.Where(p => p.TipoProductoID == Config.ConfigTipoProdGasto).ToList();
                //productoList = (from p in db.Producto
                //               join m in db.Movimiento on p.ProductoId equals m.ProductoId
                //               where (m.TipoMovimientoID == ConfigTipoMovEntrada || m.TipoMovimientoID == ConfigTipoMovAjEntrada) && m.MovimientoDisponible > 0
                //               select p).Distinct().ToList();
                string ConfigTipoProdGasto = Config.ConfigTipoProdGasto;
                var productosUnion = (from p in db.Producto
                                 join m in db.Movimiento on p.ProductoId equals m.ProductoId
                                 where (m.TipoMovimientoID == ConfigTipoMovEntrada || m.TipoMovimientoID == ConfigTipoMovAjEntrada) && m.MovimientoDisponible > 0
                                 select p).Distinct().Union((from p in db.Producto
                                                             where p.TipoProductoID == ConfigTipoProdGasto
                                                             select p));
                productoList = productosUnion.ToList();
            }

            return PartialView(productoList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ProductoId"></param>
        /// <returns></returns>
        /// 
        public JsonResult infoProducto(string ProductoId = null, bool ValidaInventario = false)
        {
            db.Configuration.ProxyCreationEnabled = false;
            Producto producto;
            try
            {
                if (ValidaInventario == false)
                {
                    producto = db.Producto
                                .Where(p => p.ProductoId == ProductoId)
                                .FirstOrDefault();
                }
                else
                {
                    Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                    var Config = seguridadcll.Configuracion;


                    string ConfigTipoMovEntrada = Config.ConfigTipoMovEntrada;
                    string ConfigTipoMovAjEntrada = Config.ConfigTipoMovAjEntrada;

                    //producto = (from p in db.Producto
                    //            join m in db.Movimiento on p.ProductoId equals m.ProductoId
                    //            where (m.TipoMovimientoID == ConfigTipoMovEntrada || m.TipoMovimientoID == ConfigTipoMovAjEntrada) && m.MovimientoDisponible > 0 && p.ProductoId == ProductoId
                    //            select p).FirstOrDefault();

                    string ConfigTipoProdGasto = Config.ConfigTipoProdGasto;
                    var productosUnion = (from p in db.Producto
                                          join m in db.Movimiento on p.ProductoId equals m.ProductoId
                                          where (m.TipoMovimientoID == ConfigTipoMovEntrada || m.TipoMovimientoID == ConfigTipoMovAjEntrada) && m.MovimientoDisponible > 0 && p.ProductoId == ProductoId
                                          select p).Distinct().Union((from p in db.Producto
                                                                      where p.TipoProductoID == ConfigTipoProdGasto && p.ProductoId == ProductoId
                                                                      select p));
                    producto = productosUnion.FirstOrDefault();
                }
                if (producto == null)
                {
                    producto = new Producto();
                }
            }
            catch
            {
                producto = new Producto();
            }
            return this.Json(producto, JsonRequestBehavior.AllowGet);
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
