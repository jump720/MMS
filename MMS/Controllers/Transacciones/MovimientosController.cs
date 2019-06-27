using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MMS.Models;
using MMS.Filters;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using System.Web.Routing;

namespace MMS.Controllers.Transacciones
{
    public class MovimientosController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Movimientos
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            return View();
        }

        // GET: Movimientos/Details/5
        [AuthorizeAction]
        public ActionResult Details(int? MovimientoId = 0)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Movimiento> MovimientosList;

            if (MovimientoId == null || MovimientoId == 0)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + MovimientoId;
                MovimientosList = null;
            }
            else
            {

                MovimientosList = db.Movimiento
                                    .Where(m => m.MovimientoId == MovimientoId)
                                    .ToList();
                if (MovimientosList.Count == 0)
                {
                    ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + MovimientoId;
                    MovimientosList = null;
                }
                else
                {
                    string ConfigTipoProdInv = db.Configuracion.FirstOrDefault().ConfigTipoProdInv;


                    var TipoMovConfig = db.Configuracion
                                         .Select(c => new { c.ConfigTipoMovEntrada, c.ConfigTipoMovSalida, c.ConfigTipoMovAjEntrada, c.ConfigTipoMovAjSalida })
                                         .FirstOrDefault();

                    ViewBag.TipoMovConfig = TipoMovConfig;

                    var main = MovimientosList.FirstOrDefault();

                    ViewBag.ProductoId = new SelectList(db.Producto.Where(p => p.TipoProductoID == ConfigTipoProdInv), "ProductoId", "ProductoDesc", main.ProductoId);

                    string TipoMovimientoID = main.TipoMovimientoID;
                    ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos
                                                                .Where(t => t.TipoMovimientoID == TipoMovimientoID)
                                                                .OrderByDescending(t => t.TipoMovimientoID), "TipoMovimientoID", "TipoMovimientoDesc", main.TipoMovimientoID);

                    EstadoMovimiento[] estados = (EstadoMovimiento[])Enum.GetValues(typeof(EstadoMovimiento));
                    var MovimientoEstado = from value in estados
                                           select new { value = (int)value, name = (value.ToString().Equals("Sin_Aplicar")) ? "Sin Aplicar" : value.ToString() };
                    ViewBag.MovimientoEstado = new SelectList(MovimientoEstado, "value", "name");

                    ViewBag.UsuarioIdModifica = Seguridadcll.Usuario.UsuarioId;
                }

            }
            return View(MovimientosList);
        }

        // GET: Movimientos/Create
        [AuthorizeAction]
        public ActionResult Create(int MovimientoId = 0)
        {

            var TipoMovConfig = db.Configuracion
                                 .Select(c => new { c.ConfigTipoMovEntrada, c.ConfigTipoMovSalida, c.ConfigTipoMovAjEntrada, c.ConfigTipoMovAjSalida })
                                 .FirstOrDefault();

            ViewBag.TipoMovConfig = TipoMovConfig;

            string ConfigTipoProdInv = db.Configuracion.FirstOrDefault().ConfigTipoProdInv;
            //string ConfigTipoMovSalida = db.Configuracion.FirstOrDefault().ConfigTipoMovSalida;

            //ViewBag.OrdenId = new SelectList(db.Orden, "OrdenId", "OrdenComentario");
            //ViewBag.ProductoId = new SelectList(db.Producto.Where(p => p.TipoProductoID == ConfigTipoProdInv), "ProductoId", "ProductoDesc");
            ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos.Where(t => t.TipoMovimientoID != TipoMovConfig.ConfigTipoMovSalida).OrderByDescending(t => t.TipoMovimientoID), "TipoMovimientoID", "TipoMovimientoDesc");

            EstadoMovimiento[] estados = (EstadoMovimiento[])Enum.GetValues(typeof(EstadoMovimiento));
            var MovimientoEstado = from value in estados
                                       //where (int)value == 1 || (int)value == 2
                                   select new { value = (int)value, name = (value.ToString().Equals("Sin_Aplicar")) ? "Sin Aplicar" : value.ToString() };
            ViewBag.MovimientoEstado = new SelectList(MovimientoEstado, "value", "name");


            ViewBag.UsuarioIdModifica = Seguridadcll.Usuario.UsuarioId;

            return View();
        }

        // POST: Movimientos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Movimiento movimiento, FormCollection form)
        {
            Dictionary<string, dynamic> lo_respuesta = new Dictionary<string, dynamic>();
            try
            {
                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                //Obtiene numero de Movimiento(ultimo de la tabla + 1)
                int MovimientoId = seguridad.generaConsecutivo("Movimientos");

                List<Movimiento> MovimientoListUPD = new List<Movimiento>();//Lista para lineas a afectar en transaccion UPdate
                bool flagAdd = addMovimientos(ref lo_respuesta, ref MovimientoListUPD, MovimientoId, form, movimiento, trnMode.Insert);
                if (flagAdd)
                {
                    db.SaveChanges();

                    AfectaMovimiento(MovimientoId, null, trnMode.Insert);

                    #region auditoria
                    Auditoria auditoria = new Auditoria();

                    auditoria.AuditoriaFecha = System.DateTime.Now;
                    auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                    auditoria.usuarioId = Seguridadcll.Usuario.UsuarioId;
                    auditoria.AuditoriaEvento = "Crear";
                    auditoria.AuditoriaDesc = "Crea Movimiento: " + MovimientoId;
                    auditoria.ObjetoId = "Movimiento/Create";

                    seguridad.insertAuditoria(auditoria);
                    #endregion auditoria

                    //respuesta success
                    lo_respuesta.Add("validate", true);
                    lo_respuesta.Add("inventario", true);
                    lo_respuesta.Add("titulo", "Creación Existosa");
                    lo_respuesta.Add("mensaje", "El movimiento se creo con exito");
                }
                else
                {
                    //respuesta success
                    lo_respuesta.Add("validate", false);
                    lo_respuesta.Add("inventario", true);
                    lo_respuesta.Add("titulo", "Error al agregar lineas al movimiento");
                    lo_respuesta.Add("mensaje", "Error al agregar lineas al movimiento");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                //respuesta fail
                lo_respuesta.Add("validate", false);
                lo_respuesta.Add("inventario", true);
                lo_respuesta.Add("titulo", "Error");
                lo_respuesta.Add("mensaje", e.ToString());
            }


            return this.Json(lo_respuesta);
        }

        // GET: Movimientos/Edit/5
        [AuthorizeAction]
        public ActionResult Edit(int? MovimientoId = 0)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Movimiento> MovimientosList;

            if (MovimientoId == null || MovimientoId == 0)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + MovimientoId;
                MovimientosList = null;
            }
            else
            {
                MovimientosList = db.Movimiento
                                    .Where(m => m.MovimientoId == MovimientoId)
                                    .ToList();
                if (MovimientosList.Count == 0)
                {
                    ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + MovimientoId;
                    MovimientosList = null;
                }
                else
                {
                    var TipoMovConfig = db.Configuracion
                                          .Select(c => new { c.ConfigTipoMovEntrada, c.ConfigTipoMovSalida, c.ConfigTipoMovAjEntrada, c.ConfigTipoMovAjSalida })
                                          .FirstOrDefault();

                    var header = MovimientosList.FirstOrDefault();

                    //Si es salida no se puede modificar por medio del movimiento solo desde el numero de OrdenId
                    if (header.TipoMovimientoID == TipoMovConfig.ConfigTipoMovSalida)
                    {
                        var routes = GetActualReturnSearch();
                        routes.Add("MovimientoId", MovimientoId);

                        return RedirectToAction("Details", "Movimientos", routes);
                    }

                    string ConfigTipoProdInv = db.Configuracion.FirstOrDefault().ConfigTipoProdInv;

                    ViewBag.TipoMovConfig = TipoMovConfig;

                    ViewBag.ProductoId = new SelectList(db.Producto.Where(p => p.TipoProductoID == ConfigTipoProdInv), "ProductoId", "ProductoDesc", header.ProductoId);

                    string TipoMovimientoID = header.TipoMovimientoID;
                    ViewBag.TipoMovimientoID = new SelectList(db.TipoMovimientos
                                                                .Where(t => t.TipoMovimientoID == TipoMovimientoID)
                                                                .OrderByDescending(t => t.TipoMovimientoID), "TipoMovimientoID", "TipoMovimientoDesc", header.TipoMovimientoID);

                    EstadoMovimiento[] estados = (EstadoMovimiento[])Enum.GetValues(typeof(EstadoMovimiento));
                    var MovimientoEstado = from value in estados
                                           select new { value = (int)value, name = (value.ToString().Equals("Sin_Aplicar")) ? "Sin Aplicar" : value.ToString() };
                    ViewBag.MovimientoEstado = new SelectList(MovimientoEstado, "value", "name");

                    ViewBag.UsuarioIdModifica = Seguridadcll.Usuario.UsuarioId;
                }

            }
            return View(MovimientosList);
        }

        // POST: Movimientos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(FormCollection form)
        {
            Dictionary<string, dynamic> lo_respuesta = new Dictionary<string, dynamic>();
            try
            {
                //Tipo de movimiento entrada o Ajuste entrada
                string ConfigTipoMovEntrada = db.Configuracion.FirstOrDefault().ConfigTipoMovEntrada;
                string ConfigTipoMovAjEntrada = db.Configuracion.FirstOrDefault().ConfigTipoMovAjEntrada;
                string ConfigTipoMovAjSalida = db.Configuracion.FirstOrDefault().ConfigTipoMovAjSalida;

                int MovimientoId = int.Parse(form["MovimientoId"].ToString());
                string TipoMovimientoID = form["TipoMovimientoID"].ToString();

                if (TipoMovimientoID == ConfigTipoMovAjSalida)//solo si es salida
                {
                    var MovimientosListPast = db.Movimiento
                                           .Where(m => m.MovimientoId == MovimientoId)
                                           .ToList();
                    RegresaMovimiento(MovimientosListPast);
                }


                List<Movimiento> MovimientoListUPD = new List<Movimiento>();
                bool flagAdd = addMovimientos(ref lo_respuesta, ref MovimientoListUPD, MovimientoId, form, null, trnMode.Update);

                if (flagAdd)
                {
                    db.SaveChanges();
                    if (TipoMovimientoID == ConfigTipoMovAjSalida)//solo si es salida
                    {
                        AfectaMovimiento(MovimientoId, MovimientoListUPD, trnMode.Update);
                    }



                    #region auditoria
                    Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                    Auditoria auditoria = new Auditoria();

                    auditoria.AuditoriaFecha = System.DateTime.Now;
                    auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                    auditoria.usuarioId = Seguridadcll.Usuario.UsuarioId;
                    auditoria.AuditoriaEvento = "Edit";
                    auditoria.AuditoriaDesc = "Modifico Movimiento: " + MovimientoId;
                    auditoria.ObjetoId = "Movimiento/Edit";

                    seguridad.insertAuditoria(auditoria);
                    #endregion auditoria

                    //respuesta success
                    lo_respuesta.Add("validate", true);
                    lo_respuesta.Add("inventario", true);
                    lo_respuesta.Add("titulo", "Modificación Existosa");
                    lo_respuesta.Add("mensaje", "El movimiento se modifico con exito");
                }
                else
                {
                    //respuesta error
                    if (lo_respuesta.Count == 0)
                    {
                        lo_respuesta.Add("validate", false);
                        lo_respuesta.Add("inventario", true);
                        lo_respuesta.Add("titulo", "Error");
                        lo_respuesta.Add("mensaje", "Error al modificar el movimiento Procedimiento addMovimientos()");
                    }
                }


            }
            catch //(Exception e)
            {
                lo_respuesta.Add("validate", false);
                lo_respuesta.Add("inventario", true);
                lo_respuesta.Add("titulo", "Error");
                lo_respuesta.Add("mensaje", "Error al modificar el movimiento Try catch");
            }
            return this.Json(lo_respuesta);
        }

        // GET: Movimientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Movimiento movimiento = db.Movimiento.Find(id);
            if (movimiento == null)
            {
                return HttpNotFound();
            }
            return View(movimiento);
        }

        // POST: Movimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Movimiento movimiento = db.Movimiento.Find(id);
            db.Movimiento.Remove(movimiento);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        private bool addMovimientos(ref Dictionary<string, dynamic> lo_respuesta, ref List<Movimiento> MovimientoListUPD, int MovimientoId = 0, FormCollection form = null, Movimiento movimiento = null, trnMode mode = trnMode.Insert)
        {

            //bool flagResult = true;
            try
            {
                //Tipo de movimiento entrada o Ajuste entrada
                string ConfigTipoMovEntrada = db.Configuracion.FirstOrDefault().ConfigTipoMovEntrada;
                string ConfigTipoMovAjEntrada = db.Configuracion.FirstOrDefault().ConfigTipoMovAjEntrada;
                string ConfigTipoMovSalida = db.Configuracion.FirstOrDefault().ConfigTipoMovSalida;
                string ConfigTipoMovAjSalida = db.Configuracion.FirstOrDefault().ConfigTipoMovAjSalida;
                //int MovimientoId = 0;
                if (mode == trnMode.Insert)
                {

                    //Numero de Lineas en el movimiento 
                    int ln_inx = int.Parse(form["nroTableMovProductos"].ToString());

                    for (int idx = 1; idx <= ln_inx; idx++)
                    {
                        Movimiento mov = new Movimiento();
                        if (form["ProductoId" + idx.ToString()].ToString() != ""
                            && (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString()) != EstadoMovimiento.Eliminado)
                        {
                            if (idx == 1)
                            {

                                #region cabecera Movimiento
                                movimiento.MovimientoId = MovimientoId;


                                #endregion

                                #region Table Productos
                                movimiento.MovimientoLinea = idx;
                                movimiento.ProductoId = form["ProductoId" + idx.ToString()].ToString();
                                movimiento.MovimientoIDEntrada = form["MovimientoIDEntrada" + idx.ToString()].ToString();
                                //var cantidad = form["MovimientoCantidad" + idx.ToString()].ToString();
                                //var valor = form["MovimientoValor" + idx.ToString()].ToString();
                                movimiento.MovimientoCantidad = int.Parse(form["MovimientoCantidad" + idx.ToString()].ToString());
                                movimiento.MovimientoValor = Decimal.Parse(form["MovimientoValor" + idx.ToString()].ToString());
                                movimiento.MovimientoEstado = (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString());
                                if (movimiento.TipoMovimientoID == ConfigTipoMovEntrada || movimiento.TipoMovimientoID == ConfigTipoMovAjEntrada)
                                {
                                    movimiento.MovimientoDisponible = movimiento.MovimientoCantidad;
                                }
                                else
                                {

                                    bool flagInventario = validaInventario(0, int.Parse(movimiento.MovimientoIDEntrada), movimiento.ProductoId, movimiento.MovimientoCantidad, mode);
                                    if (!flagInventario)
                                    {
                                        lo_respuesta.Add("validate", flagInventario);
                                        lo_respuesta.Add("inventario", flagInventario);
                                        lo_respuesta.Add("titulo", "Error");
                                        lo_respuesta.Add("mensaje", "El inventario de algunos productos a cambiado por favor verifique nuevamente");
                                        return flagInventario;
                                    }

                                    movimiento.MovimientoDisponible = 0;
                                }
                                movimiento.MovimientoReservado = 0;

                                #endregion Table Productos


                                db.Movimiento.Add(movimiento);
                            }
                            else
                            {
                                #region cabecera Movimiento
                                mov.MovimientoId = MovimientoId;
                                mov.TipoMovimientoID = form["TipoMovimientoID"].ToString();
                                mov.MovimientoFechaCrea = DateTime.Parse(form["MovimientoFechaCrea"].ToString());
                                mov.MovimientoFechaMod = DateTime.Parse(form["MovimientoFechaMod"].ToString());
                                mov.UsuarioIdModifica = form["UsuarioIdModifica"].ToString();
                                mov.MovimientoNota = form["MovimientoNota"].ToString();
                                #endregion

                                #region Table Productos
                                mov.MovimientoLinea = idx;
                                mov.ProductoId = form["ProductoId" + idx.ToString()].ToString();
                                mov.MovimientoIDEntrada = form["MovimientoIDEntrada" + idx.ToString()].ToString();
                                mov.MovimientoCantidad = int.Parse(form["MovimientoCantidad" + idx.ToString()].ToString());
                                mov.MovimientoValor = Decimal.Parse(form["MovimientoValor" + idx.ToString()].ToString());
                                mov.MovimientoEstado = (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString());
                                if (mov.TipoMovimientoID == ConfigTipoMovEntrada || mov.TipoMovimientoID == ConfigTipoMovAjEntrada)
                                {
                                    mov.MovimientoDisponible = mov.MovimientoCantidad;
                                }
                                else
                                {
                                    bool flagInventario = validaInventario(0, int.Parse(movimiento.MovimientoIDEntrada), movimiento.ProductoId, movimiento.MovimientoCantidad, mode);
                                    if (!flagInventario)
                                    {
                                        lo_respuesta.Add("validate", flagInventario);
                                        lo_respuesta.Add("inventario", flagInventario);
                                        lo_respuesta.Add("titulo", "Error");
                                        lo_respuesta.Add("mensaje", "El inventario de algunos productos a cambiado por favor verifique nuevamente");
                                        return false;
                                    }
                                    mov.MovimientoDisponible = 0;
                                }
                                mov.MovimientoReservado = 0;

                                #endregion Table Productos

                                db.Movimiento.Add(mov);
                            }//if (idx == 1)


                        }//if (form["ProductoId" + idx.ToString()].ToString() != "" && (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString()) != EstadoMovimiento.Eliminado)
                    }//for (int idx = 1; idx <= ln_inx; idx++)
                }//if (mode == trnMode.Insert)
                else if (mode == trnMode.Update)
                {



                    int ln_inx = int.Parse(form["nroTableMovProductos"].ToString());

                    for (int idx = 1; idx <= ln_inx; idx++)
                    {
                        //Movimiento mov = new Movimiento();
                        string Key = "MovimientoLinea" + idx;

                        bool keyExist = form.AllKeys.ToList().Contains(Key);
                        if (keyExist)
                        {
                            if (form["ProductoId" + idx.ToString()].ToString() != "")
                            {
                                int MovimientoLinea = int.Parse(form["MovimientoLinea" + idx].ToString());
                                var mov = db.Movimiento
                                            .Where(m => m.MovimientoId == MovimientoId && m.MovimientoLinea == MovimientoLinea)
                                            .FirstOrDefault();

                                if (mov != null)
                                {

                                    #region Table Productos
                                    //mov.MovimientoLinea = idx;
                                    mov.ProductoId = form["ProductoId" + idx.ToString()].ToString();
                                    mov.MovimientoIDEntrada = form["MovimientoIDEntrada" + idx.ToString()].ToString();
                                    mov.MovimientoCantidad = int.Parse(form["MovimientoCantidad" + idx.ToString()].ToString());
                                    mov.MovimientoValor = Decimal.Parse(form["MovimientoValor" + idx.ToString()].ToString());
                                    mov.MovimientoEstado = (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString());
                                    if (mov.TipoMovimientoID == ConfigTipoMovEntrada || mov.TipoMovimientoID == ConfigTipoMovAjEntrada)
                                    {
                                        if (mov.MovimientoEstado != EstadoMovimiento.Eliminado)
                                        {
                                            mov.MovimientoDisponible = mov.MovimientoCantidad;
                                        }
                                        else
                                        {
                                            mov.MovimientoDisponible = 0;
                                        }
                                    }
                                    else
                                    {
                                        bool flagInventario = validaInventario(0, int.Parse(mov.MovimientoIDEntrada), mov.ProductoId, mov.MovimientoCantidad, trnMode.Insert);
                                        if (!flagInventario)
                                        {
                                            lo_respuesta.Add("validate", flagInventario);
                                            lo_respuesta.Add("inventario", flagInventario);
                                            lo_respuesta.Add("titulo", "Error");
                                            lo_respuesta.Add("mensaje", "El inventario de algunos productos a cambiado por favor verifique nuevamente");
                                            return false;
                                        }
                                        mov.MovimientoDisponible = 0;
                                    }
                                    mov.MovimientoReservado = 0;



                                    #endregion Table Productos

                                    db.Entry(mov).State = EntityState.Modified;
                                    if (mov.TipoMovimientoID == ConfigTipoMovAjSalida || mov.TipoMovimientoID == ConfigTipoMovSalida) { MovimientoListUPD.Add(mov); }

                                }
                                else
                                {
                                    if ((EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString()) != EstadoMovimiento.Eliminado)
                                    {
                                        mov = new Movimiento();

                                        #region cabecera Movimiento
                                        mov.MovimientoId = MovimientoId;
                                        mov.TipoMovimientoID = form["TipoMovimientoID"].ToString();
                                        mov.MovimientoFechaCrea = DateTime.Parse(form["MovimientoFechaCrea"].ToString());
                                        mov.MovimientoFechaMod = DateTime.Parse(form["MovimientoFechaMod"].ToString());
                                        mov.UsuarioIdModifica = form["UsuarioIdModifica"].ToString();
                                        mov.MovimientoNota = form["MovimientoNota"].ToString();
                                        #endregion

                                        #region Table Productos
                                        mov.MovimientoLinea = idx;
                                        mov.ProductoId = form["ProductoId" + idx.ToString()].ToString();
                                        mov.MovimientoIDEntrada = form["MovimientoIDEntrada" + idx.ToString()].ToString();
                                        mov.MovimientoCantidad = int.Parse(form["MovimientoCantidad" + idx.ToString()].ToString());
                                        mov.MovimientoValor = Decimal.Parse(form["MovimientoValor" + idx.ToString()].ToString());
                                        mov.MovimientoEstado = (EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString());
                                        if (mov.TipoMovimientoID == ConfigTipoMovEntrada || mov.TipoMovimientoID == ConfigTipoMovAjEntrada)
                                        {
                                            mov.MovimientoDisponible = mov.MovimientoCantidad;
                                        }
                                        else
                                        {
                                            bool flagInventario = validaInventario(0, int.Parse(mov.MovimientoIDEntrada), mov.ProductoId, mov.MovimientoCantidad, trnMode.Insert);
                                            if (!flagInventario)
                                            {
                                                lo_respuesta.Add("validate", flagInventario);
                                                lo_respuesta.Add("inventario", flagInventario);
                                                lo_respuesta.Add("titulo", "Error");
                                                lo_respuesta.Add("mensaje", "El inventario de algunos productos a cambiado por favor verifique nuevamente");
                                                return false;
                                            }
                                            mov.MovimientoDisponible = 0;
                                        }
                                        mov.MovimientoReservado = 0;

                                        #endregion Table Productos

                                        db.Movimiento.Add(mov);

                                        if (mov.TipoMovimientoID == ConfigTipoMovAjSalida || mov.TipoMovimientoID == ConfigTipoMovSalida) { MovimientoListUPD.Add(mov); }

                                    }//if ((EstadoMovimiento)int.Parse(form["MovimientoEstado" + idx.ToString()].ToString()) != EstadoMovimiento.Eliminado)
                                }//if (mov != null)
                            }//if (form["ProductoId" + idx.ToString()].ToString() != "")
                        }//if (keyExist)

                    }//for (int idx = 1; idx <= ln_inx; idx++)

                }//else if (mode == trnMode.Update)

            }
            catch (Exception e)
            {
                //ViewBag.Error = e.ToString();
                lo_respuesta.Add("validate", false);
                lo_respuesta.Add("inventario", true);
                lo_respuesta.Add("titulo", "Error");
                lo_respuesta.Add("mensaje", e.ToString());
                return false;
            }

            return true;
        }

        //Lista de movimientos con productos que tienen inventario
        public ActionResult _ListaProductosDisponible()
        {
            var TipoMovConfig = db.Configuracion
                                  .Select(c => new { c.ConfigTipoMovEntrada, c.ConfigTipoMovAjEntrada })
                                  .FirstOrDefault();

            var Movimientos = (from m in db.Movimiento
                               join p in db.Producto on m.ProductoId equals p.ProductoId
                               where (m.TipoMovimientoID == TipoMovConfig.ConfigTipoMovEntrada || m.TipoMovimientoID == TipoMovConfig.ConfigTipoMovAjEntrada)
                                       && m.MovimientoDisponible > 0 && m.MovimientoEstado == EstadoMovimiento.Abierto
                               select new { m.MovimientoId, p.ProductoId, p.ProductoDesc, m.MovimientoFechaCrea, m.MovimientoValor, m.MovimientoCantidad, m.MovimientoDisponible, m.MovimientoReservado }).ToList();



            return PartialView(Movimientos.ToList());
        }


        /// <summary>
        /// regresa el movimiento de salida(solo los estados activos (abierto))
        /// </summary>
        /// <param name="MovimientoList"></param>
        public void RegresaMovimiento(List<Movimiento> MovimientoList)
        {

            foreach (var mov in MovimientoList.Where(m => m.MovimientoEstado == EstadoMovimiento.Abierto))
            {
                int movIdEntrada = int.Parse(mov.MovimientoIDEntrada);
                if (movIdEntrada != 0)
                {
                    var movEntrada = db.Movimiento
                                        .Where(m => m.MovimientoId == movIdEntrada && m.ProductoId == mov.ProductoId && m.MovimientoEstado == EstadoMovimiento.Abierto)
                                        .FirstOrDefault();

                    if (movEntrada != null)
                    {
                        //Guarda Modificación en la entrada
                        movEntrada.MovimientoDisponible += mov.MovimientoCantidad;
                        movEntrada.MovimientoReservado -= mov.MovimientoCantidad;
                        db.Entry(movEntrada).State = EntityState.Modified;
                        db.SaveChanges();


                        //Guarda Modificación en la salida
                        mov.MovimientoEstado = EstadoMovimiento.Sin_Aplicar;
                        db.Entry(mov).State = EntityState.Modified;
                        db.SaveChanges();
                    }//if (movEntrada != null)
                }//if (movIdEntrada != 0)
            }//foreach (var mov in MovimientoList.Where(m => m.MovimientoEstado == EstadoMovimiento.Abierto))
        }

        public void AfectaMovimiento(int MovimientoId, List<Movimiento> MovimientoListUPD, trnMode mode)
        {
            //Configuración de Tipos de Movimientos
            var TipoMovConfig = db.Configuracion
                                  .Select(c => new { c.ConfigTipoMovSalida, c.ConfigTipoMovAjSalida })
                                  .FirstOrDefault();



            if (mode == trnMode.Insert)//Insertando un movimiento nuevo
            {

                //Obtiene lista de los movimientos a Afectar
                var MovimientoList = db.Movimiento
                                        .Where(m => m.MovimientoId == MovimientoId &&
                                                    //(m.MovimientoEstado == EstadoMovimiento.Abierto || m.MovimientoEstado == EstadoMovimiento.Ejecutado) &&
                                                    (m.TipoMovimientoID == TipoMovConfig.ConfigTipoMovAjSalida ||
                                                    m.TipoMovimientoID == TipoMovConfig.ConfigTipoMovSalida))
                                        .ToList();

                foreach (var mov in MovimientoList)
                {
                    try
                    {
                        int MovimientoIDEntrada = int.Parse(mov.MovimientoIDEntrada);
                        var movEntrada = db.Movimiento
                                            .Where(m => m.MovimientoId == MovimientoIDEntrada && m.MovimientoEstado == EstadoMovimiento.Abierto && m.MovimientoDisponible > 0 &&
                                                        m.ProductoId == mov.ProductoId)
                                            .FirstOrDefault();


                        //Sin aplicar afecta el disponible pero lo deja como reserva
                        if (mov.MovimientoEstado == EstadoMovimiento.Abierto)
                        {
                            movEntrada.MovimientoDisponible -= mov.MovimientoCantidad;
                            movEntrada.MovimientoReservado += mov.MovimientoCantidad;
                        }
                        //Abierto afecta el disponible sin dejar nada en reserva.
                        else if (mov.MovimientoEstado == EstadoMovimiento.Ejecutado)
                        {
                            movEntrada.MovimientoDisponible -= mov.MovimientoCantidad;
                        }

                        db.Entry(movEntrada).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch
                    {
                        mov.MovimientoEstado = EstadoMovimiento.Sin_Aplicar;
                        db.Entry(mov).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

            }
            else if (mode == trnMode.Update)//Modificando la tabla de movimientos
            {

                foreach (var mov in MovimientoListUPD)
                {
                    try
                    {
                        int MovimientoIDEntrada = int.Parse(mov.MovimientoIDEntrada);
                        var movEntrada = db.Movimiento
                                            .Where(m => m.MovimientoId == MovimientoIDEntrada && m.MovimientoEstado == EstadoMovimiento.Abierto &&
                                                        m.ProductoId == mov.ProductoId)
                                            .FirstOrDefault();


                        //Sin aplicar afecta el disponible pero lo deja como reserva
                        if (mov.MovimientoEstado == EstadoMovimiento.Abierto)
                        {
                            movEntrada.MovimientoDisponible -= mov.MovimientoCantidad;
                            movEntrada.MovimientoReservado += mov.MovimientoCantidad;
                        }
                        //Abierto afecta el disponible sin dejar nada en reserva.
                        else if (mov.MovimientoEstado == EstadoMovimiento.Ejecutado)
                        {
                            movEntrada.MovimientoDisponible -= mov.MovimientoCantidad;
                        }

                        db.Entry(movEntrada).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch //(Exception e)
                    {
                        mov.MovimientoEstado = EstadoMovimiento.Sin_Aplicar;
                        db.Entry(mov).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            else if (mode == trnMode.Delete)//Colocando en estado elimiando los movimiento
            {

            }

        }

        /// <summary>
        /// Valida inventario en tiempo real esto evita que dos personas afecten el mismo movimiento al mismo tiempo
        /// En el caso de movimientos de Salidas o Ajustes de Salidas
        /// true = si hay inventario
        /// flase = no hay inventario
        /// </summary>
        /// <param name="MovimientoId"></param>
        /// <param name="ProductoId"></param>
        /// <param name="MovimientoCant"></param>
        /// <param name="mode"
        private bool validaInventario(int MovimientoId, int MovimientoEntradaId, string ProductoId, int MovimientoCant, trnMode mode)
        {
            if (mode == trnMode.Insert)
            {
                var Movimiento = db.Movimiento
                                    .Where(m => m.MovimientoId == MovimientoEntradaId && m.ProductoId == ProductoId)
                                    .FirstOrDefault();

                if (Movimiento.MovimientoDisponible >= MovimientoCant)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (mode == trnMode.Update)
            {
                SqlParameter[] parametters = new SqlParameter[4];
                int cont = 0;
                parametters[cont++] = new SqlParameter("@MovimientoId", MovimientoId);
                parametters[cont++] = new SqlParameter("@ProductoId", ProductoId);
                parametters[cont++] = new SqlParameter("@MovimientoEntradaId", MovimientoEntradaId);
                parametters[cont++] = new SqlParameter("@ProductoEntrada", ProductoId);

                int MovimientoDisponible = db.Database.SqlQuery<int>("SELECT M.MovimientoDisponible + S.MovimientoCantidad FROM Movimiento AS M" +
                                                                     "   INNER JOIN(SELECT * FROM Movimiento WHERE MovimientoId = @MovimientoId AND ProductoId = @ProductoId) AS S" +
                                                                     "   ON M.MovimientoId = S.MovimientoIDEntrada" +
                                                                     "   WHERE M.MovimientoId = @MovimientoEntradaId AND M.ProductoId = @ProductoEntrada ", parametters).FirstOrDefault();

                if (MovimientoDisponible >= MovimientoCant)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        [HttpPost]
        public JsonResult DisponibleXProducto(string productos, trnMode mode)
        {
            List<Movimiento> MovimientoList = new List<Movimiento>();
            var jss = new JavaScriptSerializer();

            dynamic data = jss.Deserialize<dynamic>(productos);//crear variable dinamica

            if (mode == trnMode.Insert)
            {
                foreach (var item in data)
                {
                    int MovimientoEntradaId = int.Parse(item["MovimientoEntradaId"]);
                    string ProductoId = item["ProductoId"];
                    var Movimiento = db.Movimiento
                                        .Where(m => m.MovimientoId == MovimientoEntradaId && m.ProductoId == ProductoId)
                                        .FirstOrDefault();

                    MovimientoList.Add(Movimiento);
                }
            }
            else if (mode == trnMode.Update)
            {
                foreach (var item in data)
                {
                    int MovimientoId = int.Parse(item["MovimientoId"]);
                    int MovimientoEntradaId = int.Parse(item["MovimientoEntradaId"]);
                    string ProductoId = item["ProductoId"];
                    SqlParameter[] parametters = new SqlParameter[4];
                    int cont = 0;
                    parametters[cont++] = new SqlParameter("@MovimientoId", MovimientoId);
                    parametters[cont++] = new SqlParameter("@ProductoId", ProductoId);
                    parametters[cont++] = new SqlParameter("@MovimientoEntradaId", MovimientoEntradaId);
                    parametters[cont++] = new SqlParameter("@ProductoEntrada", ProductoId);

                    var Movimiento = db.Database.SqlQuery<Movimiento>("SELECT  M.MovimientoId, M.MovimientoEstado, M.MovimientoValor, M.MovimientoCantidad, " +
                                                                         "   CASE S.MovimientoEstado " +
                                                                         "   WHEN 1 THEN M.MovimientoDisponible " +
                                                                         "   WHEN 2 THEN(M.MovimientoDisponible + S.MovimientoCantidad) " +
                                                                         "   WHEN 3 THEN(M.MovimientoDisponible + S.MovimientoCantidad) " +
                                                                         "   WHEN 4 THEN M.MovimientoDisponible " +
                                                                         "   END AS MovimientoDisponible, " +
                                                                         "   M.MovimientoIDEntrada, M.ProductoId, M.TipoMovimientoID, M.OrdenId, M.MovimientoLinea," +
                                                                         "   M.MovimientoReservado, M.MovimientoFechaCrea, M.MovimientoFechaMod, M.UsuarioIdModifica, M.MovimientoNota " +
                                                                         "   FROM Movimiento AS M" +
                                                                         "   INNER JOIN(SELECT * FROM Movimiento WHERE MovimientoId = @MovimientoId AND ProductoId = @ProductoId) AS S" +
                                                                         "   ON M.MovimientoId = S.MovimientoIDEntrada" +
                                                                         "   WHERE M.MovimientoId = @MovimientoEntradaId AND M.ProductoId = @ProductoEntrada ", parametters).FirstOrDefault();

                    if (Movimiento == null)
                    {
                        Movimiento = db.Movimiento
                                       .Where(m => m.MovimientoId == MovimientoEntradaId && m.ProductoId == ProductoId)
                                       .FirstOrDefault();
                    }


                    MovimientoList.Add(Movimiento);
                }
            }

            var result = MovimientoList.Select(m => new { m.MovimientoId, m.ProductoId, m.MovimientoDisponible }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _DetalleMovimiento(int MovimientoId = 0, int linea = 0)
        {
            var movEntrada = db.Movimiento
                                .Where(m => m.MovimientoId == MovimientoId && m.MovimientoLinea == linea)
                                .FirstOrDefault();

            var movSalidaList = new List<Movimiento>(); ;
            if (movEntrada.MovimientoEstado != EstadoMovimiento.Eliminado)
            {
                movSalidaList = db.Movimiento
                                        .Include(m => m.producto)
                                        .Include(m => m.tipoMovimiento)
                                        .Where(m => m.MovimientoIDEntrada == MovimientoId.ToString() &&
                                                    m.ProductoId == movEntrada.ProductoId &&
                                                    (m.MovimientoEstado == EstadoMovimiento.Abierto || m.MovimientoEstado == EstadoMovimiento.Ejecutado))
                                        .ToList();
            }
            else
            {
                movSalidaList = new List<Movimiento>();
            }
            ViewBag.movEntrada = movEntrada;

            return View(movSalidaList);
        }

        public dynamic CreaMovimientoXOrden(int OrdenId = 0, trnMode mode = trnMode.Insert, Seguridadcll seguridadcll = null)
        {
            int MovimientoId = 0;
            if (mode == trnMode.Update)
            {
                var MovimientoSalidaList = db.Movimiento.Where(m => m.OrdenId == OrdenId).ToList();
                if (MovimientoSalidaList.Count > 0)
                {
                    MovimientoId = MovimientoSalidaList.FirstOrDefault().MovimientoId;

                    MovimientoSalidaList.ForEach(m => db.Movimiento.Remove(m));
                    db.SaveChanges();
                }
            }

            int NroMovimiento = 0;
            var orden = db.Orden.Where(o => o.OrdenId == OrdenId).FirstOrDefault();
            //Valida que exista la orden
            if (orden != null)
            {

                Configuracion config = seguridadcll.Configuracion;

                var ordenItems = db.OrdenItems.Where(oi => oi.OrdenId == OrdenId).ToList();

                //Obtiene numero de Movimiento(ultimo de la tabla + 1)
                Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                if (MovimientoId == 0)
                {
                    //Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                    NroMovimiento = seguridad.generaConsecutivo("Movimientos");
                }
                else
                {
                    NroMovimiento = MovimientoId;
                }

                int Linea = 1;
                //Recorre lineas para crear movimientos
                foreach (var item in ordenItems)
                {
                    List<Movimiento> MovEntradas = DisponibilidadProductoNonSession(item.ProductoId, false, seguridadcll);
                    int OrdenItemsCant = 0;
                    if (mode == trnMode.Update)
                    {
                        OrdenItemsCant = item.OrdenItemsCantConfirmada;
                    }
                    else
                    {
                        OrdenItemsCant = item.OrdenItemsCant;
                    }
                    //Existan entradas con disponibilidad
                    if (MovEntradas.Count > 0)
                    {
                        //Recorre las entradas gastando la salida de la orden 
                        foreach (var mov in MovEntradas)
                        {

                            Movimiento movimiento = new Movimiento();
                            //Cabecera Movimiento
                            movimiento.MovimientoId = NroMovimiento;
                            movimiento.MovimientoFechaCrea = DateTime.Today;
                            movimiento.MovimientoFechaMod = DateTime.Today;
                            movimiento.TipoMovimientoID = config.ConfigTipoMovSalida;
                            movimiento.UsuarioIdModifica = orden.UsuarioIdModifica;
                            movimiento.MovimientoNota = orden.OrdenComentario;
                            movimiento.OrdenId = orden.OrdenId;


                            //Detalle del Movimiento
                            movimiento.MovimientoEstado = EstadoMovimiento.Abierto;
                            movimiento.ProductoId = item.ProductoId;
                            movimiento.MovimientoValor = item.OrdenItemsVlr;
                            movimiento.MovimientoDisponible = 0;
                            movimiento.MovimientoReservado = 0;

                            //por cada linea
                            movimiento.MovimientoLinea = Linea;
                            movimiento.MovimientoIDEntrada = mov.MovimientoId.ToString();//Movimiento de donde se saca el inventario
                            //Se pide mas de lo del inventario
                            if (OrdenItemsCant > mov.MovimientoDisponible)
                            {
                                movimiento.MovimientoCantidad = mov.MovimientoDisponible;
                                OrdenItemsCant -= mov.MovimientoDisponible;
                                item.OrdenItemsNroMov += mov.MovimientoId.ToString() + "-" + mov.MovimientoDisponible.ToString() + ";";
                            }
                            else
                            {
                                movimiento.MovimientoCantidad = OrdenItemsCant;
                                item.OrdenItemsNroMov += mov.MovimientoId.ToString() + "-" + OrdenItemsCant.ToString() + ";";
                                OrdenItemsCant = 0;

                            }//if (OrdenItemsCant > mov.MovimientoDisponible)

                            db.Movimiento.Add(movimiento);
                            Linea++;
                            //Ya la no hay mas cantidad solicitada
                            if (OrdenItemsCant == 0)
                            {
                                break;
                            }
                            else
                            {

                            }//if (OrdenItemsCant == 0)
                        }//foreach (var mov in MovEntradas)

                    }
                    else
                    {
                        Movimiento movimiento = new Movimiento();
                        //Cabecera Movimiento
                        movimiento.MovimientoId = NroMovimiento;
                        movimiento.MovimientoFechaCrea = DateTime.Today;
                        movimiento.MovimientoFechaMod = DateTime.Today;
                        movimiento.TipoMovimientoID = config.ConfigTipoMovSalida;
                        movimiento.UsuarioIdModifica = orden.UsuarioIdModifica;
                        movimiento.MovimientoNota = orden.OrdenComentario;
                        movimiento.OrdenId = orden.OrdenId;


                        //Detalle del Movimiento
                        movimiento.MovimientoEstado = EstadoMovimiento.Abierto;
                        movimiento.ProductoId = item.ProductoId;
                        movimiento.MovimientoValor = item.OrdenItemsVlr;
                        movimiento.MovimientoDisponible = 0;
                        movimiento.MovimientoReservado = 0;
                        //No hay inventario para realizar la salida, pero se registra en cero el movimiento
                        movimiento.MovimientoLinea = Linea;
                        movimiento.MovimientoCantidad = 0;
                        movimiento.MovimientoIDEntrada = "";
                        db.Movimiento.Add(movimiento);
                        Linea++;
                    }//if (MovEntradas.Count > 0)
                    if (mode == trnMode.Insert)
                    {
                        item.OrdenItemsCantConfirmada = item.OrdenItemsCant - OrdenItemsCant;
                    }
                    else
                    {
                        item.OrdenItemsCantConfirmada = item.OrdenItemsCantConfirmada;
                    }

                    db.Entry(item).State = EntityState.Modified;
                    //db.SaveChanges();
                }//foreach (var item in ordenItems)
                db.SaveChanges();
                AfectaMovimiento(NroMovimiento, null, trnMode.Insert);


                #region auditoria

                Auditoria auditoria = new Auditoria();


                auditoria.AuditoriaFecha = System.DateTime.Now;
                auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                if (mode == trnMode.Insert)
                {
                    auditoria.AuditoriaEvento = "Create";
                    auditoria.AuditoriaDesc = "Se Creo el movimiento: " + NroMovimiento;
                }
                else
                {
                    auditoria.AuditoriaEvento = "Edit";
                    auditoria.AuditoriaDesc = "Se Modifico el Movimiento: " + NroMovimiento;
                }
                auditoria.ObjetoId = "Movimientos/CreaMovimientoXOrden";

                seguridad.insertAuditoria(auditoria);
                #endregion auditoria

            }//if (orden != null)
            else
            {
                NroMovimiento = 0;
            }

            //return RedirectToAction("Index", "Ordenes", null);
            return NroMovimiento;
        }



        public dynamic DisponibilidadProducto(string productoId = null, bool TypeJson = false)
        {
            List<Movimiento> movimientoList;
            if (HttpContext.Session.Count > 0 && productoId != null)
            {
                Configuracion config = Seguridadcll.Configuracion;

                movimientoList = db.Movimiento
                                    .Where(m => (m.TipoMovimientoID == config.ConfigTipoMovAjEntrada || m.TipoMovimientoID == config.ConfigTipoMovEntrada)
                                                && m.ProductoId == productoId && m.MovimientoDisponible > 0 && m.MovimientoEstado == EstadoMovimiento.Abierto)
                                                .OrderBy(m => m.MovimientoFechaCrea)
                                    .ToList();

            }
            else
            {
                movimientoList = new List<Movimiento>();
            }

            //Si solicita la informacion en formato JSON o C# List<Movimiento>
            if (TypeJson)
            {
                return this.Json(movimientoList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return movimientoList;
            }

        }

        public dynamic DisponibilidadProductoNonSession(string productoId = null, bool TypeJson = false, Seguridadcll seguridadcll = null)
        {
            List<Movimiento> movimientoList;
            if (seguridadcll != null && productoId != null)
            {
                Configuracion config = seguridadcll.Configuracion;


                movimientoList = db.Movimiento
                                    .Where(m => (m.TipoMovimientoID == config.ConfigTipoMovAjEntrada || m.TipoMovimientoID == config.ConfigTipoMovEntrada)
                                                && m.ProductoId == productoId && m.MovimientoDisponible > 0 && m.MovimientoEstado == EstadoMovimiento.Abierto)
                                                .OrderBy(m => m.MovimientoFechaCrea)
                                    .ToList();

            }
            else
            {
                movimientoList = new List<Movimiento>();
            }

            //Si solicita la informacion en formato JSON o C# List<Movimiento>
            if (TypeJson)
            {
                return this.Json(movimientoList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return movimientoList;
            }

        }
        /// <summary>
        /// Consulta una apiController con la información del inventario actual.
        /// </summary>
        /// <returns></returns>
        //[AuthorizeAction]
        public ActionResult ConsultaInventario()
        {
            return View();
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
