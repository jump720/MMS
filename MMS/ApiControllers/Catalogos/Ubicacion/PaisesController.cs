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
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers.PIV
{
    public class PaisController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpGet]
        public async Task<IHttpActionResult> Departamentos(string id)
        {
            try
            {
                var departamentos = await db.Departamento.OrderBy(d => d.DepartamentoDesc)
                    .Where(d => d.PaisID == id)
                    .Select(d => new
                    {
                        Id = d.DepartamentoID,
                        Nombre = d.DepartamentoDesc
                    }).ToListAsync();

                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Index(FormDataCollection form)
        {
            try
            {
                int displayStart = int.Parse(form["start"]);
                int displayLength = int.Parse(form["length"]);
                string search = form["search[value]"];

                int count = await db.Pais
                    .Where(p => p.PaisID.Contains(search) || p.PaisDesc.Contains(search))
                    .CountAsync();

                var data = await db.Pais
                    .Where(p => p.PaisID.Contains(search) || p.PaisDesc.Contains(search))
                    .Select(p => new { p.PaisID, p.PaisDesc })
                    .OrderBy(p => p.PaisID)
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
        public async Task<IHttpActionResult> Create(Pais pais)
        {
            try
            {
                    db.Pais.Add(pais);
                    await db.SaveChangesAsync();
                    AddLog("", pais.PaisID, pais);
                    return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Pais pais)
        {
            try
            {
                    db.Entry(pais).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", pais.PaisID, pais);

                    return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Delete(string id)
        {
            try
            {
                var pais = await db.Pais.FindAsync(id);
                if (pais == null)
                    return NotFound();

                db.Pais.Remove(pais);
                await db.SaveChangesAsync();
                AddLog("", pais.PaisID, pais);

                return Ok(true);
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
