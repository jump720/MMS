using MMS.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

namespace MMS.Controllers.Transacciones
{

    public class ConsultaInventarioApiController : ApiController
    {

        private MMSContext db = new MMSContext();

        [HttpPost]
        public SysDataTablePager filldata(FormDataCollection form)
        {

            string sEcho = form["draw"].ToString();//pagina
            int iDisplayStart = Convert.ToInt32(form["Start"]);//numero de objeto a esconder
            int iDisplayLength = Convert.ToInt32(form["Length"]);//tamaño de la grilla
            string sSearch = form["search[value]"].ToString().ToLower();//filtro smart (global)

            var movimientosList = new List<Movimiento>();
            var Count = 0;
            if (HttpContext.Current.Session != null)
            {
                try
                {
                    Seguridadcll seguridadcll = (Seguridadcll)HttpContext.Current.Session["seguridad"];
                    Configuracion config = seguridadcll.Configuracion;

                    if (!string.IsNullOrEmpty(sSearch))
                    {
                        movimientosList = db.Movimiento.Include(m => m.producto)
                                   .Where(m => (m.TipoMovimientoID == config.ConfigTipoMovAjEntrada || m.TipoMovimientoID == config.ConfigTipoMovEntrada)
                                               && m.MovimientoDisponible > 0 && m.MovimientoEstado == EstadoMovimiento.Abierto)
                                   .ToList()
                                   .Where(m => m.MovimientoId.ToString().Contains(sSearch) ||
                                               (m.MovimientoFechaCrea != null && m.MovimientoFechaCrea.ToString("d").ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.ProductoId) && m.ProductoId.ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.producto.ProductoDesc) && m.producto.ProductoDesc.ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.MovimientoValor.ToString()) && m.MovimientoValor.ToString().ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.MovimientoCantidad.ToString()) && m.MovimientoCantidad.ToString().ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.MovimientoDisponible.ToString()) && m.MovimientoDisponible.ToString().ToLower().Contains(sSearch)) ||
                                               (!string.IsNullOrEmpty(m.MovimientoReservado.ToString()) && m.MovimientoReservado.ToString().ToLower().Contains(sSearch))
                                               )
                                   .ToList();
                    }
                    else
                    {
                        movimientosList = db.Movimiento.Include(m => m.producto)
                                    .Where(m => (m.TipoMovimientoID == config.ConfigTipoMovAjEntrada || m.TipoMovimientoID == config.ConfigTipoMovEntrada)
                                                && m.MovimientoDisponible > 0 && m.MovimientoEstado == EstadoMovimiento.Abierto).ToList();


                    }//if (!string.IsNullOrEmpty(sSearch))
                }
                catch
                {
                    movimientosList = new List<Movimiento>();
                }// try
            }
            else
            {
                movimientosList = new List<Movimiento>();
            }//if (HttpContext.Current.Session != null)

            Count = movimientosList.Count;

            movimientosList = movimientosList.OrderByDescending(m => m.MovimientoFechaCrea)
                                           .Skip(iDisplayStart).Take(iDisplayLength).ToList();


            var CustomerPaged = new SysDataTablePager();

            CustomerPaged.draw = sEcho;
            CustomerPaged.recordsTotal = Count;
            CustomerPaged.recordsFiltered = Count;
            CustomerPaged.data = movimientosList.Select(m => new { m.MovimientoId, m.ProductoId, m.producto.ProductoDesc, m.MovimientoFechaCrea, m.MovimientoValor, m.MovimientoCantidad, m.MovimientoDisponible, m.MovimientoReservado, m.MovimientoLinea });

            return CustomerPaged;
        }
    }
}
