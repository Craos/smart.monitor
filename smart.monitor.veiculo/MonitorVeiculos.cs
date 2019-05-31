using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;
using smart.controles;
using smart.monitor.veiculo.Properties;
using smart.unidade;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace smart.monitor.veiculos
{
    public partial class MonitorVeiculos : OfficeForm
    {
        delegate void InvokeControle(DevComponents.DotNetBar.LabelX control, string text);

        public MonitorVeiculos()
        {
            InitializeComponent();

            // Carrega as configurações de posicionamento e tamanho da janela
            Location = Settings.Default.local_janela;
            Size = Settings.Default.tamanho_janela;

            // Habilita a janela para poder fazer alterações
            StartPosition = FormStartPosition.Manual;

        }

        public void Exibir()
        {
            Show();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="passagem"></param>
        public void NaoLocalizado(Passagem passagem)
        {
            DefineTexto(unidade, "Não localizado");
            DefineTexto(identificador_rfid, passagem.rfid);
            DefineTexto(modelo, null);
            DefineTexto(placa, null);
            DefineTexto(cor, null);
            ObterControle.InvokeIfRequired(this, Refresh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="passagem"></param>
        public void ExibirIdentificacao(Passagem passagem)
        {

            LimpaInformacoes();

            // Apresenta o Serial RFID na tela
            DefineTexto(identificador_rfid, passagem.rfid);
            CarregaInformacoesnaTela(passagem);
            ObterControle.InvokeIfRequired(this, Refresh);
        }

        void CarregaInformacoesnaTela(Passagem passagem)
        {
            DefineTexto(unidade, $"{passagem.Unidade.Bloco} - {passagem.Unidade.Numero}");
            DefineTexto(modelo, passagem.Unidade.VeiculoIdentificado.Modelo);
            DefineTexto(placa, passagem.Unidade.VeiculoIdentificado.Placa);
            DefineTexto(cor, passagem.Unidade.VeiculoIdentificado.Cor);
            ObterControle.InvokeIfRequired(this, Refresh);
        }

        void LimpaInformacoes()
        {
            DefineTexto(identificador_rfid, null);
            DefineTexto(unidade, null);
            DefineTexto(modelo, null);
            DefineTexto(placa, null);
            DefineTexto(cor, null);
            ObterControle.InvokeIfRequired(this, Refresh);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        private void DefineTexto(DevComponents.DotNetBar.LabelX control, string text)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new InvokeControle(DefineTexto), new object[] { control, text });
            }
            else
            {
                control.Text = text;
            }
        }



        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (identificador_rfid.Text.Length > 0)
            {
                Clipboard.SetText(identificador_rfid.Text.PadRight(5));
                var notification = new NotifyIcon()
                {
                    Visible = true,
                    Icon = SystemIcons.Information,
                    BalloonTipText = $"Copiando: {identificador_rfid.Text.PadRight(5)}",
                    BalloonTipTitle = "Leitor RFID",
                };
                notification.ShowBalloonTip(5000);
                Thread.Sleep(10000);
                notification.Dispose();
            }
        }

        private void MonitorVeiculos_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Salva as configurações do posicionamento e tamanho da janela
            Settings.Default.local_janela = Location;
            Settings.Default.tamanho_janela = Size;
            Settings.Default.Save();
        }
    }
}
