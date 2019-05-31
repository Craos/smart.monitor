using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LinearHCS
{
    public partial class Sniffer : Form
    {
        public Sniffer()
        {
            InitializeComponent();
        }

        private void richTextBox_sniffer_TextChanged(object sender, EventArgs e)
        {
            richTextBox_sniffer.Text = ControleLinearHCS.rt_sniffer.Text;

            richTextBox_sniffer.AppendText(ControleLinearHCS.rt_sniffer.Text);
            richTextBox_sniffer.ScrollToCaret();
        }
    }
}
