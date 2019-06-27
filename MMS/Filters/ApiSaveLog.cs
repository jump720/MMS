using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace MMS.Filters
{
    public class ApiSaveLog : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (HttpContext.Current.Items["__logs"] == null)
                return;

            List<Log> logs = (List<Log>)HttpContext.Current.Items["__logs"];

            using (var db = new MMSContext())
            {
                db.Log.AddRange(logs);
                db.SaveChanges();
            }
        }
    }
}