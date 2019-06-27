using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers
{
    [ApiAuthorizeUser]
    [ApiSaveLog]
    public class ApiBaseController : ApiController
    {
        public Seguridadcll Seguridadcll
        {
            get { return (Seguridadcll)HttpContext.Current.Session["seguridad"]; }
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

            if (HttpContext.Current.Items["__logs"] == null)
                logs = new List<Log>();
            else
                logs = (List<Log>)HttpContext.Current.Items["__logs"];

            logs.Add(new Log()
            {
                Fecha = DateTime.Now,
                Usuario = Seguridadcll.Usuario.UsuarioId,
                Data = Fn.GetJsonString(data),
                Cliente = HttpContext.Current.Request.Browser.Browser,
                EventoId = (int)eventoId,
                Key = key.ToString()
            });

            HttpContext.Current.Items["__logs"] = logs;
        }

        [NonAction]
        public string GetControllerActionName()
        {
            return ControllerContext.ControllerDescriptor.ControllerName + "/" + ActionContext.ActionDescriptor.ActionName;
        }
    }
}