using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using GeneradorVoucher_MP.Enums;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public class ManejoRegistros
    {
        // Tu cadena de conexión segura provista por tu hosting de Postgres
        private readonly string _connectionString = "Host=aws-1-us-west-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.jaqvtgzkrajesvbqublf;Password=CAMINANDES_2026;SSL Mode=Require;Timeout=30;Command Timeout=30;";

        // Método auxiliar para abrir la conexión de forma limpia
        private IDbConnection ObtenerConexion() => new NpgsqlConnection(_connectionString);

        private readonly string rutaArchivo;
        readonly string rutaCarpetaDirectorio = AppDomain.CurrentDomain.BaseDirectory;

        public ManejoRegistros(string nombreArchivo = "PlanillaViajes.xlsx")
        {
            rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nombreArchivo);
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

        public List<ActividadPreestablecida> ObtenerCatalogoActividades(IdiomaVoucher idioma)
        {
            var listaResultado = new List<ActividadPreestablecida>();
            string rutaCatalogo = Path.Combine(rutaCarpetaDirectorio, "Template_cotizaciones.xlsx");

            if (!File.Exists(rutaCatalogo))
            {
                throw new FileNotFoundException("No se encontró el archivo 'Template_cotizaciones.xlsx'.");
            }

            string hojaSeleccionada = idioma switch
            {
                IdiomaVoucher.Espanol => "Espanol",
                IdiomaVoucher.Ingles => "Ingles",
                IdiomaVoucher.Portugues => "Portugues",
                _ => throw new ArgumentException("Idioma no encontrado.")
            };

            using (var workbook = new XLWorkbook(rutaCatalogo))
            {
                // Intentamos obtener la hoja; si no existe, lanzamos un error controlado
                if (!workbook.Worksheets.TryGetWorksheet(hojaSeleccionada, out var worksheet))
                {
                    throw new KeyNotFoundException($"No se encontró la pestaña '{hojaSeleccionada}' en el archivo Excel.");
                }

                var rango = worksheet.RangeUsed();
                if (rango == null) return listaResultado; // hoja vacía o sin rango usado

                var filas = rango.RowsUsed().Skip(1);

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

        public int GuardarViajeDB(DatosCliente datosCliente, IEnumerable<DatosActividad> actividades, int idActividad = 0)
        {
            using var db = ObtenerConexion();
            db.Open();

            // Iniciamos una Transacción: si falla el guardado de una actividad, 
            // se deshace todo para no dejar datos corruptos a medias
            using var transaccion = db.BeginTransaction();

            try
            {
                // 1. Insertar el cliente y recuperar el ID generado automáticamente por Postgres
                string sqlCliente = @"
                    INSERT INTO clientes (nombre, cantidad_adultos, cantidad_ninos, fecha_viaje, telefono, usuario_creacion)
                    VALUES (@NombreCliente, @CantidadAdultosCliente, @CantidadNinosCliente, @FechaInicioCliente, @TelefonoCliente, @UsuarioCreacion)
                    RETURNING id;"; // Retorna el ID en tiempo real

                // Le inyectamos los datos del responsable actual
                datosCliente.UsuarioCreacion = SesionSistema.UsuarioActual;

                // Ejecutamos la consulta pasándole el modelo directamente (Dapper hace el mapeo)
                int nuevoClienteId = db.QuerySingle<int>(sqlCliente, datosCliente, transaccion);

                // 2. Insertar las actividades del itinerario amarradas a ese nuevo ID
                string sqlActividad = @"
                    INSERT INTO actividades (cliente_id, fecha_actividad, tipo_actividad, pickup_actividad, regreso_actividad, servicio_actividad, precio_entrada, precio_tour_adulto, precio_tour_nino, subtotal)
                    VALUES (@ClienteId, @FechaActividad, @TipoActividad, @PickupActividad, @RegresoActividad, @ServicioActividad, @PrecioEntrada, @PrecioTourAdulto, @PrecioTourNino, @Subtotal);";

                foreach (var act in actividades)
                {
                    double subtotalCalculado = (act.PrecioEntrada + act.PrecioTourAdulto) * datosCliente.CantidadAdultosCliente +
                                              (act.PrecioEntrada + act.PrecioTourNino) * datosCliente.CantidadNinosCliente;

                    // Usamos un objeto anónimo para inyectar los datos en el SQL de Dapper
                    db.Execute(sqlActividad, new
                    {
                        ClienteId = nuevoClienteId,
                        FechaActividad = act.FechaActividad,
                        TipoActividad = act.TipoActividad,
                        PickupActividad = act.PickupActividad,
                        RegresoActividad = act.RegresoActividad,
                        ServicioActividad = act.IncluyeActividad,
                        PrecioEntrada = act.PrecioEntrada,
                        PrecioTourAdulto = act.PrecioTourAdulto,
                        PrecioTourNino = act.PrecioTourNino,
                        Subtotal = subtotalCalculado
                    }, transaccion);
                }

                // Si todo salió bien, guardamos los cambios definitivamente
                transaccion.Commit();
                return nuevoClienteId;
            }
            catch (Exception ex)
            {
                transaccion.Rollback(); // Si hubo un error (ej: corte de internet), deshace todo
                Debug.WriteLine(ex);
                throw;
            }
        }

        public List<VoucherLookup> CargarVouchersDB()
        {
            using var db = ObtenerConexion();
            string sql = @"
            SELECT c.id AS Id, 
                    'ID: ' || c.id || ' - ' || c.nombre || ' (' || COALESCE(MAX(a.tipo_actividad), 'Sin Actividad') || ')' AS DisplayText
            FROM clientes c
            LEFT JOIN actividades a ON c.id = a.cliente_id
            GROUP BY c.id, c.nombre
            ORDER BY c.id DESC;";

            return db.Query<VoucherLookup>(sql).ToList();
        }

        public (DatosCliente Cliente, List<DatosActividad> Actividades) ObtenerDetalleVoucher(int idSeleccionado)
        {
            using var db = new NpgsqlConnection(_connectionString);
            db.Open();

            // 1. Buscar los datos del cliente por su ID único
            string sqlCliente = "SELECT id, nombre AS NombreCliente, cantidad_adultos AS CantidadAdultosCliente, cantidad_ninos AS CantidadNinosCliente, fecha_viaje::TIMESTAMP AS FechaInicioCliente, telefono AS TelefonoCliente FROM clientes WHERE id = @Id;";
            var cliente = db.QueryFirstOrDefault<DatosCliente>(sqlCliente, new { Id = idSeleccionado });

            if (cliente == null)
            {
                throw new Exception("No se encontraron registros para el ID seleccionado.");
            }

            // Aseguramos que el ID interno quede asignado por si tu modelo lo requiere
            //cliente.id = idSeleccionado;

            // 2. Buscar todas las actividades amarradas a ese cliente_id
            string sqlActividades = @"
                SELECT fecha_actividad::TIMESTAMP AS fechaActividad,
                       tipo_actividad AS tipoActividad, 
                       pickup_actividad AS pickupActividad, 
                       regreso_actividad AS regresoActividad, 
                       servicio_actividad AS incluyeActividad, 
                       precio_entrada AS precioEntrada, 
                       precio_tour_adulto AS precioTourAdulto, 
                       precio_tour_nino AS precioTourNino 
                FROM actividades 
                WHERE cliente_id = @ClienteId;";

            var actividades = db.Query<DatosActividad>(sqlActividades, new { ClienteId = idSeleccionado }).ToList();

            return (cliente, actividades);
        }

        public void ActualizarVoucher(int idSeleccionado, DatosCliente clienteModificado, IEnumerable<DatosActividad> actividadesModificadas)
        {
            using var db = new NpgsqlConnection(_connectionString);
            db.Open();

            // Iniciamos una transacción para asegurar consistencia total
            using var transaccion = db.BeginTransaction();

            try
            {
                // 1. Actualizar los datos maestros del cliente
                string sqlUpdateCliente = @"
                    UPDATE clientes 
                    SET nombre = @NombreCliente, 
                        cantidad_adultos = @CantidadAdultosCliente, 
                        cantidad_ninos = @CantidadNinosCliente, 
                        fecha_viaje = @FechaInicioCliente, 
                        telefono = @TelefonoCliente
                    WHERE id = @Id;";

                // Agregamos de forma temporal el ID al objeto anónimo para el WHERE
                db.Execute(sqlUpdateCliente, new
                {
                    NombreCliente = clienteModificado.NombreCliente,
                    CantidadAdultosCliente = clienteModificado.CantidadAdultosCliente,
                    CantidadNinosCliente = clienteModificado.CantidadNinosCliente,
                    FechaInicioCliente = clienteModificado.FechaInicioCliente,
                    TelefonoCliente = clienteModificado.TelefonoCliente,
                    Id = idSeleccionado
                }, transaction: transaccion);

                // 2. Eliminar todas las actividades viejas que pertenecían a este cliente
                string sqlDeleteActividades = "DELETE FROM actividades WHERE cliente_id = @ClienteId;";
                db.Execute(sqlDeleteActividades, new { ClienteId = idSeleccionado }, transaction: transaccion);

                // 3. Insertar el nuevo listado de actividades corregido
                string sqlInsertActividad = @"
                    INSERT INTO actividades (cliente_id, fecha_actividad, tipo_actividad, pickup_actividad, regreso_actividad, servicio_actividad, precio_entrada, precio_tour_adulto, precio_tour_nino, subtotal)
                    VALUES (@ClienteId, @FechaActividad, @TipoActividad, @PickupActividad, @RegresoActividad, @ServicioActividad, @PrecioEntrada, @PrecioTourAdulto, @PrecioTourNino, @Subtotal);";

                foreach (var act in actividadesModificadas)
                {
                    // Validamos que el usuario no haya dejado la fecha vacía en el DataGrid
                    if (act.FechaActividad == null) continue;

                    // Recalculamos el subtotal dinámico de la fila basado en la nueva cantidad de pasajeros
                    double subtotalCalculado = (act.PrecioEntrada + act.PrecioTourAdulto) * clienteModificado.CantidadAdultosCliente +
                                              (act.PrecioEntrada + act.PrecioTourNino) * clienteModificado.CantidadNinosCliente;

                    db.Execute(sqlInsertActividad, new
                    {
                        ClienteId = idSeleccionado,
                        FechaActividad = act.FechaActividad.Value, // .Value extrae el DateTime real del objeto anulable
                        TipoActividad = act.TipoActividad,
                        PickupActividad = act.PickupActividad,
                        RegresoActividad = act.RegresoActividad,
                        ServicioActividad = act.IncluyeActividad,
                        PrecioEntrada = act.PrecioEntrada,
                        PrecioTourAdulto = act.PrecioTourAdulto,
                        PrecioTourNino = act.PrecioTourNino,
                        Subtotal = subtotalCalculado
                    }, transaction: transaccion);
                }

                // Si todas las operaciones se completaron correctamente en el servidor, consolidamos los cambios
                transaccion.Commit();
            }
            catch (Exception)
            {
                // Si hubo micro-cortes de internet o errores, revertimos todo para proteger la integridad de los datos
                if (db.State == ConnectionState.Open)
                {
                    try { transaccion.Rollback(); } catch { /* Ignorar si la conexión colapsó por completo */ }
                }
                throw; // Re-lanzamos la excepción para capturarla en la Vista y mostrar la alerta visual
            }
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
                // Aseguramos existencia de la hoja
                worksheet = workbook.Worksheets.Contains("Viajes") ? workbook.Worksheet("Viajes") : workbook.Worksheets.Add("Viajes");
            }

            // 2. ESCRITURA DE LOS DATOS
            var lastRow = worksheet.LastRowUsed();
            int nuevaFilaId = lastRow == null ? 2 : lastRow.RowNumber() + 1;

            string fechaCreacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var actividad in actividades)
            {
                // Cálculo rápido del subtotal por fila (Ajusta la fórmula matemática según tus reglas de negocio)
                double subtotal = (actividad.PrecioEntrada + actividad.PrecioTourAdulto) * datosCliente.CantidadAdultosCliente
                    + (actividad.PrecioEntrada + actividad.PrecioTourNino) * datosCliente.CantidadNinosCliente;

                worksheet.Cell(nuevaFilaId, 1).Value = idActividad;
                worksheet.Cell(nuevaFilaId, 2).Value = fechaCreacion;
                worksheet.Cell(nuevaFilaId, 3).Value = datosCliente.NombreCliente;
                worksheet.Cell(nuevaFilaId, 4).Value = datosCliente.CantidadAdultosCliente;
                worksheet.Cell(nuevaFilaId, 5).Value = datosCliente.CantidadNinosCliente;
                worksheet.Cell(nuevaFilaId, 6).Value = datosCliente.FechaInicioCliente.ToString("yyyy-MM-dd");
                worksheet.Cell(nuevaFilaId, 7).Value = datosCliente.TelefonoCliente;

                worksheet.Cell(nuevaFilaId, 8).Value = actividad.FechaActividad.HasValue
                    ? actividad.FechaActividad.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
                worksheet.Cell(nuevaFilaId, 9).Value = actividad.TipoActividad;
                worksheet.Cell(nuevaFilaId, 10).Value = actividad.PickupActividad;
                worksheet.Cell(nuevaFilaId, 11).Value = actividad.RegresoActividad;
                worksheet.Cell(nuevaFilaId, 12).Value = actividad.IncluyeActividad;
                worksheet.Cell(nuevaFilaId, 13).Value = actividad.PrecioEntrada;
                worksheet.Cell(nuevaFilaId, 14).Value = actividad.PrecioTourAdulto;
                worksheet.Cell(nuevaFilaId, 15).Value = actividad.PrecioTourNino;
                worksheet.Cell(nuevaFilaId, 16).Value = subtotal;
                worksheet.Cell(nuevaFilaId, 17).Value = SesionSistema.UsuarioActual; // Responsable de la sesión

                nuevaFilaId++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(rutaArchivo);
            workbook.Dispose();
        }
    }
}