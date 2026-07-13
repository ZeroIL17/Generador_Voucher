using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class DatosActividad
    {
        public DateTime? FechaActividad { get; set; }
        public string TipoActividad { get; set; } = string.Empty;
        public string PickupActividad { get; set; } = string.Empty;
        public string RegresoActividad { get; set; } = string.Empty;
        public string IncluyeActividad { get; set; } = string.Empty;
        public double PrecioEntrada { get; set; }
        public double PrecioTourAdulto { get; set; }
        public double PrecioTourNino { get; set; }
    }
}
