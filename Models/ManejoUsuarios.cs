using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class ManejoUsuarios
    {
        private readonly string rutaArchivo;
        string rutaCarpeta = AppDomain.CurrentDomain.BaseDirectory;

        public ManejoUsuarios(string nombreArchivo = "Usuarios.xlsx")
        {
            rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);
        }

        public List<string> ObtenerUsuarios()
        {
            var listaUsuarios = new List<string>();

            if (!File.Exists(rutaArchivo)) return listaUsuarios;

            using var workbook = new XLWorkbook(rutaArchivo);
            var worksheet = workbook.Worksheet("Usuarios");
            var filas = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var fila in filas)
            {
                string usuario = fila.Cell(1).GetValue<string>().Trim();

                // Evitamos agregar celdas vacías por error si el Excel tiene filas fantasmas
                if (!string.IsNullOrEmpty(usuario))
                {
                    listaUsuarios.Add(usuario);
                }
            }

            return listaUsuarios;
        }
    }
}
