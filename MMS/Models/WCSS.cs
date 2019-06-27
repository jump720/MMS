using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Web.Mvc;

namespace MMS.Models
{

    [Table("Devolucion")]
    public class Devolucion
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }//PK

        [Required]
        [Display(Name = "Date")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "User")]
        public string UsuarioIdCreacion { get; set; }

        [Required]
        [Display(Name = "Status")]
        public EstadoFormatoPQRS Estado { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Customer")]
        public string ClienteId { get; set; }

        [StringLength(5)]
        [Display(Name = "Analyst")]
        public string AnalistaId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Subject")]
        public string Asunto { get; set; }

        [Required]
        [StringLength(700)]
        [Display(Name = "Comments")]
        public string Observacion { get; set; }


        [StringLength(200)]
        [Display(Name = "Recipient")]
        public string Destinatarios { get; set; }

        [StringLength(10)]//D+id+CARACTERES HASH
        [Display(Name = "Tracking No")]
        public string NroTracking { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioCreacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Analista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Cliente Cliente { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DevolucionItem> DevolucionItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DevolucionArchivo> DevolucionArchivos { get; set; }

    }

    [Table("DevolucionItem")]
    public class DevolucionItem
    {
        [Required]
        [Display(Name = "Returns Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DevolucionId { get; set; }//PK

        [Required]
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }//PK

        [Display(Name = "Product")]
        public int ItemId { get; set; }

        [Display(Name = "Total Qty")]
        public int Cantidad { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Total Price")]
        public decimal Precio { get; set; }

        [StringLength(50)]
        [Display(Name = "Invoice")]
        public string NroFactura { get; set; }

        [StringLength(50)]
        [Display(Name = "Tracking number")]
        public string NroGuia { get; set; }

        [Display(Name = "Reason")]
        public int MotivoPQRSId { get; set; }


        [StringLength(200)]
        [Display(Name = "Comments")]
        public string ComentarioAdicional { get; set; }

        //Segunda Parte
        [Display(Name = "Status")]
        public EstadoFormatoItemPQRS? Estado { get; set; }

        [Display(Name = "Received Qty")]
        public int? CantidadRecibida { get; set; }

        [Display(Name = "Loaded Qty")]
        public int? CantidadSubida { get; set; }

        [StringLength(200)]
        [Display(Name = "Status product")]
        public string ComentarioEstadoMercancia { get; set; }

        [StringLength(50)]
        [Display(Name = "Support document")]
        public string DocSoporte { get; set; }

        [Display(Name = "PQRS Record Id")]//FK
        public int? PQRSRecordId { get; set; }


        [Column(TypeName = "Money")]
        [Display(Name = "Price assumed")]
        public decimal? PrecioAsumido { get; set; }


        [Display(Name = "Cause")]//FK
        public int? CausaPQRSId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CausaPQRS CausaPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Devolucion Devolucion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual MotivoPQRS MotivoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Items { get; set; }



    }

    [Table("DevolucionArchivo")]
    public class DevolucionArchivo
    {
        public int DevolucionId { get; set; }//PK

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
        public virtual Devolucion Devolucion { get; set; }
    }

    [Table("MotivoPQRS")]
    public class MotivoPQRS
    {

        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Type PQRS")]
        public TipoPQRS TipoPQRS { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Active")]
        public bool? Activo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DevolucionItem> DevolucionItems { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<GarantiaItem> GarantiaItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NovedadItem> NovedadItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<FlujoPQRS> FlujoPQRS { get; set; }

    }

    [Table("Garantia")]
    public class Garantia
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }//PK

        [Required]
        [Display(Name = "Date")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "User")]
        public string UsuarioIdCreacion { get; set; }

        [Required]
        [Display(Name = "Status")]
        public EstadoFormatoPQRS Estado { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Customer")]
        public string ClienteId { get; set; }

        [StringLength(5)]
        [Display(Name = "Analyst")]
        public string AnalistaId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Subject")]
        public string Asunto { get; set; }

        [Required]
        [StringLength(700)]
        [Display(Name = "Comments")]
        public string Observacion { get; set; }


        [StringLength(200)]
        [Display(Name = "Recipients")]
        public string Destinatarios { get; set; }

        [StringLength(10)]//G+id+CARACTERES HASH
        [Display(Name = "Tracking No")]
        public string NroTracking { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioCreacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Analista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Cliente Cliente { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<GarantiaItem> GarantiaItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<GarantiaArchivo> GarantiaArchivos { get; set; }

    }

    [Table("Recruitment")]
    public class Recruitment
    {
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecruitmentId { get; set; }//PK

        [Required]
        [Display(Name = "Tipo")]
        public Types Type { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Cargo")]
        public string Appointment { get; set; }

        [Required]
        [Display(Name = "Departamento")]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Horário de Trabalho")]
        public string WorkSchedule { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Centro de Custo")]
        public string CentroCostoID { get; set; }

        [Required]
        [Display(Name = "Data de Inicio")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Salário")]
        public int Salary { get; set; }

        [StringLength(50)]
        [Display(Name = "Nome do Empregado")]
        public string EmployeeName { get; set; }

        [StringLength(50)]
        [Display(Name = "Cargo Proposto")]
        public string ProposedAppointment { get; set; }

        [Display(Name = "Área Proposta")]
        public int? ProposedDepartmentId { get; set; }

        [StringLength(50)]
        [Display(Name = "RE")]
        public string Registration { get; set; }

        [Display(Name = "Salário Proposto")]
        public int? ProposedSalary { get; set; }

        [StringLength(3)]
        [Display(Name = "Centro de Custo")]
        public string ProposedCostCenterID { get; set; }

        [Required]
        [Display(Name = "Setor")]
        public Sectors Sector { get; set; }

        [Required]
        [Display(Name = "Posição")]
        public Positions Position { get; set; }

        [Required]
        [Display(Name = "Tipo de Contrato")]
        public ContractTypes ContractType { get; set; }

        [Required]
        [Display(Name = "Budget")]
        public Budgets Budget { get; set; }

        [Display(Name = "Resignation - Reason")]
        public ResignationReasons? ResignationReason { get; set; }

        [Display(Name = "Aviso Prévio")]
        public PreviousNotices? PreviousNotice { get; set; }

        [Display(Name = "Data")]
        public DateTime ResignationDate { get; set; }

        //[Required]
        [StringLength(5)]
        [Display(Name = "Nome do Substituto")]
        public string UsuarioIdSubstitute { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Cargo do Substituto")]
        public string SubstituteAppointment { get; set; }    

        [Required]
        [StringLength(50)]
        [Display(Name = "Tempo de Experiência")]
        public string ExperienceTime { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Observação")]
        public string Observation { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Diretor Area")]
        public string AreaManagerID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Recursos Humanos")]
        public string HumanResourcesID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Chefe")]
        public string ImmediateBossID { get; set; }

        [Required]
        [Display(Name = "Status")]
        public EstadoFormatoPQRS Estado { get; set; }

        [StringLength(10)]//G+id+CARACTERES HASH
        [Display(Name = "Tracking No")]
        public string NroTracking { get; set; }
        
        [StringLength(5)]
        [Display(Name = "Analyst")]
        public string AnalistaId { get; set; }


        [Required]
        [Display(Name = "Date")]
        public DateTime CreationDate { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "User")]
        public string UsuarioIdCreacion { get; set; }

        [Display(Name = "PQRS Record Id")]//FK
        public int? PQRSRecordId { get; set; }
        
        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioCreacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Analista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Area Department { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Area ProposedDepartment { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CentroCosto CentroCosto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CentroCosto ProposedCostCenter { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioSubstitute { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario AreaManager { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario HumanResources { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario ImmediateBoss { get; set; }

    }

    [Table("GarantiaItem")]
    public class GarantiaItem
    {
        [Required]
        [Display(Name = "Guarantee Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GarantiaId { get; set; }//PK

        [Required]
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }//PK

        [Display(Name = "Product")]
        public int ItemId { get; set; }

        [Display(Name = "Total Qty")]
        public int Cantidad { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Total Price")]
        public decimal Precio { get; set; }

        [StringLength(50)]
        [Display(Name = "Invoice")]
        public string NroFactura { get; set; }

        [StringLength(50)]
        [Display(Name = "Tracking number")]
        public string NroGuia { get; set; }
        //Tipo de Devolución
        [Display(Name = "Reason")]
        public int MotivoPQRSId { get; set; }

        [StringLength(200)]
        [Display(Name = "Additional comment")]
        public string ComentarioAdicional { get; set; }

        //Segunda Parte
        [Display(Name = "Status")]
        public EstadoFormatoItemPQRS? Estado { get; set; }

        [Display(Name = "Received Qty")]
        public int? CantidadRecibida { get; set; }

        [Display(Name = "Loaded Qty")]
        public int? CantidadSubida { get; set; }

        [StringLength(200)]
        [Display(Name = "Status Product")]
        public string ComentarioEstadoMercancia { get; set; }

        [StringLength(50)]
        [Display(Name = "Support doc")]
        public string DocSoporte { get; set; }

        [Display(Name = "PQRS Record Id")]//FK
        public int? PQRSRecordId { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Price assumed")]
        public decimal? PrecioAsumido { get; set; }

        [Display(Name = "Cause")]//FK
        public int? CausaPQRSId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CausaPQRS CausaPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Garantia Garantia { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual MotivoPQRS MotivoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Items { get; set; }


    }

    [Table("GarantiaArchivo")]
    public class GarantiaArchivo
    {
        public int GarantiaId { get; set; }

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
        public virtual Garantia Garantia { get; set; }
    }

    [Table("Novedad")]
    public class Novedad
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }//PK

        [Required]
        [Display(Name = "Date")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "User")]
        public string UsuarioIdCreacion { get; set; }

        [Required]
        [Display(Name = "Status")]
        public EstadoFormatoPQRS Estado { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public Prioridad Prioridad { get; set; }

        [Required]
        [Display(Name = "Reporting user")]
        public TipoPersona TipoPersona { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Reporting user")]
        public string Persona { get; set; }

        [Required]
        [StringLength(15)]
        [Display(Name = "Customer")]
        public string ClienteId { get; set; }

        [StringLength(5)]
        [Display(Name = "Analyst")]
        public string AnalistaId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Subject")]
        public string Asunto { get; set; }

        [Required]
        [StringLength(700)]
        [Display(Name = "Comment")]
        public string Observacion { get; set; }


        [StringLength(200)]
        [Display(Name = "Recipients")]
        public string Destinatarios { get; set; }

        [StringLength(10)]//N+id+CARACTERES HASH
        [Display(Name = "Tracking No")]
        public string NroTracking { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioCreacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Analista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Cliente Cliente { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NovedadItem> NovedadItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NovedadArchivo> NovedadArchivos { get; set; }

    }

    [Table("NovedadItem")]
    public class NovedadItem
    {
        [Required]
        [Display(Name = "New Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NovedadId { get; set; }//PK

        [Required]
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }//PK

        [Display(Name = "Product")]
        public int ItemId { get; set; }

        [Display(Name = "Total Qty")]
        public int Cantidad { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Total Price")]
        public decimal Precio { get; set; }

        [StringLength(50)]
        [Display(Name = "Invoice")]
        public string NroFactura { get; set; }

        [StringLength(50)]
        [Display(Name = "Tracking Number")]
        public string NroGuia { get; set; }


        //Tipo de Devolución
        [Display(Name = "Reason")]
        public int MotivoPQRSId { get; set; }

        [StringLength(200)]
        [Display(Name = "Additional Comment")]
        public string ComentarioAdicional { get; set; }

        //Segunda Parte
        [Display(Name = "Status")]
        public EstadoFormatoItemPQRS? Estado { get; set; }

        [Display(Name = "Received Qty")]
        public int? CantidadRecibida { get; set; }

        [Display(Name = "Loaded Qty")]
        public int? CantidadSubida { get; set; }

        [StringLength(200)]
        [Display(Name = "Status Product")]
        public string ComentarioEstadoMercancia { get; set; }

        [StringLength(50)]
        [Display(Name = "Support doc")]
        public string DocSoporte { get; set; }

        [Display(Name = "PQRS Record Id")]//FK
        public int? PQRSRecordId { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Price assumed")]
        public decimal? PrecioAsumido { get; set; }

        [Display(Name = "Cause")]//FK
        public int? CausaPQRSId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CausaPQRS CausaPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Novedad Novedad { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual MotivoPQRS MotivoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Items { get; set; }


    }

    [Table("NovedadArchivo")]
    public class NovedadArchivo
    {
        public int NovedadId { get; set; }

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
        public virtual Novedad Novedad { get; set; }
    }

    [Table("FlujoPQRS")]
    public class FlujoPQRS
    {

        [Display(Name = "Reason")]//PK
        public int MotivoPQRSId { get; set; }

        [Display(Name = "Id")]//PK
        public int Id { get; set; }

        [Display(Name = "Order")]
        public int Order { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Type")]
        public TipoPaso TipoPaso { get; set; }

        [Display(Name = "Send Mail Recipients")]
        public bool? EnviaCorreoDestinatarios { get; set; }

        [AllowHtml]
        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual MotivoPQRS MotivoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioFlujoPQRS> UsuarioFlujoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<FlujoPQRSTareas> FlujoPQRSTareas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<FlujoPQRSCondiciones> FlujoPQRSCondiciones { get; set; }

    }

    [Table("UsuarioFlujoPQRS")]
    public class UsuarioFlujoPQRS
    {
        [Display(Name = "Reason Id")]//Pk FK
        public int MotivoPQRSId { get; set; }

        [Display(Name = "Id")]//Pk FK
        public int FlujoPQRSId { get; set; }

        [MaxLength(5)]
        [Display(Name = "User")]//Pk FK
        public string UsuarioId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual FlujoPQRS FlujoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }

    [Table("FlujoPQRSTareas")]
    public class FlujoPQRSTareas
    {
        [Display(Name = "Id")]//Pk FK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Reason Id")]//Pk FK
        public int MotivoPQRSId { get; set; }

        [Display(Name = "Flow Id")]//Pk FK
        public int FlujoPQRSId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [Display(Name = "Required?")]
        public bool Requerido { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual FlujoPQRS FlujoPQRS { get; set; }
    }

    [Table("FlujoPQRSCondiciones")]
    public class FlujoPQRSCondiciones
    {

        [Display(Name = "Id")]//Pk FK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Reason Id")]//FK
        public int MotivoPQRSId { get; set; }

        [Display(Name = "Flow Id")]//FK
        public int FlujoPQRSId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Condition Type")]
        public TipoCondicion TipoCondicion { get; set; }
        
        [Display(Name = "Condition")]
        public CondicionesValor? CondicionesValor { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Value  (Case to apply condition)")]
        public decimal? Valor { get; set; }

        [Display(Name = "YesNo  (Case to apply condition)")]
        public bool SiNo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual FlujoPQRS FlujoPQRS { get; set; }
    }

    [Table("PQRSRecord")]
    public class PQRSRecord
    {
        [Display(Name = "Id")]//PK de la tabla tipo R1,R2,R3
        public int Id { get; set; }

        [Display(Name = "Order")]//PK del flujo, No paso
        public int Order { get; set; }

        [Display(Name = "Status")]
        public EstadoStep EstadoStep { get; set; }

        [Display(Name = "Paso Actual")]
        public bool? PasoActual { get; set; }

        /*-- Motivo PQRS --*/
        [Display(Name = "Reason")]
        public int MotivoPQRSId { get; set; }

        [Display(Name = "Type PQRS")]
        public TipoPQRS TipoPQRS { get; set; }

        [Display(Name = "Reason Name")]
        [StringLength(50)]
        public string MotivoPQRSNombre { get; set; }

        /*-- Motivo PQRS --*/

        /*-- Flujo PQRS --*/

        [StringLength(50)]
        [Display(Name = "Name")]
        public string FlujoPQRSNombre { get; set; }

        [Display(Name = "Type")]
        public TipoPaso FlujoPQRSTipoPaso { get; set; }

        [Display(Name = "Send Mail Recipients")]
        public bool? EnviaCorreoDestinatarios { get; set; }

        [AllowHtml]
        [Display(Name = "Description")]
        public string FlujoPQRSDescripcion { get; set; }

        /*-- Flujo PQRS --*/


        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordUsuario> PQRSRecordUsuarios { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordTareas> PQRSRecordTareas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordCondiciones> PQRSRecordCondiciones { get; set; }
    }


    [Table("PQRSRecordUsuario")]
    public class PQRSRecordUsuario
    {

        [Display(Name = "Id")]//PK
        public int PQRSRecordId { get; set; }

        [Display(Name = "Order")]//PK
        public int PQRSRecordOrder { get; set; }

        [MaxLength(5)]
        [Display(Name = "User")]//Pk FK
        public string UsuarioId { get; set; }

        //Campo que señale quien hizo el comentario
        [Display(Name = "State")]
        public EstadoUsuarioFlujoPQRS EstadoUsuarioFlujoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PQRSRecord PQRSRecord { get; set; }
    }

    [Table("PQRSRecordTareas")]
    public class PQRSRecordTareas
    {

        [Display(Name = "Id")]//Pk FK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "PQRS Id")]//FK
        public int PQRSRecordId { get; set; }

        [Display(Name = "PQRS Order")]//FK
        public int PQRSRecordOrder { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [Display(Name = "Required?")]
        public bool Requerido { get; set; }
        /*Respuestas*/
        [Display(Name = "Done")]
        public bool Terminado { get; set; }
        /*Respuestas*/

        [ScriptIgnore, JsonIgnore]
        public virtual PQRSRecord PQRSRecord { get; set; }
    }
    
    [Table("PQRSRecordCondiciones")]
    public class PQRSRecordCondiciones
    {

        [Display(Name = "Id")]//Pk FK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "PQRS Id")]//FK
        public int PQRSRecordId { get; set; }

        [Display(Name = "PQRS Order")]//FK
        public int PQRSRecordOrder { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Condition Type")]
        public TipoCondicion TipoCondicion { get; set; }

        [Display(Name = "Condition")]
        public CondicionesValor? CondicionesValor { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Value (Case to apply condition)")]
        public decimal? Valor { get; set; }

        [Display(Name = "YesNo (Case to apply condition)")]
        public bool SiNo { get; set; }

        /*Respuestas*/
        [Column(TypeName = "Money")]
        [Display(Name = "Value (answer)")]
        public decimal? RespValor { get; set; }

        [Display(Name = "YesNo (answer)")]
        public bool RespSiNo { get; set; }

        /*Respuestas*/

        [ScriptIgnore, JsonIgnore]
        public virtual PQRSRecord PQRSRecord { get; set; }
    }

    [Table("PQRSRecordComentario")]
    public class PQRSRecordComentario
    {
        [Display(Name = "Id")]//PK llave de la tabla consecutivo
        public int Id { get; set; }

        [Display(Name = "Id")]//
        public int PQRSRecordId { get; set; }

        [Display(Name = "Order")]//
        public int PQRSRecordOrder { get; set; }

        [MaxLength(5)]
        [Display(Name = "User")]//
        public string UsuarioId { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Comment")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Comentario { get; set; }

        [Display(Name = "Type")]
        public TipoComentario TipoComentario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordArchivo> PQRSRecordArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordDocumento> PQRSRecordDocumentos { get; set; }

    }

    [Table("PQRSRecordArchivo")]
    public class PQRSRecordArchivo
    {
        [Display(Name = "Comment Id")]//PK llave de la tabla consecutivo
        public int PQRSRecordComentarioId { get; set; }

        public int Item { get; set; }//PK

        [Required]
        public byte[] File { get; set; }

        [Required]
        [StringLength(200)]
        public string FileName { get; set; }

        [Required]
        [StringLength(100)]
        public string MediaType { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PQRSRecordComentario PQRSRecordComentario { get; set; }
    }

    [Table("PQRSRecordDocumento")]
    public class PQRSRecordDocumento
    {
        [Display(Name = "Comment Id")]//PK llave de la tabla consecutivo
        public int PQRSRecordComentarioId { get; set; }

        public int Item { get; set; }//PK 

        [Display(Name = "Document")]
        [StringLength(100)]
        public string NroDocumento { get; set; }

        [Display(Name = "Doc Type")]
        public int TipoDocSoporteId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual TipoDocSoporte TipoDocSoporte { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PQRSRecordComentario PQRSRecordComentario { get; set; }
    }

    [Table("TipoDocSoporte")]
    public class TipoDocSoporte
    {
        [Display(Name = "Id")]//PK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Nombre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordDocumento> PQRSRecordDocumentos { get; set; }
    }

    [Table("CausaPQRS")]
    public class CausaPQRS
    {
        [Display(Name = "Id")]//PK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Type PQRS")]
        public TipoPQRS TipoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DevolucionItem> DevolucionItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<GarantiaItem> GarantiaItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NovedadItem> NovedadItems { get; set; }
    }

    // VIEW MODELS ------------------------------------------------

    public class DevolucionViewModel
    {
        public Devolucion Devolucion { get; set; }
        public List<DevolucionItem> Items { get; set; }
        public List<int> ItemsDelete { get; set; }
        public List<Archivos> DevolucionArchivos { get; set; }

        public class Archivos
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }
    }

    public class GarantiaViewModel
    {
        public Garantia Garantia { get; set; }
        public List<GarantiaItem> Items { get; set; }
        public List<int> ItemsDelete { get; set; }
        public List<Archivos> GarantiaArchivos { get; set; }

        public class Archivos
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }
    }

    public class NovedadViewModel
    {
        public Novedad Novedad { get; set; }
        public List<NovedadItem> Items { get; set; }
        public List<int> ItemsDelete { get; set; }
        public List<Archivos> NovedadArchivos { get; set; }

        public class Archivos
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }
    }

    public class FlujoPQRSViewModel
    {
        public List<FlujoPQRS> Flujo { get; set; }
    }

    public class UsuarioFlujoPQRSViewModel
    {
        public List<UsuariosStep> UsuariosStepList { get; set; }

        public class UsuariosStep
        {
            public int MotivoPQRSId { get; set; }
            public int FlujoPQRSId { get; set; }
            [Display(Name = "User")]
            public string UsuarioId { get; set; }
            public string UsuarioNombre { get; set; }
            [Display(Name = "Select")]
            public bool check { get; set; }
        }

    }

    public class PQRSPanel
    {

        [Display(Name = "Type")]
        public int TipoPQRS { get; set; }//PK

        [Display(Name = "Id")]
        public int Id { get; set; }//PK Data Id

        [Display(Name = "Tracking")]
        public string NroTracking { get; set; }

        [Display(Name = "PQRS")]
        public int PQRSRecordId { get; set; }//PK

        [Display(Name = "PQRS Order")]
        public int PQRSRecordOrder { get; set; }//PK

        [Display(Name = "Step")]
        public string FlujoPQRSNombre { get; set; }

        [Display(Name = "Reason")]
        public string MotivoPQRSNombre { get; set; }

        [Display(Name = "Date")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "User")]
        public string UsuarioIdCreacion { get; set; }

        [Display(Name = "Status")]
        public EstadoFormatoPQRS Estado { get; set; }

        [Display(Name = "Priority")]
        public Prioridad Prioridad { get; set; }

        [Display(Name = "Reporting user")]
        public string Persona { get; set; }

        [Display(Name = "Customer")]
        public string ClienteId { get; set; }

        [Display(Name = "CS Analyst")]
        public string AnalistaId { get; set; }

        [Display(Name = "Subject")]
        public string Asunto { get; set; }

        public int Days { get; set; }
    }

    public class AsignarAnalistaViewModel
    {

        [Display(Name = "Type PQRS")]
        public TipoPQRS TipoPQRS { get; set; }

        [Display(Name = "Return or Guarantee or New Id")]
        public int DataId { get; set; }

        [Display(Name = "Subject")]//Pk FK
        public string Asunto { get; set; }

        [Required]
        [MaxLength(5)]
        [Display(Name = "Analyst")]//Pk FK
        public string AnalistaId { get; set; }

        [Required]
        [Display(Name = "Comment")]//Pk FK
        public string Comment { get; set; }
    }

    public class AddCommentViewModel
    {

        [Display(Name = "Type PQRS")]
        public TipoPQRS TipoPQRS { get; set; }


        [Display(Name = "Data Id")]//
        public int DataId { get; set; }

        [Display(Name = "Id")]//
        public int PQRSRecordId { get; set; }

        [Display(Name = "Order")]//
        public int PQRSRecordOrder { get; set; }

        [Display(Name = "Subject")]//Pk FK
        public string Asunto { get; set; }

        [Required]
        [AllowHtml]
        [Display(Name = "Comment")]//Pk FK
        public string Comment { get; set; }

        [Display(Name = "Type")]
        public TipoComentario TipoComentario { get; set; }

        [AllowHtml]
        [Display(Name = "Description")]
        public string FlujoPQRSDescripcion { get; set; }


        //public List<Archivos> PQRSRecordArchivos { get; set; }
        public List<PQRSRecordDocumento> PQRSRecordDocumentos { get; set; }
        public List<HttpPostedFileBase> Files { get; set; }

        public List<PQRSRecordTareas> Tareas { get; set; }
        public List<CondicionesView> Condiciones { get; set; }

        public class Archivos
        {
            public int Item { get; set; }
            public string FileName { get; set; }
        }

        public class CondicionesView
        {
            public PQRSRecord PQRSRecord { get; set; }
            public List<PQRSRecordCondiciones> Condiciones { get; set; }
        }

    }


    public class PQRSTimeLineViewModel
    {
        public Formato formato { get; set; }
        public List<Comentarios.Archivos> formatoArchivos { get; set; }
        public List<FormatoItems> formatoItems { get; set; }

        public List<PQRSRecord> PQRSRecords { get; set; }
        public List<Comentarios> PQRSRecordComentarios { get; set; }



        public class Comentarios
        {
            public PQRSRecordComentario PQRSRecordComentarios { get; set; }
            public List<Archivos> PQRSRecordArchivos { get; set; }
            public List<PQRSRecordDocumento> PQRSRecordDocumentos { get; set; }

            public class Archivos
            {
                public int Id { get; set; }
                public int Item { get; set; }
                public string FileName { get; set; }
            }
        }

        public class FormatoItems
        {

            [Display(Name = "Id")]
            public int Id { get; set; }//PK

            [Display(Name = "Product")]
            public int ItemId { get; set; }

            [Display(Name = "Qty")]
            public int Cantidad { get; set; }

            [Display(Name = "Price")]
            public decimal Precio { get; set; }

            [Display(Name = "Invoice")]
            public string NroFactura { get; set; }

            [Display(Name = "Tracking number")]
            public string NroGuia { get; set; }

            [Display(Name = "Reason")]
            public int MotivoPQRSId { get; set; }

            [Display(Name = "Comments")]
            public string ComentarioAdicional { get; set; }

            [Display(Name = "Status")]
            public EstadoFormatoItemPQRS? Estado { get; set; }

            [Display(Name = "Received Qty")]
            public int? CantidadRecibida { get; set; }

            [Display(Name = "Loaded Qty")]
            public int? CantidadSubida { get; set; }

            [Display(Name = "Status product")]
            public string ComentarioEstadoMercancia { get; set; }

            [Display(Name = "Support document")]
            public string DocSoporte { get; set; }

            [ScriptIgnore, JsonIgnore]
            public virtual MotivoPQRS MotivoPQRS { get; set; }

            [ScriptIgnore, JsonIgnore]
            public virtual Item Items { get; set; }
        }

        public class Formato
        {

            [Display(Name = "Type PQRS")]
            public TipoPQRS TipoPQRS { get; set; }

            [Display(Name = "Id")]
            public int Id { get; set; }

            [Display(Name = "Date")]
            public DateTime FechaCreacion { get; set; }

            [Display(Name = "User")]
            public string UsuarioIdCreacion { get; set; }

            [Display(Name = "Status")]
            public EstadoFormatoPQRS Estado { get; set; }

            [Display(Name = "Priority")]
            public Prioridad Prioridad { get; set; }

            [Display(Name = "Reporting user")]
            public TipoPersona TipoPersona { get; set; }

            [Display(Name = "Reporting user")]
            public string Persona { get; set; }

            [Display(Name = "Customer")]
            public string ClienteId { get; set; }

            [Display(Name = "Analyst")]
            public string AnalistaId { get; set; }

            [Display(Name = "Subject")]
            public string Asunto { get; set; }

            [Display(Name = "Comment")]
            public string Observacion { get; set; }

            [Display(Name = "Recipients")]
            public string Destinatarios { get; set; }

            [Display(Name = "Tracking No")]
            public string NroTracking { get; set; }
        }
    }

    public class PQRSMotivoViewModel
    {
        public int DataId { get; set; }
        public int MotivoPQRSId { get; set; }
        public int? PQRSRecordId { get; set; }
    }

    public class FlujoPQRSConfigViewModel
    {
        public FlujoPQRS Flujo { get; set; }
        public List<FlujoPQRSTareas> Tareas { get; set; }
        public List<FlujoPQRSCondiciones> Condiciones { get; set; }
    }

    public class PQRSUsuarioStepViewModel
    {
        public List<PQRSRecordUsuario> Usuarios { get; set; }
        public List<PQRSRecordTareas> Tareas { get; set; }
        public List<PQRSRecordCondiciones> Condiciones { get; set; }
    }
}