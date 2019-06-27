using MMS.Classes;
using MMS.Filters;
using MMS.Models;
using System;//ok
using System.Collections.Generic;//ok
using System.Data;//ok
using System.Data.Entity;//ok
using System.Linq;//ok
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MMS.Controllers.Seguridad
{
    public class UsuariosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Usuarios
        [AuthorizeAction]
        [FillPermission("Usuarios/_ResetPassword")]
        public ActionResult Index()
        {
            //var usuarios = db.Usuarios.Include(u => u.UsuarioPadre);
            return View();
        }


        private async Task<ActionResult> GetView(string id)
        {
            var usuario = await db.Usuarios.FindAsync(id);
            var usuarioHV = await db.UsuarioHV.FindAsync(id);

            if (usuario == null)
                return HttpNotFound();

            var roles = await (from a in db.Roles
                               join ao in db.RolUsuario.Where(a => a.UsuarioId == id) on a equals ao.Rol into oaos
                               from aoa in oaos.DefaultIfEmpty()
                               select new UsuariosViewModel.RolesViewModel
                               {
                                   RolId = a.RolId,
                                   RolNombre = a.RolNombre,
                                   Seleccionado = (aoa.Usuario == null) ? false : true
                               }).ToListAsync();

            var plantas = await (from p in db.Plantas
                                 join up in db.UsuarioPlanta.Where(up => up.UsuarioId == id) on p equals up.Planta into upps
                                 from upp in upps.DefaultIfEmpty()
                                 select new UsuariosViewModel.PlantasViewModel
                                 {
                                     PlantaId = p.PlantaID,
                                     PlantaNombre = p.PlantaDesc,
                                     Seleccionado = (upp.Usuario == null) ? false : true
                                 }).ToListAsync();
            var canales = await (from c in db.Canales
                                 join uc in db.UsuarioCanal.Where(uc => uc.UsuarioId == id) on c equals uc.Canal into uccs
                                 from ucc in uccs.DefaultIfEmpty()
                                 select new UsuariosViewModel.CanalesViewModel
                                 {
                                     CanalId = c.CanalID,
                                     CanalNombre = c.CanalDesc,
                                     Seleccionado = (ucc.Usuario == null) ? false : true
                                 }).ToListAsync();



            ViewBag.Usuario_UsuarioPadreId = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", usuario.UsuarioPadreId);
            //ViewData["Usuario.UsuarioPadreId"] = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", usuario.UsuarioPadreId);

            if (usuarioHV != null)
                ViewBag.UsuarioHV_AreaId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre", usuarioHV.AreaId);
            else
                ViewBag.UsuarioHV_AreaId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre");

            return View(GetCrudMode().ToString(), new UsuariosViewModel
            {
                Usuario = usuario,
                UsuarioHV = (usuarioHV != null) ? usuarioHV : new UsuarioHV(),
                Roles = roles,
                Plantas = plantas,
                Canales = canales
            });
        }

        // GET: Usuarios/Details/5
        [AuthorizeAction]
        [FillPermission("Usuarios/Edit")]
        public async Task<ActionResult> Details(string id)
        {



            return await GetView(id);
        }

        // GET: Usuarios/Create
        [AuthorizeAction]
        public async Task<ActionResult> Create()
        {


            /*ViewBags*/
            ViewBag.Usuario_UsuarioPadreId = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre");
            ViewBag.UsuarioHV_AreaId = new SelectList(await db.Area.ToListAsync(), "Id", "Nombre");
            //ViewBag.RolesList = db.Roles.ToList();
            //ViewBag.RolUsuarioList = new List<RolUsuario>();

            var roles = await db.Roles
                                .Select(r => new UsuariosViewModel.RolesViewModel
                                {
                                    RolId = r.RolId,
                                    RolNombre = r.RolNombre,
                                    Seleccionado = false
                                }).ToListAsync();

            var plantas = await db.Plantas
                              .Select(r => new UsuariosViewModel.PlantasViewModel
                              {
                                  PlantaId = r.PlantaID,
                                  PlantaNombre = r.PlantaDesc,
                                  Seleccionado = false
                              }).ToListAsync();

            var canales = await db.Canales
                             .Select(r => new UsuariosViewModel.CanalesViewModel
                             {
                                 CanalId = r.CanalID,
                                 CanalNombre = r.CanalDesc,
                                 Seleccionado = false
                             }).ToListAsync();


            return View(new UsuariosViewModel { Roles = roles, Plantas = plantas, Canales = canales });
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UsuariosViewModel model)
        {


            if (ModelState.IsValid)
            {
                try
                {
                    var usuarioTemp = db.Usuarios.Where(u => u.UsuarioId == model.Usuario.UsuarioId).FirstOrDefault();
                    if (usuarioTemp == null)
                    {
                        Seguridad seguridad = new Seguridad();
                        model.Usuario.Usuariopassword = Fn.EncryptText(model.Usuario.Usuariopassword);
                        db.Usuarios.Add(model.Usuario);
                        await db.SaveChangesAsync();

                        //Crea registro en HV
                        model.UsuarioHV.UsuarioId = model.Usuario.UsuarioId;
                        db.UsuarioHV.Add(model.UsuarioHV);
                        await db.SaveChangesAsync();

                        // guardaRolUsuario(RolUsuarioList, usuario.UsuarioId);
                        //roles(rolUsuario)
                        foreach (var rol in model.Roles)
                        {
                            if (rol.Seleccionado)
                            {
                                RolUsuario ru = new RolUsuario();
                                ru.RolId = rol.RolId;
                                ru.UsuarioId = model.Usuario.UsuarioId;
                                db.RolUsuario.Add(ru);
                            }
                        }

                        foreach (var p in model.Plantas)
                        {
                            if (p.Seleccionado)
                            {
                                UsuarioPlanta up = new UsuarioPlanta();
                                up.PlantaId = p.PlantaId;
                                up.UsuarioId = model.Usuario.UsuarioId;
                                db.UsuarioPlanta.Add(up);
                            }
                        }


                        foreach (var c in model.Canales)
                        {
                            if (c.Seleccionado)
                            {
                                UsuarioCanal uc = new UsuarioCanal();
                                uc.CanalId = c.CanalId;
                                uc.UsuarioId = model.Usuario.UsuarioId;
                                db.UsuarioCanal.Add(uc);
                            }
                        }


                        if (!ModelState.Values.Any(ms => ms.Errors.Count > 0))
                        {
                            await db.SaveChangesAsync();
                            AddLog("", model.Usuario.UsuarioId, model);
                            return RedirectToAction("Index", GetReturnSearch());
                        }

                        //return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Warning, This User " + model.Usuario.UsuarioId + " already exists");

                    }
                }
                catch (Exception e)
                {
                    ViewBag.error = e.ToString();
                }


            }

            /*ViewBags*/
            ViewBag.Usuario_UsuarioPadreId = new SelectList(await db.Usuarios.ToListAsync(), "UsuarioId", "UsuarioNombre", model.Usuario.UsuarioPadreId);

            return View(new UsuariosViewModel { Usuario = model.Usuario, Roles = model.Roles });
        }

        // GET: Usuarios/Edit/5
        [AuthorizeAction]
        public async Task<ActionResult> Edit(string id)
        {

            return await GetView(id);
        }

        private void SendNotificationEmailTask(string to, string toName, EstadoPeak estado, int peakId, string peakUserName)
        {
            if (string.IsNullOrWhiteSpace(to))
                return;

            string subject = "AIS - A Peak Performance has been assigned to you.";
            string msg = $"The Peak Performance of the user  <b>{peakUserName}</b> is now under your supervision.";

            msg += $"<br /><br /><b>Peak Performance Status</b>: {estado.ToString().Replace("_", " ")}<br /><br />";
            msg += $"<a style='color:#22BCE5' href={{url}}/Peak/Manage/{peakId}>Click here to view the Peak.</a>";

            Task.Run(() => Fn.SendHtmlEmail(to, toName, subject, msg, Seguridadcll.Aplicacion.Link));
        }

        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        //Bind(Include = "UsuarioId,UsuarioNombre,UsuarioCorreo,Usuariopassword,Usuarioactivo,UsuarioPadreId")] 
        public async Task<ActionResult> Edit(UsuariosViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(model.Usuario).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    //Registro de HV

                    if (model.UsuarioHV.UsuarioId == null || model.UsuarioHV.UsuarioId == "")
                    {
                        //Crea registro en HV
                        model.UsuarioHV.UsuarioId = model.Usuario.UsuarioId;
                        db.UsuarioHV.Add(model.UsuarioHV);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        db.Entry(model.UsuarioHV).State = EntityState.Modified;

                        var openPeaks = await db.Peak
                            .Include(p => p.Periodo)
                            .Where(p => p.UsuarioId == model.Usuario.UsuarioId && p.Estado != EstadoPeak.Finished)
                            .ToListAsync();

                        foreach (var peak in openPeaks)
                        {
                            peak.Cargo = model.UsuarioHV.Cargo;
                            peak.AreaId = (int)model.UsuarioHV.AreaId;

                            if (peak.UsuarioIdPadre != model.Usuario.UsuarioPadreId)
                            {
                                var newUsuarioPadre = await db.Usuarios
                                    .Where(u => u.UsuarioId == model.Usuario.UsuarioPadreId).Select(u => new
                                    {
                                        u.UsuarioCorreo,
                                        u.UsuarioNombre
                                    }).FirstOrDefaultAsync();

                                SendNotificationEmailTask(newUsuarioPadre.UsuarioCorreo, newUsuarioPadre.UsuarioNombre, peak.Estado, peak.Id, model.Usuario.UsuarioNombre);
                            }

                            peak.UsuarioIdPadre = model.Usuario.UsuarioPadreId;
                            db.Entry(peak).State = EntityState.Modified;
                        }

                        await db.SaveChangesAsync();

                        if (await DeleteRolUsuario(model.Usuario.UsuarioId))
                        {
                            foreach (var rol in model.Roles)
                            {
                                if (rol.Seleccionado)
                                {
                                    RolUsuario ru = new RolUsuario();
                                    ru.RolId = rol.RolId;
                                    ru.UsuarioId = model.Usuario.UsuarioId;
                                    db.RolUsuario.Add(ru);
                                }
                            }

                            //Plantas
                            var currentPlantas = await db.UsuarioPlanta
                                                       .Where(ul => ul.UsuarioId == model.Usuario.UsuarioId)
                                                       .ToListAsync();

                            if (model.Plantas != null)
                            {
                                var PlantasId = model.Plantas.Where(l => l.Seleccionado == true).Select(a => a.PlantaId).ToArray();


                                var itemsToDelete = currentPlantas.Where(a => !PlantasId.Contains(a.PlantaId)).ToList();

                                if (itemsToDelete.Count > 0)
                                {
                                    db.UsuarioPlanta.RemoveRange(itemsToDelete);
                                    await db.SaveChangesAsync();
                                }

                                //Insertar nuevos
                                foreach (var planta in model.Plantas)
                                {
                                    if (planta.Seleccionado && currentPlantas.Where(cl => cl.PlantaId == planta.PlantaId).FirstOrDefault() == null)
                                    {
                                        var up = new UsuarioPlanta();
                                        up.PlantaId = planta.PlantaId;
                                        up.UsuarioId = model.Usuario.UsuarioId;
                                        db.UsuarioPlanta.Add(up);
                                    }
                                }
                            }
                            else
                            {
                                var itemsToDelete = currentPlantas.ToList();

                                if (itemsToDelete.Count > 0)
                                {
                                    db.UsuarioPlanta.RemoveRange(itemsToDelete);
                                    await db.SaveChangesAsync();
                                }
                            }

                            //Canales
                            var currentCanales = await db.UsuarioCanal
                                                       .Where(ul => ul.UsuarioId == model.Usuario.UsuarioId)
                                                       .ToListAsync();

                            if (model.Canales != null)
                            {
                                var CanalesId = model.Canales.Where(l => l.Seleccionado == true).Select(a => a.CanalId).ToArray();


                                var itemsToDelete = currentCanales.Where(a => !CanalesId.Contains(a.CanalId)).ToList();

                                if (itemsToDelete.Count > 0)
                                {
                                    db.UsuarioCanal.RemoveRange(itemsToDelete);
                                    await db.SaveChangesAsync();
                                }

                                //Insertar nuevos
                                foreach (var canal in model.Canales)
                                {
                                    if (canal.Seleccionado && currentCanales.Where(cl => cl.CanalId == canal.CanalId).FirstOrDefault() == null)
                                    {
                                        var uc = new UsuarioCanal();
                                        uc.CanalId = canal.CanalId;
                                        uc.UsuarioId = model.Usuario.UsuarioId;
                                        db.UsuarioCanal.Add(uc);
                                    }
                                }
                            }
                            else
                            {
                                var itemsToDelete = currentCanales.ToList();

                                if (itemsToDelete.Count > 0)
                                {
                                    db.UsuarioCanal.RemoveRange(itemsToDelete);
                                    await db.SaveChangesAsync();
                                }
                            }

                            await db.SaveChangesAsync();
                            AddLog("", model.Usuario.UsuarioId, model);
                            return RedirectToAction("Index", GetReturnSearch());
                        }
                        else
                        {
                            ModelState.AddModelError("", "Error Deleting Detail (RolUsuario)");
                        }
                        //guardaRolUsuario(RolUsuarioList, model.Usuario.UsuarioId);

                        //Seguridad seguridad = new Seguridad();
                        //Auditoria auditoria = new Auditoria();
                        //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                        //auditoria.AuditoriaFecha = System.DateTime.Now;
                        //auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                        //auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                        //auditoria.AuditoriaEvento = "Modificar";
                        //auditoria.AuditoriaDesc = "Modifico Usuario: " + model.Usuario.UsuarioId;
                        //auditoria.ObjetoId = "Usuario/Edit";

                        //seguridad.insertAuditoria(auditoria);

                        //return RedirectToAction("Index");
                    }
                }
                catch (Exception e)
                {
                    ViewBag.error = e.ToString();
                }

            }

            //ViewBag.UsuarioPadreId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", usuario.UsuarioPadreId);
            //ViewBag.RolesList = db.Roles.ToList();
            return await GetView(model.Usuario.UsuarioId);
        }

        // GET: Usuarios/Delete/5
        [Seguridad]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            ViewBag.UsuarioPadreId = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", usuario.UsuarioPadreId);
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Usuario usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl = null)
        {

            //string host = HttpContext.Request.Url.Host;
            //string appName = host.Substring(0, host.IndexOf('.'));

            ViewBag.ReturnUrl = ReturnUrl;
            try
            {
                Session.Remove("seguridad");
                FormsAuthentication.SignOut();
            }
            catch (Exception e)
            {
                Errores l_err = new Errores();
                l_err.titulo = "Advertencia";
                l_err.mensaje = e.ToString();
                l_err.objeto = "Login";
                return View(@"~/Views/Home/Home.cshtml", l_err);
                //return View(@"~/Views/Home/Error404.cshtml", l_err);

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(Login model, string ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                try
                {
                    Seguridad seguridad = new Seguridad();
                    string passwordEncrypt = Fn.EncryptText(model.Password);
                    var usuario = db.Usuarios
                                    .Where(u => u.UsuarioId.ToLower() == model.Usuario.ToLower()
                                                && u.Usuariopassword == passwordEncrypt && u.Usuarioactivo == true)
                                    .FirstOrDefault();

                    if (usuario != null)
                    {
                        CargueSeguridad(usuario);

                        var cookie = FormsAuthentication.GetAuthCookie(usuario.UsuarioId, false);
                        cookie.HttpOnly = true;
                        cookie.Expires = DateTime.Now.AddYears(1);
                        Response.AppendCookie(cookie);

                        // fin autenticacion


                        @ViewBag.Error = "Login Success";

                        /*Genera Auditoria*/
                        Auditoria auditoria = new Auditoria();

                        auditoria.AuditoriaFecha = System.DateTime.Now;
                        auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                        auditoria.usuarioId = model.Usuario;
                        auditoria.AuditoriaEvento = "Login";
                        auditoria.AuditoriaDesc = "Ingreso al sistema Correcto: " + model.Usuario;
                        auditoria.ObjetoId = "Usuario/Login";

                        seguridad.insertAuditoria(auditoria);


                        if (ViewBag.ReturnUrl != null)
                        {
                            return Redirect(ViewBag.ReturnUrl);
                        }

                        else
                        {
                            return RedirectToAction("Index", "Test");
                        }
                    }
                    else
                    {
                        @ViewBag.Error = "Usuario/Contraseña Invalida o Usuario no activo";
                    }
                }
                catch (Exception e)
                {
                    @ViewBag.Error = "Error al ingresar, por favor comunicarse con el administrador " + e.ToString();
                }

            }

            return View(model);
        }

        public Seguridadcll CargueSeguridad(Usuario usuario, HttpContext context = null, int App = 1)
        {
            db.Configuration.ProxyCreationEnabled = false;

            /*Obtiene roles*/
            var rolesUsuario = db.RolUsuario.Where(r => r.UsuarioId == usuario.UsuarioId).ToList();
            var rolesUsuarioId = rolesUsuario.Select(ru => ru.RolId).ToArray();

            /*Aplicaciones asignadas al rol*/
            var aplicaciones = db.RolAplicaciones
                .Include(ra => ra.Aplicacion)
                .Where(ra => ra.Aplicacion.Activo && rolesUsuarioId.Contains(ra.RolId))
                .Select(ra => ra.Aplicacion).Distinct().ToList();

            /*Obtiene RolObjeto*/
            var rolesObjetos = db.RolObjeto.Where(ro => rolesUsuarioId.Contains(ro.RolId) && ro.RolObjetoActivo == true).ToList();

            /*Obtiene subordinados*/
            var usuarios = db.Usuarios.Where(r => r.UsuarioPadreId == usuario.UsuarioId).ToList();

            var rolObj = rolesObjetos.Where(ro => ro.ObjetoId == "Clientes/All").FirstOrDefault();

            /*Obtiene clientes*/
            List<Cliente> clienteList = new List<Cliente>();

            if (rolObj == null)
            {
                //Plantas
                var plantas = db.UsuarioPlanta
                                .Where(ul => ul.UsuarioId == usuario.UsuarioId)
                                .Select(a => a.PlantaId).ToArray();
                //Canales
                var canales = db.UsuarioCanal
                                .Where(ul => ul.UsuarioId == usuario.UsuarioId)
                                .Select(a => a.CanalId).ToArray();

                
                if (plantas.Length == 0)
                {
                    //Nivel 1
                    db.Clientes.Where(c => c.VendedorId == usuario.UsuarioId)
                            .ToList().ForEach(c => clienteList.Add(c)); 
                }
                else
                {

                    if (canales.Length > 0)
                    {
                        //Nivel 2
                        db.Clientes.Where(c => plantas.Contains(c.PlantaID) && canales.Contains(c.CanalID))
                            .ToList().ForEach(c => clienteList.Add(c));
                    }
                    else
                    {
                        //Nivel 3
                        db.Clientes.Where(c => plantas.Contains(c.PlantaID))
                          .ToList().ForEach(c => clienteList.Add(c));
                    }

                }


                //obtiene sus clientes
                //db.Clientes
                //    .Where(c => c.PlantaID == usuario.PlantaID)
                //    .ToList()
                //    .ForEach(c => clienteList.Add(c));

                //agregamos los clientes de sus subordinados
                //foreach (var usu in usuarios)
                //{
                //    db.Clientes
                //        .Where(c => c.PlantaID == usu.PlantaID)
                //        .ToList()
                //        .ForEach(c => clienteList.Add(c));
                //}

                //clienteList = (from cl in clienteList
                //               join p in db.PresupuestoVendedor on cl.ClienteID equals p.ClienteID
                //               select cl).Distinct().ToList();
            }
            else
            {
                //Nivel 4
                clienteList = db.Clientes.ToList();
                //clienteList = (from cl in clienteList
                //               join p in db.PresupuestoVendedor on cl.ClienteID equals p.ClienteID
                //               select cl).Distinct().ToList();
            }

            //obtiene Objetos del Menu
            var objetosId = rolesObjetos.Select(ro => ro.ObjetoId).Distinct().ToArray();
            var objetosMenu = db.Objeto
                .Include(o => o.AplicacionObjetos)
                .Where(o => o.ObjetoMenu == true && objetosId.Contains(o.ObjetoId)).ToList();

            var objetosMenuDirectorio = db.Objeto
                .Include(o => o.AplicacionObjetos)
                .Where(o => o.ObjetoId.StartsWith("__") && o.ObjetoMenu).ToList();

            /*Llena variable para generar session */
            var seguridadcll = new Seguridadcll()
            {
                Usuario = usuario,
                RolUsuarioList = rolesUsuario,
                RolObjetoList = rolesObjetos,
                UsuariosHijos = usuarios,
                ClienteList = clienteList,
                ObjetosMenuList = objetosMenu,
                ObjetosMenuDirectorioList = objetosMenuDirectorio,
                Configuracion = db.Configuracion.FirstOrDefault(),
                Aplicaciones = aplicaciones
            };

            if (context == null)
                Session["seguridad"] = seguridadcll;
            else
                context.Session["seguridad"] = seguridadcll;

            return seguridadcll;
        }

        /*Metodo que refresca la seguridad Session[seguridad]*/

        public JsonResult actualizaSeguridad()
        {
            try
            {
                Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                if (seguridadcll != null)
                    CargueSeguridad(seguridadcll.Usuario);
                else
                    return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [AuthorizeAction]
        public async Task<ActionResult> _ResetPassword(string id)
        {
            //ResetPassword reset = new ResetPassword();
            //reset.Usuario = id;
            //reset.UsuarioNombre = "";
            var data = await db.Usuarios
                                .Where(u => u.UsuarioId == id)
                                .Select(u => new ResetPassword { Usuario = u.UsuarioId, UsuarioNombre = u.UsuarioNombre, Password1 = "", Password2 = "" })
                                .FirstOrDefaultAsync();



            return PartialView("_ResetPassword", data);
        }


        //genera la lista con lo roles asignados al usuario
        private List<RolUsuario> addRolUsuario(FormCollection form, string UsuarioId)
        {
            List<RolUsuario> RolUsuarioList = new List<RolUsuario>();

            for (int i = 1; i < form.Count; i++)
            {

                int RolId;
                bool flagrol = int.TryParse(form.GetKey(i), out RolId);
                if (flagrol)
                {
                    RolUsuario rolusuario = new RolUsuario();
                    rolusuario.UsuarioId = UsuarioId;
                    rolusuario.RolId = int.Parse(form.GetKey(i).ToString());
                    RolUsuarioList.Add(rolusuario);
                }
            }

            return RolUsuarioList;
        }



        //Elimina y guarda los roles asignados al usuario
        private void guardaRolUsuario(List<RolUsuario> RolUsuarioList, string UsuarioId)
        {
            db.RolUsuario
                .Where(r => r.UsuarioId == UsuarioId).ToList()
                .ForEach(r => db.RolUsuario.Remove(r));
            db.SaveChanges();

            foreach (var rol in RolUsuarioList)
            {
                db.RolUsuario.Add(rol);
            }
            db.SaveChanges();
        }


        [AuthorizeAction]
        public ActionResult SendMailTest()
        {
            return View();
        }

        [HttpPost]
        [AuthorizeAction]
        public ActionResult SendMailTest(SendMailTest model)
        {
            if (ModelState.IsValid)
            {

                Task.Run(() => Fn.SendHtmlEmail(model.To, model.ToName, model.Subject, model.Message, Seguridadcll.Aplicacion.Link));

                //string body = string.Empty;
                //using (var sr = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/Email_Base.html"))
                //    body = sr.ReadToEnd();


                //body = body.Replace("{username}", model.ToName);
                //body = body.Replace("{message}", model.Message);

                //using (var mail = new MailMessage())
                //{
                //    using (var db = new MMSContext())
                //    {
                //       // var config = db.Configuracion.First();


                //        mail.From = new MailAddress("cali-comunicacion@apextoolgroup.com");
                //        mail.To.Add(new MailAddress(model.To));
                //        mail.Subject = model.Subject;
                //        mail.Body = body;
                //        mail.IsBodyHtml = true;

                //        var smtp = new SmtpClient()
                //        {
                //            Host = "smtp.atg.root",
                //            UseDefaultCredentials = false,
                //            //EnableSsl = true,
                //            //Credentials = new NetworkCredential("cali-comunicacion@apextoolgroup.com", "Recursos2"),
                //            Port = 25,
                //        };
                //        smtp.Send(mail);
                //    }
                //}
            }
            return View();
        }


        [AllowAnonymous]
        public ActionResult RememberUser()
        {

            return PartialView("_RememberUser");
        }

        [AuthorizeAction]
        public async Task<ActionResult> ChangePassword(string id)
        {
            var data = await db.Usuarios
                                .Where(u => u.UsuarioId == id)
                                .Select(u => new ChangePassword { Usuario = u.UsuarioId, PasswordOld = "", UsuarioNombre = u.UsuarioNombre, Password1 = "", Password2 = "" })
                                .FirstOrDefaultAsync();

            return PartialView("_ChangePassword", data);
        }

        private async Task<bool> DeleteRolUsuario(string id)
        {
            bool result = true;
            try
            {
                var ru = await db.RolUsuario.Where(a => a.UsuarioId == id).ToListAsync();
                if (ru.Count > 0)
                {
                    db.RolUsuario.RemoveRange(ru);
                    await db.SaveChangesAsync();
                }
            }
            catch
            {
                result = false;
            }

            return result;
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
