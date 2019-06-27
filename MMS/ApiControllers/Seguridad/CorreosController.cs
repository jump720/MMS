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

namespace MMS.ApiControllers.Seguridad
{
    public class CorreosController : ApiBaseController
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



                int count = await db.Correos
                    .Where(c => c.Nombre.Contains(search) && (c.Mail.ToString().Contains(search) || c.Grupo.ToString().Contains(search)))
                    .Select(p => new
                    {
                        p.Nombre,
                        p.Mail,
                        p.Grupo,
                        p.Active,
                        p.Id
                    })
                    .CountAsync();

                var data = await db.Correos
                  .Where(c => c.Nombre.Contains(search) && (c.Mail.ToString().Contains(search) || c.Grupo.ToString().Contains(search)))
                  .Select(p => new
                    {
                      p.Nombre,
                      p.Mail,
                      p.Grupo,
                      p.Active,
                      p.Id
                  })
                    .OrderBy(a => a.Nombre)
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
        public async Task<IHttpActionResult> Create(Correos model)
        {
            try
            {
                var result = new AjaxResult();

                if (ModelState.IsValid)
                {
                    var correo = await db.Correos.Where(c => c.Mail.Contains(model.Mail))
                                            .FirstOrDefaultAsync();
                    if (correo == null)
                    {

                        db.Correos.Add(model);
                        await db.SaveChangesAsync();
                        AddLog("", model.Id, model);
                    }
                    else
                    {

                        return Ok(result.False("Warning, This e-Mail already exists"));
                        //return InternalServerError(new Exception("Warning, This Budget  already exists"));
                    }

                    return Ok(result.True("Record Saved"));
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
        public async Task<IHttpActionResult> Edit(Correos model)
        {
            try
            {
                var result = new AjaxResult();

                if (ModelState.IsValid)
                {
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);

                    return Ok(result.True("Record Updated"));
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
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                var result = new AjaxResult();

                var correo = await db.Correos.FindAsync(id);
                if (correo == null)
                    return NotFound();

                db.Correos.Remove(correo);
                await db.SaveChangesAsync();
                AddLog("", correo.Id, correo);

                return Ok(result.True("Record Deleted"));

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


    }
}
