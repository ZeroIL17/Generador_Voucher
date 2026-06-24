namespace GeneradorVoucher
{
    partial class PagActividad
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            dgvActividades = new DataGridView();
            textActividadFecha = new DateTimePicker();
            textActividadIncluye = new TextBox();
            textActividadPrecioEntrada = new TextBox();
            textActividadPrecioTour = new TextBox();
            btnActividadAgregarDatos = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label9 = new Label();
            label10 = new Label();
            label12 = new Label();
            btnAgregarExcel = new Button();
            cmbTour = new ComboBox();
            textPickUpActividad = new TextBox();
            textRegresoActividad = new TextBox();
            btnVolverPagActividades = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvActividades).BeginInit();
            SuspendLayout();
            // 
            // dgvActividades
            // 
            dgvActividades.AllowUserToOrderColumns = true;
            dgvActividades.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvActividades.Location = new Point(414, 70);
            dgvActividades.Name = "dgvActividades";
            dgvActividades.Size = new Size(422, 313);
            dgvActividades.TabIndex = 0;
            // 
            // textActividadFecha
            // 
            textActividadFecha.CalendarMonthBackground = Color.WhiteSmoke;
            textActividadFecha.Font = new Font("Nirmala UI", 9F);
            textActividadFecha.Location = new Point(180, 70);
            textActividadFecha.Name = "textActividadFecha";
            textActividadFecha.Size = new Size(219, 23);
            textActividadFecha.TabIndex = 1;
            // 
            // textActividadIncluye
            // 
            textActividadIncluye.BackColor = Color.WhiteSmoke;
            textActividadIncluye.Font = new Font("Nirmala UI", 9F);
            textActividadIncluye.Location = new Point(180, 262);
            textActividadIncluye.Multiline = true;
            textActividadIncluye.Name = "textActividadIncluye";
            textActividadIncluye.Size = new Size(219, 48);
            textActividadIncluye.TabIndex = 7;
            // 
            // textActividadPrecioEntrada
            // 
            textActividadPrecioEntrada.BackColor = Color.WhiteSmoke;
            textActividadPrecioEntrada.Font = new Font("Nirmala UI", 9F);
            textActividadPrecioEntrada.Location = new Point(180, 331);
            textActividadPrecioEntrada.Name = "textActividadPrecioEntrada";
            textActividadPrecioEntrada.Size = new Size(219, 23);
            textActividadPrecioEntrada.TabIndex = 8;
            // 
            // textActividadPrecioTour
            // 
            textActividadPrecioTour.BackColor = Color.WhiteSmoke;
            textActividadPrecioTour.Font = new Font("Nirmala UI", 9F);
            textActividadPrecioTour.Location = new Point(180, 379);
            textActividadPrecioTour.Name = "textActividadPrecioTour";
            textActividadPrecioTour.Size = new Size(219, 23);
            textActividadPrecioTour.TabIndex = 9;
            // 
            // btnActividadAgregarDatos
            // 
            btnActividadAgregarDatos.BackColor = Color.FromArgb(65, 81, 99);
            btnActividadAgregarDatos.Font = new Font("Nirmala UI", 12F, FontStyle.Bold);
            btnActividadAgregarDatos.ForeColor = SystemColors.Control;
            btnActividadAgregarDatos.Image = Properties.Resources.boton_add;
            btnActividadAgregarDatos.Location = new Point(54, 422);
            btnActividadAgregarDatos.Name = "btnActividadAgregarDatos";
            btnActividadAgregarDatos.Padding = new Padding(20, 0, 0, 0);
            btnActividadAgregarDatos.Size = new Size(221, 47);
            btnActividadAgregarDatos.TabIndex = 10;
            btnActividadAgregarDatos.Text = "Agregar actividad";
            btnActividadAgregarDatos.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnActividadAgregarDatos.UseVisualStyleBackColor = false;
            btnActividadAgregarDatos.Click += btnActividadAgregarDatos_Click;
            // 
            // label1
            // 
            label1.BackColor = Color.LightGray;
            label1.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(23, 70);
            label1.Name = "label1";
            label1.Size = new Size(156, 23);
            label1.TabIndex = 11;
            label1.Text = "Fecha";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.BackColor = Color.LightGray;
            label2.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(23, 118);
            label2.Name = "label2";
            label2.Size = new Size(156, 23);
            label2.TabIndex = 12;
            label2.Text = "Tour / Servicio";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.BackColor = Color.LightGray;
            label3.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(23, 166);
            label3.Name = "label3";
            label3.Size = new Size(156, 23);
            label3.TabIndex = 13;
            label3.Text = "Horario Pick Up";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            label4.BackColor = Color.LightGray;
            label4.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(23, 214);
            label4.Name = "label4";
            label4.Size = new Size(156, 23);
            label4.TabIndex = 14;
            label4.Text = "Horario Regreso";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            label9.BackColor = Color.LightGray;
            label9.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label9.ForeColor = Color.Black;
            label9.Location = new Point(23, 262);
            label9.Name = "label9";
            label9.Size = new Size(156, 47);
            label9.TabIndex = 19;
            label9.Text = "Incluye";
            label9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            label10.BackColor = Color.LightGray;
            label10.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label10.ForeColor = Color.Black;
            label10.Location = new Point(23, 331);
            label10.Name = "label10";
            label10.Size = new Size(156, 23);
            label10.TabIndex = 20;
            label10.Text = "Entrada P/P ($ CLP)";
            label10.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            label12.BackColor = Color.LightGray;
            label12.Font = new Font("Nirmala UI", 9.75F, FontStyle.Bold);
            label12.ForeColor = Color.Black;
            label12.Location = new Point(23, 379);
            label12.Name = "label12";
            label12.Size = new Size(156, 23);
            label12.TabIndex = 22;
            label12.Text = "Precio Tour P/P ($ CLP)";
            label12.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnAgregarExcel
            // 
            btnAgregarExcel.BackColor = Color.FromArgb(65, 81, 99);
            btnAgregarExcel.Font = new Font("Nirmala UI", 12F, FontStyle.Bold);
            btnAgregarExcel.ForeColor = SystemColors.Control;
            btnAgregarExcel.Image = Properties.Resources.boton_crear;
            btnAgregarExcel.Location = new Point(316, 422);
            btnAgregarExcel.Name = "btnAgregarExcel";
            btnAgregarExcel.Padding = new Padding(20, 0, 0, 0);
            btnAgregarExcel.Size = new Size(221, 47);
            btnAgregarExcel.TabIndex = 23;
            btnAgregarExcel.Text = "Crear Voucher";
            btnAgregarExcel.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAgregarExcel.UseVisualStyleBackColor = false;
            btnAgregarExcel.Click += btnAgregarExcel_Click;
            // 
            // cmbTour
            // 
            cmbTour.AllowDrop = true;
            cmbTour.AutoCompleteMode = AutoCompleteMode.Suggest;
            cmbTour.Font = new Font("Nirmala UI", 9F);
            cmbTour.FormattingEnabled = true;
            cmbTour.Location = new Point(180, 118);
            cmbTour.Name = "cmbTour";
            cmbTour.Size = new Size(219, 23);
            cmbTour.TabIndex = 2;
            cmbTour.SelectedIndexChanged += cmbTour_SelectedIndexChanged;
            // 
            // textPickUpActividad
            // 
            textPickUpActividad.Font = new Font("Nirmala UI", 9F);
            textPickUpActividad.Location = new Point(180, 166);
            textPickUpActividad.Name = "textPickUpActividad";
            textPickUpActividad.Size = new Size(219, 23);
            textPickUpActividad.TabIndex = 24;
            // 
            // textRegresoActividad
            // 
            textRegresoActividad.Font = new Font("Nirmala UI", 9F);
            textRegresoActividad.Location = new Point(180, 214);
            textRegresoActividad.Name = "textRegresoActividad";
            textRegresoActividad.Size = new Size(219, 23);
            textRegresoActividad.TabIndex = 25;
            // 
            // btnVolverPagActividades
            // 
            btnVolverPagActividades.BackColor = Color.FromArgb(65, 81, 99);
            btnVolverPagActividades.Font = new Font("Nirmala UI", 12F, FontStyle.Bold);
            btnVolverPagActividades.ForeColor = SystemColors.Control;
            btnVolverPagActividades.Image = Properties.Resources.boton_volver;
            btnVolverPagActividades.Location = new Point(578, 422);
            btnVolverPagActividades.Name = "btnVolverPagActividades";
            btnVolverPagActividades.Padding = new Padding(20, 0, 0, 0);
            btnVolverPagActividades.Size = new Size(221, 47);
            btnVolverPagActividades.TabIndex = 26;
            btnVolverPagActividades.Text = "Volver";
            btnVolverPagActividades.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnVolverPagActividades.UseVisualStyleBackColor = false;
            btnVolverPagActividades.Click += btnVolverPagActividades_Click;
            // 
            // PagActividad
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackgroundImage = Properties.Resources.fondo_registro_actividades;
            BackgroundImageLayout = ImageLayout.Stretch;
            Controls.Add(btnVolverPagActividades);
            Controls.Add(textRegresoActividad);
            Controls.Add(textPickUpActividad);
            Controls.Add(cmbTour);
            Controls.Add(btnAgregarExcel);
            Controls.Add(label12);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnActividadAgregarDatos);
            Controls.Add(textActividadPrecioTour);
            Controls.Add(textActividadPrecioEntrada);
            Controls.Add(textActividadIncluye);
            Controls.Add(textActividadFecha);
            Controls.Add(dgvActividades);
            Name = "PagActividad";
            Size = new Size(852, 489);
            Load += PagActividad_Load;
            ((System.ComponentModel.ISupportInitialize)dgvActividades).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvActividades;
        private DateTimePicker textActividadFecha;
        private TextBox textActividadIncluye;
        private TextBox textActividadPrecioEntrada;
        private TextBox textActividadPrecioTour;
        private Button btnActividadAgregarDatos;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label9;
        private Label label10;
        private Label label12;
        private Button btnAgregarExcel;
        private ComboBox cmbTour;
        private TextBox textPickUpActividad;
        private TextBox textRegresoActividad;
        private Button btnVolverPagActividades;
    }
}
