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

namespace MMS.Controllers.Seguridad
{
    public class AuditoriaController : Controller
    {
        private MMSContext db = new MMSContext();

        // GET: Auditoria
        [Seguridad]
        public ActionResult Index()
        {
            //return View(db.Auditoria.ToList());
            return View();
        }

        // GET: Auditoria/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auditoria auditoria = db.Auditoria.Find(id);
            if (auditoria == null)
            {
                return HttpNotFound();
            }
            return View(auditoria);
        }

        //// GET: Auditoria/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Auditoria/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "AuditoriaId,AuditoriaFecha,AuditoriaHora,usuarioId,AuditoriaEvento,AuditoriaDesc,ObjetoId,AuditoriaEquipo")] Auditoria auditoria)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Auditoria.Add(auditoria);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(auditoria);
        //}

        //// GET: Auditoria/Edit/5
        //public ActionResult Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Auditoria auditoria = db.Auditoria.Find(id);
        //    if (auditoria == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(auditoria);
        //}

        //// POST: Auditoria/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "AuditoriaId,AuditoriaFecha,AuditoriaHora,usuarioId,AuditoriaEvento,AuditoriaDesc,ObjetoId,AuditoriaEquipo")] Auditoria auditoria)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(auditoria).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(auditoria);
        //}

        //// GET: Auditoria/Delete/5
        //public ActionResult Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Auditoria auditoria = db.Auditoria.Find(id);
        //    if (auditoria == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(auditoria);
        //}

        //// POST: Auditoria/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(long id)
        //{
        //    Auditoria auditoria = db.Auditoria.Find(id);
        //    db.Auditoria.Remove(auditoria);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
