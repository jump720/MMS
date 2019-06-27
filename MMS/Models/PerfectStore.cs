using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace MMS.Models
{
    [Table("Marca")]
    public class Marca
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Item> Items { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaPublicidad> VisitaPublicidades { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Regla> Reglas { get; set; }
    }

    [Table("Item")]
    public class Item
    {
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Marca")]
        public int MarcaId { get; set; }

        [Required]
        [Display(Name = "Código")]
        [Index("IXU_Codigo", 1, IsUnique = true)]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Required]
        [Display(Name = "Categoria")]
        [StringLength(50)]
        public string Categoria { get; set; }

        [Required]
        [Display(Name = "Grupo")]
        [StringLength(100)]
        public string Grupo { get; set; }

        [Required]
        [Display(Name = "Descrição")]
        [StringLength(150)]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Preço Base")]
        public decimal PrecioSugerido { get; set; }


        [Display(Name = "Multiplicidade")]
        public int? UnidadEmpaque { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Marca Marca { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PlantillaItem> ItemPlantillas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaItem> ItemVisitas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ColeccionPIVItem> ItemColeccionesPIV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DevolucionItem> DevolucionItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ItemDisponibilidad> ItemDisponibilidades { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Regla> Reglas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<GarantiaItem> GarantiaItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NovedadItem> NovedadItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DisponibilidadArchivoItem> DisponibilidadArchivoItems { get; set; }
    }

    [Table("Plantilla")]
    public class Plantilla
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Display(Name = "Enabled")]
        public bool Activa { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PlantillaItem> PlantillaItems { get; set; }
    }

    [Table("PlantillaItem")]
    public class PlantillaItem
    {
        [Display(Name = "Plantilla")]
        public int PlantillaId { get; set; }

        [Display(Name = "Item")]
        public int ItemId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Plantilla Plantilla { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }
    }

    [Table("Visita")]
    public class Visita
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Establishment name")]
        [Index("IX_NombreEstablecimiento")]
        public string NombreEstablecimiento { get; set; }

        [Required]
        [Display(Name = "Nit")]
        [StringLength(15)]
        public string Nit { get; set; }

        [Display(Name = "Dv")]
        public int Dv { get; set; }

        [Required]
        [Display(Name = "Business name")]
        [StringLength(50)]
        public string RazonSocial { get; set; }

        [Display(Name = "Manager")]
        [StringLength(30)]
        public string Administrador { get; set; }

        [StringLength(15)]
        [Display(Name = "Phone")]
        public string Telefono { get; set; }

        [Required]
        [Display(Name = "Country")]
        [StringLength(3)]
        public string PaisId { get; set; }

        [Required]
        [Display(Name = "State")]
        [StringLength(3)]
        public string DepartamentoId { get; set; }

        [Required]
        [Display(Name = "City")]
        [StringLength(3)]
        public string CiudadId { get; set; }

        [Display(Name = "Neighborhood")]
        [StringLength(30)]
        public string Barrio { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(100)]
        public string Direccion { get; set; }

        [Display(Name = "Availability of Product")]
        public bool DisponibilidadProducto { get; set; }

        [Display(Name = "Interest to buy")]
        public bool InteresCompra { get; set; }

        [Display(Name = "Latitude")]
        public double? Latitud { get; set; }

        [Display(Name = "Longitude")]
        public double? Longitud { get; set; }

        [Required]
        [Display(Name = "User")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "Date")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Confirmation date")]
        public DateTime FechaConfirmacion { get; set; }

        [Display(Name = "Active")]
        public bool Activa { get; set; }

        [Display(Name = "Completed")]
        public bool Completada { get; set; }

        [Display(Name = "Invoiced")]
        public decimal? VentaRealizada { get; set; }

        [Display(Name = "Comments")]
        public string Comentarios { get; set; }

        [Display(Name = "Type of visit")]
        public int? TipoVisitaId { get; set; }

        [Display(Name = "Type of industry")]
        public int? TipoIndustriaId { get; set; }

        [Display(Name = "Number of mechanics")]
        public int? NumeroMecanicos { get; set; }

        [StringLength(50)]
        [Display(Name = "Brans")]
        public string Marcas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Ciudad Ciudad { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual TipoVisita TipoVisita { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual TipoIndustria TipoIndustria { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaFoto> VisitaFotos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaItem> VisitaItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaCliente> VisitaClientes { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaPublicidad> VisitaPublicidades { get; set; }
    }

    [Table("VisitaFoto")]
    public class VisitaFoto
    {
        public int VisitaId { get; set; }

        public int Order { get; set; }

        [Required]
        public byte[] Foto { get; set; }

        [Required]
        [StringLength(50)]
        public string MediaType { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Visita Visita { get; set; }
    }

    [Table("VisitaItem")]
    public class VisitaItem
    {
        public int VisitaId { get; set; }

        public int ItemId { get; set; }

        public int Order { get; set; }

        public decimal PrecioVenta { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Visita Visita { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Item Item { get; set; }
    }

    [Table("VisitaCliente")]
    public class VisitaCliente
    {
        public int VisitaId { get; set; }

        [StringLength(15)]
        public string ClienteId { get; set; }

        public int Order { get; set; }

        public int? NroCompras { get; set; }

        public decimal? ValorCompras { get; set; }

        public decimal? ValorVentas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Visita Visita { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Cliente Cliente { get; set; }
    }

    [Table("VisitaPublicidad")]
    public class VisitaPublicidad
    {
        public int VisitaId { get; set; }

        public int Order { get; set; }

        public int Tipo { get; set; }

        public int MarcaId { get; set; }

        public int? Nivel { get; set; }

        public byte[] Foto { get; set; }

        [StringLength(50)]
        public string MediaType { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Visita Visita { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Marca Marca { get; set; }
    }

    [Table("TipoVisita")]
    public class TipoVisita
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Visita> VisitasList { get; set; }
    }


    [Table("TipoIndustria")]
    public class TipoIndustria
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Visita> VisitasList { get; set; }
    }


    // VIEW MODELS ----------------------------------------------------

    public class PlantillaViewModel
    {
        public Plantilla Plantilla { get; set; }
        public List<PlantillaItem> PlantillaItems { get; set; }
    }

    public class VisitasViewModel
    {
        public Visita Visita { get; set; }
        public List<Fotos> VisitaFotos { get; set; }
        public List<VisitaItem> VisitaItems { get; set; }
        public List<VisitaCliente> VisitaClientes { get; set; }
        public List<Publicidad> VisitaPublicidad { get; set; }

        public class Fotos
        {
            public int Order { get; set; }
            public string FileName { get; set; }
        }

        public class Publicidad
        {
            public int VisitaId { get; set; }

            public int Order { get; set; }

            public int Tipo { get; set; }

            public int MarcaId { get; set; }

            public int? Nivel { get; set; }                       
        }

    }


    public class EstablecimientosViewModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Display(Name = "Establishment name")]
        public string NombreEstablecimiento { get; set; }

        [Display(Name = "Country")]
        public string Pais { get; set; }

        [Display(Name = "State")]
        public string Departamento { get; set; }

        [Display(Name = "City")]
        public string Ciudad { get; set; }

        [Display(Name = "Address")]
        public string Direccion { get; set; }

        [Display(Name = "Latitude")]
        public double? Latitud { get; set; }

        [Display(Name = "Longitude")]
        public double? Longitud { get; set; }

        [Display(Name = "User")]
        public string UsuarioId { get; set; }

        [Display(Name = "Date")]
        public DateTime Fecha { get; set; }

    }
}