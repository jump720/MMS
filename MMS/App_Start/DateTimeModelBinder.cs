using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Mvc;
namespace MMS
{
    public class DateTimeModelBinder : IModelBinder
    {

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            object result = null;
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            ModelState modelState = new ModelState { Value = valueResult };

            if (valueResult != null)
            {
                try
                {
                    //var stateHandler = new StateHandler(controllerContext.HttpContext.Session);
                    result = valueResult.ConvertTo(typeof(DateTime?), CultureInfo.CurrentCulture);
                }
                catch
                {
                    try
                    {
                        result = valueResult.ConvertTo(typeof(DateTime?), CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        modelState.Errors.Add(e);
                        //_log.Error("DateTimeModelBinder parse exception", ex);
                        //_log.KeyValue("AttemptedValue", valueResult.AttemptedValue);
                    }
                }
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);

            return result;
        }
    }
}