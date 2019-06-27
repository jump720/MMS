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

namespace MMS.Controllers.Transacciones
{
    public class OrdenesController : BaseController
    {
        private MMSContext db = new MMSContext();

        // GET: Ordenes
        [AuthorizeAction]
        [FillPermission]
        public ActionResult Index()
        {
            EstadoOrden[] estados = (EstadoOrden[])Enum.GetValues(typeof(EstadoOrden));
            var EstadoOrden = from value in estados
                              select new { value = (int)value, name = value.ToString() };
            ViewBag.EstadoOrden = new SelectList(EstadoOrden, "value", "name");

            ViewBag.rolid = Seguridadcll.RolObjetoList.FirstOrDefault().RolId;
            return View();
        }

        // GET: Ordenes/Details/5
        [AuthorizeAction]
        public ActionResult Details(int? id)
        {
            Orden orden = db.Orden.Find(id);
            if (orden == null)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
            }
            else
            {
                db.Configuration.ProxyCreationEnabled = false;
                ViewBag.OrdenItems = db.OrdenItems
                                        .Include(o => o.producto)
                                        .Where(o => o.OrdenId == id).ToList()
                                        .Select(o => new { o.OrdenItemsLinea, o.ProductoId, o.producto.ProductoDesc, o.OrdenItemsVlr, o.OrdenItemsCant, o.OrdenItemsCantConfirmada, o.OrdenItemsNroMov, o.CentroCostoID });
            }
            return View(orden);
        }

        // GET: Ordenes/Create
        //[Seguridad]
        //public ActionResult Create()
        //{
        //    ViewBag.ActividadId = new SelectList(db.Actividad, "ActividadId", "ActividadTitulo");
        //    ViewBag.UsuarioIdModifica = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre");
        //    return View();
        //}

        // POST: Ordenes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //[HttpPost]
        //[Seguridad]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Orden orden)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            //orden.OrdenId = 1;
        //            db.Orden.Add(orden);
        //            db.SaveChanges();
        //        }
        //        catch (Exception e)
        //        {
        //            ViewBag.error = e.ToString();
        //        }

        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.ActividadId = new SelectList(db.Actividad, "ActividadId", "ActividadTitulo", orden.ActividadId);
        //    ViewBag.UsuarioIdModifica = new SelectList(db.Usuarios, "UsuarioId", "UsuarioNombre", orden.UsuarioIdModifica);
        //    return View(orden);
        //}

        // GET: Ordenes/Edit/5
        [AuthorizeAction]
        public ActionResult Edit(int? id)
        {
            Orden orden = db.Orden.Find(id);
            if (orden == null)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
            }
            else
            {
                db.Configuration.ProxyCreationEnabled = false;
                ViewBag.OrdenItems = db.OrdenItems
                                        .Include(o => o.producto)
                                        .Where(o => o.OrdenId == id).ToList()
                                        .Select(o => new { o.OrdenItemsLinea, o.ProductoId, o.producto.ProductoDesc, o.OrdenItemsVlr, o.OrdenItemsCant, o.OrdenItemsCantConfirmada, o.OrdenItemsNroMov, o.CentroCostoID });
            }
            return View(orden);
        }

        // POST: Ordenes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Orden orden, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                //Modifica Cabecera de la orden
                db.Entry(orden).State = EntityState.Modified;
                db.SaveChanges();

                //Modifica Items de la orden
                bool flagMod = ModificaOrdenItems(orden.OrdenId, form);

                if (flagMod)
                {

                    //Afecta cantidad de Productos Inventariables en el Gasto

                    //Reversa Movimiento de salida por la orden
                    List<Movimiento> movimientoList = db.Movimiento.Where(m => m.OrdenId == orden.OrdenId).ToList();
                    MovimientosController movCtrl = new MovimientosController();
                    movCtrl.RegresaMovimiento(movimientoList);

                    Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];
                    // MovimientosController movCtrl = new MovimientosController();
                    movCtrl.CreaMovimientoXOrden(orden.OrdenId, trnMode.Update, seguridadcll);

                    #region auditoria
                    Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                    Auditoria auditoria = new Auditoria();


                    auditoria.AuditoriaFecha = System.DateTime.Now;
                    auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                    auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                    auditoria.AuditoriaEvento = "Edit";
                    auditoria.AuditoriaDesc = "Se Modifico La Orden: " + orden.OrdenId;
                    auditoria.ObjetoId = "Ordenes/Edit";

                    seguridad.insertAuditoria(auditoria);
                    #endregion auditoria


                    ////Modifica Cantidades del Movimiento
                    //return RedirectToAction("CreaMovimientoXOrden", "Movimientos", new { OrdenId = orden.OrdenId, mode = trnMode.Update });
                    //Afecta cantidad de Productos Inventariables en el Gasto


                    return RedirectToAction("Index", GetReturnSearch());
                }
            }
            db.Configuration.ProxyCreationEnabled = false;
            ViewBag.OrdenItems = db.OrdenItems.Where(o => o.OrdenId == orden.OrdenId).ToList();

            return View(orden);
        }

        // GET: Ordenes/Delete/5
        [AuthorizeAction]
        public ActionResult Delete(int? id)
        {
            Orden orden = db.Orden.Find(id);
            if (orden == null)
            {
                ViewBag.Error = "Advertencia, Registro no encontrado o Invalido " + id;
            }
            else
            {
                db.Configuration.ProxyCreationEnabled = false;
                ViewBag.OrdenItems = db.OrdenItems
                                        .Include(o => o.producto)
                                        .Where(o => o.OrdenId == id).ToList()
                                        .Select(o => new { o.OrdenItemsLinea, o.ProductoId, o.producto.ProductoDesc, o.OrdenItemsVlr, o.OrdenItemsCant, o.OrdenItemsCantConfirmada, o.OrdenItemsNroMov, o.CentroCostoID });
            }
            return View(orden);
        }

        // POST: Ordenes/Delete/5
        [HttpPost, ActionName("Delete")]
        [AuthorizeAction]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //Reversar Movimientos
            List<Movimiento> movimientoList = db.Movimiento.Where(m => m.OrdenId == id).ToList();
            MovimientosController movCtrl = new MovimientosController();
            movCtrl.RegresaMovimiento(movimientoList);

            //Poner eliminado cada Movimiento
            foreach (var mov in movimientoList)
            {
                mov.MovimientoEstado = EstadoMovimiento.Eliminado;
                db.Entry(mov).State = EntityState.Modified;
                db.SaveChanges();
            }



            //Cambiar estado de la orden a eliminado
            Orden orden = db.Orden.Find(id);
            orden.OrdenEstado = EstadoOrden.Eliminado;
            db.SaveChanges();


            //Actualiza Estado del Gasto
            var GastoList = db.Gasto.Where(g => g.ActividadId == orden.ActividadId).ToList();
            foreach (var gasto in GastoList)
            {
                gasto.GastoEstado = EstadoGasto.Eliminado;
                db.Entry(gasto).State = EntityState.Modified;
                db.SaveChanges();
            }

            #region auditoria
            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
            Auditoria auditoria = new Auditoria();
            Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

            auditoria.AuditoriaFecha = System.DateTime.Now;
            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
            auditoria.AuditoriaEvento = "Delete";
            auditoria.AuditoriaDesc = "Se cambio a estado eliminado la Orden: " + id;
            auditoria.ObjetoId = "Ordenes/Delete";

            seguridad.insertAuditoria(auditoria);
            #endregion auditoria

            return RedirectToAction("Index");
        }

        public dynamic CreaOrdenXActividad(int actividadId = 0, Seguridadcll seguridadcll = null)
        {
            int NroOrden = 0;
            var actividad = db.Actividad.Where(a => a.ActividadId == actividadId).FirstOrDefault();
            //valida que exista la actividad
            if (actividad != null)
            {
                //valida que el estado sea autorizado
                if (actividad.ActividadEstado == EstadosActividades.Autorizado)
                {

                    //Valida que no exista orden para esta actividad

                    var ordenTemp = db.Orden.Where(o => o.ActividadId == actividadId && o.OrdenEstado != EstadoOrden.Eliminado)
                                            .FirstOrDefault();

                    if (ordenTemp == null)
                    {
                        var actividadItems = db.ActividadItem
                                                .Include(a => a.producto)
                                                .Where(ai => ai.ActividadId == actividadId &&
                                                             ai.producto.TipoProductoID == "1").ToList();
                        if (actividadItems.Count > 0)
                        {
                            //Obtiene numero de Orden(ultimo de la tabla + 1)
                            Seguridad.Seguridad seguridad = new Seguridad.Seguridad();
                            NroOrden = seguridad.generaConsecutivo("Ordenes");


                            //Crear cabecera de la Orden
                            Orden orden = new Orden();
                            orden.OrdenId = NroOrden;
                            orden.ActividadId = actividad.ActividadId;
                            orden.OrdenFecha = DateTime.Today;
                            orden.OrdenFechaDespacho = DateTime.Today;
                            orden.OrdenFechaModificacion = DateTime.Today;
                            orden.OrdenNroGuia = "";
                            orden.OrdenComentario = actividad.ActividadTitulo;
                            orden.UsuarioIdModifica = actividad.UsuarioIdElabora;
                            orden.OrdenEstado = EstadoOrden.Abierta;

                            db.Orden.Add(orden);

                            db.SaveChanges();

                            //Crea Detalle de la orden 
                            int Linea = 1;
                            foreach (var item in actividadItems)
                            {
                                var producto = db.Producto.Where(p => p.ProductoId == item.ProductoId).FirstOrDefault();
                                if (producto.TipoProductoID == "1")
                                {
                                    OrdenItems ordenItems = new OrdenItems();
                                    ordenItems.OrdenId = NroOrden;
                                    ordenItems.OrdenItemsLinea = Linea;
                                    ordenItems.ProductoId = item.ProductoId;
                                    ordenItems.OrdenItemsCant = item.ActividadItemCantidad;
                                    ordenItems.OrdenItemsCantConfirmada = 0;
                                    ordenItems.OrdenItemsVlr = item.ActividadItemPrecio ?? 0;
                                    ordenItems.CentroCostoID = item.CentroCostoID;

                                    db.OrdenItems.Add(ordenItems);

                                    Linea++;
                                }

                            }//foreach (var item in actividadItems)
                            db.SaveChanges();
                            //MovimientosController movCtrl = new MovimientosController();
                            //int NroMovimiento = movCtrl.CreaMovimientoXOrden(NroOrden);

                            #region auditoria

                            Auditoria auditoria = new Auditoria();
                            //Seguridadcll seguridadcll = (Seguridadcll)Session["seguridad"];

                            auditoria.AuditoriaFecha = System.DateTime.Now;
                            auditoria.AuditoriaHora = System.DateTime.Now.TimeOfDay;
                            auditoria.usuarioId = seguridadcll.Usuario.UsuarioId;
                            auditoria.AuditoriaEvento = "Create";
                            auditoria.AuditoriaDesc = "Se Creo la Orden: " + NroOrden;
                            auditoria.ObjetoId = "Ordenes/CreaOrdenXActividad";

                            seguridad.insertAuditoria(auditoria);
                            #endregion auditoria
                            //Afecta cantidad de Productos Inventariables en el Gasto
                            MovimientosController movCtrl = new MovimientosController();
                            movCtrl.CreaMovimientoXOrden(orden.OrdenId, trnMode.Insert, seguridadcll);
                            //return RedirectToAction("CreaMovimientoXOrden", "Movimientos", new { OrdenId = orden.OrdenId, mode = trnMode.Insert });
                        }
                        else
                        {
                            //Cambia estado de la actividad a despachada y el gasto tambien
                            //Actualiza Estado del Gasto
                            var GastoList = db.Gasto.Where(g => g.ActividadId == actividad.ActividadId).ToList();
                            foreach (var gasto in GastoList)
                            {
                                gasto.GastoEstado = EstadoGasto.Ejecutado;
                                db.Entry(gasto).State = EntityState.Modified;
                                db.SaveChanges();
                            }//foreach (var gasto in GastoList)


                            //Actualiza estado de la actividad
                            ActividadesController actCtrl = new ActividadesController();
                            actCtrl.CambiaEstadoActividad(actividad.ActividadId, (int)EstadosActividades.Despachado);

                            //return RedirectToAction("Index", "Actividades");
                        }//if (actividadItems.Count > 0)
                    }
                    else
                    {
                        NroOrden = 0;
                    }//if (ordenTemp == null) {


                }
                else
                {
                    NroOrden = 0;
                }//if(actividad.ActividadEstado == EstadosActividades.Autorizado)
            }
            else
            {
                NroOrden = 0;
            }//if (actividad != null)


            return NroOrden;
        }


        public bool ModificaOrdenItems(int OrdenId, FormCollection form)
        {
            bool result = true;
            try
            {
                //Numero de Lineas en el movimiento 
                int ln_inx = int.Parse(form["nroTableOrdenItems"].ToString());
                for (int idx = 1; idx <= ln_inx; idx++)
                {

                    string Key = "OrdenItemsLinea" + idx;

                    bool keyExist = form.AllKeys.ToList().Contains(Key);

                    if (keyExist)
                    {

                        int OrdenItemsLinea = int.Parse(form["OrdenItemsLinea" + idx].ToString());
                        var item = db.OrdenItems
                                    .Where(o => o.OrdenId == OrdenId && o.OrdenItemsLinea == OrdenItemsLinea)
                                    .FirstOrDefault();

                        if (item != null)
                        {
                            item.OrdenItemsCantConfirmada = int.Parse(form["OrdenItemsCantConfirmada" + idx.ToString()].ToString());
                            item.OrdenItemsNroMov = "";
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }//if (item != null)



                    }// if (keyExist)

                }//for (int idx = 1; idx <= ln_inx; idx++)

                result = true;
            }
            catch
            {
                result = false;
            }



            return result;
        }


        public bool CambiaEstadoOrden(int OrdenId, EstadoOrden estado)
        {
            bool result = true;

            var orden = db.Orden.Where(o => o.OrdenId == OrdenId).FirstOrDefault();
            if (orden != null)
            {
                //Realiza una acción dependiendo el estado
                switch (estado)
                {
                    case EstadoOrden.Por_despachar:
                        //Cambia estado de la orden                    
                        orden.OrdenEstado = estado;
                        db.Entry(orden).State = EntityState.Modified;
                        db.SaveChanges();

                        break;
                    case EstadoOrden.Despachado:
                        if (!string.IsNullOrEmpty(orden.OrdenNroGuia))
                        {
                            orden.OrdenEstado = estado;
                            orden.OrdenFechaDespacho = DateTime.Today;
                            db.Entry(orden).State = EntityState.Modified;
                            db.SaveChanges();

                            //Actualiza Estado del Gasto
                            var GastoList = db.Gasto.Where(g => g.ActividadId == orden.ActividadId).ToList();
                            foreach (var gasto in GastoList)
                            {
                                gasto.GastoEstado = EstadoGasto.Ejecutado;
                                db.Entry(gasto).State = EntityState.Modified;
                                db.SaveChanges();
                            }


                            //Actualiza estado de la actividad
                            ActividadesController actCtrl = new ActividadesController();
                            actCtrl.CambiaEstadoActividad(orden.ActividadId, (int)EstadosActividades.Despachado);

                            //var actividad = db.Actividad.Where(a => a.ActividadId == orden.ActividadId).FirstOrDefault();
                            //actividad.ActividadEstado = EstadosActividades.Despachado;
                            //db.Entry(actividad).State = EntityState.Modified;
                            //db.SaveChanges();

                            //regresa Movimiento
                            List<Movimiento> movimientoList = db.Movimiento.Where(m => m.OrdenId == OrdenId).ToList();
                            MovimientosController movCtrl = new MovimientosController();
                            if (movimientoList.Count > 0)
                                movCtrl.RegresaMovimiento(movimientoList);

                            //Actualiza estado del movimiento
                            foreach (var mov in movimientoList)
                            {
                                mov.MovimientoEstado = EstadoMovimiento.Ejecutado;
                                db.Entry(mov).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            //Afecta Movimiento
                            if (movimientoList.Count > 0)
                                movCtrl.AfectaMovimiento(movimientoList.FirstOrDefault().MovimientoId, movimientoList, trnMode.Update);
                        }
                        else
                        {
                            result = false;
                        }
                        break;
                    default:
                        result = false;
                        break;
                }//switch (estado)


            }//if (orden != null)




            return result;
        }


        //private bool Modifica

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
