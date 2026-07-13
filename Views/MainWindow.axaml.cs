using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using GeneradorVoucher_MP;
using System;
using System.Diagnostics;

namespace AvaloniaApplication1.Views
{
    public partial class MainWindow : Window
    {
        public WindowNotificationManager Notificador { get; private set; }
        private const double RatioAncho = 16.0;
        private const double RatioAlto = 9.0;   // cambia a 4/3, 16/10, etc.
        private bool _ajustando = false;          // evita recursión infinita

        public MainWindow()
        {
            InitializeComponent();

            this.Opened += (s, e) =>
            {
                var pantalla = this.Screens.ScreenFromWindow(this);
                if (pantalla != null)
                {
                    double escala = pantalla.Scaling;
                    this.Width = (pantalla.WorkingArea.Width * 0.7) / escala;
                    this.Height = this.Width * (RatioAlto / RatioAncho); // altura derivada del ancho
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            };

            this.Resized += OnVentanaResized;

            Notificador = new WindowNotificationManager(this)
            {
                Position = NotificationPosition.BottomRight,
                MaxItems = 3
            };

            var manager = this.FindControl<WindowNotificationManager>("NotificationManager");

            ContenedorPrincipal.Content = new PagInicio();
        }

        private void OnVentanaResized(object? sender, WindowResizedEventArgs e)
        {
            if (_ajustando) return;   // evita que el ajuste dispare otro Resized
            _ajustando = true;

            // El usuario arrastró → recalcular altura en base al ancho actual
            this.Height = this.Width * (RatioAlto / RatioAncho);

            _ajustando = false;
        }

        public void CambiarPagina(UserControl nuevaPagina)
        {
            ContenedorPrincipal.Content = nuevaPagina;
        }
    }
}