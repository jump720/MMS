using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MMS.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeAction : AuthorizeAttribute
    {
        string objetoId = "";
        string objetoIdView = "";

        public AuthorizeAction()
        {
        }

        public AuthorizeAction(string objetoId)
        {
            objetoIdView = objetoId;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Any())
                return;

            if (string.IsNullOrWhiteSpace(objetoIdView))
                objetoIdView = (filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + filterContext.ActionDescriptor.ActionName);

            objetoId = objetoIdView.ToLower();

            var seguridadcll = (Seguridadcll)filterContext.HttpContext.Session["seguridad"];
            var objetos = seguridadcll.RolObjetoList.Select(ro => ro.ObjetoId.Trim().ToLower()).Distinct().ToList();

            if (objetos.Any(o => o == objetoId))
                return;

            HandleUnauthorizedRequest(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Test",
                        action = "Unauthorized",
                        objetoId = objetoIdView
                    })
                );
        }
    }
}