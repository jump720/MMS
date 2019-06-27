using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace MMS.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ApiAuthorizeAction : AuthorizeAttribute
    {
        string objetoId = "";

        public ApiAuthorizeAction()
        {
        }

        public ApiAuthorizeAction(string objetoId)
        {
            this.objetoId = objetoId.ToLower();
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
                return;

            if (string.IsNullOrWhiteSpace(objetoId))
                objetoId = (actionContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + actionContext.ActionDescriptor.ActionName).ToLower();

            Seguridadcll seguridadcll = (Seguridadcll)HttpContext.Current.Session["seguridad"];

            var objetos = seguridadcll.RolObjetoList.Select(ro => ro.ObjetoId.Trim().ToLower()).Distinct().ToList();

            if (objetos.Any(o => o == objetoId))
                return;

            HandleUnauthorizedRequest(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }
    }
}