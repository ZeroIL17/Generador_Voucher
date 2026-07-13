using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Wordprocessing;
using GeneradorVoucher;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GeneradorVoucher_MP;

public partial class PagModificar : UserControl
{
    private readonly ManejoRegistros manejoRegistros = new ManejoRegistros();
    private readonly ObservableCollection<DatosActividad> actividadesMostradas = new();
    private readonly ObservableCollection<VoucherLookup> vouchersDisponibles = new();
    private List<ActividadPreestablecida> catalogoOpciones = new();


    public ObservableCollection<DatosActividad> DatosActividades => actividadesMostradas;
    public ObservableCollection<VoucherLookup> VouchersDisponibles => vouchersDisponibles;
    public ManejoRegistros ManejoRegistros => manejoRegistros;
    public List<ActividadPreestablecida> CatalogoOpciones => catalogoOpciones;

    public record IdiomaOpcion(IdiomaVoucher Value, string Label);
    public ObservableCollection<IdiomaOpcion> IdiomaOpciones { get; } = new ObservableCollection<IdiomaOpcion>
    {
        new IdiomaOpcion(IdiomaVoucher.Espanol, "Español"),
        new IdiomaOpcion(IdiomaVoucher.Ingles, "Inglés"),
        new IdiomaOpcion(IdiomaVoucher.Portugues, "Portugués")
    };

    public PagModificar()
    {
        InitializeComponent();
        SetInputEnabled(false);
        dgvActividadesMostradas.ItemsSource = actividadesMostradas;
        cmbIdiomaVoucherModificar.SelectedItem = IdiomaOpciones[0];
        this.DataContext = this;

        // Evita llamadas async no aguardadas en el constructor suscribiendo un handler Loaded
        this.Loaded += PagModificar_Loaded;
    }

    private async void PagModificar_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Ahora se awaitea la carga asíncrona y la carga sincrónica se ejecuta después
        await CargarVouchers();
        CargarCatalogo(IdiomaVoucher.Espanol);
    }

    private async Task CargarVouchers()
    {
        try
        {
            var listaVouchers = await Task.Run(() => manejoRegistros.CargarVouchersDB());
            vouchersDisponibles.Clear();
            foreach (var voucher in listaVouchers)
            {
                vouchersDisponibles.Add(voucher);
            }
            cmbVoucherCreados.ItemsSource = vouchersDisponibles;
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error", $"No se pudieron cargar los vouchers: {ex.Message}", NotificationType.Error);
        }
    }

    private void cmbVoucherCreados_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (cmbVoucherCreados.SelectedItem is not VoucherLookup voucherSeleccionado) return;

        try
        {
            var (cliente, listaActividades) = manejoRegistros.ObtenerDetalleVoucher(voucherSeleccionado.Id);

            textNombreClienteModificar.Text = cliente.NombreCliente;
            textAdultosModificar.Value = cliente.CantidadAdultosCliente;
            textNinosModificar.Value = cliente.CantidadNinosCliente;
            textFechaInicioModificar.SelectedDate = cliente.FechaInicioCliente;
            textTelefonoModificar.Text = cliente.TelefonoCliente;

            actividadesMostradas.Clear();

            foreach (var actividad in listaActividades)
            {
                actividadesMostradas.Add(actividad);
            }

            SetInputEnabled(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            this.MostrarAlerta("Error de Lectura", ex.Message, NotificationType.Error);
        }
    }

    private void SetInputEnabled(bool fieldState)
    {
        textNombreClienteModificar.IsEnabled = fieldState;
        textAdultosModificar.IsEnabled = fieldState;
        textNinosModificar.IsEnabled = fieldState;
        textFechaInicioModificar.IsEnabled = fieldState;
        textTelefonoModificar.IsEnabled = fieldState;
        cmbTourServicioActividadModificar.IsEnabled = fieldState;
    }

    private void btnVolverPagCliente_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IrPagina(new PagCliente());
    }

    private async void btnActualizarVoucher_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (cmbVoucherCreados.SelectedItem is not VoucherLookup voucherSeleccionado)
        {
            this.MostrarAlerta("Error", "Por favor, seleccione un voucher para modificar.", NotificationType.Error);
            return;
        }

        textNombreClienteModificar.RestaurarEstilo();

        // 2. Validar campos requeridos mínimos
        if (string.IsNullOrWhiteSpace(textNombreClienteModificar.Text) ||
            string.IsNullOrWhiteSpace(textTelefonoModificar.Text) ||
            textFechaInicioModificar.SelectedDate == null ||
            textAdultosModificar.Value < 1 || textAdultosModificar.Value is null)
        {
            this.MostrarAlerta("Campos Incompletos", "Por favor, complete todos los campos del cliente antes de guardar.", NotificationType.Warning);
            if (textAdultosModificar.Value < 1 || textAdultosModificar.Value is null) textAdultosModificar.MarcarError();
            if (string.IsNullOrWhiteSpace(textTelefonoModificar.Text)) textTelefonoModificar.MarcarError();
            if (textFechaInicioModificar.SelectedDate == null) textFechaInicioModificar.MarcarError();
            if (string.IsNullOrWhiteSpace(textNombreClienteModificar.Text)) textNombreClienteModificar.MarcarError();

            return;
        }

        foreach (var act in actividadesMostradas)
        {
            if (act.FechaActividad == null || act.FechaActividad == DateTime.MinValue)
            {
                this.MostrarAlerta("Falta Información",
                                   $"Por favor, asigna una fecha válida al servicio: '{act.TipoActividad}'.",
                                   NotificationType.Warning);
                return; // Detiene la ejecución completa del guardado
            }
        }

        try
        {
            IdiomaVoucher idiomaSeleccionado;
            if (cmbIdiomaVoucherModificar.SelectedValue is IdiomaOpcion idiomaOpcion)
            {
                idiomaSeleccionado = idiomaOpcion.Value;
            }
            else
            {
                idiomaSeleccionado = IdiomaVoucher.Espanol; // Valor por defecto
            }

            var clienteModificado = new DatosCliente
            {
                NombreCliente = textNombreClienteModificar.Text,
                CantidadAdultosCliente = Convert.ToInt32(textAdultosModificar.Value),
                CantidadNinosCliente = Convert.ToInt32(textNinosModificar.Value),
                FechaInicioCliente = textFechaInicioModificar.SelectedDate ?? DateTime.Now,
                TelefonoCliente = textTelefonoModificar.Text
            };

            manejoRegistros.ActualizarVoucher(voucherSeleccionado.Id, clienteModificado, actividadesMostradas);
            this.MostrarAlerta("Operación Completada", "Voucher modificado y PDF actualizado con éxito.", NotificationType.Success);
            await GeneradorPdf.GenerarReportePDF(voucherSeleccionado.Id, clienteModificado, actividadesMostradas, idiomaSeleccionado, this);

            CleanInputs();

            await CargarVouchers();
            this.IrPagina(new PagCliente());
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error Crítico", $"No se pudo guardar la modificación: {ex.Message}", NotificationType.Error);
            Debug.WriteLine(ex);
        }
    }

    private void CleanInputs()
    {
        textNombreClienteModificar.Text = string.Empty;
        textAdultosModificar.Value = 1;
        textNinosModificar.Value = 0;
        textFechaInicioModificar.SelectedDate = null;
        textTelefonoModificar.Text = string.Empty;

        cmbVoucherCreados.SelectedItem = null;
        cmbVoucherCreados.Text = string.Empty;
        actividadesMostradas.Clear();


        SetInputEnabled(false);
    }

    private void dgvActividadesMostradas_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            var seleccionada = dgvActividadesMostradas.SelectedItem as DatosActividad;
            if (seleccionada != null && actividadesMostradas.Count > 1)
            {
                actividadesMostradas.Remove(seleccionada);
            }
        }
    }
    private void CargarCatalogo(IdiomaVoucher idioma)
    {
        try
        {
            catalogoOpciones = manejoRegistros.ObtenerCatalogoActividades(idioma);
            cmbTourServicioActividadModificar.ItemsSource = catalogoOpciones;
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

    private void cmbTourServicioActividadModificar_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (cmbTourServicioActividadModificar.SelectedItem is not ActividadPreestablecida actividadPreestablecida) return;

        var nuevaActividad = new DatosActividad
        {
            FechaActividad = null,
            IncluyeActividad = actividadPreestablecida.incluye,
            PickupActividad = actividadPreestablecida.pickUp,
            PrecioEntrada = actividadPreestablecida.precioEntrada,
            PrecioTourAdulto = actividadPreestablecida.precioTourAdulto,
            PrecioTourNino = double.TryParse(actividadPreestablecida.precioTourNino, out var pNino) ? pNino : 0,
            RegresoActividad = actividadPreestablecida.regreso,
            TipoActividad = actividadPreestablecida.tour
        };
        actividadesMostradas.Add(nuevaActividad);
    }

    private void cmbIdiomaVoucherModificar_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        IdiomaVoucher idiomaSeleccionado;
        if (cmbIdiomaVoucherModificar.SelectedValue is IdiomaOpcion idiomaOpcion)
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
}