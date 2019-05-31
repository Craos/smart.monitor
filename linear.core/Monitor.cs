using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace linear.core
{
    public class MonitorLinearHCS
    {
        public delegate void AoConectar(object sender, EventArgs e);
        public event AoConectar EventoAoAbrir;

        public delegate void AoDesconectar(object sender, EventArgs e);
        public event AoDesconectar EventoAoFechar;

        public delegate void AoReceberMensagem(object sender, EventArgs e);
        public event AoReceberMensagem EventoAoReceberMensagem;

        public delegate void AoReceberSerial(string Serial, string porta, string leitora);
        public event AoReceberSerial EventoAoReceberSerial;

        public int Porta { get; set; }
        public string Endereco { get; set; }
        
       
        private readonly System.Timers.Timer TimerConexao;
        private readonly System.Timers.Timer TimerVerificacao;

        private string estadoconexao = "Conectar TCP";
        private string lblEnderecoPlaca;
        private string lblNumeroSerie;
        private string lblSaidaAcionada;
        private bool tcpClientState = false;
        private int tentativadeConexao = 0;
        private ConexaoTCP conexaoTCP = null;
        private readonly controladoraLinear pControl = new controladoraLinear();
        private TimeSpan tempoAnterior;
        private controladoraLinear.MemoriaPlaca MemoriaPlaca { get => memoriaPlaca; set => memoriaPlaca = value; }
        private readonly controladoraLinear.MemoriaPlaca2[] bufferMsgRecebida = new controladoraLinear.MemoriaPlaca2[255];
        private controladoraLinear.MemoriaPlaca memoriaPlaca;
        
        public MonitorLinearHCS()
        {
            TimerConexao = new System.Timers.Timer(1000 * 5);
            TimerConexao.Elapsed += MonitorTempo;
            TimerConexao.AutoReset = true;
            TimerConexao.Enabled = false;

            TimerVerificacao = new System.Timers.Timer(1000 * 60);
            TimerVerificacao.Elapsed += MonitordeRede;
            TimerVerificacao.AutoReset = true;
            TimerVerificacao.Enabled = false;

        }

        public void FinalizarConexao()
        {
            conexaoTCP.Close();
        }

        private void MonitorTempo(object sender, EventArgs e)
        {
            try
            {
                if (tentativadeConexao > 0)
                {
                    tentativadeConexao--;
                    if (tentativadeConexao == 0)
                    {
                        Console.WriteLine($"Aguardando disponibilidade da controladora");
                        Conectar_TCP1();
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);

                //throw new LinearException("Temporizador de conexão", err);
            }
        }

        private void MonitordeRede(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (conexaoTCP.State == true)
            {
                byte[] buf = pControl.lerQuantidadeDispositivos(controladoraLinear.DISP_CT);
                conexaoTCP.Client.Send(buf);
                EventoAoReceberMensagem("Enviando sinal de conexão para a controladora...", new EventArgs());
            }
            else
            {
                Console.WriteLine($"A conexão parece está finalizada. O sistema irá efetuar uma nova tentativa.");
                conexaoTCP = null;
                Conectar_TCP1();
            }
        }

        public void IniciarConexao()
        {
            try
            {
                tcpClientState = true;

                if (conexaoTCP != null)
                    conexaoTCP.Close();

                ConnectToServer_TCP1();
                tentativadeConexao = 1;

            }
            catch (ArgumentException err)
            {
                throw new LinearException("IniciarConexao", err);
            }
            catch (Exception err)
            {
                throw new LinearException("IniciarConexao", err);
            }
        }

        public void Carga()
        {
            try
            {
                int i;

                TimerConexao.Enabled = true;
                TimerConexao.Start();
                
                controladoraLinear.FLAGS_CFG_E flagsE = new controladoraLinear.FLAGS_CFG_E();
                controladoraLinear.FLAGS_CFG_1 flags1 = new controladoraLinear.FLAGS_CFG_1();
                controladoraLinear.FLAGS_CFG_2 flags2 = new controladoraLinear.FLAGS_CFG_2();
                controladoraLinear.FLAGS_CFG_3 flags3 = new controladoraLinear.FLAGS_CFG_3();
                controladoraLinear.FLAGS_CFG_4 flags4 = new controladoraLinear.FLAGS_CFG_4();
                controladoraLinear.FLAGS_CFG_5 flags5 = new controladoraLinear.FLAGS_CFG_5();
                controladoraLinear.FLAGS_CFG_6 flags6 = new controladoraLinear.FLAGS_CFG_6();
                controladoraLinear.FLAGS_CFG_7 flags7 = new controladoraLinear.FLAGS_CFG_7();
                controladoraLinear.FLAGS_CFG_8 flags8 = new controladoraLinear.FLAGS_CFG_8();
                controladoraLinear.FLAGS_CFG_9 flags9 = new controladoraLinear.FLAGS_CFG_9();
                controladoraLinear.FLAGS_CFG_10 flags10 = new controladoraLinear.FLAGS_CFG_10();
                controladoraLinear.FLAGS_CFG_11 flags11 = new controladoraLinear.FLAGS_CFG_11();
                controladoraLinear.FLAGS_CFG_12 flags12 = new controladoraLinear.FLAGS_CFG_12();
                controladoraLinear.FLAGS_CFG_13 flags13 = new controladoraLinear.FLAGS_CFG_13();
                controladoraLinear.FLAGS_CFG_14 flags14 = new controladoraLinear.FLAGS_CFG_14();
                controladoraLinear.FLAGS_CFG_15 flags15 = new controladoraLinear.FLAGS_CFG_15();
                controladoraLinear.FLAGS_CFG_16 flags16 = new controladoraLinear.FLAGS_CFG_16();
                controladoraLinear.FLAGS_CFG_17 flags17 = new controladoraLinear.FLAGS_CFG_17();
                controladoraLinear.FLAGS_CFG_18 flags18 = new controladoraLinear.FLAGS_CFG_18();
                controladoraLinear.FLAGS_CFG_19 flags19 = new controladoraLinear.FLAGS_CFG_19();

                byte[,] dispositivos = new byte[controladoraLinear.N_LINES_DISPOSITIVO, controladoraLinear.SIZE_OF_DISPOSITIVO];
                byte[,] rotas = new byte[controladoraLinear.N_LINES_HABILITACAO, controladoraLinear.SIZE_OF_HABILITACAO];
                byte[,] gruposLeitoras = new byte[controladoraLinear.N_LINES_GRUPO_LEITORA, controladoraLinear.SIZE_OF_GRUPO_LEITORA];
                byte[,] jornadas = new byte[controladoraLinear.N_LINES_JORNADAS, controladoraLinear.SIZE_OF_JORNADAS];
                byte[,] turnos = new byte[controladoraLinear.N_LINES_TURNOS, controladoraLinear.SIZE_OF_TURNOS];
                byte[] feriados = new byte[controladoraLinear.N_LINES_FERIADO * controladoraLinear.SIZE_OF_FERIADO];
                byte[,] labelsRotas = new byte[controladoraLinear.N_LINES_HABILITACAO, 8];
                byte[,] mensagensDisplay = new byte[12, 32];

                controladoraLinear.s_setup setup = new controladoraLinear.s_setup(0, 0, 0, 0, 0, 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0,
                                    0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.SIZE_OF_BYTES_RESERVADOS], 0, 0, 0, 0,
                                    0, 0, 0, 0, 0, 0, new byte[controladoraLinear.SIZE_OF_DDNS_USUARIO], new byte[controladoraLinear.SIZE_OF_DDNS_SENHA], new byte[controladoraLinear.SIZE_OF_DDNS_DEVICE],
                                    new byte[controladoraLinear.SIZE_OF_USUARIO_LOGIN], new byte[controladoraLinear.SIZE_OF_SENHA_LOGIN], new byte[controladoraLinear.SIZE_OF_DNS_HOST], new byte[controladoraLinear.SIZE_OF_DDNS_HOST], 0, new byte[controladoraLinear.N_LEITORAS], 0,
                                    0, new byte[4], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    flagsE, flags1, flags2, flags3, flags4, flags5, flags6, flags7, flags8, flags9, flags10, flags11, flags12, flags13, flags14, flags15, flags16, flags17, flags18, new byte[2], flags19, 0);

                controladoraLinear.MemoriaPlaca memoriaPlacaTemp = new controladoraLinear.MemoriaPlaca(dispositivos, setup, rotas, gruposLeitoras, jornadas, turnos, feriados, labelsRotas, mensagensDisplay);
                for (i = 0; i < 255; i++) bufferMsgRecebida[i] = new controladoraLinear.MemoriaPlaca2(null, IPAddress.Any, 0, null, 0, new byte[256, 1500]);

                MemoriaPlaca = memoriaPlacaTemp;
                DateTime result = DateTime.Today.Add(TimeSpan.FromDays(3650)); // + 10 anos

            }
            catch (Exception err)
            {
                throw new LinearException("Carga", err);
            }
        }

        private bool VerificaChecksum(byte[] buf, int indice, int tamanho)
        {
            try
            {
                int i;
                byte soma = 0;
                for (i = 0; i < tamanho - 1; i++) soma += buf[indice + i];
                if (soma == buf[indice + tamanho - 1]) return true;
                return false;
            }
            catch { return false; };
        }

        //

        private void MostraRetorno(byte retorno)
        {
            try
            {
                string DescricaoRetorno = controladoraLinear.TipoRetorno.Find(x=>x.Key == retorno).Value;
                
                int hora, minuto, segundo, miles;
                hora = DateTime.Now.Hour;
                minuto = DateTime.Now.Minute;
                segundo = DateTime.Now.Second;
                miles = DateTime.Now.Millisecond;

                TimeSpan tempoAtual = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

                DescricaoRetorno += "     " + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond);

                TimeSpan dt = tempoAnterior.Subtract(tempoAtual);

                DescricaoRetorno += "  (" + Convert.ToString(Math.Abs(dt.Hours)) + ':'
                                            + Convert.ToString(Math.Abs(dt.Minutes)) + ':'
                                            + Convert.ToString(Math.Abs(dt.Seconds)) + ':'
                                            + Convert.ToString(Math.Abs(dt.Milliseconds)) + ")";

                tempoAnterior = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                EventoAoReceberMensagem(DescricaoRetorno, new EventArgs());


            }
            catch (Exception err)
            {
                throw new LinearException("Mostra Retorno", err);
            }
        }
        private void TrataMensagemRecebida(byte[] buf)
        {
            try
            {

                byte cmd = buf[1];
                byte retorno = buf[2];

                switch (cmd)
                {
                    case 105: // Ler eventos  (PROGRESSIVO)
                        if (retorno == controladoraLinear.RT_OK)
                            TrataRespostaCmd_lerEventos(buf);
                        break;

                    case 116: // Evento automático
                        TrataRespostaCmd_lerEventos(buf);
                        break;

                    case 134: // Evento automático com endereço do ponteiro (EVENTO INDEXADO)
                        TrataRespostaCmd_lerEventos(buf);
                        break;
                }
                MostraRetorno(retorno);

            }
            catch (Exception err)
            {
                throw new LinearException("Trata mensagem recebida", err);
            }
        }

        private void TrataRespostaCmd_lerEventos(byte[] buf)
        {
            try
            {
                byte contadorAtual = 0, evento = 0, modo = 0, endereco = 0, tipo = 0, dia = 0, mes = 0, ano = 0, hora = 0, minuto = 0, segundo = 0, nivel = 0, setor = 0;
                controladoraLinear.FLAGS_EVENTO flagsEvento = new controladoraLinear.FLAGS_EVENTO(0, 0, 0, 0);
                ulong serial = 0;
                ushort contadorHCS = 0;
                pControl.vetorParaEvento(buf, ref contadorAtual, ref evento, ref modo, ref endereco, ref tipo, ref serial, ref contadorHCS, ref dia, ref mes, ref ano, ref hora, ref minuto, ref segundo, ref nivel, ref flagsEvento, ref setor);
                

                // endereco
                lblEnderecoPlaca = Convert.ToString(endereco + 1);

               
                // serial
                byte[] serialTemp = new byte[6];
                byte[] contadorTemp = new byte[2];
                contadorTemp[0] = 0;
                contadorTemp[1] = 0;
                if (tipo == controladoraLinear.DISP_TX)
                {
                    serialTemp[2] = (byte)(serial >> 24);
                    serialTemp[3] = (byte)(serial >> 16);
                    serialTemp[4] = (byte)(serial >> 8);
                    serialTemp[5] = (byte)serial;
                }
                else
                {
                    serialTemp[0] = (byte)(serial >> 40);
                    serialTemp[1] = (byte)(serial >> 32);
                    serialTemp[2] = (byte)(serial >> 24);
                    serialTemp[3] = (byte)(serial >> 16);
                    serialTemp[4] = (byte)(serial >> 8);
                    serialTemp[5] = (byte)serial;
                }
                contadorTemp[0] = (byte)(contadorHCS >> 8);
                contadorTemp[1] = (byte)contadorHCS;

                lblNumeroSerie = BytesToHexString(serialTemp);

                // saída acionada
                lblSaidaAcionada = Convert.ToString(flagsEvento.leitoraAcionada + 1);
               
                if (evento == controladoraLinear.LOG_SERIAL_NAO_CADASTRADO)
                {
                    //TimerVerificacao.Stop();
                    //TimerVerificacao.Start();
                    EventoAoReceberSerial(lblNumeroSerie, lblEnderecoPlaca, lblSaidaAcionada);
                }
                    

            }
            catch (Exception err)
            {
                throw new LinearException("Leitura dos eventos", err);
            }
        }

        public void AcionarRele()
        {
            try
            {
                int tipoacionamento = 0; // 0 Rele principal, 1 auxiliar
                int endereco = 0;
                int rele = 0; // de 0 a 3

                byte[] buf = pControl.acionamento((byte)tipoacionamento,
                                        (byte)endereco,
                                        (byte)rele);

                EnviarMensagem(buf);
            }
            catch { };
        }
        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                try
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                catch (Exception err)
                {
                    throw new LinearException("Valor inválido!!", err);
                }
            }
            return sb.ToString();
        }

        
        private void EnviarMensagem(byte[] buf)
        {
            try
            {
                int tamanhoMsg = buf.Length;
                string msg = BytesToHexString(buf);
                msg = msg.Substring(0, tamanhoMsg * 2);
                for (int j = 2; j < tamanhoMsg * 3; j += 3) msg = msg.Insert(j, " ");

                Console.WriteLine(msg);
            }
            catch (Exception err)
            {
                throw new LinearException("Enviar mensagem para controladora", err);
            }

            tempoAnterior = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            try
            {
                // TCP client
                if ((conexaoTCP != null) && (conexaoTCP.Connected == true) && (tcpClientState == false))
                {
                    conexaoTCP.Client.Send(buf);
                }
            }
            catch (Exception err)
            {
                throw new LinearException("Erro ao enviar mensagem via TCP", err);
            }
        }
        private void Conectar_TCP1()
        {
            if (estadoconexao == "Conectar TCP")
            {
                try
                {
                    ConnectToServer_TCP1();
                    if (conexaoTCP.Connected == true)
                    {
                        tcpClientState = false;
                        estadoconexao = "Desconectar";
                        //TimerVerificacao.Enabled = true;
                        EventoAoAbrir(this, new EventArgs());
                        tentativadeConexao = 0;
                        byte[] buf = pControl.lerSetup();
                        EnviarMensagem(buf);
                    }
                    else
                    {
                        tentativadeConexao = 100;
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    //throw new LinearException("Falha na conexão", err);
                }
            }
            else
            {
                try
                {
                    tentativadeConexao = 0;
                    tcpClientState = true;

                    if (conexaoTCP != null)
                        conexaoTCP.Close();

                    estadoconexao = "Conectar TCP";
                    //TimerVerificacao.Enabled = false;
                    EventoAoFechar(this, new EventArgs());
                }
                catch (Exception err)
                {
                    throw new LinearException("Falha ao fechar o socket", err);
                }
            }
        }
        private void ConnectToServer_TCP1()
        {
            conexaoTCP = new ConexaoTCP(AddressFamily.InterNetwork)
            {
                Endereco = Endereco,
                Porta = Porta
            };
            conexaoTCP.EventoAoReceberInfo += ConexaoTCP_EventoAoReceberInfo;
            conexaoTCP.EventoAoFalharRede += ConexaoTCP_EventoAoFalharRede;
            conexaoTCP.Connect();
        }

        private void ConexaoTCP_EventoAoFalharRede(string mensagem)
        {
            EventoAoReceberMensagem(mensagem, new EventArgs());
        }

        private void ConexaoTCP_EventoAoReceberInfo(byte[] buf, string tipoPorta, string epOrigem)
        {
            int tamanhoMsg = 0;
            int tamanho = buf.Length;

            for (int i = 0; i < tamanho; i++)
            {
                if ((buf[i] == 'S') &&
                    (buf[i + 1] == 'T') &&
                    (buf[i + 2] == 'X'))
                {
                    tamanhoMsg = (buf[i + 3] << 8) + buf[i + 4];
                    if (VerificaChecksum(buf, i + 5, tamanhoMsg) == true)
                    {
                        byte[] newBuf = new byte[tamanhoMsg];
                        for (int k = 0; k < tamanhoMsg; k++) newBuf[k] = buf[i + 5 + k];

                        byte[] newBuf2 = new byte[tamanhoMsg + 8];
                        for (int k = 0; k < (tamanhoMsg + 8); k++) newBuf2[k] = buf[i + k];
                        string nBytes = Convert.ToString(tamanhoMsg + 8);
                        string msg = BytesToHexString(newBuf2);
                        msg = msg.Substring(0, (tamanhoMsg + 8) * 2);
                        for (int j = 2; j < (tamanhoMsg + 8) * 3; j += 3) msg = msg.Insert(j, " ");

                        EventoAoReceberMensagem(msg, new EventArgs());

                        TrataMensagemRecebida(newBuf);

                        i += tamanhoMsg;// + 8; // soma header("STX"), tamanho(2 bytes) e rodape("ETX")
                    }
                }
            }
        }
    }
}