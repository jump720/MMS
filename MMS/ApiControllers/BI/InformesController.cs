using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MMS.Models;
using MMS.Filters;
using System.Net.Http.Formatting;
using MMS.Classes;
using System.Threading.Tasks;
using System.Web;

namespace MMS.ApiControllers.BI
{
    public class InformesController : ApiBaseController
    {

        private MMSContext db = new MMSContext();

        [HttpPost]
        public IHttpActionResult PQRS(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string cliente = form["_cliente"];


                string sqlWhere = "";



                if (cliente.Trim() != "")
                    sqlWhere += "ClienteId like '%" + cliente + "%' AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";
                if (sqlWhere.Trim() != "")
                {
                    sqlCount = "select COUNT(Id)  from PQRSView" + sqlWhere;
                    sqlData = "SELECT * FROM (select Row_number() OVER( ORDER BY id DESC) AS Seq, * from PQRSView " + sqlWhere +
                             $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";
                }
                else
                {
                    sqlCount = "select COUNT(Id)  from PQRSView where 1 = 0";
                    sqlData = "SELECT * FROM (select Row_number() OVER( ORDER BY id DESC) AS Seq, * from PQRSView " + sqlWhere +
                             $") AS T2 WHERE Seq BETWEEN 0 AND 0";
                }

                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
        public IHttpActionResult ActividadesMMS(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string cliente = form["_cliente"];


                string sqlWhere = "";



                if (cliente.Trim() != "")
                    sqlWhere += "ClienteID like '%" + cliente + "%' AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";
                if (sqlWhere.Trim() != "")
                {
                    sqlCount =
                    "SELECT COUNT(ActividadId) FROM ( " +
                    "SELECT a.ActividadId, CONVERT(nvarchar, A.ActividadFecha, 103) + ' ' + CONVERT(nvarchar, A.ActividadFecha, 108) as ActividadFecha, " +
                    "case a.ActividadEstado " +
                    "when 1 then 'Abierto' " +
                    "when 2 then 'Pendiente' " +
                    "when 3 then 'Autorizado' " +
                    "when 4 then 'Despachado' " +
                    "when 5 then 'Ejecutado' " +
                    "when 6 then 'Rechazado' " +
                    "when 7 then 'PendienteTrade' " +
                    "end as ActividadEstado , " +
                    "a.ActividadTitulo,c.ClienteID, C.ClienteRazonSocial, U.UsuarioId + ' - ' + U.UsuarioNombre AS Usuario " +
                    "FROM Actividad AS A " +
                    "INNER JOIN Cliente AS C ON A.ClienteID = C.ClienteID " +
                    "INNER JOIN Usuario AS U ON A.UsuarioIdElabora = U.UsuarioId " +
                    ") AS T " +
                    sqlWhere;


                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY ActividadId DESC) AS Seq, * FROM ( " +
                     "SELECT a.ActividadId, CONVERT(nvarchar, A.ActividadFecha, 103) + ' ' + CONVERT(nvarchar, A.ActividadFecha, 108) as ActividadFecha, " +
                    "case a.ActividadEstado " +
                    "when 1 then 'Abierto' " +
                    "when 2 then 'Pendiente' " +
                    "when 3 then 'Autorizado' " +
                    "when 4 then 'Despachado' " +
                    "when 5 then 'Ejecutado' " +
                    "when 6 then 'Rechazado' " +
                    "when 7 then 'PendienteTrade' " +
                    "end as ActividadEstado , " +
                    "a.ActividadTitulo,c.ClienteID, C.ClienteRazonSocial, U.UsuarioId + ' - ' + U.UsuarioNombre AS Usuario " +
                    "FROM Actividad AS A " +
                    "INNER JOIN Cliente AS C ON A.ClienteID = C.ClienteID " +
                    "INNER JOIN Usuario AS U ON A.UsuarioIdElabora = U.UsuarioId " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";
                }
                else
                {
                    sqlCount =
                    "SELECT COUNT(ActividadId) FROM ( " +
                    "SELECT a.ActividadId, CONVERT(nvarchar, A.ActividadFecha, 103) + ' ' + CONVERT(nvarchar, A.ActividadFecha, 108) as ActividadFecha, " +
                    "case a.ActividadEstado " +
                    "when 1 then 'Abierto' " +
                    "when 2 then 'Pendiente' " +
                    "when 3 then 'Autorizado' " +
                    "when 4 then 'Despachado' " +
                    "when 5 then 'Ejecutado' " +
                    "when 6 then 'Rechazado' " +
                    "when 7 then 'PendienteTrade' " +
                    "end as ActividadEstado , " +
                    "a.ActividadTitulo,c.ClienteID, C.ClienteRazonSocial, U.UsuarioId + ' - ' + U.UsuarioNombre AS Usuario " +
                    "FROM Actividad AS A " +
                    "INNER JOIN Cliente AS C ON A.ClienteID = C.ClienteID " +
                    "INNER JOIN Usuario AS U ON A.UsuarioIdElabora = U.UsuarioId " +
                    ") AS T  where 1 = 0";

                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY ActividadId DESC) AS Seq, * FROM ( " +
                     "SELECT a.ActividadId, CONVERT(nvarchar, A.ActividadFecha, 103) + ' ' + CONVERT(nvarchar, A.ActividadFecha, 108) as ActividadFecha, " +
                    "case a.ActividadEstado " +
                    "when 1 then 'Abierto' " +
                    "when 2 then 'Pendiente' " +
                    "when 3 then 'Autorizado' " +
                    "when 4 then 'Despachado' " +
                    "when 5 then 'Ejecutado' " +
                    "when 6 then 'Rechazado' " +
                    "when 7 then 'PendienteTrade' " +
                    "end as ActividadEstado , " +
                    "a.ActividadTitulo,c.ClienteID, C.ClienteRazonSocial, U.UsuarioId + ' - ' + U.UsuarioNombre AS Usuario " +
                    "FROM Actividad AS A " +
                    "INNER JOIN Cliente AS C ON A.ClienteID = C.ClienteID " +
                    "INNER JOIN Usuario AS U ON A.UsuarioIdElabora = U.UsuarioId " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN 0 AND 0";
                }

                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
        public IHttpActionResult PIV(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string cliente = form["_cliente"];


                string sqlWhere = "";


                int? ColeccionPIVId = db.Clientes.Where(c => c.ClienteID == cliente).Select(c => c.ColeccionPIVId).FirstOrDefault() ?? 0;


                if (ColeccionPIVId != 0)
                    sqlWhere += "ColeccionPIVId = " + ColeccionPIVId + " AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";
                if (sqlWhere.Trim() != "")
                {
                    sqlCount =
                    "SELECT COUNT(LiquidacionId) FROM ( " +
                    "SELECT * FROM PIVView " +
                    ") AS T " +
                    sqlWhere;


                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY LiquidacionId DESC) AS Seq, * FROM ( " +
                     "SELECT * FROM PIVView " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";
                }
                else
                {
                    sqlCount =
                    "SELECT COUNT(LiquidacionId) FROM ( " +
                    "select * from PIVView" +
                    ") AS T  where 1 = 0";

                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY LiquidacionId DESC) AS Seq, * FROM ( " +
                     "SELECT * FROM PIVView " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN 0 AND 0";
                }

                //int count = (int)db.FillScalarSql(sqlCount);
                //var data = db.FillDynamicCollectionSql(sqlData);

                int count = db.Database.SqlQuery<int>(sqlCount).FirstOrDefault();
                //var data = db.Database.SqlQuery<ClienteResumenViewModel.LiquidacionInfo>(sqlData).ToList();

                //int count = (int)db.fillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
        public IHttpActionResult Ventas(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string cliente = form["_cliente"];


                string sqlWhere = "";



                if (cliente.Trim() != "")
                    sqlWhere += "Customer like '%" + cliente + "%' AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";
                if (sqlWhere.Trim() != "")
                {
                    sqlCount =
                    "SELECT COUNT(Customer) FROM ( " +
                     "SELECT Customer,CustomerName, [Brand Description] as Brand, " +
                    " sum([Billed Qty.]) as Qty, sum(NetValue) as Amount " +
                    " FROM ApexRep.dbo.ReporteVentas " +
                    " where [Brand Code] not in ('T40','H45') " +
                    " group by Customer,CustomerName,[Brand Description] " +
                    ") AS T " +
                    sqlWhere;
                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY Customer DESC) AS Seq, * FROM ( " +
                    "SELECT Customer,CustomerName, [Brand Description] as Brand, " +
                    " sum([Billed Qty.]) as Qty, sum(NetValue) as Amount " +
                    " FROM ApexRep.dbo.ReporteVentas " +
                    " where [Brand Code] not in ('T40','H45') " +
                    " group by Customer,CustomerName,[Brand Description] " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";
                }
                else
                {
                    sqlCount = "select COUNT(Customer)  from (" +
                               "SELECT Customer,CustomerName, [Brand Description] as Brand, " +
                               " sum([Billed Qty.]) as Qty, sum(NetValue) as Amount " +
                               " FROM ApexRep.dbo.ReporteVentas " +
                               " where [Brand Code] not in ('T40','H45') " +
                               " group by Customer,CustomerName,[Brand Description] " +
                               $") AS T where 1 = 0";
                    sqlData =
                      "SELECT * FROM ( " +
                      "SELECT ROW_NUMBER() OVER (ORDER BY Customer DESC) AS Seq, * FROM ( " +
                     "SELECT Customer,CustomerName, [Brand Description] as Brand, " +
                     " sum([Billed Qty.]) as Qty, sum(NetValue) as Amount " +
                     " FROM ApexRep.dbo.ReporteVentas " +
                     " where [Brand Code] not in ('T40','H45') " +
                     " group by Customer,CustomerName,[Brand Description] " +
                      ") AS T " +
                      sqlWhere +
                      $") AS T2 WHERE Seq BETWEEN 0 AND 0";
                }

                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
        public IHttpActionResult Cartera(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string cliente = form["_cliente"];


                string sqlWhere = "";



                if (cliente.Trim() != "")
                    sqlWhere += "CustomerNo like '%" + cliente + "%' AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";
                if (sqlWhere.Trim() != "")
                {
                    sqlCount =
                    "SELECT COUNT(CustomerNo) FROM ( " +
                     "SELECT  CustomerNo,CustomerName, sum(LocalCurrencyAmount) as LocalCurrencyAmount, " +
                     "sum([Days-1-30]) as Age1, sum([Days-31-60]) as Age2, sum([Days-61-90]) as Age3, sum([Days-91-120]) as Age4, sum([Days-120]) as Age5 " +
                    "FROM ApexRep.dbo.ZFIAR " +
                    "where CustomerNo is not null " +
                    "group by CustomerNo,CustomerName, Currency " +
                    ") AS T " +
                    sqlWhere;
                    sqlData =
                     "SELECT * FROM ( " +
                     "SELECT ROW_NUMBER() OVER (ORDER BY CustomerNo DESC) AS Seq, * FROM ( " +
                    "SELECT  CustomerNo,CustomerName, sum(LocalCurrencyAmount) as LocalCurrencyAmount, " +
                     "sum([Days-1-30]) as Age1, sum([Days-31-60]) as Age2, sum([Days-61-90]) as Age3, sum([Days-91-120]) as Age4, sum([Days-120]) as Age5 " +
                    "FROM ApexRep.dbo.ZFIAR " +
                    "where CustomerNo is not null " +
                    "group by CustomerNo,CustomerName, Currency " +
                     ") AS T " +
                     sqlWhere +
                     $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";
                }
                else
                {
                    sqlCount = "select COUNT(CustomerNo)  from (" +
                            "SELECT  CustomerNo,CustomerName, sum(LocalCurrencyAmount) as LocalCurrencyAmount, " +
                            "sum([Days-1-30]) as Age1, sum([Days-31-60]) as Age2, sum([Days-61-90]) as Age3, sum([Days-91-120]) as Age4, sum([Days-120]) as Age5 " +
                            "FROM ApexRep.dbo.ZFIAR " +
                            "where CustomerNo is not null " +
                            "group by CustomerNo,CustomerName, Currency " +
                            $") AS T where 1 = 0";
                    sqlData =
                      "SELECT * FROM ( " +
                        "SELECT ROW_NUMBER() OVER (ORDER BY CustomerNo DESC) AS Seq, * FROM ( " +
                        "SELECT  CustomerNo,CustomerName, sum(LocalCurrencyAmount) as LocalCurrencyAmount, " +
                         "sum([Days-1-30]) as Age1, sum([Days-31-60]) as Age2, sum([Days-61-90]) as Age3, sum([Days-91-120]) as Age4, sum([Days-120]) as Age5 " +
                        "FROM ApexRep.dbo.ZFIAR " +
                        "where CustomerNo is not null " +
                        "group by CustomerNo,CustomerName, Currency " +
                      ") AS T " +
                      sqlWhere +
                      $") AS T2 WHERE Seq BETWEEN 0 AND 0";
                }

                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
        public IHttpActionResult Disponibilidad(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];


                string sloc = form["_sloc"];
                string material = form["_material"];


                string sqlWhere = "";



                if (sloc.Trim() != "")
                    sqlWhere += "SLoc like '%" + sloc + "%' AND ";

                if (material.Trim() != "")
                    sqlWhere += "(Material + MaterialDescription) like '%" + material + "%'  AND ";

                sqlWhere += "AvailableQty > 0 ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");



                string sqlCount = "";
                string sqlData = "";

                sqlCount =
                "SELECT COUNT(SLoc) FROM ( " +
                "SELECT " +
                    " z.[SLoc] AS[SLoc], " +
                    " z.[Material]  AS[Material], " +
                    " upper(isnull(m.[Description], z.MaterialDescription )) AS [MaterialDescription]," +
                    "sum(TotalStock-OpenOrdersQty-PickQty) as [AvailableQty] " +
                    "     FROM ApexRep.[dbo].[ZINV] as  z " +
                    "     left join ApexRep.[dbo].Materials as M on m.Material = z.Material " +
                    "where sloc in (3000,2001,3002) " +
                    "group by SLoc,SlocDesc, z.Material, m.Description, z.MaterialDescription" +
                ") AS T " +
                sqlWhere + " ";
                sqlData = "SELECT * FROM ( " +
                    "SELECT ROW_NUMBER() OVER (ORDER BY SLoc DESC) AS Seq, * FROM ( " +
                    "SELECT " +
                    "z.[SLoc] AS[SLoc], " +
                    "z.[Material]  AS[Material], " +
                    " upper(isnull(m.[Description], z.MaterialDescription )) AS [MaterialDescription]," +
                    "sum(TotalStock-OpenOrdersQty-PickQty) as [AvailableQty] " +
                    "     FROM ApexRep.[dbo].[ZINV] as z " +
                    "     left join ApexRep.[dbo].Materials as M on m.Material = z.Material " +
                    "where sloc in (3000,2001,3002) " +
                    "group by SLoc,SlocDesc, z.Material, m.Description, z.MaterialDescription" +
                  ") AS T   " +
                 sqlWhere +
                 $") AS T2 WHERE Seq BETWEEN {int.Parse(displayStart) + 1} AND {(int.Parse(displayStart) + int.Parse(displayLength))}";



                int count = (int)db.FillScalarSql(sqlCount);
                var data = db.FillDynamicCollectionSql(sqlData);

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
