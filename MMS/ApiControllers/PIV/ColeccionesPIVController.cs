using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.PIV
{
    public class ColeccionesPIVController : ApiBaseController
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

                int count = await db.ColeccionPIV
                    .Where(c => c.Nombre.Contains(search))
                    .CountAsync();

                var data = await db.ColeccionPIV
                    .Select(c => new { c.Id, c.Nombre, c.CDE })
                    .Where(c => c.Nombre.Contains(search))
                    .OrderBy(c => c.Id)
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

        [HttpGet]
        public async Task<IHttpActionResult> BuscarCliente(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Cliente>());

                return Ok(await db.Clientes
                    .Include(i => i.ciudad)
                    .Include(i => i.ciudad.departamentos)
                    .Include(i => i.ciudad.departamentos.paises)
                    .Include(i => i.canal)
                    .Include(i => i.ColeccionPIV)
                    .Where(c => (c.ClienteID.Contains(q) || c.ClienteRazonSocial.Contains(q)))
                    .Select(c => new
                    {
                        Id = c.ClienteID,
                        RazonSocial = c.ClienteRazonSocial,
                        Ciudad = c.ciudad.CiudadDesc,
                        Departamento = c.ciudad.departamentos.DepartamentoDesc,
                        Pais = c.ciudad.departamentos.paises.PaisDesc,
                        Canal = c.canal.CanalDesc,
                        ColeccionPIV = c.ColeccionPIV == null ? null : new
                        {
                            c.ColeccionPIV.Id,
                            c.ColeccionPIV.Nombre
                        }
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

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Items(int id, FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.ColeccionPIVItem
                    .Include(ci => ci.Item)
                    .Where(ci => ci.ColeccionPIVId == id && (ci.Codigo.Contains(search) || ci.Item.Codigo.Contains(search)))
                    .CountAsync();

                var data = await db.ColeccionPIVItem
                    .Include(ci => ci.Item)
                    .Where(ci => ci.ColeccionPIVId == id && (ci.Codigo.Contains(search) || ci.Item.Codigo.Contains(search)))
                    .Select(c => new { c.Id, c.Codigo, Item = c.Item.Codigo + " - " + c.Item.Descripcion })
                    .OrderBy(c => c.Id)
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

        [HttpGet]
        public async Task<IHttpActionResult> ValidateCodeItem(int coleccionPIVId, string codigo, int id = 0)
        {
            try
            {
                var res = await db.ColeccionPIVItem.AnyAsync(ci => ci.ColeccionPIVId == coleccionPIVId && ci.Codigo == codigo && ci.Id != id);
                return Ok(!res);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> CreateItem(ColeccionPIVItem coleccionItem)
        {
            try
            {
                db.ColeccionPIVItem.Add(coleccionItem);
                await db.SaveChangesAsync();
                AddLog("", coleccionItem.Id.ToString(), coleccionItem);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> EditItem(ColeccionPIVItem coleccionItem)
        {
            try
            {
                db.Entry(coleccionItem).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", coleccionItem.Id.ToString(), coleccionItem);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> DeleteItem(int id)
        {
            try
            {
                var collectionItem = await db.ColeccionPIVItem.FindAsync(id);
                if (collectionItem == null)
                    return NotFound();

                db.ColeccionPIVItem.Remove(collectionItem);
                await db.SaveChangesAsync();
                AddLog("", collectionItem.Id.ToString(), collectionItem);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Asesores(int id, FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.Asesor
                    .Where(a => a.ColeccionPIVId == id && (a.Cedula.Contains(search) || (a.Nombre + " " + a.Apellido1 + " " + a.Apellido2).Contains(search)))
                    .CountAsync();

                var asesores = await db.Asesor
                    .Where(a => a.ColeccionPIVId == id && (a.Cedula.Contains(search) || (a.Nombre + " " + a.Apellido1 + " " + a.Apellido2).Contains(search)))
                    .Select(a => new { a.Id, a.Cedula, NombreCompleto = (a.Nombre + " " + a.Apellido1 + " " + a.Apellido2), a.Meta, Icon = "" })
                    .OrderBy(a => a.Id)
                    .Skip(displayStart).Take(displayLength).ToListAsync();

                var coleccion = await db.ColeccionPIV.FindAsync(id);
                if (coleccion.CDE)
                {
                    var categorias = await db.CategoriaCDE.Where(c => c.LiquidacionId == null).ToListAsync();
                    asesores = asesores.Select(a => new
                    {
                        a.Id,
                        a.Cedula,
                        a.NombreCompleto,
                        a.Meta,
                        Icon = categorias.Where(c => c.ValorMinimo <= a.Meta && c.ValorMaximo >= a.Meta).Select(c => c.Icon).FirstOrDefault()
                    })
                    .ToList();
                }

                return Ok(new SysDataTablePager()
                {
                    draw = form["draw"],
                    recordsTotal = count,
                    recordsFiltered = count,
                    data = asesores
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> ValidateCedula(string cedula, int id = 0)
        {
            try
            {
                var res = await db.Asesor.AnyAsync(a => a.Cedula == cedula && a.Id != id);
                return Ok(!res);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetCategoriaCDEIcon(decimal meta)
        {
            try
            {
                string icon = await db.CategoriaCDE.Where(c => c.LiquidacionId == null && c.ValorMinimo <= meta && c.ValorMaximo >= meta).Select(c => c.Icon).FirstOrDefaultAsync();
                return Ok(icon);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> CreateAsesor(Asesor asesor)
        {
            try
            {
                db.Asesor.Add(asesor);
                await db.SaveChangesAsync();
                AddLog("", asesor.Id, asesor);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> EditAsesor(Asesor asesor)
        {
            try
            {
                Asesor actualAsesor;
                using (var db2 = new MMSContext())
                {
                    actualAsesor = await db2.Asesor.FindAsync(asesor.Id);
                    if (actualAsesor == null)
                        return NotFound();
                }

                if (actualAsesor.Meta != asesor.Meta)
                {
                    var aprobaciones = await db.LiquidacionAprobacion
                        .Include(la => la.Liquidacion)
                        .Where(la => la.Liquidacion.Estado == EstadoLiquidacion.Open && la.AsesorId == asesor.Id)
                        .ToListAsync();

                    if (aprobaciones.Count > 0)
                        db.LiquidacionAprobacion.RemoveRange(aprobaciones);
                }

                db.Entry(asesor).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", asesor.Id, asesor);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> DeleteAsesor(int id)
        {
            try
            {
                var asesor = await db.Asesor.FindAsync(id);
                if (asesor == null)
                    return NotFound();

                db.Asesor.Remove(asesor);
                await db.SaveChangesAsync();
                AddLog("", asesor.Id, asesor);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var colecciones = await db.ColeccionPIV.Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.CDE
                })
                .ToListAsync();

                return Ok(colecciones);
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
