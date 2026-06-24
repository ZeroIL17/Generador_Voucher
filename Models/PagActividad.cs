using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Text;
using System.Xaml.Permissions;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;


namespace GeneradorVoucher
{
    public partial class PagActividad : UserControl
    {
        private BindingList<datosActividad> listaActividad;
        private dynamic datosClientes;
        public int IdActividad = 1;
        private List<ActividadPreestablecida> catalogoOpciones = new List<ActividadPreestablecida>();

        public PagActividad(dynamic datosClienteAnterior)
        {
            InitializeComponent();
            listaActividad = new BindingList<datosActividad>();
            this.datosClientes = datosClienteAnterior;

        }

        public class datosActividad
        {
            public string fechaActividad { get; set; }
            public string tipoActividad { get; set; } = string.Empty;
            public string pickupActividad { get; set; }
            public string regresoActividad { get; set; }
            public string servicioActividad { get; set; } = string.Empty;
            public double precioEntrada { get; set; }
            public double precioTour { get; set; }

        }

        public class filaExcel
        {
            public int Id { get; set; }
            public string nombreCliente { get; set; }
            public int cantidadCliente { get; set; }
            public string fechaInicio { get; set; }
            public string telefonoCliente { get; set; }
            public string fechaActividad { get; set; }
            public string tipoActividad { get; set; } = string.Empty;
            public string pickupActividad { get; set; }
            public string regresoActividad { get; set; }
            public string servicioActividad { get; set; } = string.Empty;
            public double precioEntrada { get; set; }
            public double precioTour { get; set; }
            public double subtotalActividad => (precioEntrada + precioTour) * cantidadCliente;
        }
        public class ActividadPreestablecida
        {
            public string tour { get; set; }
            public string pickUp { get; set; }
            public string regreso { get; set; }
            public string incluye { get; set; }
            public double precioEntrada { get; set; }
            public double precioTour { get; set; }
        }
        private void PagActividad_Load(object sender, EventArgs e)
        {
            textActividadFecha.Focus();
            dgvActividades.DataSource = listaActividad;
            dgvActividades.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvActividades.AllowUserToAddRows = false; // Evita la fila vacía extra del final

            cargarCatalogo();
        }

        private void cargarCatalogo()
        {
            string rutaCatalogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template_cotizaciones.xlsx");

            if (!File.Exists(rutaCatalogo))
            {
                MessageBox.Show("No se encontró el archivo 'Template_cotizaciones.xlsx'. Las opciones del ComboBox estarán vacías.",
                                "Configuración Opcional", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
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
                            precioTour = fila.Cell(6).GetValue<double>()
                        };
                        catalogoOpciones.Add(opcion);
                    }
                }

                cmbTour.Items.Clear();
                foreach (var opcion in catalogoOpciones)
                {
                    cmbTour.Items.Add(opcion.tour);
                }
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al cargar el catálogo de actividades. Las opciones del ComboBox estarán vacías.",
                                "Error Carga", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        private void btnActividadAgregarDatos_Click(object sender, EventArgs e)
        {
            datosActividad nuevaActividad = new datosActividad
            {
                fechaActividad = textActividadFecha.Value.ToString("yyyy-MM-dd"),
                tipoActividad = cmbTour.SelectedItem?.ToString().Trim() ?? string.Empty,
                pickupActividad = textPickUpActividad.Text.Trim(),
                regresoActividad = textRegresoActividad.Text.Trim(),
                servicioActividad = textActividadIncluye.Text.Trim(),
                precioEntrada = textActividadPrecioEntrada.Text == "" ? 0 : double.Parse(textActividadPrecioEntrada.Text),
                precioTour = textActividadPrecioTour.Text == "" ? 0 : double.Parse(textActividadPrecioTour.Text)
            };

            listaActividad.Add(nuevaActividad);

            cleanInputs();
        }

        private void cleanInputs()
        {
            //textActividadFecha.Value = DateTime.Now;
            cmbTour.SelectedIndex = -1;
            textPickUpActividad.Text = string.Empty;
            textRegresoActividad.Text = string.Empty;
            textActividadIncluye.Text = string.Empty;
            textActividadPrecioEntrada.Text = string.Empty;
            textActividadPrecioTour.Text = string.Empty;

            cmbTour.Focus();
        }

        private void btnAgregarExcel_Click(object sender, EventArgs e)
        {
            string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlanillaViajes.xlsx");

            XLWorkbook workbook;
            IXLWorksheet worksheet;

            // El ID inicial será 1
            int IdActividad = 1;

            if (!File.Exists(rutaArchivo))
            {
                // --- ESCENARIO 1: EL ARCHIVO NO EXISTE (Primer registro) ---
                workbook = new XLWorkbook();
                worksheet = workbook.Worksheets.Add("Viajes");

                // Crear los encabezados de las columnas
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Fecha Creación";
                worksheet.Cell(1, 3).Value = "Nombre Cliente";
                worksheet.Cell(1, 4).Value = "Cantidad Personas";
                worksheet.Cell(1, 5).Value = "Fecha Inicio";
                worksheet.Cell(1, 6).Value = "Telefono";
                worksheet.Cell(1, 7).Value = "Fecha Actividad";
                worksheet.Cell(1, 8).Value = "Tipo Actividad";
                worksheet.Cell(1, 9).Value = "PickUp Actividad";
                worksheet.Cell(1, 10).Value = "Regreso Actividad";
                worksheet.Cell(1, 11).Value = "Servicio Actividad";
                worksheet.Cell(1, 12).Value = "Precio Entrada";
                worksheet.Cell(1, 13).Value = "Precio Tour";
                worksheet.Cell(1, 14).Value = "Subtotal";

                // Estilo rápido opcional para los encabezados (Negrita)
                worksheet.Row(1).Style.Font.Bold = true;

            }
            else
            {
                // --- ESCENARIO 2: EL ARCHIVO YA EXISTE ---
                workbook = new XLWorkbook(rutaArchivo);
                worksheet = workbook.Worksheet("Viajes");

                IdActividad = obtenerID();

            }

            // --- ESCRITURA DE LOS DATOS ---
            // Encontramos la siguiente fila vacía disponible
            int nuevaFilaId = worksheet.LastRowUsed() == null ? 2 : worksheet.LastRowUsed().RowNumber() + 1;

            foreach (var actividad in listaActividad)
            {
                filaExcel nuevoViaje = new filaExcel
                {
                    Id = IdActividad,
                    nombreCliente = datosClientes.nombreCliente,
                    cantidadCliente = Convert.ToInt32(datosClientes.cantidadCliente),
                    fechaInicio = datosClientes.fechaCliente,
                    telefonoCliente = datosClientes.telefonoCliente,
                    fechaActividad = actividad.fechaActividad,
                    tipoActividad = actividad.tipoActividad,
                    pickupActividad = actividad.pickupActividad,
                    regresoActividad = actividad.regresoActividad,
                    servicioActividad = actividad.servicioActividad,
                    precioEntrada = actividad.precioEntrada,
                    precioTour = actividad.precioTour
                };

                worksheet.Cell(nuevaFilaId, 1).Value = IdActividad;
                worksheet.Cell(nuevaFilaId, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(nuevaFilaId, 3).Value = nuevoViaje.nombreCliente;
                worksheet.Cell(nuevaFilaId, 4).Value = (int)nuevoViaje.cantidadCliente;
                worksheet.Cell(nuevaFilaId, 5).Value = nuevoViaje.fechaInicio;
                worksheet.Cell(nuevaFilaId, 6).Value = nuevoViaje.telefonoCliente;
                worksheet.Cell(nuevaFilaId, 7).Value = nuevoViaje.fechaActividad;
                worksheet.Cell(nuevaFilaId, 8).Value = nuevoViaje.tipoActividad;
                worksheet.Cell(nuevaFilaId, 9).Value = nuevoViaje.pickupActividad;
                worksheet.Cell(nuevaFilaId, 10).Value = nuevoViaje.regresoActividad;
                worksheet.Cell(nuevaFilaId, 11).Value = nuevoViaje.servicioActividad;
                worksheet.Cell(nuevaFilaId, 12).Value = nuevoViaje.precioEntrada;
                worksheet.Cell(nuevaFilaId, 13).Value = nuevoViaje.precioTour;
                worksheet.Cell(nuevaFilaId, 14).Value = nuevoViaje.subtotalActividad;

                nuevaFilaId++;
            }


            // Ajustar el ancho de las columnas automáticamente para que no se corte el texto
            worksheet.Columns().AdjustToContents();

            // Guardar el archivo y liberar memoria
            workbook.SaveAs(rutaArchivo);
            workbook.Dispose();
            MessageBox.Show($"¡Todo guardado con éxito! Se registró al cliente con el ID: {IdActividad}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            GeneradorPdf.GenerarReportePDF(IdActividad, datosClientes, listaActividad);
            regresarPantallaCliente();

        }

        private void regresarPantallaCliente()
        {
            Forms1 formularioPrincipal = (Forms1)this.FindForm();

            if (formularioPrincipal != null)
            {
                PagCliente primeraPagina = new PagCliente();
                primeraPagina.Dock = DockStyle.Fill;
                Panel contenedor = (Panel)formularioPrincipal.Controls["panelContenedor"];
                contenedor.Controls.Clear();
                contenedor.Controls.Add(primeraPagina);
            }
        }

        private int obtenerID()
        {
            string rutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PlanillaViajes.xlsx");

            XLWorkbook workbook;
            IXLWorksheet worksheet;

            int IdActividad = 1;

            if (!File.Exists(rutaArchivo))
            {
                return IdActividad;
            }
            else
            {
                workbook = new XLWorkbook(rutaArchivo);
                worksheet = workbook.Worksheet("Viajes");

                // Buscar la última fila con datos utilizando LastRowUsed()
                var ultimaFila = worksheet.LastRowUsed();

                if (ultimaFila != null && ultimaFila.RowNumber() > 1)
                {
                    int maxId = worksheet.Column(1).CellsUsed()
                                                    .Skip(1)
                                                    .Select(cell =>
                                                    {
                                                        int.TryParse(cell.Value.ToString(), out int val);
                                                        return val;
                                                    })
                                                    .DefaultIfEmpty(0)
                                                    .Max();
                    IdActividad = maxId + 1;

                }
                else
                {
                    // Si por alguna razón el archivo tiene solo cabeceras pero no datos
                    IdActividad = 1;
                }

                return IdActividad;
            }
        }


        private void cmbTour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTour.SelectedItem == null)
                return;

            string opcionSeleccionada = cmbTour.SelectedItem.ToString();

            if (string.IsNullOrEmpty(opcionSeleccionada))
                return;

            var actividadSeleccionada = catalogoOpciones.FirstOrDefault(o => o.tour == opcionSeleccionada);

            if (actividadSeleccionada != null)
            {
                textActividadIncluye.Text = actividadSeleccionada.incluye;
                textPickUpActividad.Text = actividadSeleccionada.pickUp;
                textRegresoActividad.Text = actividadSeleccionada.regreso;
                textActividadPrecioEntrada.Text = actividadSeleccionada.precioEntrada.ToString();
                textActividadPrecioTour.Text = actividadSeleccionada.precioTour.ToString();
            }
        }

        private void btnVolverPagActividades_Click(object sender, EventArgs e)
        {
            regresarPantallaCliente();
        }
    }

}
