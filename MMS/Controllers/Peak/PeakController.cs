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
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;

namespace MMS.Controllers.Peak
{
    public class PeakController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeAction("Peak/Index")]
        public ActionResult Manage(int id)
        {
            ViewBag.Tipo = "manage";
            ViewBag.Id = id;
            return View("Peak");
        }

        [AuthorizeAction("Peak/Index")]
        public ActionResult Review(int id)
        {
            ViewBag.Tipo = "review";
            ViewBag.Id = id;
            return View("Peak");
        }

        [AuthorizeAction("Peak/Index")]
        public async Task<ActionResult> File(int id)
        {
            string filename = "";
            XSSFWorkbook wb = null;
            try
            {
                var peak = await db.Peak
                    .Include(p => p.Usuario)
                    .Include(p => p.Area)
                    .Include(p => p.Periodo)
                    .Include(p => p.UsuarioPadre)
                    .Include(p => p.PeakObjetivos)
                    .Include(p => p.PeakRevisiones)
                    .Include(p => p.PeakPlanesDesarrollo)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (peak == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                else if (peak.Estado != EstadoPeak.Finished)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Peak is in " + peak.Estado.ToString().Replace("_", " "));

                var date = DateTime.Now;
                string baseFilename = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Peak_Base.xlsx";
                filename = $"{AppDomain.CurrentDomain.BaseDirectory}/UploadFolder/Peak_{peak.Id}_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}.xlsx";

                System.IO.File.Copy(baseFilename, filename, true);

                wb = new XSSFWorkbook(filename);
                var sheet = wb.GetSheet("PEAK");
                int rowNo = 0;
                string nl = Environment.NewLine;

                sheet.GetRow(rowNo++).Cells[2].SetCellValue(peak.Usuario.UsuarioNombre);
                sheet.GetRow(rowNo++).Cells[2].SetCellValue(peak.Cargo);

                var row = sheet.GetRow(rowNo++);
                row.Cells[2].SetCellValue(peak.Area.Nombre);
                row.Cells[4].SetCellValue($"From: {((Months)peak.Periodo.FechaIni.Month).ToString()} {peak.Periodo.FechaIni.Day}, {peak.Periodo.FechaIni.Year} through {((Months)peak.Periodo.FechaFin.Month).ToString()} {peak.Periodo.FechaFin.Day}, {peak.Periodo.FechaFin.Year}");

                if (peak.UsuarioPadre != null)
                    sheet.GetRow(rowNo++).Cells[2].SetCellValue(peak.UsuarioPadre.UsuarioNombre);

                rowNo = 8;
                var rowBase = sheet.GetRow(rowNo++);
                sheet.ShiftRows(rowNo, sheet.LastRowNum, peak.PeakObjetivos.Count, true, false);

                var setRow = new Action<int, object>((index, value) =>
                {
                    var cell = row.CreateCell(index, CellType.Blank);
                    var style = wb.CreateCellStyle();
                    style.CloneStyleFrom(rowBase.Cells[index].CellStyle);
                    cell.CellStyle = style;

                    if (value != null)
                    {
                        string typeName = value.GetType().Name;

                        if (typeName == "Int16")
                            cell.SetCellValue((short)value);
                        else if (typeName == "Int32")
                            cell.SetCellValue((int)value);
                        else if (typeName == "Single")
                            cell.SetCellValue((float)value);
                        else if (typeName == "XSSFRichTextString")
                            cell.SetCellValue((IRichTextString)value);
                        else
                            cell.SetCellValue(value.ToString());
                    }
                });

                foreach (var po in peak.PeakObjetivos)
                {
                    row = sheet.CreateRow(rowNo);

                    setRow(0, po.Numero);
                    setRow(1, po.Peso / 100f);
                    setRow(2, po.Objetivo);
                    setRow(3, null);
                    setRow(4, po.FechaMeta.ToShortDateString());
                    setRow(5, po.MedidoPor);
                    setRow(6, po.ResultadosActuales);
                    setRow(7, po.Comentarios);
                    setRow(8, null);
                    setRow(9, po.Completado / 100f);
                    setRow(10, po.Calificacion);
                    setRow(11, po.Factor);
                    setRow(12, po.ComentariosJefe);
                    setRow(13, null);
                    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 2, 3));
                    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 7, 8));
                    rowNo++;
                }
                rowBase.ZeroHeight = true;

                row = sheet.GetRow(rowNo);
                row.Cells[1].SetCellFormula($"SUM(B10:B{9 + peak.PeakObjetivos.Count})");
                row.Cells[11].SetCellFormula($"SUM(L10:L{9 + peak.PeakObjetivos.Count})");

                rowNo += 3;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.JustificacionFactorAjuste);
                row.Cells[11].SetCellValue(peak.FactorAjuste);

                row = sheet.GetRow(++rowNo);
                row.Cells[11].SetCellValue(peak.FactorAjuste);

                string comentariosRevision = "", comentariosRevisionJefe = "";
                foreach (var pr in peak.PeakRevisiones)
                {
                    comentariosRevision += pr.Comentarios + $"{nl} ---------------- {nl}";
                    comentariosRevisionJefe += pr.ComentariosJefe + $"{nl} ---------------- {nl}";
                }

                rowNo += 8;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(comentariosRevision);
                row.Cells[8].SetCellValue(comentariosRevisionJefe);

                var peakCoreValues = await db.PeakCoreValue
                    .Include(pcv => pcv.CoreValue)
                    .Where(pcv => pcv.PeakId == id)
                    .OrderBy(pcv => pcv.CoreValue.Orden)
                    .ToListAsync();

                rowNo += 8;
                rowBase = sheet.GetRow(rowNo++);
                sheet.ShiftRows(rowNo, sheet.LastRowNum, peakCoreValues.Count, true, false);

                foreach (var pcv in peakCoreValues)
                {
                    row = sheet.CreateRow(rowNo);

                    var font = new XSSFFont()
                    {
                        FontName = HSSFFont.FONT_ARIAL,
                        FontHeightInPoints = 12,
                    };

                    var rt = new XSSFRichTextString();
                    font.IsBold = true;
                    rt.Append($"{pcv.CoreValue.Nombre} /", font);
                    font.Color = IndexedColors.LightBlue.Index;
                    font.IsItalic = true;
                    rt.Append($"{pcv.CoreValue.Competencia}{nl}{nl}", font);
                    font.Color = IndexedColors.Black.Index;
                    font.IsBold = false;
                    rt.Append($"{pcv.CoreValue.Descripcion}", font);

                    setRow(0, rt);
                    setRow(1, null);
                    setRow(2, null);
                    setRow(3, null);
                    setRow(4, null);
                    setRow(5, null);
                    setRow(6, null);
                    setRow(7, null);
                    setRow(8, null);
                    setRow(9, pcv.Autoevaluacion.ToString().Replace("_", " "));
                    setRow(10, null);
                    setRow(11, null);
                    setRow(12, pcv.Evaluacion.ToString().Replace("_", " "));
                    setRow(13, null);

                    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 0, 8));
                    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 9, 11));
                    sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 12, 13));
                    row.HeightInPoints = 70;

                    rowNo++;
                }
                rowBase.ZeroHeight = true;

                rowNo += 2;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.ComentariosCompetencias);

                rowNo += 5;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.ResumenContribuciones);
                row.Cells[8].SetCellValue(peak.ResumenContribucionesJefe);

                rowNo += 3;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.Fortalezas);
                row.Cells[8].SetCellValue(peak.FortalezasJefe);

                rowNo += 3;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.ObjetivosFuturo);
                row.Cells[8].SetCellValue(peak.ObjetivosFuturoJefe);

                rowNo += 3;
                row = sheet.GetRow(rowNo);
                row.Cells[0].SetCellValue(peak.RendimientoGeneral);

                if (peak.PeakPlanesDesarrollo.Count > 0)
                {
                    rowNo += 6;
                    rowBase = sheet.GetRow(rowNo++);
                    sheet.ShiftRows(rowNo, sheet.LastRowNum, peak.PeakPlanesDesarrollo.Count, true, false);

                    foreach (var ppd in peak.PeakPlanesDesarrollo)
                    {
                        row = sheet.CreateRow(rowNo);

                        setRow(0, ppd.Area);
                        setRow(1, null);
                        setRow(2, null);
                        setRow(3, ppd.Plan);
                        setRow(4, null);
                        setRow(5, null);
                        setRow(6, null);
                        setRow(7, null);
                        setRow(8, null);
                        setRow(9, ppd.FechaMeta.ToShortDateString());
                        setRow(10, ppd.ResultadoDeseado);
                        setRow(11, null);
                        setRow(12, null);
                        setRow(13, null);
                        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 0, 2));
                        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 3, 8));
                        sheet.AddMergedRegion(new CellRangeAddress(rowNo, rowNo, 10, 13));
                        rowNo++;
                    }
                    rowBase.ZeroHeight = true;
                }

                sheet.ForceFormulaRecalculation = true;
                sheet = wb.GetSheet("Competency Assessment");
                rowNo = 2;
                rowBase = sheet.GetRow(rowNo++);

                foreach (var pcv in peakCoreValues)
                {
                    row = sheet.CreateRow(rowNo);

                    var font = new XSSFFont()
                    {
                        FontName = HSSFFont.FONT_ARIAL,
                        FontHeightInPoints = 10,
                        IsItalic = true,
                    };

                    var rt = new XSSFRichTextString();
                    font.IsBold = true;
                    rt.Append($"{nl}{pcv.CoreValue.Nombre}{nl}{nl}", font);
                    font.IsBold = false;
                    rt.Append($"{pcv.CoreValue.Descripcion}{nl}", font);
                    setRow(0, rt);

                    rt = new XSSFRichTextString();
                    font.IsBold = true;
                    rt.Append($"{nl}{pcv.CoreValue.Competencia}{nl}{nl}", font);
                    font.IsBold = false;
                    rt.Append($"{pcv.CoreValue.CompetenciaDescripcion}{nl}", font);
                    setRow(1, rt);

                    setRow(2, pcv.CoreValue.HabilidadAlta);
                    setRow(3, pcv.CoreValue.HabilidadMedia);
                    setRow(4, pcv.CoreValue.HabilidadBaja);
                    rowNo++;
                }
                rowBase.ZeroHeight = true;

                var ms = new MemoryStream();
                wb.Write(ms);

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Peak_{peak.Usuario.UsuarioId}.xlsx");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                if (wb != null)
                    wb.Close();

                if (System.IO.File.Exists(filename))
                    System.IO.File.Delete(filename);
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
