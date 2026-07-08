using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Views;
using GeneradorVoucher;
using GeneradorVoucher_MP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace GeneradorVoucher_MP;

public partial class PagActividad : UserControl
{
    private readonly ManejoRegistros manejoRegistros = new ManejoRegistros();
    private List<ActividadPreestablecida> catalogoOpciones = new();
    private readonly ObservableCollection<DatosActividad> datosActividades = new();
    private readonly DatosCliente? clienteActual;

    // Propiedad pública para que XAML pueda acceder
    public ObservableCollection<DatosActividad> DatosActividades => datosActividades;
    public List<ActividadPreestablecida> CatalogoOpciones => catalogoOpciones;

    public PagActividad()
    {
        InitializeComponent();
        CargarCatalogo();
        dgvListaActividades.ItemsSource = datosActividades;
        this.DataContext = this;
    }

    // Constructor con parámetros para la aplicación real
    public PagActividad(DatosCliente datosCliente)
    {
        InitializeComponent();
        CargarCatalogo();
        dgvListaActividades.ItemsSource = datosActividades;
        clienteActual = datosCliente;
        this.DataContext = this;
    }

    private void CargarCatalogo()
    {
        try
        {
            catalogoOpciones = manejoRegistros.ObtenerCatalogoActividades();
            cmbTourServicioActividad.ItemsSource = catalogoOpciones;
        }
        catch (FileNotFoundException)
        {
            // Feedback moderno usando el WindowNotificationManager a través de tu extensión
            this.MostrarAlerta("Configuración Opcional",
                "No se encontró 'Template_cotizaciones.xlsx'. Las opciones estarán vacías.",
                NotificationType.Information);
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error Carga",
                    $"Ocurrió un error al cargar el catálogo: {ex.Message}",
                    NotificationType.Error);
        }
    }

    private void btnVolverPagCliente_Click(object? sender, RoutedEventArgs e)
    {
        this.IrPagina(new PagCliente());
    }

    private void cmbTourServicio_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (cmbTourServicioActividad.SelectedItem is not ActividadPreestablecida actividadPreestablecida) return;

        textHorarioPickUpActividad.Text = actividadPreestablecida.pickUp;
        textHorarioRegresoActividad.Text = actividadPreestablecida.regreso;
        textIncluyeActividad.Text = actividadPreestablecida.incluye;
        textPrecioEntradaActividad.Text = actividadPreestablecida.precioEntrada.ToString();
        textTourAdultoActividad.Text = actividadPreestablecida.precioTourAdulto.ToString();

        if (string.IsNullOrEmpty(actividadPreestablecida.precioTourNino))
        {
            textTourNinoActividad.Text = actividadPreestablecida.precioTourAdulto.ToString();
            this.MostrarAlerta("Precio Niño", "No existe precio diferenciado para menores de edad, se utilizará el precio de adultos", NotificationType.Information, 8);
        }
        else textTourNinoActividad.Text = actividadPreestablecida.precioTourNino;

    }

    private void btnAgregarActividad_Click(object? sender, RoutedEventArgs e)
    {
        string tourSeleccionado = string.Empty;
        if (cmbTourServicioActividad.SelectedItem is ActividadPreestablecida actividad)
        {
            tourSeleccionado = actividad.tour;
        }

        var nuevaActividad = new DatosActividad
        {
            // En Avalonia se suele usar DatePicker (SelectedDate) o CalendarDatePicker
            fechaActividad = textFechaActividad.SelectedDate ?? DateTime.Now,
            tipoActividad = tourSeleccionado,
            pickupActividad = textHorarioPickUpActividad.Text?.Trim() ?? string.Empty,
            regresoActividad = textHorarioRegresoActividad.Text?.Trim() ?? string.Empty,
            incluyeActividad = textIncluyeActividad.Text?.Trim() ?? string.Empty,

            precioEntrada = double.TryParse(textPrecioEntradaActividad.Text, out var pEntrada) ? pEntrada : 0,
            precioTourAdulto = double.TryParse(textTourAdultoActividad.Text, out var pAdulto) ? pAdulto : 0,
            precioTourNino = double.TryParse(textTourNinoActividad.Text, out var pNino) ? pNino : 0
        };

        datosActividades.Add(nuevaActividad);

        CleanInputs();
    }

    private void CleanInputs()
    {
        //textFechaActividad.SelectedDate = DateTime.Now;
        cmbTourServicioActividad.SelectedItem = -1;
        textHorarioPickUpActividad.Text = string.Empty;
        textHorarioRegresoActividad.Text = string.Empty;
        textIncluyeActividad.Text = string.Empty;
        textPrecioEntradaActividad.Text = string.Empty;
        textTourAdultoActividad.Text = string.Empty;
        textTourNinoActividad.Text = string.Empty;

        cmbTourServicioActividad.Focus();
    }

    private async void btnCrearVoucher_Click(object? sender, RoutedEventArgs e)
    {
        if (datosActividades.Count == 0)
        {
            this.MostrarAlerta("Tabla Vacía", "Debe agregar al menos una actividad al itinerario.", NotificationType.Warning);
            return;
        }

        try
        {
            manejoRegistros.GuardarViajeExcel(clienteActual, datosActividades);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.MostrarAlerta("Error Crítico", $"Ocurrió un problema al procesar el registro: {ex.Message}", NotificationType.Error);
        }

        try
        {
            int idActividad = manejoRegistros.GuardarViajeDB(clienteActual, datosActividades);
            this.MostrarAlerta("Éxito", $"¡Todo guardado con éxito! Se registró con el ID: {idActividad}", NotificationType.Success);
            await GeneradorPdf.GenerarReportePDF(idActividad, clienteActual, datosActividades, this);

            this.IrPagina(new PagCliente());
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.MostrarAlerta("Error Crítico", $"Ocurrió un problema al procesar el registro: {ex.Message}", NotificationType.Error);
        }
    }

    private void dgvListaActividades_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            var seleccionada = dgvListaActividades.SelectedItem as DatosActividad;
            if (seleccionada != null)
            {
                datosActividades.Remove(seleccionada);
            }
        }
    }
}