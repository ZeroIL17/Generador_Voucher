using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class ManejoRegistros_V0
    {
        private readonly string rutaArchivo;
        string rutaCarpetaDirectorio = AppDomain.CurrentDomain.BaseDirectory;
        string rutaPlanillaViajes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlanillaViajes.xlsx");


        public ManejoRegistros_V0(string nombreArchivo = "PlanillaViajes.xlsx")
        {
            rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);
        }
     
        public int ObtenerUltimoID()
        {

            if (!File.Exists(rutaArchivo)) return 1;

            using var workbook = new XLWorkbook(rutaArchivo);
            var worksheet = workbook.Worksheet("Viajes");

            return worksheet.Column(1).CellsUsed()
                             .Skip(1)
                             .Select(cell => int.TryParse(cell.Value.ToString(), out int val) ? val : 0)
                             .DefaultIfEmpty(0)
                             .Max() + 1;
        }

        public void AbrirRegistros()
        {
            if (!Directory.Exists(rutaCarpetaDirectorio))
            {
                throw new DirectoryNotFoundException("No se pudo localizar la carpeta de la aplicación.");
            }

            // 2. Detectar el sistema operativo y configurar el arranque del proceso
            ProcessStartInfo info = new ProcessStartInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Comando nativo para Windows (Explorador de archivos)
                info.FileName = "explorer.exe";
                info.Arguments = $"\"{rutaCarpetaDirectorio}\""; // Las comillas evitan errores si la ruta tiene espacios
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Comando nativo para macOS (Finder)
                info.FileName = "open";
                info.Arguments = $"\"{rutaCarpetaDirectorio}\"";
            }
            else
            {
                // Opcional: Soporte por si acaso el sistema corre en Linux en el futuro
                info.FileName = "xdg-open";
                info.Arguments = $"\"{rutaCarpetaDirectorio}\"";
            }

            // 3. Ejecutar el proceso de forma segura sin abrir consolas negras de fondo
            info.UseShellExecute = true;

            try
            {
                Process.Start(info);
            }
            catch (Exception ex)
            {
                // Elevamos el error para que la Vista (UserControl) lo capture y muestre la alerta flotante
                throw new Exception($"Error al intentar abrir el explorador nativo: {ex.Message}");
            }
        }

        public List<ActividadPreestablecida> ObtenerCatalogoActividades()
        {
            var listaResultado = new List<ActividadPreestablecida>();
            string rutaCatalogo = Path.Combine(rutaCarpetaDirectorio, "Template_cotizaciones.xlsx");

            if (!File.Exists(rutaCatalogo))
            {
                throw new FileNotFoundException("No se encontró el archivo 'Template_cotizaciones.xlsx'.");
            }

            using (var workbook = new XLWorkbook(rutaCatalogo))
            {
                var worksheet = workbook.Worksheet("Espanol");
                var filas = worksheet.RangeUsed().RowsUsed().Skip(1);

                foreach (var fila in filas)
                {
                    var opcion = new ActividadPreestablecida
                    {
                        tour = fila.Cell(1).GetValue<string>(),
                        pickUp = fila.Cell(2).GetValue<string>(),
                        regreso = fila.Cell(3).GetValue<string>(),
                        incluye = fila.Cell(4).GetValue<string>(),
                        precioEntrada = fila.Cell(5).GetValue<double>(),
                        precioTourAdulto = fila.Cell(6).GetValue<double>(),
                        precioTourNino = fila.Cell(7).GetValue<string>()
                    };
                    listaResultado.Add(opcion);
                }
            }

            return listaResultado;
        }

        public void GuardarViajeExcel(DatosCliente datosCliente, IEnumerable<DatosActividad> actividades, int idActividad = 0)
        {
            XLWorkbook workbook;
            IXLWorksheet worksheet;

            if (idActividad == 0)
            {
                idActividad = ObtenerUltimoID();
            }

            // 1. ESCENARIO 1: EL ARCHIVO NO EXISTE
            if (!File.Exists(rutaArchivo))
            {
                workbook = new XLWorkbook();
                worksheet = workbook.Worksheets.Add("Viajes");

                string[] encabezados = {
                    "ID", "Fecha Creación", "Nombre Cliente", "Cantidad Adultos", "Cantidad Niños",
                    "Fecha Inicio", "Telefono", "Fecha Actividad", "Tipo Actividad", "PickUp Actividad",
                    "Regreso Actividad", "Servicio Actividad", "Precio Entrada", "Precio Tour Adulto",
                    "Precio Tour Niño", "Subtotal", "Usuario"
                };

                for (int i = 0; i < encabezados.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = encabezados[i];
                }
                worksheet.Row(1).Style.Font.Bold = true;
            }
            else
            {
                // ESCENARIO 2: EL ARCHIVO YA EXISTE
                workbook = new XLWorkbook(rutaArchivo);
                worksheet = workbook.Worksheet("Viajes");
            }

            // 2. ESCRITURA DE LOS DATOS
            int nuevaFilaId = worksheet.LastRowUsed() == null ? 2 : worksheet.LastRowUsed().RowNumber() + 1;
            string fechaCreacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var actividad in actividades)
            {
                // Cálculo rápido del subtotal por fila (Ajusta la fórmula matemática según tus reglas de negocio)
                double subtotal = (actividad.precioEntrada + actividad.precioTourAdulto)*datosCliente.CantidadAdultosCliente
                    + (actividad.precioEntrada + actividad.precioTourNino) * datosCliente.CantidadNinosCliente;

                worksheet.Cell(nuevaFilaId, 1).Value = idActividad;
                worksheet.Cell(nuevaFilaId, 2).Value = fechaCreacion;
                worksheet.Cell(nuevaFilaId, 3).Value = datosCliente.NombreCliente;
                worksheet.Cell(nuevaFilaId, 4).Value = datosCliente.CantidadAdultosCliente; 
                worksheet.Cell(nuevaFilaId, 5).Value = datosCliente.CantidadNinosCliente;
                worksheet.Cell(nuevaFilaId, 6).Value = datosCliente.FechaInicioCliente.ToString("yyyy-MM-dd");
                worksheet.Cell(nuevaFilaId, 7).Value = datosCliente.TelefonoCliente;

                worksheet.Cell(nuevaFilaId, 8).Value = actividad.fechaActividad.HasValue
                    ? actividad.fechaActividad.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
                worksheet.Cell(nuevaFilaId, 9).Value = actividad.tipoActividad;
                worksheet.Cell(nuevaFilaId, 10).Value = actividad.pickupActividad;
                worksheet.Cell(nuevaFilaId, 11).Value = actividad.regresoActividad;
                worksheet.Cell(nuevaFilaId, 12).Value = actividad.incluyeActividad;
                worksheet.Cell(nuevaFilaId, 13).Value = actividad.precioEntrada;
                worksheet.Cell(nuevaFilaId, 14).Value = actividad.precioTourAdulto;
                worksheet.Cell(nuevaFilaId, 15).Value = actividad.precioTourNino;
                worksheet.Cell(nuevaFilaId, 16).Value = subtotal;
                worksheet.Cell(nuevaFilaId, 17).Value = SesionSistema.UsuarioActual; // Responsable de la sesión

                nuevaFilaId++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
            workbook.Dispose();
        }

        public List<VoucherLookup> CargarVouchers()
        {
            var resultado = new List<VoucherLookup>();

            if (!File.Exists(rutaArchivo)) return resultado;

            using var workbook = new XLWorkbook(rutaArchivo);
            var worksheet = workbook.Worksheet("Viajes");
            var filas = worksheet.RangeUsed().RowsUsed().Skip(1);

            var idRegistrados = new HashSet<int>();

            foreach ( var fila in filas )
            {
                if (int.TryParse(fila.Cell(1).GetString(), out int id))
                {
                    if (!idRegistrados.Contains(id))
                    {
                        idRegistrados.Add(id);
                        string cliente = fila.Cell(3).GetValue<string>();
                        string tour = fila.Cell(9).GetValue<string>(); //Tipo de actividad

                        resultado.Add(new VoucherLookup
                        {
                            Id = id,
                            DisplayText = $"ID: {id} - {cliente} ({tour})"
                        });
                    }
                }
            }

            return resultado;
        }

        public (DatosCliente Cliente, List<DatosActividad> Actividades) ObtenerDetalleVoucher(int idSeleccionado)
        {
            if (!File.Exists(rutaArchivo)) throw new FileNotFoundException("El archivo de registros no existe.");

            using var workbook = new XLWorkbook(rutaArchivo);
            var worksheet = workbook.Worksheet("Viajes");

            // Buscamos todas las filas que coincidan con el ID
            var filasMatch = worksheet.RangeUsed().RowsUsed()
                .Where(f => int.TryParse(f.Cell(1).GetString(), out int v) && v == idSeleccionado)
                .ToList();

            if (!filasMatch.Any()) throw new Exception("No se encontraron registros para el ID seleccionado.");

            var primeraFila = filasMatch.First();

            // 1. Reconstruimos el objeto Cliente
            var cliente = new DatosCliente
            {
                NombreCliente = primeraFila.Cell(3).GetValue<string>(),
                CantidadAdultosCliente = primeraFila.Cell(4).GetValue<int>(),
                CantidadNinosCliente = primeraFila.Cell(5).GetValue<int>(),
                FechaInicioCliente = DateTime.TryParse(primeraFila.Cell(6).GetString(), out var f) ? f : DateTime.Now,
                TelefonoCliente = primeraFila.Cell(7).GetValue<string>()
            };

            // 2. Reconstruimos la lista de actividades asociadas
            var actividades = new List<DatosActividad>();
            foreach (var fila in filasMatch)
            {
                actividades.Add(new DatosActividad
                {
                    fechaActividad = DateTime.TryParse(fila.Cell(8).GetString(), out var fa) ? fa : null,
                    tipoActividad = fila.Cell(9).GetValue<string>(),
                    pickupActividad = fila.Cell(10).GetValue<string>(),
                    regresoActividad = fila.Cell(11).GetValue<string>(),
                    incluyeActividad = fila.Cell(12).GetValue<string>(),
                    precioEntrada = fila.Cell(13).GetValue<double>(),
                    precioTourAdulto = fila.Cell(14).GetValue<double>(),
                    precioTourNino = fila.Cell(15).GetValue<double>()
                });
            }

            return (cliente, actividades);
        }

        public void ActualizarVoucher(int idSeleccionado, DatosCliente clienteModificado, IEnumerable<DatosActividad> actividadesModificadas)
        {
            if (!File.Exists(rutaArchivo)) throw new FileNotFoundException("No se encontró el archivo de registros.");

            using var workbook = new XLWorkbook(rutaArchivo);
            var worksheet = workbook.Worksheet("Viajes");

            // 1. Eliminar filas antiguas que coincidan con el ID
            foreach (var r in worksheet.Rows()
                         .Where(r => r.Cell(1).GetValue<string>() == idSeleccionado.ToString())
                         .ToList())
            {
                r.Delete();
            }

            workbook.Save();

            GuardarViajeExcel(clienteModificado, actividadesModificadas, idSeleccionado);
        }
    }
}