using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace MMS.ApiControllers
{
    [ApiAuthorizeUserToken]
    [ApiSaveLog]
    public class ApiAppBaseController : ApiController
    {
        public Usuario Usuario
        {
            get { return (Usuario)HttpContext.Current.Items["__usuario"]; }
        }

        [NonAction]
        public void AddLog(string evento, object key, object data, string usuarioId = null)
        {
            if (string.IsNullOrEmpty(evento))
                return;

            int? eventoId;
            usuarioId = string.IsNullOrWhiteSpace(usuarioId) ? Usuario.UsuarioId : usuarioId;

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
                Usuario = usuarioId,
                Data = Fn.GetJsonString(data),
                Cliente = HttpContext.Current.Request.Browser.Browser,
                EventoId = (int)eventoId,
                Key = key.ToString()
            });

            HttpContext.Current.Items["__logs"] = logs;
        }
    }
}
