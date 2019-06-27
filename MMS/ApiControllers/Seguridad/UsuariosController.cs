using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using MMS.Filters;
using MMS.Models;
using MMS.Classes;

namespace MMS.ApiControllers.Seguridad
{
    public class UsuariosController : ApiBaseController
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


                string usuarioid = form["_usuarioid"];
                string usuarionombre = form["_usuarionombre"];
                string usuariocorreo = form["_usuariocorreo"];
                string usuariopadreid = form["_usuariopadreid"];
                string usuariopadrenombre = form["_usuariopadrenombre"];

                //string search = form["search[value]"];

                var countQuery = db.Usuarios.Include(u => u.UsuarioPadre).Select(u => new { u.UsuarioId, u.UsuarioNombre, u.UsuarioCorreo, u.Usuarioactivo, u.UsuarioPadreId, UsuarioPadreNombre = u.UsuarioPadre.UsuarioNombre });
                var dataQuery = db.Usuarios.Include(u => u.UsuarioPadre).Select(u => new { u.UsuarioId, u.UsuarioNombre, u.UsuarioCorreo, u.Usuarioactivo, u.UsuarioPadreId, UsuarioPadreNombre = u.UsuarioPadre.UsuarioNombre });

                if (!string.IsNullOrWhiteSpace(usuarioid))
                {
                    string value = usuarioid.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(usuarionombre))
                {
                    string value = usuarionombre.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioNombre.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioNombre.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(usuariocorreo))
                {
                    string value = usuariocorreo.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioCorreo.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioCorreo.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(usuariopadreid))
                {
                    string value = usuariopadreid.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioPadreId.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioPadreId.Contains(value));
                }

                if (!string.IsNullOrWhiteSpace(usuariopadrenombre))
                {
                    string value = usuariopadrenombre.Trim();
                    countQuery = countQuery.Where(id => id.UsuarioPadreNombre.Contains(value));
                    dataQuery = dataQuery.Where(id => id.UsuarioPadreNombre.Contains(value));
                }

                int count = await countQuery.CountAsync();

                var data = await dataQuery
                    .OrderBy(a => a.UsuarioId)
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


        [HttpGet]
        public async Task<IHttpActionResult> BuscarUsuario(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Usuario>());

                return Ok(await db.Usuarios
                                .Where(u => (u.UsuarioId.Contains(q) || u.UsuarioNombre.Contains(q)) && u.Usuarioactivo == true)
                                .Select(u => new { u.UsuarioId, u.UsuarioNombre })
                                .Take(50)
                                .ToListAsync());

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetUsuario(string id)
        {
            try
            {

                return Ok(await db.Usuarios
                   .Where(u => u.UsuarioId == id)
                    .Select(i => new
                    {
                        i.UsuarioId,
                        i.UsuarioNombre
                    })
                    .FirstOrDefaultAsync()
                    );
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> BuscarAnalista(string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return Ok(new List<Usuario>());

                return Ok(await db.Usuarios
                                .Where(u => u.UsuarioAnalistaSC == true && u.Usuarioactivo == true && (u.UsuarioId.Contains(q) || u.UsuarioNombre.Contains(q)))
                                .Select(u => new { u.UsuarioId, u.UsuarioNombre })
                                .Take(50)
                                .ToListAsync());

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> UsuarioDescripcion(string id)
        {
            try
            {
                return Ok(await db.Usuarios.Where(u => u.UsuarioId == id).Select(u => u.UsuarioNombre).FirstOrDefaultAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> _ResetPassword(ResetPassword reset)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Usuario usuario = db.Usuarios
                        .Include(u => u.UsuarioTokensList)
                        .Where(u => u.UsuarioId == reset.Usuario)
                        .FirstOrDefault();

                    usuario.Usuariopassword = Fn.EncryptText(reset.Password1);
                    db.UsuarioToken.RemoveRange(usuario.UsuarioTokensList);

                    db.Entry(usuario).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    AddLog("Usuarios/_ResetPassword", usuario.UsuarioId, usuario);

                }
                catch (Exception e)
                {

                    return InternalServerError(e);
                }
            }
            else
            {
                // ViewBag.flag = false;
                return Ok(false);
            }

            return Ok(true);
        }



        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> RememberUser(RememberUser model)
        {

            if (ModelState.IsValid)
            {
                var user = await db.Usuarios
                                   .Where(u => (u.UsuarioId.ToLower() == model.UserOrMail.ToLower() || u.UsuarioCorreo.ToLower() == model.UserOrMail.ToLower()) && u.Usuarioactivo == true)
                                   .FirstOrDefaultAsync();
                if (user != null)
                {  //Genera HASHNroTracking
                    string NewPass = Fn.HASH(model.UserOrMail).Substring(0, 10).ToUpper();
                    user.Usuariopassword = Fn.EncryptText(NewPass);
                    user.UsuarioCambiaContrasena = true;
                    db.Entry(user).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    SendNotificationChangePassTask(user.UsuarioCorreo, user.UsuarioNombre, user.UsuarioId, NewPass);
                    //AddLog("", user, model);
                }
                else
                {
                    return Ok(false);
                }
            }
            else
            {
                return Ok(false);
            }

            return Ok(true);
        }

        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(ChangePassword model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Usuario usuario = db.Usuarios
                        .Include(u => u.UsuarioTokensList)
                        .Where(u => u.UsuarioId == model.Usuario)
                        .FirstOrDefault();

                    usuario.Usuariopassword = Fn.EncryptText(model.Password1);
                    db.UsuarioToken.RemoveRange(usuario.UsuarioTokensList);

                    db.Entry(usuario).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    AddLog("Usuarios/ChangePassword", usuario.UsuarioId, usuario);

                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }
            else
            {
                // ViewBag.flag = false;
                return Ok(false);
            }

            return Ok(true);
        }

        [HttpGet]
        public async Task<IHttpActionResult> ValidatePassword(string Usuario, string PasswordOld)
        {
            try
            {
                string PassEncryp = Fn.EncryptText(PasswordOld);
                var usuario = await db.Usuarios.Where(u => u.UsuarioId == Usuario && u.Usuariopassword == PassEncryp).FirstOrDefaultAsync();
                if (usuario != null)
                {
                    return Ok(true);
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

        [HttpGet]
        public async Task<IHttpActionResult> PorPlanta(string id)
        {
            try
            {
                return Ok(await db.Usuarios
                                .Where(u => u.PlantaID == id && u.Usuarioactivo == true)
                                .Select(u => new { u.UsuarioId, u.UsuarioNombre })
                                .ToListAsync());
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }


        private void SendNotificationChangePassTask(string to, string toName, string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(to))
                return;

            string subject = "AIS - Your new password";
            string msg = $"As you requested, your password for the platform has now been reset. Your new login details are as follows:<br/>";
            msg += "<a href='{url}/Usuarios/Login'>ais.apextoolgroup.com.co<a/> <br/>";
            msg += $"Email: {usuario} <br/>";
            msg += $"Password: {password} <br/>";
            msg += $"<br/>To change your password to something more memorable, after logging in go to My account > Change Password. <br/>";
            msg += $"<br/>Regards <br/>";

            Task.Run(() => Fn.SendHtmlEmail(to, toName, subject, msg, "ais"));
        }

        
    }
}
