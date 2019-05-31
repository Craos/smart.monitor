namespace smart.automacao
{
    partial class FormEquipamentos
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEquipamentos));
            this.grid = new DevComponents.DotNetBar.Controls.DataGridViewX();
            this.Titulo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MonitoRede = new DevComponents.DotNetBar.Controls.DataGridViewCheckBoxXColumn();
            this.Endereco = new DevComponents.DotNetBar.Controls.DataGridViewIpAddressInputColumn();
            this.BotaoListaEquipamentos = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // grid
            // 
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.grid.ColumnHeadersHeight = 25;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Titulo,
            this.MonitoRede,
            this.Endereco,
            this.BotaoListaEquipamentos});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(21)))), ((int)(((byte)(110)))));
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grid.DefaultCellStyle = dataGridViewCellStyle5;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.EnableHeadersVisualStyles = false;
            this.grid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(215)))), ((int)(((byte)(229)))));
            this.grid.Location = new System.Drawing.Point(0, 0);
            this.grid.Name = "grid";
            this.grid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.grid.Size = new System.Drawing.Size(502, 184);
            this.grid.TabIndex = 9;
            // 
            // Titulo
            // 
            this.Titulo.FillWeight = 165.4823F;
            this.Titulo.HeaderText = "Nome do equipamento";
            this.Titulo.Name = "Titulo";
            // 
            // MonitoRede
            // 
            this.MonitoRede.Checked = true;
            this.MonitoRede.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.MonitoRede.CheckValue = null;
            this.MonitoRede.HeaderText = "Monitor na rede";
            this.MonitoRede.Name = "MonitoRede";
            // 
            // Endereco
            // 
            this.Endereco.AutoOverwrite = true;
            this.Endereco.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.Endereco.BackgroundStyle.Class = "DataGridViewIpAddressBorder";
            this.Endereco.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.Endereco.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Endereco.HeaderText = "Endereço";
            this.Endereco.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Endereco.Name = "Endereco";
            this.Endereco.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Endereco.Text = "";
            // 
            // BotaoListaEquipamentos
            // 
            this.BotaoListaEquipamentos.FillWeight = 34.51777F;
            this.BotaoListaEquipamentos.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.BotaoListaEquipamentos.HeaderText = "";
            this.BotaoListaEquipamentos.Name = "BotaoListaEquipamentos";
            this.BotaoListaEquipamentos.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.BotaoListaEquipamentos.Text = "...";
            this.BotaoListaEquipamentos.ToolTipText = "Lista os equipamentos do setor";
            this.BotaoListaEquipamentos.UseColumnTextForButtonValue = true;
            // 
            // FormEquipamentos
            // 
            this.ClientSize = new System.Drawing.Size(502, 184);
            this.Controls.Add(this.grid);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormEquipamentos";
            this.Text = "Equipamentos";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.Controls.DataGridViewX grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Titulo;
        private DevComponents.DotNetBar.Controls.DataGridViewCheckBoxXColumn MonitoRede;
        private DevComponents.DotNetBar.Controls.DataGridViewIpAddressInputColumn Endereco;
        private System.Windows.Forms.DataGridViewButtonColumn BotaoListaEquipamentos;
    }
}