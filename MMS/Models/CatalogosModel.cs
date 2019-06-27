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
    //public class CatalogosModel
    //{
    //}
    [Serializable]
    [Table("Canal")]
    public class Canal
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string CanalID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string CanalDesc { get; set; }

        public virtual ICollection<Actividad> ActividadList { get; set; }
        public virtual ICollection<PresupuestoVendedor> PresupuestoVendedor { get; set; }
        public virtual ICollection<Cliente> ClienteList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NivelesAprobacion> NivelesAprobacion { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioCanal> UsuarioCanales { get; set; }


    }

    [Table("TipoActividad")]
    public class TipoActividad
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Codigo")]
        public string TipoActividadID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string TipoActividadDesc { get; set; }

        public virtual ICollection<Actividad> ActividadList { get; set; }

    }

    [Table("TipoGasto")]
    public class TipoGasto
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "ID")]
        public string TipoGastoID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string TipoGastoDesc { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Gasto> GastoList { get; set; }

    }

    [Table("TipoProducto")]
    public class TipoProducto
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string TipoProductoID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string TipoProductoDesc { get; set; }

        public virtual ICollection<Producto> ProductoList { get; set; }

    }

    [Table("CentroCosto")]
    public class CentroCosto
    {
        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string CentroCostoID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string CentroCostoDesc { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PresupuestoVendedor> PresupuestoVendedor { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Gasto> Gasto { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<ActividadItem> ActividadItem { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<OrdenItems> OrdenItems { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> Recruitments { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> ProposedRecruitments { get; set; }

    }

    [Table("Plantas")]
    public class Plantas
    {
        [Key]
        [MaxLength(4)]
        [Display(Name = "PlantaID")]
        public string PlantaID { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Planta Desc")]
        public string PlantaDesc { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PresupuestoVendedor> PresupuestosVendedor { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Actividad> Actividades { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Cliente> Clientes { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Usuario> Usuario { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NivelesAprobacion> NivelesAprobacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioPlanta> UsuarioPlantas { get; set; }
       
    }

    [Table("TipoMovimiento")]
    public class TipoMovimiento
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string TipoMovimientoID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Nome")]
        public string TipoMovimientoDesc { get; set; }

        public virtual ICollection<Movimiento> MovimientoList { get; set; }

    }

    [Table("Pais")]
    public class Pais
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string PaisID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "País")]
        public string PaisDesc { get; set; }


        public virtual ICollection<Departamento> departamentoList { get; set; }
    }


    [Table("Departamento")]
    public class Departamento
    {

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string DepartamentoID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Estado")]
        public string DepartamentoDesc { get; set; }

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "País")]
        public string PaisID { get; set; }


        public Pais paises { get; set; }

        public virtual ICollection<Ciudad> ciudadList { get; set; }
    }

    [Serializable]
    [Table("Ciudad")]
    public class Ciudad
    {
        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Código")]
        public string CiudadID { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Cidade")]
        public string CiudadDesc { get; set; }

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "País")]
        public string PaisID { get; set; }

        [Key]
        [Required]
        [MaxLength(3)]
        [Display(Name = "Estado")]
        public string DepartamentoID { get; set; }

        public Departamento departamentos { get; set; }

        public virtual ICollection<Cliente> ClienteList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Visita> Visitas { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Asesor> Asesores { get; set; }
    }

    [Table("Area")]
    public class Area
    {
        [Display(Name = "ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nome")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Peak> Peaks { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioHV> UsuarioHVs { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> Recluiments { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> ProposedRecluiments { get; set; }

    }

    [Serializable]
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        [Required]
        [MaxLength(50)]
        [Display(Name = "Código")]
        public string ClienteID { get; set; }

        [Required]
        [MaxLength(50)]//Cayo
        [Display(Name = "Código")]
        public string ClienteNit { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Name")]
        public string ClienteRazonSocial { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "País")]
        public string PaisID { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Estado")]
        public string DepartamentoID { get; set; }

        [Required]
        [MaxLength(3)]
        [Display(Name = "Cidade")]
        public string CiudadID { get; set; }

        [Required]
        [MaxLength(5)]
        [Display(Name = "Coordenador")]
        public string VendedorId { get; set; }//Viene de la tabla Usuario

        [MaxLength(3)]
        [Display(Name = "Channel")]
        public string CanalID { get; set; }

        [MaxLength(4)]
        [Display(Name = "PlantaID")]
        public string PlantaID { get; set; }

        [Display(Name = "Necesita Aprobación Final")]
        public bool? ClienteAprobacion { get; set; }//Add 13/02/2017 Carlos Delgado.

        [Display(Name = "Representante")]
        public int? ColeccionPIVId { get; set; }

        //ADD 05/10  Cayo Diebe
        [MaxLength(10)]
        [Display(Name = "Matriz/Filial")]
        public string MatrizFilial { get; set; }
        
        //ADD 05/10  Cayo Diebe
        [MaxLength(14)]
        [Display(Name = "CNPJ")]
        public string CNPJ { get; set; }



        //public Actividad actividad { get; set; }
        public Ciudad ciudad { get; set; }
        public Usuario usuario { get; set; }
        public Canal canal { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Plantas planta { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ColeccionPIV ColeccionPIV { get; set; }

        public virtual ICollection<Actividad> ActividadList { get; set; }
        public virtual ICollection<VentasxCliente> VentasxClienteList { get; set; }
        //public virtual ICollection<PresupuestoVendedor> PresupuestoVendedorList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<VisitaCliente> VisitasClientes { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Devolucion> Devoluciones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Garantia> Garantias { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Novedad> Novedades { get; set; }
    }

    // [Serializable]
    public class ReporteActividades
    {
        [Display(Name = "No. Actividad")]
        public int? ActividadId { get; set; }

        [MaxLength(50)]
        [Display(Name = "Título Actividad")]
        public string ActividadTitulo { get; set; }

        [MaxLength(20)]
        [Display(Name = "Cod. Cliente")]
        public string ClienteID { get; set; }

        [MaxLength(50)]
        [Display(Name = "Nombre Cliente")]
        public string ClienteNombre { get; set; }
    }

    /*Clase para imprementar Server Side con DataTables.js*/
        public class SysDataTablePager
    {
        public string draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public dynamic data { get; set; }
    }

}