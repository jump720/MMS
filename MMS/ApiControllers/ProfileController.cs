using MMS.Classes;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MMS.ApiControllers
{
    public class ProfileController : ApiBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpPost]
        public async Task<IHttpActionResult> CambiarFoto([FromBody]string profilePictureBase64)
        {
            try
            {
                var usuarioHV = await db.UsuarioHV.FirstOrDefaultAsync(u => u.UsuarioId == Seguridadcll.Usuario.UsuarioId);
                string[] profilePicture = profilePictureBase64.Split(',');

                using (var ms = new MemoryStream(Convert.FromBase64String(profilePicture[1])))
                {
                    var bitmap = Fn.ResizeBitmap(new System.Drawing.Bitmap(ms), 200, 200);

                    if (usuarioHV == null)
                    {
                        usuarioHV = new UsuarioHV
                        {
                            UsuarioId = Seguridadcll.Usuario.UsuarioId,
                            FotoMediaType = profilePicture[0].Replace("data:", "").Replace(";base64", ""),
                            Foto = Fn.BitmapToByte(bitmap),
                        };
                        db.UsuarioHV.Add(usuarioHV);
                    }
                    else
                    {
                        usuarioHV.FotoMediaType = profilePicture[0].Replace("data:", "").Replace(";base64", "");
                        usuarioHV.Foto = Fn.BitmapToByte(bitmap);
                        db.Entry(usuarioHV).State = EntityState.Modified;
                    }
                }

                await db.SaveChangesAsync();
                AddLog("", usuarioHV.UsuarioId, null);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> EliminarFoto()
        {
            try
            {
                var usuarioHV = await db.UsuarioHV.FirstOrDefaultAsync(u => u.UsuarioId == Seguridadcll.Usuario.UsuarioId);
                if (usuarioHV == null)
                    return NotFound();

                usuarioHV.FotoMediaType = null;
                usuarioHV.Foto = null;

                db.Entry(usuarioHV).State = EntityState.Modified;
                await db.SaveChangesAsync();

                AddLog("", usuarioHV.UsuarioId, null);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> CambiarInformacion(UsuarioHV model)
        {
            try
            {
                var usuarioHV = await db.UsuarioHV
                    .FirstOrDefaultAsync(u => u.UsuarioId == Seguridadcll.Usuario.UsuarioId);

                if (usuarioHV == null)
                {
                    usuarioHV = new UsuarioHV();
                    db.UsuarioHV.Add(usuarioHV);
                }
                else
                    db.Entry(usuarioHV).State = EntityState.Modified;

                usuarioHV.UsuarioId = Seguridadcll.Usuario.UsuarioId;
                usuarioHV.Identificacion = model.Identificacion;
                usuarioHV.FechaNacimiento = model.FechaNacimiento;
                usuarioHV.NivelEducativo = model.NivelEducativo;
                usuarioHV.FechaIngreso = model.FechaIngreso;
                usuarioHV.Celular = model.Celular;
                usuarioHV.Contacto = model.Contacto;

                await db.SaveChangesAsync();

                usuarioHV.Foto = null; // no guardar foto en log
                AddLog("", usuarioHV.UsuarioId, usuarioHV);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> PeakPerformance()
        {
            try
            {
                var peaks = await db.Peak
                    .Include(p => p.Periodo)
                    .Include(p => p.PeakObjetivos)
                    .Where(p => p.UsuarioId == Seguridadcll.Usuario.UsuarioId && p.Estado == EstadoPeak.Finished)
                    .OrderBy(p => p.Periodo.FechaIni)
                    .Select(p => new
                    {
                        Periodo = p.Periodo.Descripcion,
                        Score = p.PeakObjetivos.Sum(po => po.Factor) + p.FactorAjuste,
                        Completado = p.PeakObjetivos.Average(po => po.Completado)
                    })
                    .ToListAsync();

                return Ok(peaks);
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
