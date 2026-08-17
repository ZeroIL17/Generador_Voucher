using Avalonia.Controls;
using Avalonia.Interactivity;
using GeneradorVoucher_MP.Models;
using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace GeneradorVoucher_MP.Views
{
    public partial class PagAjustes : UserControl
    {
        public PagAjustes()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var s = Ajustes.GetAppSettings();
            txtClpToUsd.Text = s.ClpToUsd.ToString(CultureInfo.InvariantCulture);
            txtClpToBrl.Text = s.ClpToBrl.ToString(CultureInfo.InvariantCulture);
            txtStatus.Text = "Valores cargados";
        }

        private void btnGuardarAjustes_Click(object? sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtClpToUsd.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var usd) ||
                !double.TryParse(txtClpToBrl.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var brl))
            {
                txtStatus.Text = "Valores inválidos";
                return;
            }

            var s = new TasasConversion { ClpToUsd = usd, ClpToBrl = brl };

            try
            {
                Ajustes.Save(s);
                txtStatus.Text = "Ajustes guardados";
            }
            catch
            {
                txtStatus.Text = "Error al guardar ajustes";
            }
        }

        private void btnRestablecerAjustes_Click(object? sender, RoutedEventArgs e)
        {
            // Valores por defecto (coinciden con AppSettings)
            txtClpToUsd.Text = "850";
            txtClpToBrl.Text = "160";
            txtStatus.Text = "Valores restablecidos (no guardados)";
        }

        private void btnVolverPagInicio_Click(object? sender, RoutedEventArgs e)
        {
            txtStatus.Text = string.Empty;
            this.IrPagina(new PagPrincipal());
        }
    }
}