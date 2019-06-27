using LinqToExcel;
using LinqToExcel.Attributes;
using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.PIV
{
    public class LiquidacionesController : ApiBaseController
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

                int count = await db.Liquidacion
                    .Where(c => c.Descripcion.Contains(search))
                    .CountAsync();

                var data = await db.Liquidacion
                    .Select(c => new { c.Id, c.Descripcion, c.FechaInicial, c.FechaFinal, Estado = c.Estado.ToString() })
                    .Where(c => c.Descripcion.Contains(search))
                    .OrderByDescending(c => c.Id)
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
        public async Task<IHttpActionResult> Create(Liquidacion liquidacion)
        {
            try
            {
                db.Liquidacion.Add(liquidacion);
                await db.SaveChangesAsync();
                AddLog("", liquidacion.Id, liquidacion);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Liquidacion liquidacion)
        {
            try
            {
                db.Entry(liquidacion).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", liquidacion.Id, liquidacion);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                var liquidacion = await db.Liquidacion.FindAsync(id);
                if (liquidacion == null)
                    return NotFound();

                db.Liquidacion.Remove(liquidacion);
                await db.SaveChangesAsync();
                AddLog("", liquidacion.Id, liquidacion);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> Files(int id)
        {
            try
            {
                var liquidacionArchivos = (await db.LiquidacionArchivo
                    .Include(la => la.Usuario)
                    .Include(la => la.ColeccionPIV)
                    .Where(la => la.LiquidacionId == id)
                    .Select(la => new
                    {
                        la.Id,
                        la.Descripcion,
                        UsuarioNombre = la.Usuario.UsuarioNombre,
                        la.FechaSubida,
                        ColeccionPIVNombre = la.ColeccionPIV.Nombre
                    })
                    .OrderBy(la => la.FechaSubida)
                    .ToListAsync())
                    .Select(la => new
                    {
                        la.Id,
                        la.Descripcion,
                        la.UsuarioNombre,
                        FechaSubida = la.FechaSubida.ToShortDateString() + " " + la.FechaSubida.ToShortTimeString(),
                        la.ColeccionPIVNombre
                    })
                    .ToList();

                return Ok(liquidacionArchivos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        static class FileFields
        {
            public const string Mes = "MES";
            public const string Año = "AÑO";
            public const string Tipo = "TIPO";
            public const string Id = "ID";
            public const string Codigo = "CODIGO PRODUCTO";
            public const string Cantidad = "VENTA UND";
            public const string ValorTotal = "VENTA PESOS";
        }

        class AsesorVenta
        {
            [ExcelColumn(FileFields.Mes)]
            public int Mes { get; set; }

            [ExcelColumn(FileFields.Año)]
            public int Año { get; set; }

            [ExcelColumn(FileFields.Id)]
            public string Id { get; set; }

            [ExcelColumn(FileFields.Tipo)]
            public string Tipo { get; set; }

            [ExcelColumn(FileFields.Codigo)]
            public string Codigo { get; set; }

            [ExcelColumn(FileFields.Cantidad)]
            public int Cantidad { get; set; }

            [ExcelColumn(FileFields.ValorTotal)]
            public decimal ValorTotal { get; set; }
        }

        class AsesorCache
        {
            public int Id { get; set; }
            public string Cedula { get; set; }
            public int ColeccionPIVId { get; set; }
        }

        class AprobacionRemove
        {
            public LiquidacionAprobacion LiquidacionAprobacion { get; set; }
            public bool Remove { get; set; }
        }

        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> UploadFile(int id, string desc, int coleccionPIVId)
        {
            string filePath = "";
            try
            {
                var result = new AjaxResult();
                var liquidacion = await db.Liquidacion
                    .Include(l => l.LiquidacionAprobaciones)
                    .Where(l => l.Id == id)
                    .FirstOrDefaultAsync();

                if (liquidacion == null)
                    return NotFound();

                if (liquidacion.Estado == EstadoLiquidacion.Closed)
                    return Ok(result.False("This Settlement is closed"));

                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count != 1)
                    return BadRequest("File not given");

                var postedFile = httpRequest.Files[0];
                var date = DateTime.Now;
                filePath = HttpContext.Current.Server.MapPath($"~/UploadFolder/PIV_SalesFile_{Seguridadcll.Usuario.UsuarioId}_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}_{Path.GetExtension(postedFile.FileName)}");
                postedFile.SaveAs(filePath);

                var liquidacionArchivo = new LiquidacionArchivo
                {
                    LiquidacionId = id,
                    Descripcion = desc,
                    FechaSubida = DateTime.Now,
                    UsuarioId = Seguridadcll.Usuario.UsuarioId,
                    ColeccionPIVId = coleccionPIVId
                };

                var excel = new ExcelQueryFactory(filePath);

                if (!excel.GetWorksheetNames().Any(s => s == "Formato"))
                    return Ok(result.False("Incorrect given File"));

                var rows = (from r in excel.WorksheetRange<AsesorVenta>("B6", "H16384", "Formato")
                            select r).ToList();

                var aprobaciones = liquidacion.LiquidacionAprobaciones.Select(la => new AprobacionRemove { Remove = false, LiquidacionAprobacion = la }).ToList();
                var liquidacionAsesores = new List<LiquidacionAsesor>();
                var allErrors = new List<string>();

                var clientesCache = new Dictionary<string, int?>();
                var asesoresCache = new List<AsesorCache>();
                var itemsCache = new Dictionary<int, string>();

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];

                    if (row.Mes == 0 && row.Año == 0 && string.IsNullOrWhiteSpace(row.Tipo) && string.IsNullOrWhiteSpace(row.Id) && string.IsNullOrWhiteSpace(row.Codigo) && row.Cantidad == 0 && row.ValorTotal == 0)
                        continue; // omitir linea en blanco

                    int line = i + 7;
                    var errors = new List<string>();
                    bool timeFields = true;
                    var liquidacionAsesor = new LiquidacionAsesor();
                    var liquidacionItem = new LiquidacionItem();
                    string tipo = null;

                    if (row.Mes < 1 || row.Mes > 12)
                    {
                        AddValidationError(errors, $"Field {FileFields.Mes} must be between 1 and 12", line);
                        timeFields = false;
                    }

                    if (row.Año < 1)
                    {
                        AddValidationError(errors, $"Field {FileFields.Año} must be greater than 0", line);
                        timeFields = false;
                    }

                    if (timeFields)
                    {
                        var rowDate = new DateTime(row.Año, row.Mes, 1);
                        if (rowDate < liquidacion.FechaInicial || rowDate > liquidacion.FechaFinal)
                            AddValidationError(errors, $"Date Fields must be between {liquidacion.FechaInicial.Month}-{liquidacion.FechaInicial.Year} and {liquidacion.FechaFinal.Month}-{liquidacion.FechaFinal.Year}", line);
                        else
                        {
                            liquidacionItem.Mes = row.Mes;
                            liquidacionItem.Ano = row.Año;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(row.Tipo))
                        AddValidationError(errors, $"Field {FileFields.Tipo} is required", line);
                    else if (row.Tipo.ToLower() != "asesor" && row.Tipo.ToLower() != "cliente")
                        AddValidationError(errors, $"Field {FileFields.Tipo} must be 'ASESOR' or 'CLIENTE'", line);
                    else
                        tipo = row.Tipo.ToLower();

                    if (string.IsNullOrWhiteSpace(row.Id))
                        AddValidationError(errors, $"Field {FileFields.Id} is required", line);
                    else if (tipo != null)
                    {
                        if (tipo == "asesor")
                        {
                            string cedula = row.Id.Trim();
                            var asesor = asesoresCache.FirstOrDefault(a => a.Cedula == cedula);

                            if (asesor == null)
                            {
                                asesor = await db.Asesor.Where(a => a.Cedula == cedula).Select(a => new AsesorCache
                                {
                                    Id = a.Id,
                                    Cedula = a.Cedula,
                                    ColeccionPIVId = a.ColeccionPIVId
                                }).FirstOrDefaultAsync();

                                if (asesor != null)
                                    asesoresCache.Add(asesor);
                            }

                            if (asesor == null)
                                AddValidationError(errors, $"The Seller with ID '{cedula}' does not exist", line);
                            else if (asesor.ColeccionPIVId != coleccionPIVId)
                                AddValidationError(errors, $"The Seller with ID '{cedula}' does not belong to selected PIP Collection", line);
                            else
                            {
                                liquidacionAsesor.AsesorId = asesor.Id;
                                var aprobacion = aprobaciones.Where(a => a.LiquidacionAprobacion.AsesorId == asesor.Id && !a.Remove).FirstOrDefault();

                                if (aprobacion != null)
                                    aprobacion.Remove = true;
                            }
                        }
                        else
                        {
                            string clienteId = row.Id.Trim();
                            int? clienteColeccionPIVId = clientesCache.Where(c => c.Key == clienteId).Select(c => c.Value).FirstOrDefault();

                            if (clienteColeccionPIVId == null)
                            {
                                var clienteData = await db.Clientes.Where(c => c.ClienteID == clienteId).Select(c => new
                                {
                                    c.ClienteID,
                                    c.ColeccionPIVId
                                }).FirstOrDefaultAsync();

                                if (clienteData != null)
                                {
                                    clientesCache.Add(clienteData.ClienteID, clienteData.ColeccionPIVId);
                                    clienteColeccionPIVId = clienteData.ColeccionPIVId;
                                }
                            }

                            if (clienteColeccionPIVId == null)
                                AddValidationError(errors, $"The Customer with ID '{clienteId}' does not exist", line);
                            else if (clienteColeccionPIVId != coleccionPIVId)
                                AddValidationError(errors, $"The Customer with ID '{clienteId}' does not belong to selected PIP Collection", line);
                            else
                                liquidacionAsesor.AsesorId = null;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(row.Codigo))
                        AddValidationError(errors, $"Field {FileFields.Codigo} is required", line);
                    else
                    {
                        string codigo = row.Codigo.Trim();
                        int itemId = itemsCache.Where(it => it.Value == codigo).Select(it => it.Key).FirstOrDefault();

                        if (itemId == 0)
                        {
                            itemId = await db.ColeccionPIVItem.Where(it => it.ColeccionPIVId == coleccionPIVId && it.Codigo == codigo).Select(it => it.Id).FirstOrDefaultAsync();
                            if (itemId != 0)
                                itemsCache.Add(itemId, codigo);
                        }

                        if (itemId == 0)
                            AddValidationError(errors, $"The Item with Code '{codigo}' does not exist in selected PIP Collection", line);
                        else
                            liquidacionItem.ColeccionPIVItemId = itemId;
                    }

                    if (row.Cantidad == 0 /*|| row.Cantidad < 1*/)
                        AddValidationError(errors, $"Field {FileFields.Cantidad} must be a Number grather than 0", line);
                    else
                        liquidacionItem.Cantidad = row.Cantidad;

                    if (row.ValorTotal == 0 /*|| row.ValorTotal < 1*/)
                        AddValidationError(errors, $"Field {FileFields.ValorTotal} must be a Number grather than 0", line);
                    else
                        liquidacionItem.ValorTotal = row.ValorTotal;

                    if (errors.Count == 0)
                    {
                        var liquidacionAsesorProcesar = liquidacionAsesores
                            .Where(la => la.AsesorId == liquidacionAsesor.AsesorId)
                            .FirstOrDefault();

                        if (liquidacionAsesorProcesar == null)
                        {
                            liquidacionAsesorProcesar = liquidacionAsesor;
                            liquidacionAsesorProcesar.LiquidacionItems = new List<LiquidacionItem>();
                            liquidacionAsesores.Add(liquidacionAsesorProcesar);
                        }

                        liquidacionAsesorProcesar.LiquidacionItems.Add(liquidacionItem);
                    }
                    else
                        allErrors.AddRange(errors);
                }

                if (allErrors.Count > 0)
                    return Ok(result.False("validation", allErrors));

                var aprobacioneToRemove = aprobaciones.Where(a => a.Remove).Select(a => a.LiquidacionAprobacion).ToList();
                if (aprobacioneToRemove.Count > 0)
                    db.LiquidacionAprobacion.RemoveRange(aprobacioneToRemove);

                liquidacionArchivo.LiquidacionAsesores = liquidacionAsesores;
                db.LiquidacionArchivo.Add(liquidacionArchivo);
                await db.SaveChangesAsync();
                AddLog("", liquidacionArchivo.LiquidacionId, liquidacionArchivo);

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        private void AddValidationError(List<string> errors, string error, int line)
        {
            errors.Add($"Line {line}: {error}.");
        }

        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> DeleteFile(int id)
        {
            try
            {
                var liquidacionArchivo = await db.LiquidacionArchivo
                    .Include(l => l.Liquidacion)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (liquidacionArchivo == null)
                    return NotFound();

                if (liquidacionArchivo.Liquidacion.Estado == EstadoLiquidacion.Closed)
                    return BadRequest("This Settlement is closed.");

                var asesoresId = await db.LiquidacionAsesor
                    .Where(la => la.LiquidacionArchivoId == liquidacionArchivo.Id)
                    .Select(la => la.AsesorId)
                    .ToArrayAsync();

                var aprobaciones = await db.LiquidacionAprobacion
                    .Where(la => asesoresId.Contains(la.AsesorId))
                    .ToListAsync();

                if (aprobaciones.Count > 0)
                    db.LiquidacionAprobacion.RemoveRange(aprobaciones);

                db.LiquidacionArchivo.Remove(liquidacionArchivo);
                await db.SaveChangesAsync();
                AddLog("", liquidacionArchivo.LiquidacionId, liquidacionArchivo);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        class LiquidacionInfo
        {
            public ColeccionPIVInfo ColeccionPIV { get; set; }
            public AsesorInfo Asesor { get; set; }
            public decimal Total { get; set; }
            public CategoriaCDE CategoriaCDE { get; set; }
            public List<ItemInfo> Items { get; set; }

            public class ColeccionPIVInfo
            {
                public int Id { get; set; }
                public string Nombre { get; set; }
                public bool CDE { get; set; }
            }

            public class AsesorInfo
            {
                public int Id { get; set; }
                public string Cedula { get; set; }
                public string NombreCompleto { get; set; }
                public decimal Meta { get; set; }
            }

            public class ItemInfo
            {
                public int ItemId { get; set; }
                public int MarcaId { get; set; }
                public string Codigo { get; set; }
                public int Cantidad { get; set; }
                public decimal Total { get; set; }
            }
        }

        class Disponibilidad
        {
            public int ColeccionPIVId { get; set; }
            public int ItemId { get; set; }
            public int Disponible { get; set; }
            public int Vendido { get; set; }
            public int Nuevo { get; set; }
        }



        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> Manage(int id, FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string searchColeccionPIV = form["_coleccionPIV"];
                string searchCedula = form["_cedula"];
                string searchNombreCompleto = form["_nombreCompleto"];

                var liquidacion = await db.Liquidacion
                    .Include(l => l.CategoriasCDE)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (liquidacion == null)
                    return NotFound();

                List<Regla> reglas;
                List<CategoriaCDE> categorias;
                CategoriaCDE categoriaBase;
                float porcentajePIVBase;
                List<LiquidacionInfo> baseData;
                List<Disponibilidad> disponibilidad = null;
                int count;
                var data = new List<dynamic>();

                var aprobaciones = await db.LiquidacionAprobacion
                    .Include(la => la.Usuario)
                    .Where(la => la.LiquidacionId == id)
                    .Select(la => new { la.Id, la.AsesorId, la.Observacion, UsuarioNombre = la.Usuario.UsuarioNombre })
                    .ToListAsync();

                if (liquidacion.Estado == EstadoLiquidacion.Open)
                {
                    reglas = await db.Regla
                        .Include(r => r.Item)
                        .Include(r => r.Marca)
                        .Where(r => r.LiquidacionId == null && r.Activa)
                        .ToListAsync();

                    categorias = await db.CategoriaCDE.Where(c => c.LiquidacionId == null).ToListAsync();
                    porcentajePIVBase = await db.Configuracion.Select(c => c.ConfigPorcentajePIV).FirstOrDefaultAsync();

                    var itemsVendido = (await db.LiquidacionItem
                        .Include(li => li.ColeccionPIVItem)
                        .Include(li => li.LiquidacionAsesor)
                        .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                        .Where(li => li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id)
                        .Select(li => new
                        {
                            li.ColeccionPIVItem.ColeccionPIVId,
                            li.ColeccionPIVItem.ItemId,
                            li.Cantidad,
                        })
                        .ToListAsync())
                        .GroupBy(i => new { i.ColeccionPIVId, i.ItemId })
                        .Select(g => new
                        {
                            Grupo = g.FirstOrDefault(),
                            Cantidad = g.Sum(g2 => g2.Cantidad)
                        })
                        .Select(g => new
                        {
                            g.Grupo.ColeccionPIVId,
                            g.Grupo.ItemId,
                            g.Cantidad
                        })
                        .ToList();

                    var itemsDisponibilidad = new List<ItemDisponibilidad>();

                    if (itemsVendido.Count > 0)
                    {
                        var sb = new StringBuilder();
                        foreach (var iv in itemsVendido)
                            sb.Append($"'{iv.ColeccionPIVId}|{iv.ItemId}',");

                        itemsDisponibilidad = await db.ItemDisponibilidad.SqlQuery($"SELECT * FROM ItemDisponibilidad WHERE CONVERT(NVARCHAR, ColeccionPIVId) + '|' + CONVERT(NVARCHAR, ItemId) IN ({Fn.RemoveLastString(sb.ToString(), ",")})").ToListAsync();
                    }

                    disponibilidad = itemsVendido
                       .GroupJoin(itemsDisponibilidad,
                           ven => new { ven.ColeccionPIVId, ven.ItemId },
                           dis => new { dis.ColeccionPIVId, dis.ItemId },
                           (ven, g) => new { Ven = ven, Dis = g.FirstOrDefault() })
                        .Select(x => new Disponibilidad
                        {
                            ColeccionPIVId = x.Ven.ColeccionPIVId,
                            ItemId = x.Ven.ItemId,
                            Disponible = x.Dis == null ? -1 : x.Dis.Cantidad,
                            Vendido = x.Ven.Cantidad,
                            Nuevo = (x.Dis == null ? 0 : x.Dis.Cantidad) - x.Ven.Cantidad
                        })
                        .ToList();
                }
                else
                {
                    reglas = await db.Regla
                        .Include(r => r.Item)
                        .Include(r => r.Marca)
                        .Where(r => r.LiquidacionId == liquidacion.Id && r.Activa)
                        .ToListAsync();

                    categorias = liquidacion.CategoriasCDE.ToList();
                    porcentajePIVBase = (float)liquidacion.PorcentajePIV;
                }

                categoriaBase = categorias.OrderBy(c => c.ValorMinimo).First();

                var countQuery = db.LiquidacionItem
                        .Include(li => li.LiquidacionAsesor)
                        .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                        .Include(li => li.LiquidacionAsesor.Asesor)
                        .Where(li => li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id && li.LiquidacionAsesor.AsesorId != null);

                var dataQuery = db.LiquidacionItem
                    .Include(li => li.ColeccionPIVItem)
                    .Include(li => li.ColeccionPIVItem.Item)
                    .Include(li => li.LiquidacionAsesor)
                    .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                    .Include(li => li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIV)
                    .Include(li => li.LiquidacionAsesor.Asesor)
                    .Where(li => li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id);

                if (!string.IsNullOrWhiteSpace(searchColeccionPIV))
                {
                    int value = int.Parse(searchColeccionPIV);
                    countQuery = countQuery.Where(li => li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIVId == value);
                    dataQuery = dataQuery.Where(li => li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIVId == value);
                }

                var baseCountQuery = countQuery
                    .GroupBy(li => li.LiquidacionAsesor.Asesor.Id)
                    .Select(g => g.Select(li => new
                    {
                        li.LiquidacionAsesor.Asesor.Cedula,
                        NombreCompleto = li.LiquidacionAsesor.Asesor.Nombre + " " + li.LiquidacionAsesor.Asesor.Apellido1 + " " + li.LiquidacionAsesor.Asesor.Apellido2
                    }).FirstOrDefault());

                var baseDataQuery = dataQuery
                    .GroupBy(li => li.LiquidacionAsesor.AsesorId)
                    .Select(g => new
                    {
                        Items = g.Select(li => new LiquidacionInfo.ItemInfo
                        {
                            ItemId = li.ColeccionPIVItem.ItemId,
                            MarcaId = li.ColeccionPIVItem.Item.MarcaId,
                            Codigo = li.ColeccionPIVItem.Item.Codigo,
                            Cantidad = li.Cantidad,
                            Total = li.ValorTotal
                        }),
                        Data = g.Select(li => li.LiquidacionAsesor).Select(li => new
                        {
                            ColeccionPIV = new LiquidacionInfo.ColeccionPIVInfo
                            {
                                Id = li.LiquidacionArchivo.ColeccionPIV.Id,
                                Nombre = li.LiquidacionArchivo.ColeccionPIV.Nombre,
                                CDE = li.LiquidacionArchivo.ColeccionPIV.CDE
                            },
                            Asesor = li.Asesor != null ? new LiquidacionInfo.AsesorInfo
                            {
                                Id = li.Asesor.Id,
                                Cedula = li.Asesor.Cedula,
                                NombreCompleto = li.Asesor.Nombre + " " + li.Asesor.Apellido1 + " " + li.Asesor.Apellido2,
                                Meta = li.Asesor.Meta
                            } : null
                        }).FirstOrDefault(),
                        Total = g.Sum(li => li.ValorTotal)
                    })
                    .Select(g => new LiquidacionInfo
                    {
                        ColeccionPIV = g.Data.ColeccionPIV,
                        Asesor = g.Data.Asesor,
                        Items = g.Items.ToList(),
                        Total = g.Total
                    });

                if (!string.IsNullOrWhiteSpace(searchCedula))
                {
                    string value = searchCedula.Trim();
                    baseCountQuery = baseCountQuery.Where(li => li.Cedula.Contains(value));
                    baseDataQuery = baseDataQuery.Where(li => li.Asesor.Cedula.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(searchNombreCompleto))
                {
                    string value = searchNombreCompleto.Trim();
                    baseCountQuery = baseCountQuery.Where(li => li.NombreCompleto.Contains(value));
                    baseDataQuery = baseDataQuery.Where(li => li.Asesor.NombreCompleto.Contains(value));
                }

                count = await baseCountQuery.CountAsync();
                baseData = await baseDataQuery
                    .OrderBy(la => new { la.ColeccionPIV.Nombre, la.ColeccionPIV.Id })
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                foreach (var d in baseData)
                {
                    if (d.Asesor == null)
                        continue;

                    dynamic aprobacion = null;
                    var categoria = d.ColeccionPIV.CDE ? categorias.Where(c => c.ValorMinimo <= d.Asesor.Meta && c.ValorMaximo >= d.Asesor.Meta).FirstOrDefault() : null;
                    decimal porcentaje = (100 * d.Total) / d.Asesor.Meta;
                    string img = "", msg = "";
                    float porcentajeAplicado = 0;
                    decimal valorAplicado = 0, nuevoTotal = d.Total;

                    if (disponibilidad != null)
                        foreach (var item in d.Items)
                        {
                            var dispo = disponibilidad.FirstOrDefault(di => di.ColeccionPIVId == d.ColeccionPIV.Id && di.ItemId == item.ItemId);
                            if (dispo.Nuevo < 0)
                                msg += $"The Item '{item.Codigo}' has {(dispo.Disponible == -1 ? "None" : dispo.Disponible.ToString())} Stock and {dispo.Vendido} Sold \n";
                        }

                    if (msg != "")
                        img = "dead";
                    else if (d.ColeccionPIV.CDE && categoria == null)
                    {
                        img = "angry";
                        msg = "This Seller's Customer is COE but this Seller Target is not in any COE Category";
                    }
                    else if (porcentaje >= 100)
                    {
                        img = "happy";
                        msg = $"Target achieved \n {(d.Total - d.Asesor.Meta).ToString("C")} (COP) over Target";

                        if (d.ColeccionPIV.CDE)
                            porcentajeAplicado = categoria.Porcentaje;
                        else
                            porcentajeAplicado = porcentajePIVBase;
                    }
                    else if (porcentaje < 100)
                    {
                        if (d.ColeccionPIV.CDE)
                        {
                            if (d.Total >= categoriaBase.ValorMinimo && categoria.Id != categoriaBase.Id)
                            {
                                porcentajeAplicado = categoriaBase.Porcentaje;

                                img = "embarrass";
                                msg = $"Target not achieved \n {(d.Asesor.Meta - d.Total).ToString("C")} (COP) left. \n Base COE Category achieved";
                            }
                            else
                            {
                                img = "cry";
                                msg = $"Target not achieved \n {(d.Asesor.Meta - d.Total).ToString("C")} (COP) left for Target.";

                                if (categoria.Id != categoriaBase.Id)
                                    msg += $"\n {(categoriaBase.ValorMinimo - d.Total).ToString("C")} left for Base COE Category";
                            }
                        }
                        else
                        {
                            img = "cry";
                            msg = $"Target not achieved \n {(d.Asesor.Meta - d.Total).ToString("C")} (COP) left";
                        }

                        aprobacion = aprobaciones.Where(la => la.AsesorId == d.Asesor.Id).Select(la => new { la.Id, la.Observacion, la.UsuarioNombre }).FirstOrDefault();
                        if (aprobacion != null)
                        {
                            img = "love";
                            msg += "\n --------------- \n Achieved by approval";

                            if (d.ColeccionPIV.CDE)
                                porcentajeAplicado = categoria.Porcentaje;
                            else
                                porcentajeAplicado = porcentajePIVBase;
                        }
                    }

                    string msgReglas = "";
                    decimal valorReglas = 0;

                    if (porcentajeAplicado > 0)
                    {
                        reglas.ForEach(r =>
                        {
                            decimal total = 0, valorRegla = 0;
                            string tipo = "";

                            if (r.ItemId != null)
                            {
                                total = d.Items.Where(i => i.ItemId == r.ItemId).Sum(i => i.Total);
                                tipo = r.Item.Codigo;
                            }
                            else
                            {
                                total = d.Items.Where(i => i.MarcaId == r.MarcaId).Sum(i => i.Total);
                                tipo = r.Marca.Nombre;
                            }

                            if (total >= r.Meta)
                            {
                                valorRegla = (total * (decimal)r.Porcentaje) / 100;
                                msgReglas += $"{tipo} : {r.Porcentaje} % : {valorRegla.ToString("C")} (COP) \n";
                                valorReglas += valorRegla;
                                nuevoTotal -= total;
                            }
                        });

                        if (nuevoTotal < 0)
                            nuevoTotal = 0;

                        valorAplicado = (nuevoTotal * (decimal)porcentajeAplicado) / 100;
                    }

                    data.Add(new
                    {
                        Aprobacion = aprobacion,
                        d.ColeccionPIV,
                        d.Asesor,
                        d.Total,
                        CategoriaCDE = categoria,
                        Porcentaje = porcentaje,
                        Img = img,
                        Msg = msg,
                        PorcentajePIVBase = porcentajePIVBase,
                        PorcentajeAplicado = porcentajeAplicado,
                        ValorAplicado = valorAplicado,
                        ValorReglas = valorReglas,
                        MsgReglas = msgReglas
                    });
                }

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
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> ApproveSeller(int id, LiquidacionAprobacion liquidacionAprobacion)
        {
            try
            {
                var result = new AjaxResult();

                var liquidacion = await db.Liquidacion.FindAsync(id);
                if (liquidacion == null)
                    return NotFound();

                if (liquidacion.Estado == EstadoLiquidacion.Closed)
                    return Ok(result.False("Current Settlement is closed."));

                if (await db.LiquidacionAprobacion.AnyAsync(la => la.LiquidacionId == id && la.AsesorId == liquidacionAprobacion.AsesorId))
                    return Ok(result.False("This Seller has already been approved."));

                var asesorData = await db.LiquidacionItem
                    .Include(li => li.LiquidacionAsesor)
                    .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                    .Include(li => li.LiquidacionAsesor.Asesor)
                    .Include(li => li.LiquidacionAsesor.Asesor.ColeccionPIV)
                    .Where(li => li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id && li.LiquidacionAsesor.AsesorId == liquidacionAprobacion.AsesorId)
                    .GroupBy(li => li.LiquidacionAsesor.AsesorId)
                    .Select(li => new
                    {
                        Asesor = li.Select(g => new
                        {
                            g.LiquidacionAsesor.Asesor.Meta
                        }).FirstOrDefault(),
                        Total = li.Sum(g => g.ValorTotal)
                    })
                    .FirstOrDefaultAsync();

                if (asesorData == null)
                    return BadRequest();

                if (asesorData.Total >= asesorData.Asesor.Meta)
                    return Ok(result.False("Seller's Target it's already achieved."));

                liquidacionAprobacion.LiquidacionId = id;
                liquidacionAprobacion.UsuarioId = Seguridadcll.Usuario.UsuarioId;

                db.LiquidacionAprobacion.Add(liquidacionAprobacion);
                await db.SaveChangesAsync();
                AddLog("", liquidacionAprobacion.LiquidacionId, liquidacionAprobacion);

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> DisapproveSeller(int id)
        {
            try
            {
                var liquidacionAprobacion = await db.LiquidacionAprobacion.FindAsync(id);
                if (liquidacionAprobacion == null)
                    return NotFound();

                db.LiquidacionAprobacion.Remove(liquidacionAprobacion);
                await db.SaveChangesAsync();
                AddLog("", liquidacionAprobacion.LiquidacionId, liquidacionAprobacion);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> Close(int id)
        {
            try
            {
                var liquidacion = await db.Liquidacion.FindAsync(id);
                if (liquidacion == null)
                    return NotFound();

                if (liquidacion.Estado == EstadoLiquidacion.Closed)
                    return BadRequest("This Settlement is already closed.");

                var liquidacionAprobaciones = await db.LiquidacionAprobacion
                    .Where(la => la.LiquidacionId == id)
                    .Select(la => la.AsesorId)
                    .ToListAsync();

                var liquidacionCategorias = (await db.CategoriaCDE
                    .Where(c => c.LiquidacionId == null)
                    .ToListAsync())
                    .Select(c => new CategoriaCDE
                    {
                        Nombre = c.Nombre,
                        ValorMinimo = c.ValorMinimo,
                        ValorMaximo = c.ValorMaximo,
                        Porcentaje = c.Porcentaje,
                        Icon = c.Icon,
                        LiquidacionId = liquidacion.Id
                    }).ToList();

                var liquidacionCategoriaBase = liquidacionCategorias.OrderBy(c => c.ValorMinimo).First();

                var liquidacionReglas = (await db.Regla
                    .Where(r => r.LiquidacionId == null && r.Activa)
                    .ToListAsync())
                    .Select(r => new Regla
                    {
                        MarcaId = r.MarcaId,
                        ItemId = r.ItemId,
                        Meta = r.Meta,
                        Porcentaje = r.Porcentaje,
                        Activa = r.Activa,
                        LiquidacionId = liquidacion.Id
                    }).ToList();

                var data = await db.LiquidacionItem
                    .Include(li => li.ColeccionPIVItem)
                    .Include(li => li.ColeccionPIVItem.Item)
                    .Include(li => li.LiquidacionAsesor)
                    .Include(li => li.LiquidacionAsesor.LiquidacionArchivo)
                    .Include(li => li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIV)
                    .Include(li => li.LiquidacionAsesor.Asesor)
                    .Where(li => li.LiquidacionAsesor.LiquidacionArchivo.LiquidacionId == id)
                    .GroupBy(li => li.LiquidacionAsesor.AsesorId)
                    .Select(g => new
                    {
                        Items = g.Select(li => new
                        {
                            ColeccionPIVNombre = li.ColeccionPIVItem.ColeccionPIV.Nombre,
                            li.ColeccionPIVItem.ColeccionPIVId,
                            li.ColeccionPIVItem.ItemId,
                            li.ColeccionPIVItem.Item.MarcaId,
                            li.ColeccionPIVItem.Item.Codigo,
                            li.Cantidad,
                            li.ValorTotal
                        }),
                        Data = g.Select(li => new
                        {
                            li.LiquidacionAsesor.Asesor,
                            li.LiquidacionAsesor.LiquidacionArchivo.ColeccionPIV.CDE
                        }).FirstOrDefault(),
                        Total = g.Sum(li => li.ValorTotal)
                    })
                    .ToListAsync();

                var itemsVendido = data
                    .SelectMany(d => d.Items.Select(i => new
                    {
                        i.ColeccionPIVNombre,
                        i.ColeccionPIVId,
                        i.ItemId,
                        i.Cantidad
                    }).ToList())
                    .GroupBy(i => new { i.ColeccionPIVId, i.ItemId })
                    .Select(g => new
                    {
                        Grupo = g.FirstOrDefault(),
                        Cantidad = g.Sum(g2 => g2.Cantidad)
                    })
                    .Select(g => new
                    {
                        g.Grupo.ColeccionPIVId,
                        g.Grupo.ColeccionPIVNombre,
                        g.Grupo.ItemId,
                        g.Cantidad
                    })
                    .ToList();


                var itemsDisponibilidad = new List<ItemDisponibilidad>();

                if (itemsVendido.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var iv in itemsVendido)
                        sb.Append($"'{iv.ColeccionPIVId}|{iv.ItemId}',");

                    itemsDisponibilidad = await db.ItemDisponibilidad.SqlQuery($"SELECT * FROM ItemDisponibilidad WHERE CONVERT(NVARCHAR, ColeccionPIVId) + '|' + CONVERT(NVARCHAR, ItemId) IN ({Fn.RemoveLastString(sb.ToString(), ",")})").ToListAsync();
                }

                var disponibilidad = itemsVendido
                       .GroupJoin(itemsDisponibilidad,
                           ven => new { ven.ColeccionPIVId, ven.ItemId },
                           dis => new { dis.ColeccionPIVId, dis.ItemId },
                           (ven, g) => new { Ven = ven, Dis = g.FirstOrDefault() })
                        .Select(x => new
                        {
                            ItemDisponibilidad = x.Dis,
                            x.Ven.ColeccionPIVNombre,
                            x.Ven.ColeccionPIVId,
                            x.Ven.ItemId,
                            Disponible = x.Dis == null ? -1 : x.Dis.Cantidad,
                            Vendido = x.Ven.Cantidad,
                            NuevoSaldo = (x.Dis == null ? 0 : x.Dis.Cantidad) - x.Ven.Cantidad
                        })
                        .ToList();

                var liquidacionCierres = new List<LiquidacionCierre>();
                var errores = new List<string>();
                float porcentajePIVBase = await db.Configuracion.Select(c => c.ConfigPorcentajePIV).FirstOrDefaultAsync();

                foreach (var d in data)
                {
                    if (d.Data.Asesor == null)
                    {
                        foreach (var item in d.Items)
                        {
                            var dispo = disponibilidad.FirstOrDefault(di => di.ColeccionPIVId == item.ColeccionPIVId && di.ItemId == item.ItemId);
                            if (dispo.NuevoSaldo < 0)
                                errores.Add($"The Item '{item.Codigo}' has {(dispo.Disponible == -1 ? "None" : dispo.Disponible.ToString())} Stock and {dispo.Vendido} Sold in PIP Collection '{dispo.ColeccionPIVNombre}'\n");
                        }
                    }
                    else
                    {
                        foreach (var item in d.Items)
                        {
                            var dispo = disponibilidad.FirstOrDefault(di => di.ColeccionPIVId == item.ColeccionPIVId && di.ItemId == item.ItemId);
                            if (dispo.NuevoSaldo < 0)
                            {
                                errores.Add($"The Seller '{d.Data.Asesor.Cedula}' has Stock problems.");
                                break;
                            }
                        }

                        var categoria = d.Data.CDE ? liquidacionCategorias.Where(c => c.ValorMinimo <= d.Data.Asesor.Meta && c.ValorMaximo >= d.Data.Asesor.Meta).FirstOrDefault() : null;
                        decimal porcentaje = (100 * d.Total) / d.Data.Asesor.Meta;
                        decimal nuevoTotal = d.Total;
                        float porcentajeAplicado = 0;

                        if (d.Data.CDE && categoria == null)
                        {
                        }
                        else if (porcentaje >= 100)
                        {
                            if (d.Data.CDE)
                                porcentajeAplicado = categoria.Porcentaje;
                            else
                                porcentajeAplicado = porcentajePIVBase;
                        }
                        else if (porcentaje < 100)
                        {
                            if (d.Data.CDE && d.Total >= liquidacionCategoriaBase.ValorMinimo && categoria.Nombre != liquidacionCategoriaBase.Nombre)
                                porcentajeAplicado = liquidacionCategoriaBase.Porcentaje;

                            if (liquidacionAprobaciones.Any(a => a == d.Data.Asesor.Id))
                            {
                                if (d.Data.CDE)
                                    porcentajeAplicado = categoria.Porcentaje;
                                else
                                    porcentajeAplicado = porcentajePIVBase;
                            }
                        }

                        decimal valorReglas = 0;

                        if (porcentajeAplicado > 0)
                        {
                            liquidacionReglas.ForEach(r =>
                            {
                                decimal total = 0;

                                if (r.ItemId != null)
                                    total = d.Items.Where(i => i.ItemId == r.ItemId).Sum(i => i.ValorTotal);
                                else
                                    total = d.Items.Where(i => i.MarcaId == r.MarcaId).Sum(i => i.ValorTotal);

                                if (total >= r.Meta)
                                {
                                    valorReglas += (total * (decimal)r.Porcentaje) / 100;
                                    nuevoTotal -= total;
                                }

                                if (nuevoTotal < 0)
                                    nuevoTotal = 0;
                            });
                        }

                        liquidacionCierres.Add(new LiquidacionCierre
                        {
                            LiquidacionId = liquidacion.Id,
                            AsesorId = d.Data.Asesor.Id,
                            Meta = d.Data.Asesor.Meta,
                            CDE = d.Data.CDE,
                            Total = d.Total,
                            CategoriaCDE = categoria,
                            TotalReglas = valorReglas,
                            PorcentajeAplicado = porcentajeAplicado,
                            TotalNuevo = nuevoTotal
                        });
                    }
                }

                var result = new AjaxResult();

                if (errores.Count > 0)
                    return Ok(result.False("", errores));

                liquidacion.Estado = EstadoLiquidacion.Closed;
                liquidacion.PorcentajePIV = porcentajePIVBase;
                liquidacion.CategoriasCDE = liquidacionCategorias;
                liquidacion.Reglas = liquidacionReglas;
                liquidacion.LiquidacionCierres = liquidacionCierres;

                foreach (var item in disponibilidad)
                {
                    item.ItemDisponibilidad.Cantidad = item.NuevoSaldo;
                    db.Entry(item.ItemDisponibilidad).State = EntityState.Modified;
                }

                await db.SaveChangesAsync();
                AddLog("", liquidacion.Id, new
                {
                    PorcentajePIV = liquidacion.PorcentajePIV,
                    CategoriasCDE = liquidacion.CategoriasCDE,
                    Reglas = liquidacion.Reglas,
                    Cierre = liquidacion.LiquidacionCierres
                });
                return Ok(result.True());
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
