using MMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MMS.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FillPermission : ActionFilterAttribute
    {
        bool crud;
        string[] objetos;
        List<string> objetosUsuario;

        public string TrueValue { get; set; }
        public string FalseValue { get; set; }
        public bool BoolValues { get; set; }

        public FillPermission()
        {
            crud = true;
            objetos = new string[] { };
            InitValues();
        }

        public FillPermission(params string[] objetos)
        {
            crud = false;
            this.objetos = objetos;
            InitValues();
        }

        private void InitValues()
        {
            TrueValue = TrueValue ?? "";
            FalseValue = FalseValue ?? "hide";
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var seguridadCll = (Seguridadcll)HttpContext.Current.Session["seguridad"];
            objetosUsuario = seguridadCll.RolObjetoList.Select(ro => ro.ObjetoId).Distinct().ToList();

            if (crud)
            {
                string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

                SetViewDataValue(filterContext, controllerName + "/Create");
                SetViewDataValue(filterContext, controllerName + "/Edit");
                SetViewDataValue(filterContext, controllerName + "/Details");
                SetViewDataValue(filterContext, controllerName + "/Delete");
            }

            foreach (var objeto in objetos)
                SetViewDataValue(filterContext, objeto);
        }

        private void SetViewDataValue(ActionExecutedContext filterContext, string objeto)
        {
            object viewDataValue;
            bool tienePermiso = objetosUsuario.Any(p => p.ToLower() == objeto.ToLower());

            if (BoolValues)
                viewDataValue = tienePermiso;
            else
                viewDataValue = tienePermiso ? TrueValue : FalseValue;

            filterContext.Controller.ViewData["has_" + objeto.Replace("/", "")] = viewDataValue;
        }
    }
}