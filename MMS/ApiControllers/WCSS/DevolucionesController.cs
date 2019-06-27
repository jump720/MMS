
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
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MMS.ApiControllers.WCSS
{
    public class DevolucionesController : ApiBaseController
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
                    "    CONVERT(nvarchar, D.FechaCreacion, 103) +' ' + CONVERT(nvarchar, D.FechaCreacion, 108) AS FechaCreacion, NroTracking," +
                    "   case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Devolucion AS D " +
                    "INNER JOIN Cliente AS C ON D.ClienteId = C.ClienteID " +
                    "LEFT JOIN Usuario AS U ON D.AnalistaId = U.UsuarioId " +
                    ") as T" +
                   sqlWhere;

                string sqlData =
                    "SELECT * FROM (" +
                    "SELECT ROW_NUMBER() OVER(ORDER BY Id DESC) AS Seq, * FROM ( " +
                    "SELECT D.Id, D.Asunto,D.ClienteId, D.ClienteId + ' - ' + C.ClienteRazonSocial AS Cliente, D.AnalistaId + ' - ' + U.UsuarioNombre AS Analista, " +
                    "CONVERT(nvarchar, D.FechaCreacion, 103) + ' ' + CONVERT(nvarchar, D.FechaCreacion, 108) AS FechaCreacion, NroTracking," +
                    "case D.Estado when 1 then 'Open' when 2 then 'In Process' when 3 then 'Completed' when 4 then 'Deleted' else 'Sin Estado'  end as Estado " +
                    "FROM Devolucion AS D " +
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

        //[HttpPost]
        //[ApiAuthorizeAction]
        //public async Task<IHttpActionResult> Index(FormDataCollection form)
        //{
        //    try
        //    {
        //        int displayStart = int.Parse(form["start"]);
        //        int displayLength = int.Parse(form["length"]);

        //        string id = form["_id"];
        //        string asunto = form["_asunto"];
        //        string cliente = form["_cliente"];
        //        string analista = form["_analista"];
        //        string fechacreacion = form["_fechacreacion"];
        //        string estado = form["_estado"];
        //        string nrotracking = form["_nrotracking"];


        //        var countQuery = db.Devoluciones.Include(d => d.Cliente).Include(d => d.Analista);

        //        var dataQuery = db.Devoluciones.Include(d => d.Cliente).Include(d => d.Analista)
        //                        .Select(d => new
        //                        {
        //                            d.Id,
        //                            d.Asunto,
        //                            Cliente = d.ClienteId + " - " + d.Cliente.ClienteRazonSocial,
        //                            Analista = d.AnalistaId + " - " + d.Analista.UsuarioNombre,
        //                            //FechaCreacion = d.FechaCreacion.ToString("dd/mm/yyyy"),
        //                            d.FechaCreacion,
        //                            d.NroTracking,
        //                            Estado = d.Estado.ToString()
        //                        });



        //        int count = await countQuery.CountAsync();

        //        var data = await dataQuery
        //            .OrderBy(a => a.Id)
        //            .Skip(displayStart).Take(displayLength).ToListAsync();



        //        return Ok(new SysDataTablePager()
        //        {
        //            draw = form["draw"],
        //            recordsTotal = count,
        //            recordsFiltered = count,
        //            data = data
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }

        //    // return InternalServerError("");
        //}


        [HttpGet]
        public async Task<IHttpActionResult> BuscarCliente(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Cliente>());

                return Ok(await db.Clientes
                    .Include(i => i.canal)
                    .Include(i => i.ciudad.departamentos.paises)
                    .Include(i => i.usuario)
                    .Where(i => (i.ClienteID.Contains(q) || i.ClienteRazonSocial.Contains(q)))
                    .Select(i => new
                    {
                        i.ClienteID,
                        i.ClienteRazonSocial,
                        Zona = i.ciudad.departamentos.paises.PaisDesc + "-" + i.ciudad.departamentos.DepartamentoDesc + "-" + i.ciudad.CiudadDesc,
                        Canal = i.canal.CanalDesc,
                        Vendedor = i.usuario.UsuarioNombre
                    })
                    .Take(50)
                    .ToListAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCliente(string id)
        {
            try
            {

                return Ok(await db.Clientes
                    .Include(i => i.canal)
                    .Include(i => i.ciudad.departamentos.paises)
                    .Include(i => i.usuario)
                    .Where(i => (i.ClienteID == id))
                    .Select(i => new
                    {
                        i.ClienteID,
                        i.ClienteRazonSocial,
                        Zona = i.ciudad.departamentos.paises.PaisDesc + "-" + i.ciudad.departamentos.DepartamentoDesc + "-" + i.ciudad.CiudadDesc,
                        Canal = i.canal.CanalDesc,
                        Vendedor = i.usuario.UsuarioNombre
                    })

                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> ClienteDescripcion(string id)
        {
            try
            {
                return Ok(await db.Clientes.Where(c => c.ClienteID == id).Select(c => c.ClienteRazonSocial).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetReturns()
        {
            try
            {
                
             
                return Ok(db.Database.SqlQuery<ActividadesView>("SELECT Titulo, Estado, Usuario, Usuario_Nombre, Cliente, Razon_social, Vendedor, Vendedor_Nombre, Tipo_Actividad, Meta_Evacuacion, Meta_venta, Canal, " +
                                                                "Canal_Nombre, Producto, Producto_Nombre, Cantidad, Valor FROM ActividadesView").ToListAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class ActividadesView
        {
            public string Titulo { get; set; }
            public string Estado { get; set; }
            public string Usuario { get; set; }
            public string Usuario_Nombre { get; set; }
            public string Cliente { get; set; }
            public string Razon_social { get; set; }
            public string Vendedor { get; set; }
            public string Vendedor_Nombre { get; set; }
            public string Tipo_Actividad { get; set; }
            public decimal Meta_Evacuacion { get; set; }//
            public decimal Meta_venta { get; set; }//
            public string Canal { get; set; }
            public string Canal_Nombre { get; set; }
            public string Producto { get; set; }
            public string Producto_Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal Valor { get; set; }

        }
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IHttpActionResult> HASH(string id)
        //{

        //    SHA1 sha1 = SHA1Managed.Create();
        //    ASCIIEncoding encoding = new ASCIIEncoding();
        //    byte[] stream = null;
        //    StringBuilder sb = new StringBuilder();
        //    stream = sha1.ComputeHash(encoding.GetBytes(id));
        //    for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);

        //    return Ok(sb.ToString().ToUpper());
        //}


        //[HttpGet]
        //public async Task<IHttpActionResult> BuscarTipoDevolucion(string q)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(q))
        //            return Ok(new List<TipoDevolucion>());

        //        return Ok(await db.TipoDevoluciones
        //           .Where(i => (i.Nombre.Contains(q)))
        //            .Select(i => new
        //            {
        //                i.Id,
        //                i.Nombre
        //            })
        //            .Take(50)
        //            .ToListAsync()
        //            );
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        //[HttpGet]
        //public async Task<IHttpActionResult> GetTipoDevolucion(int id)
        //{
        //    try
        //    {            

        //        return Ok(await db.TipoDevoluciones
        //           .Where(i => i.Id == id)
        //            .Select(i => new
        //            {
        //                i.Id,
        //                i.Nombre
        //            })
        //            .FirstOrDefaultAsync()
        //            );
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        //[HttpPost]
        //public async Task<IHttpActionResult> 
    }
}
