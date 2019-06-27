using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using MMS.Classes;

namespace MMS.Controllers
{
    public class ProfileController : BaseController
    {
        private MMSContext db = new MMSContext();

        public async Task<ActionResult> Index()
        {
            var usuario = await db.Usuarios
                .Include(u => u.UsuarioPadre)
                .Include(u => u.UsuarioHV)
                .FirstOrDefaultAsync(u => u.UsuarioId == Seguridadcll.Usuario.UsuarioId);

            ViewBag.Nombre = usuario.UsuarioNombre;
            ViewBag.Codigo = usuario.UsuarioId;
            ViewBag.Email = usuario.UsuarioCorreo;
            ViewBag.Manager = usuario.UsuarioPadre?.UsuarioNombre;
            ViewBag.HasProfilePicture = usuario.UsuarioHV?.Foto != null;

            ViewBag.AreaId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre", usuario.UsuarioHV?.AreaId);
            ViewBag.NivelEducativo = new SelectList(Fn.EnumToIEnumarable<NivelEducativo>().ToList(), "Value", "Name", usuario.UsuarioHV?.NivelEducativo);

            return View(usuario.UsuarioHV);
        }

        public async Task<ActionResult> Picture(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                id = Seguridadcll.Usuario.UsuarioId;

            var profilePictureData = await db.UsuarioHV.Where(u => u.UsuarioId == id).Select(u => new { u.Foto, u.FotoMediaType }).FirstOrDefaultAsync();

            if (profilePictureData?.Foto == null || profilePictureData?.FotoMediaType == null)
                return File("/Content/dist/images/default_user.png", "image/png");

            return File(profilePictureData.Foto, profilePictureData.FotoMediaType);
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