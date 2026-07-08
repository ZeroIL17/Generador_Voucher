using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public static class SesionSistema
    {
        public static string UsuarioActual { get; set; } = "Sin Usuario";
        public static string LabelUsuarioActual => $"Usuario : {UsuarioActual}";
    }
}
