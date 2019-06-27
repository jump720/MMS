using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using MMS.Classes;
using System.IO;

namespace MMS.Controllers.Catalogos
{
    public class ClientesController : BaseController
    {
        private MMSContext db = new MMSContext();

        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        private async Task<ActionResult> GetView(string ClienteID)
        {
            var cliente = db.Clientes.Include(c => c.ciudad.departamentos.paises).Where(c => c.ClienteID == ClienteID).FirstOrDefault();
            if (cliente == null)
                return HttpNotFound();
            ViewData["ColeccionPIVId"] = new SelectList(await db.ColeccionPIV.Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewData["VendedorId"] = new SelectList(await db.Usuarios.Select(u => new { u.UsuarioId, u.UsuarioNombre }).ToListAsync(), "UsuarioId", "UsuarioNombre");
            ViewData["CanalID"] = new SelectList(await db.Canales.Select(c => new { c.CanalID, c.CanalDesc }).ToListAsync(), "CanalID", "CanalDesc");
            ViewData["CiudadID"] = new SelectList(await db.Ciudad.Where(c => c.DepartamentoID == cliente.DepartamentoID).Select(c => new { c.CiudadID, c.CiudadDesc }).ToListAsync(), "CiudadID", "CiudadDesc");
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Where(d => d.PaisID == cliente.PaisID).Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");

            return PartialView(GetCrudMode().ToString(), cliente);
        }

        [AuthorizeAction]
        [FillPermission("Clientes/Edit")]
        public async Task<ActionResult> Details(string Id)
        {
            return await GetView(Id);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {
            ViewData["ColeccionPIVId"] = new SelectList(await db.ColeccionPIV.Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewData["VendedorId"] = new SelectList(await db.Usuarios.Select(u => new { u.UsuarioId, u.UsuarioNombre }).ToListAsync(), "UsuarioId", "UsuarioNombre");
            ViewData["CanalID"] = new SelectList(await db.Canales.Select(c => new { c.CanalID, c.CanalDesc }).ToListAsync(), "CanalID", "CanalDesc");
            ViewData["CiudadID"] = new SelectList(await db.Ciudad.Select(c => new { c.CiudadID, c.CiudadDesc }).ToListAsync(), "CiudadID", "CiudadDesc");
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Cliente model)
        {
            if (ModelState.IsValid)
            {
                var clienteTemp = db.Clientes.Where(c => c.ClienteID == model.ClienteID).FirstOrDefault();
                if (clienteTemp == null)
                {
                    db.Clientes.Add(model);
                    await db.SaveChangesAsync();
                    AddLog("", model.ClienteID, model);

                    return RedirectToAction("Index", GetReturnSearch());
                }
                else
                {
                    ModelState.AddModelError("", "Error, The Client already exists.");
                }
            }
            ViewData["ColeccionPIVId"] = new SelectList(await db.ColeccionPIV.Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewData["VendedorId"] = new SelectList(await db.Usuarios.Select(u => new { u.UsuarioId, u.UsuarioNombre }).ToListAsync(), "UsuarioId", "UsuarioNombre");
            ViewData["CanalID"] = new SelectList(await db.Canales.Select(c => new { c.CanalID, c.CanalDesc }).ToListAsync(), "CanalID", "CanalDesc");
            ViewData["CiudadID"] = new SelectList(await db.Ciudad.Select(c => new { c.CiudadID, c.CiudadDesc }).ToListAsync(), "CiudadID", "CiudadDesc");
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Edit(string Id)
        {
            return await GetView(Id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAction]
        public async Task<ActionResult> Edit(Cliente model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                await db.SaveChangesAsync();
                AddLog("", model.ClienteID, model);

                return RedirectToAction("Index", GetReturnSearch());
            }
            ViewData["ColeccionPIVId"] = new SelectList(await db.ColeccionPIV.Select(c => new { c.Id, c.Nombre }).ToListAsync(), "Id", "Nombre");
            ViewData["VendedorId"] = new SelectList(await db.Usuarios.Select(u => new { u.UsuarioId, u.UsuarioNombre }).ToListAsync(), "UsuarioId", "UsuarioNombre");
            ViewData["CanalID"] = new SelectList(await db.Canales.Select(c => new { c.CanalID, c.CanalDesc }).ToListAsync(), "CanalID", "CanalDesc");
            ViewData["CiudadID"] = new SelectList(await db.Ciudad.Where(c => c.DepartamentoID == model.DepartamentoID).Select(c => new { c.CiudadID, c.CiudadDesc }).ToListAsync(), "CiudadID", "CiudadDesc");
            ViewData["DepartamentoID"] = new SelectList(await db.Departamento.Where(d => d.PaisID == model.PaisID).Select(d => new { d.DepartamentoID, d.DepartamentoDesc }).ToListAsync(), "DepartamentoID", "DepartamentoDesc");
            ViewData["PaisID"] = new SelectList(await db.Pais.Select(p => new { p.PaisID, p.PaisDesc }).ToListAsync(), "PaisID", "PaisDesc");
            return View(model);
        }

        [AuthorizeAction]
        public async Task<ActionResult> Delete(string Id)
        {
            return await GetView(Id);
        }

        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string Id)
        {
            var cliente = await db.Clientes.FindAsync(Id);
            try
            {
                db.Clientes.Remove(cliente);
                await db.SaveChangesAsync();
                AddLog("", cliente.ClienteID, cliente);

                return RedirectToAction("Index", GetReturnSearch());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return await GetView(Id);
        }

        // Para enviar la lista resultante al AJAX
        public JsonResult GetDropDownInCascada(string id, string tipo, string idPais)
        {
            var result = new[] { new { id = "0", name = "Seleccione una Opción" } }.ToList();
            List<Departamento> departamentos = new List<Departamento>();
            List<Ciudad> ciudades = new List<Ciudad>();
            if (tipo == "PaisID")
            {
                if (id != null)
                {
                    departamentos = db.Departamento.Where(p => p.PaisID == id).ToList();
                }
                result = null;
                var resultTemp = (from r in departamentos
                                  select new
                                  {
                                      id = r.DepartamentoID,
                                      name = r.DepartamentoDesc
                                  }).ToList();
                result = resultTemp;
            }
            else if (tipo == "DepartamentoID")
            {
                if (id != null)
                {
                    ciudades = db.Ciudad.Where(p => p.DepartamentoID == id && p.PaisID == idPais).ToList();
                }
                result = null;
                var resultTemp = (from r in ciudades
                                  select new
                                  {
                                      id = r.CiudadID,
                                      name = r.CiudadDesc
                                  }).ToList();
                result = resultTemp;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Seguridad(isModal = true)]
        public ActionResult _Clientes()
        {
            List<Cliente> clientes = new List<Cliente>();
            try
            {
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                //para poder incluir la ciudad
                clientes = (from cs in seguridadcll.ClienteList
                            join cl in db.Clientes.Include(c => c.ciudad)
                            on cs.ClienteID equals cl.ClienteID
                            select cl).ToList();
            }
            catch
            {
                clientes = new List<Cliente>();
            }

            return PartialView(clientes);
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

//[Seguridad]
//// GET: Clientes
//public ActionResult Index()
//{
//    var clientes = db.Clientes
//                .Include(c => c.ciudad.departamentos.paises)
//                .Include(c => c.canal)
//                .Include(c => c.usuario);
//    return View(clientes.ToList());
//}

//// GET: Clientes/Details/5
//public ActionResult Details(string id)
//{
//    if (id == null)
//    {
//        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//    }
//    Cliente cliente = db.Clientes.Find(id);
//    if (cliente == null)
//    {
//        return HttpNotFound();
//    }
//    return View(cliente);
//}

//[Seguridad]
//// GET: Clientes/Create
//public ActionResult Create()
//{
//    List<Pais> lstPais = db.Pais.ToList();
//    List<Departamento> lstDepartamento = new List<Departamento>();
//    List<Ciudad> lstCiudad = new List<Ciudad>();
//    lstPais.Insert(0, new Pais { PaisID = "0", PaisDesc = "Seleccione un País" });
//    //lstDepartamento.Insert(0, new Departamento { DepartamentoID = "0", DepartamentoDesc = "Seleccione un Departamento" });
//    //lstCiudad.Insert(0, new Ciudad { CiudadID = "0", CiudadDesc = "Seleccione una Ciudad" });
//    ViewBag.PaisID = new SelectList(lstPais, "PaisID", "PaisDesc");
//    ViewBag.DepartamentoID = new SelectList(lstDepartamento, "DepartamentoID", "DepartamentoDesc");
//    ViewBag.CiudadID = new SelectList(lstCiudad, "CiudadID", "CiudadDesc");
//    ViewBag.VendedorId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
//    ViewBag.CanalID = new SelectList(db.Canales, "CanalID", "CanalDesc");
//    return View();
//}

//// POST: Clientes/Create
//// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[Seguridad]
//[ValidateAntiForgeryToken]
//public ActionResult Create(Cliente cliente)
//{
//    if (ModelState.IsValid)
//    {
//        try
//        {
//            var clienteTemp = db.Clientes.Where(u => u.ClienteID == cliente.ClienteID).FirstOrDefault();
//            if (clienteTemp == null)
//            {
//                db.Clientes.Add(cliente);
//                db.SaveChanges();

//                //Auditoria
//                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
//                Auditoria auditoria = new Auditoria();
//                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

//                auditoria.AuditoriaFecha = System.DateTime.Now;
//                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
//                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
//                auditoria.AuditoriaEvento = "Crear";
//                auditoria.AuditoriaDesc = "Crea Cliente: " + cliente.ClienteID;
//                auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

//                seguridad.insertAuditoria(auditoria);
//                //Auditoria

//                return RedirectToAction("Index");
//            }
//            else
//            {
//                ViewBag.error = "Advertencia, el Cliente " + cliente.ClienteID + " a crear ya existe.";
//            }
//        }
//        catch (Exception e)
//        {
//            ViewBag.error = e.ToString();
//        }
//    }
//    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc", cliente.PaisID);
//    ViewBag.DepartamentoID = new SelectList(db.Departamento, "DepartamentoID", "DepartamentoDesc", cliente.DepartamentoID);
//    ViewBag.CiudadID = new SelectList(db.Ciudad, "CiudadID", "CiudadDesc", cliente.CiudadID);
//    ViewBag.VendedorId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", cliente.VendedorId);
//    ViewBag.CanalID = new SelectList(db.Canales, "CanalID", "CanalDesc",cliente.CanalID);
//    return View(cliente);
//}

//[Seguridad]
//// GET: Clientes/Edit/5
//public ActionResult Edit(string id, string idF1, string idF2, string idF3, string idF4)
//{
//    if (id == null || idF1 == null || idF2 == null || idF3 == null || idF4 == null)
//    {
//        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//    }
//    Cliente cliente = db.Clientes
//                        .Include(c => c.ciudad.departamentos.paises)
//                        .Include(c => c.usuario)
//                        .Where(u => u.ClienteID == id)//&& u.CiudadID == idF1 && u.DepartamentoID == idF2 && u.PaisID == idF3 && u.VendedorId == idF4
//                        .FirstOrDefault();

//    List<Departamento> departamentos = new List<Departamento>();
//    if (idF2 != null)
//    {
//        departamentos = db.Departamento.Where(p => p.PaisID == idF3).ToList();
//    }
//    List<Ciudad> ciudades = new List<Ciudad>();
//    if (idF1 != null)
//    {
//        ciudades = db.Ciudad.Where(p => p.DepartamentoID == idF2 && p.PaisID == idF3).ToList();
//    }
//    if (cliente == null)
//    {
//        //return HttpNotFound();    
//        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
//    }
//    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc", cliente.PaisID);
//    ViewBag.DepartamentoID = new SelectList(departamentos, "DepartamentoID", "DepartamentoDesc", cliente.DepartamentoID);
//    ViewBag.CiudadID = new SelectList(ciudades, "CiudadID", "CiudadDesc", cliente.CiudadID);
//    ViewBag.VendedorId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", cliente.VendedorId);
//    ViewBag.CanalID = new SelectList(db.Canales, "CanalID", "CanalDesc", cliente.CanalID);
//    return View(cliente);
//}

//// POST: Clientes/Edit/5
//// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
//[HttpPost]
//[Seguridad]
//[ValidateAntiForgeryToken]
////[Bind(Include = "ClienteID,ClienteNit,ClienteRazonSocial,PaisID,DepartamentoID,CiudadID,VendedorId")]
//public ActionResult Edit( Cliente cliente)
//{
//    if (ModelState.IsValid)
//    {
//        db.Entry(cliente).State = EntityState.Modified;
//        db.SaveChanges();

//        //Auditoria
//        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
//        Auditoria auditoria = new Auditoria();
//        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

//        auditoria.AuditoriaFecha = System.DateTime.Now;
//        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
//        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
//        auditoria.AuditoriaEvento = "Modificar";
//        auditoria.AuditoriaDesc = "Modificó Cliente: " + cliente.ClienteID;
//        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

//        seguridad.insertAuditoria(auditoria);
//        //Auditoria

//        return RedirectToAction("Index");
//    }
//    ViewBag.PaisID = new SelectList(db.Pais, "PaisID", "PaisDesc", cliente.PaisID);
//    ViewBag.DepartamentoID = new SelectList(db.Departamento, "DepartamentoID", "DepartamentoDesc", cliente.DepartamentoID);
//    ViewBag.CiudadID = new SelectList(db.Ciudad, "CiudadID", "CiudadDesc", cliente.CiudadID);
//    ViewBag.VendedorId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", cliente.VendedorId);
//    ViewBag.CanalID = new SelectList(db.Canales, "CanalID", "CanalDesc", cliente.CanalID);
//    return View(cliente);
//}

//[Seguridad(isModal = true)]
//// GET: Clientes/Delete/5
//public ActionResult Delete(string[] ids)
//{
//    //POSISIÓN [0] Es el ID de la Ciudad, [1] el del País y [2] del Departamento.
//    string idCliente = ids[0];
//    string idCiudad = ids[1];
//    string idDep = ids[2];
//    string idPais = ids[3];
//    string idUsuario = ids[4];
//    if (idCliente == null || idCiudad == null || idDep == null || idPais == null || idUsuario == null)
//    {
//        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//    }
//    Cliente clientes = db.Clientes.Where(u => u.ClienteID == idCliente && u.CiudadID == idCiudad && u.DepartamentoID == idDep && u.PaisID == idPais && u.VendedorId == idUsuario).FirstOrDefault();
//    if (clientes == null)
//    {
//        ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idCliente;
//    }
//    return View(clientes);
//}

//// POST: Clientes/Delete/5
//[HttpPost, ActionName("Delete")]
//[Seguridad]
//[ValidateAntiForgeryToken]
//public ActionResult DeleteConfirmed(string[] ids)
//{
//    //POSISIÓN [0] Es el ID de la Ciudad, [1] el del País y [2] del Departamento.
//    string idCliente = ids[0];
//    string idCiudad = ids[1];
//    string idDep = ids[2];
//    string idPais = ids[3];
//    string idUsuario = ids[4];
//    try
//    {
//        Cliente cliente = db.Clientes.Where(u => u.ClienteID == idCliente && u.CiudadID == idCiudad && u.DepartamentoID == idDep && u.PaisID == idPais && u.VendedorId == idUsuario).FirstOrDefault();
//        db.Clientes.Remove(cliente);
//        db.SaveChanges();

//        //Auditoria
//        Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
//        Auditoria auditoria = new Auditoria();
//        Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

//        auditoria.AuditoriaFecha = System.DateTime.Now;
//        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
//        auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
//        auditoria.AuditoriaEvento = "Eliminar";
//        auditoria.AuditoriaDesc = "Eliminó Cliente: " + cliente.ClienteID;
//        auditoria.ObjetoId = RouteData.Values["controller"].ToString() + "/" + RouteData.Values["action"].ToString();

//        seguridad.insertAuditoria(auditoria);
//        //Auditoria             
//    }
//    catch (Exception e)
//    {
//        var clienteTemp = db.Clientes.Where(u => u.ClienteID == idCliente && u.CiudadID == idCiudad && u.DepartamentoID == idDep && u.PaisID == idPais && u.VendedorId == idUsuario).FirstOrDefault();
//        if (clienteTemp == null)
//        {
//            ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + idCliente;
//        }
//        else
//        {
//            ViewBag.Error = e.ToString();
//        }
//    }
//    return RedirectToAction("Index");
//}

//protected override void Dispose(bool disposing)
//{
//    if (disposing)
//    {
//        db.Dispose();
//    }
//    base.Dispose(disposing);
//}