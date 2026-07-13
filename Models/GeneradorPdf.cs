using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using GeneradorVoucher_MP;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Localization;
using GeneradorVoucher_MP.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Colors = QuestPDF.Helpers.Colors;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace GeneradorVoucher
{
    public static class GeneradorPdf
    {
        public static async Task GenerarReportePDF(int IdActividad, DatosCliente datosClientes, IEnumerable<DatosActividad> actividades, IdiomaVoucher idioma ,Visual visualOrigen)
        {
            // 1. Definir la ruta de guardado del PDF (Misma carpeta del programa)
            string nombreArchivo = $"Itinerario_Cliente_{IdActividad}.pdf";

            // crear carpeta para guardar los PDF si no existe
            string carpetaPdf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PDFs");
            if (!Directory.Exists(carpetaPdf))
            {
                Directory.CreateDirectory(carpetaPdf);
            }

            string rutaPdf = Path.Combine(carpetaPdf, nombreArchivo);
            byte[] logo = RecursosPdf.Logo;
            byte[] fondoPie = RecursosPdf.ImagenPie;

            // Calcular el total general sumando el valor base más los subtotales de las actividades
            int totalPasajeros = (int)datosClientes.CantidadAdultosCliente + (int)datosClientes.CantidadNinosCliente;
            double totalEntrada = actividades.Sum(a => a.PrecioEntrada * totalPasajeros);
            double totalTour = actividades.Sum(a => (int)datosClientes.CantidadAdultosCliente * a.PrecioTourAdulto + (int)datosClientes.CantidadNinosCliente * a.PrecioTourNino);
            double granTotal = totalEntrada + totalTour;

            // 2. Construcción del documento con la sintaxis fluida de QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de la página (Márgenes y Tamaño A4)
                    page.Size(PageSizes.Letter);
                    page.Margin(0, Unit.Centimetre);
                    page.PageColor("#F1F5F8");
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    // --- SECCIÓN 1: ENCABEZADO ---
                    page.Header().BackgroundLinearGradient(90, ["#053559", "#1F4E78"]).Padding(15).Row(row =>
                    {
                        row.ConstantItem(120).Image(logo);
                        row.RelativeItem();

                        row.ConstantItem(250).AlignMiddle().Column(col =>
                        {
                            col.Item().Text("Calle Caracoles 66, San Pedro de Atacama, Chile")
                                .FontSize(11)
                                .FontColor(Colors.White)
                                .SemiBold();

                            col.Item().Text("reservas@caminandesagencia.com")
                                .FontSize(11)
                                .FontColor(Colors.White);

                            col.Item().Text("+56 9 51759544")
                                .FontSize(11)
                                .FontColor(Colors.White);
                        });
                    });

                    // --- SECCIÓN 2: CONTENIDO PRINCIPAL ---
                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        column.Item().PaddingHorizontal(15).PaddingBottom(5).Column(titulo =>
                        {
                            titulo.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "TituloVoucher"))
                                .FontSize(20)
                                .Bold()
                                .FontColor("#1F4E78");

                            // Línea decorativa debajo del título
                            titulo.Item().PaddingTop(6).BorderBottom(1).BorderColor("#1F4E78").Width(200);
                        });

                        // Bloque de Información del Cliente
                        column.Item().Background("#F0F8FB").PaddingHorizontal(15).PaddingVertical(5).Column(subColumn =>
                        {
                            subColumn.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "TituloCliente")).Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                            subColumn.Item().PaddingTop(4);

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "Responsable")}: {SesionSistema.UsuarioActual}");
                                r.RelativeItem().Text($"ID Voucher: {IdActividad}");

                            });

                            subColumn.Item().Row(r =>
                            {

                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "Cliente")}: {datosClientes.NombreCliente}");
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "FechaCreacion")}: {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");

                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "CantidadAdultos")}: {datosClientes.CantidadAdultosCliente}");
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "FechaViaje")}: {datosClientes.FechaInicioCliente:dd/MM/yyyy}");
                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "CantidadNinos")}: {datosClientes.CantidadNinosCliente}");
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "Telefono")}: {datosClientes.TelefonoCliente}");
                            });
                        });

                        column.Item().PaddingTop(1, Unit.Centimetre);
                        column.Item().PaddingLeft(15).Text(LocalizadorPdf.ObtenerIdioma(idioma, "TituloActividad")).Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        column.Item().PaddingTop(5);

                        // TABLA 
                        column.Item().PaddingHorizontal(10).Table(table =>
                        {
                            // Definición de las columnas (Ancho relativo)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.2f); // Fecha Actividad
                                columns.RelativeColumn(3); // Tipo Actividad
                                columns.RelativeColumn(2.5f); // PickUp
                                columns.RelativeColumn(2); // Regreso
                                columns.RelativeColumn(3); // Servicio Incluido
                                columns.RelativeColumn(2); // Precio Entrada
                                columns.RelativeColumn(2); // Precio Tour Adulto
                                columns.RelativeColumn(2); // Precio Tour Niño
                            });

                            // Encabezados de la tabla
                            table.Header(header =>
                            {
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "FechaActividad")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "TipoActividad")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "PickupActividad")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "RegresoActividad")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "IncluyeActividad")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "PrecioEntrada")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "PrecioTourAdulto")).Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text(LocalizadorPdf.ObtenerIdioma(idioma, "PrecioTourNino")).Bold().FontColor(Colors.White);
                            });

                            // Filas de datos
                            foreach (var actividad in actividades)
                            {
                                // Evitar acceso directo a .Value de DateTime? y proteger referencias que pueden ser null
                                string fechaText = actividad.FechaActividad?.ToString("dd/MM/yyyy") ?? "";

                                table.Cell().Element(CellStyle).Text(fechaText);
                                table.Cell().Element(CellStyle).Text(actividad.TipoActividad ?? "");
                                table.Cell().Element(CellStyle).Text(actividad.PickupActividad ?? "");
                                table.Cell().Element(CellStyle).Text(actividad.RegresoActividad ?? "");
                                table.Cell().Element(CellStyle).Text(actividad.IncluyeActividad ?? "");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.PrecioEntrada:N0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.PrecioTourAdulto:N0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.PrecioTourNino:N0}");
                            }

                            static IContainer CellStyle(IContainer container)
                            => container.Border(0.5f).BorderColor("#1F4E78").Padding(5);

                        });



                        // --- SECCIÓN 3: RESUMEN DE COSTOS ---
                        column.Item().PaddingRight(20).PaddingTop(1, Unit.Centimetre).AlignRight().Width(200).Column(resumen =>
                        {
                            resumen.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "ValorEntradas")}:");
                                r.ConstantItem(80).AlignRight().Text($"${totalEntrada:N0}");
                            });
                            resumen.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "ValorTour")}:");
                                r.ConstantItem(80).AlignRight().Text($"${totalTour:N0}");
                            });

                            resumen.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                            {
                                r.RelativeItem().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "TotalGeneral")}:").Bold().FontColor(Colors.Blue.Darken3);
                                r.ConstantItem(80).AlignRight().Text($"${granTotal:N0}").Bold().FontColor(Colors.Blue.Darken3);
                            });
                        });
                    });

                    // --- SECCIÓN 4: PIE DE PÁGINA ---

                    page.Footer().Background("#F1F5F8").Column(col =>
                    {
                        // Numero de pagina
                        col.Item().AlignCenter().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });

                        col.Item().Height(90).Image(fondoPie).FitUnproportionally();
                    });
                });
            }).GeneratePdf(rutaPdf); // Compila y escribe el archivo en disco

            try
            {
                var topLevel = TopLevel.GetTopLevel(visualOrigen);
                if (topLevel is Window ventanaMaestra)
                {
                    var dialogo = new PdfDialog(rutaPdf);
                    bool deseaAbrir = await dialogo.ShowDialog<bool>(ventanaMaestra);

                    if (deseaAbrir && File.Exists(rutaPdf))
                    {
                        OpenFileCrossPlatform(rutaPdf);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones por si el lector de PDF del sistema falla
                Console.WriteLine($"Error al intentar abrir el archivo: {ex.Message}");
            }
        }

        private static void OpenFileCrossPlatform(string path)
        {
            try
            {
                // Intento simple que funciona en la mayoría de plataformas con .NET moderno
                var psi = new ProcessStartInfo(path) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch
            {
                // Fallbacks explícitos
                if (OperatingSystem.IsMacOS())
                {
                    Process.Start("open", path);
                }
                else if (OperatingSystem.IsLinux())
                {
                    Process.Start("xdg-open", path);
                }
                else if (OperatingSystem.IsWindows())
                {
                    var psi = new ProcessStartInfo(path) { UseShellExecute = true };
                    Process.Start(psi);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
