using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;
using smart.controles;
using smart.monitor.pedestre.Properties;
using smart.unidade;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using smart.info;

namespace smart.monitor.pedestre
{
    public partial class MonitorPedestres : OfficeForm
    {

        delegate void InvokeControle(Control control, string text);

        public MonitorPedestres()
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
        private void NaoLocalizado(Passagem passagem)
        {
            DefineTexto(unidade, null);
            DefineTexto(_nome, "Usuário não localizado");
            DefineTexto(_idade, null);
            DefineTexto(identificador_rfid, passagem.rfid);
            _foto.Style.BackgroundImage = null;
            PaineldeFotos.SubItems.Clear();
            ObterControle.InvokeIfRequired(this, Refresh);
        }


        public void ExibirIdentificacao(object info)
        {
            Passagem passagem = (Passagem)info;
            LimpaInformacoes();

            if (passagem.Status == EstadoIdentificacao.NaoLocalizado)
            {
                NaoLocalizado(passagem);
                return;
            }

            // Apresenta o Serial RFID na tela
            DefineTexto(identificador_rfid, passagem.rfid);

            if (passagem.Unidade != null)
            {
                CarregaInformacoesnaTela(passagem);
                ObterControle.InvokeIfRequired(this, Refresh);
            }
            else
            {
                ObterControle.InvokeIfRequired(this, Refresh);
            }
        }

        void CarregaInformacoesnaTela(Passagem passagem)
        {
            Unidade infounidade = passagem.Unidade;
            DefineTexto(unidade, $"{infounidade.Bloco} - {infounidade.Numero}");
            DefineTexto(_nome, infounidade.PedestreIdentificado.Nome);
            DefineTexto(_idade, $"{infounidade.PedestreIdentificado.Idade} anos");

            if (infounidade.PedestreIdentificado.Foto1 != null && infounidade.PedestreIdentificado.Foto1.Length > 256)
                _foto.Style.BackgroundImage = infounidade.PedestreIdentificado.Foto();

            foreach (Pedestre item in infounidade.ListaPessoas)
                MontaFotodaUnidade(item);

            ObterControle.InvokeIfRequired(this, Refresh);
        }

        void LimpaInformacoes()
        {
            DefineTexto(identificador_rfid, null);
            DefineTexto(unidade, null);
            DefineTexto(_nome, null);
            DefineTexto(_idade, null);

            _foto.Style.BackgroundImage = null;
            PaineldeFotos.SubItems.Clear();
            ObterControle.InvokeIfRequired(this, Refresh);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Pedestre"></param>
        private void MontaFotodaUnidade(Pedestre Pedestre)
        {
            MetroTileItem FotoItemUnidade = new MetroTileItem
            {
                SymbolColor = Color.Empty,
                TitleTextAlignment = ContentAlignment.BottomCenter,
                TitleTextFont = new Font("Microsoft Sans Serif", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0),
                TileSize = new Size(169, 126)
            };

            FotoItemUnidade.TileStyle.BackColor = SystemColors.Control;
            FotoItemUnidade.TileStyle.BackColor2 = SystemColors.Control;
            FotoItemUnidade.TileStyle.BorderBottom = eStyleBorderType.Solid;
            FotoItemUnidade.TileStyle.BorderColor = Color.FromArgb(255, 255, 255);
            FotoItemUnidade.TileStyle.BorderLeft = eStyleBorderType.Solid;
            FotoItemUnidade.TileStyle.BorderRight = eStyleBorderType.Solid;
            FotoItemUnidade.TileStyle.BorderTop = eStyleBorderType.Solid;
            FotoItemUnidade.TileStyle.CornerType = eCornerType.Square;
            FotoItemUnidade.TitleTextColor = SystemColors.GrayText;
            FotoItemUnidade.TitleText = Pedestre.Nome;

            if (Pedestre.Foto1 != null && Pedestre.Foto1.Length > 256)
            {
                FotoItemUnidade.TitleTextColor = Color.White;
                FotoItemUnidade.TileStyle.BackgroundImage = Pedestre.Foto();
            }

            PaineldeFotos.SubItems.AddRange(new BaseItem[] { FotoItemUnidade });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="control"></param>
        /// <param name="text"></param>
        private void DefineTexto(Control control, string text)
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

        private void MonitorPedestres_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Salva as configurações do posicionamento e tamanho da janela
            Settings.Default.local_janela = Location;
            Settings.Default.tamanho_janela = Size;
            Settings.Default.Save();
        }
    }
}
