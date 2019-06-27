using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using MMS.Filters;
using MMS.Models;
using MMS.Classes;
namespace MMS.ApiControllers.Catalogos
{
    public class MarcasController : ApiBaseController
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



                //string search = form["search[value]"];

                int count = await db.Marca
                                  .Where(a => a.Nombre.Contains(search))
                                  .CountAsync();
                var data = await db.Marca
                                   .Where(a => a.Nombre.Contains(search))
                                   .Select(a => new { a.Id, a.Nombre })
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



        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Create(Marca marca)
        {
            try
            {
                db.Marca.Add(marca);
                await db.SaveChangesAsync();
                AddLog("", marca.Id, marca);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(Marca marca)
        {
            try
            {
                db.Entry(marca).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", marca.Id, marca);

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
                var marca = await db.Marca.FindAsync(id);
                if (marca == null)
                    return NotFound();

                db.Marca.Remove(marca);
                await db.SaveChangesAsync();
                AddLog("", marca.Id, marca);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
