using Newtonsoft.Json;
using smart.info;
using smart.ctrlacesso;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text;
using smart.pesquisa;
using smart.unidade;
using System.ComponentModel;
using System.Threading.Tasks;

namespace smart.craospwm.serial
{
    public class Principal : ISmartPlugins
    {
        bool formaberto = false;
        public string Nome => "SmartPWMSerial";
        public string Titulo => "Controle de acesso CraosPWM";
        public Image Icone => Properties.Resources.engrenagem;
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
                monitor = new MonitorOperacoes(CraosPWM.Parametros);
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
            if (CraosPWM.Serial != null)
            {
                monitor.RegistraLog("Inciando tentativa de conexão com a controladora", Color.Black);
                CraosPWM.Serial.Abrir();
            }
            else if (CraosPWM.Serial != null && CraosPWM.Serial.IsOpen)
            {
                monitor.RegistraLog("Finalizando a conexão com a controladora através do comando do usuário", Color.Black);
                monitor.buttonItemIniciar.Text = "Iniciar";
                CraosPWM.Serial.Close();
            }
        }

        private void Monitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CraosPWM.Serial != null)
                CraosPWM.Serial.Close();
        }

        private void Monitor_Shown(object sender, EventArgs e)
        {
            monitor.RegistraLog("Detectando o hardware USB", Color.Gray);
            if (HardwareInfo.USBSerial)
            {
                monitor.RegistraLog($"Hardware {PortCom.Fabricante} {PortCom.Modelo} detectado na porta {PortCom.PortName}", Color.LimeGreen, FontStyle.Bold);
                CraosPWM.Serial = new Serial
                {
                    DiscardNull = true,
                    PortName = PortCom.PortName,
                    Tipo = PortCom.Modelo,
                    BaudRate = 9600
                };

                CraosPWM.Serial.AoAbrir += Controle_AoAbrir;
                CraosPWM.Serial.AoEncontrarErro += Controle_AoEncontrarErro;
                CraosPWM.Serial.AoFechar += Controle_AoFechar;
                CraosPWM.Serial.AoIdentificar += Controle_AoIdentificar;
                CraosPWM.Serial.AoConectarCaboUSB += Serial_AoConectarCaboUSB;

            }
            else
            {
                monitor.RegistraLog("Nenhuma controladora foi localizada na porta USB", Color.Red, FontStyle.Bold);
                return;
            }

            if (CraosPWM.Parametros == null)
                return;

            if (CraosPWM.Parametros.UsarBaseLocal == true)
            {
                monitor.RegistraLog($"Usando a pesquisa na base local", Color.Black, FontStyle.Regular);
                pesquisa = pesquisaNaBaseLocal;
            }
            else
            {
                monitor.RegistraLog($"Usando a pesquisa na base remota", Color.Black, FontStyle.Regular);
                pesquisa = pesquisaNaBaseRemota;
            }
            pesquisa.Address = CraosPWM.Parametros.Endereco;
        }


        private void Controle_AoAbrir(object sender, EventArgs e)
        {
            monitor.buttonItemIniciar.Text = "Desconectar";
            monitor.RegistraLog(sender.ToString(), Color.Blue, FontStyle.Bold);
            monitor.RegistraLog($"(Controladora: {CraosPWM.Parametros.Descricao} {PortCom.Description})<div align=\"vcenter\">Conexão estabelecida com sucesso</div>", Color.Blue, FontStyle.Bold);
        }

        private void Controle_AoEncontrarErro(object sender, EventArgs e)
        {
            TratamentoErros erros = (TratamentoErros)sender;
            monitor.RegistraLog(erros.Detalhes.Message, Color.Red, FontStyle.Bold);

            if (erros.Numero == 1)
                monitor.buttonItemIniciar.Text = "Iniciar";

        }


        private void Controle_AoFechar(object sender, EventArgs e)
        {
            monitor.buttonItemIniciar.Text = "Iniciar";
            monitor.RegistraLog(sender.ToString(), Color.Gray);
            monitor.RegistraLog($"<div align=\"vcenter\">Conexão fechada</div>", Color.Red, FontStyle.Regular);
        }
        private void Controle_AoIdentificar(object sender, EventArgs e)
        {
            string Serial = sender.ToString();
            LeitorCorrente = CraosPWM.Parametros.Leitores[0];

            InformacoesPassagem = new Passagem
            {
                rfid = Serial,
                equipamento = CraosPWM.Parametros.Num,
                localizacao_equipamento = CraosPWM.Parametros.Descricao,
                leitor = LeitorCorrente.Num,
                localizacao_leitor = LeitorCorrente.Descricao,
                sentido = LeitorCorrente.Sentido,
                idade_minima = LeitorCorrente.idade_minima
            };

            if (!backgroundSerialPedestre.IsBusy)
                backgroundSerialPedestre.RunWorkerAsync();

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
                    CraosPWM.Serial.Write("8");

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
            CraosPWM.Salvar();
        }

        public void Iniciar()
        {
            CraosPWM.Carregar();
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

    public static class CraosPWM
    {
        private static string arquivoconfiguracao = $@"{AssemblyDirectory}\config.json";
        public static Parametros Parametros { get; set; }
        public static Serial Serial { get; set; }

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
