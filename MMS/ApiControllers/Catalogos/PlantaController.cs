using MMS.Filters;
using MMS.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;

namespace MMS.ApiControllers.PIV
{
    public class PlantaController : ApiBaseController
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

                int count = await db.Plantas
                    .Where(cc => cc.PlantaID.Contains(search) || cc.PlantaDesc.Contains(search))
                    .CountAsync();

                var data = await db.Plantas
                    .Where(cc => cc.PlantaID.Contains(search) || cc.PlantaDesc.Contains(search))
                    .Select(cc => new { cc.PlantaID, cc.PlantaDesc })
                    .OrderBy(cc => cc.PlantaID)
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
        public async Task<IHttpActionResult> Create(Plantas planta)
        {
            try
            {
                db.Plantas.Add(planta);
                await db.SaveChangesAsync();
                AddLog("", planta.PlantaID, planta);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Plantas planta)
        {
            try
            {
                db.Entry(planta).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", planta.PlantaID, planta);

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
                var planta = await db.Plantas.FindAsync(id);
                if (planta == null)
                    return NotFound();

                db.Plantas.Remove(planta);
                await db.SaveChangesAsync();
                AddLog("", planta.PlantaID, planta);

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