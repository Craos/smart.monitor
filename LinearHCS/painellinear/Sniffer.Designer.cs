namespace LinearHCS
{
    partial class Sniffer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sniffer));
            this.richTextBox_sniffer = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // richTextBox_sniffer
            // 
            this.richTextBox_sniffer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox_sniffer.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.richTextBox_sniffer.Location = new System.Drawing.Point(0, 0);
            this.richTextBox_sniffer.Name = "richTextBox_sniffer";
            this.richTextBox_sniffer.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.richTextBox_sniffer.Size = new System.Drawing.Size(1008, 187);
            this.richTextBox_sniffer.TabIndex = 0;
            this.richTextBox_sniffer.Text = "";
            this.richTextBox_sniffer.TextChanged += new System.EventHandler(this.richTextBox_sniffer_TextChanged);
            // 
            // Sniffer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 187);
            this.Controls.Add(this.richTextBox_sniffer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Sniffer";
            this.Text = "Sniffer";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox richTextBox_sniffer;

    }
}