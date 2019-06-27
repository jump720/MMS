using LinqToExcel;
using LinqToExcel.Attributes;
using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.PIV
{
    public class DisponibilidadController : ApiBaseController
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

                string coleccionPIVId = form["_coleccionPIVId"];
                string marcaId = form["_marcaId"];
                string codigo = form["_codigo"];
                string descripcion = form["_descripcion"];

                var countQuery = db.ItemDisponibilidad
                    .Include(id => id.Item);

                var dataQuery = db.ItemDisponibilidad
                    .Include(id => id.ColeccionPIV)
                    .Include(id => id.Item)
                    .Include(id => id.Item.Marca).Include(id => id.Item);

                
                if (!string.IsNullOrWhiteSpace(coleccionPIVId))
                {
                    int value = int.Parse(coleccionPIVId);
                    countQuery = countQuery.Where(id => id.ColeccionPIVId == value);
                    dataQuery = dataQuery.Where(id => id.ColeccionPIVId == value);
                }

                if (!string.IsNullOrWhiteSpace(marcaId))
                {
                    int value = int.Parse(marcaId);
                    countQuery = countQuery.Where(id => id.Item.MarcaId == value);
                    dataQuery = dataQuery.Where(id => id.Item.MarcaId == value);
                }

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    string value = codigo.Trim();
                    countQuery = countQuery.Where(id => id.Item.Codigo.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Item.Codigo.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(descripcion))
                {
                    string value = descripcion.Trim();
                    countQuery = countQuery.Where(id => id.Item.Descripcion.Contains(value));
                    dataQuery = dataQuery.Where(id => id.Item.Descripcion.Contains(value));
                }

                int count = await countQuery.CountAsync();
                var data = await dataQuery
                    .Select(id => new
                    {
                        ColeccionPIV = new
                        {
                            id.ColeccionPIV.Id,
                            id.ColeccionPIV.Nombre
                        },
                        Item = new
                        {
                            id.Item.Id,
                            id.Item.Codigo,
                            id.Item.Descripcion,
                            Marca = id.Item.Marca.Nombre
                        },
                        
                        id.Cantidad
                    })
                    .OrderBy(id => new { id.ColeccionPIV.Nombre, id.Item.Codigo })
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

        static class FileFields
        {
            public const string Cliente = "CLIENTE CODIGO";
            public const string Codigo = "ITEM CODIGO ATG";
            public const string Cantidad = "CANTIDAD";
        }

        class StockRow
        {
            [ExcelColumn(FileFields.Cliente)]
            public string Cliente { get; set; }

            [ExcelColumn(FileFields.Codigo)]
            public string Codigo { get; set; }

            [ExcelColumn(FileFields.Cantidad)]
            public int Cantidad { get; set; }
        }

        class CodigoUnico
        {
            public int ColeccionPIVId { get; set; }
            public string Codigo { get; set; }
        }

        [HttpPost]
        [ApiAuthorizeAction("Liquidaciones/Edit")]
        public async Task<IHttpActionResult> Upload(int mes, int ano)
        {
            string filePath = "";
            try
            {
                var result = new AjaxResult();

                var httpRequest = HttpContext.Current.Request;
                if (httpRequest.Files.Count != 1)
                    return BadRequest("File not given");

                var postedFile = httpRequest.Files[0];
                var date = DateTime.Now;
                filePath = HttpContext.Current.Server.MapPath($"~/UploadFolder/PIV_StockFile_{Seguridadcll.Usuario.UsuarioId}_{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{date.Millisecond}_{Path.GetExtension(postedFile.FileName)}");
                postedFile.SaveAs(filePath);

                var excel = new ExcelQueryFactory(filePath);

                if (!excel.GetWorksheetNames().Any(s => s == "Formato"))
                    return Ok(result.False("Incorrect given File"));

                var rows = (from r in excel.WorksheetRange<StockRow>("B6", "D16384", "Formato")
                            select r).ToList();

                var allErrors = new List<string>();
                var clientesCache = new Dictionary<string, int>();
                var codigos = new List<CodigoUnico>();
                var disponibilidadArchivoItems = new List<DisponibilidadArchivoItem>();

                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];

                    if (string.IsNullOrWhiteSpace(row.Cliente) && string.IsNullOrWhiteSpace(row.Codigo) && row.Cantidad == 0)
                        continue; // omitir linea en blanco

                    int line = i + 7;
                    var errors = new List<string>();
                    int coleccionPIVId = 0, itemId = 0, cantidad = 0;

                    if (string.IsNullOrWhiteSpace(row.Cliente))
                        AddValidationError(errors, $"Field {FileFields.Cliente} is required", line);
                    else
                    {
                        string clienteId = row.Cliente.Trim();
                        coleccionPIVId = clientesCache.Where(c => c.Key == clienteId).Select(c => c.Value).FirstOrDefault();

                        if (coleccionPIVId == 0)
                        {
                            var clienteData = await db.Clientes.Where(c => c.ClienteID == clienteId).Select(c => new
                            {
                                c.ClienteID,
                                c.ColeccionPIVId
                            }).FirstOrDefaultAsync();

                            if (clienteData != null)
                            {
                                if (clienteData.ColeccionPIVId == null)
                                    coleccionPIVId = -1;
                                else
                                {
                                    clientesCache.Add(clienteData.ClienteID, (int)clienteData.ColeccionPIVId);
                                    coleccionPIVId = (int)clienteData.ColeccionPIVId;
                                }
                            }
                        }

                        if (coleccionPIVId == 0)
                            AddValidationError(errors, $"The Customer with ID '{clienteId}' does not exist", line);
                        else if (coleccionPIVId == -1)
                            AddValidationError(errors, $"The Customer with ID '{clienteId}' does not belong to any PIP Collection", line);
                    }

                    if (string.IsNullOrWhiteSpace(row.Codigo))
                        AddValidationError(errors, $"Field {FileFields.Codigo} is required", line);
                    else
                    {
                        string codigo = row.Codigo.Trim();

                        if (codigos.Any(c => c.ColeccionPIVId == coleccionPIVId && c.Codigo.ToLower() == codigo.ToLower()))
                            AddValidationError(errors, $"The Item with Code '{codigo}' and Customer Id '{row.Cliente.Trim()}' is duplicated", line);
                        else
                        {
                            itemId = await db.Item.Where(it => it.Codigo == codigo).Select(it => it.Id).FirstOrDefaultAsync();
                            if (itemId == 0)
                                AddValidationError(errors, $"The Item with Code '{codigo}' does not exist", line);
                            else
                                codigos.Add(new CodigoUnico
                                {
                                    ColeccionPIVId = coleccionPIVId,
                                    Codigo = codigo
                                });
                        }
                    }

                    if (row.Cantidad == 0 || row.Cantidad < 1)
                        AddValidationError(errors, $"Field {FileFields.Cantidad} must be a Number grather than 0", line);
                    else
                        cantidad = row.Cantidad;

                    if (errors.Count == 0)
                    {
                        var itemDisponibilidad = await db.ItemDisponibilidad.Where(d => d.ColeccionPIVId == coleccionPIVId && d.ItemId == itemId).FirstOrDefaultAsync();
                        if (itemDisponibilidad == null)
                        {
                            itemDisponibilidad = new ItemDisponibilidad()
                            {
                                ColeccionPIVId = coleccionPIVId,
                                ItemId = itemId,
                                Cantidad = cantidad
                            };
                            db.ItemDisponibilidad.Add(itemDisponibilidad);
                        }
                        else
                        {
                            itemDisponibilidad.Cantidad += cantidad;
                            db.Entry(itemDisponibilidad).State = EntityState.Modified;
                        }

                        disponibilidadArchivoItems.Add(new DisponibilidadArchivoItem
                        {
                            ColeccionPIVId = coleccionPIVId,
                            ItemId = itemId,
                            Cantidad = cantidad
                        });
                    }
                    else
                        allErrors.AddRange(errors);
                }

                if (allErrors.Count > 0)
                    return Ok(result.False("validation", allErrors));

                var disponibilidadArchivo = new DisponibilidadArchivo
                {
                    FechaSubida = DateTime.Now,
                    UsuarioId = Seguridadcll.Usuario.UsuarioId,
                    DisponibilidadArchivoItems = disponibilidadArchivoItems,
                    Ano = ano,
                    Mes = mes
                };

                db.DisponibilidadArchivo.Add(disponibilidadArchivo);

                await db.SaveChangesAsync();
                AddLog("", "", new { DisponibilidadArchivo = disponibilidadArchivo, DisponibilidadArchivoItems = disponibilidadArchivoItems });

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
