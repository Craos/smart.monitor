using smart.monitor.Properties;

namespace smart
{
    partial class FormConfig
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConfig));
            this.StyleManagerCraos = new DevComponents.DotNetBar.StyleManager(this.components);
            this.ControleTabulacao = new DevComponents.DotNetBar.TabControl();
            this.tabControlPanel1 = new DevComponents.DotNetBar.TabControlPanel();
            this.tabItemGeral = new DevComponents.DotNetBar.TabItem(this.components);
            this.panelEx1 = new DevComponents.DotNetBar.PanelEx();
            this.buttonSalvar = new DevComponents.DotNetBar.ButtonX();
            ((System.ComponentModel.ISupportInitialize)(this.ControleTabulacao)).BeginInit();
            this.ControleTabulacao.SuspendLayout();
            this.panelEx1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StyleManagerCraos
            // 
            this.StyleManagerCraos.ManagerColorTint = System.Drawing.Color.Gray;
            this.StyleManagerCraos.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2016;
            this.StyleManagerCraos.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(115)))), ((int)(((byte)(199))))));
            // 
            // ControleTabulacao
            // 
            this.ControleTabulacao.AntiAlias = true;
            this.ControleTabulacao.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ControleTabulacao.CanReorderTabs = true;
            this.ControleTabulacao.Controls.Add(this.tabControlPanel1);
            this.ControleTabulacao.Dock = System.Windows.Forms.DockStyle.Top;
            this.ControleTabulacao.ForeColor = System.Drawing.Color.Black;
            this.ControleTabulacao.Location = new System.Drawing.Point(0, 0);
            this.ControleTabulacao.Name = "ControleTabulacao";
            this.ControleTabulacao.SelectedTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.ControleTabulacao.SelectedTabIndex = 0;
            this.ControleTabulacao.Size = new System.Drawing.Size(655, 350);
            this.ControleTabulacao.Style = DevComponents.DotNetBar.eTabStripStyle.Metro;
            this.ControleTabulacao.TabIndex = 0;
            this.ControleTabulacao.TabLayoutType = DevComponents.DotNetBar.eTabLayoutType.FixedWithNavigationBox;
            this.ControleTabulacao.Tabs.Add(this.tabItemGeral);
            // 
            // tabControlPanel1
            // 
            this.tabControlPanel1.DisabledBackColor = System.Drawing.Color.Empty;
            this.tabControlPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlPanel1.Location = new System.Drawing.Point(0, 27);
            this.tabControlPanel1.Name = "tabControlPanel1";
            this.tabControlPanel1.Padding = new System.Windows.Forms.Padding(1);
            this.tabControlPanel1.Size = new System.Drawing.Size(655, 323);
            this.tabControlPanel1.Style.BackColor1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.tabControlPanel1.Style.Border = DevComponents.DotNetBar.eBorderType.SingleLine;
            this.tabControlPanel1.Style.BorderColor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.tabControlPanel1.Style.BorderSide = ((DevComponents.DotNetBar.eBorderSide)(((DevComponents.DotNetBar.eBorderSide.Left | DevComponents.DotNetBar.eBorderSide.Right)
            | DevComponents.DotNetBar.eBorderSide.Bottom)));
            this.tabControlPanel1.Style.GradientAngle = 90;
            this.tabControlPanel1.TabIndex = 1;
            this.tabControlPanel1.TabItem = this.tabItemGeral;
            // 
            // tabItemGeral
            // 
            this.tabItemGeral.AttachedControl = this.tabControlPanel1;
            this.tabItemGeral.Name = "tabItemGeral";
            this.tabItemGeral.Text = "Geral";
            // 
            // panelEx1
            // 
            this.panelEx1.CanvasColor = System.Drawing.SystemColors.Control;
            this.panelEx1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.panelEx1.Controls.Add(this.buttonSalvar);
            this.panelEx1.DisabledBackColor = System.Drawing.Color.Empty;
            this.panelEx1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelEx1.Location = new System.Drawing.Point(0, 359);
            this.panelEx1.Name = "panelEx1";
            this.panelEx1.Size = new System.Drawing.Size(655, 42);
            this.panelEx1.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelEx1.Style.BackColor1.Color = System.Drawing.Color.White;
            this.panelEx1.Style.BackColor2.Color = System.Drawing.Color.White;
            this.panelEx1.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelBorder;
            this.panelEx1.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.PanelText;
            this.panelEx1.Style.GradientAngle = 90;
            this.panelEx1.TabIndex = 2;
            // 
            // buttonSalvar
            // 
            this.buttonSalvar.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonSalvar.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonSalvar.Location = new System.Drawing.Point(571, 9);
            this.buttonSalvar.Name = "buttonSalvar";
            this.buttonSalvar.Size = new System.Drawing.Size(75, 23);
            this.buttonSalvar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonSalvar.TabIndex = 2;
            this.buttonSalvar.Text = "&Salvar";
            // 
            // FormConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 401);
            this.Controls.Add(this.panelEx1);
            this.Controls.Add(this.ControleTabulacao);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfig";
            this.Text = "Configurações";
            ((System.ComponentModel.ISupportInitialize)(this.ControleTabulacao)).EndInit();
            this.ControleTabulacao.ResumeLayout(false);
            this.panelEx1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public DevComponents.DotNetBar.StyleManager StyleManagerCraos;
        private DevComponents.DotNetBar.TabControl ControleTabulacao;
        private DevComponents.DotNetBar.TabControlPanel tabControlPanel1;
        private DevComponents.DotNetBar.TabItem tabItemGeral;
        private DevComponents.DotNetBar.PanelEx panelEx1;
        private DevComponents.DotNetBar.ButtonX buttonSalvar;
    }
}

