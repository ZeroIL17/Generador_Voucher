using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using GeneradorVoucher_MP.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GeneradorVoucher_MP;

public partial class PagCliente : UserControl
{
    private readonly ManejoRegistros manejoRegistros = new ManejoRegistros();
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
            manejoRegistros.AbrirRegistros();
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
        this.IrPagina(new PagInicio());
    }

    private async void btnExportarRegistros_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Deshabilitamos el botón momentáneamente
            btnExportarRegistros.IsEnabled = false;

            // Ejecutamos la consulta y creación del Excel en un hilo de fondo
            string rutaArchivoReporte = await Task.Run(() => manejoRegistros.ExportarRegistrosExcel());

            this.MostrarAlerta("Exportación Exitosa",
                               $"El reporte se guardó correctamente en:\n{Path.GetFileName(rutaArchivoReporte)}",
                               NotificationType.Success);

            // Opcional: Si quieres abrir el Excel automáticamente al finalizar
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(rutaArchivoReporte)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error al Exportar", $"Ocurrió un detalle: {ex.Message}", NotificationType.Error);
        }
        finally
        {
            btnExportarRegistros.IsEnabled = true;
        }
    }
}