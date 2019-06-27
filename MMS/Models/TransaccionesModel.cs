using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

// Juan Manuel Palomino
// Modelo transacciones principales
// 09-09-2016


namespace MMS.Models
{
    [Table("Movimiento")]
    public class Movimiento
    {

        [Key]
        [Required]
        //[MaxLength(3)]
        [Display(Name = "Nro Mov")]
        public int MovimientoId { get; set; }

        [Key]
        [Required]
        [Display(Name = "Línea")]
        public int MovimientoLinea { get; set; }

        [Required]
        [Display(Name = "Fecha Crea")]
        public DateTime MovimientoFechaCrea { get; set; }

        [Required]
        [Display(Name = "Fecha Modifica")]
        public DateTime MovimientoFechaMod { get; set; }

        [MaxLength(5000)]
        [Display(Name = "Nota")]
        public string MovimientoNota { get; set; }

        [Required]
        //[MaxLength(100)]
        [Display(Name = "Estado")]
        public EstadoMovimiento MovimientoEstado { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        [Display(Name = "Valor")]
        public decimal? MovimientoValor { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        public int MovimientoCantidad { get; set; }

        [Required]
        [Display(Name = "Disponibilidad")]
        public int MovimientoDisponible { get; set; }

        [Required]
        [Display(Name = "Reservado")]
        public int MovimientoReservado { get; set; }

        // [Required]
        [MaxLength(100)]
        [Display(Name = "No. movimiento de entrada")]
        public string MovimientoIDEntrada { get; set; }
        //Este campo relaciona la salida de un producto al # de movimiento de entrada 


        //Campos relacionados

        [Required]
        [MaxLength(3)]
        [Display(Name = "Producto")]
        public string ProductoId { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Tipo Movimiento")]
        public string TipoMovimientoID { get; set; }

        // [Required]
        [Display(Name = "No. Orden")]
        public int? OrdenId { get; set; }

        [Required]
        [MaxLength(5)]
        [Display(Name = "Usuario Modifica")]
        public string UsuarioIdModifica { get; set; }


        [ScriptIgnore, JsonIgnore]
        public TipoMovimiento tipoMovimiento { get; set; }
        //public OrdenItems ordenitems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Producto producto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Usuario usuario { get; set; }
    }

    [Table("Producto")]
    public class Producto
    {
        [Key]
        [Required]
        [MaxLength(10)]
        [Display(Name = "Código")]
        public string ProductoId { get; set; }

        [Required]
        [MaxLength(10)]
        [Display(Name = "Código AutoLog")]
        public string CodigoAutoLog { get; set; }//Cayo

        [Required]
        [MaxLength(20)]
        [Display(Name = "Código Cliente")]
        public string CodigoClienteID { get; set; }//Cayo

        [Required]
        [MaxLength(100)]
        [Display(Name = "Informações Complementares")]
        public string ProductoDesc { get; set; }

        [Required]
        [MaxLength(30)]
        [Display(Name = "Base/Filial")]
        public string BaseFilial { get; set; }//Cayo

        [Required]
        [MaxLength(30)]
        [Display(Name = "Grupo")]
        public string Grupo { get; set; }//Cayo

        [Required]
        [MaxLength(30)]
        [Display(Name = "Departamento")]
        public string Departamento { get; set; }//Cayo

        [Required]
        [MaxLength(3)]
        [Display(Name = "Tipo")]
        public string TipoProductoID { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Valor Unitário")]
        public decimal? ProductoPrecio { get; set; }

        [Display(Name = "Saldo")]
        public decimal? SaldoProducto { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Valor Total")]
        public decimal? ValorTotal { get; set; }

        public TipoProducto tipoProducto { get; set; }


        public virtual ICollection<ActividadItem> ActividadItemList { get; set; }
        public virtual ICollection<OrdenItems> OrdenItemsList { get; set; }
        public virtual ICollection<Movimiento> MovimientoList { get; set; }
        public virtual ICollection<Gasto> GastoList { get; set; }
    }

    [Table("Orden")]
    public class Orden
    {

        [Key]
        [Display(Name = "Nro Orden")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OrdenId { get; set; }

        [Required]
        [Display(Name = "Actividad")]
        public int ActividadId { get; set; }

        [Required]
        [Display(Name = "Estado")]
        public EstadoOrden OrdenEstado { get; set; }

        [Required]
        [Display(Name = "Fecha Crea")]
        public DateTime OrdenFecha { get; set; }

        [Display(Name = "Fecha Mod")]
        public DateTime OrdenFechaModificacion { get; set; }

        [Display(Name = "Fecha Des")]
        public DateTime OrdenFechaDespacho { get; set; }

        [MaxLength(3000)]
        [Display(Name = "Comentario")]
        public string OrdenComentario { get; set; }


        [MaxLength(3000)]
        [Display(Name = "Guia")]
        public string OrdenNroGuia { get; set; }

        [Required]
        [MaxLength(5)]
        [Display(Name = "Usuario Modifica")]
        public string UsuarioIdModifica { get; set; }


        public Actividad actividad { get; set; }
        public Usuario usuario { get; set; }


        //public virtual ICollection<Movimiento> MovimientoList { get; set; }
        public virtual ICollection<OrdenItems> OrdenItemsList { get; set; }

    }

    [Table("OrdenItems")]
    public class OrdenItems
    {
        [Key]
        [Display(Name = "Nro Orden")]
        public int OrdenId { get; set; }

        [Key]
        [Required]
        [Display(Name = "Lin")]
        public int OrdenItemsLinea { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Producto")]
        public string ProductoId { get; set; }

        [Display(Name = "Cant.")]
        public int OrdenItemsCant { get; set; }

        [Display(Name = "Cant. Confirmada")]
        public int OrdenItemsCantConfirmada { get; set; }

        [Display(Name = "Valor")]
        public decimal OrdenItemsVlr { get; set; }

        [MaxLength(100)]
        [Display(Name = "Numeros de Movimientos")]
        public string OrdenItemsNroMov { get; set; }

        [StringLength(3)]
        [Display(Name = "Centro Costo")]
        public string CentroCostoID { get; set; }

        [ScriptIgnore, JsonIgnore]
        public CentroCosto centroCosto { get; set; }


        public Producto producto { get; set; }
        public Orden orden { get; set; }

    }

    [Serializable]
    [Table("Actividad")]
    public class Actividad
    {

        [Key]
        [Required]
        [Display(Name = "No.")]
        public int ActividadId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Ação")]
        public string ActividadTitulo { get; set; } // Cayo Diebe

        
        [MaxLength(50)]
        [Display(Name = "Centro de Custo")]
        public string ActividadCuenta { get; set; } // Cuenta para relacionar actividad con SAP

        [Display(Name = "Estado")]
        public EstadosActividades ActividadEstado { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Descrição Breve da Ação")]
        public string ActividadDesc { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Motivo da Ação")]
        public string ActividadObjetivo { get; set; }

        //[MaxLength(100)]
        //[Display(Name = "KeyFiles")]
        //public string ActividadKeyFiles { get; set; }

        [Required]
        [Display(Name = "Fecha")]
        public DateTime ActividadFecha { get; set; }

        //Fecha se actrualiza cuando hay cambio en la tabla
        [Display(Name = "Fecha Mod")]
        public DateTime? ActividadFechaMod { get; set; }

        //Fecha solo se actualiza cuando el usuario o Kam envia a autorizar
        [Display(Name = "Fecha Envio Apro")]
        public DateTime? ActividadFechaAprob { get; set; }

        [Required]
        [Display(Name = "Desde")]
        public DateTime ActividadFechaDesde { get; set; }

        [Required]
        [Display(Name = "Término")]
        public DateTime ActividadFechaHasta { get; set; }


        [Column(TypeName = "Money")]
        [Display(Name = "SELL-OUT CLIENTE (MÉDIA 6 MESES)")]
        public decimal? ActividadMetaV { get; set; }


        [Column(TypeName = "Money")]
        [Display(Name = "Meta de Crescimento (%)")]
        public decimal? ActividadMetaE { get; set; }


        [Display(Name = "ultima posición Item")]
        public int ActividadUltimoItem { get; set; }

        //Relacionados

        [Required]
        [MaxLength(5)]
        [Display(Name = "Solicitante")]
        public string UsuarioIdElabora { get; set; }


        [MaxLength(3)]
        [Display(Name = "Canal")]
        public string CanalID { get; set; }

        
        [MaxLength(4)]
        [Display(Name = "PlantaID")]
        public string PlantaID { get; set; }

        [MaxLength(3)]
        [Display(Name = "No. Gasto")]
        public string GastoId { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Tipo")]
        public string TipoActividadID { get; set; }

        [Required]
        [MaxLength(15)]
        [Display(Name = "Cliente")]
        public string ClienteID { get; set; }

        [MaxLength(300)]
        [Display(Name = "Upload Arquivos")]
        public string ActividadLugarEnvioPOP { get; set; }//Add 13/02/2017 Carlos Delgado.

        [StringLength(50)]
        [Display(Name = "Marcas")]
        public string Marcas { get; set; }

        #region cierreActividad
        [Column(TypeName = "Money")]
        [Display(Name = "$ Cumplimiento")]
        public decimal? CumplimientoTotal { get; set; }

        [Display(Name = "% Cumplimiento")]
        public int? CumplimientoPorcentaje{ get; set; }

        [MaxLength(300)]
        [Display(Name = "Resultado")]
        public string Resultado { get; set; }

        [Display(Name = "Estado cierre actividad")]
        public EstadoCierreActividad? EstadoCierre { get; set; }

        [Display(Name = "Comparar cumplimiento $ contra:")]
        public MetaCierreActividad? MetaCierre { get; set; }        

        #endregion

        //public virtual ICollection<ActividadAnexos> ActividadAnexosList { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ActividadItem> ActividadItemList { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ActividadAutorizacion> ActividadAutorizacionesList { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Orden> OrdenList { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Gasto> GastoList { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ActividadArchivo> ActividadArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public TipoActividad tipoActividad { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Canal canal { get; set; }

        [ScriptIgnore, JsonIgnore]
        public CentroCosto centroCosto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Cliente cliente { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Plantas planta { get; set; }

    }

    [Table("ActividadAutorizacion")]
    public class ActividadAutorizacion
    {

        [Key]
        [Required]
        [Display(Name = "Activ.")]
        public int ActividadId { get; set; }

     
        [Display(Name = "Fecha Auto.")]
        public DateTime ActividadAutorizacionFecha { get; set; }

        [Display(Name = "Estado")]
        public EstadoAutorizaActividad? ActividadAutorizacionAutoriza { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Motivo")]
        public string ActividadAutorizacionMotivo { get; set; }
        //Relacionados

        [Key]
        [Required]
        [MaxLength(5)]
        [Display(Name = "Usuario Autoriza")]
        public string UsuarioIdAutoriza { get; set; }

        [MaxLength(50)]
        [Display(Name = "Aprobación")]
        public string AprobacionDesc { get; set; }

        [Display(Name = "Orden")]
        public int? Orden { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Usuario usuario { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Actividad Actividad { get; set; }

    }

    [Table("ActividadItem")]
    public class ActividadItem
    {

        [Key]
        [Required]
        [Display(Name = "Código")]
        public int ActividadId { get; set; }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID Item")]
        public int ActividadItemId { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        public int ActividadItemCantidad { get; set; }

        [Required]
        [Display(Name = "Descripción Producto")]
        public string ActividadItemProducto { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Precio")]
        public decimal? ActividadItemPrecio { get; set; }

        [Required]
        [Display(Name = "Descripción Producto")]
        public string ActividadItemDescripcion { get; set; }
        //Relacionados

        [Required]
        [MaxLength(3)]
        [Display(Name = "Producto")]
        public string ProductoId { get; set; }


        [StringLength(3)]
        [Display(Name = "Centro Costo")]
        public string CentroCostoID { get; set; }

        [ScriptIgnore, JsonIgnore]
        public CentroCosto centroCosto { get; set; }

        public Actividad Actividad { get; set; }
        public Producto producto { get; set; }

    }

    //[Table("ActividadAnexos")]
    //public class ActividadAnexos
    //{

    //    [Key]
    //    [Required]
    //    [Display(Name = "Código")]
    //    public int ActividadId { get; set; }

    //    [Key]
    //    [Required]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    [Display(Name = "ID Item")]
    //    public int ActividadAnexosId { get; set; }

    //    [Required]
    //    [MaxLength(1000)]
    //    [Display(Name = "Nombre del Archivo")]
    //    public string ActividadAnexosDesc { get; set; }

    //    [Required]
    //    [MaxLength(360)]
    //    [Display(Name = "Ruta al Archivo")]
    //    public string ActividadAnexosNombre { get; set; }


    //    [Display(Name = "Fecha Carga")]
    //    public DateTime ActividadAnexosFecha { get; set; }




    //    public Actividad Actividad { get; set; }

    //}

    [Table("Presupuesto")]
    public class Presupuesto
    {

        [Key]
        [Required]
        [Display(Name = "Año")]
        public int PresupuestoAno { get; set; }

        [Key]
        [Required]
        [Display(Name = "Mes")]
        public int PresupuestoMes { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        [Display(Name = "Valor")]
        public decimal? PresupuestoValor { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        [Display(Name = "Valor")]
        public decimal? PresupuestoGasto { get; set; }
    }

    [Serializable]
    [Table("PresupuestoVendedor")]
    public class PresupuestoVendedor : IValidatableObject
    {

        //Mantener
        [Key]
        [Required]
        [Display(Name = "Ano")]
        public int? PresupuestoVendedorAno { get; set; }

        //Cayo 12/11/2018
        [Required]
        [MaxLength(4)]
        [Display(Name = "PlantaID")]
        public string PlantaID { get; set; }
        
        //Cayo 21/11/2018
        [Required]
        [MaxLength(3)]
        [Display(Name = "CanalID")]
        public string CanalID { get; set; }

        //Mantener
        [Required]
        [Column(TypeName = "Money")]
        [Display(Name = "Valor Planejado")] //Planejamento
        public decimal? PresupuestoValor { get; set; }

        //Mantener
        [Column(TypeName = "Money")]
        [Display(Name = "Valor Aprovado")] // Gasto
        public decimal? PresupuestoGasto { get; set; }

        //Mantener
        [StringLength(3)]
        [Display(Name = "Centro de Custo")]
        public string CentroCostoID { get; set; }

        //Mantener
        [ScriptIgnore, JsonIgnore]
        public CentroCosto centroCosto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Plantas planta { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Canal canal { get; set; }

        //Mantener
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PresupuestoGasto > PresupuestoValor)
                yield return new ValidationResult("Gasto não pode ser maior que o Budget", new[] { "PresupuestoGasto", "PresupuestoValor" });       
        }
    }

    [Table("VentasxCliente")]
    public class VentasxCliente
    {

        [Key]
        [Required]
        [Display(Name = "Ano")]
        public int VentasxClienteAno { get; set; }

        [Key]
        [Required]
        [Display(Name = "Mes")]
        public int VentasxClienteMes { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        [Display(Name = "Faturamento")]
        public decimal? VentasxClienteVenta { get; set; }

        //Relacionados

        [Key]
        [Required]
        [MaxLength(15)]
        [Display(Name = "Customer")]
        public string ClienteID { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Cliente cliente { get; set; }

    }

    [Serializable]
    [Table("Configuracion")]
    public class Configuracion
    {
        [Key]
        [Required]
        [Display(Name = "llave")]
        public int ConfigLlave { get; set; }

        [MaxLength(200)]
        [Display(Name = "Listado de autorizadores en Actividad")]
        public string ConfigActAutoriza { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo de Producto Inventariable")]
        public string ConfigTipoProdInv { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Producto Gasto")]
        public string ConfigTipoProdGasto { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Movimiento Entrada")]
        public string ConfigTipoMovEntrada { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Movimiento Salida")]
        public string ConfigTipoMovSalida { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Movimiento Ajuste Entrada")]
        public string ConfigTipoMovAjEntrada { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Movimiento Ajuste Salida")]
        public string ConfigTipoMovAjSalida { get; set; }

        [Display(Name = "Tipo Usuario KAM")]
        public int? ConfigTipoUsuKam { get; set; }

        [Display(Name = "Tipo Usuario Vendedor")]
        public int? ConfigTipoUsuVendedor { get; set; }

        //PQRS
        [Display(Name = "Tipo Usuario PQRS Coordinador")]
        public int? ConfigTipoUsuPQRSCoordinador { get; set; }

        [Display(Name = "Tipo Usuario PQRS Analista")]
        public int? ConfigTipoUsuPQRSAnalista { get; set; }
        //PQRS

        //Para el envío de correos

        [MaxLength(100)]
        [Display(Name = "Remitente")]
        public string ConfigSmtpFrom { get; set; }

        [MaxLength(100)]
        [Display(Name = "Host")]
        public string ConfigSmtpHost { get; set; }

        [Display(Name = "Puerto")]
        public int? ConfigSmtpPort { get; set; }

        [MaxLength(100)]
        [Display(Name = "Nombre de Usuario")]
        public string ConfigSmtpUserName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Contraseña")]
        public string ConfigSmtPassword { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Actividad Colocación")]
        public string ConfigTipoActiColocacion { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Actividad Evacuación")]
        public string ConfigTipoActiEvacuacion { get; set; }

        [MaxLength(3)]
        [Display(Name = "Tipo Actividad Mixta")]
        public string ConfigTipoActiMixta { get; set; }

        [Display(Name = "Fecha de Actualización de Datos Base")]
        public DateTime? ConfigFechaActualizacionDatosBase { get; set; }

        [MaxLength(100)]
        [Display(Name = "URL")]
        public string ConfigURLServidor { get; set; }

        [Display(Name = "PIP Base Percent")]
        public float ConfigPorcentajePIV { get; set; }

        public short ConfigPeakNumeroObjetivos { get; set; }

        [MaxLength(100)]
        [Display(Name = "Dominio WEB")]
        public string ConfigDominioWeb { get; set; }

        [Display(Name = "Smtp UseDefaultCredentials")]
        public bool? configSmtpUseDefaultCredentials { get; set; }

        [MaxLength(5)]
        [Display(Name = "Usuario Vendedor")]
        public string UsuarioVendedorPQRS { get; set; }

        [Display(Name = "Usuario Vendedor")] 
        public int? TipoIndustriaAutomotriz { get; set; }
    }

    [Table("Gasto")]
    public class Gasto
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Spent")]
        public int GastoId { get; set; }

        [Key]
        [Display(Name = "Line")]
        public int GastoLinea { get; set; }

        [Display(Name = "Date")]
        public DateTime GastoFecha { get; set; }

        
        [Display(Name = "Last Modified")]
        public DateTime GastoFechaMod { get; set; }

        //Relacionados
        [Required]
        [MaxLength(3)]
        [Display(Name = "Spent Type")]
        public string TipoGastoID { get; set; }


        //[Required]
        [Display(Name = "Activity")]
        public int? ActividadId { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Product")]
        public string ProductoId { get; set; }

        [StringLength(3)]
        [Display(Name = "Cost Center")]
        public string CentroCostoID { get; set; }
        //Relacionados


        [Column(TypeName = "Money")]
        [Display(Name = "Price")]
        public decimal GastoValor { get; set; }


        [Display(Name = "QTY")]
        public int GastoCant { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Comment")]
        public string GastoComentario { get; set; }

        [MaxLength(300)]
        [Display(Name = "No. Invoice")]
        public string GastoFactura { get; set; }

        [Required]
        [Display(Name = "Status")]
        public EstadoGasto GastoEstado { get; set; }
    

        [ScriptIgnore, JsonIgnore]
        public CentroCosto centroCosto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Actividad actividad { get; set; }
        [ScriptIgnore, JsonIgnore]
        public TipoGasto tipogasto { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Producto producto { get; set; }

    }


    [Table("Consecutivo")]
    public class Consecutivo
    {
        [Key]
        [Required]
        [MaxLength(100)]
        [Display(Name = "Id")]
        public string ConsecutivoId { get; set; }

        [Required]
        [Display(Name = "Nro")]
        public int ConsecutivoNro { get; set; }
    }

    [Table("ActividadArchivo")]
    public class ActividadArchivo
    {
        public int ActividadId { get; set; }

        public int Order { get; set; }

        [Required]
        public byte[] File { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string MediaType { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Actividad Actividad { get; set; }
    }

    [Table("NivelesAprobacion")]
    public class NivelesAprobacion
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(3)]
        [Index("IXU_Canal_Planta_Usuario", 1, IsUnique = true)]
        [Index("IXU_Canal_Planta_Orden", 1, IsUnique = true)]
        [Display(Name = "Canal")]
        public string CanalID { get; set; }

        [Required]
        [MaxLength(4)]
        [Index("IXU_Canal_Planta_Usuario", 2, IsUnique = true)]
        [Index("IXU_Canal_Planta_Orden", 2, IsUnique = true)]
        [Display(Name = "Planta")]
        public string PlantaID { get; set; }

        [Required]     
        [Display(Name = "Orden")]
        [Index("IXU_Canal_Planta_Orden", 3, IsUnique = true)]
        public int Orden { get; set; }

        [Required]
        [Display(Name = "Usuario")]
        [Index("IXU_Canal_Planta_Usuario", 3, IsUnique = true)]
        [StringLength(5)]
        public string UsuarioId { get; set; }//Solo usuario de la planta que se elija

        [Required]
        [Display(Name = "Desc")]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Canal canal { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Usuario usuario { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Plantas planta { get; set; }
    }

    #region ViewModels

    public class ActividadViewModel
    {
        public Actividad Actividad { get; set; }
        public List<ActividadItemViewModel> Items { get; set; }
        public List<Archivo> Archivos { get; set; }
        public List<ActividadAutorizacion> ActividadAutorizaciones { get; set; }
        public class Archivo
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }

        public class ActividadItemViewModel
        {          
            [Display(Name = "Código")]
            public int ActividadId { get; set; }
            
            [Display(Name = "ID Item")]
            public int ActividadItemId { get; set; }

            [Display(Name = "Cantidad")]
            public int ActividadItemCantidad { get; set; }

            [Display(Name = "Descripción Producto")]
            public string ActividadItemProducto { get; set; }

            [Display(Name = "Precio")]
            public decimal? ActividadItemPrecio { get; set; }

            [Display(Name = "Descripción Producto")]
            public string ActividadItemDescripcion { get; set; }    
   
            [Display(Name = "Producto")]
            public string ProductoId { get; set; }

            [Display(Name = "Centro Costo")]
            public string CentroCostoID { get; set; }

            public bool delete { get; set; }
        }
    }


    public class ActividadCierreViewModel
    {
        [Display(Name = "Actividad")]
        public int ActividadId { get; set; }

        [Display(Name = "Titulo")]
        public string ActividadTitulo { get; set; }

        [Display(Name = "Estado")]
        public string ActividadEstado { get; set; }

        [Display(Name = "Cliente")]
        public string ClienteID { get; set; }

        [Display(Name = "Sell-Out Cliente")]
        public decimal ActividadMetaV { get; set; }

        [Display(Name = "Meta de Crescimento")]
        public decimal ActividadMetaE { get; set; }

        [Display(Name = "Total Inversión")]
        public decimal ActividadTotal { get; set; } 

        #region cierreActividad
        [Required]
        [Display(Name = "$ Cumplimiento")]
        public decimal CumplimientoTotal { get; set; }

        [Required]
        [Display(Name = "% Cumplimiento")]
        public int CumplimientoPorcentaje { get; set; }

        [Required]
        [MaxLength(300)]
        [Display(Name = "Resultado")]
        public string Resultado { get; set; }

        [Required]
        [Display(Name = "Estado cierre actividad")]
        public EstadoCierreActividad EstadoCierre { get; set; }

        [Required]
        [Display(Name = "Comparar cumplimiento $ contra:")]
        public MetaCierreActividad MetaCierre { get; set; }
        #endregion

        public List<Archivo> Archivos { get; set; }

        public class Archivo
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }
    }
    #endregion
}