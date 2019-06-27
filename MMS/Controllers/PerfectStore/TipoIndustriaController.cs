using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MMS.Models;
using MMS.Filters;
namespace MMS.Controllers.PerfectStore
{
    public class TipoIndustriaController : BaseController
    {
        
        private MMSContext db = new MMSContext();

        // GET: TipoIndustria
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(int id)
        {
            var tipo = await db.TipoIndustrias.FindAsync(id);
            if (tipo == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), tipo);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return PartialView("_Create");
        }


        [AuthorizeAction]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Edit(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(int id)
        {
            return await GetView(id);
        }
    }
}