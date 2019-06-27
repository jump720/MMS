using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace MMS.Models
{
    [Table("CategoriaCDE")]
    public class CategoriaCDE
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Min. Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Min. Value must not be lower than 0.")]
        public decimal ValorMinimo { get; set; }

        [Display(Name = "Max. Value")]
        public decimal ValorMaximo { get; set; }

        [Display(Name = "Icon")]
        [StringLength(2)]
        public string Icon { get; set; }

        [Display(Name = "Settlement")]
        public int? LiquidacionId { get; set; }

        [Display(Name = "Percent")]
        [Range(0, float.MaxValue, ErrorMessage = "Percent must not be lower than 0.")]
        public float Porcentaje { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Liquidacion Liquidacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionCierre> LiquidacionCierres { get; set; }
    }

    [Table("ColeccionPIV")]
    public class ColeccionPIV
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Club of Excellence")]
        public bool CDE { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ColeccionPIVItem> ColeccionPIVItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Asesor> Asesores { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Cliente> Clientes { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionArchivo> LiquidacionArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ItemDisponibilidad> ItemDisponibilidades { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DisponibilidadArchivoItem> DisponibilidadArchivoItems { get; set; }
    }

    [Table("ColeccionPIVItem")]
    public class ColeccionPIVItem
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Remote("ValidateCodeItem", "api/ColeccionesPIV", AdditionalFields = "Id, ColeccionPIVId", ErrorMessage = "A PIP Collection Item already exists with this Code.", HttpMethod = "GET")]
        [Index("IXU_Codigo", 1, IsUnique = true)]
        [Display(Name = "Code")]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Index("IXU_Codigo", 2, IsUnique = true)]
        [Display(Name = "PIP Collection")]
        public int ColeccionPIVId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionItem> LiquidacionItems { get; set; }
    }

    [Table("Asesor")]
    public class Asesor
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Id")]
        [Remote("ValidateCedula", "api/ColeccionesPIV", AdditionalFields = "Id", ErrorMessage = "A Seller already exists with this Id.", HttpMethod = "GET")]
        [Index("IXU_Cedula", 1, IsUnique = true)]
        [StringLength(20)]
        public string Cedula { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Surname")]
        [StringLength(25)]
        public string Apellido1 { get; set; }

        [Display(Name = "Second Surname")]
        [StringLength(25)]
        public string Apellido2 { get; set; }

        [Display(Name = "Target")]
        [Range(1, double.MaxValue, ErrorMessage = "Target must be greater than 0.")]
        public decimal Meta { get; set; }

        [Display(Name = "PIP Collection")]
        public int ColeccionPIVId { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Country")]
        public string PaisId { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "State")]
        public string DepartamentoId { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "City")]
        public string CiudadId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Ciudad Ciudad { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionAsesor> LiquidacionAsesores { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionAprobacion> LiquidacionAprobaciones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionCierre> LiquidacionCierres { get; set; }
    }

    [Table("Liquidacion")]
    public class Liquidacion
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [Display(Name = "Month From")]
        [Column(TypeName = "date")]
        public DateTime FechaInicial { get; set; }

        [Display(Name = "Month Till")]
        [Column(TypeName = "date")]
        public DateTime FechaFinal { get; set; }

        [Display(Name = "State")]
        public EstadoLiquidacion Estado { get; set; }

        [Display(Name = "PIP Percent")]
        public float? PorcentajePIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionArchivo> LiquidacionArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionAprobacion> LiquidacionAprobaciones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<CategoriaCDE> CategoriasCDE { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionCierre> LiquidacionCierres { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Regla> Reglas { get; set; }
    }

    [Table("LiquidacionCierre")]
    public class LiquidacionCierre
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Settlement")]
        [Index("IXU_Asesor", 1, IsUnique = true)]
        public int LiquidacionId { get; set; }

        [Display(Name = "Seller")]
        [Index("IXU_Asesor", 2, IsUnique = true)]
        public int AsesorId { get; set; }

        [Display(Name = "Target")]
        public decimal Meta { get; set; }

        [Display(Name = "COE")]
        public bool CDE { get; set; }

        [Display(Name = "COE Category")]
        public int? CategoriaCDEId { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [Display(Name = "Rules")]
        public decimal TotalReglas { get; set; }

        [Display(Name = "Applied Percent")]
        public float PorcentajeAplicado { get; set; }

        [Display(Name = "New Total")]
        public decimal TotalNuevo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Liquidacion Liquidacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Asesor Asesor { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CategoriaCDE CategoriaCDE { get; set; }
    }

    [Table("LiquidacionArchivo")]
    public class LiquidacionArchivo
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [Display(Name = "Settlement")]
        public int LiquidacionId { get; set; }

        [Display(Name = "Uploaded Date")]
        public DateTime FechaSubida { get; set; }

        [Required]
        [Display(Name = "Uploaded by")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "PIP Collection")]
        public int ColeccionPIVId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Liquidacion Liquidacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionAsesor> LiquidacionAsesores { get; set; }
    }

    [Table("LiquidacionAsesor")]
    public class LiquidacionAsesor
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "File Settlement")]
        public int LiquidacionArchivoId { get; set; }

        [Display(Name = "Seller")]
        public int? AsesorId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual LiquidacionArchivo LiquidacionArchivo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Asesor Asesor { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionItem> LiquidacionItems { get; set; }
    }

    [Table("LiquidacionItem")]
    public class LiquidacionItem
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Seller")]
        public int LiquidacionAsesorId { get; set; }

        [Display(Name = "Month")]
        public int Mes { get; set; }

        [Display(Name = "Year")]
        public int Ano { get; set; }

        [Display(Name = "PIP Collection Item")]
        public int ColeccionPIVItemId { get; set; }

        [Display(Name = "Quantity")]
        public int Cantidad { get; set; }

        [Display(Name = "Sales (COP)")]
        public decimal ValorTotal { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual LiquidacionAsesor LiquidacionAsesor { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIVItem ColeccionPIVItem { get; set; }
    }

    [Table("LiquidacionAprobacion")]
    public class LiquidacionAprobacion
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Settlement")]
        [Index("IXU_Asesor", 1, IsUnique = true)]
        public int LiquidacionId { get; set; }

        [Display(Name = "Seller")]
        [Index("IXU_Asesor", 2, IsUnique = true)]
        public int AsesorId { get; set; }

        [Required]
        [Display(Name = "Observation")]
        [StringLength(500)]
        public string Observacion { get; set; }

        [Required]
        [Display(Name = "Approved by")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Liquidacion Liquidacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Asesor Asesor { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }

    [Table("ItemDisponibilidad")]
    public class ItemDisponibilidad
    {
        [Display(Name = "PIP Collection")]
        public int ColeccionPIVId { get; set; }

        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [Display(Name = "Quantity")]
        public int Cantidad { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }
    }

    [Table("DisponibilidadArchivo")]
    public class DisponibilidadArchivo
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Uploaded Date")]
        public DateTime FechaSubida { get; set; }

        [Required]
        [Display(Name = "Uploaded by")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "Month")]
        public int Mes { get; set; }

        [Display(Name = "Year")]
        public int Ano { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DisponibilidadArchivoItem> DisponibilidadArchivoItems { get; set; }
    }

    [Table("DisponibilidadArchivoItem")]
    public class DisponibilidadArchivoItem
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Stock File")]
        [Index("IXU_Item", 1, IsUnique = true)]
        public int DisponibilidadArchivoId { get; set; }

        [Display(Name = "Item")]
        [Index("IXU_Item", 3, IsUnique = true)]
        public int ItemId { get; set; }

        [Display(Name = "Quantity")]
        public int Cantidad { get; set; }

        [Display(Name = "PIP Collection")]
        [Index("IXU_Item", 2, IsUnique = true)]
        public int ColeccionPIVId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual DisponibilidadArchivo DisponibilidadArchivo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }
    }

    [Table("Regla")]
    public class Regla
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Brand")]
        public int? MarcaId { get; set; }

        [Display(Name = "Item")]
        public int? ItemId { get; set; }

        [Display(Name = "Target")]
        [Range(1, double.MaxValue, ErrorMessage = "Target must be greater than 0.")]
        public decimal Meta { get; set; }

        [Display(Name = "Percent")]
        [Range(0, float.MaxValue, ErrorMessage = "Percent must not be lower than 0.")]
        public float Porcentaje { get; set; }

        [Display(Name = "Enabled")]
        public bool Activa { get; set; }

        [Display(Name = "Settlement")]
        public int? LiquidacionId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Marca Marca { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Liquidacion Liquidacion { get; set; }
    }

    // VIEW MODELS ----------------------------------------------------

    public class ColeccionPIVViewModel
    {
        public ColeccionPIV ColeccionPIV { get; set; }
        public List<Cliente> Clientes { get; set; }
        public List<string> ClientesId { get; set; }
    }

    public class ReglaViewModel
    {
        public Regla Regla { get; set; }

        [Required]
        [Display(Name = "Type")]
        public TipoRegla Tipo { get; set; }

        public enum TipoRegla
        {
            Item = 1,
            Brand = 2
        }
    }

    public class DisponibilidadDetalleViewModel
    {
        [Display(Name = "Date")]
        public DateTime Fecha { get; set; }

        [Display(Name = "User")]
        public string Usuario { get; set; }

        [Display(Name = "Description")]
        public string Descripcion { get; set; }

        [Display(Name = "Quantity")]
        public int Cantidad { get; set; }
    }
}