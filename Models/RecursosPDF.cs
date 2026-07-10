using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public static class RecursosPdf
    {
        private static byte[] LeerRecurso(string nombreArchivo)
        {
            var ensamblado = Assembly.GetExecutingAssembly();

            // El nombre del recurso sigue el patrón:
            // NombreNamespace.RutaConPuntosEnVezDeSlash.archivo.ext
            string nombreRecurso = $"GeneradorVoucher_MP.Assets.{nombreArchivo}";

            using var stream = ensamblado.GetManifestResourceStream(nombreRecurso)
                ?? throw new FileNotFoundException(
                    $"Recurso embebido no encontrado: {nombreRecurso}");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public static byte[] Logo => LeerRecurso("logo_caminandes_2.png");
        public static byte[] ImagenPie => LeerRecurso("imagen_pie_pdf.png");
    }
}
