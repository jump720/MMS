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
    [Table("Periodo")]
    public class Periodo
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(50)]
        public string Descripcion { get; set; }

        [Display(Name = "Start Date")]
        [Column(TypeName = "date")]
        public DateTime FechaIni { get; set; }

        [Display(Name = "Finish Date")]
        [Column(TypeName = "date")]
        public DateTime FechaFin { get; set; }

        [Display(Name = "Final Review Start Date")]
        [Column(TypeName = "date")]
        public DateTime RevisionFinalFechaIni { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeriodoRevision> PeriodoRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<Peak> Peaks { get; set; }
    }

    [Table("PeriodoRevision")]
    public class PeriodoRevision
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Period")]
        public int PeriodoId { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(30)]
        public string Nombre { get; set; }

        [Display(Name = "Start Date")]
        [Column(TypeName = "date")]
        public DateTime FechaIni { get; set; }

        [Display(Name = "Finish Date")]
        [Column(TypeName = "date")]
        public DateTime FechaFin { get; set; }

        [Display(Name = "Manually Enabled")]
        public bool ActivoManual { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Periodo Periodo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakRevision> PeakRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivoRevision> PeakObjetivosRevisiones { get; set; }
    }

    [Table("CoreValue")]
    public class CoreValue
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(500)]
        public string Descripcion { get; set; }

        [Required]
        [Display(Name = "Competency")]
        [StringLength(50)]
        public string Competencia { get; set; }

        [Required]
        [Display(Name = "Competency Description")]
        [StringLength(500)]
        public string CompetenciaDescripcion { get; set; }

        [Required]
        [Display(Name = "Highly Skilled")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string HabilidadAlta { get; set; }

        [Required]
        [Display(Name = "Skilled")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string HabilidadMedia { get; set; }

        [Required]
        [Display(Name = "Unskilled")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string HabilidadBaja { get; set; }

        [Display(Name = "Order")]
        public short Orden { get; set; }

        [Display(Name = "Enabled")]
        public bool Activo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakCoreValue> PeakCoreValues { get; set; }
    }

    [Table("Peak")]
    public class Peak
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index("IXU_Peak", 1, IsUnique = true)]
        [Display(Name = "User")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "Period")]
        [Index("IXU_Peak", 2, IsUnique = true)]
        public int PeriodoId { get; set; }

        [Display(Name = "Title")]
        [StringLength(50)]
        public string Cargo { get; set; }

        [Display(Name = "Area")]
        public int AreaId { get; set; }

        [Display(Name = "Manager")]
        [StringLength(5)]
        public string UsuarioIdPadre { get; set; }

        [Display(Name = "Summarize your key contributions to Apex Tool Group for the past year")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ResumenContribuciones { get; set; }

        [Display(Name = "Manager's Comments - Competencies")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ComentariosCompetencias { get; set; }

        [Display(Name = "Summarize the associate's contributions and overall performance, noting results related to objectives and general job duties, to support the merit rating")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ResumenContribucionesJefe { get; set; }

        [Display(Name = " Describe 2-3 strengths and 2-3 development areas for your current job or a future role")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Fortalezas { get; set; }

        [Display(Name = "Describe the associate's strengths and development areas using associate feedback")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string FortalezasJefe { get; set; }

        [Display(Name = "Describe your short term (next 18 months) and long term (3-5 years) career objectives")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ObjetivosFuturo { get; set; }

        [Display(Name = "Provide your assessment of the associate's career goals")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ObjetivosFuturoJefe { get; set; }

        [Display(Name = "Adjusted Factor")]
        public float FactorAjuste { get; set; }

        [Display(Name = "Manager's Justification for Adjusted Factor")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string JustificacionFactorAjuste { get; set; }

        [Display(Name = "Overall Performance Rating")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string RendimientoGeneral { get; set; }

        [Display(Name = "Status")]
        public EstadoPeak Estado { get; set; }

        [Display(Name = "Date Sent")]
        public DateTime? FechaEnvio { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Periodo Periodo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Area Area { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario UsuarioPadre { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakCoreValue> PeakCoreValues { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivo> PeakObjetivos { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakRevision> PeakRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakPlanDesarrollo> PeakPlanesDesarrollo { get; set; }
    }

    [Table("PeakObjetivo")]
    public class PeakObjetivo
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Peak")]
        [Index("IXU_PeakObjetivo", 1, IsUnique = true)]
        public int PeakId { get; set; }

        [Display(Name = "No.")]
        [Index("IXU_PeakObjetivo", 2, IsUnique = true)]
        public short Numero { get; set; }

        [Display(Name = "Weight")]
        public short Peso { get; set; }

        [Required]
        [Display(Name = "Statement of Objective")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Objetivo { get; set; }

        [Display(Name = "Target Date")]
        [Column(TypeName = "date")]
        public DateTime FechaMeta { get; set; }

        [Required]
        [Display(Name = "Measured By")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string MedidoPor { get; set; }

        [Display(Name = "Actual Results")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ResultadosActuales { get; set; }

        [Display(Name = "Associate's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Comentarios { get; set; }

        [Display(Name = "Achieved")]
        public short? Completado { get; set; }

        [Display(Name = "Manager's Rating")]
        public float? Calificacion { get; set; }

        [Display(Name = "Weighted Factor")]
        public float? Factor { get; set; }

        [Display(Name = "Disapproval Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ComentariosRechazo { get; set; }

        [Display(Name = "Reason for Change")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string MotivoModificacion { get; set; }

        [Display(Name = "Heritable")]
        public bool Heredable { get; set; }

        [Display(Name = "Objetive")]
        public int? PeakObjetivoId { get; set; }

        [Display(Name = "Status")]
        public EstadoPeakObjetivo Estado { get; set; }

        [Display(Name = "Last Approval Date")]
        public DateTime? UltimaFechaAprobacion { get; set; }

        [Display(Name = "Modification Approval")]
        public bool? AprobacionModificacion { get; set; }

        [Display(Name = "Request Inherited Removal")]
        public bool? SolicitudEliminacionHeredado { get; set; }

        [Display(Name = "Manager's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ComentariosJefe { get; set; }

        [Display(Name = "Manager's Comments Date")]
        public DateTime? FechaComentariosJefe { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Peak Peak { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PeakObjetivo PeakObjetivoHeredado { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivo> PeakObjetivosHeredados { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivoRevision> PeakObjetivoRevisiones { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual ICollection<PeakObjetivoRevision> PeakObjetivoRevisionesHeredados { get; set; }
    }

    [Table("PeakObjetivoRevision")]
    public class PeakObjetivoRevision
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Objective")]
        [Index("IXU_PeakObjetivoRevision", 1, IsUnique = true)]
        public int PeakObjetivoId { get; set; }

        [Display(Name = "Period Review")]
        [Index("IXU_PeakObjetivoRevision", 2, IsUnique = true)]
        public int PeriodoRevisionId { get; set; }

        [Required]
        [Display(Name = "Statement of Objective")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Objetivo { get; set; }

        [Display(Name = "Target Date")]
        [Column(TypeName = "date")]
        public DateTime FechaMeta { get; set; }

        [Required]
        [Display(Name = "Measured By")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string MedidoPor { get; set; }

        [Required]
        [Display(Name = "Actual Results")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ResultadosActuales { get; set; }

        [Required]
        [Display(Name = "Associate's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Comentarios { get; set; }

        [Display(Name = "Achieved")]
        public short Completado { get; set; }

        [Display(Name = "Manager's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ComentariosJefe { get; set; }

        [Display(Name = "Manager's Comments Date")]
        public DateTime? FechaComentariosJefe { get; set; }

        [Display(Name = "Manager")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "Inherited Objetive")]
        public int? PeakObjetivoIdHeredado { get; set; }

        [Display(Name = "Has Changes")]
        public bool TieneCambios { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PeakObjetivo PeakObjetivo { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PeriodoRevision PeriodoRevision { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PeakObjetivo PeakObjetivoHeredado { get; set; }
    }

    [Table("PeakRevision")]
    public class PeakRevision
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Peak")]
        [Index("IXU_PeakRevision", 1, IsUnique = true)]
        public int PeakId { get; set; }

        [Display(Name = "Period Review")]
        [Index("IXU_PeakRevision", 2, IsUnique = true)]
        public int PeriodoRevisionId { get; set; }

        [Required]
        [Display(Name = "Associate's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Comentarios { get; set; }

        [Display(Name = "Associate's Comments Date")]
        public DateTime FechaComentarios { get; set; }

        [Display(Name = "Manager's Comments")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string ComentariosJefe { get; set; }

        [Display(Name = "Manager's Comments Date")]
        public DateTime? FechaComentariosJefe { get; set; }

        [Display(Name = "Manager")]
        [StringLength(5)]
        public string UsuarioId { get; set; }

        [Display(Name = "Closed")]
        public bool Cerrada { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Peak Peak { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual PeriodoRevision PeriodoRevision { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }
    }

    [Table("PeakCoreValue")]
    public class PeakCoreValue
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Peak")]
        [Index("IXU_PeakCoreValue", 1, IsUnique = true)]
        public int PeakId { get; set; }

        [Display(Name = "Core Value")]
        [Index("IXU_PeakCoreValue", 2, IsUnique = true)]
        public int CoreValueId { get; set; }

        [Display(Name = "Self Assessment")]
        public Skill Autoevaluacion { get; set; }

        [Display(Name = "Manager's Assessment")]
        public Skill? Evaluacion { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Peak Peak { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual CoreValue CoreValue { get; set; }
    }

    [Table("PeakPlanDesarrollo")]
    public class PeakPlanDesarrollo
    {
        [Display(Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Peak")]
        public int PeakId { get; set; }

        [Required]
        [Display(Name = "Area")]
        [StringLength(50)]
        public string Area { get; set; }

        [Required]
        [Display(Name = "Plan")]
        [StringLength(500)]
        public string Plan { get; set; }

        [Display(Name = "Due Date")]
        [Column(TypeName = "date")]
        public DateTime FechaMeta { get; set; }

        [Required]
        [Display(Name = "Desired Measurable Outcome")]
        [StringLength(500)]
        public string ResultadoDeseado { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Peak Peak { get; set; }
    }

    [Table("UsuarioHV")]
    public class UsuarioHV
    {
        [StringLength(5)]
        [Display(Name = "Code")]
        public string UsuarioId { get; set; }

        [Display(Name = "Birthday")]
        [Column(TypeName = "date")]
        public DateTime? FechaNacimiento { get; set; }

        [Display(Name = "Title")]
        [StringLength(50)]
        public string Cargo { get; set; }

        [Display(Name = "Area")]
        public int? AreaId { get; set; }

        [StringLength(20)]
        [Display(Name = "Identification Number")]
        public string Identificacion { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Phone Number")]
        public string Celular { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string Contacto { get; set; }

        [Display(Name = "Education Level")]
        public NivelEducativo? NivelEducativo { get; set; }

        [Display(Name = "Date of Admission")]
        [Column(TypeName = "date")]
        public DateTime? FechaIngreso { get; set; }

        [Display(Name = "Profile Picture")]
        public byte[] Foto { get; set; }

        [StringLength(50)]
        public string FotoMediaType { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Usuario Usuario { get; set; }

        [ScriptIgnore, JsonIgnore]
        public virtual Area Area { get; set; }
    }

    // VIEW MODELS ----------------------------------------------------

    public class PeriodoViewModel
    {
        public Periodo Periodo { get; set; }
        public List<PeriodoRevision> Revisiones { get; set; }
        public int[] RevisionesId { get; set; }
    }

    public class PeakConfirmacionViewModel
    {
        public int PeakId { get; set; }
        public string UsuarioIdPadre { get; set; }
        public int PeriodoId { get; set; }
        public int AreaId { get; set; }
        public string Cargo { get; set; }
    }
}