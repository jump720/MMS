using MMS.Controllers.Seguridad;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MMS.Filters
{
    public class AuthorizeUser : AuthorizeAttribute
    {
        class Menu
        {
            public Objeto Objeto { get; set; }
            public bool Active { get; set; }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AllowAnonymousAttribute), false).Any())
                return;

            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                Seguridadcll seguridadcll = (Seguridadcll)filterContext.HttpContext.Session["seguridad"];

                if (seguridadcll == null)
                    using (var db = new MMSContext())
                    {
                        var usuario = db.Usuarios.FirstOrDefault(u => u.UsuarioId == filterContext.HttpContext.User.Identity.Name && u.Usuarioactivo);
                        seguridadcll = new UsuariosController().CargueSeguridad(usuario, HttpContext.Current);
                    }

                string host = filterContext.HttpContext.Request.Url.Host;
                string appName = host.Substring(0, host.IndexOf('.'));

                seguridadcll.Aplicacion = seguridadcll.Aplicaciones.FirstOrDefault(a => a.Link == appName);

                if (seguridadcll.Aplicacion == null) // no tiene permiso para la app
                {
                    base.HandleUnauthorizedRequest(filterContext);
                    return;
                }

                HttpContext.Current.Session["seguridad"] = seguridadcll;

                if (!filterContext.HttpContext.Request.IsAjaxRequest()) // no crear menu cuando sea una peticion Ajax
                {
                    var menus = seguridadcll.ObjetosMenuList
                        .Concat(seguridadcll.ObjetosMenuDirectorioList)
                        .Where(o => o.AplicacionObjetos.Any(ao => ao.AplicacionId == seguridadcll.Aplicacion.Id))
                        .Select(o => new Menu()
                        {
                            Objeto = o,
                            Active = false
                        })
                    .ToList();

                    var sbMenu = new StringBuilder();
                    string urlMenu = filterContext.HttpContext.Request.Url.AbsolutePath;

                    if (urlMenu.StartsWith("/"))
                        urlMenu = urlMenu.Remove(0, 1);

                    if (urlMenu.EndsWith("/"))
                        urlMenu = urlMenu.Remove(urlMenu.LastIndexOf('/'));

                    if (!urlMenu.ToLower().EndsWith("/index") && filterContext.HttpContext.Request.Url.Segments.Length == 2)
                        urlMenu += "/Index";

                    var menu = menus.Where(m => m.Objeto.ObjetoId == urlMenu).FirstOrDefault();

                    if (menu == null && filterContext.HttpContext.Request.Url.Segments.Length > 2)
                        for (int i = 2; i < filterContext.HttpContext.Request.Url.Segments.Length; i++)
                            urlMenu = urlMenu.Remove(urlMenu.LastIndexOf('/'));

                    if (!urlMenu.ToLower().EndsWith("/index"))
                        urlMenu += "/Index";

                    menu = menus.Where(m => m.Objeto.ObjetoId.ToLower() == urlMenu.ToLower()).FirstOrDefault();

                    while (menu != null)
                    {
                        menu.Active = true;
                        menu = menus.Where(m => m.Objeto.ObjetoId == menu.Objeto.ObjetoIdPadre).FirstOrDefault();
                    }

                    var menusProcesar = menus.Where(m => m.Objeto.ObjetoIdPadre == null).OrderBy(m => m.Objeto.ObjetoOrden).ToList();
                    GenerarMenu(menusProcesar, menus, sbMenu);

                    filterContext.Controller.ViewBag.SidebarMenuHtml = sbMenu.ToString();
                }
            }

            base.OnAuthorization(filterContext);
        }

        private void GenerarMenu(List<Menu> menusProcesar, List<Menu> menus, StringBuilder sbMenu)
        {
            foreach (var menu in menusProcesar)
            {
                string href = "", classname = "";

                if (menu.Objeto.ObjetoId.StartsWith("__"))
                {
                    href = "javascript:void(0);";
                    classname = "menu-toggle";
                }
                else
                    href = "/" + menu.Objeto.ObjetoId.Trim();

                sbMenu.Append($"<li {(menu.Active ? "class='active'" : "")}><a href='{href}' class='{classname}'>{(string.IsNullOrEmpty(menu.Objeto.ObjetoIcono) ? "" : $"<i class='material-icons'>{menu.Objeto.ObjetoIcono}</i>")}<span>{menu.Objeto.ObjetoDesc}</span></a>");

                var subMenusProcesar = menus.Where(m => m.Objeto.ObjetoIdPadre == menu.Objeto.ObjetoId).OrderBy(m => m.Objeto.ObjetoOrden).ToList();

                if (subMenusProcesar.Count > 0)
                {
                    sbMenu.Append("<ul class='ml-menu'>");
                    GenerarMenu(subMenusProcesar, menus, sbMenu);
                    sbMenu.Append("</ul>");
                }
                else
                    sbMenu.Append("</li>");
            }
        }
    }
}