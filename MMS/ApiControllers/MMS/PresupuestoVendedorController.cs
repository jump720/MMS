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
namespace MMS.ApiControllers.MMS
{
    public class PresupuestoVendedorController : ApiBaseController
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

                var canal = Seguridadcll.PlantaList.Select(c => c.PlantaID).ToArray();

                int count = await db.PresupuestoVendedor
                    .Include(p => p.centroCosto)
                    .Include(p => p.canal)
                    .Include(p => p.planta)
                    .Where(a => 
                    //canal.Contains(a.CanalID) && 
                    (a.PresupuestoVendedorAno.ToString().Contains(search) ||
                    a.centroCosto.CentroCostoDesc.Contains(search) || 
                    a.planta.PlantaDesc.Contains(search) || 
                    a.canal.CanalDesc.Contains(search)))
                    .Select(p => new
                    {
                        p.canal.CanalDesc,
                        p.PresupuestoVendedorAno,
                        p.planta.PlantaID,
                        p.canal.CanalID,
                        p.planta.PlantaDesc,
                        p.CentroCostoID,
                        p.centroCosto.CentroCostoDesc,
                        p.PresupuestoValor,
                        p.PresupuestoGasto,
                    }).Distinct().CountAsync();
                // stop here
                var data = await db.PresupuestoVendedor
                    .Include(p => p.centroCosto)
                    .Include(p => p.canal)
                    .Include(p => p.planta)
                    .Where(a => 
                    //canal.Contains(a.CanalID) && 
                    (a.PresupuestoVendedorAno.ToString().Contains(search) ||
                    a.centroCosto.CentroCostoDesc.Contains(search) || 
                    a.planta.PlantaDesc.Contains(search)))
                    .Select(p => new
                    {
                        p.planta.PlantaDesc,
                        p.canal.CanalDesc,
                        p.PresupuestoVendedorAno,
                        p.PlantaID,
                        p.CanalID,
                        p.CentroCostoID,
                        p.centroCosto.CentroCostoDesc,
                        p.PresupuestoValor,
                        p.PresupuestoGasto,
                    })
                    .Distinct()
                    .OrderBy(a => a.PlantaID)
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
        public async Task<IHttpActionResult>  Create(PresupuestoVendedor model)
        {
            try
            {
                var result = new AjaxResult();

                if (ModelState.IsValid)
                {
                    var presupuesto = await db.PresupuestoVendedor.Where(
                        p => 
                        p.PresupuestoVendedorAno == model.PresupuestoVendedorAno &&
                        p.PlantaID == model.PlantaID &&
                        p.CanalID == model.CanalID &&
                        p.CentroCostoID == model.CentroCostoID).FirstOrDefaultAsync();
                    if (presupuesto == null)
                    {
                        db.PresupuestoVendedor.Add(model);
                        await db.SaveChangesAsync();
                        AddLog("", model.PlantaID, model);
                    }
                    else
                    {
                        
                        return Ok(result.False("Warning, This Budget  already exists"));
                        //return InternalServerError(new Exception("Warning, This Budget  already exists"));
                    }

                    return Ok(result.True("Record saved"));
                }
                else
                {
                    //return InternalServerError(new Exception("Error, All field are required"));
                    string s = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Ok(result.False(s));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> Edit(PresupuestoVendedor model)
        {
            try
            {
                var result = new AjaxResult();

                if (ModelState.IsValid)
                {

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", model.PlantaID, model);
                 
                    return Ok(result.True("Record saved"));
                }
                else
                {
                    //return InternalServerError(new Exception("Error, All field are required"));
                    string s = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return Ok(result.False(s));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
