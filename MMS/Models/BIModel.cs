using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MMS.Models
{
    public class ClienteResumenViewModel
    {

        public List<Actividad> Actividades { get; set; }
        public List<PQRSPanel> PQRSPanel { get; set; }
        public List<LiquidacionInfo> Liquidaciones { get; set; }
        public List<VentasInfo> Ventas { get; set; }
        public List<CarteraInfo> Cartera { get; set; }


        public class LiquidacionInfo
        {
            public string Liquidacion { get; set; }
            public string ColeccionPIVId { get; set; }
            public string ColeccionPIVNombre { get; set; }
            public string AsesorId { get; set; }
            public string AsesorNombre { get; set; }
            public string AsesorPresupuesto { get; set; }
            public string AsesorCategoria { get; set; }
            public decimal CategoriaValorMin { get; set; }
            public decimal CategoriaValorMax { get; set; }
            public decimal CategoriaPorcentaje { get; set; }
            public decimal ValorTotal { get; set; }
            public decimal PorcentajeCumplimiento { get; set; }
            public string Aprobacion { get; set; }
            public decimal PorcentajePago { get; set; }
            public decimal ValorPago { get; set; }
            public decimal ValorReglasPago { get; set; }
        }


        public class VentasInfo
        {
            public string Customer { get; set; }
            public string CustomerName { get; set; }
            public string Brand { get; set; }
            public decimal Qty { get; set; }
            public decimal Amount { get; set; }
        }


        public class CarteraInfo
        {
            public string Customer { get; set; }
            public string CustomerName { get; set; }
            public decimal LocalCurrencyAmount { get; set; }
            public decimal Age1 { get; set; }
            public decimal Age2 { get; set; }
            public decimal Age3 { get; set; }
            public decimal Age4 { get; set; }
            public decimal Age5 { get; set; }
        }
    }



    public class DisponibilidadViewModel
    {

        [Display(Name = "Date Stamp")]
        public string DateStamp { get; set; }

        [Display(Name = "Store Location")]
        public string SLoc { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Name")]
        public string MaterialDescription { get; set; }

        [Display(Name = "Available Qty")]
        public int AvailableQty { get; set; }

    }
}