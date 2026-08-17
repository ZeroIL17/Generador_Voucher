using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using GeneradorVoucher_MP.Models;
using GeneradorVoucher_MP.Views;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GeneradorVoucher_MP;

public partial class PagPrincipal : UserControl
{
    public PagPrincipal()
    {
        InitializeComponent();
        labelUsuario.Text = SesionSistema.LabelUsuarioActual;
    }

    private void btnPaginaCliente_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IrPagina(new PagCliente());
    }

    private void btnPaginaConfirmacion_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IrPagina(new PagConfirmacion());
    }

    private void btnModificarVoucher_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IrPagina(new PagModificar());
    }

    private void btnVolverPagInicio_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.IrPagina(new PagInicio());
    }

    private async void btnExportarDatos_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            // Deshabilitamos el botón momentáneamente
            btnExportarDatos.IsEnabled = false;

            // Ejecutamos la consulta y creación del Excel en un hilo de fondo
            string rutaArchivoReporte = await Task.Run(() => App.RegistrosService.ExportarRegistrosExcel());

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
            btnExportarDatos.IsEnabled = true;
        }
    }
    private void btnAbrirCarpeta_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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

    private void btnAjustes_Click(object? sender, RoutedEventArgs e)
    {
        this.IrPagina(new PagAjustes());
    }
}