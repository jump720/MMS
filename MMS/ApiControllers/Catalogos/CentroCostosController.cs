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
    public class CentroCostosController : ApiBaseController
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

                int count = await db.CentroCostos
                    .Where(cc => cc.CentroCostoID.Contains(search) || cc.CentroCostoDesc.Contains(search))
                    .CountAsync();

                var data = await db.CentroCostos
                    .Where(cc => cc.CentroCostoID.Contains(search) || cc.CentroCostoDesc.Contains(search))
                    .Select(cc => new { cc.CentroCostoID, cc.CentroCostoDesc })
                    .OrderBy(cc => cc.CentroCostoID)
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
        public async Task<IHttpActionResult> Create(CentroCosto centroCosto)
        {
            try
            {
                db.CentroCostos.Add(centroCosto);
                await db.SaveChangesAsync();
                AddLog("", centroCosto.CentroCostoID, centroCosto);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(CentroCosto centroCosto)
        {
            try
            {
                db.Entry(centroCosto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", centroCosto.CentroCostoID, centroCosto);

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
                var centroCosto = await db.CentroCostos.FindAsync(id);
                if (centroCosto == null)
                    return NotFound();

                db.CentroCostos.Remove(centroCosto);
                await db.SaveChangesAsync();
                AddLog("", centroCosto.CentroCostoID, centroCosto);

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