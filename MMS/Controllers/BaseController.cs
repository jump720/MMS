using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace MMS.Controllers
{
    [AuthorizeUser]
    [SaveLog]
    public class BaseController : Controller
    {
        public Seguridadcll Seguridadcll
        {
            get { return (Seguridadcll)Session["seguridad"]; }
        }

        [NonAction]
        public void AddLog(string evento, object key, object data)
        {
            int? eventoId;

            evento = string.IsNullOrEmpty(evento) ? GetControllerActionName() : evento;

            using (var db = new MMSContext())
                eventoId = db.Evento.FirstOrDefault(e => e.Nombre == evento && e.Activo)?.Id;

            if (eventoId == null)
                return;

            List<Log> logs;

            if (HttpContext.Items["__logs"] == null)
                logs = new List<Log>();
            else
                logs = (List<Log>)HttpContext.Items["__logs"];

            logs.Add(new Log()
            {
                Fecha = DateTime.Now,
                Usuario = Seguridadcll.Usuario.UsuarioId,
                Data = Fn.GetJsonString(data),
                Cliente = Request.Browser.Browser,
                EventoId = (int)eventoId,
                Key = key.ToString()
            });

            HttpContext.Items["__logs"] = logs;
        }

        [NonAction]
        [ValidateInput(false)]
        public RouteValueDictionary GetReturnSearch()
        {
            string returnSearch = Request.Params["returnSearch"];
            var routes = new RouteValueDictionary();

            if (string.IsNullOrWhiteSpace(returnSearch))
                return routes;

            string[] queries = returnSearch.Split('&');

            if (queries.Length > 0)
                queries[0] = queries[0].Replace("?", "");

            foreach (string query in queries)
            {
                string[] queryValue = query.Split('=');
                routes.Add(queryValue[0], queryValue[1]);
            }

            return routes;
        }

        [NonAction]
        [ValidateInput(false)]
        public RouteValueDictionary GetActualReturnSearch()
        {
            string returnSearch = Request.Params["returnSearch"];
            var routes = new RouteValueDictionary();

            if (string.IsNullOrWhiteSpace(returnSearch))
                return routes;

            routes.Add("returnSearch", returnSearch);
            return routes;
        }

        [NonAction]
        public Fn.CrudMode GetCrudMode()
        {
            string actionName = ControllerContext.RouteData.Values["action"].ToString();
            Fn.CrudMode crudMode = Fn.CrudMode.Other;

            if (actionName.Contains("Create"))
                crudMode = Fn.CrudMode.Create;
            else if (actionName.Contains("Edit"))
                crudMode = Fn.CrudMode.Edit;
            else if (actionName.Contains("Details"))
                crudMode = Fn.CrudMode.Details;
            else if (actionName.Contains("Delete"))
                crudMode = Fn.CrudMode.Delete;

            return crudMode;
        }

        [NonAction]
        public string GetControllerActionName()
        {
            return ControllerContext.RouteData.Values["controller"].ToString() + "/" + ControllerContext.RouteData.Values["action"].ToString();
        }

        private string GetCrudName()
        {
            var mode = GetCrudMode();

            string modeName = "";

            if (mode == Fn.CrudMode.Create)
                modeName = "Crear";
            else if (mode == Fn.CrudMode.Edit)
                modeName = "Modificar";
            else if (mode == Fn.CrudMode.Delete)
                modeName = "Eliminar";
            else if (mode == Fn.CrudMode.Details)
                modeName = "Ver";

            return modeName;
        }
    }
}