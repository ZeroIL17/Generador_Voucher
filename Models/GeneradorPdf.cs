using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using GeneradorVoucher_MP;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Localization;
using GeneradorVoucher_MP.Models;
using QuestPDF.Companion;
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
        public static async Task GenerarReportePDF(string modo, int IdActividad, DatosCliente datosClientes, IEnumerable<DatosActividad> actividades, IdiomaVoucher idioma ,Visual visualOrigen, MonedasPago moneda, DatosConfirmacion? datosConfirmacion = null)
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
            double subtotal = totalEntrada + totalTour;
            double porcentajeDescuento = actividades.ElementAt(0).DescuentoActividad / 100;
            double descuento = porcentajeDescuento * totalTour;
            double tourConDescuento = totalTour - descuento;
            double total = subtotal - descuento;

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
                        column.Item().Background("#F1F5F8").PaddingHorizontal(15).PaddingVertical(5).Column(subColumn =>
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

                        column.Item().PaddingTop(0.5f, Unit.Centimetre);
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


                        if (modo == "Voucher")
                        {

                            // --- SECCIÓN 3: RESUMEN DE COSTOS ---
                            column.Item().PaddingTop(50).AlignCenter().PaddingHorizontal(10).Column(resumen =>
                            {
                                // Datos base
                                int pasajeros = totalPasajeros == 0 ? 1 : totalPasajeros; // evitar división por cero

                                //double perPersonTourCLP = pasajeros > 0 ? totalTour / pasajeros : 0;
                                //double perPersonEntradaCLP = pasajeros > 0 ? totalEntrada / pasajeros : 0;

                                // Conversión: tasa = cantidad CLP por 1 unidad de moneda destino.
                                // Ajusta las tasas según sea necesario o reemplaza por una fuente real de tipos de cambio.
                                double tasaClpPorUnidad = moneda switch
                                {
                                    MonedasPago.R => Ajustes.GetAppSettings().ClpToBrl, 
                                    MonedasPago.USD => Ajustes.GetAppSettings().ClpToUsd, 
                                    _ => 1.0 // CLP$ o desconocido -> 1:1 (no convertir)
                                };

                                bool mostrarConversion = moneda != MonedasPago.CLP;

                                //double perPersonTourConv = mostrarConversion ? Math.Round(perPersonTourCLP / tasaClpPorUnidad, 0) : 0;
                                double groupTourConv = mostrarConversion ? Math.Round(totalTour / tasaClpPorUnidad, 0) : 0;

                                //double perPersonEntradaConv = mostrarConversion ? Math.Round(perPersonEntradaCLP / tasaClpPorUnidad, 0) : 0;
                                double groupEntradaConv = mostrarConversion ? Math.Round(totalEntrada / tasaClpPorUnidad, 0) : 0;

                                double totalConv = mostrarConversion ? Math.Round(subtotal / tasaClpPorUnidad, 0) : 0;

                                // Descuentos (DescuentoActividad es porcentaje)
                                bool hayDescuento = porcentajeDescuento != 0.0;

                                double montoDescuentoCLP = Math.Round(totalTour * porcentajeDescuento, 0);
                                //double montoDescuentoPorPersonaCLP = pasajeros > 0 ? Math.Round(montoDescuentoCLP / pasajeros, 0) : 0;

                                double montoDescuentoConv = mostrarConversion ? Math.Round(montoDescuentoCLP / tasaClpPorUnidad, 0) : 0;
                                //double montoDescuentoPorPersonaConv = mostrarConversion ? Math.Round(montoDescuentoPorPersonaCLP / tasaClpPorUnidad, 0) : 0;

                                double subtotalConDescuentoCLP = total;
                                //double perPersonTotalConDescuentoCLP = pasajeros > 0 ? Math.Round(subtotalConDescuentoCLP / pasajeros, 0) : 0;

                                double subtotalConDescuentoConv = mostrarConversion ? Math.Round(subtotalConDescuentoCLP / tasaClpPorUnidad, 0) : 0;
                                //double perPersonTotalConDescuentoConv = mostrarConversion ? Math.Round(perPersonTotalConDescuentoCLP / tasaClpPorUnidad, 0) : 0;

                                string simboloConv = mostrarConversion ? moneda.ToString() : "";

                                // Construcción de la tabla centrada
                                resumen.Item().Element(ctx => ctx.Table(table =>
                                {
                                    // Definición de columnas: etiqueta, por persona CLP, grupo CLP, [por persona conv, grupo conv]
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(160);
                                        //columns.ConstantColumn(100);
                                        columns.ConstantColumn(100);
                                        if (mostrarConversion)
                                        {
                                            //columns.ConstantColumn(100);
                                            columns.ConstantColumn(100);
                                        }
                                    });

                                    // Estilo de celdas
                                    static IContainer CellStyle(IContainer c) => c.PaddingVertical(1.5f);

                                    // Encabezado
                                    table.Header(header =>
                                    {
                                        header.Cell().Background("#1F4E78").Padding(6).Text("SAN PEDRO DE ATACAMA").FontColor(Colors.White).SemiBold();
                                        //header.Cell().Background("#1F4E78").Padding(6).AlignCenter().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "PorPersona")} CLP").FontColor(Colors.White).SemiBold();
                                        header.Cell().Background("#1F4E78").Padding(6).AlignCenter().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "Grupo")} CLP").FontColor(Colors.White).SemiBold();
                                        if (mostrarConversion)
                                        {
                                            header.Cell().Background("#1F4E78").Padding(6).AlignCenter().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "PorPersona")} {simboloConv}").FontColor(Colors.White).SemiBold();
                                            //header.Cell().Background("#1F4E78").Padding(6).AlignCenter().Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "Grupo")} {simboloConv}").FontColor(Colors.White).SemiBold();
                                        }
                                    });

                                    // Fila: Precio de tour
                                    table.Cell().Element(CellStyle).Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "TotalTour")}");
                                    //table.Cell().Element(CellStyle).AlignRight().Text($"${perPersonTourCLP:N0}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${totalTour:N0}");
                                    if (mostrarConversion)
                                    {
                                        //table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv} {perPersonTourConv:N0}");
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv} {groupTourConv:N0}");
                                    }

                                    // Fila: Precio de entradas
                                    table.Cell().Element(CellStyle).Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "TotalEntradas")}");
                                    //table.Cell().Element(CellStyle).AlignRight().Text($"${perPersonEntradaCLP:N0}");
                                    table.Cell().Element(CellStyle).AlignRight().Text($"${totalEntrada:N0}");
                                    if (mostrarConversion)
                                    {
                                        //table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv} {perPersonEntradaConv:N0}");
                                        table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv} {groupEntradaConv:N0}");
                                    }

                                    // Fila: TOTAL
                                    table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "SubTotalGeneral")}").SemiBold();
                                    //double perPersonTotalCLP = pasajeros > 0 ? Math.Round(subtotal / pasajeros, 0) : 0;
                                    //table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"${perPersonTotalCLP:N0}").SemiBold();
                                    table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"${subtotal:N0}").SemiBold();
                                    if (mostrarConversion)
                                    {
                                        //table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"{simboloConv} {totalConv:N0}").SemiBold();
                                        table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"{simboloConv} {totalConv:N0}").SemiBold();
                                    }

                                    // Si hay descuento, mostrar filas de descuento y total con descuento
                                    if (hayDescuento)
                                    {
                                        // Espacio visual
                                        //table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text("");
                                        if (mostrarConversion)
                                        {
                                            //table.Cell().Element(CellStyle).Text("");
                                            table.Cell().Element(CellStyle).Text("");
                                        }

                                        // Fila: Descuento aplicado (porcentaje)
                                        table.Cell().Element(CellStyle).Text($"{porcentajeDescuento:P0} {LocalizadorPdf.ObtenerIdioma(idioma, "Descuento")}");
                                        //table.Cell().Element(CellStyle).AlignRight().Text($"$({montoDescuentoPorPersonaCLP:N0})");
                                        table.Cell().Element(CellStyle).AlignRight().Text($"$({montoDescuentoCLP:N0})");
                                        if (mostrarConversion)
                                        {
                                            //table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv}({montoDescuentoPorPersonaConv:N0})");
                                            table.Cell().Element(CellStyle).AlignRight().Text($"{simboloConv}({montoDescuentoConv:N0})");
                                        }

                                        // Fila: Total con descuento
                                        table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").Text($"{LocalizadorPdf.ObtenerIdioma(idioma, "TotalGeneral")}").SemiBold();
                                        //table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"${perPersonTotalConDescuentoCLP:N0}").SemiBold();
                                        table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"${subtotalConDescuentoCLP:N0}").SemiBold();
                                        if (mostrarConversion)
                                        {
                                            //table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"{simboloConv}{perPersonTotalConDescuentoConv:N0}").SemiBold();
                                            table.Cell().Element(CellStyle).BorderTop(0.5f).BorderColor("#6EC1E4").AlignRight().Text($"{simboloConv}{subtotalConDescuentoConv:N0}").SemiBold();
                                        }
                                    }
                                }));
                            });
                        }
                        else if (modo == "Confirmacion" && datosConfirmacion != null)
                        {
                            // ── SECCIÓN DE CONFIRMACIÓN Y RESUMEN ────────────────────────────────
                            double montoPagado = datosConfirmacion.AbonoConfirmacion;
                            double saldoPendiente = Math.Round(tourConDescuento - montoPagado, 2);

                            column.Item().PaddingTop(16).PaddingHorizontal(15).Column(conf =>
                            {
                                // ── TÍTULO DE SECCIÓN ─────────────────────────────────────────
                                conf.Item().PaddingBottom(8).Row(r =>
                                {
                                    r.RelativeItem()
                                        .Text(LocalizadorPdf.ObtenerIdioma(idioma, "ResumenPago"))
                                        .Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                                });

                                // ── TABLA DE RESUMEN ──────────────────────────────────────────
                                conf.Item().PaddingTop(8).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.ConstantColumn(170);   // etiqueta
                                        cols.ConstantColumn(100);   // valor
                                    });

                                    // Fila helper local
                                    void FilaResumen(string etiqueta, string valor,
                                                     bool esTotal = false, bool esSaldo = false)
                                    {
                                        string colorFondo = esTotal ? Colors.Transparent
                                                           : esSaldo ? "#FEF3C7"
                                                           : Colors.Transparent;
                                        string colorTexto = esTotal ? "#FFFFFF"
                                                           : esSaldo ? "#92400E"
                                                           : "#333333";
                                        string colorBorde = "#E0E8F0";

                                        table.Cell().Border(0.5f).BorderColor(colorBorde)
                                            .Background(colorFondo).Padding(5).AlignMiddle()
                                            .Text(etiqueta).FontSize(10)
                                            .SemiBold().FontColor(colorTexto);

                                        table.Cell().Border(0.5f).BorderColor(colorBorde)
                                            .Background(colorFondo).Padding(7).AlignRight().AlignMiddle()
                                            .Text(valor).FontSize(10)
                                            .SemiBold().FontColor(colorTexto);
                                    }

                                    FilaResumen(
                                        LocalizadorPdf.ObtenerIdioma(idioma, "ValorTotal"),
                                        $"${totalTour:N0}");

                                    FilaResumen(
                                        LocalizadorPdf.ObtenerIdioma(idioma, "Descuento"),
                                        $"- ${descuento:N0}");

                                    FilaResumen(
                                        LocalizadorPdf.ObtenerIdioma(idioma, "Subtotal"),
                                        $"${tourConDescuento:N0}");

                                    FilaResumen(
                                        LocalizadorPdf.ObtenerIdioma(idioma, "MontoPagado"),
                                        $"${montoPagado:N0}");

                                    // Medio de pago
                                    string medioPagoTexto = datosConfirmacion.MedioPago switch
                                    {
                                        MediosPago.Efectivo => LocalizadorPdf.ObtenerIdioma(idioma, "MedioPagoEfectivo"),
                                        MediosPago.TarjetaCredito => LocalizadorPdf.ObtenerIdioma(idioma, "MedioPagoTarjetaCredito"),
                                        MediosPago.TarjetaDebito => LocalizadorPdf.ObtenerIdioma(idioma, "MedioPagoTarjetaDebito"),
                                        MediosPago.TransferenciaBancaria => LocalizadorPdf.ObtenerIdioma(idioma, "MedioPagoTransferenciaBancaria"),
                                        MediosPago.Cheque => LocalizadorPdf.ObtenerIdioma(idioma, "Cheque"),
                                        _ => LocalizadorPdf.ObtenerIdioma(idioma, "MedioPagoOtro")
                                    };

                                    FilaResumen(
                                        LocalizadorPdf.ObtenerIdioma(idioma, "MedioPago"),
                                        medioPagoTexto);

                                    // Fila saldo pendiente — fondo ámbar si hay saldo, verde si está saldado
                                    bool saldado = saldoPendiente <= 0;
                                    table.Cell().Border(0.5f).BorderColor("#E0E8F0")
                                        .Background(Colors.Transparent).Padding(7).AlignMiddle()
                                        .Text(LocalizadorPdf.ObtenerIdioma(idioma, "SaldoPendiente"))
                                        .FontSize(10).SemiBold();

                                    table.Cell().Border(0.5f).BorderColor("#E0E8F0")
                                        .Background(Colors.Transparent).Padding(7).AlignRight().AlignMiddle()
                                        .Text($"${saldoPendiente:N0}")
                                        .FontSize(10).SemiBold();
                                });

                                // ── DATOS ADICIONALES ─────────────────────────────────────────
                                conf.Item().PaddingTop(10).Row(r =>
                                {
                                    // Fecha límite de pago
                                    r.RelativeItem().Padding(8).Column(c =>
                                    {
                                        c.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "FechaPagoPendiente"))
                                            .FontSize(9).SemiBold().FontColor("#666666");
                                        c.Item().PaddingTop(2)
                                            .Text(datosConfirmacion.FechaPagoPendiente?.ToString("dd/MM/yyyy") ?? "—")
                                            .FontSize(11).Bold().FontColor("#1F4E78");
                                    });

                                    r.ConstantItem(10); // separador

                                    // Valor entradas
                                    r.RelativeItem().Padding(8).Column(c =>
                                    {
                                        c.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "ValorEntradasConfirmacion"))
                                            .FontSize(9).SemiBold().FontColor("#666666");
                                        c.Item().PaddingTop(2)
                                            .Text($"CLP ${totalEntrada:N0}")
                                            .FontSize(11).Bold().FontColor("#1F4E78");
                                    });
                                });

                                // ── INFORMACIÓN DE RESERVA ────────────────────────────────────────
                                conf.Item().PaddingTop(14).PreventPageBreak().Column(info =>
                                {
                                    info.Item().BorderTop(1).BorderColor("#D0D8E4").PaddingTop(10);

                                    info.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "InformacionReservaTitulo"))
                                        .FontSize(11).Bold().FontColor("#1F4E78");

                                    // Bloque: pago de saldo
                                    info.Item().PaddingTop(8).Row(r =>
                                    {
                                        r.ConstantItem(3).Background("#1F4E78");
                                        r.ConstantItem(8);
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "InformacionReservaPago"))
                                                .FontSize(10).Bold().FontColor("#333333");
                                            c.Item().PaddingTop(3)
                                                .Text(LocalizadorPdf.ObtenerIdioma(idioma, "InformacionReservaPagoContenido"))
                                                .FontSize(9).FontColor("#555555").LineHeight(1.4f);
                                        });
                                    });

                                    // Bloque: modificaciones
                                    info.Item().PaddingTop(8).Row(r =>
                                    {
                                        r.ConstantItem(3).Background("#1F4E78");
                                        r.ConstantItem(8);
                                        r.RelativeItem().Column(c =>
                                        {
                                            c.Item().Text(LocalizadorPdf.ObtenerIdioma(idioma, "InformacionReservaModificacion"))
                                                .FontSize(10).Bold().FontColor("#333333");
                                            c.Item().PaddingTop(3)
                                                .Text(LocalizadorPdf.ObtenerIdioma(idioma, "InformacionReservaModificacionContenido"))
                                                .FontSize(9).FontColor("#555555").LineHeight(1.4f);
                                        });
                                    });
                                });
                            });
                        }
                        else
                        {
                            throw new ArgumentException("Modo de generación de PDF no reconocido o datos de confirmación faltantes.");
                        }
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
