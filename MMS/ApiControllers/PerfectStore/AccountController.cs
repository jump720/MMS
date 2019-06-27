using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin.Infrastructure;
using MMS.Classes;
using System.Web;

namespace MMS.ApiControllers.PerfectStore
{
    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [RoutePrefix("api/Account")]
    public class AccountController : ApiAppBaseController
    {
        private MMSContext db = new MMSContext();

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginData data)
        {
            try
            {
                var result = new AjaxResult();

                var usuario = await db.Usuarios
                    .Include(u => u.RolUsuarioList)
                    .Where(u => u.Usuarioactivo && u.UsuarioId == data.Username && u.Usuariopassword == data.Password)
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return Unauthorized();

                var rolesId = usuario.RolUsuarioList.Select(ru => ru.RolId).ToArray();

                if (await db.RolObjeto.Where(ro => rolesId.Contains(ro.RolId) && ro.ObjetoId == "PerfectStore").AnyAsync())
                {
                    var identity = new ClaimsIdentity(Startup.OAuthBearerOptions.AuthenticationType);
                    identity.AddClaim(new Claim(ClaimTypes.Name, data.Username));
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId));
                    AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());

                    var currentUtc = new SystemClock().UtcNow;
                    ticket.Properties.IssuedUtc = currentUtc;
                    ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(365));

                    string token = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
                    DateTime tokenDate = DateTime.Now;

                    db.UsuarioToken.Add(new UsuarioToken
                    {
                        UsuarioId = usuario.UsuarioId,
                        Token = token,
                        FechaCreacion = tokenDate,
                        FechaUltimoUso = tokenDate
                    });

                    await db.SaveChangesAsync();

                    result.Data = new
                    {
                        Usuario = new
                        {
                            Id = usuario.UsuarioId,
                            Nombre = usuario.UsuarioNombre,
                            Correo = usuario.UsuarioCorreo,
                        },
                        Token = token
                    };

                    AddLog("Login", "", result.Data, usuario.UsuarioId);
                    return Ok(result);
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IHttpActionResult> Logout()
        {
            try
            {
                string token = Usuario.UsuarioTokensList.FirstOrDefault().Token;
                var usuarioToken = await db.UsuarioToken.Where(ut => ut.Token == token).FirstOrDefaultAsync();

                db.UsuarioToken.Remove(usuarioToken);
                await db.SaveChangesAsync();

                AddLog("Logout", "", usuarioToken, Usuario.UsuarioId);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Paises")]
        public async Task<IHttpActionResult> Paises()
        {
            try
            {
                var paises = await db.Pais.Select(p => new
                {
                    Id = p.PaisID,
                    Nombre = p.PaisDesc
                }).ToListAsync();

                return Ok(new AjaxResult { Data = paises });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Departamentos")]
        public async Task<IHttpActionResult> Departamentos()
        {
            try
            {
                var departamentos = await db.Departamento.Select(d => new
                {
                    PaisId = d.PaisID,
                    Id = d.DepartamentoID,
                    Nombre = d.DepartamentoDesc
                }).ToListAsync();

                return Ok(new AjaxResult { Data = departamentos });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Ciudades")]
        public async Task<IHttpActionResult> Ciudades()
        {
            try
            {
                var ciudades = await db.Ciudad.Select(c => new
                {
                    PaisId = c.PaisID,
                    DepartamentoId = c.DepartamentoID,
                    Id = c.CiudadID,
                    Nombre = c.CiudadDesc
                }).ToListAsync();

                return Ok(new AjaxResult { Data = ciudades });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Clientes")]
        public async Task<IHttpActionResult> Clientes()
        {
            try
            {
                var clientes = await db.Clientes.Select(c => new
                {
                    Id = c.ClienteID,
                    RazonSocial = c.ClienteRazonSocial,
                    PaisId = c.PaisID,
                    DepartamentoId = c.DepartamentoID,
                    CiudadId = c.CiudadID,
                }).ToListAsync();

                return Ok(new AjaxResult { Data = clientes });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Plantillas")]
        public async Task<IHttpActionResult> Plantillas()
        {
            try
            {
                var plantillas = await db.Plantilla
                    .Where(pi => pi.Activa)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Nombre = c.Nombre
                    }).ToListAsync();

                return Ok(new AjaxResult { Data = plantillas });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("PlantillasItems")]
        public async Task<IHttpActionResult> PlantillasItems()
        {
            try
            {
                var plantillasItems = await db.PlantillaItem
                    .Include(pi => pi.Plantilla)
                    .Where(pi => pi.Plantilla.Activa)
                    .Select(pi => new
                    {
                        PlantillaId = pi.PlantillaId,
                        ItemId = pi.ItemId
                    }).ToListAsync();

                return Ok(new AjaxResult { Data = plantillasItems });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Marcas")]
        public async Task<IHttpActionResult> Marcas()
        {
            try
            {
                var marcas = await db.Marca.Select(m => new
                {
                    Id = m.Id,
                    Nombre = m.Nombre
                }).ToListAsync();

                return Ok(new AjaxResult { Data = marcas });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Usuarios")]
        public async Task<IHttpActionResult> Usuarios()
        {
            try
            {
                var usuarios = await db.Usuarios.Select(u => new
                {
                    Id = u.UsuarioId,
                    Nombre = u.UsuarioNombre,
                    Correo = u.UsuarioCorreo
                })
                .OrderBy(u => u.Nombre)
                .ToListAsync();

                return Ok(new AjaxResult { Data = usuarios });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("Items")]
        public async Task<IHttpActionResult> Items(int step, int take)
        {
            try
            {
                if (step > 0)
                    step--;

                if (take == 0)
                {
                    int count = await db.Item.CountAsync();
                    take = (count / 6) + 1;
                }

                var items = await db.Item.Select(i => new
                {
                    Id = i.Id,
                    MarcaId = i.MarcaId,
                    Codigo = i.Codigo,
                    Categoria = i.Categoria,
                    Grupo = i.Grupo,
                    Descripcion = i.Descripcion,
                    PrecioSugerido = i.PrecioSugerido
                })
                .OrderBy(i => i.Id)
                .Skip(step * take)
                .Take(take)
                .ToListAsync();

                if (step == 5)
                    AddLog("UpdateBaseData", "", null);

                return Ok(new AjaxResult { Data = items });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("CheckConfig")]
        public async Task<IHttpActionResult> CheckConfig()
        {
            try
            {
                var fechas = new List<DateTime>();
                var fechaConf = await db.Configuracion.Select(c => c.ConfigFechaActualizacionDatosBase).FirstOrDefaultAsync();
                fechas.Add((DateTime)fechaConf);
                fechas.Add(DateTime.Now);

                return Ok(new AjaxResult { Data = fechas });
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
