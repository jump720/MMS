using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MMS.Models;
using MMS.Filters;
using System.Net.Http.Formatting;
using MMS.Classes;
using System.Threading.Tasks;

namespace MMS.ApiControllers.WCSS
{
    public class FlujosPQRSController : ApiBaseController
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

                string tipopqrs = form["_tipopqrs"];


                var countQuery = db.FlujosPQRS
                    .Include(f => f.MotivoPQRS)
                    .Where(f => f.Nombre.Contains(search) || f.MotivoPQRS.Nombre.ToString().Contains(search));
                // .GroupBy(f => new { f.MotivoPQRS.Nombre });
                // .Select(f => new { Motivo = f.Key });


                var dataQuery = db.FlujosPQRS
                    .Include(f => f.MotivoPQRS)
                    .Where(f => f.Nombre.Contains(search) || f.MotivoPQRS.Nombre.ToString().Contains(search));
                //.GroupBy(f => new { f.MotivoPQRS.Nombre, f.MotivoPQRS.TipoPQRS });
                //.Select(f => new { Id = f.FirstOrDefault().MotivoPQRSId, Motivo = f.FirstOrDefault().MotivoPQRS.Nombre, TipoPQRS = f.FirstOrDefault().MotivoPQRS.TipoPQRS.ToString() });



                if (!string.IsNullOrWhiteSpace(tipopqrs))
                {
                    int value = int.Parse(tipopqrs);
                    countQuery = countQuery.Where(q => q.MotivoPQRS.TipoPQRS == (TipoPQRS)value);
                    dataQuery = dataQuery.Where(q => q.MotivoPQRS.TipoPQRS == (TipoPQRS)value);
                }

                int count = await countQuery.GroupBy(f => new { f.MotivoPQRS.Nombre }).Select(f => new { Motivo = f.Key }).CountAsync();

                var data = await dataQuery.GroupBy(f => new { f.MotivoPQRS.Nombre, f.MotivoPQRS.TipoPQRS })
                    .Select(f => new
                    {
                        Id = f.FirstOrDefault().MotivoPQRSId,
                        Motivo = f.FirstOrDefault().MotivoPQRS.Nombre,
                        TipoPQRS = (f.FirstOrDefault().MotivoPQRS.TipoPQRS == TipoPQRS.Devolucion) ? "Return" : (f.FirstOrDefault().MotivoPQRS.TipoPQRS == TipoPQRS.Garantia) ? "Guarantee" : (f.FirstOrDefault().MotivoPQRS.TipoPQRS == TipoPQRS.Novedad) ? "New" : "Recruitment"
                    })
                    .OrderBy(f => f.Id)
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
        public async Task<IHttpActionResult> CreateStep(FlujoPQRS model)
        {
            try
            {
                //Obtiene ultima linea
                int lastid = await db.FlujosPQRS
                                    .Where(f => f.MotivoPQRSId == model.MotivoPQRSId)
                                    .Select(f => f.Id).MaxAsync() + 1;

                if (ModelState.IsValid)
                {
                    model.Id = lastid;
                    model.EnviaCorreoDestinatarios = model.EnviaCorreoDestinatarios ?? false;
                    db.FlujosPQRS.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);

                    //await ActivaMotivoPQRS(model.UsuariosStepList.FirstOrDefault().MotivoPQRSId);
                    await ActivaMotivoPQRS(model.MotivoPQRSId);
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> EditStep(FlujoPQRS model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.EnviaCorreoDestinatarios = model.EnviaCorreoDestinatarios ?? false;
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> DeleteStep(int MotivoPQRSId, int Id)
        {
            var flujopqrs = await db.FlujosPQRS.Where(f => f.Id == Id && f.MotivoPQRSId == MotivoPQRSId).FirstOrDefaultAsync();
            try
            {
                if (await DeleteUserStep(MotivoPQRSId, Id))
                {
                    db.FlujosPQRS.Remove(flujopqrs);
                    await db.SaveChangesAsync();
                    AddLog("", flujopqrs.Id, flujopqrs);

                    await ActivaMotivoPQRS(flujopqrs.MotivoPQRSId);
                    return Ok(flujopqrs);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> SaveSteps(string mod, int motivo, int OrderOld = 0, int OrderNew = 0)
        {
            try
            {
                int order = 3;


                if (mod == "create")
                {
                    var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == motivo)
                                    .OrderBy(f => f.Order)
                                    .ToListAsync();
                    foreach (var step in flujo.Where(f => f.Id > 3))
                    {
                        step.Order = order++;
                        db.Entry(step).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }

                    //actualiza orden etapa de cierre
                    var cierre = flujo.Where(f => f.Id == 3).FirstOrDefault();
                    if (cierre != null)
                    {
                        cierre.Order = order++;
                        db.Entry(cierre).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }

                    return Ok(flujo.Select(f => new { f.MotivoPQRSId, f.Id, f.Nombre, f.Order, TipoPaso = f.TipoPaso.ToString(), f.EnviaCorreoDestinatarios }));
                }
                else if (mod == "order")
                {
                    var flujo = new List<FlujoPQRS>();
                    //if old es mayor que new(viene de abajo el movimiento)
                    if (OrderOld > OrderNew)
                    {
                        flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == motivo && (f.Order >= OrderNew && f.Order <= OrderOld))
                                 .OrderBy(f => f.Order)
                                 .ToListAsync();

                        foreach (var step in flujo)
                        {
                            if (step.Order == OrderOld)
                            {
                                step.Order = OrderNew;
                            }
                            else
                            {
                                step.Order = step.Order + 1;
                            }
                            db.Entry(step).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            if (step.Order == 3)
                                await DeleteCondiciones(step.MotivoPQRSId, step.Id);
                        }
                    }
                    else//f old es mayor que new(viene de arriba el movimiento)
                    {

                        flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == motivo && (f.Order >= OrderOld && f.Order <= OrderNew))
                                     .OrderBy(f => f.Order)
                                     .ToListAsync();

                        foreach (var step in flujo)
                        {
                            if (step.Order == OrderOld)
                            {
                                step.Order = OrderNew;
                            }
                            else
                            {
                                step.Order = step.Order - 1;
                            }
                            db.Entry(step).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            if (step.Order == 3)
                                await DeleteCondiciones(step.MotivoPQRSId, step.Id);
                        }
                    }


                    return Ok(true);
                }
                return Ok(false);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }


        }


        private async Task<bool> DeleteCondiciones(int MotivoId, int FlujoId)
        {
            var result = true;
            try
            {
                var Condiciones = await db.FlujoPQRSCondiciones.Where(fc => fc.MotivoPQRSId == MotivoId && fc.FlujoPQRSId == FlujoId).ToListAsync();

                db.FlujoPQRSCondiciones.RemoveRange(Condiciones);

                await db.SaveChangesAsync();
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> UsuariosStep(UsuarioFlujoPQRSViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (await DeleteUserStep(model.UsuariosStepList.FirstOrDefault().MotivoPQRSId, model.UsuariosStepList.FirstOrDefault().FlujoPQRSId))
                    {
                        foreach (var usuario in model.UsuariosStepList)
                        {
                            if (usuario.check)
                            {
                                UsuarioFlujoPQRS usuarioFlujoPQRS = new UsuarioFlujoPQRS();
                                usuarioFlujoPQRS.MotivoPQRSId = usuario.MotivoPQRSId;
                                usuarioFlujoPQRS.FlujoPQRSId = usuario.FlujoPQRSId;
                                usuarioFlujoPQRS.UsuarioId = usuario.UsuarioId;
                                db.UsuarioFlujoPQRS.Add(usuarioFlujoPQRS);

                            }
                        }
                        await db.SaveChangesAsync();

                        await ActivaMotivoPQRS(model.UsuariosStepList.FirstOrDefault().MotivoPQRSId);
                    }

                    //db.Entry(model).State = EntityState.Modified;
                    //await db.SaveChangesAsync();
                    AddLog("", model.UsuariosStepList.FirstOrDefault().MotivoPQRSId, model);
                }
                return Ok(model);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }



        [HttpPost]
        [ApiAuthorizeAction]
        public async Task<IHttpActionResult> ConfigStep(FlujoPQRSConfigViewModel model)
        {
            var result = new AjaxResult();
            try
            {
                if (ModelState.IsValid)
                {

                    #region Tareas

                    if (model.Tareas == null)
                        model.Tareas = new List<FlujoPQRSTareas>();


                    //Edit
                    foreach (var t in model.Tareas.Where(t => t.Id != 0))
                    {
                        db.Entry(t).State = EntityState.Modified;
                        await db.SaveChangesAsync();
                    }

                    var currentTareas = await db.FlujoPQRSTareas
                        .Where(ft => ft.MotivoPQRSId == model.Flujo.MotivoPQRSId && ft.FlujoPQRSId == model.Flujo.Id)
                        .ToListAsync();

                    var currentTareasId = currentTareas.Select(ct => ct.Id).ToArray();
                    var TareasId = model.Tareas.Where(t => t.Id != 0).Select(t => t.Id).ToArray();


                    var tareasToDelete = currentTareas.Where(ct => !TareasId.Contains(ct.Id)).ToList();
                    var tareasToAdd = model.Tareas.Where(t => t.Id == 0).ToList();



                    //Delete
                    if (tareasToDelete.Count > 0)
                        db.FlujoPQRSTareas.RemoveRange(tareasToDelete);

                    //Add
                    if (tareasToAdd.Count > 0)
                        db.FlujoPQRSTareas.AddRange(tareasToAdd);



                    await db.SaveChangesAsync();
                    //AddLog("", model.Flujo.Id, model);

                    #endregion Tareas

                    #region Condiciones
                    if (model.Flujo.Order > 3)
                    {
                        if (model.Condiciones == null)
                            model.Condiciones = new List<FlujoPQRSCondiciones>();


                        //Edit
                        foreach (var c in model.Condiciones.Where(c => c.Id != 0))
                        {
                            if (c.TipoCondicion == TipoCondicion.Value)
                            {
                                c.SiNo = false;
                            }
                            else
                            {
                                c.Valor = 0;
                                c.CondicionesValor = CondicionesValor.Equal;
                            }
                            db.Entry(c).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        var currentCondiciones = await db.FlujoPQRSCondiciones
                            .Where(ft => ft.MotivoPQRSId == model.Flujo.MotivoPQRSId && ft.FlujoPQRSId == model.Flujo.Id)
                            .ToListAsync();

                        var currentCondicionesId = currentCondiciones.Select(ct => ct.Id).ToArray();
                        var CondicionesId = model.Condiciones.Where(t => t.Id != 0).Select(t => t.Id).ToArray();


                        var CondicionesToDelete = currentCondiciones.Where(ct => !CondicionesId.Contains(ct.Id)).ToList();
                        var CondicionesToAdd = model.Condiciones.Where(t => t.Id == 0).ToList();



                        //Delete
                        if (CondicionesToDelete.Count > 0)
                            db.FlujoPQRSCondiciones.RemoveRange(CondicionesToDelete);

                        //Add
                        if (CondicionesToAdd.Count > 0)
                            db.FlujoPQRSCondiciones.AddRange(CondicionesToAdd);



                        await db.SaveChangesAsync();
                        AddLog("", model.Flujo.Id, model);

                    }
                    #endregion Condiciones


                }
                else
                {
                    return Ok(result.False("All fields are required."));
                }
                return Ok(result.True());
                // return Ok(model);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private async Task<bool> DeleteUserStep(int MotivoPQRSId, int Id)
        {
            bool result = true;
            try
            {
                var usuariosStep = await db.UsuarioFlujoPQRS.Where(uf => uf.MotivoPQRSId == MotivoPQRSId && uf.FlujoPQRSId == Id).ToListAsync();
                db.UsuarioFlujoPQRS.RemoveRange(usuariosStep);
                await db.SaveChangesAsync();
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private async Task ActivaMotivoPQRS(int MotivoPQRSId)
        {
            bool activo = true;

            var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId && f.Id != 1).ToListAsync();

            foreach (var step in flujo)
            {

                var usuarios = await db.UsuarioFlujoPQRS.Where(uf => uf.MotivoPQRSId == MotivoPQRSId && uf.FlujoPQRSId == step.Id).ToListAsync();

                if (usuarios.Count <= 0)
                {
                    activo = false;
                }
            }


            var motivo = await db.MotivosPQRS.Where(m => m.Id == MotivoPQRSId).FirstOrDefaultAsync();
            if (motivo != null)
            {
                motivo.Activo = activo;
                db.Entry(motivo).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }



        }

    }
}
