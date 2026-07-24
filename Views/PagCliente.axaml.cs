using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using GeneradorVoucher_MP.Models;
using GeneradorVoucher_MP.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GeneradorVoucher_MP;

public partial class PagCliente : UserControl
{
    public PagCliente()
    {
        InitializeComponent();
        labelUsuario.Text = SesionSistema.LabelUsuarioActual;
        var manager = this.FindControl<WindowNotificationManager>("NotificationManager");
    }

    private void btnAgregarCliente_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(textNombreCliente.Text) ||
            string.IsNullOrEmpty(textTelefonoContacto.Text) ||
            textFechaInicioActividades.SelectedDate is null ||
            textPasajerosAdultos.Value <= 0 || textPasajerosAdultos.Value is null
            )
        {
            this.MostrarAlerta("Campo requerido", "Por favor, complete todos los campos correctamente.", NotificationType.Warning);

            if (textPasajerosAdultos.Value <= 0 || textPasajerosAdultos.Value is null) { textPasajerosAdultos.MarcarError(); }
            if (textFechaInicioActividades.SelectedDate is null) { textFechaInicioActividades.MarcarError(); }
            if (string.IsNullOrEmpty(textTelefonoContacto.Text)) { textTelefonoContacto.MarcarError(); }
            if (string.IsNullOrEmpty(textNombreCliente.Text)) { textNombreCliente.MarcarError(); }
            return;
        }

        try
        {
            var cliente = new DatosCliente
            {
                NombreCliente = textNombreCliente.Text,
                CantidadAdultosCliente = Convert.ToInt32(textPasajerosAdultos.Value),
                CantidadNinosCliente = Convert.ToInt32(textPasajerosNinos.Value),
                FechaInicioCliente = textFechaInicioActividades.SelectedDate ?? DateTime.Now,
                TelefonoCliente = textTelefonoContacto.Text
            };

            this.IrPagina(new PagActividad(cliente));

        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error", $"No se pudo registrar: {ex.Message}", NotificationType.Error);
        }
    }

    private void btnVerCarpeta_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            App.RegistrosService.AbrirRegistros();
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error", $"No se pudo abrir el archivo: {ex.Message}", NotificationType.Error);
        }
    }

    private void btnModificarPag_Click(object? sender, RoutedEventArgs e)
    {
        this.IrPagina(new PagModificar());
    }

    private void btnVolverInicio_Click(object? sender, RoutedEventArgs e)
    {
        this.IrPagina(new PagPrincipal());
    }
}