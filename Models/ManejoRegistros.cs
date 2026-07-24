using ClosedXML.Excel;
using Dapper;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Views;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GeneradorVoucher_MP.Models
{
    public class ManejoRegistros
    {
        // Tu cadena de conexión segura provista por tu hosting de Postgres
        private static readonly string BaseConnectionString = "Host=aws-1-us-west-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.jaqvtgzkrajesvbqublf;SSL Mode=Require;Timeout=30;Command Timeout=30;";
        private readonly string _connectionString;

        // Método auxiliar para abrir la conexión de forma limpia
        private IDbConnection ObtenerConexion() => new NpgsqlConnection(_connectionString);

        private readonly string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registro_Cotizaciones.xlsx");
        readonly string rutaCarpetaDirectorio = AppDomain.CurrentDomain.BaseDirectory;

        public ManejoRegistros(string password)
        {
            _connectionString =  $"{BaseConnectionString}Password={password}";
        }

        public static bool ProbarConexion(string password)
        {
            try
            {
                string connString = $"{BaseConnectionString}Password={password};";
                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                return true; // Conexión exitosa
            }
            catch
            {
                return false; // Contraseña incorrecta o sin internet
            }
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

        public string ExportarRegistrosExcel()
        {
            string nombreArchivoExportado = $"PlanillaViajes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            // crear carpeta para guardar los export 
            string carpetaExport = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export");
            if (!Directory.Exists(carpetaExport))
            {
                Directory.CreateDirectory(carpetaExport);
            }

            string rutaDestino = Path.Combine(carpetaExport, nombreArchivoExportado);

            string query = @"
                SELECT 
                    v.id,
                    v.fecha_creacion,
                    v.nombre,
                    v.cantidad_adultos,
                    v.cantidad_ninos,
                    v.fecha_viaje,
                    v.telefono,
                    v.usuario_creacion,
                    a.fecha_actividad,
                    a.tipo_actividad,
                    a.pickup_actividad,
                    a.regreso_actividad,
                    a.servicio_actividad,
                    a.precio_entrada,
                    a.precio_tour_adulto,
                    a.precio_tour_nino
                FROM clientes v
                LEFT JOIN actividades a ON v.id = a.cliente_id
                ORDER BY v.id DESC, a.fecha_actividad ASC;";

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("General Vouchers");

                // 1. Estilizar y crear Encabezados
                string[] encabezados = {
                    "Voucher_ID", "Fecha Creación", "Cliente", "Cantidad_Adultos", "Cantidad_Niños",
                    "Fecha_Inicio_Viaje", "Teléfono", "Usuario_responsable" ,"Fecha Actividad", "Actividad"
                    ,"Pick-up", "Retorno","Incluye", "Precio Entrada", "Precio Adulto",
                    "Precio Niño"
                };

                for (int i = 0; i < encabezados.Length; i++)
                {
                    var celda = worksheet.Cell(1, i + 1);
                    celda.Value = encabezados[i];
                    celda.Style.Font.Bold = true;
                    celda.Style.Font.FontColor = XLColor.White;
                    celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E78"); // Azul corporativo
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // 2. Conectarse a la Base de Datos y rellenar filas
                int filaActual = 2;
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            worksheet.Cell(filaActual, 1).Value = reader.GetInt32(0); // ID
                            if (!reader.IsDBNull(1)) worksheet.Cell(filaActual, 2).Value = reader.GetDateTime(1).ToString("dd/MM/yyyy HH:mm"); // fecha creacion
                            worksheet.Cell(filaActual, 3).Value = reader.IsDBNull(2) ? string.Empty : reader.GetString(2); // Cliente
                            worksheet.Cell(filaActual, 4).Value = reader.IsDBNull(3) ? 0 : reader.GetInt32(3); // Adultos
                            worksheet.Cell(filaActual, 5).Value = reader.IsDBNull(4) ? 0 : reader.GetInt32(4); // Niños
                            if (!reader.IsDBNull(5)) worksheet.Cell(filaActual, 6).Value = reader.GetDateTime(5).ToString("dd/MM/yyyy"); // Fecha de viaje
                            worksheet.Cell(filaActual, 7).Value = reader.IsDBNull(6) ? string.Empty : reader.GetString(6); // Teléfono
                            worksheet.Cell(filaActual, 8).Value = reader.IsDBNull(7) ? string.Empty : reader.GetString(7); // usuario responsable


                            // Datos de la Actividad (Provenientes del LEFT JOIN)
                            if (!reader.IsDBNull(8)) worksheet.Cell(filaActual, 9).Value = reader.GetDateTime(8).ToString("dd/MM/yyyy"); // Fecha Actividad
                            worksheet.Cell(filaActual, 10).Value = reader.IsDBNull(9) ? "Sin Actividades" : reader.GetString(9); // Tipo de Actividad
                            worksheet.Cell(filaActual, 11).Value = reader.IsDBNull(10) ? string.Empty : reader.GetString(10); // pickup
                            worksheet.Cell(filaActual, 12).Value = reader.IsDBNull(11) ? string.Empty : reader.GetString(11); // regreso
                            worksheet.Cell(filaActual, 13).Value = reader.IsDBNull(12) ? "Sin Actividades" : reader.GetString(12); // incluye

                            // Precios (Formatos numéricos)
                            worksheet.Cell(filaActual, 14).Value = reader.IsDBNull(13) ? 0.0 : reader.GetDouble(13); //entrada
                            worksheet.Cell(filaActual, 15).Value = reader.IsDBNull(14) ? 0.0 : reader.GetDouble(14); //precio tour adulto
                            worksheet.Cell(filaActual, 16).Value = reader.IsDBNull(15) ? 0.0 : reader.GetDouble(15); // precio tour niño


                            // Aplicar formato de moneda a las columnas de dinero (Columnas 12, 13 y 14)
                            worksheet.Cell(filaActual, 14).Style.NumberFormat.Format = "$#,##0.00";
                            worksheet.Cell(filaActual, 15).Style.NumberFormat.Format = "$#,##0.00";
                            worksheet.Cell(filaActual, 16).Style.NumberFormat.Format = "$#,##0.00";

                            filaActual++;
                        }
                    }
                }

                // 3. Ajustes de diseño automáticos
                worksheet.Columns().AdjustToContents(); // Autoajustar ancho de columnas
                worksheet.SheetView.FreezeRows(1);    // Congelar la fila de encabezados

                // 4. Guardar archivo en el disco
                workbook.SaveAs(rutaDestino);
            }

            return rutaDestino; // Retornamos la ruta por si quieres abrirlo o mostrar un aviso

        }
    }
}