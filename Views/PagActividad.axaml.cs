using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DocumentFormat.OpenXml.Drawing.Charts;
using GeneradorVoucher;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Models;
using GeneradorVoucher_MP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace GeneradorVoucher_MP;

public partial class PagActividad : UserControl
{
    private List<ActividadPreestablecida> catalogoOpciones = new();
    private readonly ObservableCollection<DatosActividad> datosActividades = new();
    private readonly DatosCliente? clienteActual;

    public record IdiomaOpcion(IdiomaVoucher Value, string Label);
    public ObservableCollection<IdiomaOpcion> IdiomaOpciones { get; } = new ObservableCollection<IdiomaOpcion>
    {
        new IdiomaOpcion(IdiomaVoucher.Espanol, "Español"),
        new IdiomaOpcion(IdiomaVoucher.Ingles, "Inglés"),
        new IdiomaOpcion(IdiomaVoucher.Portugues, "Portugués")
    };
    public ObservableCollection<MonedasPago> MonedasPagoDisponibles { get; } = new ObservableCollection<MonedasPago>
    {
        MonedasPago.CLP,
        MonedasPago.R,
        MonedasPago.USD
    };
    public ObservableCollection<DatosActividad> DatosActividades => datosActividades;
    public List<ActividadPreestablecida> CatalogoOpciones => catalogoOpciones;

    public PagActividad()
    {
        InitializeComponent();
        CargarCatalogo(IdiomaVoucher.Espanol);
        cmbIdiomaVoucherActividad.SelectedItem = IdiomaOpciones[0]; // Selecciona Español por defecto
        datosActividades.CollectionChanged += datosActividades_CollectionChanged;
        dgvListaActividades.ItemsSource = datosActividades;
        this.DataContext = this;
    }

    // Constructor con parámetros para la aplicación real
    public PagActividad(DatosCliente datosCliente)
    {
        InitializeComponent();
        CargarCatalogo(IdiomaVoucher.Espanol);
        cmbIdiomaVoucherActividad.SelectedItem = IdiomaOpciones[0]; // Selecciona Español por defecto


        datosActividades.CollectionChanged += datosActividades_CollectionChanged;
        dgvListaActividades.ItemsSource = datosActividades;
        clienteActual = datosCliente;
        this.DataContext = this;
    }

    private void CargarCatalogo(IdiomaVoucher idioma)
    {
        try
        {
            catalogoOpciones = App.RegistrosService.ObtenerCatalogoActividades(idioma);
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
        else if (textFechaActividad.SelectedDate == null || cmbTourServicioActividad.SelectedItem == null)
        {
            this.MostrarAlerta("Campos Vacíos", "Debe seleccionar un tour o llenar los campos manualmente.", NotificationType.Warning);
            if (textFechaActividad.SelectedDate == null) textFechaActividad.MarcarError();

            return;
        }

        var nuevaActividad = new DatosActividad
        {
            FechaActividad = textFechaActividad.SelectedDate ?? DateTime.Now,
            TipoActividad = tourSeleccionado,
            PickupActividad = textHorarioPickUpActividad.Text?.Trim() ?? string.Empty,
            RegresoActividad = textHorarioRegresoActividad.Text?.Trim() ?? string.Empty,
            IncluyeActividad = textIncluyeActividad.Text?.Trim() ?? string.Empty,

            PrecioEntrada = double.TryParse(textPrecioEntradaActividad.Text, out var pEntrada) ? pEntrada : 0,
            PrecioTourAdulto = double.TryParse(textTourAdultoActividad.Text, out var pAdulto) ? pAdulto : 0,
            PrecioTourNino = double.TryParse(textTourNinoActividad.Text, out var pNino) ? pNino : 0
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

        if (clienteActual == null)
        {
            this.MostrarAlerta("Cliente faltante", "No se ha proporcionado información del cliente. Imposible crear el voucher.", NotificationType.Error);
            return;
        }

        if (chkDescuentoActividad.IsChecked == true)
        {
            if (!double.TryParse(textDescuentoActividad.Text, out double descuento) || descuento < 0 || descuento > 100)
            {
                this.MostrarAlerta("Descuento Inválido", "El valor del descuento debe ser un número positivo entre 0 y 100.", NotificationType.Warning);
                return;
            }

            foreach (var actividad in datosActividades)
            {
                actividad.DescuentoActividad = (float)descuento;
            }
        }
        else
        {
            foreach (var actividad in datosActividades)
            {
                actividad.DescuentoActividad = 0;
            }
        }

        try
        {

            IdiomaVoucher idiomaSeleccionado;
            if (cmbIdiomaVoucherActividad.SelectedValue is IdiomaOpcion idiomaOpcion)
            {
                idiomaSeleccionado = idiomaOpcion.Value;
            }
            else
            {
                idiomaSeleccionado = IdiomaVoucher.Espanol; // Valor por defecto
            }

            MonedasPago monedasSeleccionada;
            if (cmbMonedaSelect.SelectedItem is MonedasPago moneda)
            {
                monedasSeleccionada = moneda;
            }
            else
            {
                monedasSeleccionada = MonedasPago.CLP; // Valor por defecto
            }

            int idActividad = App.RegistrosService.GuardarViajeDB(clienteActual, datosActividades);
            this.MostrarAlerta("Éxito", $"¡Todo guardado con éxito! Se registró con el ID: {idActividad}", NotificationType.Success);
            await GeneradorPdf.GenerarReportePDF("Voucher",idActividad, clienteActual, datosActividades, idiomaSeleccionado, this, monedasSeleccionada);

            try
            {
                App.RegistrosService.GuardarViajeExcel(clienteActual, datosActividades, idActividad);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.MostrarAlerta("Error al guardar Excel", $"Ocurrió un problema al procesar el registro: {ex.Message}", NotificationType.Error);
            }

            cmbIdiomaVoucherActividad.IsEnabled = true;
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

    private void cmbIdiomaVoucherActividad_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IdiomaVoucher idiomaSeleccionado;
        if (cmbIdiomaVoucherActividad.SelectedValue is IdiomaOpcion idiomaOpcion)
        {
            idiomaSeleccionado = idiomaOpcion.Value;
        }
        else
        {
            idiomaSeleccionado = IdiomaVoucher.Espanol; // Valor por defecto
        }

        try
        {
            CargarCatalogo(idiomaSeleccionado);

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.MostrarAlerta("Error Crítico", $"Ocurrió un problema al cambiar el idioma: {ex.Message}", NotificationType.Error);

        }
    }

    private void datosActividades_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (datosActividades.Count > 0)
        {
            cmbIdiomaVoucherActividad.IsEnabled = false;
        }
        else
        {
            cmbIdiomaVoucherActividad.IsEnabled = true;
        }
    }
}