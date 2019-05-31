using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace smart.ctrlacesso
{
    public partial class MonitorOperacoes : OfficeForm
    {
        public delegate void Notificar(string info);
        public event Notificar AoNotificar;

        public MonitorOperacoes(Parametros parametros)
        {
            InitializeComponent();
            MontaConfiguracoesNaTela(parametros);
        }

        public void RegistraLog(string Log, Color color, FontStyle fontStyle = FontStyle.Regular)
        {
            Invoke((MethodInvoker)delegate
            {
                AppendText(richTextLog, $"{string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Now)} - {Log}", color, fontStyle, true);
            });
            AoNotificar?.Invoke(Log);
        }
        public static void AppendText(RichTextBoxEx box, string text, Color color, FontStyle fontStyle, bool AddNewLine = false)
        {
            if (AddNewLine)
            {
                text += Environment.NewLine;
            }

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.SelectionFont = new Font(box.SelectionFont, fontStyle);
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        public void MontaConfiguracoesNaTela(Parametros parametros)
        {
            //panelControladora.Text = null;
            if (parametros == null)
            {
                buttonItemIniciar.Enabled = false;
                MessageBox.Show(this, "Nenhum paraâmetro de configuração disponível");
                return;
            }
            else
            {
                //panelControladora.Text = $"<div align=\"vcenter\">Aguardando nova conexão</div>";
            }
        }

    }
}