namespace smart.login
{
    partial class Login
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
            this.Navegador = new Gecko.GeckoWebBrowser();
            this.SuspendLayout();
            // 
            // Navegador
            // 
            this.Navegador.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Navegador.FrameEventsPropagateToMainWindow = false;
            this.Navegador.Location = new System.Drawing.Point(0, 0);
            this.Navegador.Name = "Navegador";
            this.Navegador.Size = new System.Drawing.Size(384, 590);
            this.Navegador.TabIndex = 0;
            this.Navegador.UseHttpActivityObserver = false;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 590);
            this.Controls.Add(this.Navegador);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart Login";
            this.ResumeLayout(false);

        }

        #endregion

        private Gecko.GeckoWebBrowser Navegador;
    }
}

