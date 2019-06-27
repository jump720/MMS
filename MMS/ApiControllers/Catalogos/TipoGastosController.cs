using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
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
    public class TipoGastosController : ApiBaseController
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

                int count = await db.TipoGastos
                    .Where(tg => tg.TipoGastoID.Contains(search) || tg.TipoGastoDesc.Contains(search))
                    .CountAsync();

                var data = await db.TipoGastos
                    .Where(tg => tg.TipoGastoID.Contains(search) || tg.TipoGastoDesc.Contains(search))
                    .Select(cc => new { cc.TipoGastoID, cc.TipoGastoDesc })
                    .OrderBy(cc => cc.TipoGastoID)
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
        public async Task<IHttpActionResult> Create(TipoGasto tipoGasto)
        {
            try
            {
                var result = new AjaxResult();

                var tipoGastoTemp = await db.TipoGastos.Where(u => u.TipoGastoID == tipoGasto.TipoGastoID).FirstOrDefaultAsync();
                if(tipoGastoTemp == null)
                {
                    db.TipoGastos.Add(tipoGasto);
                    await db.SaveChangesAsync();
                    AddLog("", tipoGasto.TipoGastoID, tipoGasto);
                }else
                {
                    return Ok(result.False("Spent Type already exists"));
                }
               

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(TipoGasto tipoGasto)
        {
            try
            {
                var result = new AjaxResult();

                db.Entry(tipoGasto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", tipoGasto.TipoGastoID, tipoGasto);

                return Ok(result.True());
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
                var result = new AjaxResult();
                var tipogasto = await db.TipoGastos.FindAsync(id);
                if (tipogasto == null)
                    return NotFound();

                var gasto = await db.Gasto.Where(g => g.TipoGastoID == id).ToListAsync();

                if (gasto.Count == 0)
                {
                    db.TipoGastos.Remove(tipogasto);
                    await db.SaveChangesAsync();
                    AddLog("", tipogasto.TipoGastoID, tipogasto);
                }else
                {
                    return Ok(result.False("Spent Type is related with some records"));
                }
                

                return Ok(result.True());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
