using Avalonia;
using Avalonia.Controls;
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
        public static ManejoRegistros RegistrosService { get; private set; } = null!;
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

                if (string.IsNullOrEmpty(passwordValida))
                {
                    // Ventana temporal visible pero fuera de pantalla
                    var ventanaTemporal = new Window
                    {
                        Width = 0,
                        Height = 0,
                        MinWidth = 0,
                        MinHeight = 0,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        Position = new PixelPoint(-9999, -9999),
                        ShowInTaskbar = false,
                        Opacity = 0
                    };

                    desktop.MainWindow = ventanaTemporal;
                    base.OnFrameworkInitializationCompleted();  // base con ventana temporal activa y visible
                    ventanaTemporal.Show();

                    // Ahora ShowDialog funciona porque el owner es visible
                    var passWindow = new PasswordWindow();
                    bool? resultado = await passWindow.ShowDialog<bool?>(ventanaTemporal);

                    if (resultado == true)
                    {
                        passwordValida = passWindow.PassIngresada;
                        RegistrosService = new ManejoRegistros(passwordValida);

                        var mainWindow = new MainWindow
                        {
                            DataContext = new MainWindowViewModel()
                        };

                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                        ventanaTemporal.Close();
                    }
                    else
                    {
                        desktop.Shutdown();
                    }

                    return;
                }

                // Contraseña guardada válida → MainWindow directo
                RegistrosService = new ManejoRegistros(passwordValida);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };

                base.OnFrameworkInitializationCompleted();
            }
            else
            {
                base.OnFrameworkInitializationCompleted();
            }
        }
    }
}