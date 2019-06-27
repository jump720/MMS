using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
//using WebMatrix.WebData;
using MMS.Models;
using System.Web.Routing;
using MMS.Controllers.Seguridad;
namespace MMS.Filters
{
    public class SeguridadAttribute : FilterAttribute, IAuthorizationFilter
    {
        public bool isModal { get; set; }
        //public string ViewModal { get; set; }
        void IAuthorizationFilter.OnAuthorization(AuthorizationContext filterContext)
        {

            try
            {
                Seguridad seguridad = new Seguridad();

                Seguridadcll seguridadcll = (Seguridadcll)filterContext.HttpContext.Session["seguridad"];
                if (seguridadcll == null)
                {
                    if (isModal == false)
                    {
                        string ViewModal = "~/Views/Usuarios/Login.cshtml";
                        filterContext.Result = new ViewResult()
                        {
                            ViewData = new ViewDataDictionary { { "ReturnUrl", "Prueba" } },
                            ViewName = ViewModal
                        };
                        //filterContext.HttpContext.Request.Url.AbsoluteUri
                        //filterContext.Controller.TempData["Url"] = filterContext.HttpContext.Request.Url.AbsoluteUri;//Request.Url.AbsoluteUri;


                        filterContext.Result = new RedirectToRouteResult(
                                                    new RouteValueDictionary
                                                    {
                                                        { "controller", "Usuarios" },
                                                        { "action", "Login" },
                                                        { "ReturnUrl" , filterContext.HttpContext.Request.Url.AbsoluteUri}
                                                    }
                                                    );
                    }
                    else
                    {
                        //string ViewModal = "~/Views/Home/_NotAccess.cshtml";
                        //filterContext.Result = new ViewResult()
                        //{
                        //    //ViewData = new ViewDataDictionary { { "UsuarioId", seguridadcll.Usuario.UsuarioId }, { "ObjetoId", ObjetoId } },

                        //    ViewName = ViewModal
                        //};
                    }
                }
                else
                {
                    //Seguridadcll seguridadcll = (Seguridadcll)filterContext.HttpContext.Session["seguridad"];
                    string controller = filterContext.RouteData.Values["controller"].ToString();
                    string action = filterContext.RouteData.Values["action"].ToString();

                    string ObjetoId = controller + "/" + action;
                    bool seguridadFlag = seguridad.validaSeguridad(seguridadcll, ObjetoId);
                    if (!seguridadFlag)
                    {
                        filterContext.Controller.TempData["UsuarioId"] = seguridadcll.Usuario.UsuarioId;
                        filterContext.Controller.TempData["ObjetoId"] = ObjetoId;

                        if (isModal == false)
                        {
                            filterContext.Result = new RedirectToRouteResult(
                                    new RouteValueDictionary
                                    {
                                    { "controller", "Home" },
                                    { "action", "NotAccess" }
                                    });
                        }
                        else
                        {
                            //filterContext.Controller.ViewBag.Error = "IsModal sin permisos";
                            string ViewModal = "~/Views/Home/_NotAccess.cshtml";
                            filterContext.Result = new ViewResult()
                            {
                                ViewData = new ViewDataDictionary { { "UsuarioId", seguridadcll.Usuario.UsuarioId }, { "ObjetoId", ObjetoId } },
                                ViewName = ViewModal
                            };
                        }
                        //new ViewResult();
                        //filterContext.Controller.ViewBag.UsuarioId = seguridadcll.Usuario.UsuarioId;
                        //filterContext.Controller.ViewBag.ObjetoId = ObjetoId;
                    }

                }
            }
            catch //(System.Runtime.Serialization.SerializationException s)
            {
                filterContext.Result = new RedirectToRouteResult(
                                                    new RouteValueDictionary
                                                    {
                                                        { "controller", "Usuarios" },
                                                        { "action", "Login" },
                                                        { "ReturnUrl" , filterContext.HttpContext.Request.Url.AbsoluteUri}
                                                    }
                                                    );
            }
        }
    }
}