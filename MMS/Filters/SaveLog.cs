using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MMS.Filters
{
    public class SaveLog : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (HttpContext.Current.Items["__logs"] == null)
                return;

            List<Log> logs = (List<Log>)HttpContext.Current.Items["__logs"];

            if (logs.Count > 0)
                using (var db = new MMSContext())
                {
                    db.Log.AddRange(logs);
                    db.SaveChanges();
                }
        }
    }
}