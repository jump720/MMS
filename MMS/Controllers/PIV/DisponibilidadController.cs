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

namespace MMS.Controllers.PIV
{
    public class DisponibilidadController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission("Disponibilidad/Upload")]
        public async Task<ActionResult> Index()
        {
            var años = new List<dynamic>();
            años.Add(new { Value = DateTime.Now.Year - 1 });
            años.Add(new { Value = DateTime.Now.Year });

            ViewBag.ColeccionPIV = new SelectList(await db.ColeccionPIV.OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewBag.Marca = new SelectList(await db.Marca.OrderBy(m => m.Nombre).Select(m => new { m.Id, m.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewData["daModel.Mes"] = new SelectList(Fn.EnumToIEnumarable<Months>(), "Value", "Name");
            ViewData["daModel.Ano"] = new SelectList(años, "Value", "Value");
            return View();
        }

        [AuthorizeAction("Disponibilidad/Upload")]
        public ActionResult BaseFile()
        {
            string filename = "PIV_StockFile_Base.xlsx";
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

        [AuthorizeAction("Disponibilidad/Index")]
        public async Task<ActionResult> Detalle(int id, int itemId)
        {
            ViewBag.ColeccionPIVNombre = await db.ColeccionPIV.Where(c => c.Id == id).Select(c => c.Nombre).FirstOrDefaultAsync();
            ViewBag.Item = await db.Item
                .Include(i => i.Marca)
                .Where(i => i.Id == itemId)
                .FirstOrDefaultAsync();

            var detalle = await db.DisponibilidadArchivoItem
                .Include(dai => dai.DisponibilidadArchivo)
                .Include(dai => dai.DisponibilidadArchivo.Usuario)
                .Where(dai => dai.ItemId == itemId && dai.ColeccionPIVId == id)
                .Select(dai => new DisponibilidadDetalleViewModel
                {
                    Fecha = dai.DisponibilidadArchivo.FechaSubida,
                    Usuario = dai.DisponibilidadArchivo.Usuario.UsuarioNombre,
                    Descripcion = "Stock: " + dai.DisponibilidadArchivo.Mes + "-" + dai.DisponibilidadArchivo.Ano,
                    Cantidad = dai.Cantidad
                })
                .ToListAsync();

            detalle.AddRange(await db.LiquidacionItem
                .Include(li => li.LiquidacionAsesor)
                .Include(li => li.LiquidacionAsesor.Asesor)
                .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                .Include(li => li.LiquidacionAsesor.LiquidacionArchivo.Usuario)
                .Include(li => li.LiquidacionAsesor.LiquidacionArchivo.Liquidacion)
                .Include(li => li.ColeccionPIVItem)
                .Where(li => li.ColeccionPIVItem.ItemId == itemId && li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIVId == id && li.LiquidacionAsesor.LiquidacionArchivo.Liquidacion.Estado == EstadoLiquidacion.Closed)
                .Select(li => new DisponibilidadDetalleViewModel
                {
                    Fecha = li.LiquidacionAsesor.LiquidacionArchivo.FechaSubida,
                    Usuario = li.LiquidacionAsesor.LiquidacionArchivo.Usuario.UsuarioNombre,
                    Descripcion = "Settlement: '" + li.LiquidacionAsesor.LiquidacionArchivo.Liquidacion.Descripcion + "'" + (li.LiquidacionAsesor.Asesor == null ? "" : " - Seller: '" + li.LiquidacionAsesor.Asesor.Nombre + " " + li.LiquidacionAsesor.Asesor.Apellido1 + " " + li.LiquidacionAsesor.Asesor.Apellido2 + "'") + " - Date: " + li.Mes + "-" + li.Ano,
                    Cantidad = li.Cantidad * -1
                })
                .ToListAsync());

            return PartialView("_Detalle", detalle.OrderByDescending(d => d.Fecha).ToList());
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
