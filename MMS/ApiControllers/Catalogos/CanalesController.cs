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
    public class CanalesController : ApiBaseController
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

                int count = await db.Canales
                    .Where(c => c.CanalDesc.Contains(search))
                    .CountAsync();

                var data = await db.Canales
                    .Where(c => c.CanalDesc.Contains(search))
                    .Select(c => new { c.CanalID, c.CanalDesc })
                    .OrderBy(c => c.CanalID)
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
        public async Task<IHttpActionResult> Create(Canal canal)
        {
            try
            {
                db.Canales.Add(canal);
                await db.SaveChangesAsync();
                AddLog("", canal.CanalID, canal);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Canal canal)
        {
            try
            {
                db.Entry(canal).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", canal.CanalID, canal);

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
                var canal = await db.Canales.FindAsync(id);
                if (canal == null)
                    return NotFound();

                db.Canales.Remove(canal);
                await db.SaveChangesAsync();
                AddLog("", canal.CanalID, canal);

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
