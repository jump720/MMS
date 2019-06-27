using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Security;
using System.Configuration;

namespace MMS.Models
{
    public class MMSContext : DbContext
    {
        //private object po;
#if DEBUG
        public MMSContext() : base("MMS")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }
#else
        public MMSContext() : base(Crypto.AESGCM.SimpleDecryptWithPassword(ConfigurationManager.ConnectionStrings["MMS"].ConnectionString, "4p3xT00lsGroup"))
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }
#endif

        #region Catálogos

        public DbSet<Canal> Canales { get; set; }
        public DbSet<TipoActividad> TipoActividades { get; set; }
        public DbSet<TipoGasto> TipoGastos { get; set; }
        public DbSet<TipoProducto> TipoProductos { get; set; }
        public DbSet<CentroCosto> CentroCostos { get; set; }
        public DbSet<TipoMovimiento> TipoMovimientos { get; set; }
        public DbSet<Pais> Pais { get; set; }
        public DbSet<Departamento> Departamento { get; set; }
        public DbSet<Ciudad> Ciudad { get; set; }
        public DbSet<Area> Area { get; set; }
        public DbSet<Auditoria> Auditoria { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RolUsuario> RolUsuario { get; set; }
        public DbSet<Objeto> Objeto { get; set; }
        public DbSet<RolObjeto> RolObjeto { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Evento> Evento { get; set; }
        public DbSet<Marca> Marca { get; set; }
        public DbSet<Item> Item { get; set; }
        public DbSet<Plantilla> Plantilla { get; set; }
        public DbSet<PlantillaItem> PlantillaItem { get; set; }
        public DbSet<UsuarioToken> UsuarioToken { get; set; }
        public DbSet<Visita> Visita { get; set; }
        public DbSet<VisitaFoto> VisitaFoto { get; set; }
        public DbSet<VisitaItem> VisitaItem { get; set; }
        public DbSet<VisitaCliente> VisitaCliente { get; set; }
        public DbSet<VisitaPublicidad> VisitaPublicidad { get; set; }
        public DbSet<CategoriaCDE> CategoriaCDE { get; set; }
        public DbSet<ColeccionPIV> ColeccionPIV { get; set; }
        public DbSet<ColeccionPIVItem> ColeccionPIVItem { get; set; }
        public DbSet<Asesor> Asesor { get; set; }
        public DbSet<Liquidacion> Liquidacion { get; set; }
        public DbSet<LiquidacionArchivo> LiquidacionArchivo { get; set; }
        public DbSet<LiquidacionAsesor> LiquidacionAsesor { get; set; }
        public DbSet<LiquidacionItem> LiquidacionItem { get; set; }
        public DbSet<LiquidacionAprobacion> LiquidacionAprobacion { get; set; }
        public DbSet<LiquidacionCierre> LiquidacionCierre { get; set; }
        public DbSet<Aplicacion> Aplicaciones { get; set; }
        public DbSet<RolAplicacion> RolAplicaciones { get; set; }
        public DbSet<AplicacionObjeto> AplicacionObjetos { get; set; }
        public DbSet<Devolucion> Devoluciones { get; set; }
        public DbSet<DevolucionItem> DevolucionItems { get; set; }
        public DbSet<ItemDisponibilidad> ItemDisponibilidad { get; set; }
        public DbSet<Regla> Regla { get; set; }
        public DbSet<DevolucionArchivo> DevolucionArchivos { get; set; }
        public DbSet<MotivoPQRS> MotivosPQRS { get; set; }
        public DbSet<Garantia> Garantias { get; set; }
        public DbSet<Recruitment> Recruitments { get; set; }
        public DbSet<GarantiaItem> GarantiaItems { get; set; }
        public DbSet<GarantiaArchivo> GarantiaArchivos { get; set; }
        public DbSet<DisponibilidadArchivo> DisponibilidadArchivo { get; set; }
        public DbSet<DisponibilidadArchivoItem> DisponibilidadArchivoItem { get; set; }
        public DbSet<Novedad> Novedad { get; set; }
        public DbSet<NovedadItem> NovedadItem { get; set; }
        public DbSet<NovedadArchivo> NovedadArchivo { get; set; }
        public DbSet<FlujoPQRS> FlujosPQRS { get; set; }
        public DbSet<UsuarioFlujoPQRS> UsuarioFlujoPQRS { get; set; }
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<PeriodoRevision> PeriodoRevision { get; set; }
        public DbSet<CoreValue> CoreValue { get; set; }
        public DbSet<Peak> Peak { get; set; }
        public DbSet<PeakObjetivo> PeakObjetivo { get; set; }
        public DbSet<PeakObjetivoRevision> PeakObjetivoRevision { get; set; }
        public DbSet<PeakRevision> PeakRevision { get; set; }
        public DbSet<PeakCoreValue> PeakCoreValue { get; set; }
        public DbSet<PeakPlanDesarrollo> PeakPlanDesarrollo { get; set; }
        public DbSet<PQRSRecord> PQRSRecords { get; set; }
        public DbSet<PQRSRecordUsuario> PQRSRecordUsuarios { get; set; }
        public DbSet<PQRSRecordComentario> PQRSRecordComentarios { get; set; }
        public DbSet<PQRSRecordArchivo> PQRSRecordArchivos { get; set; }
        public DbSet<PQRSRecordDocumento> PQRSRecordDocumentos { get; set; }
        public DbSet<TipoDocSoporte> TipoDocSoporte { get; set; }
        public DbSet<UsuarioHV> UsuarioHV { get; set; }
        public DbSet<CausaPQRS> CausaPQRS { get; set; }
        public DbSet<FlujoPQRSTareas> FlujoPQRSTareas { get; set; }
        public DbSet<PQRSRecordTareas> PQRSRecordTareas { get; set; }
        public DbSet<FlujoPQRSCondiciones> FlujoPQRSCondiciones { get; set; }
        public DbSet<PQRSRecordCondiciones> PQRSRecordCondiciones { get; set; }
        public DbSet<TipoVisita> TipoVisitas { get; set; }
        public DbSet<TipoIndustria> TipoIndustrias { get; set; }
        public DbSet<Correos> Correos { get; set; }
        public DbSet<UsuarioPlanta> UsuarioPlanta { get; set; }
        public DbSet<UsuarioCanal> UsuarioCanal { get; set; }
        #endregion

        #region Transacciones
        public DbSet<Movimiento> Movimiento { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Orden> Orden { get; set; }
        public DbSet<Actividad> Actividad { get; set; }
        public DbSet<ActividadAutorizacion> ActividadAutorizacion { get; set; }
        public DbSet<ActividadItem> ActividadItem { get; set; }
        //public DbSet<ActividadAnexos> ActividadAnexos { get; set; }
        public DbSet<Presupuesto> Presupuesto { get; set; }
        public DbSet<PresupuestoVendedor> PresupuestoVendedor { get; set; }
        public DbSet<VentasxCliente> VentasxCliente { get; set; }
        public DbSet<Plantas> Plantas { get; set; }
        public DbSet<Gasto> Gasto { get; set; }
        public DbSet<Configuracion> Configuracion { get; set; }
        public DbSet<Consecutivo> Consecutivo { get; set; }
        public DbSet<OrdenItems> OrdenItems { get; set; }
        public DbSet<ActividadArchivo> ActividadArchivo { get; set; }
        public DbSet<NivelesAprobacion> NivelesAprobacion { get; set; }
        #endregion


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            #region Configuración LLaves

            /*Llave primaria Departamento*/
            modelBuilder.Entity<Departamento>().HasKey(d => new { d.DepartamentoID, d.PaisID });

            /*Relación de uno a muchos entre pais y departamento*/
            modelBuilder.Entity<Departamento>().HasRequired<Pais>(d => d.paises)
                .WithMany(p => p.departamentoList)
                .HasForeignKey(d => d.PaisID).WillCascadeOnDelete(false);

            /*Llave primaria Ciudad*/
            modelBuilder.Entity<Ciudad>().HasKey(c => new { c.CiudadID, c.DepartamentoID, c.PaisID });

            /*Relación de uno a muchos entre Departamento y Ciudad*/
            modelBuilder.Entity<Ciudad>().HasRequired<Departamento>(c => c.departamentos)
                .WithMany(d => d.ciudadList)
                .HasForeignKey(c => new { c.DepartamentoID, c.PaisID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Usuario y ella misma :D*/
            modelBuilder.Entity<Usuario>().HasOptional<Usuario>(u => u.UsuarioPadre)
                .WithMany(up => up.UsuariosList)
                .HasForeignKey(u => u.UsuarioPadreId).WillCascadeOnDelete(false);
            
            /*Relação de um entre muitos de Usuários e Planta*/
            modelBuilder.Entity<Usuario>().HasOptional<Plantas>(p => p.planta)
                         .WithMany(u => u.Usuario)
                         .HasForeignKey(p => new { p.PlantaID }).WillCascadeOnDelete(false);

            /*Relación de uno a uno entre Usuario y UsuarioHV */
            modelBuilder.Entity<Usuario>().HasOptional<UsuarioHV>(u => u.UsuarioHV)
                .WithRequired(uhv => uhv.Usuario)
                .WillCascadeOnDelete(false);

            /*Llave primaria RolUsuario*/
            modelBuilder.Entity<RolUsuario>().HasKey(ru => new { ru.UsuarioId, ru.RolId });

            /*Relación de uno a muchos entre Usuario y RolUsuario :D*/
            modelBuilder.Entity<RolUsuario>().HasRequired<Usuario>(ru => ru.Usuario)
                .WithMany(u => u.RolUsuarioList)
                .HasForeignKey(ru => ru.UsuarioId).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Rol y RolUsuario :D*/
            modelBuilder.Entity<RolUsuario>().HasRequired<Rol>(ru => ru.Rol)
                .WithMany(r => r.RolUsuarioList)
                .HasForeignKey(ru => ru.RolId).WillCascadeOnDelete(false);

            /*Llave primaria Presupuesto*/
            modelBuilder.Entity<Presupuesto>().HasKey(p => new { p.PresupuestoAno, p.PresupuestoMes });




            /*Llave primaria ActividadAutorizacion*/
            modelBuilder.Entity<ActividadAutorizacion>().HasKey(a => new { a.ActividadId, a.UsuarioIdAutoriza });

            ///*Llave primaria ActividadAnexos*/
            //modelBuilder.Entity<ActividadAnexos>().HasKey(a => new { a.ActividadId, a.ActividadAnexosId });



            /*Relación de uno a muchos entre Objeto y Objeto*/
            modelBuilder.Entity<Objeto>().HasOptional<Objeto>(o => o.ObjetoPadre)
                .WithMany(o => o.ObjetoList)
                .HasForeignKey(o => o.ObjetoIdPadre).WillCascadeOnDelete(false);


            /*Llave primaria RolObjeto*/
            modelBuilder.Entity<RolObjeto>().HasKey(ro => new { ro.RolId, ro.ObjetoId });


            /*Relación de uno a muchos entre Rol y RolObjeto :D*/
            modelBuilder.Entity<RolObjeto>().HasRequired<Rol>(ro => ro.Rol)
                .WithMany(r => r.RolObjetoList)
                .HasForeignKey(ro => ro.RolId).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Objeto y RolObjeto :D*/
            modelBuilder.Entity<RolObjeto>().HasRequired<Objeto>(ro => ro.Objeto)
                .WithMany(o => o.RolObjetoList)
                .HasForeignKey(ro => ro.ObjetoId).WillCascadeOnDelete(false);


            /*Llave primaria RolObjeto*/
            modelBuilder.Entity<RolObjeto>().HasKey(ro => new { ro.RolId, ro.ObjetoId });


            /*Relación de uno a muchos entre Rol y RolObjeto :D*/
            modelBuilder.Entity<RolObjeto>().HasRequired<Rol>(ro => ro.Rol)
                .WithMany(r => r.RolObjetoList)
                .HasForeignKey(ro => ro.RolId).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Objeto y RolObjeto :D*/
            modelBuilder.Entity<RolObjeto>().HasRequired<Objeto>(ro => ro.Objeto)
                .WithMany(o => o.RolObjetoList)
                .HasForeignKey(ro => ro.ObjetoId).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Tipo producto y Producto*/
            modelBuilder.Entity<Producto>().HasRequired<TipoProducto>(p => p.tipoProducto)
                .WithMany(p => p.ProductoList)
                .HasForeignKey(p => new { p.TipoProductoID }).WillCascadeOnDelete(false);


            ///*Relación de uno a muchos entre Actividad y los Anexos de la actividad*/
            //modelBuilder.Entity<ActividadAnexos>().HasRequired<Actividad>(a => a.Actividad)
            //    .WithMany(b => b.ActividadAnexosList)
            //    .HasForeignKey(c => new { c.ActividadId }).WillCascadeOnDelete(false);



            /*Relación de uno a muchos entre Actividad y los items de la actividad*/
            modelBuilder.Entity<ActividadAutorizacion>().HasRequired<Actividad>(a => a.Actividad)
                .WithMany(b => b.ActividadAutorizacionesList)
                .HasForeignKey(c => new { c.ActividadId }).WillCascadeOnDelete(false);


            /*Relación de uno a muchos entre Actividad y los items de la actividad*/
            modelBuilder.Entity<ActividadAutorizacion>().HasRequired<Usuario>(a => a.usuario)
                .WithMany(b => b.ActividadAutorizacionesList)
                .HasForeignKey(c => new { c.UsuarioIdAutoriza }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Tipo Actividad y Actividad*/
            modelBuilder.Entity<Actividad>().HasRequired<TipoActividad>(p => p.tipoActividad)
                .WithMany(p => p.ActividadList)
                .HasForeignKey(p => new { p.TipoActividadID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Canal y Actividad*/
            modelBuilder.Entity<Actividad>().HasRequired<Canal>(p => p.canal)
                .WithMany(p => p.ActividadList)
                .HasForeignKey(p => new { p.CanalID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Actividad y Plantas*/
            modelBuilder.Entity<Actividad>().HasOptional<Plantas>(a => a.planta)
                .WithMany(p => p.Actividades)
                .HasForeignKey(p => new { p.PlantaID }).WillCascadeOnDelete(false);

            /*Chave Primária para Planta*/ // Cayo Diebe (09/11/2018)
            modelBuilder.Entity<Plantas>().HasKey(c => new { c.PlantaID });

            /*Llave primaria Cliente*/
            modelBuilder.Entity<Cliente>().HasKey(c => new { c.ClienteID });

            /*Relación de uno a muchos entre ciudad y cliente*/
            modelBuilder.Entity<Cliente>().HasRequired<Ciudad>(c => c.ciudad)
                .WithMany(ci => ci.ClienteList)
                .HasForeignKey(c => new { c.CiudadID, c.DepartamentoID, c.PaisID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre usuario y cliente*/
            modelBuilder.Entity<Cliente>().HasRequired<Usuario>(c => c.usuario)
                .WithMany(u => u.ClienteList)
                .HasForeignKey(c => new { c.VendedorId }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre canal y cliente*/
            modelBuilder.Entity<Cliente>().HasOptional<Canal>(c => c.canal)
                .WithMany(c => c.ClienteList)
                .HasForeignKey(c => new { c.CanalID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Actividad y Plantas*/
            modelBuilder.Entity<Cliente>().HasOptional<Plantas>(c => c.planta)
                .WithMany(p => p.Clientes)
                .HasForeignKey(p => new { p.PlantaID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre colecccionPIV y cliente*/
            modelBuilder.Entity<Cliente>().HasOptional<ColeccionPIV>(c => c.ColeccionPIV)
                .WithMany(c => c.Clientes)
                .HasForeignKey(c => new { c.ColeccionPIVId }).WillCascadeOnDelete(false);

            /*Llave primaria Orden*/
            modelBuilder.Entity<Orden>().HasKey(o => new { o.OrdenId });

            /*Relación de uno a muchos entre Orden y Usuario*/
            modelBuilder.Entity<Orden>().HasRequired<Usuario>(o => o.usuario)
                .WithMany(u => u.OrdenList)
                .HasForeignKey(o => new { o.UsuarioIdModifica }).WillCascadeOnDelete(false);


            /*Relación de uno a muchos entre Orden y Actividad*/
            modelBuilder.Entity<Orden>().HasRequired<Actividad>(o => o.actividad)
                .WithMany(a => a.OrdenList)
                .HasForeignKey(o => new { o.ActividadId }).WillCascadeOnDelete(false);

            /*Llave primaria Movimiento*/
            modelBuilder.Entity<Movimiento>().HasKey(m => new { m.MovimientoId, m.MovimientoLinea });


            /*Relación de uno a muchos entre Movimiento y Producto*/
            modelBuilder.Entity<Movimiento>().HasRequired<Producto>(m => m.producto)
                .WithMany(p => p.MovimientoList)
                .HasForeignKey(m => new { m.ProductoId }).WillCascadeOnDelete(false);



            /*Relación de uno a muchos entre Actividad y Cliente*/
            modelBuilder.Entity<Actividad>().HasRequired<Cliente>(A => A.cliente)
                .WithMany(c => c.ActividadList)
                .HasForeignKey(A => new { A.ClienteID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Movimiento y TipoMovimiento*/
            modelBuilder.Entity<Movimiento>().HasRequired<TipoMovimiento>(m => m.tipoMovimiento)
                .WithMany(t => t.MovimientoList)
                .HasForeignKey(m => new { m.TipoMovimientoID }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Movimiento y Usuario*/
            modelBuilder.Entity<Movimiento>().HasRequired<Usuario>(m => m.usuario)
                .WithMany(t => t.MovimientoList)
                .HasForeignKey(m => new { m.UsuarioIdModifica }).WillCascadeOnDelete(false);



            /*Llave primaria VentasxCliente*/
            modelBuilder.Entity<VentasxCliente>()
                .HasKey(v => new { v.VentasxClienteAno, v.VentasxClienteMes, v.ClienteID });

            /*Relación de uno a muchos entre VentasxCliente y Cliente*/
            modelBuilder.Entity<VentasxCliente>().HasRequired<Cliente>(v => v.cliente)
                .WithMany(c => c.VentasxClienteList)
                .HasForeignKey(v => new { v.ClienteID }).WillCascadeOnDelete(false);


            #endregion

            // TT

            #region Log

            modelBuilder.Entity<Log>().HasKey(l => new { l.Id });

            modelBuilder.Entity<Log>().HasRequired<Evento>(l => l.Evento)
                .WithMany(e => e.Logs)
                .HasForeignKey(l => new { l.EventoId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Evento

            modelBuilder.Entity<Evento>().HasKey(e => new { e.Id });

            #endregion

            #region Marca

            modelBuilder.Entity<Marca>().HasKey(m => new { m.Id });

            #endregion

            #region Item

            modelBuilder.Entity<Item>().HasKey(i => new { i.Id });

            modelBuilder.Entity<Item>().HasRequired<Marca>(i => i.Marca)
                .WithMany(m => m.Items)
                .HasForeignKey(i => new { i.MarcaId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Plantilla

            modelBuilder.Entity<Plantilla>().HasKey(p => new { p.Id });

            #endregion

            #region PlantillaItem

            modelBuilder.Entity<PlantillaItem>().HasKey(pi => new { pi.PlantillaId, pi.ItemId });

            modelBuilder.Entity<PlantillaItem>().HasRequired<Plantilla>(pi => pi.Plantilla)
                .WithMany(p => p.PlantillaItems)
                .HasForeignKey(pi => new { pi.PlantillaId });

            modelBuilder.Entity<PlantillaItem>().HasRequired<Item>(pi => pi.Item)
                .WithMany(i => i.ItemPlantillas)
                .HasForeignKey(pi => new { pi.ItemId })
                .WillCascadeOnDelete(false);

            #endregion

            #region UsuarioToken

            modelBuilder.Entity<UsuarioToken>().HasKey(i => new { i.Id });

            modelBuilder.Entity<UsuarioToken>().HasRequired<Usuario>(ut => ut.Usuario)
                .WithMany(u => u.UsuarioTokensList)
                .HasForeignKey(ut => ut.UsuarioId);

            #endregion

            #region Visita

            modelBuilder.Entity<Visita>().HasKey(v => new { v.Id });

            modelBuilder.Entity<Visita>().HasRequired<Ciudad>(v => v.Ciudad)
                .WithMany(c => c.Visitas)
                .HasForeignKey(v => new { v.CiudadId, v.DepartamentoId, v.PaisId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Visita>().HasRequired<Usuario>(v => v.Usuario)
                .WithMany(u => u.VisitasList)
                .HasForeignKey(v => v.UsuarioId)
                .WillCascadeOnDelete(false);


            modelBuilder.Entity<Visita>().HasOptional<TipoVisita>(v => v.TipoVisita)
                .WithMany(tv => tv.VisitasList)
                .HasForeignKey(v => v.TipoVisitaId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Visita>().HasOptional<TipoIndustria>(v => v.TipoIndustria)
                .WithMany(ti => ti.VisitasList)
                .HasForeignKey(v => v.TipoIndustriaId)
                .WillCascadeOnDelete(false);

            #endregion

            #region VisitaFoto

            modelBuilder.Entity<VisitaFoto>().HasKey(vf => new { vf.VisitaId, vf.Order });

            modelBuilder.Entity<VisitaFoto>().HasRequired<Visita>(vf => vf.Visita)
                .WithMany(v => v.VisitaFotos)
                .HasForeignKey(vf => vf.VisitaId);

            #endregion

            #region VisitaItem

            modelBuilder.Entity<VisitaItem>().HasKey(vi => new { vi.VisitaId, vi.ItemId });

            modelBuilder.Entity<VisitaItem>().HasRequired<Visita>(vi => vi.Visita)
                .WithMany(v => v.VisitaItems)
                .HasForeignKey(vi => vi.VisitaId);

            modelBuilder.Entity<VisitaItem>().HasRequired<Item>(vi => vi.Item)
                .WithMany(i => i.ItemVisitas)
                .HasForeignKey(vi => vi.ItemId)
                .WillCascadeOnDelete(false);

            #endregion

            #region VisitaCliente

            modelBuilder.Entity<VisitaCliente>().HasKey(vc => new { vc.VisitaId, vc.ClienteId });

            modelBuilder.Entity<VisitaCliente>().HasRequired<Visita>(vc => vc.Visita)
                .WithMany(v => v.VisitaClientes)
                .HasForeignKey(vc => vc.VisitaId);

            modelBuilder.Entity<VisitaCliente>().HasRequired<Cliente>(vi => vi.Cliente)
                .WithMany(c => c.VisitasClientes)
                .HasForeignKey(vc => vc.ClienteId)
                .WillCascadeOnDelete(false);

            #endregion

            #region VisitaPublicidad

            modelBuilder.Entity<VisitaPublicidad>().HasKey(vp => new { vp.VisitaId, vp.Order });

            modelBuilder.Entity<VisitaPublicidad>().HasRequired<Visita>(vp => vp.Visita)
                .WithMany(v => v.VisitaPublicidades)
                .HasForeignKey(vp => vp.VisitaId);

            modelBuilder.Entity<VisitaPublicidad>().HasRequired<Marca>(vp => vp.Marca)
                .WithMany(v => v.VisitaPublicidades)
                .HasForeignKey(vp => vp.MarcaId)
                .WillCascadeOnDelete(false);

            #endregion

            #region CategoriaCDE

            modelBuilder.Entity<CategoriaCDE>().HasKey(c => new { c.Id });

            modelBuilder.Entity<CategoriaCDE>().HasOptional<Liquidacion>(c => c.Liquidacion)
                .WithMany(l => l.CategoriasCDE)
                .HasForeignKey(c => c.LiquidacionId)
                .WillCascadeOnDelete(false);

            #endregion

            #region ColeccionPIV

            modelBuilder.Entity<ColeccionPIV>().HasKey(c => new { c.Id });

            #endregion

            #region ColeccionPIVItem

            modelBuilder.Entity<ColeccionPIVItem>().HasKey(ci => new { ci.Id });

            modelBuilder.Entity<ColeccionPIVItem>().HasRequired<ColeccionPIV>(ci => ci.ColeccionPIV)
                .WithMany(c => c.ColeccionPIVItems)
                .HasForeignKey(ci => new { ci.ColeccionPIVId });

            modelBuilder.Entity<ColeccionPIVItem>().HasRequired<Item>(ci => ci.Item)
                .WithMany(i => i.ItemColeccionesPIV)
                .HasForeignKey(ci => new { ci.ItemId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Asesor

            modelBuilder.Entity<Asesor>().HasKey(a => new { a.Id });

            modelBuilder.Entity<Asesor>().HasRequired<ColeccionPIV>(a => a.ColeccionPIV)
                .WithMany(c => c.Asesores)
                .HasForeignKey(a => new { a.ColeccionPIVId });

            modelBuilder.Entity<Asesor>().HasRequired<Ciudad>(a => a.Ciudad)
                .WithMany(c => c.Asesores)
                .HasForeignKey(a => new { a.CiudadId, a.DepartamentoId, a.PaisId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Liquidacion

            modelBuilder.Entity<Liquidacion>().HasKey(l => new { l.Id });

            #endregion

            #region LiquidacionArchivo

            modelBuilder.Entity<LiquidacionArchivo>().HasKey(la => new { la.Id });

            modelBuilder.Entity<LiquidacionArchivo>().HasRequired<Liquidacion>(la => la.Liquidacion)
                .WithMany(c => c.LiquidacionArchivos)
                .HasForeignKey(la => new { la.LiquidacionId });

            modelBuilder.Entity<LiquidacionArchivo>().HasRequired<Usuario>(la => la.Usuario)
                .WithMany(u => u.LiquidacionArchivos)
                .HasForeignKey(la => new { la.UsuarioId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LiquidacionArchivo>().HasRequired<ColeccionPIV>(la => la.ColeccionPIV)
                .WithMany(c => c.LiquidacionArchivos)
                .HasForeignKey(la => new { la.ColeccionPIVId })
                .WillCascadeOnDelete(false);

            #endregion

            #region LiquidacionAsesor

            modelBuilder.Entity<LiquidacionAsesor>().HasKey(la => new { la.Id });

            modelBuilder.Entity<LiquidacionAsesor>().HasRequired<LiquidacionArchivo>(la => la.LiquidacionArchivo)
                .WithMany(la => la.LiquidacionAsesores)
                .HasForeignKey(la => new { la.LiquidacionArchivoId });

            modelBuilder.Entity<LiquidacionAsesor>().HasOptional<Asesor>(la => la.Asesor)
                .WithMany(c => c.LiquidacionAsesores)
                .HasForeignKey(la => new { la.AsesorId })
                .WillCascadeOnDelete(false);

            #endregion

            #region LiquidacionItem

            modelBuilder.Entity<LiquidacionItem>().HasKey(li => new { li.Id });

            modelBuilder.Entity<LiquidacionItem>().HasRequired<LiquidacionAsesor>(li => li.LiquidacionAsesor)
                .WithMany(la => la.LiquidacionItems)
                .HasForeignKey(li => new { li.LiquidacionAsesorId });

            modelBuilder.Entity<LiquidacionItem>().HasRequired<ColeccionPIVItem>(li => li.ColeccionPIVItem)
                .WithMany(ci => ci.LiquidacionItems)
                .HasForeignKey(la => new { la.ColeccionPIVItemId })
                .WillCascadeOnDelete(false);

            #endregion

            #region LiquidacionAprobacion

            modelBuilder.Entity<LiquidacionAprobacion>().HasKey(la => new { la.Id });

            modelBuilder.Entity<LiquidacionAprobacion>().HasRequired<Liquidacion>(la => la.Liquidacion)
                .WithMany(l => l.LiquidacionAprobaciones)
                .HasForeignKey(la => new { la.LiquidacionId });

            modelBuilder.Entity<LiquidacionAprobacion>().HasRequired<Asesor>(la => la.Asesor)
                .WithMany(a => a.LiquidacionAprobaciones)
                .HasForeignKey(la => new { la.AsesorId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LiquidacionAprobacion>().HasRequired<Usuario>(la => la.Usuario)
                .WithMany(u => u.LiquidacionAprobaciones)
                .HasForeignKey(la => new { la.UsuarioId })
                .WillCascadeOnDelete(false);

            #endregion

            #region LiquidacionCierre

            modelBuilder.Entity<LiquidacionCierre>().HasKey(lc => new { lc.Id });

            modelBuilder.Entity<LiquidacionCierre>().HasRequired<Liquidacion>(lc => lc.Liquidacion)
                .WithMany(l => l.LiquidacionCierres)
                .HasForeignKey(la => new { la.LiquidacionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LiquidacionCierre>().HasRequired<Asesor>(lc => lc.Asesor)
                .WithMany(la => la.LiquidacionCierres)
                .HasForeignKey(la => new { la.AsesorId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LiquidacionCierre>().HasOptional<CategoriaCDE>(lc => lc.CategoriaCDE)
                .WithMany(c => c.LiquidacionCierres)
                .HasForeignKey(la => new { la.CategoriaCDEId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Aplicacion
            modelBuilder.Entity<Aplicacion>().HasKey(a => new { a.Id });
            #endregion

            #region RolAplicacion
            modelBuilder.Entity<RolAplicacion>().HasKey(ra => new { ra.RolId, ra.AplicacionId });

            modelBuilder.Entity<RolAplicacion>().HasRequired<Rol>(ra => ra.Rol)
               .WithMany(r => r.RolAplicaciones)
               .HasForeignKey(ra => new { ra.RolId })
               .WillCascadeOnDelete(false);


            modelBuilder.Entity<RolAplicacion>().HasRequired<Aplicacion>(ra => ra.Aplicacion)
               .WithMany(a => a.RolAplicaciones)
               .HasForeignKey(ra => new { ra.AplicacionId })
               .WillCascadeOnDelete(false);
            #endregion

            #region AplicacionObjeto
            modelBuilder.Entity<AplicacionObjeto>().HasKey(ra => new { ra.AplicacionId, ra.ObjetoId });

            modelBuilder.Entity<AplicacionObjeto>().HasRequired<Aplicacion>(ao => ao.Aplicacion)
               .WithMany(a => a.AplicacionObjetos)
               .HasForeignKey(ao => new { ao.AplicacionId });

            modelBuilder.Entity<AplicacionObjeto>().HasRequired<Objeto>(ao => ao.Objeto)
               .WithMany(a => a.AplicacionObjetos)
               .HasForeignKey(ao => new { ao.ObjetoId })
               .WillCascadeOnDelete(false);
            #endregion

            //#region TipoDevolucion
            //modelBuilder.Entity<TipoDevolucion>().HasKey(td => new { td.Id });

            //#endregion

            #region Devolucion
            modelBuilder.Entity<Devolucion>().HasKey(d => new { d.Id });



            modelBuilder.Entity<Devolucion>().HasRequired<Usuario>(d => d.UsuarioCreacion)
             .WithMany(uc => uc.DevolucionesUsuario)
             .HasForeignKey(d => new { d.UsuarioIdCreacion })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Devolucion>().HasOptional<Usuario>(d => d.Analista)
            .WithMany(ua => ua.DevolucionesAnalista)
            .HasForeignKey(d => new { d.AnalistaId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<Devolucion>().HasRequired<Cliente>(d => d.Cliente)
            .WithMany(c => c.Devoluciones)
            .HasForeignKey(d => new { d.ClienteId })
            .WillCascadeOnDelete(false);
            #endregion

            #region DevolucionItem

            modelBuilder.Entity<DevolucionItem>().HasKey(di => new { di.DevolucionId, di.Id });

            modelBuilder.Entity<DevolucionItem>().HasRequired<Devolucion>(di => di.Devolucion)
            .WithMany(d => d.DevolucionItems)
            .HasForeignKey(di => new { di.DevolucionId })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<DevolucionItem>().HasRequired<MotivoPQRS>(di => di.MotivoPQRS)
            .WithMany(m => m.DevolucionItems)
            .HasForeignKey(di => new { di.MotivoPQRSId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<DevolucionItem>().HasRequired<Item>(di => di.Items)
           .WithMany(i => i.DevolucionItems)
           .HasForeignKey(di => new { di.ItemId })
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<DevolucionItem>().HasOptional<CausaPQRS>(di => di.CausaPQRS)
            .WithMany(c => c.DevolucionItems)
            .HasForeignKey(di => new { di.CausaPQRSId })
            .WillCascadeOnDelete(false);

            #endregion

            #region ItemDisponibilidad

            modelBuilder.Entity<ItemDisponibilidad>().HasKey(id => new { id.ColeccionPIVId, id.ItemId });

            modelBuilder.Entity<ItemDisponibilidad>().HasRequired<ColeccionPIV>(id => id.ColeccionPIV)
                .WithMany(c => c.ItemDisponibilidades)
                .HasForeignKey(id => new { id.ColeccionPIVId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ItemDisponibilidad>().HasRequired<Item>(id => id.Item)
                .WithMany(i => i.ItemDisponibilidades)
                .HasForeignKey(id => new { id.ItemId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Regla

            modelBuilder.Entity<Regla>().HasKey(r => new { r.Id });

            modelBuilder.Entity<Regla>().HasOptional<Marca>(r => r.Marca)
                .WithMany(m => m.Reglas)
                .HasForeignKey(r => new { r.MarcaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Regla>().HasOptional<Item>(r => r.Item)
                .WithMany(i => i.Reglas)
                .HasForeignKey(r => new { r.ItemId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Regla>().HasOptional<Liquidacion>(r => r.Liquidacion)
                .WithMany(l => l.Reglas)
                .HasForeignKey(r => new { r.LiquidacionId })
                .WillCascadeOnDelete(false);

            #endregion

            #region DevolucionArchivo

            modelBuilder.Entity<DevolucionArchivo>().HasKey(da => new { da.DevolucionId, da.Order });

            modelBuilder.Entity<DevolucionArchivo>().HasRequired<Devolucion>(da => da.Devolucion)
                .WithMany(d => d.DevolucionArchivos)
                .HasForeignKey(da => da.DevolucionId);

            #endregion

            #region MotivoPQRS
            modelBuilder.Entity<MotivoPQRS>().HasKey(m => new { m.Id });

            #endregion

            #region Garantia
            modelBuilder.Entity<Garantia>().HasKey(g => new { g.Id });



            modelBuilder.Entity<Garantia>().HasRequired<Usuario>(g => g.UsuarioCreacion)
             .WithMany(uc => uc.GarantiasUsuario)
             .HasForeignKey(g => new { g.UsuarioIdCreacion })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Garantia>().HasOptional<Usuario>(g => g.Analista)
            .WithMany(ua => ua.GarantiasAnalista)
            .HasForeignKey(g => new { g.AnalistaId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<Garantia>().HasRequired<Cliente>(g => g.Cliente)
            .WithMany(c => c.Garantias)
            .HasForeignKey(g => new { g.ClienteId })
            .WillCascadeOnDelete(false);
            #endregion

            #region Recruitment
            modelBuilder.Entity<Recruitment>().HasKey(r => new { r.RecruitmentId });



            modelBuilder.Entity<Recruitment>().HasRequired<Area>(r => r.Department)
             .WithMany(rd => rd.Recluiments)
             .HasForeignKey(r => new { r.DepartmentId })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasOptional<Area>(r => r.ProposedDepartment)
             .WithMany(rp => rp.ProposedRecluiments)
             .HasForeignKey(r => new { r.ProposedDepartmentId })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<CentroCosto>(r => r.CentroCosto)
             .WithMany(rc => rc.Recruitments)
             .HasForeignKey(r => new { r.CentroCostoID })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasOptional<CentroCosto>(r => r.ProposedCostCenter)
             .WithMany(rp => rp.ProposedRecruitments)
             .HasForeignKey(r => new { r.ProposedCostCenterID })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<Usuario>(r => r.UsuarioSubstitute)
             .WithMany(ru => ru.RecruitmentsUS)
             .HasForeignKey(r => new { r.UsuarioIdSubstitute })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<Usuario>(r => r.HumanResources)
             .WithMany(rh => rh.RecruitmentsHR)
             .HasForeignKey(r => new { r.HumanResourcesID })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<Usuario>(r => r.AreaManager)
             .WithMany(ra => ra.RecruitmentsAM)
             .HasForeignKey(r => new { r.AreaManagerID })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<Usuario>(r => r.ImmediateBoss)
             .WithMany(ri => ri.RecruitmentsIB)
             .HasForeignKey(r => new { r.ImmediateBossID })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasOptional<Usuario>(r => r.Analista)
            .WithMany(U => U.RecruitmentsAnalista)
            .HasForeignKey(r => new { r.AnalistaId })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recruitment>().HasRequired<Usuario>(r => r.UsuarioCreacion)
           .WithMany(U => U.RecruitmentsUsuarioCreacion)
           .HasForeignKey(r => new { r.UsuarioIdCreacion })
           .WillCascadeOnDelete(false);

            
            #endregion

            #region GarantiaItem

            modelBuilder.Entity<GarantiaItem>().HasKey(gi => new { gi.GarantiaId, gi.Id });

            modelBuilder.Entity<GarantiaItem>().HasRequired<Garantia>(gi => gi.Garantia)
            .WithMany(g => g.GarantiaItems)
            .HasForeignKey(gi => new { gi.GarantiaId })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<GarantiaItem>().HasRequired<MotivoPQRS>(gi => gi.MotivoPQRS)
            .WithMany(m => m.GarantiaItems)
            .HasForeignKey(gi => new { gi.MotivoPQRSId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<GarantiaItem>().HasRequired<Item>(gi => gi.Items)
           .WithMany(i => i.GarantiaItems)
           .HasForeignKey(gi => new { gi.ItemId })
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<GarantiaItem>().HasOptional<CausaPQRS>(gi => gi.CausaPQRS)
            .WithMany(c => c.GarantiaItems)
            .HasForeignKey(gi => new { gi.CausaPQRSId })
            .WillCascadeOnDelete(false);

            #endregion

            #region GarantiaArchivo

            modelBuilder.Entity<GarantiaArchivo>().HasKey(ga => new { ga.GarantiaId, ga.Order });

            modelBuilder.Entity<GarantiaArchivo>().HasRequired<Garantia>(ga => ga.Garantia)
                .WithMany(d => d.GarantiaArchivos)
                .HasForeignKey(ga => ga.GarantiaId);

            #endregion

            #region DisponibilidadArchivo

            modelBuilder.Entity<DisponibilidadArchivo>().HasKey(da => new { da.Id });

            modelBuilder.Entity<DisponibilidadArchivo>().HasRequired<Usuario>(da => da.Usuario)
                .WithMany(u => u.DisponibilidadArchivos)
                .HasForeignKey(da => new { da.UsuarioId })
                .WillCascadeOnDelete(false);

            #endregion

            #region DisponibilidadArchivoItem

            modelBuilder.Entity<DisponibilidadArchivoItem>().HasKey(dai => new { dai.Id });

            modelBuilder.Entity<DisponibilidadArchivoItem>().HasRequired<DisponibilidadArchivo>(dai => dai.DisponibilidadArchivo)
                .WithMany(da => da.DisponibilidadArchivoItems)
                .HasForeignKey(dai => new { dai.DisponibilidadArchivoId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DisponibilidadArchivoItem>().HasRequired<Item>(dai => dai.Item)
                .WithMany(i => i.DisponibilidadArchivoItems)
                .HasForeignKey(dai => new { dai.ItemId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DisponibilidadArchivoItem>().HasRequired<ColeccionPIV>(dai => dai.ColeccionPIV)
                .WithMany(c => c.DisponibilidadArchivoItems)
                .HasForeignKey(da => new { da.ColeccionPIVId })
                .WillCascadeOnDelete(false);

            #endregion

            #region Novedad
            modelBuilder.Entity<Novedad>().HasKey(g => new { g.Id });



            modelBuilder.Entity<Novedad>().HasRequired<Usuario>(n => n.UsuarioCreacion)
             .WithMany(uc => uc.NovedadesUsuario)
             .HasForeignKey(n => new { n.UsuarioIdCreacion })
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<Novedad>().HasOptional<Usuario>(n => n.Analista)
            .WithMany(ua => ua.NovedadesAnalista)
            .HasForeignKey(n => new { n.AnalistaId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<Novedad>().HasRequired<Cliente>(n => n.Cliente)
            .WithMany(c => c.Novedades)
            .HasForeignKey(n => new { n.ClienteId })
            .WillCascadeOnDelete(false);
            #endregion

            #region NovedadItem

            modelBuilder.Entity<NovedadItem>().HasKey(ni => new { ni.NovedadId, ni.Id });

            modelBuilder.Entity<NovedadItem>().HasRequired<Novedad>(ni => ni.Novedad)
            .WithMany(n => n.NovedadItems)
            .HasForeignKey(ni => new { ni.NovedadId })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<NovedadItem>().HasRequired<MotivoPQRS>(ni => ni.MotivoPQRS)
            .WithMany(m => m.NovedadItems)
            .HasForeignKey(gi => new { gi.MotivoPQRSId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<NovedadItem>().HasRequired<Item>(ni => ni.Items)
           .WithMany(i => i.NovedadItems)
           .HasForeignKey(ni => new { ni.ItemId })
           .WillCascadeOnDelete(false);

            modelBuilder.Entity<NovedadItem>().HasOptional<CausaPQRS>(ni => ni.CausaPQRS)
            .WithMany(c => c.NovedadItems)
            .HasForeignKey(ni => new { ni.CausaPQRSId })
            .WillCascadeOnDelete(false);

            #endregion

            #region NovedadArchivo

            modelBuilder.Entity<NovedadArchivo>().HasKey(na => new { na.NovedadId, na.Order });

            modelBuilder.Entity<NovedadArchivo>().HasRequired<Novedad>(na => na.Novedad)
                .WithMany(g => g.NovedadArchivos)
                .HasForeignKey(na => na.NovedadId)
                .WillCascadeOnDelete(false); ;

            #endregion

            #region FlujoPQRS

            modelBuilder.Entity<FlujoPQRS>().HasKey(f => new { f.MotivoPQRSId, f.Id });

            modelBuilder.Entity<FlujoPQRS>().HasRequired<MotivoPQRS>(f => f.MotivoPQRS)
              .WithMany(m => m.FlujoPQRS)
              .HasForeignKey(f => new { f.MotivoPQRSId })
              .WillCascadeOnDelete(false);

            #endregion

            #region UsuarioFlujoPQRS


            modelBuilder.Entity<UsuarioFlujoPQRS>().HasKey(uf => new { uf.MotivoPQRSId, uf.FlujoPQRSId, uf.UsuarioId });

            modelBuilder.Entity<UsuarioFlujoPQRS>().HasRequired<FlujoPQRS>(uf => uf.FlujoPQRS)
            .WithMany(f => f.UsuarioFlujoPQRS)
            .HasForeignKey(uf => new { uf.MotivoPQRSId, uf.FlujoPQRSId })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<UsuarioFlujoPQRS>().HasRequired<Usuario>(uf => uf.Usuario)
             .WithMany(u => u.UsuarioFlujoPQRS)
             .HasForeignKey(uf => new { uf.UsuarioId })
             .WillCascadeOnDelete(false);

            #endregion

            #region Periodo

            modelBuilder.Entity<Periodo>().HasKey(p => new { p.Id });

            #endregion

            #region PeriodoRevision

            modelBuilder.Entity<PeriodoRevision>().HasKey(pr => new { pr.Id });

            modelBuilder.Entity<PeriodoRevision>().HasRequired<Periodo>(pr => pr.Periodo)
                .WithMany(p => p.PeriodoRevisiones)
                .HasForeignKey(pr => new { pr.PeriodoId });

            #endregion

            #region CoreValue

            modelBuilder.Entity<CoreValue>().HasKey(cv => new { cv.Id });

            #endregion

            #region Peak

            modelBuilder.Entity<Peak>().HasKey(p => new { p.Id });

            modelBuilder.Entity<Peak>().HasRequired<Usuario>(p => p.Usuario)
                .WithMany(u => u.Peaks)
                .HasForeignKey(p => new { p.UsuarioId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Peak>().HasRequired<Periodo>(p => p.Periodo)
                .WithMany(p => p.Peaks)
                .HasForeignKey(p => new { p.PeriodoId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Peak>().HasRequired<Area>(p => p.Area)
                .WithMany(a => a.Peaks)
                .HasForeignKey(p => new { p.AreaId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Peak>().HasOptional<Usuario>(p => p.UsuarioPadre)
                .WithMany(u => u.PeaksPadre)
                .HasForeignKey(p => new { p.UsuarioIdPadre })
                .WillCascadeOnDelete(false);

            #endregion

            #region PeakObjetivo

            modelBuilder.Entity<PeakObjetivo>().HasKey(po => new { po.Id });

            modelBuilder.Entity<PeakObjetivo>().HasRequired<Peak>(po => po.Peak)
                .WithMany(p => p.PeakObjetivos)
                .HasForeignKey(po => new { po.PeakId });

            modelBuilder.Entity<PeakObjetivo>().HasOptional<PeakObjetivo>(po => po.PeakObjetivoHeredado)
                .WithMany(p => p.PeakObjetivosHeredados)
                .HasForeignKey(po => new { po.PeakObjetivoId })
                .WillCascadeOnDelete(false);

            #endregion

            #region PeakObjetivoRevision

            modelBuilder.Entity<PeakObjetivoRevision>().HasKey(por => new { por.Id });

            modelBuilder.Entity<PeakObjetivoRevision>().HasRequired<PeakObjetivo>(por => por.PeakObjetivo)
                .WithMany(po => po.PeakObjetivoRevisiones)
                .HasForeignKey(por => new { por.PeakObjetivoId });

            modelBuilder.Entity<PeakObjetivoRevision>().HasRequired<PeriodoRevision>(por => por.PeriodoRevision)
                .WithMany(pr => pr.PeakObjetivosRevisiones)
                .HasForeignKey(por => new { por.PeriodoRevisionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PeakObjetivoRevision>().HasOptional<Usuario>(por => por.Usuario)
                .WithMany(u => u.PeakObjetivosRevisiones)
                .HasForeignKey(por => new { por.UsuarioId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PeakObjetivoRevision>().HasOptional<PeakObjetivo>(por => por.PeakObjetivoHeredado)
                .WithMany(u => u.PeakObjetivoRevisionesHeredados)
                .HasForeignKey(por => new { por.PeakObjetivoIdHeredado })
                .WillCascadeOnDelete(false);

            #endregion

            #region PeakRevision

            modelBuilder.Entity<PeakRevision>().HasKey(pr => new { pr.Id });

            modelBuilder.Entity<PeakRevision>().HasRequired<Peak>(pr => pr.Peak)
                .WithMany(p => p.PeakRevisiones)
                .HasForeignKey(pr => new { pr.PeakId });

            modelBuilder.Entity<PeakRevision>().HasRequired<PeriodoRevision>(pr => pr.PeriodoRevision)
                .WithMany(pr => pr.PeakRevisiones)
                .HasForeignKey(pr => new { pr.PeriodoRevisionId })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PeakRevision>().HasOptional<Usuario>(pr => pr.Usuario)
               .WithMany(u => u.PeakRevisiones)
               .HasForeignKey(pr => new { pr.UsuarioId })
               .WillCascadeOnDelete(false);

            #endregion

            #region PeakCoreValue

            modelBuilder.Entity<PeakCoreValue>().HasKey(pcv => new { pcv.Id });

            modelBuilder.Entity<PeakCoreValue>().HasRequired<Peak>(pcv => pcv.Peak)
                .WithMany(p => p.PeakCoreValues)
                .HasForeignKey(pcv => new { pcv.PeakId });

            modelBuilder.Entity<PeakCoreValue>().HasRequired<CoreValue>(pcv => pcv.CoreValue)
                .WithMany(cv => cv.PeakCoreValues)
                .HasForeignKey(pcv => new { pcv.CoreValueId })
                .WillCascadeOnDelete(false);

            #endregion

            #region PeakPlanDesarrollo

            modelBuilder.Entity<PeakPlanDesarrollo>().HasKey(ppd => new { ppd.Id });

            modelBuilder.Entity<PeakPlanDesarrollo>().HasRequired<Peak>(ppd => ppd.Peak)
                .WithMany(p => p.PeakPlanesDesarrollo)
                .HasForeignKey(ppd => new { ppd.PeakId });

            #endregion

            #region Area

            modelBuilder.Entity<Area>().HasKey(a => new { a.Id });

            #endregion

            #region PQRSRecord

            modelBuilder.Entity<PQRSRecord>().HasKey(p => new { p.Id, p.Order });

            #endregion

            #region PQRSRecordUsuario

            modelBuilder.Entity<PQRSRecordUsuario>().HasKey(pu => new { pu.PQRSRecordId, pu.PQRSRecordOrder, pu.UsuarioId });

            modelBuilder.Entity<PQRSRecordUsuario>().HasRequired<PQRSRecord>(pu => pu.PQRSRecord)
            .WithMany(p => p.PQRSRecordUsuarios)
            .HasForeignKey(pu => new { pu.PQRSRecordId, pu.PQRSRecordOrder })
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<PQRSRecordUsuario>().HasRequired<Usuario>(pu => pu.Usuario)
           .WithMany(u => u.PQRSRecordUsuarios)
           .HasForeignKey(pu => new { pu.UsuarioId })
           .WillCascadeOnDelete(false);

            #endregion

            #region PQRSRecordComentario

            modelBuilder.Entity<PQRSRecordComentario>().HasKey(pc => new { pc.Id });

            modelBuilder.Entity<PQRSRecordComentario>().HasRequired<Usuario>(pc => pc.Usuario)
              .WithMany(u => u.PQRSRecordComentarios)
              .HasForeignKey(pc => new { pc.UsuarioId })
              .WillCascadeOnDelete(false);

            #endregion

            #region PQRSRecordArchivo

            modelBuilder.Entity<PQRSRecordArchivo>().HasKey(pa => new { pa.PQRSRecordComentarioId, pa.Item });


            modelBuilder.Entity<PQRSRecordArchivo>().HasRequired<PQRSRecordComentario>(pa => pa.PQRSRecordComentario)
            .WithMany(pc => pc.PQRSRecordArchivos)
            .HasForeignKey(pa => new { pa.PQRSRecordComentarioId })
            .WillCascadeOnDelete(false);

            #endregion

            #region TipoDocSoporte

            modelBuilder.Entity<TipoDocSoporte>().HasKey(t => new { t.Id });

            #endregion

            #region PQRSRecordDocumento

            modelBuilder.Entity<PQRSRecordDocumento>().HasKey(pd => new { pd.PQRSRecordComentarioId, pd.Item });


            modelBuilder.Entity<PQRSRecordDocumento>().HasRequired<PQRSRecordComentario>(pd => pd.PQRSRecordComentario)
            .WithMany(pc => pc.PQRSRecordDocumentos)
            .HasForeignKey(pd => new { pd.PQRSRecordComentarioId })
            .WillCascadeOnDelete(false);


            modelBuilder.Entity<PQRSRecordDocumento>().HasRequired<TipoDocSoporte>(pd => pd.TipoDocSoporte)
            .WithMany(pc => pc.PQRSRecordDocumentos)
            .HasForeignKey(pd => new { pd.TipoDocSoporteId })
            .WillCascadeOnDelete(false);

            #endregion

            #region UsuarioHV

            modelBuilder.Entity<UsuarioHV>().HasKey(u => new { u.UsuarioId });

            modelBuilder.Entity<UsuarioHV>().HasOptional<Area>(u => u.Area)
                .WithMany(a => a.UsuarioHVs)
                .HasForeignKey(u => new { u.AreaId })
                .WillCascadeOnDelete(false);

            #endregion

            #region ActividadArchivo

            modelBuilder.Entity<ActividadArchivo>().HasKey(aa => new { aa.ActividadId, aa.Order });

            modelBuilder.Entity<ActividadArchivo>().HasRequired<Actividad>(aa => aa.Actividad)
                .WithMany(a => a.ActividadArchivos)
                .HasForeignKey(aa => aa.ActividadId);

            #endregion

            #region PresupuestoVendedor
            /*Llave primaria PresupuestoVendedor*/
            modelBuilder.Entity<PresupuestoVendedor>()
                .HasKey(p => new { p.PresupuestoVendedorAno, p.CentroCostoID, p.PlantaID, p.CanalID});

            modelBuilder.Entity<PresupuestoVendedor>().HasRequired<Plantas>(p => p.planta)
                          .WithMany(u => u.PresupuestosVendedor)
                          .HasForeignKey(p => new { p.PlantaID }).WillCascadeOnDelete(false);

            modelBuilder.Entity<PresupuestoVendedor>().HasRequired<Canal>(p => p.canal)
                          .WithMany(u => u.PresupuestoVendedor)
                          .HasForeignKey(p => new { p.CanalID }).WillCascadeOnDelete(false);

            modelBuilder.Entity<PresupuestoVendedor>().HasRequired<CentroCosto>(p => p.centroCosto)
               .WithMany(cc => cc.PresupuestoVendedor)
               .HasForeignKey(p => new { p.CentroCostoID }).WillCascadeOnDelete(false);

            /*Relação um a muitas entre Planta e Investimento*/
          


            #endregion

            #region Gasto
            /*Llave primaria Gasto*/
            modelBuilder.Entity<Gasto>().HasKey(g => new { g.GastoId, g.GastoLinea });

            /*Relación de uno a muchos entre Gasto y TipoGasto */
            modelBuilder.Entity<Gasto>().HasRequired<TipoGasto>(g => g.tipogasto)
                .WithMany(tg => tg.GastoList)
                .HasForeignKey(g => g.TipoGastoID).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Gasto y Actividad */
            modelBuilder.Entity<Gasto>().HasOptional<Actividad>(g => g.actividad)
                .WithMany(a => a.GastoList)
                .HasForeignKey(g => g.ActividadId).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Gasto y Producto */
            modelBuilder.Entity<Gasto>().HasRequired<Producto>(g => g.producto)
                .WithMany(a => a.GastoList)
                .HasForeignKey(g => g.ProductoId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Gasto>().HasOptional<CentroCosto>(p => p.centroCosto)
               .WithMany(cc => cc.Gasto)
               .HasForeignKey(p => new { p.CentroCostoID }).WillCascadeOnDelete(false);

            #endregion

            #region ActividadItem

            /*Llave primaria ActividadItem*/
            modelBuilder.Entity<ActividadItem>().HasKey(a => new { a.ActividadId, a.ActividadItemId });

            /*Relación de uno a muchos entre Actividad y los items de la actividad*/
            modelBuilder.Entity<ActividadItem>().HasRequired<Actividad>(a => a.Actividad)
                .WithMany(b => b.ActividadItemList)
                .HasForeignKey(c => new { c.ActividadId }).WillCascadeOnDelete(false);

            /*Relación de uno a muchos entre Producto y Actividad*/
            modelBuilder.Entity<ActividadItem>().HasRequired<Producto>(p => p.producto)
                .WithMany(p => p.ActividadItemList)
                .HasForeignKey(p => new { p.ProductoId }).WillCascadeOnDelete(false);

            modelBuilder.Entity<ActividadItem>().HasOptional<CentroCosto>(p => p.centroCosto)
              .WithMany(cc => cc.ActividadItem)
              .HasForeignKey(p => new { p.CentroCostoID }).WillCascadeOnDelete(false);

            #endregion

            #region OrdenItems
            /*Llave primaria OrdenItems*/
            modelBuilder.Entity<OrdenItems>()
                .HasKey(oi => new { oi.OrdenId, oi.OrdenItemsLinea });

            /*Relación de uno a muchos entre Orden y OrdenItems*/
            modelBuilder.Entity<OrdenItems>().HasRequired<Orden>(oi => oi.orden)
                .WithMany(o => o.OrdenItemsList)
                .HasForeignKey(oi => new { oi.OrdenId }).WillCascadeOnDelete(false);


            /*Relación de uno a muchos entre Orden y OrdenItems*/
            modelBuilder.Entity<OrdenItems>().HasRequired<Producto>(oi => oi.producto)
                .WithMany(o => o.OrdenItemsList)
                .HasForeignKey(oi => new { oi.ProductoId }).WillCascadeOnDelete(false);

            modelBuilder.Entity<OrdenItems>().HasOptional<CentroCosto>(p => p.centroCosto)
                .WithMany(cc => cc.OrdenItems)
                .HasForeignKey(p => new { p.CentroCostoID }).WillCascadeOnDelete(false);

            #endregion

            #region CausaPQRS

            modelBuilder.Entity<CausaPQRS>().HasKey(c => new { c.Id });

            #endregion


            #region FlujoPQRSTareas

            modelBuilder.Entity<FlujoPQRSTareas>().HasKey(ft => new { ft.Id });

            modelBuilder.Entity<FlujoPQRSTareas>().HasRequired<FlujoPQRS>(ft => ft.FlujoPQRS)
            .WithMany(f => f.FlujoPQRSTareas)
            .HasForeignKey(ft => new { ft.MotivoPQRSId, ft.FlujoPQRSId })
            .WillCascadeOnDelete(false);

            #endregion


            #region PQRSRecordTareas

            modelBuilder.Entity<PQRSRecordTareas>().HasKey(pt => new { pt.Id });

            modelBuilder.Entity<PQRSRecordTareas>().HasRequired<PQRSRecord>(pt => pt.PQRSRecord)
            .WithMany(p => p.PQRSRecordTareas)
            .HasForeignKey(pt => new { pt.PQRSRecordId, pt.PQRSRecordOrder })
            .WillCascadeOnDelete(false);

            #endregion

            #region FlujoPQRSCondiciones

            modelBuilder.Entity<FlujoPQRSCondiciones>().HasKey(ft => new { ft.Id });

            modelBuilder.Entity<FlujoPQRSCondiciones>().HasRequired<FlujoPQRS>(ft => ft.FlujoPQRS)
            .WithMany(f => f.FlujoPQRSCondiciones)
            .HasForeignKey(ft => new { ft.MotivoPQRSId, ft.FlujoPQRSId })
            .WillCascadeOnDelete(false);

            #endregion

            #region PQRSRecordCondiciones

            modelBuilder.Entity<PQRSRecordCondiciones>().HasKey(pt => new { pt.Id });

            modelBuilder.Entity<PQRSRecordCondiciones>().HasRequired<PQRSRecord>(pt => pt.PQRSRecord)
            .WithMany(p => p.PQRSRecordCondiciones)
            .HasForeignKey(pt => new { pt.PQRSRecordId, pt.PQRSRecordOrder })
            .WillCascadeOnDelete(false);

            #endregion

            #region TipoVisita

            modelBuilder.Entity<TipoVisita>().HasKey(m => new { m.Id });

            #endregion

            #region TipoIndustria

            modelBuilder.Entity<TipoIndustria>().HasKey(m => new { m.Id });

            #endregion

            #region Correos

            modelBuilder.Entity<Correos>().HasKey(c => new { c.Id });

            #endregion


            #region NivelesAprobacion

            modelBuilder.Entity<NivelesAprobacion>().HasKey(c => new { c.Id });

            modelBuilder.Entity<NivelesAprobacion>().HasRequired<Plantas>(p => p.planta)
                         .WithMany(u => u.NivelesAprobacion)
                         .HasForeignKey(p => new { p.PlantaID }).WillCascadeOnDelete(false);

            modelBuilder.Entity<NivelesAprobacion>().HasRequired<Canal>(p => p.canal)
              .WithMany(u => u.NivelesAprobacion)
              .HasForeignKey(p => new { p.CanalID }).WillCascadeOnDelete(false);

            modelBuilder.Entity<NivelesAprobacion>().HasRequired<Usuario>(p => p.usuario)
              .WithMany(u => u.NivelesAprobacion)
              .HasForeignKey(p => new { p.UsuarioId }).WillCascadeOnDelete(false);

            #endregion

            #region UsuarioPlanta
            
            modelBuilder.Entity<UsuarioPlanta>().HasKey(up => new { up.UsuarioId, up.PlantaId });

            
            modelBuilder.Entity<UsuarioPlanta>().HasRequired<Usuario>(up => up.Usuario)
                .WithMany(u => u.UsuarioPlantas)
                .HasForeignKey(up => up.UsuarioId).WillCascadeOnDelete(false);

            
            modelBuilder.Entity<UsuarioPlanta>().HasRequired<Plantas>(up => up.Planta)
                .WithMany(p => p.UsuarioPlantas)
                .HasForeignKey(up => up.PlantaId).WillCascadeOnDelete(false);
            #endregion

            #region UsuarioCanal

            modelBuilder.Entity<UsuarioCanal>().HasKey(uc => new { uc.UsuarioId, uc.CanalId });


            modelBuilder.Entity<UsuarioCanal>().HasRequired<Usuario>(uc => uc.Usuario)
                .WithMany(u => u.UsuarioCanales)
                .HasForeignKey(uc => uc.UsuarioId).WillCascadeOnDelete(false);


            modelBuilder.Entity<UsuarioCanal>().HasRequired<Canal>(uc => uc.Canal)
                .WithMany(p => p.UsuarioCanales)
                .HasForeignKey(uc => uc.CanalId).WillCascadeOnDelete(false);
            #endregion
        }

    }
}