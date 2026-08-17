using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using GeneradorVoucher;
using GeneradorVoucher_MP.Enums;
using GeneradorVoucher_MP.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GeneradorVoucher_MP.Views
{
    public partial class PagConfirmacion : UserControl
    {
        private double totalTour = 0;
        private readonly ObservableCollection<VoucherLookup> vouchersDisponibles = new();
        private readonly ObservableCollection<DatosActividad> actividadesMostradas = new();
        public ObservableCollection<VoucherLookup> VouchersDisponibles => vouchersDisponibles;
        public ObservableCollection<DatosActividad> DatosActividades => actividadesMostradas;
        public ObservableCollection<MediosPago> MediosPagoDisponibles { get; } = new ObservableCollection<MediosPago>
        {
            MediosPago.Efectivo,
            MediosPago.TarjetaCredito,
            MediosPago.TarjetaDebito,
            MediosPago.TransferenciaBancaria,
            MediosPago.Cheque,
            MediosPago.Otro
        };
        public ObservableCollection<MonedasPago> MonedasPagoDisponibles { get; } = new ObservableCollection<MonedasPago>
        {
            MonedasPago.CLP,
            MonedasPago.R,
            MonedasPago.USD
        };

        public record IdiomaOpcion(IdiomaVoucher Value, string Label);
        public ObservableCollection<IdiomaOpcion> IdiomaOpciones { get; } = new ObservableCollection<IdiomaOpcion>
    {
        new IdiomaOpcion(IdiomaVoucher.Espanol, "Español"),
        new IdiomaOpcion(IdiomaVoucher.Ingles, "Inglés"),
        new IdiomaOpcion(IdiomaVoucher.Portugues, "Portugués")
    };

        public PagConfirmacion()
        {
            InitializeComponent();
            textFechaAbonoConfirmacion.SelectedDate = DateTime.Now;
            SetInputEnabled(false);
            this.DataContext = this;
            dgvActividadesMostradas.ItemsSource = actividadesMostradas;
            cmbIdiomaVoucherConfirmacion.SelectedItem = IdiomaOpciones[0];
            this.Loaded += PagModificar_Loaded;

        }

        private async void PagModificar_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Ahora se awaitea la carga asíncrona y la carga sincrónica se ejecuta después
            await CargarVouchers();
        }

        private async Task CargarVouchers()
        {
            try
            {
                var listaVouchers = await Task.Run(() => App.RegistrosService.CargarVouchersDB());
                vouchersDisponibles.Clear();
                foreach (var voucher in listaVouchers)
                {
                    vouchersDisponibles.Add(voucher);
                }
                cmbVoucherCreados.ItemsSource = vouchersDisponibles;
            }
            catch (Exception ex)
            {
                this.MostrarAlerta("Error", $"No se pudieron cargar los vouchers: {ex.Message}", NotificationType.Error);
            }
        }

        private void cmbVoucherCreados_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (cmbVoucherCreados.SelectedItem is not VoucherLookup voucherSeleccionado) return;

            try
            {
                var (cliente, listaActividades) = App.RegistrosService.ObtenerDetalleVoucher(voucherSeleccionado.Id);

                textCantidadAdultos.Text = cliente.CantidadAdultosCliente.ToString();
                textCantidadNinos.Text = cliente.CantidadNinosCliente.ToString();

                actividadesMostradas.Clear();

                foreach (var actividad in listaActividades)
                {
                    actividadesMostradas.Add(actividad);
                }

                SetInputEnabled(true);

                MostrarTotales(cliente);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.MostrarAlerta("Error de Lectura", ex.Message, NotificationType.Error);
            }
        }

        private void MostrarTotales(DatosCliente cliente)
        {
            double totalAdultos = 0;
            double totalNinos = 0;
            double totalEntradas = 0;

            foreach (var actividad in actividadesMostradas)
            {
                totalAdultos += actividad.PrecioTourAdulto * cliente.CantidadAdultosCliente;
                totalNinos += actividad.PrecioTourNino * cliente.CantidadNinosCliente;
                totalEntradas += actividad.PrecioEntrada * (cliente.CantidadAdultosCliente + cliente.CantidadNinosCliente);
                
            }

            double descuento = (totalNinos + totalAdultos) * actividadesMostradas[0].DescuentoActividad / 100;
            totalTour = totalAdultos + totalNinos - descuento;
            double totalGeneral = totalAdultos + totalNinos + totalEntradas;

            textValorTotalAdultos.Text = totalAdultos.ToString("N0");
            textValorTotalNinos.Text = totalNinos.ToString("N0");
            textValorTotalEntrada.Text = totalEntradas.ToString("N0");
            textValorTotalTour.Text = totalTour.ToString("N0");
            textDescuento.Text = descuento.ToString("N0");
            textValorTotalVoucher.Text = (totalGeneral - descuento).ToString("N0");
            Debug.WriteLine($"Total Adultos: {totalAdultos}, Total Niños: {totalNinos}, Total Entradas: {totalEntradas}, Total General: {totalGeneral}, Descuento: {descuento}, Total Tour: {totalTour}");
        }

        private void SetInputEnabled(bool fieldState)
        {
            cmbMedioPagoConfirmacion.IsEnabled = fieldState;
            textMontoAbonoConfirmacion.IsEnabled = fieldState;
            textFechaAbonoConfirmacion.IsEnabled = fieldState;
            textFechaPagoSaldoConfirmacion.IsEnabled = fieldState;
        }

        private async void btnConfirmarVoucher_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (cmbVoucherCreados.SelectedItem is not VoucherLookup voucherSeleccionado)
            {
                this.MostrarAlerta("Error", "Debe seleccionar un voucher para confirmar.", NotificationType.Error);
                return;
            }

            if (cmbMedioPagoConfirmacion.SelectedItem is null
                || !double.TryParse(textMontoAbonoConfirmacion.Text, out double montoAbono)
                || montoAbono <= 0
                || textFechaAbonoConfirmacion.SelectedDate is null
            )
            {
                this.MostrarAlerta("Error", "Debe ingresar todos los datos correctamente.", NotificationType.Error);
                return;
            }

            if (montoAbono > totalTour)
            {
                this.MostrarAlerta("Error", "El monto del abono no puede ser mayor al valor total del voucher.", NotificationType.Error);
                return;
            }

            else if (textFechaPagoSaldoConfirmacion.SelectedDate is null &&
                montoAbono != totalTour)
            {
                this.MostrarAlerta("Error", "Debe ingresar la fecha de pago del saldo.", NotificationType.Error);
                return;
            }

            IdiomaVoucher idiomaSeleccionado;
            if (cmbIdiomaVoucherConfirmacion.SelectedValue is IdiomaOpcion idiomaOpcion)
            {
                idiomaSeleccionado = idiomaOpcion.Value;
            }
            else
            {
                idiomaSeleccionado = IdiomaVoucher.Espanol; // Valor por defecto
            }

            MonedasPago monedasSeleccionada;
            if (cmbMonedaSelect.SelectedItem is MonedasPago moneda)
            {
                monedasSeleccionada = moneda;
            }
            else
            {
                monedasSeleccionada = MonedasPago.CLP; // Valor por defecto
            }

            try
            {
                int idSeleccionado = voucherSeleccionado.Id;

                var (datosClientes, datosActividades) = App.RegistrosService.ObtenerDetalleVoucher(idSeleccionado);

                double saldoPendiente = totalTour - montoAbono;

                DatosConfirmacion datosConfirmacion = new DatosConfirmacion
                {
                    MedioPago = (MediosPago)cmbMedioPagoConfirmacion.SelectedItem,
                    AbonoConfirmacion = montoAbono,
                    SaldoPendiente = saldoPendiente,
                    FechaCreacionAbono = DateTime.Today,
                    // Usar la fecha del abono (control validado) en lugar de forzar el SelectedDate del saldo
                    FechaPagoAbono = textFechaAbonoConfirmacion.SelectedDate!.Value,
                    // Mantener nulo posible para la fecha del saldo pendiente
                    FechaPagoPendiente = textFechaPagoSaldoConfirmacion?.SelectedDate
                };

                App.RegistrosService.GuardarConfirmacion(datosConfirmacion, idSeleccionado) ;
                this.MostrarAlerta("Éxito", "Voucher confirmado correctamente.", NotificationType.Success);
                await GeneradorPdf.GenerarReportePDF("Confirmacion", idSeleccionado, datosClientes, datosActividades, idiomaSeleccionado, this, monedasSeleccionada, datosConfirmacion);
                CleanInputs();
                SetInputEnabled(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.MostrarAlerta("Error al confirmar", ex.Message, NotificationType.Error);
            }
        }

        private void CleanInputs()
        {
            cmbVoucherCreados.SelectedItem = null;
            cmbMedioPagoConfirmacion.SelectedItem = null;
            textMontoAbonoConfirmacion.Text = string.Empty;
            textFechaAbonoConfirmacion.SelectedDate = DateTime.Now;
            textFechaPagoSaldoConfirmacion.SelectedDate = null;

            textValorTotalAdultos.Text = string.Empty;
            textValorTotalNinos.Text = string.Empty;
            textValorTotalEntrada.Text =string.Empty;
            textValorTotalTour.Text = string.Empty;
            textDescuento.Text = string.Empty;
            textValorTotalVoucher.Text =string.Empty;
        }

        private void btnVolverPagPrincipal_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.IrPagina(new PagPrincipal());
        }
    }
}