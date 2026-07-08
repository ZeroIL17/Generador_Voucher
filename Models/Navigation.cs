using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using AvaloniaApplication1.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeneradorVoucher_MP.Models
{
    public static class Navegacion
    {
        public static void IrPagina(this UserControl paginaActual, UserControl nuevaPagina)
        {
            var topLevel = TopLevel.GetTopLevel(paginaActual);

            if (topLevel is MainWindow mainWindow)
            {
                mainWindow.CambiarPagina(nuevaPagina);
            }
        }

        public static void MostrarAlerta(this UserControl paginaActual, string titulo, string mensaje, NotificationType tipo, int timeSpan = 4)
        {
            var topLevel = TopLevel.GetTopLevel(paginaActual);

            if (topLevel is MainWindow mainWindow)
            {
                var notificacion = new Notification(
                    titulo,
                    mensaje,
                    tipo,
                    expiration: TimeSpan.FromSeconds(timeSpan)
                    );

                mainWindow.Notificador.Show(notificacion);
            }
        }

        public static void MarcarError(this TextBox textBox)
        {
            // Creamos un color rojo pastel/suave para que no sea molesto a la vista
            textBox.Background = new SolidColorBrush(Color.Parse("#FFEBEE"));
            textBox.BorderBrush = Brushes.Red;
            textBox.Focus(); // Pone el cursor directo en el control vacío

            // Truco: Cuando el usuario empiece a escribir, limpiamos el color rojo automáticamente
            textBox.TextInput += (s, e) => textBox.RestaurarEstilo();
        }

        // 2. Método para restaurar el diseño original del TextBox
        public static void RestaurarEstilo(this TextBox textBox)
        {
            textBox.ClearValue(TextBox.BackgroundProperty); // Borra el fondo rojo
            textBox.ClearValue(TextBox.BorderBrushProperty); // Borra el borde rojo
        }

        // 3. Sobrecarga del método para aplicarlo también a ComboBox
        public static void MarcarError(this ComboBox comboBox)
        {
            comboBox.Background = new SolidColorBrush(Color.Parse("#FFEBEE"));
            comboBox.BorderBrush = Brushes.Red;

            // Al cambiar la selección, se quita el color de error
            comboBox.SelectionChanged += (s, e) => comboBox.RestaurarEstilo();
        }

        public static void RestaurarEstilo(this ComboBox comboBox)
        {
            comboBox.ClearValue(ComboBox.BackgroundProperty);
            comboBox.ClearValue(ComboBox.BorderBrushProperty);
        }

        public static void MarcarError(this CalendarDatePicker calendarDatePicker)
        {
            calendarDatePicker.Background = new SolidColorBrush(Color.Parse("#FFEBEE"));
            calendarDatePicker.BorderBrush = Brushes.Red;

            // Al cambiar la selección, se quita el color de error
            calendarDatePicker.SelectedDateChanged += (s, e) => calendarDatePicker.RestaurarEstilo();
        }

        public static void RestaurarEstilo(this CalendarDatePicker calendarDatePicker)
        {
            calendarDatePicker.ClearValue(ComboBox.BackgroundProperty);
            calendarDatePicker.ClearValue(ComboBox.BorderBrushProperty);
        }

        public static void MarcarError(this NumericUpDown numericUpDown)
        {
            numericUpDown.Background = new SolidColorBrush(Color.Parse("#FFEBEE"));
            numericUpDown.BorderBrush = Brushes.Red;

            // Al cambiar la selección, se quita el color de error
            numericUpDown.TextInput += (s, e) => numericUpDown.RestaurarEstilo();
        }

        public static void RestaurarEstilo(this NumericUpDown numericUpDown)
        {
            numericUpDown.ClearValue(ComboBox.BackgroundProperty);
            numericUpDown.ClearValue(ComboBox.BorderBrushProperty);
        }
    }
}
