using Newtonsoft.Json;
using smart.ctrlacesso;
using smart.info;
using smart.pesquisa;
using smart.unidade;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinearHCS
{
    public class Principal : ISmartPlugins
    {
        bool formaberto = false;
        public string Nome => "LinearHCS";
        public string Titulo => "Controle de acesso LinearHCS";
        public Image Icone => LinearHCS.Properties.Resources.engrenagem;
        public bool ApresentaMenu => false;
        public UserControl PainelConfiguracoes => new Configuracoes();


        PesquisaRemota pesquisaNaBaseRemota = new PesquisaRemota();
        PesquisaLocal pesquisaNaBaseLocal = new PesquisaLocal();
        IPesquisaDispositivo pesquisa;

        MonitorOperacoes monitor;
        Passagem InformacoesPassagem;
        Leitor LeitorCorrente;

        BackgroundWorker backgroundSerialPedestre = new BackgroundWorker();
        BackgroundWorker backgroundSerialVeiculo = new BackgroundWorker();

        public event EventHandler Logs;
        public event EventHandler AoIdentificarDispositivo;

        public void Exibir()
        {
            if (formaberto == false)
            {
                monitor = new MonitorOperacoes(Linear.Parametros);
                monitor.AoNotificar += Monitor_AoNotificar;
                monitor.Shown += Monitor_Shown;
                monitor.FormClosing += Monitor_FormClosing;
                monitor.buttonItemIniciar.Click += IniciarFinalizar;
                monitor.FormClosed += (object fcsender, FormClosedEventArgs fce) => { formaberto = false; };
                monitor.Show();
                formaberto = true;

                backgroundSerialPedestre.WorkerReportsProgress = true;
                backgroundSerialPedestre.WorkerSupportsCancellation = true;
                backgroundSerialPedestre.DoWork += Background_DoWorkPedestre;

                backgroundSerialVeiculo.WorkerReportsProgress = true;
                backgroundSerialVeiculo.WorkerSupportsCancellation = true;
                backgroundSerialVeiculo.DoWork += Background_DoWorkVeiculo;

            }
        }

        private void Monitor_AoNotificar(string info)
        {
            if (Logs != null) Logs(info, new EventArgs());
        }

        private void IniciarFinalizar(object sender, EventArgs e)
        {
            if (Linear.Controle != null)
            {
                monitor.RegistraLog("Inciando tentativa de conexão com a controladora", Color.Black);
                Linear.Controle.IniciarConexao();
            }
            else if (Linear.Controle != null)
            {
                monitor.RegistraLog("Finalizando a conexão com a controladora através do comando do usuário", Color.Black);
                monitor.buttonItemIniciar.Text = "Iniciar";



            }
        }

        private void Monitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Linear.Controle != null)
                Linear.Controle.Close();
        }

        private void Monitor_Shown(object sender, EventArgs e)
        {
            monitor.RegistraLog("Detectando o hardware USB", Color.Gray);
            if (HardwareInfo.USBSerial)
            {
                monitor.RegistraLog($"Hardware {PortCom.Fabricante} {PortCom.Modelo} detectado na porta {PortCom.PortName}", Color.LimeGreen, FontStyle.Bold);

                if (Linear.Parametros.Tipo_conexao == "Serial")
                {
                    Linear.Controle = new ControleLinearHCS
                    {
                        Porta = PortCom.PortName
                    };

                    Linear.Controle.textBoxSetup_lerInfo.TextChanged += (object _sender, EventArgs _e) =>
                    {
                        string resultado = Linear.Controle.textBoxSetup_lerInfo.Text;
                        string[] informacoes = resultado.Split(',');

                        foreach (string info in informacoes)
                            monitor.RegistraLog(info.ToLower(), Color.Gray);
                    };

#if (DEBUG == true)
                    Linear.Controle.Show();
#endif

                }
                else
                {
                    Linear.Controle = new ControleLinearHCS
                    {
                        Endereco = Linear.Parametros.Endereco,
                        Porta = Linear.Parametros.Porta.ToString(),
                        TipoConexao = Linear.Parametros.Tipo_conexao
                    };

                }

                Linear.Controle.btConectarSerial.TextChanged += BtConectarSerial_TextChanged;
                Linear.Controle.btConectarTCP1.TextChanged += AlternarConexao;
                Linear.Controle.btConectarUDP1.TextChanged += AlternarConexao;
                Linear.Controle.EventoAoReceberSerial += Controle_EventoAoReceberSerial;
                Linear.Controle.Carga(this, new EventArgs());
                Linear.Controle.IniciarConexao();

            }
            else
            {
                monitor.RegistraLog("Nenhuma controladora foi localizada na porta USB", Color.Red, FontStyle.Bold);
                return;
            }

            if (Linear.Parametros == null)
                return;

            if (Linear.Parametros.UsarBaseLocal == true)
            {
                monitor.RegistraLog($"Usando a pesquisa na base local", Color.Black, FontStyle.Regular);
                pesquisa = pesquisaNaBaseLocal;
            }
            else
            {
                monitor.RegistraLog($"Usando a pesquisa na base remota", Color.Black, FontStyle.Regular);
                pesquisa = pesquisaNaBaseRemota;
            }
            pesquisa.Address = Linear.Parametros.Endereco;
        }

        private void AlternarConexao(object sender, EventArgs e)
        {
            monitor.buttonItemIniciar.Text = ((Button)sender).Text;
            if (monitor.buttonItemIniciar.Text == "Desconectar")
            {
                monitor.RegistraLog("<div align=\"vcenter\">Conexão estabelecida com sucesso</div>", Color.Blue, FontStyle.Bold);

                if (Linear.Controle != null)
                    Linear.Controle.btLerInformacoes_Click(this, new EventArgs());
            }
            else
            {
                monitor.RegistraLog("<div align=\"vcenter\">Conexão fechada</div>", Color.Red, FontStyle.Bold);
            }
        }

        private void Controle_EventoAoReceberSerial(string Serial, string porta, string leitora)
        {
            LeitorCorrente = Linear.Parametros.Leitores[0];

            InformacoesPassagem = new Passagem
            {
                rfid = Serial,
                equipamento = Linear.Parametros.Num,
                localizacao_equipamento = Linear.Parametros.Descricao,
                leitor = LeitorCorrente.Num,
                localizacao_leitor = LeitorCorrente.Descricao,
                sentido = LeitorCorrente.Sentido,
                idade_minima = LeitorCorrente.idade_minima
            };

            if (!backgroundSerialPedestre.IsBusy)
                backgroundSerialPedestre.RunWorkerAsync();
        }

        private void BtConectarSerial_TextChanged(object sender, EventArgs e)
        {
            monitor.buttonItemIniciar.Text = "Desconectar";
            monitor.RegistraLog(sender.ToString(), Color.Blue, FontStyle.Bold);
            monitor.RegistraLog($"(Controladora: {Linear.Parametros.Descricao} {PortCom.Description})<div align=\"vcenter\">Conexão estabelecida com sucesso</div>", Color.Blue, FontStyle.Bold);
        }

        private void Background_DoWorkPedestre(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            Task.Run(async () =>
            {

                Unidade info = await pesquisa.BuscarPedestre(InformacoesPassagem, LeitorCorrente.Procedimento);

                if (info == null)
                {
                    monitor.RegistraLog($"Leitora: {LeitorCorrente.Porta} {LeitorCorrente.Descricao} - Serial:{InformacoesPassagem.rfid} O Registro não foi localizado ", Color.Red, FontStyle.Bold);
                    InformacoesPassagem.Status = EstadoIdentificacao.NaoLocalizado;
                    AoIdentificarDispositivo?.Invoke(InformacoesPassagem, e);
                    worker.ReportProgress(100);
                    return;
                }

                InformacoesPassagem.Unidade = info;

                if (InformacoesPassagem.Autorizado)
                {
                    foreach (Acionador acionador in LeitorCorrente.Acionadores)
                        Linear.Controle.Acionamento(0, acionador.Endereco);
                }

                Pedestre pedestre = InformacoesPassagem.Unidade.PedestreIdentificado;
                AoIdentificarDispositivo?.Invoke(InformacoesPassagem, e);
                string detalhespedestre = $"({pedestre.Bloco}-{pedestre.Unidade}) {pedestre.Nome}";
                monitor.RegistraLog($"Leitora: {LeitorCorrente.Porta} {LeitorCorrente.Descricao} - Serial:{InformacoesPassagem.rfid} {detalhespedestre} ", Color.DeepSkyBlue);

            }).Wait();

            worker.ReportProgress(100);
        }


        private void Background_DoWorkVeiculo(object sender, DoWorkEventArgs e)
        {

        }
        private void Serial_AoConectarCaboUSB(object sender, EventArgs e)
        {
            monitor.RegistraLog(sender.ToString(), Color.Orange, FontStyle.Bold);
        }


        public void Finalizar()
        {
            Linear.Salvar();
        }

        public void Iniciar()
        {
            Linear.Carregar();
        }

        public void NovoLog()
        {
            AoNotificar(new EventArgs());
        }

        void AoNotificar(EventArgs e)
        {
            if (Logs != null) Logs(this, e);
        }

        public void NovoDispositivo()
        {
            NovaIdentificacao(new EventArgs());
        }

        public void NovaIdentificacao(EventArgs e)
        {
            if (AoIdentificarDispositivo != null) AoIdentificarDispositivo(this, e);
        }
    }

    public static class Linear
    {
        private static string arquivoconfiguracao = $@"{AssemblyDirectory}\config.json";
        public static Parametros Parametros { get; set; }
        public static ControleLinearHCS Controle { get; set; }

        public static void Carregar()
        {
            if (File.Exists(arquivoconfiguracao))
                Parametros = JsonConvert.DeserializeObject<Parametros>(File.ReadAllText(arquivoconfiguracao));
        }

        public static void Salvar()
        {
            File.WriteAllText(arquivoconfiguracao, JsonConvert.SerializeObject(Parametros));
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
