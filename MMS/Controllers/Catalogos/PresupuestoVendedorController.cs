using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Classes;

namespace MMS.Controllers.Transacciones
{
    public class PresupuestoVendedorController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int Ano, string CanalId, string PlantaId, string CentroCostoId)
        {
            var presupuesto = await db.PresupuestoVendedor.Where(
                p => 
                p.PresupuestoVendedorAno == Ano && 
                p.PlantaID == PlantaId &&
                p.CanalID == CanalId &&
                p.CentroCostoID == CentroCostoId).FirstOrDefaultAsync();
            if (presupuesto == null)
                return HttpNotFound();

            ViewBag.PresupuestoVendedorAno = new SelectList(GetYears(), "Id", "Name", presupuesto.PresupuestoVendedorAno);
            ViewBag.CentroCostoID = new SelectList(await db.CentroCostos.ToListAsync(), "CentroCostoID", "CentroCostoDesc", presupuesto.CentroCostoID);
            ViewBag.PlantaID = new SelectList(await db.Plantas.ToListAsync(), "PlantaID", "PlantaDesc", presupuesto.PlantaID);
            ViewBag.CanalId = new SelectList(await db.Canales.ToListAsync(), "CanalID", "CanalDesc", presupuesto.CanalID);
            return PartialView("_" + GetCrudMode().ToString(), presupuesto);
        }

        [AuthorizeAction]
        // GET: PresupuestoVendedor/Details/5
        public async Task<ActionResult> Details(int Ano, string CanalId, string PlantaId, string CentroCostoId)
        {
            return await GetView(Ano, CanalId, PlantaId, CentroCostoId);
        }

        [AuthorizeAction]
        // GET: PresupuestoVendedor/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.PresupuestoVendedorMes = new SelectList(Fn.EnumToIEnumarable<Quart>(), "Value", "Name");
            ViewBag.PresupuestoVendedorAno = new SelectList(GetYears(), "Id", "Name");
            ViewBag.CentroCostoID = new SelectList(await db.CentroCostos.ToListAsync(), "CentroCostoID", "CentroCostoDesc");
            ViewBag.PlantaID = new SelectList(await db.Plantas.ToListAsync(), "PlantaID", "PlantaDesc");
            ViewBag.CanalId = new SelectList(await db.Canales.ToListAsync(), "CanalID", "CanalDesc");
            return PartialView("_Create");
        }

        [AuthorizeAction]
        // GET: PresupuestoVendedor/Edit
        public async Task<ActionResult> Edit(int Ano, string CanalId, string PlantaId, string CentroCostoId)
        {
            return await GetView(Ano, CanalId, PlantaId, CentroCostoId);
        }

        /* Para enviar la lista resultante al AJAX
        public JsonResult obtenerMesesPresupuesto(string cliente = null, int ano = 0)
        {
            var result = new[] { new { mes = 0, valor = (decimal?)0, gasto = (decimal?)0 } }.ToList();
            try
            {
                DateTime Date = DateTime.Today;


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
                }

                List<PresupuestoVendedor> presupuestos = new List<PresupuestoVendedor>();
                if (cliente != null && ano != 0)
                {
                    presupuestos = db.PresupuestoVendedor.Where(p => p.ClienteID == cliente &&
                                                                p.PresupuestoVendedorAno == ano).ToList();

                    result = (from r in presupuestos
                              select new
                              {
                                  valor = r.PresupuestoValor,
                                  gasto = r.PresupuestoGasto
                              }).ToList();
                    //result = resultTemp;
                    //result = new[] { new { mes = 0, valor = (decimal?)0, gasto = (decimal?)0 } }.ToList(); ;
                }
                else
                {
                    result = new[] { new { mes = 0, valor = (decimal?)0, gasto = (decimal?)0 } }.ToList(); ;
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }*/

        // GET: PresupuestoVendedor/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PresupuestoVendedor presupuestoVendedor = db.PresupuestoVendedor.Find(id);
            if (presupuestoVendedor == null)
            {
                return HttpNotFound();
            }
            return View(presupuestoVendedor);
        }

        // POST: PresupuestoVendedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PresupuestoVendedor presupuestoVendedor = db.PresupuestoVendedor.Find(id);
            db.PresupuestoVendedor.Remove(presupuestoVendedor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ReporteDetalleActividades()
        {
            try
            {
                var actividades = new List<Actividad>();
                ViewBag.TableActividades = actividades;
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
            }
            return View();
        }

        // POST: PresupuestoVendedor
        [Seguridad]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReporteDetalleActividades(ReporteActividades model, int? ActividadId = null, string ActividadTitulo = "", string PlantaID = "", string PlantaDesc = "")
        {
            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
            var actividades = new List<Actividad>();
            //Para conocer los campos vacíos.
            bool esVacioId = String.IsNullOrEmpty(ActividadId.ToString());
            bool esVacioTitulo = String.IsNullOrEmpty(ActividadTitulo);
            bool esVacioIdPlanta = String.IsNullOrEmpty(PlantaID);
            bool esVacioDescPlanta = String.IsNullOrEmpty(PlantaDesc);

            if (!esVacioId || !esVacioTitulo || !esVacioIdPlanta || !esVacioDescPlanta)
            {
                actividades = db.Actividad.Include(a => a.cliente).ToList()
                    .Where(a =>
                    ((esVacioId) ? true : a.ActividadId.ToString().Contains(ActividadId.ToString())) &&
                    ((esVacioTitulo) ? true : a.ActividadTitulo.ToLower().Contains(ActividadTitulo.ToLower())) &&
                    ((esVacioIdPlanta) ? true : a.ClienteID.ToLower().Contains(PlantaID.ToLower())) &&
                    ((esVacioDescPlanta) ? true : a.cliente.ClienteRazonSocial.ToLower().Contains(PlantaDesc.ToLower()))
                    )
                    .OrderByDescending(a => a.ActividadId)
                    .ToList();
                ViewBag.TableActividades = actividades;
            }
            else
            {
                ViewBag.TableActividades = actividades;
            }
            return View(model);
        }

        // Para enviar la lista de Ordenes por Actividad a AJAX
        public JsonResult ObtenerOrdenDeActividad(int ActividadId)
        {
            dynamic result;
            result = null;
            try
            {
                List<Orden> ordenes = new List<Orden>();
                if (ordenes != null)
                {
                    ordenes = db.Orden.Where(a => a.ActividadId == ActividadId).ToList();
                    var resultTemp = (from a in ordenes
                                      select new
                                      {
                                          ordenId = a.OrdenId,
                                          ordenEstado = a.OrdenEstado.ToString(),
                                          fechaCrea = a.OrdenFecha,
                                          guia = a.OrdenNroGuia
                                      }).ToList();
                    result = resultTemp;
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private List<dynamic> GetYears()
        {
            var years = new List<dynamic>();
            var yearTill = DateTime.Now.Year + 1;

            for (int i = 2017; i <= yearTill; i++)
                years.Add(new { Id = i, Name = i.ToString() });

            return years;
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
