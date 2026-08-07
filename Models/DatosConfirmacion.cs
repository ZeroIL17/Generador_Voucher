using GeneradorVoucher_MP.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class DatosConfirmacion
    {
        public MediosPago MedioPago { get; set; }
        public double AbonoConfirmacion { get; set; }
        public double SaldoPendiente { get; set; }
        public DateTime FechaCreacionAbono { get; set; }
        public DateTime FechaPagoAbono { get; set; }
        public DateTime? FechaPagoPendiente { get; set; }
    }
}
