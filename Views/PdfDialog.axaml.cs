using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GeneradorVoucher_MP;

public partial class PdfDialog : Window
{
    public PdfDialog()
    {
        InitializeComponent();
    }

    // Permite personalizar el nombre del archivo en el mensaje si lo deseas
    public PdfDialog(string nombreArchivo) : this()
    {
        LblMensaje.Text = $"¡PDF generado con éxito!\n\n¿Desea abrir el archivo ahora?";
    }

    private void BtnSi_Click(object? sender, RoutedEventArgs e)
    {
        this.Close(true); // Retorna true si el usuario quiere abrirlo
    }

    private void BtnNo_Click(object? sender, RoutedEventArgs e)
    {
        this.Close(false); // Retorna false si decide no abrirlo
    }
}