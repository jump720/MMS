using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;

namespace MMS.Controllers.Seguridad
{
    public class ObjetosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Objetos
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(string id)
        {
            var objeto = await db.Objeto.FindAsync(id);

            if (objeto == null)
            {
                return HttpNotFound();
            }

            var apps = await (from a in db.Aplicaciones
                              join ao in db.AplicacionObjetos.Where(a => a.ObjetoId == id) on a equals ao.Aplicacion into oaos
                              from aoa in oaos.DefaultIfEmpty()
                              select new ObjetosViewModel.AplicacionViewModel
                              {
                                  AplicacionId = a.Id,
                                  AplicacionNombre = a.Nombre,
                                  Seleccionado = (aoa.Objeto == null) ? false : true
                              }).ToListAsync();

            ViewBag.Objeto_ObjetoIdPadre = new SelectList(await db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToListAsync(), "ObjetoId", "ObjetoDesc", objeto.ObjetoIdPadre);
            return View(GetCrudMode().ToString(), new ObjetosViewModel
            {
                Objeto = objeto,
                Apps = apps
            });
        }
        // GET: Objetos/Details/5
        [AuthorizeAction]
        [FillPermission("Objetos/Edit")]
        public async Task<ActionResult> Details(string id = "")
        {

            //Objeto objeto = db.Objeto.Find(ObjetoId);
            //if (objeto == null)
            //{
            //    ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + ObjetoId;
            //}
            //ViewBag.ObjetoIdPadre = new SelectList(db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToList(), "ObjetoId", "ObjetoDesc", objeto.ObjetoIdPadre);
            return await GetView(id);
        }

        // GET: Objetos/Create
        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            var apps = await db.Aplicaciones
                             .Select(a => new ObjetosViewModel.AplicacionViewModel
                             {
                                 AplicacionId = a.Id,
                                 AplicacionNombre = a.Nombre,
                                 Seleccionado = false
                             }).ToListAsync();

            ViewBag.Objeto_ObjetoIdPadre = new SelectList(db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToList(), "ObjetoId", "ObjetoDesc");
            return View(new ObjetosViewModel { Apps = apps });
        }

        // POST: Objetos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ObjetosViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var objetoTemp = await db.Objeto.Where(o => o.ObjetoId == model.Objeto.ObjetoId).FirstOrDefaultAsync();
                    if (objetoTemp == null)
                    {
                        if (!model.Objeto.ObjetoMenu)
                        {
                            model.Objeto.ObjetoIcono = null;
                            model.Objeto.ObjetoIdPadre = null;
                            model.Objeto.ObjetoOrden = null;
                        }

                        db.Objeto.Add(model.Objeto);
                        await db.SaveChangesAsync();
                        // AddLog("", model.Objeto.ObjetoId, model);
                        if (model.Objeto.ObjetoMenu)
                        {
                            foreach (var app in model.Apps)
                            {
                                if (app.Seleccionado)
                                {
                                    AplicacionObjeto ao = new AplicacionObjeto();
                                    ao.AplicacionId = app.AplicacionId;
                                    ao.ObjetoId = model.Objeto.ObjetoId;
                                    db.AplicacionObjetos.Add(ao);
                                }
                            }
                        }
                        if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                        {
                            await db.SaveChangesAsync();
                            AddLog("", model.Objeto.ObjetoId, model);
                            return RedirectToAction("Index", GetReturnSearch());
                        }


                    }
                    else
                    {
                        ModelState.AddModelError("", "Warning, this object " + model.Objeto.ObjetoId + " already exists");

                    }
                }
                catch (Exception e)
                {
                    ViewBag.error = e.ToString();
                }
            }

            //ViewBag.ObjetoIdPadre = new SelectList(db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToList(), "ObjetoId", "ObjetoDesc", objeto.ObjetoIdPadre);
            ViewBag.Objeto_ObjetoIdPadre = new SelectList(await db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToListAsync(), "ObjetoId", "ObjetoDesc", model.Objeto.ObjetoIdPadre);
            return View(new ObjetosViewModel { Objeto = model.Objeto, Apps = model.Apps });
        }

        // GET: Objetos/Edit/5
        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id = "")
        {


            return await GetView(id);
        }

        // POST: Objetos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ObjetosViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (!model.Objeto.ObjetoMenu)
                    {
                        model.Objeto.ObjetoIcono = null;
                        model.Objeto.ObjetoIdPadre = null;
                        model.Objeto.ObjetoOrden = null;
                    }

                    db.Entry(model.Objeto).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    if (await DeleteAplicacionObjetos(model.Objeto.ObjetoId))
                    {
                        if (model.Objeto.ObjetoMenu)
                        {
                            foreach (var app in model.Apps)
                            {
                                if (app.Seleccionado)
                                {
                                    AplicacionObjeto ao = new AplicacionObjeto();
                                    ao.AplicacionId = app.AplicacionId;
                                    ao.ObjetoId = model.Objeto.ObjetoId;
                                    db.AplicacionObjetos.Add(ao);
                                }
                            }
                            await db.SaveChangesAsync();
                        }//if (model.Objeto.ObjetoMenu)
                        AddLog("", model.Objeto.ObjetoId, model);
                        return RedirectToAction("Index", GetReturnSearch());
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error Deleting Detail (AplicacionObjetos)");
                    }


                }
                catch (Exception e)
                {
                    ViewBag.error = e.ToString();
                }
            }
            ViewBag.Objeto_ObjetoIdPadre = new SelectList(await db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToListAsync(), "ObjetoId", "ObjetoDesc", model.Objeto.ObjetoIdPadre);
            return await GetView(model.Objeto.ObjetoId);
        }

        // Para enviar la lista resultante al AJAX
        public JsonResult obtenerObjetosConMenu()
        {
            var result = new[] { new { objeto = "" } }.ToList();
            try
            {
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                if (seguridadcll != null)
                {
                    result = null;
                    var resultTemp = (from r in seguridadcll.ObjetosMenuList
                                      select new
                                      {
                                          objeto = r.ObjetoId,
                                      }).ToList();
                    result = resultTemp;
                }
                else
                {
                    result = new[] { new { objeto = "Sin_Objetos" } }.ToList(); ;
                }
            }
            catch (Exception e)
            {
                ViewBag.error = e.ToString();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: Objetos/Delete/5
        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {

            var data = await db.Objeto
                .Include(o => o.RolObjetoList)
                .Include(o => o.ObjetoList)
                .Where(o => o.ObjetoId == id)
                .Select(o => new
                {
                    Objeto = o,
                    RolObjetoCount = o.RolObjetoList.Count,
                    ObjetoCount = o.ObjetoList.Count
                })
                .FirstOrDefaultAsync();

            if (data == null)
                ViewBag.Error = "Warning, Record not found or Invalid " + id;
            else
            {
                if (data.RolObjetoCount > 0)
                    ViewBag.Error = "Warning, This object " + id + " is related with differents roles.";
                else if (data.ObjetoCount > 0)
                    ViewBag.Error = "Warning, This object " + id + " is related with differents objects.";

                //ViewBag.Objeto.ObjetoIdPadre = new SelectList(await db.Objeto.Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToListAsync(), "ObjetoId", "ObjetoDesc", data.Objeto.ObjetoIdPadre);
                return await GetView(data.Objeto.ObjetoId);
            }

            return await GetView(null);

        }

        // POST: Objetos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> DeleteConfirmed(string id = null)
        {
            try
            {
                /*Primero Eliminar registros de RolObjetos*/
                db.RolObjeto
                    .Where(o => o.ObjetoId == id)
                    .ToList()
                    .ForEach(o => db.RolObjeto.Remove(o));
                await db.SaveChangesAsync();

                if (await DeleteAplicacionObjetos(id))
                {

                    //Segundo se elimina registro de objeto
                    Objeto objeto = await db.Objeto.FindAsync(id);
                    db.Objeto.Remove(objeto);
                    await db.SaveChangesAsync();
                    AddLog("", objeto.ObjetoId, objeto);

                }
                ////Tercero de inserta Auditoria
                //Seguridad seguridad = new Seguridad();
                //Auditoria auditoria = new Auditoria();
                //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                //auditoria.AuditoriaFecha = System.DateTime.Now;
                //auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                //auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                //auditoria.AuditoriaEvento = "Eliminar";
                //auditoria.AuditoriaDesc = "Eliminar objeto: " + ObjetoId;
                //auditoria.ObjetoId = "objetos/Delete";

                //seguridad.insertAuditoria(auditoria);
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
            }

            return RedirectToAction("Index", GetReturnSearch());
        }


        private async Task<bool> DeleteAplicacionObjetos(string id)
        {
            bool result = true;
            try
            {
                var ao = await db.AplicacionObjetos.Where(a => a.ObjetoId == id).ToListAsync();
                if (ao.Count > 0)
                {
                    db.AplicacionObjetos.RemoveRange(ao);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
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
