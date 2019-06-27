using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MMS.Models;
using MMS.Filters;

namespace MMS.Controllers.Seguridad
{
    public class AuditoriaApiController : ApiController
    {
        private MMSContext db = new MMSContext();

        public SysDataTablePager Get()
        {

            NameValueCollection nvc = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            string sEcho = nvc["draw"].ToString();//pagina
            int iDisplayStart = Convert.ToInt32(nvc["Start"]);//numero de objeto a esconder
            int iDisplayLength = Convert.ToInt32(nvc["Length"]);//tamaño de la grilla
            string sSearch = nvc["search[value]"].ToString().ToLower();//filtro smart (global)

            List<Auditoria> AuditoriaList = null;
            var Count = 0;
            try
            {
                if (!string.IsNullOrEmpty(sSearch))
                {
                    AuditoriaList = (from a in db.Auditoria
                                     select a).ToList()
                                    .Where(a => a.AuditoriaId.ToString().Contains(sSearch) ||
                                                 (a.AuditoriaFecha != null && a.AuditoriaFecha.ToString("d").ToLower().Contains(sSearch)) ||
                                                 (a.AuditoriaHora != null && a.AuditoriaHora.ToString().ToLower().Contains(sSearch)) ||
                                                 (!string.IsNullOrEmpty(a.usuarioId) && a.usuarioId.ToLower().Contains(sSearch)) ||
                                                 (!string.IsNullOrEmpty(a.AuditoriaEvento) && a.AuditoriaEvento.ToLower().Contains(sSearch)) ||
                                                 (!string.IsNullOrEmpty(a.AuditoriaDesc) && a.AuditoriaDesc.ToLower().Contains(sSearch)) ||
                                                 (!string.IsNullOrEmpty(a.ObjetoId) && a.ObjetoId.ToLower().Contains(sSearch)) ||
                                                 (!string.IsNullOrEmpty(a.AuditoriaEquipo) && a.AuditoriaEquipo.ToLower().Contains(sSearch))
                                     ).ToList();

                }
                else
                {
                    AuditoriaList = db.Auditoria.ToList();
                }
                Count = AuditoriaList.Count;
                AuditoriaList = AuditoriaList.OrderByDescending(a => a.AuditoriaId).Skip(iDisplayStart).Take(iDisplayLength).ToList();

            }
            catch
            {
                AuditoriaList = new List<Auditoria>();
            }
            var CustomerPaged = new SysDataTablePager();

            CustomerPaged.draw = sEcho;
            CustomerPaged.recordsTotal = Count;
            CustomerPaged.recordsFiltered = Count;
            CustomerPaged.data = AuditoriaList;

            return CustomerPaged;
        }
    }
}
