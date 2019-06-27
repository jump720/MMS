using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MMS.Classes
{
    public class DataTablePager
    {
        public string sEcho { get; set; }
        public int iTotalRecords { get; set; }
        public int iTotalDisplayRecords { get; set; }
        public int recordsFiltered { get; set; }
        public dynamic aaData { get; set; }
    }
}