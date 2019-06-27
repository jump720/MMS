using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using System.Net;

namespace MMS.Controllers.BI
{
    public class InformesController : BaseController
    {

        private MMSContext db = new MMSContext();

        // GET: Informes
        //public ActionResult Index()
        //{
        //    return View();
        //}

        [AuthorizeAction]
        public ActionResult ClienteResumen()
        {
            return View();
        }


        [AuthorizeAction]
        public ActionResult Disponibilidad()
        {
            return View();
        }

        [AuthorizeAction("Informes/Disponibilidad")]
        public async Task<ActionResult> DescargaDisponibilidad(int sloc = 3000)
        {
            string filename = "";

            XSSFWorkbook wb = null;
            try
            {
                string dataquery =  "SELECT " +
                                    " CONVERT(nvarchar, z.DateStamp, 103) +' ' + CONVERT(nvarchar, z.DateStamp, 108) AS DateStamp," +
                                    "convert(nvarchar(5),z.[SLoc]) AS[SLoc], " +
                                    "z.[Material]  AS[Material], " +
                                    " upper(isnull(m.[Description], z.MaterialDescription )) AS [MaterialDescription]," +
                                    "convert(int,sum(TotalStock-OpenOrdersQty-PickQty)) as [AvailableQty] " +
                                    "     FROM ApexRep.[dbo].[ZINV] as z " +
                                    "     left join ApexRep.[dbo].Materials as M on m.Material = z.Material " +
                                    "where sloc =  "  + sloc +
                                    " group by z.DateStamp, SLoc,SlocDesc, z.Material, m.Description, z.MaterialDescription";

                var disponibilidad = await db.Database.SqlQuery<DisponibilidadViewModel>(dataquery).ToListAsync();

                if (disponibilidad == null)
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);

                var date = DateTime.Now;
                string baseFilename = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Disponibilidad_Base.xlsx";
                filename = $"{AppDomain.CurrentDomain.BaseDirectory}/UploadFolder/Disponibilidad_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}.xlsx";

                System.IO.File.Copy(baseFilename, filename, true);


                wb = new XSSFWorkbook(filename);
                var sheet = wb.GetSheet("DISPONIBILIDAD");
                int rowNo = 6;
                string nl = Environment.NewLine;

                sheet.GetRow(3).Cells[1].SetCellValue("Ultima actualización: " + disponibilidad.FirstOrDefault().DateStamp);
                

                foreach (var d in disponibilidad)
                {
                    var row = sheet.CreateRow(rowNo);
                    row.CreateCell(1);
                    row.CreateCell(2);
                    row.CreateCell(3);
                    row.CreateCell(4);

                    row.Cells[0].SetCellValue(d.SLoc);
                    row.Cells[1].SetCellValue(d.Material);
                    row.Cells[2].SetCellValue(d.MaterialDescription);
                    row.Cells[3].SetCellValue(d.AvailableQty);

                    rowNo++;
                }
               

                var ms = new MemoryStream();
                wb.Write(ms);

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Disponibilidad_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}.xlsx");
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

            //return View();
        }
    }
}