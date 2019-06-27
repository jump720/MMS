using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using System.Web.Security;
using System.Threading.Tasks;
using MMS.Classes;
namespace MMS.Controllers.Catalogos
{
    public class MarcasController : BaseController
    {
        
        private MMSContext db = new MMSContext();

        // GET: Marcas
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        private async Task<ActionResult> GetView(int id)
        {
            var marca = await db.Marca.FindAsync(id);
            if (marca == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), marca);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Details(int id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return PartialView("_Create");
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