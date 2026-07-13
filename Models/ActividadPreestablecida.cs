using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class ActividadPreestablecida
    {
        public required string tour { get; set; }
        public required string pickUp { get; set; }
        public required string regreso { get; set; }
        public required string incluye { get; set; }
        public double precioEntrada { get; set; }
        public double precioTourAdulto { get; set; }
        public string? precioTourNino { get; set; }
    }
}
