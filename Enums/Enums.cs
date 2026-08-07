using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Enums
{
    public enum IdiomaVoucher
    {
        Espanol,
        Ingles,
        Portugues
    }

    public enum MediosPago
    {
        Efectivo,
        TarjetaCredito,
        TarjetaDebito,
        TransferenciaBancaria,
        Cheque,
        Otro
    }

    public enum MonedasPago
    {
        CLP,
        R,
        USD
    }
}
