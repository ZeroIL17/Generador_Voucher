using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Views;
using GeneradorVoucher_MP.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeneradorVoucher_MP;

public partial class PagInicio : UserControl
{
    private readonly ManejoUsuarios manejoUsuarios = new ManejoUsuarios();
    public PagInicio()
    {
        InitializeComponent();
        CargarUsuarios();
    }

    private void btnPagCliente_Click(object? sender, RoutedEventArgs e)
    {
        string? usuario = null;

        if (cmbSelectUsuario.SelectedItem is string selectedUsuario)
        {
            usuario = selectedUsuario;
        }
        else if (!string.IsNullOrEmpty(cmbSelectUsuario.Text))
        {
            usuario = cmbSelectUsuario.Text;
        }
        else
        {
            this.MostrarAlerta("Usuario", "Debe ingresar con un usuario válido.", NotificationType.Error);
            return;
        }

        if (usuario != null)
        {
            SesionSistema.UsuarioActual = usuario;
        }

        this.IrPagina(new PagCliente());

    }

    private void CargarUsuarios()
    {
        try
        {
            List<string> listaUsuarios = manejoUsuarios.ObtenerUsuarios();

            cmbSelectUsuario.ItemsSource = listaUsuarios;
        }
        catch (Exception ex)
        {
            this.MostrarAlerta("Error de Configuración",
                    $"No se pudieron cargar los operadores del sistema: {ex.Message}",
                    NotificationType.Error);
        }
    }
}