using GeneradorVoucher_MP.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Localization
{
    public static class LocalizadorPdf
    {
        private static readonly Dictionary<IdiomaVoucher, Dictionary<string, string>> Idiomas = new()
        {
            [IdiomaVoucher.Espanol] = new Dictionary<string, string>
            {
                ["TituloVoucher"] = "Cotización de servicios",
                ["TituloConfirmacion"] = "Voucher de Confirmación",
                ["TituloCliente"] = "INFORMACIÓN DEL CLIENTE",
                ["Responsable"] = "Responsable",
                ["Cliente"] = "Cliente",
                ["FechaCreacion"] = "Fecha creación",
                ["CantidadAdultos"] = "Cant. Adultos",
                ["CantidadNinos"] = "Cant. Niños",
                ["FechaViaje"] = "Fecha de viaje",
                ["Telefono"] = "Teléfono",
                ["TituloActividad"] = "DETALLE DE ACTIVIDADES CONTRATADAS",
                ["FechaActividad"] = "Fecha",
                ["TipoActividad"] = "Actividad",
                ["PickupActividad"] = "PickUp",
                ["RegresoActividad"] = "Regreso",
                ["IncluyeActividad"] = "Incluye",
                ["PrecioEntrada"] = "Precio entrada",
                ["PrecioTourAdulto"] = "Precio adulto",
                ["PrecioTourNino"] = "Precio niños",
                ["ValorEntradas"] = "Valor Entradas",
                ["ValorTour"] = "Valor Tour",
                ["TotalTour"] = "Precio de tour",
                ["TotalEntradas"] = "Precio de entradas",
                ["SubTotalGeneral"] = "TOTAL",
                ["Descuento"] = "Descuento en tour",
                ["TotalGeneral"] = "TOTAL CON DESCUENTO",
                ["PorPersona"] = "Por persona",
                ["Grupo"] = "Grupo",
                ["InformacionReservaTitulo"] = "Información importante sobre su reserva",
                ["InformacionReservaPago"] = "Pago del saldo: ",
                ["InformacionReservaPagoContenido"] = "Nuestro servicio opera de forma online y semipresencial. Por favor, coordine con nosotros su hora de atención para el día de su llegada a San Pedro de Atacama para liquidar el saldo pendiente.",
                ["InformacionReservaModificacion"] = "Modificaciones del itinerario",
                ["InformacionReservaModificacionContenido"] = "Modificaciones del itinerario: El programa puede sufrir cambios debido a factores climáticos, disponibilidad de los parques o fuerza mayor. El itinerario definitivo se le enviará 3 días antes del inicio de sus excursiones.",
                ["Agradecimiento"] = "Gracias por preferirnos, equipo de Caminandes",

                //Medio de pago
                ["MedioPagoEfectivo"] = "Efectivo",
                ["MedioPagoTarjetaCredito"] = "Tarjeta de crédito",
                ["MedioPagoTarjetaDebito"] = "Tarjeta de débito",
                ["MedioPagoTransferenciaBancaria"] = "Transferencia bancaria",
                ["MedioPagoCheque"] = "Cheque",
                ["MedioPagoOtro"] = "Otro",

                ["ValorTotal"] = "Valor tour:",
                ["MontoPagado"] = "Monto pagado:",
                ["SaldoPendiente"] = "Saldo pendiente:",
                ["FechaPago"] = "Fecha de pago:",
                ["MedioPago"] = "Medio de pago:",
                ["MedioPagoPendiente"] = "Medio de pago de saldo pendiente:",
                ["FechaPagoPendiente"] = "Fecha de pago saldo pendiente:",
                ["ValorEntradasConfirmacion"] = "Valor total de entradas (Deben ser pagadas en efectivo pesos chilenos): CLP $",
                ["ResumenPago"] = "RESUMEN DE PAGO"
            },

            [IdiomaVoucher.Ingles] = new Dictionary<string, string>
            {
                ["TituloVoucher"] = "Service Quotation",
                ["TituloConfirmacion"] = "Confirmation Voucher",
                ["TituloCliente"] = "CUSTOMER INFORMATION",
                ["Responsable"] = "Responsible",
                ["Cliente"] = "Client",
                ["FechaCreacion"] = "Creation Date",
                ["CantidadAdultos"] = "No. of Adults",
                ["CantidadNinos"] = "No. of Children",
                ["FechaViaje"] = "Travel Date",
                ["Telefono"] = "Phone",
                ["TituloActividad"] = "DETAIL OF BOOKED ACTIVITIES",
                ["FechaActividad"] = "Date",
                ["TipoActividad"] = "Activity",
                ["PickupActividad"] = "PickUp",
                ["RegresoActividad"] = "Return",
                ["IncluyeActividad"] = "Includes",
                ["PrecioEntrada"] = "Entrance fee",
                ["PrecioTourAdulto"] = "Adult Price",
                ["PrecioTourNino"] = "Children Price",
                ["ValorEntradas"] = "Total Entrance Fees",
                ["ValorTour"] = "Total Tour Price",
                ["TotalTour"] = "Total Tour",
                ["TotalEntradas"] = "Total Entrance Fees",
                ["SubTotalGeneral"] = "TOTAL",
                ["Descuento"] = "Discount",
                ["TotalGeneral"] = "TOTAL WITH DISCOUNT",
                ["PorPersona"] = "Per Person",
                ["Grupo"] = "Group",
                ["InformacionReservaTitulo"] = "Important information about your reservation",
                ["InformacionReservaPago"] = "Balance payment: ",
                ["InformacionReservaPagoContenido"] = "Our service operates both online and semi‑in‑person. Please coordinate with us to schedule your appointment on the day of your arrival in San Pedro de Atacama in order to settle the outstanding balance.",
                ["InformacionReservaModificacion"] = "Itinerary modifications",
                ["InformacionReservaModificacionContenido"] = "The program may undergo changes due to weather conditions, park availability, or force majeure. The final itinerary will be sent to you 3 days before the start of your excursions.",
                ["Agradecimiento"] = "We appreciate your preference, CaminAndes team",

                //Medio de pago
                ["MedioPagoEfectivo"] = "Cash",
                ["MedioPagoTarjetaCredito"] = "Credit Card",
                ["MedioPagoTarjetaDebito"] = "Debit Card",
                ["MedioPagoTransferenciaBancaria"] = "Bank Transfer",
                ["MedioPagoCheque"] = "Check",
                ["MedioPagoOtro"] = "Other",

                ["ValorTotal"] = "Total Value:",
                ["MontoPagado"] = "Amount Paid:",
                ["SaldoPendiente"] = "Pending Balance:",
                ["FechaPago"] = "Payment Date:",
                ["MedioPago"] = "Payment Method:",
                ["MedioPagoPendiente"] = "Payment Method for Pending Balance:",
                ["FechaPagoPendiente"] = "Payment Date for Pending Balance:",
                ["ValorEntradasConfirmacion"] = "Total value of tickets (Must be paid in cash in Chilean pesos): CLP $",
                ["ResumenPago"] = "PAYMENT SUMMARY"
            },

            [IdiomaVoucher.Portugues] = new Dictionary<string, string>
            {
                ["TituloVoucher"] = "Cotação de serviços",
                ["TituloConfirmacion"] = "Voucher de Confirmação",
                ["TituloCliente"] = "INFORMAÇÃO DO CLIENTE",
                ["Responsable"] = "Responsável",
                ["Cliente"] = "Cliente",
                ["FechaCreacion"] = "Data de criação",
                ["CantidadAdultos"] = "Qtd. Adultos",
                ["CantidadNinos"] = "Qtd. Crianças",
                ["FechaViaje"] = "Data da viagem",
                ["Telefono"] = "Telefone",
                ["TituloActividad"] = "DETALHE DAS ATIVIDADES CONTRATADAS",
                ["FechaActividad"] = "Data",
                ["TipoActividad"] = "Atividade",
                ["PickupActividad"] = "PickUp",
                ["RegresoActividad"] = "Retorno",
                ["IncluyeActividad"] = "Inclui",
                ["PrecioEntrada"] = "Preço da entrada",
                ["PrecioTourAdulto"] = "Preço adulto",
                ["PrecioTourNino"] = "Preço crianças",
                ["ValorEntradas"] = "Valor das Entradas",
                ["ValorTour"] = "Valor do Tour",
                ["TotalTour"] = "Total do Tour",
                ["TotalEntradas"] = "Total das Entradas",
                ["SubTotalGeneral"] = "TOTAL",
                ["Descuento"] = "Desconto no tour",
                ["TotalGeneral"] = "Total con desconto",
                ["PorPersona"] = "Por pessoa",
                ["Grupo"] = "Grupo",
                ["TotalGeneral"] = "TOTAL GERAL",
                ["InformacionReservaTitulo"] = "Informação importante sobre a sua reserva",
                ["InformacionReservaPago"] = "Pagamento do saldo: ",
                ["InformacionReservaPagoContenido"] = "Nosso serviço funciona de forma online e semipresencial. Por favor, coordene conosco o seu horário de atendimento para o dia da sua chegada a San Pedro de Atacama, a fim de liquidar o saldo pendente.",
                ["InformacionReservaModificacion"] = "Modificações do itinerário",
                ["InformacionReservaModificacionContenido"] = "O programa pode sofrer alterações devido a fatores climáticos, disponibilidade dos parques ou força maior. O itinerário definitivo será enviado a você 3 dias antes do início das suas excursões.",
                ["Agradecimiento"] = "Obrigado por nos escolher, equipe CaminAndes",

                //Medio de pago
                ["MedioPagoEfectivo"] = "Dinheiro",
                ["MedioPagoTarjetaCredito"] = "Cartão de crédito",
                ["MedioPagoTarjetaDebito"] = "Cartão de débito",
                ["MedioPagoTransferenciaBancaria"] = "Transferência bancária",
                ["MedioPagoCheque"] = "Cheque",
                ["MedioPagoOtro"] = "Outro",

                ["ValorTotal"] = "Valor do tour:",
                ["MontoPagado"] = "Valor pago:",
                ["SaldoPendiente"] = "Saldo pendente:",
                ["FechaPago"] = "Data de pagamento:",
                ["MedioPago"] = "Forma de pagamento:",
                ["MedioPagoPendiente"] = "Forma de pagamento do saldo pendente:",
                ["FechaPagoPendiente"] = "Data de pagamento do saldo pendente:",
                ["ValorEntradasConfirmacion"] = "Valor total dos ingressos (Devem ser pagos em dinheiro em pesos chilenos): CLP $",
                ["ResumenPago"] = "RESUMO DE PAGAMENTO"
            }
        };

        public static string ObtenerIdioma(IdiomaVoucher idioma, string clave)
        {
            if (Idiomas.TryGetValue(idioma, out var diccionario) && diccionario.TryGetValue(clave, out var texto))
            {
                return texto;
            }
            return clave; // Retorna la clave como fallback si no se encuentra
        }
    }
}
