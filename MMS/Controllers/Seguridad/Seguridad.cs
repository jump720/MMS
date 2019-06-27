using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using MMS.Models;
using System.Net;

namespace MMS.Controllers.Seguridad
{
    public class Seguridad
    {
        private MMSContext db = new MMSContext();

        public void insertAuditoria(Auditoria modelo)
        {
            MMSContext db = new MMSContext();
            //SqlParameter[] parametters = new SqlParameter[9];
            //int cont = 0;
            try
            {
                System.Web.HttpContext context = HttpContext.Current;
                // System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(context.Request.UserHostAddress);

                modelo.AuditoriaEquipo = "SERVER/HTCL0003";//hostEntry.HostName;

                db.Auditoria.Add(modelo);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }

        }

        /*función o metodo que valida los permisos de un rol con los objetos (esto se valida con los datos de la session[Seguridad])*/
        public bool validaSeguridad(Seguridadcll seguridadcll = null, string ObjetoId = "")
        {
            bool Result = false;
            //List<RolUsuario> RolUsuarioList = seguridadcll.RolUsuarioList;
            List<RolObjeto> RolObjetoList = seguridadcll.RolObjetoList;


            var RolObjeto = RolObjetoList
                            .Where(o => o.ObjetoId.ToLower().Trim() == ObjetoId.ToLower().Trim()).FirstOrDefault();
            //Result = (RolObjeto != null) ? true : Result;

            if (RolObjeto != null)//Si tiene permiso
            {
                Result = true;
            }
            else//No tiene permiso
            {
                Result = false;
                /*Crear el objeto si no existe en la BD*/
                var objeto = db.Objeto
                                .Where(o => o.ObjetoId.ToLower().Trim() == ObjetoId.ToLower().Trim())
                                .FirstOrDefault();
                if (objeto == null)
                {
                    Objeto o = new Objeto();
                    o.ObjetoId = ObjetoId;
                    o.ObjetoDesc = ObjetoId;
                    o.ObjetoMenu = false;

                    db.Objeto.Add(o);
                    db.SaveChanges();
                }
                /*Crear el objeto si no existe en la BD*/
            }


            return Result;
        }


        public int generaConsecutivo(string ConsecutivoId = null)
        {
            int idx = 0;
            try
            {
                var consecutivo = db.Consecutivo
                                    .Where(c => c.ConsecutivoId.Trim().ToLower() == ConsecutivoId.Trim().ToLower())
                                    .FirstOrDefault();
                if (consecutivo == null)
                {
                    Consecutivo nConsecutivo = new Consecutivo();
                    nConsecutivo.ConsecutivoId = ConsecutivoId;
                    nConsecutivo.ConsecutivoNro = 1;
                    db.Consecutivo.Add(nConsecutivo);
                    db.SaveChanges();
                    idx = 1;
                }
                else
                {
                    idx = consecutivo.ConsecutivoNro + 1;
                    consecutivo.ConsecutivoNro = idx;
                    db.Entry(consecutivo).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch
            {
                idx = 0;
            }

            return idx;
        }



    }
}