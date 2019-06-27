using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;
using System.Data.Entity;
using MMS.Classes;
namespace MMS.ApiControllers.MMS
{
    public class GastosController : ApiBaseController
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


                int count = await db.Gasto
                    .Include(g => g.centroCosto)
                    .Include(g => g.actividad)
                    .Include(g => g.actividad.cliente)
                    .Include(g => g.producto)
                    .Include(g => g.tipogasto)
                    .Where(g => (g.GastoId.ToString().Contains(search) || g.ActividadId.ToString().Contains(search) || g.actividad.ActividadTitulo.Contains(search) ||
                                 g.ProductoId.Contains(search) || g.producto.ProductoDesc.Contains(search) || g.GastoEstado.ToString().Contains(search) ||
                                 g.centroCosto.CentroCostoDesc.Contains(search) || (g.actividad.ClienteID + g.actividad.cliente.ClienteRazonSocial).Contains(search)))
                    .Select(g => new
                    {
                        g.GastoId,
                        g.GastoLinea,
                        g.ActividadId,
                        g.actividad.ActividadTitulo,
                        ClienteId = g.actividad.ClienteID,
                        ClienteRazonSocial = g.actividad.cliente.ClienteRazonSocial,
                        ProductoId = g.ProductoId,
                        ProductoDesc = g.producto.ProductoDesc,
                        CentroCostoID = g.CentroCostoID,
                        CentroCostoDesc = g.CentroCostoID,
                        g.GastoValor,
                        g.GastoCant,
                        GastoEstado = g.GastoEstado.ToString()
                    })
                    .Distinct()
                    .CountAsync();

                var data = await db.Gasto
                    .Include(g => g.centroCosto)
                    .Include(g => g.actividad)
                    .Include(g => g.actividad.cliente)
                    .Include(g => g.producto)
                    .Include(g => g.tipogasto)
                    .Where(g => (g.GastoId.ToString().Contains(search) || g.ActividadId.ToString().Contains(search) || g.actividad.ActividadTitulo.Contains(search) ||
                                 g.ProductoId.Contains(search) || g.producto.ProductoDesc.Contains(search) || g.GastoEstado.ToString().Contains(search) ||
                                 g.centroCosto.CentroCostoDesc.Contains(search) || (g.actividad.ClienteID + g.actividad.cliente.ClienteRazonSocial).Contains(search)))
                  .Select(g => new
                  {
                      g.GastoId,
                      g.GastoLinea,
                      g.ActividadId,
                      g.actividad.ActividadTitulo,
                      ClienteId = g.actividad.ClienteID,
                      ClienteRazonSocial = g.actividad.cliente.ClienteRazonSocial,
                      ProductoId = g.ProductoId,
                      ProductoDesc = g.producto.ProductoDesc,
                      CentroCostoID = g.CentroCostoID,
                      CentroCostoDesc = g.centroCosto.CentroCostoDesc,
                      g.GastoValor,
                      g.GastoCant,
                      GastoEstado = g.GastoEstado.ToString()
                  })
                    .Distinct()
                    .OrderBy(a => a.GastoId)
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
        public async Task<IHttpActionResult> Create(Gasto model)
        {
            var result = new AjaxResult();

            try
            {
                Controllers.Seguridad.Seguridad seguridad = new Controllers.Seguridad.Seguridad();
                model.GastoId = seguridad.generaConsecutivo("Gastos");
                model.GastoLinea = 1;
                model.GastoFecha = DateTime.Today;
                model.GastoFechaMod = DateTime.Today;
                model.GastoValor = Decimal.Parse(model.GastoValor.ToString().Replace(".", ","));
                if (ModelState.IsValid)
                {
                                     
                    db.Gasto.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.GastoId, model);

                    return Ok(result.True("Record saved"));
                }
                else
                {
                    //return InternalServerError(new Exception("Error, All field are required"));
                    string s = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Ok(result.False(s));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]        
        public async Task<IHttpActionResult> Edit(Gasto model)
        {
            var result = new AjaxResult();

            try
            {
                model.GastoValor = Decimal.Parse(model.GastoValor.ToString().Replace(".", ","));
                if (ModelState.IsValid)
                {
                    Controllers.Transacciones.GastosController GastosCtrl = new Controllers.Transacciones.GastosController();
                    if (model.ActividadId != null)
                    {

                        //Devuelve el Gasto
                        if (model.GastoEstado == EstadoGasto.Ejecutado || model.GastoEstado == EstadoGasto.Pagado)
                        {

                            GastosCtrl.AfectaPresupuestoXGasto(model.GastoId, 1);
                            // AfectaPresupuestoXGasto(model.GastoId, 1);
                        }
                    }
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    if (model.ActividadId != null)
                    {
                        //Afecta el gasto
                        if (model.GastoEstado == EstadoGasto.Ejecutado || model.GastoEstado == EstadoGasto.Pagado)
                        {
                            //AfectaPresupuestoXGasto(model.GastoId, 2);
                            GastosCtrl.AfectaPresupuestoXGasto(model.GastoId, 2);
                        }
                    }


                    AddLog("", model.GastoId, model);

                    return Ok(result.True("Record saved"));
                }
                else
                {
                    //return InternalServerError(new Exception("Error, All field are required"));
                    string s = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Ok(result.False(s));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> CambioEstado(int Id = 0, int Linea = 0, EstadoGasto Estado = EstadoGasto.Abierta)
        {
            var result = new AjaxResult();
            //bool result = false;
            var gasto = await db.Gasto.Where(g => g.GastoId == Id && g.GastoLinea == Linea).FirstOrDefaultAsync();

            if (gasto != null)
            {
                gasto.GastoEstado = Estado;
                db.Entry(gasto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //result = true;
                AddLog("", gasto.GastoId, gasto);
            }
            else
            {
                return Ok(result.False("Record not found"));
            }

            return Ok(result.True("State change successful"));

        }

    }
}
