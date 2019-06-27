using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using MMS.Classes;
using System.Threading.Tasks;

namespace MMS.Controllers.Transacciones
{
    public class GastosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Gastos
        [AuthorizeAction]
        [FillPermission("Gastos/CambioEstado")]
        public ActionResult Index()
        {
           
            return View();
        }




        private async Task<ActionResult> GetView(int Id, int Linea)
        {
            var gasto = await db.Gasto
                                //.Include(g => g.actividad)
                                //.Include(g => g.actividad.cliente)
                                .Where(p => p.GastoId == Id && p.GastoLinea == Linea).FirstOrDefaultAsync();
            if (gasto == null)
                return HttpNotFound();

            ViewBag.TipoGastoID = new SelectList(db.TipoGastos, "TipoGastoID", "TipoGastoDesc", gasto.TipoGastoID);
            ViewBag.CentroCostoID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc", gasto.CentroCostoID);
            ViewBag.Actividad = await db.Actividad.Include(a => a.cliente).Where(a => a.ActividadId == gasto.ActividadId).FirstOrDefaultAsync();
            if (GetCrudMode() == Fn.CrudMode.Edit)
            {
                if (gasto.GastoEstado == EstadoGasto.Eliminado || gasto.GastoEstado == EstadoGasto.Rechazado)
                    return PartialView("_" + Fn.CrudMode.Details.ToString(), gasto);
            }

            return PartialView("_" + GetCrudMode().ToString(), gasto);
        }

        // GET: Gastos/Details/5
        [AuthorizeAction]
        public async Task<ActionResult> Details(int Id, int Linea)
        {

            return await GetView(Id, Linea);
        }

        // GET: Gastos/Create
        [AuthorizeAction]
        public ActionResult Create()
        {
           
            ViewBag.TipoGastoID = new SelectList(db.TipoGastos, "TipoGastoID", "TipoGastoDesc");
            ViewBag.CentroCostoID = new SelectList(db.CentroCostos, "CentroCostoID", "CentroCostoDesc");
            return PartialView("_Create");
        }



        // GET: Gastos/Edit/5
        [AuthorizeAction]
        public async Task<ActionResult> Edit(int Id, int Linea)
        {

            return await GetView(Id, Linea);
        }

        

        // GET: Gastos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Gasto gasto = db.Gasto.Find(id);
            if (gasto == null)
            {
                return HttpNotFound();
            }
            return View(gasto);
        }

        // POST: Gastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Gasto gasto = db.Gasto.Find(id);
            db.Gasto.Remove(gasto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        //public bool CambioEstadoGasto(int GastoId = 0, int GastoLinea = 0, EstadoGasto Estado = EstadoGasto.Abierta)
        //public bool CambioEstadoGasto(int GastoId = 0, int GastoLinea = 0, EstadoGasto Estado = EstadoGasto.Abierta)
        //{
        //    bool result = false;
        //    var gasto = db.Gasto.Where(g => g.GastoId == GastoId && g.GastoLinea == GastoLinea).FirstOrDefault();

        //    if (gasto != null)
        //    {
        //        gasto.GastoEstado = Estado;
        //        db.Entry(gasto).State = EntityState.Modified;
        //        db.SaveChanges();
        //        result = true;

        //        #region auditoria
        //        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
        //        Auditoria auditoria = new Auditoria();
        //        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

        //        auditoria.AuditoriaFecha = System.DateTime.Now;
        //        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
        //        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
        //        auditoria.AuditoriaEvento = "CambioEstadoGasto";
        //        auditoria.AuditoriaDesc = "Se cambio el estado del gasto: " + GastoId + " Linea: " + GastoLinea + " Estado: " + Estado.ToString();
        //        auditoria.ObjetoId = "Gastos/CambioEstadoGasto";

        //        seguridad.insertAuditoria(auditoria);
        //        #endregion auditoria
        //    }
        //    else
        //    {
        //        result = false;
        //    }

        //    return result;

        //}

        /// <summary>
        /// Steps:
        /// 1. Devuelve lo gastado(aun no ha guarado el gasto)
        /// 2. Afecta el gasto(el gasto ya fue guardado)
        /// </summary>
        /// <param name="gasto"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public void AfectaPresupuestoXGasto(int gastoId = 0, int step = 0)
        {
            // bool result = true;
            using (var Context = new MMSContext())
            {
                var gastoTemp = Context.Gasto
                            .Include(g => g.actividad)
                            .Where(g => g.GastoId == gastoId && (g.GastoEstado == EstadoGasto.Ejecutado || g.GastoEstado == EstadoGasto.Pagado))
                            .FirstOrDefault();
                decimal ValorGasto = 0;


                if (gastoTemp != null)
                {
                    //var actividad = Context.Actividad.Where(a => a.ActividadId == gastoTemp.ActividadId).FirstOrDefault();
                    ValorGasto = gastoTemp.GastoValor * gastoTemp.GastoCant;
                    //Buscar Prespuesto a Afectar    
                    DateTime Date = gastoTemp.actividad.ActividadFecha;

                    int Year = Date.Year;
                    int Month = Date.Month;
                    int quartile = 0;

                    if (Month >= 1 && Month <= 3)//Q1
                    {
                        quartile = 1;
                    }
                    else if (Month >= 4 && Month <= 6)//Q2
                    {
                        quartile = 2;
                    }
                    else if (Month >= 7 && Month <= 9)//Q3
                    {
                        quartile = 3;
                    }
                    else if (Month >= 10 && Month <= 12)//Q4
                    {
                        quartile = 4;
                    }//if (Month >= 1 && Month <= 3)

                    var prespuesto = db.PresupuestoVendedor.Where(p => p.PlantaID == gastoTemp.actividad.ClienteID && p.CentroCostoID == gastoTemp.CentroCostoID &&
                                                                      p.PresupuestoVendedorAno == Year).FirstOrDefault();

                    switch (step)
                    {
                        case 1://1. Devuelve lo gastado(aun no ha guarado el gasto)
                            if (prespuesto != null)
                            {
                                prespuesto.PresupuestoGasto -= ValorGasto;
                                Context.Entry(prespuesto).State = EntityState.Modified;
                                Context.SaveChanges();
                            }//if (prespuesto != null)
                            break;

                        case 2://2. Afecta el gasto(el gasto ya fue guardado)
                            if (prespuesto != null)
                            {
                                prespuesto.PresupuestoGasto += ValorGasto;
                                Context.Entry(prespuesto).State = EntityState.Modified;
                                Context.SaveChanges();
                            }//if (prespuesto != null)
                            break;
                    }
                }
                else
                {

                }
            }// using (var Context = new MMSContext())
            //return result;
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
