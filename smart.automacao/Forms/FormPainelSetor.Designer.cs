namespace smart.automacao
{
    partial class FormPainelSetor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPainelSetor));
            this.metroTileItem1 = new DevComponents.DotNetBar.Metro.MetroTileItem();
            this.Layout = new DevComponents.DotNetBar.ItemPanel();
            this.SuspendLayout();
            // 
            // metroTileItem1
            // 
            this.metroTileItem1.Name = "metroTileItem1";
            this.metroTileItem1.Symbol = "57399";
            this.metroTileItem1.SymbolColor = System.Drawing.Color.Empty;
            this.metroTileItem1.SymbolSet = DevComponents.DotNetBar.eSymbolSet.Material;
            this.metroTileItem1.SymbolSize = 16F;
            this.metroTileItem1.TileColor = DevComponents.DotNetBar.Metro.eMetroTileColor.Default;
            this.metroTileItem1.TileSize = new System.Drawing.Size(200, 30);
            // 
            // 
            // 
            this.metroTileItem1.TileStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.metroTileItem1.TileStyle.BackColor2 = System.Drawing.Color.Transparent;
            this.metroTileItem1.TileStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.metroTileItem1.TileStyle.BorderBottomWidth = 1;
            this.metroTileItem1.TileStyle.BorderColor = System.Drawing.SystemColors.Control;
            this.metroTileItem1.TileStyle.BorderLeftWidth = 1;
            this.metroTileItem1.TileStyle.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.metroTileItem1.TileStyle.BorderRightWidth = 1;
            this.metroTileItem1.TileStyle.BorderTopWidth = 1;
            this.metroTileItem1.TileStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.metroTileItem1.TileStyle.PaddingLeft = 30;
            this.metroTileItem1.TileStyle.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(65)))), ((int)(((byte)(66)))));
            this.metroTileItem1.TitleText = "Portão acessibilidade cadeirante";
            // 
            // Layout
            // 
            this.Layout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            // 
            // 
            // 
            this.Layout.BackgroundStyle.BackColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.DockSiteBackColor;
            this.Layout.BackgroundStyle.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.Layout.BackgroundStyle.BorderBottomColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.DockSiteBackColor;
            this.Layout.BackgroundStyle.BorderBottomWidth = 1;
            this.Layout.BackgroundStyle.BorderColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.DockSiteBackColor;
            this.Layout.BackgroundStyle.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.Layout.BackgroundStyle.BorderLeftWidth = 1;
            this.Layout.BackgroundStyle.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.Layout.BackgroundStyle.BorderRightWidth = 1;
            this.Layout.BackgroundStyle.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.Layout.BackgroundStyle.BorderTopWidth = 1;
            this.Layout.BackgroundStyle.Class = "ItemPanel";
            this.Layout.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.Layout.ContainerControlProcessDialogKey = true;
            this.Layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Layout.DragDropSupport = true;
            this.Layout.ForeColor = System.Drawing.Color.Black;
            this.Layout.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.Layout.Location = new System.Drawing.Point(0, 0);
            this.Layout.MultiLine = true;
            this.Layout.Name = "Layout";
            this.Layout.ReserveLeftSpace = false;
            this.Layout.Size = new System.Drawing.Size(457, 291);
            this.Layout.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.Layout.TabIndex = 0;
            // 
            // FormPainelSetor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BottomLeftCornerSize = 0;
            this.BottomRightCornerSize = 0;
            this.ClientSize = new System.Drawing.Size(457, 291);
            this.Controls.Add(this.Layout);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.HelpButtonText = "&Reiniciar";
            this.HelpButtonVisible = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPainelSetor";
            this.SettingsButtonText = "Ajustes";
            this.SettingsButtonVisible = true;
            this.Text = "Painel de equipamentos do setor";
            this.TopLeftCornerSize = 0;
            this.TopRightCornerSize = 0;
            this.ResumeLayout(false);

        }

        #endregion
        private DevComponents.DotNetBar.Metro.MetroTileItem metroTileItem1;
        private DevComponents.DotNetBar.ItemPanel Layout;
    }
}