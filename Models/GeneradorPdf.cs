using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using GeneradorVoucher.Properties;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static GeneradorVoucher.PagActividad;
using Colors = QuestPDF.Helpers.Colors;

namespace GeneradorVoucher
{
    public static class GeneradorPdf
    {
        public static void GenerarReportePDF(int IdActividad, dynamic datosClientes, BindingList<datosActividad> actividades)
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

            // Calcular el total general sumando el valor base más los subtotales de las actividades
            double totalActividades = actividades.Sum(a => (a.precioEntrada + a.precioTour));
            double granTotal = totalActividades * Convert.ToInt32(datosClientes.cantidadCliente);

            // 2. Construcción del documento con la sintaxis fluida de QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configuración de la página (Márgenes y Tamaño A4)
                    page.Size(PageSizes.Letter);
                    page.Margin(0, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

                    // --- SECCIÓN 1: ENCABEZADO ---
                    page.Header().Background("#053559").Padding(15).Row(row =>
                    {
                        row.ConstantItem(120).Image(logo);
                        row.RelativeItem();

                        row.ConstantItem(220).AlignMiddle().Column(col =>
                        {
                            col.Item().Text("Calle Caracoles 66, San Pedro de Atacama, Chile")
                                .FontSize(10)
                                .FontColor(Colors.White)
                                .SemiBold();

                            col.Item().Text("reservas@caminandesagencia.com")
                                .FontSize(10)
                                .FontColor(Colors.White);

                            col.Item().Text("+56 9 51759544")
                                .FontSize(10)
                                .FontColor(Colors.White);
                        });
                    });

                    // --- SECCIÓN 2: CONTENIDO PRINCIPAL ---
                    page.Content().PaddingVertical(15).Column(column =>
                    {
                        // Bloque de Información del Cliente
                        column.Item().Background(Colors.White).Padding(15).Column(subColumn =>
                        {
                            subColumn.Item().Text("INFORMACIÓN DEL CLIENTE").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                            subColumn.Item().PaddingTop(4);

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"ID Voucher: {IdActividad}");
                                r.RelativeItem().Text($"Cliente: {datosClientes.nombreCliente}");
                                
                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Cant. Personas: {Convert.ToInt32(datosClientes.cantidadCliente)}");
                                r.RelativeItem().Text($"Fecha Creación: {DateTime.Now.ToString("yyyy-MM-dd HH:mm")}");
                            });

                            subColumn.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Fecha de Viaje: {datosClientes.fechaCliente}");
                                r.RelativeItem().Text($"Teléfono: {datosClientes.telefonoCliente}");
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
                                columns.RelativeColumn(2); // Fecha Actividad
                                columns.RelativeColumn(3); // Tipo Actividad
                                columns.RelativeColumn(2); // PickUp
                                columns.RelativeColumn(3); // Regreso
                                columns.RelativeColumn(3); // Servicio Incluido
                                columns.RelativeColumn(2); // Precio Entrada
                                columns.RelativeColumn(2); // Precio Tour
                            });

                            // Encabezados de la tabla
                            table.Header(header =>
                            {
                                header.Cell().Background("#053559").Padding(5).Text("Fecha").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("Actividad").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("PickUp").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("Regreso").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("Incluye").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("Precio Entrada").Bold().FontColor(Colors.White);
                                header.Cell().Background("#053559").Padding(5).Text("Precio Tour").Bold().FontColor(Colors.White);
                            });

                            // Filas de datos
                            foreach (var actividad in actividades)
                            {
                                table.Cell().Padding(5).Text($"{actividad.fechaActividad}");
                                table.Cell().Padding(5).Text($"{actividad.tipoActividad}");
                                table.Cell().Padding(5).Text($"{actividad.pickupActividad}");
                                table.Cell().Padding(5).Text($"{actividad.regresoActividad}");
                                table.Cell().Padding(5).Text($"{actividad.servicioActividad}");
                                table.Cell().Padding(5).AlignRight().Text($"${actividad.precioEntrada:N0}");
                                table.Cell().Padding(5).AlignRight().Text($"${actividad.precioTour:N0}");
                            }
                        });

                        // --- SECCIÓN 3: RESUMEN DE COSTOS ---
                        column.Item().PaddingRight(20).PaddingTop(1, Unit.Centimetre).AlignRight().Width(200).Column(resumen =>
                        {
                            resumen.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Valor Base Cliente:");
                                r.ConstantItem(80).AlignRight().Text($"${totalActividades:N0}");
                            });

                            resumen.Item().PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Darken1).Row(r =>
                            {
                                r.RelativeItem().Text("TOTAL GENERAL:").Bold().FontColor(Colors.Blue.Darken3);
                                r.ConstantItem(80).AlignRight().Text($"${granTotal:N0}").Bold().FontColor(Colors.Blue.Darken3);
                            });
                        });
                    });

                    // --- SECCIÓN 4: PIE DE PÁGINA ---
                    page.Footer().PaddingBottom(15).AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(rutaPdf); // Compila y escribe el archivo en disco

            // 3. Notificar al usuario y preguntar si desea abrirlo
            var resultado = MessageBox.Show($"¡PDF '{nombreArchivo}' generado con éxito!\n\n¿Desea abrir el archivo ahora?",
                                            "PDF Creado", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (resultado == DialogResult.Yes)
            {
                // Abre el lector de PDFs predeterminado del sistema operativo
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(rutaPdf) { UseShellExecute = true });
            }
        }

    }

}
