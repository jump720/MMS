using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;

namespace MMS.Controllers.WCSS
{
    public class MotivosPQRSController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: TipoDevoluciones
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {

            var motivo = await db.MotivosPQRS.FindAsync(id);

            if (motivo == null)
            {
                return HttpNotFound();
            }


            return View(GetCrudMode().ToString(), motivo);
        }

        [AuthorizeAction]
        [FillPermission("MotivosPQRS/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MotivoPQRS model, int MotivoPQRSCopy = 0)
        {

            if (ModelState.IsValid)
            {
                if (model.TipoPQRS != 0)
                {
                    model.Activo = false;
                    db.MotivosPQRS.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.Id, model);
                    await CreateFlujoPQRS(model.Id, MotivoPQRSCopy);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Please select type PQRS");
                }

            }
            return View(model);
        }

      


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MotivoPQRS model)
        {

            if (ModelState.IsValid)
            {
                if (model.TipoPQRS != 0)
                {
                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    AddLog("", model.Id, model);
                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, Please select type PQRS");
                }
            }

            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var garantia = await db.GarantiaItems.Where(di => di.MotivoPQRSId == id).ToListAsync();
            if (garantia.Count > 0)
            {
                ModelState.AddModelError("", "El motivo de PQRS no puede ser eliminado, ya se encuentra relacionada con una Garantia");
            }

            var devolucion = await db.DevolucionItems.Where(di => di.MotivoPQRSId == id).ToListAsync();
            if (devolucion.Count > 0)
            {
                ModelState.AddModelError("", "El motivo de PQRS no puede ser eliminado, ya se encuentra relacionada con una Devolución");
            }

            var novedad = await db.NovedadItem.Where(di => di.MotivoPQRSId == id).ToListAsync();
            if (novedad.Count > 0)
            {
                ModelState.AddModelError("", "El motivo de PQRS no puede ser eliminado, ya se encuentra relacionada con una Novedad");
            }

            if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
            {
                var motivo = await db.MotivosPQRS.FindAsync(id);
                if (motivo != null)
                {
                    if (await DeleteFlujoPQRS(id))
                    {
                        db.MotivosPQRS.Remove(motivo);
                        await db.SaveChangesAsync();
                        AddLog("", id, motivo);
                        return RedirectToAction("Index", GetReturnSearch());
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al eliminar Flujo PQRS");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Motivo PQRS no encontrada");
                }
            }

            return await GetView(id);

        }


        private async Task<bool> CreateFlujoPQRS(int MotivoPQRSId, int MotivoPQRSCopy = 0)
        {
            bool result = true;
            try
            {
                if (MotivoPQRSCopy == 0)
                {
                    var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId).FirstOrDefaultAsync();
                    if (flujo == null)
                    {

                        for (int i = 1; i <= 3; i++)
                        {
                            FlujoPQRS nFlujo = new FlujoPQRS();

                            nFlujo.Id = i;
                            nFlujo.MotivoPQRSId = MotivoPQRSId;
                            nFlujo.Order = i;
                            nFlujo.EnviaCorreoDestinatarios = true;
                            if (i == 1)
                            {
                                nFlujo.TipoPaso = TipoPaso.LlenarFormato;
                                nFlujo.Nombre = "Creación de formato";
                            }
                            else if (i == 2)
                            {
                                nFlujo.TipoPaso = TipoPaso.General;
                                nFlujo.Nombre = "Asignación de Analista";
                            }
                            else if (i == 3)
                            {
                                nFlujo.TipoPaso = TipoPaso.General;
                                nFlujo.Nombre = "Cierre o Solución";
                            }

                            db.FlujosPQRS.Add(nFlujo);



                        }//for (int i = 1; i <= 3; i++)
                        await db.SaveChangesAsync();
                        var config = Seguridadcll.Configuracion;
                        //Crear usuarios por paso (usuario PQRS Coordinador)                    
                        var usuPQRSCoordinador = await db.RolUsuario.Include(ru => ru.Usuario)
                                                         .Where(ru => ru.RolId == (config.ConfigTipoUsuPQRSCoordinador ?? null))
                                                         .Select(ru => ru.Usuario)
                                                         .ToListAsync();
                        foreach (var uc in usuPQRSCoordinador)
                        {
                            UsuarioFlujoPQRS uf = new UsuarioFlujoPQRS();
                            uf.FlujoPQRSId = 2;
                            uf.MotivoPQRSId = MotivoPQRSId;
                            uf.UsuarioId = uc.UsuarioId;
                            db.UsuarioFlujoPQRS.Add(uf);

                        }
                        await db.SaveChangesAsync();
                        //Crear usuarios por paso (usuario PQRS Coordinador)

                        //Crear usuarios por paso (usuario PQRS Analista)                    
                        var usuPQRSAnalista = await db.RolUsuario.Include(ru => ru.Usuario)
                                                         .Where(ru => ru.RolId == (config.ConfigTipoUsuPQRSAnalista ?? null))
                                                         .Select(ru => ru.Usuario)
                                                         .ToListAsync();
                        foreach (var ua in usuPQRSAnalista)
                        {
                            UsuarioFlujoPQRS uf = new UsuarioFlujoPQRS();
                            uf.FlujoPQRSId = 3;
                            uf.MotivoPQRSId = MotivoPQRSId;
                            uf.UsuarioId = ua.UsuarioId;
                            db.UsuarioFlujoPQRS.Add(uf);

                        }
                        await db.SaveChangesAsync();
                        //Crear usuarios por paso (usuario PQRS Analista)

                    }//if (flujo == null)
                }else
                {
                    var flujoCopy = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSCopy).ToListAsync();
                    foreach(var f in flujoCopy)
                    {
                        FlujoPQRS nFlujo = new FlujoPQRS();

                        nFlujo.Id = f.Id;
                        nFlujo.MotivoPQRSId = MotivoPQRSId;
                        nFlujo.Order = f.Order;
                        nFlujo.EnviaCorreoDestinatarios = f.EnviaCorreoDestinatarios;
                        nFlujo.TipoPaso = f.TipoPaso;
                        nFlujo.Nombre = f.Nombre;                      

                        db.FlujosPQRS.Add(nFlujo);

                        var flujoCopyUsuarios = await db.UsuarioFlujoPQRS.Where(u => u.FlujoPQRSId == f.Id && u.MotivoPQRSId == f.MotivoPQRSId).ToListAsync();

                        foreach (var ua in flujoCopyUsuarios)
                        {
                            UsuarioFlujoPQRS uf = new UsuarioFlujoPQRS();
                            uf.FlujoPQRSId = nFlujo.Id;
                            uf.MotivoPQRSId = MotivoPQRSId;
                            uf.UsuarioId = ua.UsuarioId;
                            db.UsuarioFlujoPQRS.Add(uf);

                        }
                        await db.SaveChangesAsync();
                    }

                }
            }
            catch
            {
                result = false;
            }
            return result;
        }


        private async Task<bool> DeleteFlujoPQRS(int MotivoPQRSId)
        {
            bool result = true;
            try
            {

                //Eliminar UsuariosFlujoPQRS
                var usuarios = await db.UsuarioFlujoPQRS.Where(uf => uf.MotivoPQRSId == MotivoPQRSId).ToListAsync();
                db.UsuarioFlujoPQRS.RemoveRange(usuarios);
                await db.SaveChangesAsync();

                //Eliminar Steps
                var flujo = await db.FlujosPQRS.Where(f => f.MotivoPQRSId == MotivoPQRSId).ToListAsync();
                db.FlujosPQRS.RemoveRange(flujo);
                await db.SaveChangesAsync();

            }
            catch
            {
                result = false;
            }
            return result;
        }

    }
}