using System.ComponentModel.DataAnnotations;

namespace MMS.Models
{

    enum TipoNotificacion
    {
        Review, Result, Undo, Finished, Opened, DP, NextStep
    }
    public enum objetosOpciones
    {
        create = 1,
        edit = 2,
        delete = 3,
        detail = 4,
        display = 5,
        cancel = 6,
        print = 7
    }

    public enum EstadosActividades
    {
        Abierto = 1,//cuando recien se crea
        Pendiente = 2,//Este estado es para todos los niveles de aprobación
        Autorizado = 3,//Cuando ya paso todas las aprobaciones
        Despachado = 4,//Cuando ya despacharon la orden y el gasto
        //Ejecutado = 5,//Quitar si no se usa
        Rechazado = 6,//No paso las aprobaciones
        //PendienteTrade = 7,//Quitar
        Cerrado = 8//Fin de la actividad
    }

    public enum EstadoOrden
    {
        Abierta = 1,
        Por_despachar = 2,
        Despachado = 3,
        Rechazado = 4,
        Eliminado = 5
    }

    public enum EstadoGasto
    {
        Abierta = 1,
        Ejecutado = 2,
        Rechazado = 3,
        Eliminado = 4,
        Pagado = 5
    }

    public enum EstadoMovimiento
    {
        Sin_Aplicar = 1,
        Abierto = 2,
        Ejecutado = 3,
        Eliminado = 4
    }

    //ENums de meses
    public enum Meses
    {
        Enero = 1,
        Febrero = 2,
        Marzo = 3,
        Abril = 4,
        Mayo = 5,
        Junio = 6,
        Julio = 7,
        Agosto = 8,
        Septiembre = 9,
        Octubre = 10,
        Noviembre = 11,
        Diciembre = 12
    }

    public enum trnMode
    {
        Insert = 1,
        Update = 2,
        Delete = 3,
        Display = 4
    }

    public enum EstadoAutorizaActividad
    {
        Por_Autorizar = 1,
        Autorizado = 2,
        Rechazado = 3,
        Pendiente = 4
    }

    public enum EstadoLiquidacion
    {
        Open = 1,
        Closed = 2
    }


    public enum EstadoFormatoPQRS
    {
        Open = 1,
        In_Process = 2,
        Completed = 3,
        Deleted = 4
    }

    public enum Sectors
    {
        Administrativo = 1,
        [Display(Name = @"Mão de Obra Direta")]
        Mão_de_Obra_Direta = 2,
        [Display(Name = @"Mão de Obra Indireta")]
        Mão_de_Obra_Indireta = 3
    }

    public enum Positions
    {
        [Display(Name = @"Cargo Novo")]
        Cargo_Novo = 1,
        Substituição = 2,
        Adicional = 3
    }

    public enum ContractTypes
    {
        Permanente = 1,
        Temporário = 2,
        Terceirizado = 3
    }

    public enum Budgets
    {
        [Display(Name = @"Previsto em Orçamento")]
        Previsto_em_Orçamento = 1,
        [Display(Name = @"Não Previsto")]
        Não_Previsto = 2
    }

    public enum ResignationReasons
    {
        [Display(Name = @"Sem Justa Causa")]
        Sem_Justa_Causa = 1,
        [Display(Name = @"Com Justa Causa")]
        Com_Justa_Causa = 2,
        [Display(Name = @"Pedido de Demissão")]
        Pedido_de_Demissão = 3,
        [Display(Name = @"Término de contrato")]
        Término_de_contrato = 4
    }

    public enum PreviousNotices
    {
        Indenizado = 1,
        Trabalhado = 2
    }

    public enum ExperienceTimes
    {
        Isento = 1,
        [Display(Name = @"30 días")]
        _30_dias = 2,
        [Display(Name = @"60 días")]
        _60_dias = 3,
        [Display(Name = @"90 días")]
        _90_dias = 4
    }

    public enum Types
    {
        Recrutamento = 1,
        [Display(Name = @"Alteração de Função")]
        Alteração_de_Função = 2
    }

    public enum EstadoFormatoItemPQRS
    {
        Approved = 1,
        Rejected = 2
    }




    public enum TipoPQRS
    {
        [Display(Name = @"Return")]
        Devolucion = 1,
        [Display(Name = @"Guarantee")]
        Garantia = 2,
        [Display(Name = @"New")]
        Novedad = 3,
        [Display(Name = @"Recruitment")]
        Recruitment = 4
    }

    public enum Prioridad
    {
        Baja = 1,
        Media = 2,
        Alta = 3
    }

    public enum TipoPersona
    {
        EmpleadoATG = 1,
        Cliente = 2,
        Magnum = 3,
        Coordinadora = 4,
        Otro = 5
    }

    public enum TipoPaso
    {
        [Display(Name = @"General")]
        General = 1,
        [Display(Name = @"Llenar Formato")]
        LlenarFormato = 2,
        [Display(Name = @"Aprobar")]
        Aprobar = 3
    }


    public enum Quart
    {
        [Display(Name = @"Quart 1")]
        Quart1 = 1,
        [Display(Name = @"Quart 2")]
        Quart2 = 2,
        [Display(Name = @"Quart 3")]
        Quart3 = 3,
        [Display(Name = @"Quart 4")]
        Quart4 = 4
    }

    public enum Months
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }

    public enum Skill
    {
        Unskilled = 1,
        Skilled = 2,
        Highly_Skilled = 3
    }

    public enum EstadoPeakObjetivo
    {
        Defined = 1,
        Approved = 2,
        Disapproved = 3,
        Modified = 4
    }

    public enum EstadoPeak
    {
        Objectives_Definition = 1,
        Objectives_Approval = 2,
        Standby = 3,
        Review = 4,
        Final_Review = 5,
        Development_Plan = 8,
        Finished = 6,
        Objectives_Modification_Approval = 7,
    }

    public enum RecordType
    {
        Approved = 1,
        Rejected = 2
    }

    public enum EstadoStep
    {
        Pending = 1,
        In_Process = 2,
        //Approved = 3,
        Done = 3,
        //Rejected = 4,
        Returned = 4,
        Completed = 5,
        Closed = 6,
        DoesNotApply = 7
    }

    public enum EstadoUsuarioFlujoPQRS
    {
        Unanswered = 1,
        //Approved = 2,
        Done = 2,
        //Rejected = 3,
        Returned = 3,
        Closed = 4
    }

    public enum TipoComentario
    {
        Approval = 1,
        Rejection = 2,
        Comment = 3,
        Close = 4
    }

    public enum NivelEducativo
    {
        Early_childhood_Education = 1,
        Primary_education = 2,
        Lower_secondary_education = 3,
        Upper_secondary_education = 4,
        Post__secondary_non__tertiary_education = 5,
        Short__cycle_tertiary_education = 6,
        Bachelor_or_equivalent = 7,
        Master_or_equivalent = 8,
        Doctoral_or_equivalent = 9,
    }

    public enum TipoCondicion
    {
        YesNo = 1,
        Value = 2
    }

    public enum CondicionesValor
    {
        Equal = 1,
        Less = 2,
        LessEqual = 3,
        Higher = 4,
        HigherEqual = 5
    }

    public enum StadoDoPagamento
    {
        PendientePago = 1,
        Pago = 2
    }
    public enum EstadoCierreActividad
    {
        Cumplio = 1,
        No_Cumplio = 2
    }

    public enum MetaCierreActividad
    {
        MetaCrecimiento = 1,
        SellOut = 2
    }

}