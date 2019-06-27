using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using System.Threading.Tasks;

namespace MMS.Controllers.Seguridad
{
    public class CorreosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Correos
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }


        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            return PartialView("_Create");
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(int Id = 0)
        {

            return await GetView(Id);
        }


        [AuthorizeAction]
        public async Task<ActionResult> Delete(int Id = 0)
        {

            return await GetView(Id);
        }


        private async Task<ActionResult> GetView(int Id)
        {
            var correo = await db.Correos.Where(c => c.Id== Id ).FirstOrDefaultAsync();
            if (correo == null)
                return HttpNotFound();         

            return PartialView("_" + GetCrudMode().ToString(), correo);
        }


        //[AuthorizeAction]
        //// GET: PresupuestoVendedor/Edit
        //public async Task<ActionResult> Edit(int Ano, int Mes, string ClienteId, string CentroCostoId)
        //{
        //    return await GetView(Ano, Mes, ClienteId, CentroCostoId);
        //}
    }
}