using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class DatosActividad
    {
        public DateTime? fechaActividad { get; set; }
        public string tipoActividad { get; set; } = string.Empty;
        public string pickupActividad { get; set; } = string.Empty;
        public string regresoActividad { get; set; } = string.Empty;
        public string incluyeActividad { get; set; } = string.Empty;
        public double precioEntrada { get; set; }
        public double precioTourAdulto { get; set; }
        public double precioTourNino { get; set; }
    }
}
