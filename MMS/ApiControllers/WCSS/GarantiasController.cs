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

namespace MMS.ApiControllers.WCSS
{
    public class GarantiasController : ApiBaseController
    {


        private MMSContext db = new MMSContext();

        [HttpPost]
        [ApiAuthorizeAction]
        public IHttpActionResult Index(FormDataCollection form)
        {
            try
            {
                string displayStart = form["start"];
                string displayLength = form["length"];

                string id = form["_id"];
                string asunto = form["_asunto"];
                string cliente = form["_cliente"];
                string analista = form["_analista"];
                string fechacreacion = form["_fechacreacion"];
                string estado = form["_estado"];
                string nrotracking = form["_nrotracking"];

                string sqlWhere = "";

                if (id.Trim() != "")
                    sqlWhere += "Id = " + id + " AND ";

                if (asunto.Trim() != "")
                    sqlWhere += "Asunto LIKE '%" + asunto + "%' AND ";

                if (cliente.Trim() != "")
                    sqlWhere += "Cliente LIKE '%" + cliente + "%' AND ";

                if (analista.Trim() != "")
                    sqlWhere += "Analista LIKE '%" + analista + "%' AND ";

                if (fechacreacion.Trim() != "")
                    sqlWhere += "FechaCreacion LIKE '%" + fechacreacion + "%' AND ";


                if (estado.Trim() != "")
                    sqlWhere += "Estado  LIKE '%" + estado + "%' AND ";

                if (nrotracking.Trim() != "")
                    sqlWhere += "NroTracking  LIKE '%" + nrotracking + "%' AND ";

                string clienteIds = string.Join(" , ", Seguridadcll.ClienteList.ToList().Select(e => "'" + e.ClienteID + "'"));

                if (clienteIds.Trim() != "")
                    sqlWhere += "ClienteId  in (" + clienteIds + ") AND ";
                else
                    sqlWhere += " 0 = 1  AND ";


                if (sqlWhere.Trim() != "")
                    sqlWhere = " WHERE " + sqlWhere;

                sqlWhere = Fn.RemoveLastString(sqlWhere, "AND ");

                string sqlCount =
                    "SELECT COUNT(Id) FROM ( " +
                    "SELECT D.Id,D.Asunto,D.ClienteId, D.ClienteId + ' - ' + C.ClienteRazonSocial AS Cliente, D.AnalistaId + ' - ' + U.UsuarioNombre AS Analista, " +
                    "    CONVERT(nvarchar, D.FechaCreacion, 103) +' ' + CONVERT(nvarchar, D.FechaCreacion, 108) AS FechaCreacion,  NroTracking," +
                    "   case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Garantia AS D " +
                    "INNER JOIN Cliente AS C ON D.ClienteId = C.ClienteID " +
                    "LEFT JOIN Usuario AS U ON D.AnalistaId = U.UsuarioId " +
                    ") as T" +
                   sqlWhere;

                string sqlData =
                    "SELECT * FROM (" +
                    "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS Seq, * FROM ( " +
                    "SELECT D.Id, D.Asunto,D.ClienteId, D.ClienteId + ' - ' + C.ClienteRazonSocial AS Cliente, D.AnalistaId + ' - ' + U.UsuarioNombre AS Analista, " +
                    "CONVERT(nvarchar, D.FechaCreacion, 103) + ' ' + CONVERT(nvarchar, D.FechaCreacion, 108) AS FechaCreacion,  NroTracking," +
                    "case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Garantia AS D " +
                    "INNER JOIN Cliente AS C ON D.ClienteId = C.ClienteID " +
                    "LEFT JOIN Usuario AS U ON D.AnalistaId = U.UsuarioId " +
                    ") as T" +
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

            // return InternalServerError("");
        }

    }
}
