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
    [Serializable]
    [Table("Rol")]
    public class Rol
    {
        [Key]
        [Required]
        [Display(Name = "Role")]
        public int RolId { get; set; }


        [Required]
        [MaxLength(100)]
        [Display(Name = "Description")]
        public string RolNombre { get; set; }


        public virtual ICollection<RolUsuario> RolUsuarioList { get; set; }
        public virtual ICollection<RolObjeto> RolObjetoList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<RolAplicacion> RolAplicaciones { get; set; }

    }

    [Serializable]
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Required]
        [MaxLength(5)]
        [Display(Name = "Code")]
        public string UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Username")]
        public string UsuarioNombre { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Mail")]
        public string UsuarioCorreo { get; set; }

        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Usuariopassword { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool Usuarioactivo { get; set; }

        [Column(TypeName = "Money")]
        [Display(Name = "Amount for authorization")]
        public decimal? UsuarioMontoAut { get; set; }


        [Display(Name = "Primary Approver")]
        public bool? UsuarioAprobadorPrincipal { get; set; }//Add 13/02/2017 Carlos Delgado.

        [Display(Name = "Manager")]
        public string UsuarioPadreId { get; set; }


        [Display(Name = "CS Analyst")]
        public bool UsuarioAnalistaSC { get; set; }

        [Display(Name = "Usuario debe cambiar contraseña")]
        public bool? UsuarioCambiaContrasena { get; set; }

        //Cayo 12/11/2018
        [MaxLength(4)]
        [Display(Name = "PlantaID")]
        public string PlantaID { get; set; }

        public Usuario UsuarioPadre { get; set; }

        public virtual ICollection<Usuario> UsuariosList { get; set; }
        public virtual ICollection<RolUsuario> RolUsuarioList { get; set; }
        public virtual ICollection<ActividadAutorizacion> ActividadAutorizacionesList { get; set; }
        public virtual ICollection<Cliente> ClienteList { get; set; }
        
        public virtual ICollection<Orden> OrdenList { get; set; }
        public virtual ICollection<Movimiento> MovimientoList { get; set; }

        //public virtual ICollection<PresupuestoVendedor> PresupuestoVendedorList { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioPlanta> UsuarioPlantas { get; set; }
        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioCanal> UsuarioCanales { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual UsuarioHV UsuarioHV { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Plantas planta { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioToken> UsuarioTokensList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Visita> VisitasList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionArchivo> LiquidacionArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<LiquidacionAprobacion> LiquidacionAprobaciones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Devolucion> DevolucionesUsuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Devolucion> DevolucionesAnalista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Garantia> GarantiasUsuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Garantia> GarantiasAnalista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Novedad> NovedadesUsuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Novedad> NovedadesAnalista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<DisponibilidadArchivo> DisponibilidadArchivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<UsuarioFlujoPQRS> UsuarioFlujoPQRS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Peak> Peaks { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Peak> PeaksPadre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakRevision> PeakRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivoRevision> PeakObjetivosRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordUsuario> PQRSRecordUsuarios { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PQRSRecordComentario> PQRSRecordComentarios { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsUS { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsAM { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsHR { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsIB { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsAnalista { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Recruitment> RecruitmentsUsuarioCreacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<NivelesAprobacion> NivelesAprobacion { get; set; }
    }

    [Table("UsuarioToken")]
    public class UsuarioToken
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        //[Index("IXU_Usuario", 1, IsUnique = true)]
        [Display(Name = "User Code")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Required]
        [Display(Name = "Token")]
        [StringLength(350)]
        public string Token { get; set; }

        [Required]
        [Display(Name = "Creation date")]
        public DateTime FechaCreacion { get; set; }

        [Required]
        [Display(Name = "Date of Last Use")]
        public DateTime FechaUltimoUso { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }

    [Serializable]
    [Table("RolUsuario")]
    public class RolUsuario
    {
        [Key]
        //[Required]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; }

        [Key]
        //[Required]
        [Display(Name = "Rol")]
        public int RolId { get; set; }

        public Usuario Usuario { get; set; }
        public Rol Rol { get; set; }

    }


    [Table("UsuarioPlanta")]
    public class UsuarioPlanta
    {
        [Key]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; }

        [Key]
        [Display(Name = "Planta")]
        public string PlantaId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Usuario Usuario { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Plantas Planta { get; set; }
    }

    [Table("UsuarioCanal")]
    public class UsuarioCanal
    {
        [Key]
        [Display(Name = "Usuario")]
        public string UsuarioId { get; set; }

        [Key]
        [Display(Name = "canal")]
        public string CanalId { get; set; }

        [ScriptIgnore, JsonIgnore]
        public Usuario Usuario { get; set; }
        [ScriptIgnore, JsonIgnore]
        public Canal Canal { get; set; }
    }

    [Serializable]
    [Table("Objeto")]
    public class Objeto
    {
        [Key]
        [Required]
        [MaxLength(100)]
        [Display(Name = "Object")]
        public string ObjetoId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Description")]
        public string ObjetoDesc { get; set; }

        [Display(Name = "Menu")]
        public bool ObjetoMenu { get; set; }

        [Display(Name = "Parent menu")]
        [StringLength(100)]
        public string ObjetoIdPadre { get; set; }

        [Display(Name = "Icon")]
        [StringLength(50)]
        public string ObjetoIcono { get; set; }

        [Display(Name = "Order")]
        public int? ObjetoOrden { get; set; }

        public Objeto ObjetoPadre { get; set; }

        public virtual ICollection<RolObjeto> RolObjetoList { get; set; }
        public virtual ICollection<Objeto> ObjetoList { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<AplicacionObjeto> AplicacionObjetos { get; set; }
    }

    [Serializable]
    [Table("RolObjeto")]
    public class RolObjeto
    {
        [Key]
        [Required]
        [Display(Name = "Role Id")]
        public int RolId { get; set; }

        [Key]
        [Required]
        [MaxLength(100)]
        [Display(Name = "Object")]
        public string ObjetoId { get; set; }

        [Display(Name = "Active")]
        public bool RolObjetoActivo { get; set; }

        public Objeto Objeto { get; set; }
        public Rol Rol { get; set; }
    }


    //[Serializable]
    [Table("Auditoria")]
    public class Auditoria
    {
        [Key]
        [Display(Name = "Nro")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Int64 AuditoriaId { get; set; }

        [Display(Name = "Date")]
        public System.DateTime AuditoriaFecha { get; set; }

        [Display(Name = "Hour")]
        public System.TimeSpan AuditoriaHora { get; set; }

        [Display(Name = "User Code")]
        // [ForeignKey("usuarios")]
        [StringLength(5)]
        public string usuarioId { get; set; }

        [Display(Name = "Event")]
        [MaxLength(30)]
        public string AuditoriaEvento { get; set; }

        [Display(Name = "Description")]
        public string AuditoriaDesc { get; set; }

        [Display(Name = "Object")]
        [StringLength(100)]
        public string ObjetoId { get; set; }

        [Display(Name = "Computer name")]
        [StringLength(100)]
        public string AuditoriaEquipo { get; set; }
    }

    [Table("Log")]
    public class Log
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Display(Name = "Date")]
        public DateTime Fecha { get; set; }

        [Required]
        [Display(Name = "User")]
        [StringLength(50)]
        public string Usuario { get; set; }

        [Display(Name = "Data")]
        public string Data { get; set; }

        [Required]
        [Display(Name = "Client")]
        [StringLength(100)]
        public string Cliente { get; set; }

        [Display(Name = "Event")]
        public int EventoId { get; set; }

        [Display(Name = "Key")]
        [StringLength(100)]
        public string Key { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Evento Evento { get; set; }
    }

    [Table("Evento")]
    public class Evento
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("IXU_Nombre", 1, IsUnique = true)]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(100)]
        public string Descripcion { get; set; }

        [Display(Name = "Active")]
        public bool Activo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Log> Logs { get; set; }
    }

    // [Serializable]
    public class Login
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "Username")]
        public string Usuario { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        //[Display(Name = "¿Recordar cuenta?")]
        //public bool RememberMe { get; set; }
    }

    //[Serializable]
    public class ResetPassword
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "User Code")]
        public string Usuario { get; set; }

        [Display(Name = "Username")]
        public string UsuarioNombre { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password1 { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password1", ErrorMessage = "Both passwords must be the same")]
        [Display(Name = "Confirm Password")]
        public string Password2 { get; set; }
    }

    //[Serializable]
    public class RememberUser
    {
        [Required]
        [Display(Name = "User or Email")]
        public string UserOrMail { get; set; }
    }


    public class ChangePassword
    {
        [Required]
        [MaxLength(20)]
        [Display(Name = "User Code")]
        public string Usuario { get; set; }

        [Display(Name = "Username")]
        public string UsuarioNombre { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        [Remote("ValidatePassword", "api/Usuarios", AdditionalFields = "Usuario", ErrorMessage = "The old password is not valid", HttpMethod = "GET")]
        public string PasswordOld { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password1 { get; set; }

        [Required]
        [MaxLength(20), MinLength(5)]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password1", ErrorMessage = "Both passwords must be the same")]
        [Display(Name = "Confirm Password")]
        public string Password2 { get; set; }
    }

    //[Serializable]
    public class Errores
    {
        public string titulo { get; set; }
        public string mensaje { get; set; }
        public string objeto { get; set; }
    }

    [Table("Aplicacion")]
    public class Aplicacion
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [Index("IXU_AplicacionLink", 1, IsUnique = true)]
        [Display(Name = "Link")]
        [StringLength(5)]
        public string Link { get; set; }

        [Required]
        [Display(Name = "Default Index")]
        [StringLength(100)]
        public string Inicio { get; set; }

        [Display(Name = "Enabled")]
        public bool Activo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<RolAplicacion> RolAplicaciones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<AplicacionObjeto> AplicacionObjetos { get; set; }
    }

    [Serializable]
    [Table("RolAplicacion")]
    public class RolAplicacion
    {
        [Key]
        [Required]
        [Display(Name = "Role Id")]
        public int RolId { get; set; }

        [Key]
        [Required]
        [Display(Name = "App")]
        public int AplicacionId { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual Aplicacion Aplicacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Rol Rol { get; set; }
    }

    [Serializable]
    [Table("AplicacionObjeto")]
    public class AplicacionObjeto
    {
        [Key]
        [Required]
        [Display(Name = "App")]
        public int AplicacionId { get; set; }

        [Key]
        [Required]
        [MaxLength(100)]
        [Display(Name = "Object")]
        public string ObjetoId { get; set; }


        [ScriptIgnore, JsonIgnore]
        public virtual Aplicacion Aplicacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Objeto Objeto { get; set; }
    }


    [Serializable]
    public class Seguridadcll
    {
        public Seguridadcll()
        {
            // Seguridadcll = new Seguridadcll();
            Usuario = new Usuario();
            UsuariosHijos = new List<Usuario>();
            RolObjetoList = new List<RolObjeto>();
            RolUsuarioList = new List<RolUsuario>();
            ClienteList = new List<Cliente>();
            PlantaList = new List<Plantas>();
            ObjetosMenuList = new List<Objeto>();
            ObjetosMenuDirectorioList = new List<Objeto>();
            Configuracion = new Configuracion();
            Aplicaciones = new List<Aplicacion>();
        }

        public Usuario Usuario { get; set; }
        public List<Usuario> UsuariosHijos { get; set; }
        public List<RolObjeto> RolObjetoList { get; set; }
        public List<RolUsuario> RolUsuarioList { get; set; }
        public List<Cliente> ClienteList { get; set; }
        public List<Plantas> PlantaList { get; set; }
        public List<Objeto> ObjetosMenuList { get; set; }
        public List<Objeto> ObjetosMenuDirectorioList { get; set; }
        public Configuracion Configuracion { get; set; }
        public List<Aplicacion> Aplicaciones { get; set; }
        public Aplicacion Aplicacion { get; set; }
    }

    [Table ("Correos")]
    public class Correos 
    {        

        [Display(Name = "Id")]//PK
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "E-Mail")]
        public string Mail { get; set; }
        
        [Display(Name = "Active")]
        public bool Active { get; set; }

        [Required]
        [Display(Name = "Grupo")]
        [StringLength(20)]
        public string Grupo { get; set; }

    }

    // VIEW MODELS ------------------------------------------------


    public class SendMailTest
    {
        [Required]
        [Display(Name = "To")]
        public string To { get; set; }

        [Required]
        [Display(Name = "To Name")]
        public string ToName { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }

    public class RolAplicacionViewModel
    {
        public Rol rol { get; set; }
        public List<AplicacionView> aplicaciones { get; set; }

        public class AplicacionView
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public bool Seleccionado { get; set; }
        }
    }


    public class AplicacionesViewModel
    {
        public Aplicacion Aplicacion { get; set; }
        public List<AplicacionObjetosViewModel> Objetos { get; set; }

        public class AplicacionObjetosViewModel
        {
            [Display(Name = "Object")]
            public string ObjetoId { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

    }

    public class ObjetosViewModel
    {
        public Objeto Objeto { get; set; }
        public List<AplicacionViewModel> Apps { get; set; }

        public class AplicacionViewModel
        {
            [Display(Name = "Id")]
            public int AplicacionId { get; set; }

            [Display(Name = "App")]
            public string AplicacionNombre { get; set; }


            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

    }

    public class RolesViewModel
    {
        public Rol Rol { get; set; }
        public List<RolObjetosViewModel> Objetos { get; set; }
        public List<AplicacionViewModel> Apps { get; set; }
        public class RolObjetosViewModel
        {
            [Display(Name = "Object")]
            public string ObjetoId { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

        public class AplicacionViewModel
        {
            [Display(Name = "Id")]
            public int AplicacionId { get; set; }

            [Display(Name = "App")]
            public string AplicacionNombre { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

    }

    public class UsuariosViewModel
    {
        public Usuario Usuario { get; set; }
        public UsuarioHV UsuarioHV { get; set; }
        public List<RolesViewModel> Roles { get; set; }
        public List<PlantasViewModel> Plantas { get; set; }
        public List<CanalesViewModel> Canales { get; set; }
        public class RolesViewModel
        {
            [Display(Name = "Role")]
            public int RolId { get; set; }

            [Display(Name = "Role")]
            public string RolNombre { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

        public class PlantasViewModel
        {
            [Display(Name = "Planta")]
            public string PlantaId { get; set; }

            [Display(Name = "Planta")]
            public string PlantaNombre { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }

        public class CanalesViewModel
        {
            [Display(Name = "Canal")]
            public string CanalId { get; set; }

            [Display(Name = "Canal")]
            public string CanalNombre { get; set; }

            [Display(Name = "Selected")]
            public bool Seleccionado { get; set; }
        }
    }
}