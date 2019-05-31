using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;
using smart.controles;
using smart.info;
using smart.unidade;
using smart.info.Pesquisa;
using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using WinSCP;


namespace smart.automacao.Forms
{

    public partial class UserControlEquipamento : UserControl
    {
        public delegate void delegateAoencontrarComando(Comando comando);
        public event delegateAoencontrarComando AoExecutarComando;

        private Equipamento _equipamento;
        private static System.Timers.Timer aTimer;

        public UserControlEquipamento(Equipamento Equipamento)
        {
            InitializeComponent();

            _equipamento = Equipamento;
            LabelEquipamento.Text = Equipamento.Titulo;
            LabelEndereco.Text = Equipamento.Endereco;

            if (_equipamento.Controles != null)
                foreach (Controle item in _equipamento.Controles)
                {
                    item.Setor = _equipamento.Setor;
                    item.Equipamento = _equipamento.Titulo;
                    ItemControleMonitor itemControleMonitor = new ItemControleMonitor(item);
                    itemControleMonitor.AoRegistrar += ItemControleMonitor_AoRegistrar;
                    PaineldeControles.Items.Add(itemControleMonitor);
                }


            if (_equipamento.MonitorRede == true && _equipamento.Endereco != null)
            {
                aTimer = new System.Timers.Timer(1000);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
            }

        }

        private void ItemControleMonitor_AoRegistrar(Comando Comando)
        {
            if (AoExecutarComando != null)
                AoExecutarComando(Comando);
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(_equipamento.Endereco, 1000);
                if (reply != null)
                {
                    DefineCorPadrao();
                    Console.WriteLine($"Controle {_equipamento.Titulo} at {e.SignalTime:HH:mm:ss.fff} Status: {reply.Status} Time: {reply.RoundtripTime.ToString()} Address:{reply.Address}");
                    if (reply.Status != IPStatus.Success)
                    {
                        DefineCorProblema();
                    }
                }
            }
            catch
            {
                DefineCorProblema();
                Console.WriteLine("ERROR: You have Some TIMEOUT issue");
            }
            ObterControle.InvokeIfRequired(this, Refresh);
        }

        private void DefineCorPadrao()
        {
            LabelEquipamento.SymbolColor = Color.White;
        }

        private void DefineCorProblema()
        {
            LabelEquipamento.SymbolColor = Color.Red;
        }
    }

    public class ItemControleMonitor : MetroTileItem
    {
        public delegate void delegateaoRegistrar(Comando Comando);
        public event delegateaoRegistrar AoRegistrar;

        private Controle _controle;

        public ItemControleMonitor(Controle Controle)
        {
            _controle = Controle;

            Text = Controle.Titulo;
            Symbol = "58146";
            SymbolColor = SystemColors.ControlDarkDark;
            SymbolSet = eSymbolSet.Material;
            SymbolSize = 10F;
            TileColor = eMetroTileColor.Default;
            TileSize = new Size(180, 30);
            TileStyle.BackColor = Color.Transparent;
            TileStyle.BackColor2 = Color.Transparent;
            TileStyle.BorderBottom = eStyleBorderType.Solid;
            TileStyle.BorderColor = SystemColors.ButtonFace;
            TileStyle.BorderLeft = eStyleBorderType.Solid;
            TileStyle.BorderRight = eStyleBorderType.Solid;
            TileStyle.BorderTop = eStyleBorderType.Solid;
            TileStyle.CornerType = eCornerType.Square;
            TileStyle.TextColor = Color.Black;
            TitleTextColor = Color.Black;


            switch (Controle.Acao)
            {
                case TipoAcao.Acionamento:
                    Symbol = "57399";
                    break;
                case TipoAcao.Monitoramento:
                    Symbol = "57409";
                    break;
                case TipoAcao.Restart:
                    Symbol = "57384";
                    break;
            }

            Click += ItemPainel_Click;
        }

        private void ItemPainel_Click(object sender, EventArgs e)
        {
            switch (_controle.Acao)
            {
                case TipoAcao.Acionamento:
                    Acionamento();
                    break;
                case TipoAcao.Monitoramento:
                    break;
                case TipoAcao.Restart:
                    ComandoRemoto();
                    break;
            }
        }

        private void Acionamento()
        {
            // HttpWebRequest_BeginGetResponse webservice = new HttpWebRequest_BeginGetResponse();
            // webservice.Request(_controle.Comando);

            Comando comando = new Comando
            {
                Setor = _controle.Setor,
                Equipamento = _controle.Equipamento,
                Controle = _controle.Titulo,
                NomeComando = _controle.Comando
            };

            AoRegistrar?.Invoke(comando);
        }

        private void ComandoRemoto()
        {
            try
            {
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Scp,
                    //HostName = _controle.Endereco,
                    SshHostKeyFingerprint = "ssh-ed25519 256 V/7+e0ekLbPUiuDAFlHbc64p5gvE8WlzshMRmDMzG4g=",
                    SshPrivateKeyPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\craos.ppk",

                };

                using (Session session = new Session())
                {
                    session.Open(sessionOptions);
                    session.ExecuteCommand(_controle.Comando);
                    Console.WriteLine($"Executando o comando {_controle.Comando} em {_controle.Titulo}");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);

            }
        }
    }


}
