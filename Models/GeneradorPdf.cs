using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using GeneradorVoucher_MP;
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
        public static async Task GenerarReportePDF(int IdActividad, DatosCliente datosClientes, IEnumerable<DatosActividad> actividades, Visual visualOrigen)
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
            string logo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo_caminandes.png");
            string fondoPie = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "imagen_pie_pdf.png");

            // Calcular el total general sumando el valor base más los subtotales de las actividades
            int totalPasajeros = (int)datosClientes.CantidadAdultosCliente + (int)datosClientes.CantidadNinosCliente;
            double totalEntrada = actividades.Sum(a => a.precioEntrada * totalPasajeros);
            double totalTour = actividades.Sum(a => (int)datosClientes.CantidadAdultosCliente * a.precioTourAdulto + (int)datosClientes.CantidadNinosCliente * a.precioTourNino);
            double granTotal = totalEntrada + totalTour;

            // 2. Construcción del documento con la sintaxis fluida de QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de la página (Márgenes y Tamaño A4)
                    page.Size(PageSizes.Letter);
                    page.Margin(0, Unit.Centimetre);
                    page.PageColor("#F0F8FB");
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
                        // Bloque de Información del Cliente
                        column.Item().Background("#F0F8FB").Padding(15).Column(subColumn =>
                        {
                            subColumn.Item().Text("INFORMACIÓN DEL CLIENTE").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                            subColumn.Item().PaddingTop(4);

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Responsable: {SesionSistema.UsuarioActual}");
                                r.RelativeItem().Text($"ID Voucher: {IdActividad}");

                            });

                            subColumn.Item().Row(r =>
                            {

                                r.RelativeItem().Text($"Cliente: {datosClientes.NombreCliente}");
                                r.RelativeItem().Text($"Fecha Creación: {DateTime.Now.ToString("dd-MM-yyyy HH:mm")}");

                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Cant. Adultos: {datosClientes.CantidadAdultosCliente}");
                                r.RelativeItem().Text($"Fecha de Viaje: {datosClientes.FechaInicioCliente:dd/MM/yyyy}");
                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Cant. Niños: {datosClientes.CantidadNinosCliente}");
                                r.RelativeItem().Text($"Teléfono: {datosClientes.TelefonoCliente}");
                            });
                        });

                        column.Item().PaddingTop(1, Unit.Centimetre);
                        column.Item().PaddingLeft(15).Text("DETALLE DE ACTIVIDADES CONTRATADAS").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
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
                                header.Cell().Background("#1F4E78").Padding(5).Text("Fecha").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Actividad").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("PickUp").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Regreso").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Incluye").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Precio Entrada").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Precio Adulto").Bold().FontColor(Colors.White);
                                header.Cell().Background("#1F4E78").Padding(5).Text("Precio Niño").Bold().FontColor(Colors.White);
                            });

                            // Filas de datos
                            foreach (var actividad in actividades)
                            {
                                table.Cell().Element(CellStyle).Text(actividad.fechaActividad.Value.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(actividad.tipoActividad);
                                table.Cell().Element(CellStyle).Text(actividad.pickupActividad);
                                table.Cell().Element(CellStyle).Text(actividad.regresoActividad);
                                table.Cell().Element(CellStyle).Text(actividad.incluyeActividad);
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.precioEntrada:N0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.precioTourAdulto:N0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${actividad.precioTourNino:N0}");
                            }

                            static IContainer CellStyle(IContainer container)
                            => container.Border(0.5f).BorderColor("#1F4E78").Padding(5);

                        });



                        // --- SECCIÓN 3: RESUMEN DE COSTOS ---
                        column.Item().PaddingRight(20).PaddingTop(1, Unit.Centimetre).AlignRight().Width(200).Column(resumen =>
                        {
                            resumen.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Valor Entradas:");
                                r.ConstantItem(80).AlignRight().Text($"${totalEntrada:N0}");
                            });
                            resumen.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Valor Tour:");
                                r.ConstantItem(80).AlignRight().Text($"${totalTour:N0}");
                            });

                            resumen.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL GENERAL:").Bold().FontColor(Colors.Blue.Darken3);
                                r.ConstantItem(80).AlignRight().Text($"${granTotal:N0}").Bold().FontColor(Colors.Blue.Darken3);
                            });
                        });
                    });

                    // --- SECCIÓN 4: PIE DE PÁGINA ---

                    page.Footer().Background("#F0F8FB").Column(col =>
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
                        await Task.Run(() =>
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(rutaPdf)
                            {
                                UseShellExecute = true
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones por si el lector de PDF del sistema falla
                Console.WriteLine($"Error al intentar abrir el archivo: {ex.Message}");
            }
        }
    }
}
