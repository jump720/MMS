using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace MMS.App_Start
{
    public class CustomSessionIDManager : SessionIDManager, ISessionIDManager
    {
        void ISessionIDManager.SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded)
        {
            base.SaveSessionID(context, id, out redirected, out cookieAdded);

            string name = "ASP.NET_SessionId";
            var cookie = context.Response.Cookies[name];

            if (cookie == null)
                return;

            string host = context.Request.Url.Host;
            cookie.Domain = host.Substring(host.IndexOf('.'));
        }
    }

}