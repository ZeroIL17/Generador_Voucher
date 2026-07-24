using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using GeneradorVoucher_MP.Models;
using Microsoft.Extensions.Configuration;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace GeneradorVoucher_MP.Views
{
    public partial class App : Application
    {
        public static IConfiguration? Configuration { get; private set; }
        public static ManejoRegistros? RegistrosService { get; private set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string? passwordValida = PasswordStorage.ObtenerPassword();
                if (!string.IsNullOrEmpty(passwordValida))
                {
                    bool conexionOk = ManejoRegistros.ProbarConexion(passwordValida);
                    if (!conexionOk)
                    {
                        PasswordStorage.BorrarPassword();
                        passwordValida = null;
                    }
                }

                // Si no hay contraseña válida, mostrar PasswordWindow antes de crear MainWindow
                if (string.IsNullOrEmpty(passwordValida))
                {
                    var passWindow = new PasswordWindow();

                    // Mostrar la ventana sin owner para evitar la excepción
                    passWindow.Show();

                    // Esperar a que la ventana se cierre y obtener un posible resultado
                    var tcs = new System.Threading.Tasks.TaskCompletionSource<bool?>();

                    void ClosedHandler(object? s, System.EventArgs e)
                    {
                        passWindow.Closed -= ClosedHandler;
                        bool? res = null;
                        // Intentar leer una propiedad pública DialogResult si existe
                        var prop = passWindow.GetType().GetProperty("DialogResult");
                        if (prop != null)
                        {
                            res = prop.GetValue(passWindow) as bool?;
                        }
                        tcs.TrySetResult(res);
                    }

                    passWindow.Closed += ClosedHandler;
                    bool? resultado = await tcs.Task;

                    if (resultado == true)
                    {
                        passwordValida = passWindow.PassIngresada;
                    }
                    else
                    {
                        // Si el usuario cierra la ventana de contraseña, se apaga la app
                        desktop.Shutdown();
                        return;
                    }
                }

                // Crear y asignar la MainWindow una vez obtenida la contraseña
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = new MainWindowViewModel(),
                };

                base.OnFrameworkInitializationCompleted();

                RegistrosService = new ManejoRegistros(passwordValida);

                return; // Ya hemos llamado a base y terminado la inicialización para el caso desktop
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}