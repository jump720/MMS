using System.Threading.Tasks;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;

namespace MMS.Controllers.PIV
{
    public class PlantaController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string id)
        {
            var planta = await db.Plantas.FindAsync(id);
            if (planta == null)
                return HttpNotFound();

            return PartialView("_" + GetCrudMode().ToString(), planta);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Details(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {
            return await GetView(id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string id)
        {
            return await GetView(id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}