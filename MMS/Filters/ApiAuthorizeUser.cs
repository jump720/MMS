using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace MMS.Filters
{
    public class ApiAuthorizeUser : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;

            if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                Seguridadcll seguridadcll = (Seguridadcll)HttpContext.Current.Session["seguridad"];

                if (seguridadcll == null)
                    using (var db = new MMSContext())
                    {
                        var usuario = db.Usuarios.FirstOrDefault(u => u.UsuarioId == actionContext.RequestContext.Principal.Identity.Name && u.Usuarioactivo);
                        new Controllers.Seguridad.UsuariosController().CargueSeguridad(usuario, HttpContext.Current);
                    }
            }

            base.OnAuthorization(actionContext);
        }
    }
}