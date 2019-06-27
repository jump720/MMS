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

namespace MMS.Controllers.PIV
{
    public class LiquidacionesController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private List<dynamic> GetYears()
        {
            var years = new List<dynamic>();
            var yearTill = DateTime.Now.Year + 1;

            for (int i = 2017; i <= yearTill; i++)
                years.Add(new { Id = i, Name = i.ToString() });

            return years;
        }

        private async Task<ActionResult> GetView(int id)
        {
            var liquidacion = await db.Liquidacion.FindAsync(id);
            if (liquidacion == null)
                return HttpNotFound();

            var mode = GetCrudMode();
            if ((mode == Fn.CrudMode.Edit || mode == Fn.CrudMode.Delete) && liquidacion.Estado == EstadoLiquidacion.Closed)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Settlement Closed");

            ViewBag.MonthFrom = new SelectList(Fn.EnumToIEnumarable<Months>(), "Value", "Name", liquidacion.FechaInicial.Month);
            ViewBag.YearFrom = new SelectList(GetYears(), "Id", "Name", liquidacion.FechaInicial.Year);
            ViewBag.MonthTill = new SelectList(Fn.EnumToIEnumarable<Months>(), "Value", "Name", liquidacion.FechaFinal.Month);
            ViewBag.YearTill = new SelectList(GetYears(), "Id", "Name", liquidacion.FechaFinal.Year);
            return PartialView("_" + mode.ToString(), liquidacion);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            var liquidacion = new Liquidacion() { Estado = EstadoLiquidacion.Open };

            ViewBag.MonthFrom = new SelectList(Fn.EnumToIEnumarable<Months>(), "Value", "Name");
            ViewBag.YearFrom = new SelectList(GetYears(), "Id", "Name");
            ViewBag.MonthTill = new SelectList(Fn.EnumToIEnumarable<Months>(), "Value", "Name");
            ViewBag.YearTill = new SelectList(GetYears(), "Id", "Name");
            return PartialView("_Create", liquidacion);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction("Liquidaciones/Edit")]
        [FillPermission("Liquidaciones/Approve")]
        public async Task<ActionResult> Manage(int id)
        {
            var liquidacion = await db.Liquidacion.FindAsync(id);
            if (liquidacion == null)
                return HttpNotFound();

            ViewBag.ColeccionPIV = new SelectList(await db.ColeccionPIV.OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            return View(liquidacion);
        }

        [AuthorizeAction("Liquidaciones/Edit")]
        public async Task<ActionResult> Files(int id)
        {
            var liquidacion = await db.Liquidacion.FindAsync(id);
            if (liquidacion == null)
                return HttpNotFound();

            return PartialView("_Files", liquidacion);
        }

        [AuthorizeAction("Liquidaciones/Edit")]
        public ActionResult BaseFile()
        {
            string filename = "PIV_SalesFile_Base.xlsx";
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/" + filename;
            byte[] filedata = System.IO.File.ReadAllBytes(filepath);
            string contentType = MimeMapping.GetMimeMapping(filepath);

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = true,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());
            return File(filedata, contentType);
        }

        [AuthorizeAction("Liquidaciones/Edit")]
        public async Task<ActionResult> Asesor(int id, int? asesorId)
        {
            Asesor asesor = null;

            if (asesorId == 0)
                asesorId = null;
            else
            {
                asesor = await db.Asesor
                    .Where(a => a.Id == asesorId)
                    .FirstOrDefaultAsync();

                if (asesor == null)
                    return HttpNotFound();
            }

            ViewBag.Asesor = asesor;

            var liquidacionItems = await db.LiquidacionItem
                .Include(li => li.LiquidacionAsesor)
                .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                .Include(li => li.ColeccionPIVItem)
                .Include(li => li.ColeccionPIVItem.ColeccionPIV)
                .Include(li => li.ColeccionPIVItem.Item)
                .Include(li => li.ColeccionPIVItem.Item.Marca)
                .Where(li => li.LiquidacionAsesor.AsesorId == asesorId && li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id)
                .ToListAsync();

            ViewBag.Total = liquidacionItems.Sum(li => li.ValorTotal).ToString("C");

            return PartialView("_Asesor", liquidacionItems);
        }

        [AuthorizeAction("Liquidaciones/Edit")]
        public async Task<ActionResult> PaymentFile(int id, string date)
        {
            var liquidacion = await db.Liquidacion
                .Include(l => l.CategoriasCDE)
                .Include(l => l.LiquidacionAprobaciones)
                .Where(l => l.Id == id)
                .FirstOrDefaultAsync();

            if (liquidacion == null)
                return HttpNotFound();

            if (liquidacion.Estado == EstadoLiquidacion.Open)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Settlement must be closed.");

            var liquidacionCierres = await db.LiquidacionCierre
                .Include(lc => lc.Asesor)
                .Where(lc => lc.LiquidacionId == id)
                .ToListAsync();

            var categoriaBase = liquidacion.CategoriasCDE.OrderBy(c => c.ValorMinimo).First();

            byte[] content = null;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    decimal totalComisiones = 0;
                    int totalAsesores = 0;
                    sw.WriteLine("\"01\"|DOCUMENTO|TIPODOCUMENTO|VALOR|FECHA|NOMBRES|APELLIDO1|APELLIDO2|TELEFONO|COMENTARIO|CODIGOPS|PIN");

                    foreach (var lc in liquidacionCierres)
                    {
                        decimal comision = ((lc.TotalNuevo * (decimal)(lc.PorcentajeAplicado)) / 100) + lc.TotalReglas;
                        if (comision == 0)
                            continue;

                        totalAsesores++;
                        totalComisiones += comision;
                        sw.WriteLine($"\"02\"|\"{lc.Asesor.Cedula}\"|\"CC\"|{comision.ToString("F0")}|{date}|\"{lc.Asesor.Nombre.ToUpper()}\"|\"{lc.Asesor.Apellido1.ToUpper()}\"|\"{lc.Asesor.Apellido2?.ToUpper()}\"|\"N.A\"|\"N.A\"|\"040003\"|\"N.A\"");
                    }

                    sw.WriteLine($"\"03\"|{totalAsesores}|{totalComisiones.ToString("F0")}|");
                }

                content = ms.ToArray();
            }

            return File(content, "text/plain", $"PaymentFile_{id}.txt");
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
