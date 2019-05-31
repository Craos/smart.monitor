namespace smart.automacao
{
    partial class FormAdicionarEquipamento
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.textBoxTitulo = new DevComponents.DotNetBar.Controls.TextBoxX();
            this.checkBoxMonitorRede = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.ipAddressInput = new DevComponents.Editors.IpAddressInput();
            this.gridcontroles = new DevComponents.DotNetBar.Controls.DataGridViewX();
            this.Titulo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Comando = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Acao = new DevComponents.DotNetBar.Controls.DataGridViewComboBoxExColumn();
            this.buttonSalvar = new DevComponents.DotNetBar.ButtonX();
            this.Nome = new DevComponents.DotNetBar.LabelX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            ((System.ComponentModel.ISupportInitialize)(this.ipAddressInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridcontroles)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxTitulo
            // 
            // 
            // 
            // 
            this.textBoxTitulo.Border.Class = "TextBoxBorder";
            this.textBoxTitulo.Border.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.textBoxTitulo.DisabledBackColor = System.Drawing.Color.White;
            this.textBoxTitulo.ForeColor = System.Drawing.Color.Black;
            this.textBoxTitulo.Location = new System.Drawing.Point(129, 23);
            this.textBoxTitulo.Margin = new System.Windows.Forms.Padding(0);
            this.textBoxTitulo.Name = "textBoxTitulo";
            this.textBoxTitulo.PreventEnterBeep = true;
            this.textBoxTitulo.Size = new System.Drawing.Size(174, 20);
            this.textBoxTitulo.TabIndex = 0;
            // 
            // checkBoxMonitorRede
            // 
            // 
            // 
            // 
            this.checkBoxMonitorRede.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxMonitorRede.Location = new System.Drawing.Point(320, 52);
            this.checkBoxMonitorRede.Margin = new System.Windows.Forms.Padding(0);
            this.checkBoxMonitorRede.Name = "checkBoxMonitorRede";
            this.checkBoxMonitorRede.Size = new System.Drawing.Size(246, 32);
            this.checkBoxMonitorRede.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxMonitorRede.TabIndex = 2;
            this.checkBoxMonitorRede.Text = "Verifica a situação do equipamento na rede";
            // 
            // ipAddressInput
            // 
            this.ipAddressInput.AutoOverwrite = true;
            // 
            // 
            // 
            this.ipAddressInput.BackgroundStyle.Class = "DateTimeInputBackground";
            this.ipAddressInput.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ipAddressInput.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.ipAddressInput.ButtonFreeText.Visible = true;
            this.ipAddressInput.Location = new System.Drawing.Point(129, 57);
            this.ipAddressInput.Margin = new System.Windows.Forms.Padding(0);
            this.ipAddressInput.Name = "ipAddressInput";
            this.ipAddressInput.Size = new System.Drawing.Size(174, 20);
            this.ipAddressInput.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ipAddressInput.TabIndex = 1;
            // 
            // gridcontroles
            // 
            this.gridcontroles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridcontroles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.gridcontroles.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridcontroles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridcontroles.ColumnHeadersHeight = 25;
            this.gridcontroles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Titulo,
            this.Comando,
            this.Acao});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(21)))), ((int)(((byte)(110)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridcontroles.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridcontroles.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.gridcontroles.EnableHeadersVisualStyles = false;
            this.gridcontroles.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(229)))));
            this.gridcontroles.Location = new System.Drawing.Point(0, 92);
            this.gridcontroles.Name = "gridcontroles";
            this.gridcontroles.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridcontroles.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gridcontroles.Size = new System.Drawing.Size(689, 165);
            this.gridcontroles.TabIndex = 7;
            // 
            // Titulo
            // 
            this.Titulo.HeaderText = "Título";
            this.Titulo.Name = "Titulo";
            // 
            // Comando
            // 
            this.Comando.HeaderText = "Comando";
            this.Comando.Name = "Comando";
            // 
            // Acao
            // 
            this.Acao.DisplayMember = "Text";
            this.Acao.DropDownHeight = 106;
            this.Acao.DropDownWidth = 121;
            this.Acao.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Acao.HeaderText = "Ação";
            this.Acao.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Acao.IntegralHeight = false;
            this.Acao.ItemHeight = 15;
            this.Acao.Name = "Acao";
            this.Acao.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // buttonSalvar
            // 
            this.buttonSalvar.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonSalvar.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonSalvar.Location = new System.Drawing.Point(611, 57);
            this.buttonSalvar.Name = "buttonSalvar";
            this.buttonSalvar.Size = new System.Drawing.Size(75, 23);
            this.buttonSalvar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonSalvar.TabIndex = 8;
            this.buttonSalvar.Text = "Salvar";
            // 
            // Nome
            // 
            // 
            // 
            // 
            this.Nome.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.Nome.Location = new System.Drawing.Point(8, 20);
            this.Nome.Name = "Nome";
            this.Nome.Size = new System.Drawing.Size(118, 27);
            this.Nome.TabIndex = 9;
            this.Nome.Text = "Nome do equipamento:";
            this.Nome.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // labelX1
            // 
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Location = new System.Drawing.Point(8, 55);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(118, 27);
            this.labelX1.TabIndex = 10;
            this.labelX1.Text = "Endereço de rede:";
            this.labelX1.TextAlignment = System.Drawing.StringAlignment.Far;
            // 
            // FormAdicionarEquipamento
            // 
            this.ClientSize = new System.Drawing.Size(689, 257);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.Nome);
            this.Controls.Add(this.buttonSalvar);
            this.Controls.Add(this.ipAddressInput);
            this.Controls.Add(this.gridcontroles);
            this.Controls.Add(this.textBoxTitulo);
            this.Controls.Add(this.checkBoxMonitorRede);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAdicionarEquipamento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Adicionar equipamento";
            ((System.ComponentModel.ISupportInitialize)(this.ipAddressInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridcontroles)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevComponents.DotNetBar.Controls.TextBoxX textBoxTitulo;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxMonitorRede;
        private DevComponents.Editors.IpAddressInput ipAddressInput;
        private DevComponents.DotNetBar.Controls.DataGridViewX gridcontroles;
        private DevComponents.DotNetBar.ButtonX buttonSalvar;
        private System.Windows.Forms.DataGridViewTextBoxColumn Titulo;
        private System.Windows.Forms.DataGridViewTextBoxColumn Comando;
        private DevComponents.DotNetBar.Controls.DataGridViewComboBoxExColumn Acao;
        private DevComponents.DotNetBar.LabelX Nome;
        private DevComponents.DotNetBar.LabelX labelX1;
    }
}