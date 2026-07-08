using System;

namespace GeneradorVoucher_MP.Models
{
    public class DatosCliente
    {
        public required string NombreCliente { get; set; }
        public int CantidadAdultosCliente { get; set; }
        public int CantidadNinosCliente { get; set; }
        public DateTime FechaInicioCliente { get; set; }
        public required string TelefonoCliente { get; set; }
        public string UsuarioCreacion { get; set; } = SesionSistema.UsuarioActual;
    }
}
