using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using System.Drawing;
using System.IO;
using MMS.Classes;

namespace MMS.Controllers.PerfectStore
{
    public class VisitasController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: TipoIndustria
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var visita = await db.Visita.Include(v => v.Usuario).Where(v => v.Id == id).FirstOrDefaultAsync();

            if (visita == null)
                return HttpNotFound();

            ViewBag.UsuarioId = visita.UsuarioId;
            ViewBag.UsuarioNombre = visita.Usuario.UsuarioNombre;

            ViewData["Visita.TipoVisitaId"] = new SelectList(db.TipoVisitas.ToList(), "Id", "Nombre", visita.TipoVisitaId);
            ViewData["Visita.TipoIndustriaId"] = new SelectList(db.TipoIndustrias.ToList(), "Id", "Nombre", visita.TipoIndustriaId);
            ViewData["Visita.PaisId"] = new SelectList(db.Pais.ToList(), "PaisId", "PaisDesc", visita.PaisId);
            ViewData["Visita.DepartamentoId"] = new SelectList(db.Departamento.Where(d => d.PaisID == visita.PaisId).ToList(), "DepartamentoId", "DepartamentoDesc");
            ViewData["Visita.CiudadId"] = new SelectList(db.Ciudad.Where(c => c.DepartamentoID == visita.DepartamentoId && c.PaisID == visita.PaisId).ToList(), "CiudadId", "CiudadDesc");
            ViewBag.Marcas = new SelectList(db.Marca.ToList(), "Id", "Nombre");
            ViewBag.TipoIndustriaAutomotriz = Seguridadcll.Configuracion.TipoIndustriaAutomotriz;

            var visitafotos = await db.VisitaFoto
                                   .Where(vf => vf.VisitaId == id)
                                   .ToListAsync();

            var photos = visitafotos.Select(vf => new VisitasViewModel.Fotos
            {
                Order = vf.Order,
                FileName = string.Format("data:{0};base64,{1}", vf.MediaType, Convert.ToBase64String(vf.Foto))
            }).ToList();

            if (GetCrudMode() == Fn.CrudMode.Edit || GetCrudMode() == Fn.CrudMode.Delete)
                if (visita.Completada == true)
                {
                    return RedirectToAction("Index", GetReturnSearch());
                }


            return View(GetCrudMode().ToString(), new VisitasViewModel
            {
                Visita = visita,
                VisitaFotos = photos
            });
        }

        [AuthorizeAction]
        [FillPermission("Visitas/Edit")]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult BuscarEstablecimiento()
        {
            return PartialView("_BuscarEstablecimiento");

        }

        [AuthorizeAction]
        public async Task<ActionResult> Maps()
        {
            var visitas = await db.Visita
                                  .Include(v => v.Usuario)
                                  .Include(v => v.Ciudad.departamentos.paises)
                                  .Where(v => v.Completada && v.Latitud != null && v.Longitud != null).ToListAsync();
            return View(visitas);
        }


        [AuthorizeAction]
        public ActionResult Informe()
        {
            
            return View();
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create(int id = 0)
        {
            var visita = await db.Visita.Where(v => v.Id == id).FirstOrDefaultAsync();

            
            ViewData["Visita.TipoVisitaId"] = new SelectList(db.TipoVisitas.ToList(), "Id", "Nombre");
            ViewData["Visita.TipoIndustriaId"] = new SelectList(db.TipoIndustrias.ToList(), "Id", "Nombre");
            ViewData["Visita.PaisId"] = new SelectList(db.Pais.ToList(), "PaisId", "PaisDesc", (visita != null) ? visita.PaisId : null);

            if (visita != null)
            {
                visita.TipoVisitaId = null;
                visita.TipoIndustriaId = null;
                visita.Marcas = null;
                visita.DisponibilidadProducto = false;
                visita.NumeroMecanicos = null;
                visita.Latitud = null;
                visita.Longitud = null;
                ViewData["Visita.DepartamentoId"] = new SelectList(db.Departamento.Where(d => d.PaisID == visita.PaisId).ToList(), "DepartamentoId", "DepartamentoDesc");
                ViewData["Visita.CiudadId"] = new SelectList(db.Ciudad.Where(c => c.DepartamentoID == visita.DepartamentoId && c.PaisID == visita.PaisId).ToList(), "CiudadId", "CiudadDesc");
            }
            else
            {
                ViewData["Visita.DepartamentoId"] = new SelectList(db.Departamento.ToList(), "DepartamentoId", "DepartamentoDesc");
                ViewData["Visita.CiudadId"] = new SelectList(db.Ciudad.ToList(), "CiudadId", "CiudadDesc");
            }
            ViewBag.UsuarioId = Seguridadcll.Usuario.UsuarioId;
            ViewBag.UsuarioNombre = Seguridadcll.Usuario.UsuarioNombre;
            ViewBag.Marcas = new SelectList(db.Marca.ToList(), "Id", "Nombre");
            ViewBag.TipoIndustriaAutomotriz = Seguridadcll.Configuracion.TipoIndustriaAutomotriz;

            return View(new VisitasViewModel { Visita = visita });
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(VisitasViewModel model, IEnumerable<HttpPostedFileBase> Photos)
        {

            if (ModelState.IsValid)
            {
                db.Visita.Add(model.Visita);
                await db.SaveChangesAsync();

                int order = 0;
                if (Photos != null)
                    await UploadPhotos(model.Visita.Id, Photos, order);

                AddLog("", model.Visita.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            ViewData["Visita.TipoVisitaId"] = new SelectList(db.TipoVisitas.ToList(), "Id", "Nombre", model.Visita.TipoVisitaId);
            ViewData["Visita.TipoIndustriaId"] = new SelectList(db.TipoIndustrias.ToList(), "Id", "Nombre", model.Visita.TipoIndustriaId);
            ViewData["Visita.PaisId"] = new SelectList(db.Pais.ToList(), "PaisId", "PaisDesc", model.Visita.PaisId);

            if (model.Visita != null)
            {
                ViewData["Visita.DepartamentoId"] = new SelectList(db.Departamento.Where(d => d.PaisID == model.Visita.PaisId).ToList(), "DepartamentoId", "DepartamentoDesc");
                ViewData["Visita.CiudadId"] = new SelectList(db.Ciudad.Where(c => c.DepartamentoID == model.Visita.DepartamentoId && c.PaisID == model.Visita.PaisId).ToList(), "CiudadId", "CiudadDesc");
                
            }
            else
            {
                ViewData["Visita.DepartamentoId"] = new SelectList(db.Departamento.ToList(), "DepartamentoId", "DepartamentoDesc");
                ViewData["Visita.CiudadId"] = new SelectList(db.Ciudad.ToList(), "CiudadId", "CiudadDesc");
            }

            ViewBag.UsuarioId = Seguridadcll.Usuario.UsuarioId;
            ViewBag.Marcas = new SelectList(db.Marca.ToList(), "Id", "Nombre");
            ViewBag.UsuarioNombre = Seguridadcll.Usuario.UsuarioNombre;
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
        public async Task<ActionResult> Edit(VisitasViewModel model, IEnumerable<HttpPostedFileBase> Photos)
        {

            if (ModelState.IsValid)
            {
                db.Entry(model.Visita).State = EntityState.Modified;
                await db.SaveChangesAsync();



                #region VisitaFotos
                var currentPhotos = await db.VisitaFoto
                                           .Where(vf => vf.VisitaId == model.Visita.Id)
                                           .ToListAsync();


                if (model.VisitaFotos != null)
                {
                    var PhotosId = model.VisitaFotos.Select(a => a.Order).ToArray();


                    var photosToDelete = currentPhotos.Where(a => !PhotosId.Contains(a.Order)).ToList();

                    if (photosToDelete.Count > 0)
                    {
                        db.VisitaFoto.RemoveRange(photosToDelete);
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    var photosToDelete = currentPhotos.ToList();

                    if (photosToDelete.Count > 0)
                    {
                        db.VisitaFoto.RemoveRange(photosToDelete);
                        await db.SaveChangesAsync();
                    }
                }

                int order = 0;
                if (currentPhotos.Count > 0)
                    order = currentPhotos.Select(i => i.Order).Max() + 1;

                if (Photos != null)
                    await UploadPhotos(model.Visita.Id, Photos, order);

                #endregion

                AddLog("", model.Visita.Id, model);
                return RedirectToAction("Index", GetReturnSearch());
            }

            return await GetView(model.Visita.Id);
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
            var visita = await db.Visita.FindAsync(id);
            try
            {
                var visitaFotos = await db.VisitaFoto.Where(vf => vf.VisitaId == id).ToListAsync();

                var photos = visitaFotos.Select(vf => new VisitasViewModel.Fotos
                {
                    Order = vf.Order,
                    FileName = string.Format("data:{0};base64,{1}", vf.MediaType, Convert.ToBase64String(vf.Foto))
                }).ToList();
                
                VisitasViewModel vm = new VisitasViewModel { Visita = visita, VisitaFotos = photos };


                db.VisitaFoto.RemoveRange(visitaFotos);
                await db.SaveChangesAsync();

                db.Visita.Remove(visita);
                await db.SaveChangesAsync();

                AddLog("", vm.Visita.Id, vm);

                return RedirectToAction("Index", GetReturnSearch());

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(id);
        }

        private async Task<bool> UploadPhotos(int Id, IEnumerable<HttpPostedFileBase> Photos, int porder)
        {


            int order = porder;
            foreach (var file in Photos)
            {
                if (file != null && file.ContentLength > 0)
                {
                    VisitaFoto vf = new VisitaFoto();
                    vf.VisitaId = Id;
                    vf.Order = order++;
                    vf.Foto = Fn.ConvertToByte(file);
                    //da.FileName = file.FileName;
                    vf.MediaType = file.ContentType;
                    db.VisitaFoto.Add(vf);
                }

            }


            if (db.VisitaFoto.Local.Count > 0)
                await db.SaveChangesAsync();

            return true;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> FotoGeneral(int v, int o, int s)
        {
            var data = await db.VisitaFoto
                .Where(f => f.VisitaId == v && f.Order == o)
                .Select(f => new { f.Foto, f.MediaType })
                .FirstOrDefaultAsync();

            if (data == null)
                return HttpNotFound();
            else
            {
                Bitmap bmp;
                using (var ms = new MemoryStream(data.Foto))
                    bmp = new Bitmap(ms);

                var scaledBmp = s != 0 ? Fn.ScaleBitmap(bmp, s) : bmp;

                return File(Fn.ImageToByte(scaledBmp), data.MediaType);
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> FotoPublicidad(int v, int o, int s)
        {
            var data = await db.VisitaPublicidad
                .Where(f => f.VisitaId == v && f.Order == o)
                .Select(f => new { f.Foto, f.MediaType })
                .FirstOrDefaultAsync();

            if (data == null || data.Foto == null || data.MediaType == null)
                return HttpNotFound();
            else
            {
                Bitmap bmp;
                using (var ms = new MemoryStream(data.Foto))
                    bmp = new Bitmap(ms);

                var scaledBmp = s != 0 ? Fn.ScaleBitmap(bmp, s) : bmp;

                return File(Fn.ImageToByte(scaledBmp), data.MediaType);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
