using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Data.Entity;
using MMS.Classes;

namespace MMS.Filters
{
    public class ApiAuthorizeUserToken : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;

            if (actionContext.Request.Headers.Authorization == null)
            {
                HandleUnauthorizedRequest(actionContext);
                return;
            }

            string scheme = actionContext.Request.Headers.Authorization.Scheme;
            string token = actionContext.Request.Headers.Authorization.Parameter;

            if (string.IsNullOrWhiteSpace(scheme) || string.IsNullOrWhiteSpace(token) || scheme != "Bearer")
            {
                HandleUnauthorizedRequest(actionContext);
                return;
            }

            using (var db = new MMSContext())
            {
                var data = db.UsuarioToken
                    .Include(ut => ut.Usuario)
                    .Include(ut => ut.Usuario.RolUsuarioList)
                    .Where(ut => ut.Usuario.Usuarioactivo && ut.Token == token)
                    .Select(ut => new
                    {
                        Usuario = ut.Usuario,
                        UsuarioToken = ut,
                        RolesId = ut.Usuario.RolUsuarioList.Select(ru => ru.RolId).ToList()
                    })
                    .FirstOrDefault();

                if (data == null || !db.RolObjeto.Where(ro => data.RolesId.Contains(ro.RolId) && ro.ObjetoId == "PerfectStore").Any())
                {
                    HandleUnauthorizedRequest(actionContext);
                    return;
                }

                data.UsuarioToken.FechaUltimoUso = DateTime.Now;

                db.Entry(data.UsuarioToken).State = EntityState.Modified;
                db.SaveChanges();

                HttpContext.Current.Items["__usuario"] = data.Usuario;
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            Fn.AddLog("", "Unauthorized", "", null, HttpContext.Current.Request.Browser.Browser);
            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}