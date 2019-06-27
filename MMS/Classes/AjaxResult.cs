using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Classes
{
    public class AjaxResult
    {
        public bool Res { get; private set; }
        public string Msg { get; private set; }
        public object Data { get; set; }

        public AjaxResult()
        {
            Res = true;
        }

        public AjaxResult False()
        {
            Res = false;
            return this;
        }

        public AjaxResult False(string msg)
        {
            Res = false;
            Msg = msg;
            return this;
        }

        public AjaxResult False(string msg, object data)
        {
            Res = false;
            Msg = msg;
            Data = data;
            return this;
        }

        public AjaxResult True()
        {
            Res = true;
            return this;
        }

        public AjaxResult True(string msg)
        {
            Res = true;
            Msg = msg;
            return this;
        }

        public AjaxResult True(string msg, object data)
        {
            Res = true;
            Msg = msg;
            Data = data;
            return this;
        }
    }
}