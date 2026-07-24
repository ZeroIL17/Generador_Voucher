using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GeneradorVoucher_MP.Models;

namespace GeneradorVoucher_MP.Views
{
    public partial class PasswordWindow : Window
    {
        public string PassIngresada { get; private set; } = string.Empty;
        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void btnIngresar_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string pass = txtPassword.Text ?? "";

            if (string.IsNullOrWhiteSpace(pass))
            {
                MostrarError("Por favor ingrese la contraseña.");
                return;
            }

            btnIngresar.IsEnabled = false;

            // Comprobamos la conexión directamente
            if (ManejoRegistros.ProbarConexion(pass))
            {
                // Si la conexión es exitosa, se guarda para siempre y se cierra la ventana
                PasswordStorage.GuardarPassword(pass);
                PassIngresada = pass;
                Close(true);
            }
            else
            {
                MostrarError("Contraseña incorrecta o sin conexión.");
                btnIngresar.IsEnabled = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.IsVisible = true;
        }
    }
}
