using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using HamsterAnviz;
using System.Runtime.InteropServices;
//using BiometriaAnviz;
using UNIONCOMM.SDK.UCBioBSP;

namespace LinearHCS
{

    public partial class ControleLinearHCS : Form
    {

   

        public delegate void AoReceberSerial(string Serial, string porta, string leitora);
        public event AoReceberSerial EventoAoReceberSerial;

        public string Endereco
        {
            get
            {
                return txtBox_IP1.Text;
            }

            set
            {
                txtBox_IP1.Text = value;
            }
        }

        string PortadeConexao = null;
        public string Porta
        {
            get
            {
                return  PortadeConexao;
            }
            set
            {
                PortadeConexao = value;
                if (value.ToUpper().IndexOf("COM") == -1)
                {
                    txt_portaTCP_1.Text = value;
                } else
                {
                    cboxPorta.Items.Clear();
                    cboxPorta.Items.Add(value);
                    cboxPorta.SelectedIndex = 0;
                }
                
                
                
            }
        }
        public string TipoConexao { get; set; }

        private System.IO.Ports.SerialPort serialPort1;
        TcpListener tcpServer = new TcpListener(IPAddress.Any, 9767); // Creates a TCP Listener To Listen to Any IPAddress trying to connect to the program with port 1980
        NetworkStream stream; //Creats a NetworkStream (used for sending and receiving data)
        TcpClient client = null; // Creates a TCP Client
        byte[] datalength = new byte[4]; // creates a new byte with length 4 ( used for receivng data's lenght)

        private TcpClient tcpClient1 = null;
        private TcpClient tcpClient5 = null;
        private TcpClient tcpClient6 = null;
        public Boolean tcpClient1Closed = false;
        public Boolean tcpClient5Closed = false;
        public Boolean tcpClient6Closed = false;
        public int toutConectar1 = 0;
        public int toutConectar5 = 0;
        public int toutConectar6 = 0;

        
        private UdpClient udpClient1 = null;
        public Boolean udpClient1Closed = false;

        public IPAddress ipAddress;
        public Int32 port;
        delegate void SetBoolCallback(bool conected);
        delegate void SetButton5Callback(bool conected);
        delegate void SetButton6Callback(bool conected);
        delegate void SetStringCallback(string resposta, string nBytes, string tipoPorta, string porta);
        delegate void SetResposta5Callback(string resposta, string nBytes);
        delegate void SetResposta6Callback(string resposta, string nBytes);
        delegate void SetRetornoCallback(byte retorno);
        delegate void SetCmd79Callback(int codigoRota, byte[] buf, byte indice);
        delegate void SetTrataMensagemRecebidaCallback(byte[] buf, int tamanho, String epOrigem );
        delegate void SetRemotoTeste();
        delegate void SetLabelistView_cadastro_placasRedeCallback(String label, String macString, UInt16 ini, UInt16 fim);

        public controladoraLinear.MemoriaPlaca memoriaPlaca;
        public string arquivoRestore;
        public controladoraLinear pControl = new controladoraLinear();
        public TimeSpan tempoAnterior;
        public controladoraLinear.MemoriaPlaca2[] bufferMsgRecebida = new controladoraLinear.MemoriaPlaca2[255];

        

        byte[] bufUDP = new byte[1500];

        public StringBuilder arquivoString = new StringBuilder();

        public byte[] ipVal = new byte[4];

        public int indiceProgressivo = 0, timeoutCmdProgressivo = 50;

        public void IniciarConexao()
        {
            if (TipoConexao == "Serial")
            {
                btConectarSerial_Click(this, new EventArgs());
            } else if (TipoConexao == "TCP")
            {
                btConectarTCP1_Click(this, new EventArgs());
            } else
            {
                btConectarUDP_Click(this, new EventArgs());
            }
        }

        // teste conversor IP/SERIAL
        public byte estadoBiometria = 0;
        public int toutEstadoBiometria = 0;
        public byte modeloBiometria = 0;
        public byte ultimoCmdBiometria = 0;
        public const int CMD_CADASTRAR_DIGITAL = 0,
                         CMD_BUSCAR_DIGITAL = 1,
                         CMD_APAGAR_DIGITAL = 2,
                         CMD_LER_DIGITAL = 3,
                         CMD_APAGAR_TODOS = 4,
                         CMD_AUTO_SENSE_ON = 5,
                         CMD_AUTO_SENSE_OFF = 6;

        public void FinalizarConexao()
        {
            if (TipoConexao == "Serial")
            {
                btConectarSerial_Click(this, new EventArgs());
            }
            else if (TipoConexao == "TCP")
            {
                btConectarTCP1_Click(this, new EventArgs());
            }
            else
            {
                btConectarUDP_Click(this, new EventArgs());
            }
        }

        // teste ANVIZ/MIAXIS
        BiometriaAnviz anviCmd = new BiometriaAnviz();
        // teste VIRDI
        BiometriaVirdi virdiCmd = new BiometriaVirdi();
        // teste SUPREMA
        BiometriaSuprema supremaCmd = new BiometriaSuprema();
        public int executarNvezes = 0;

        public byte[] frameDisp = new byte[32];

        // TIMER
        public int toutSerial = 0;
        public int indiceSerial = 0;
        public byte[] bufferSerial = new byte[2048];
        public const int TOUT_PROGRESSIVO = 100;
        public int toutGravarTemplateProgressivo = 0;
        public int tout3s = 30;
        
        // BIOMETRIAS
        public int toutConectarHamster = 50;
        string pathAnviz = "";
        UInt16 COMSType = 0;
        Boolean segundoTemplate_1 = false;

        // PLACAS
        public int timeoutCmdStatusColor = 0;
        public IPAddress ipEscolhido = IPAddress.Parse("0.0.0.0");
        Boolean cancelarProcura = false;

        UCBioAPI m_UCBioAPI = new UCBioAPI();

        // CTRL VAGAS
        public Boolean eventoPassagem = false;
        public List<byte[]> listaRestoreBiometria = new List<byte[]>();

        //--------------------------------------------------------------------
        // HAMSTER VIRDI
        //--------------------------------------------------------------------
        public void capturaTemplateVirdi()
        {
            
            short iDeviceID = UCBioAPI.Type.DEVICE_ID.AUTO;

            uint ret = m_UCBioAPI.OpenDevice(iDeviceID);
            if (ret != UCBioAPI.Error.NONE)
            {
                MessageBox.Show("Falha ao abrir o coletor de digitais.");
                return;
            }

            UCBioAPI.Type.HFIR hNewFIR;

            ret = this.m_UCBioAPI.Enroll( null, out hNewFIR, null, 10 , null, null);

            if (ret == UCBioAPI.Error.NONE)
            {
                UCBioAPI.Type.INPUT_FIR inputFIR = new UCBioAPI.Type.INPUT_FIR();
                inputFIR.Form = UCBioAPI.Type.INPUT_FIR_FORM.HANDLE;
                inputFIR.InputFIR.FIRinBSP = hNewFIR;
            }
            else { this.m_UCBioAPI.CloseDevice(iDeviceID); return; }

            //UCBioAPI.Type.TEMPLATE_TYPE nType = UCBioAPI.Type.TEMPLATE_TYPE.SIZE400;
            ////uint ret;
            //UCBioAPI.Type.TEMPLATE_FORMAT nFormat = UCBioAPI.Type.TEMPLATE_FORMAT.TEMPLATE_FORMAT_UNION400;
            ////Control form capture VIRDI
            //UCBioAPI.Type.WINDOW_OPTION winOption = new UCBioAPI.Type.WINDOW_OPTION();

            //byte[] templateExport;
            //byte[] templateExport2;

            /*
            m_hEnrolledFIR = hNewFIR;
            ret = this.ExportTemplate(nType, out templateExport, out templateExport2);
            */ 
            
            this.m_UCBioAPI.CloseDevice(iDeviceID);
        }

        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public ControleLinearHCS()
        {
            InitializeComponent();
        }        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btConectarTCP1_Click(object sender, EventArgs e)
        {
            try
            {
                tcpClient1Closed = true;
                tcpClient1.Close();
            }
            catch { };

            try
            {
                tcpClient1 = new TcpClient(AddressFamily.InterNetwork);
            }
            catch { };

            toutConectar1 = 1;
        }             
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void SetResposta(string resposta, string nBytes, string tipoPorta, string porta )
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.txtResposta.InvokeRequired)
            {
                SetStringCallback d1 = new SetStringCallback(SetResposta);
                this.Invoke(d1, new object[] { resposta, nBytes, tipoPorta, porta });
            }
            else
            {
                lblTamanhoResposta.Text = nBytes;
                txtResposta.Text = resposta;

                if (form_sniffer.IsHandleCreated == true)
                {
                    string t = "RX " + tipoPorta + ":" + porta + "[" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                    rt_sniffer.Text += t + resposta + "\r\n\r\n";
                    form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                }

            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void SetResposta5(string resposta, string nBytes)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.txtResposta.InvokeRequired)
            {
                SetResposta5Callback d1 = new SetResposta5Callback(SetResposta5);
                this.Invoke(d1, new object[] { resposta, nBytes });
            }
            else
            {
                txtRspCmdPorta5.Text = resposta;
                lblUltimaResposta5.Text = "=" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond);

                if (toutEstadoBiometria > 0)
                {
                    
                    trataEstadoBiometria( ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void SetResposta6(string resposta, string nBytes)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.txtResposta.InvokeRequired)
            {
                SetResposta6Callback d1 = new SetResposta6Callback(SetResposta6);
                this.Invoke(d1, new object[] { resposta, nBytes });
            }
            else
            {
                txtRspCmdPorta6.Text = resposta;
                lblUltimaResposta6.Text = "=" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond);

                if (toutEstadoBiometria > 0)
                {
                    
                    trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void mostraResposta( byte[] buf, String tipoPorta, String epOrigem )
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
                    if (verificaChecksum(buf, i + 5, tamanhoMsg) == true)
                    {
                        byte[] newBuf = new byte[tamanhoMsg];
                        for (int k = 0; k < tamanhoMsg; k++) newBuf[k] = buf[i + 5 + k];

                        // mostra resposta    
                        byte[] newBuf2 = new byte[tamanhoMsg + 8];
                        for (int k = 0; k < (tamanhoMsg + 8); k++) newBuf2[k] = buf[i + k];
                        string nBytes = Convert.ToString(tamanhoMsg + 8);
                        string msg = BytesToHexString(newBuf2);
                        msg = msg.Substring(0, (tamanhoMsg + 8) * 2);
                        for (int j = 2; j < (tamanhoMsg + 8) * 3; j += 3) msg = msg.Insert(j, " ");

                        if (tipoPorta != "SERIAL")
                        {
                            SetResposta(msg, nBytes, tipoPorta, epOrigem.Substring(epOrigem.IndexOf(':')+1, epOrigem.Length - epOrigem.IndexOf(':') - 1));
                        }
                        else
                        {
                            SetResposta( msg, nBytes, tipoPorta, epOrigem );
                        }
                        
                        trataMensagemRecebida(newBuf, tamanhoMsg , epOrigem );

                        i += tamanhoMsg;// + 8; // soma header("STX"), tamanho(2 bytes) e rodape("ETX")
                    }                    
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void mostraResposta_TCP5(byte[] buf)
        {
            int numberOfBytesRead = buf.Length;
            byte[] myReadBuffer = buf;

            // mostra resposta    
            string nBytes = Convert.ToString(numberOfBytesRead);
            string msg = BytesToHexString(myReadBuffer);
            msg = msg.Substring(0, numberOfBytesRead * 2);
            int i;
            for (i = 2; i < numberOfBytesRead * 3; i += 3) msg = msg.Insert(i, " ");
            SetResposta5(msg, nBytes);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void mostraResposta_TCP6(byte[] buf)
        {
            int numberOfBytesRead = buf.Length;
            byte[] myReadBuffer = buf;

            // mostra resposta    
            string nBytes = Convert.ToString(numberOfBytesRead);
            string msg = BytesToHexString(myReadBuffer);
            msg = msg.Substring(0, numberOfBytesRead * 2);
            int i;
            for (i = 2; i < numberOfBytesRead * 3; i += 3) msg = msg.Insert(i, " ");
            SetResposta6(msg, nBytes);
        }        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btConectarUDP_Click(object sender, EventArgs e)
        {
            Conectar_UDP1();
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void Conectar_UDP1()
        {
            if (btConectarUDP1.Text == "Conectar UDP")
            {
                try
                {
                    ConnectToServer_UDP1();
                    if( udpClient1Closed == false )
                    {
                        btConectarUDP1.Text = "Desconectar";
                        byte[] buf = pControl.lerSetup();
                        enviarMensagem(buf);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha na conexão");
                }
            }
            else if (btConectarUDP1.Text == "Desconectar")
            {
                try
                {
                    udpClient1Closed = true;
                    udpClient1.Close();
                    btConectarUDP1.Text = "Conectar UDP";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha ao fechar o socket!");
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void ConnectToServer_UDP1()
        {
            try
            {
                udpClient1Closed = true;

                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToUInt16(txtPorta_UDP1.Text));
                udpClient1 = new UdpClient();
                udpClient1.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient1.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
                //Start the async connect operation
                udpClient1.Client.Bind( RemoteIpEndPoint );
                udpClient1.BeginReceive(new AsyncCallback(ReadCallback_UDP1), null);
                
                udpClient1Closed = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        /// Callback for Read operation
        private void ReadCallback_UDP1(IAsyncResult result)
        {
            byte[] buffer = new byte[1];
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToUInt16(txt_portaTCP_1.Text));

            try
            {                
                buffer = udpClient1.EndReceive(result, ref RemoteIpEndPoint);
                udpClient1.BeginReceive(new AsyncCallback(ReadCallback_UDP1), udpClient1.Client );
            }
            catch
            {
                return;
            }

            if ((RemoteIpEndPoint.Address.ToString() == IPAddress.Parse(txtBox_IP1.Text).ToString()) ||
                (RemoteIpEndPoint.Address.ToString() == "100.0.0.76" ) ) 
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        //txtResposta.Text = BytesToHexString(buffer);
                        mostraResposta( buffer, "UDP", txtBox_IP1.Text + ":" + txtPorta_UDP1.Text );
                    });                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }        
        //-------------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------------
        private static byte[] hexStringToByteArray(String texto, int tamanho)
        {
            byte[] val = new byte[tamanho/2];
            int i = 0, j = 0;
            byte numero = 0;

            for (i = 0; i < tamanho / 2; i++) val[i] = 0;

            string textoTemp = texto.ToUpper();
            bool invalido = false;

            try
            {
                for (i = 0; i < tamanho; i++ )
                {
                    if (textoTemp[i] != ' ')
                    {
                        numero = (byte)textoTemp[i];
                        if ((textoTemp[i] >= '0') && (textoTemp[i] <= '9'))
                        {
                            numero = (byte)(numero - 0x30);
                            val[j] = (byte)(numero << 4);
                        }
                        else if ((textoTemp[i] >= 'A') && (textoTemp[i] <= 'F'))
                        {
                            numero = (byte)(numero - 0x41 + 0x0A);
                            val[j] = (byte)(numero << 4);
                        }
                        else
                        {
                            invalido = true; // ou espaço
                        }

                        if (invalido == false)
                        {
                            numero = (byte)textoTemp[i + 1];
                            if ((textoTemp[i + 1] >= '0') && (textoTemp[i + 1] <= '9'))
                            {
                                numero = (byte)(numero - 0x30);
                                val[j] += (byte)numero;
                            }
                            else
                            {
                                numero = (byte)(numero - 0x41 + 0x0A);
                                val[j] += (byte)numero;
                            }
                            j++;
                            i++;
                        }
                    }
                    invalido = false;
                }
            }
            catch
            {
                MessageBox.Show("Valor Inválido!!");
            }
            return val;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public const int __AUTALIZA_ARQUIVO_BIOMETRIA = 1;
        public const int __CONTROLE_VAGAS_ID = 1;

        public void Carga(object sender, EventArgs e)
        {
            int i;

            //if (__AUTALIZA_ARQUIVO_BIOMETRIA == 0) { groupBox26.Visible = false; }
            //if (__CONTROLE_VAGAS_ID == 0) { checkBoxDispositivos_controleVagasId.Visible = false; }

            timer1.Enabled = true;
            timer1.Start();
            timer2.Enabled = true;
            timer2.Start();

            cboxCodigoGrupoLeitoras.Items.Clear();

            for (i = 1; i <= controladoraLinear.N_LINES_HABILITACAO; i++)
            {
                cbox_codigoRota.Items.Add(Convert.ToString(i));
                cboxRota.Items.Add(Convert.ToString(i));
                comboBoxCtrlVagasHab_Rota1.Items.Add(Convert.ToString(i));
                comboBoxCtrlVagasHab_Rota2.Items.Add(Convert.ToString(i));
                cboxCodigoGrupoLeitoras.Items.Add(Convert.ToString(i));
                cboxCodigoRota2.Items.Add(Convert.ToString(i));
                
            }

            for (i = 0; i < 512; i++)
            {
                comboBox_nPacotes.Items.Add( i.ToString());
            }
            comboBox_nPacotes.SelectedIndex = 0;



            for (i = 1; i <= controladoraLinear.N_ENDERECOS; i++)
            {
                cboxEndereçoPlaca_SETUP_GERAL.Items.Add(Convert.ToString(i) + "(" + (i-1).ToString() + ")" );
                comboBoxEntradaSaida_endereco.Items.Add(Convert.ToString(i) + "(" + (i - 1).ToString() + ")" );
                cboxEnderecoGL.Items.Add(Convert.ToString(i) + "(" + (i-1).ToString() + ")" );
                cboxEnderecoRota2.Items.Add(Convert.ToString(i) + "(" + (i-1).ToString() + ")" );
            }

            for (i = 1; i <= controladoraLinear.N_LINES_FERIADO; i++)
            {
                cboxIndiceFeriado.Items.Add(Convert.ToString(i));
            }

            for (i = 1; i <= controladoraLinear.N_LINES_JORNADAS; i++)
            {
                cboxCodigoJornadaJFT.Items.Add(Convert.ToString(i));
                cboxJornadaL1.Items.Add(Convert.ToString(i));
                cboxJornadaL2.Items.Add(Convert.ToString(i));
                cboxJornadaL3.Items.Add(Convert.ToString(i));
                cboxJornadaL4.Items.Add(Convert.ToString(i));
                cboxJornadaL1_ROTA2.Items.Add(Convert.ToString(i));
                cboxJornadaL2_ROTA2.Items.Add(Convert.ToString(i));
                cboxJornadaL3_ROTA2.Items.Add(Convert.ToString(i));
                cboxJornadaL4_ROTA2.Items.Add(Convert.ToString(i));
            }

            for (i = 1; i <= controladoraLinear.N_LINES_TURNOS; i++)
            {
                cboxCodigoTurno.Items.Add(Convert.ToString(i));
                cboxSegunda.Items.Add(Convert.ToString(i));
                cboxTerca.Items.Add(Convert.ToString(i));
                cboxQuarta.Items.Add(Convert.ToString(i));
                cboxQuinta.Items.Add(Convert.ToString(i));
                cboxSexta.Items.Add(Convert.ToString(i));
                cboxSabado.Items.Add(Convert.ToString(i));
                cboxDomingo.Items.Add(Convert.ToString(i));
                cboxFeriados.Items.Add(Convert.ToString(i));
            }

            for (i = 0; i < 256; i++)
            {
                comboBoxCtrlVagasPreset.Items.Add(Convert.ToString(i));
                comboBoxCtrlVagasAtual.Items.Add(Convert.ToString(i));
            }


            for ( i = 1; i <= controladoraLinear.N_LINES_HABILITACAO; i++ )
            {
                ListViewItem novoEvento = new ListViewItem();
                novoEvento.Text = i.ToString("000");
                novoEvento.SubItems.Add("000");
                novoEvento.SubItems.Add("000");
                novoEvento.Tag = i.ToString("000");
                listView_vagas.Items.Add(novoEvento);
            }

            byte bloco = 0;
            for (i = 0; i < 8192; i++)
            {
                String sBloco = null;
                ListViewItem novoEvento = new ListViewItem();
                novoEvento.Text = i.ToString("0000");

                if (bloco < 26)
                {
                    sBloco = (char)(bloco + 'A') + " ";
                }
                else
                {

                    sBloco = (bloco - 25).ToString();
                }

                novoEvento.SubItems.Add( sBloco + i.ToString("0000") );
                novoEvento.SubItems.Add("0");
                novoEvento.SubItems.Add("0");
                listView_controleVagasId.Items.Add(novoEvento);
                bloco++;
                if (bloco > 99) bloco = 0;
            }
            

            comboBoxCtrlVagasHab_Rota2.Items.Add("FFFF (todos)");

            comboBoxCtrlVagasHab_operacao.SelectedIndex = 0;
            comboBoxCtrlVagasAtual.SelectedIndex = 0;
            comboBoxCtrlVagasPreset.SelectedIndex = 0;
            comboBoxCtrlVagasHab_operacao_131.SelectedIndex = 0;

            comboBoxCtrlVagas_tipo.SelectedIndex = 0;

            comboBoxCtrlVagasHab_Rota1.SelectedIndex = 0;
            comboBoxCtrlVagasHab_Rota2.SelectedIndex = comboBoxCtrlVagasHab_Rota2.Items.Count - 1;

            cboxModeloBiometria.SelectedIndex = 0;

            cboxTempoMsgDisplay.SelectedIndex = 0;
            cboxOperacaoDisplay.SelectedIndex = 0;
            cboxEventoDisplay.SelectedIndex = 0;
            cboxIndiceFeriado.SelectedIndex = 0;

            cboxEnderecoRota2.SelectedIndex = 0;
            cboxOperacaoRota2.SelectedIndex = 0;
            cboxCodigoRota2.SelectedIndex = 0;

            cboxOperacao_parametros.SelectedIndex = 0;
            cboxParametro.SelectedIndex = 0;

            cboxOperacoesBM1.SelectedIndex = 0;
            cboxIndiceBM.SelectedIndex = 0;
            cboxModuloBio.SelectedIndex = 4;

            cboxSegunda.SelectedIndex = 0;
            cboxTerca.SelectedIndex = 0;
            cboxQuarta.SelectedIndex = 0;
            cboxQuinta.SelectedIndex = 0;
            cboxSexta.SelectedIndex = 0;
            cboxSabado.SelectedIndex = 0;
            cboxDomingo.SelectedIndex = 0;
            cboxFeriados.SelectedIndex = 0;

            cboxTipoDispositivo.SelectedIndex = 0;
            cboxRota.SelectedIndex = 0;
            cboxTipo2.SelectedIndex = 0;
            cboxAntipassback.SelectedIndex = 0;

            cboxIPHost.SelectedIndex = 0;

            comboBoxEntradaSaida_tipoSaida.SelectedIndex = 0;
            comboBoxEntradaSaida_endereco.SelectedIndex = 0;
            comboBoxEntradaSaida_saida.SelectedIndex = 0;

            cboxMarcaEvento.SelectedIndex = 0;
            cboxModoRemoto.SelectedIndex = 0;

            cboxModoRemoto.SelectedIndex = 0;
            cboxContadorAtualVagas.SelectedIndex = 0;

            cboxServidorDDNS.SelectedIndex = 0;

            cboxTempoPanico.SelectedIndex = 0;
            cboxSinalizaSaida.SelectedIndex = 0;

            cboxGiroCatraca.SelectedIndex = 0;
            cboxCatraca2010.SelectedIndex = 0;

            cboxBrUart1.SelectedIndex = 0;
            cboxBrUart2.SelectedIndex = 0;
            cboxBrUart3.SelectedIndex = 0;            
            cboxFuncaoUart2.SelectedIndex = 0;
            cboxFuncaoUart3.SelectedIndex = 0;
            cboxStopBitsUart1.SelectedIndex = 0;
            cboxParidadeUart1.SelectedIndex = 0;

            cboxBrCan_SETUP_GERAL.SelectedIndex = 0;
            cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex = 0;
            cboxModoOperacao.SelectedIndex = 0;
            cboxAutonomia.SelectedIndex = 0;
            cboxTipoDispL1.SelectedIndex = 0;
            cboxTipoDispL2.SelectedIndex = 0;
            cboxTipoDispL3.SelectedIndex = 0;
            cboxTipoDispL4.SelectedIndex = 0;

            cboxJornadaL1_ROTA2.SelectedIndex = 0;
            cboxJornadaL2_ROTA2.SelectedIndex = 0;
            cboxJornadaL3_ROTA2.SelectedIndex = 0;
            cboxJornadaL4_ROTA2.SelectedIndex = 0;

            cboxFuncaoLeitora1.SelectedIndex = 0;
            cboxFuncaoLeitora2.SelectedIndex = 0;
            cboxFuncaoLeitora3.SelectedIndex = 0;
            cboxFuncaoLeitora4.SelectedIndex = 0;

            cboxEventoS1.SelectedIndex = 0;
            cboxEventoS2.SelectedIndex = 0;
            cboxEventoS3.SelectedIndex = 0;
            cboxEventoS4.SelectedIndex = 0;

            cboxOperacaoPacote.SelectedIndex = 0;
            cboxJornadaL1.SelectedIndex = 0;
            cboxJornadaL2.SelectedIndex = 0;
            cboxJornadaL3.SelectedIndex = 0;
            cboxJornadaL4.SelectedIndex = 0;
            cboxEnderecoGL.SelectedIndex = 0;
            cboxCodigoGrupoLeitoras.SelectedIndex = 0;
            cboxCodigoTurno.SelectedIndex = 0;
            cboxCodigoJornadaJFT.SelectedIndex = 0;
            cbox_codigoRota.SelectedIndex = 0;
            comboBoxEntradaSaida_tipoSaida.SelectedIndex = 0;
            comboBoxEntradaSaida_endereco.SelectedIndex = 0;
            comboBoxEntradaSaida_saida.SelectedIndex = 0;
            cboxModoRemoto.SelectedIndex = 0;
            cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex = 0;
            
            cboxTipo2.SelectedIndex = 0;

            comboBox_setup_setupCatraca_tipoCatraca.SelectedIndex = 0;

            arquivoRestore = null;



            controladoraLinear.FLAGS_CFG_E flagsE = new controladoraLinear.FLAGS_CFG_E();
            controladoraLinear.FLAGS_CFG_1 flags1= new controladoraLinear.FLAGS_CFG_1();
            controladoraLinear.FLAGS_CFG_2 flags2= new controladoraLinear.FLAGS_CFG_2();
            controladoraLinear.FLAGS_CFG_3 flags3= new controladoraLinear.FLAGS_CFG_3();
            controladoraLinear.FLAGS_CFG_4 flags4= new controladoraLinear.FLAGS_CFG_4();
            controladoraLinear.FLAGS_CFG_5 flags5= new controladoraLinear.FLAGS_CFG_5();
            controladoraLinear.FLAGS_CFG_6 flags6= new controladoraLinear.FLAGS_CFG_6();
            controladoraLinear.FLAGS_CFG_7 flags7= new controladoraLinear.FLAGS_CFG_7();
            controladoraLinear.FLAGS_CFG_8 flags8= new controladoraLinear.FLAGS_CFG_8();
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
            byte[]  feriados = new byte[controladoraLinear.N_LINES_FERIADO * controladoraLinear.SIZE_OF_FERIADO];
            byte[,] labelsRotas = new byte[controladoraLinear.N_LINES_HABILITACAO, 8 ];
            byte[,] mensagensDisplay = new byte[12, 32];

            controladoraLinear.s_setup setup = new controladoraLinear.s_setup(0, 0, 0, 0, 0, 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0,
                                0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.SIZE_OF_BYTES_RESERVADOS], 0, 0, 0, 0,
                                0, 0, 0, 0, 0, 0, new byte[controladoraLinear.SIZE_OF_DDNS_USUARIO], new byte[controladoraLinear.SIZE_OF_DDNS_SENHA], new byte[controladoraLinear.SIZE_OF_DDNS_DEVICE],
                                new byte[controladoraLinear.SIZE_OF_USUARIO_LOGIN], new byte[controladoraLinear.SIZE_OF_SENHA_LOGIN], new byte[controladoraLinear.SIZE_OF_DNS_HOST], new byte[controladoraLinear.SIZE_OF_DDNS_HOST], 0, new byte[controladoraLinear.N_LEITORAS], 0,
                                0, new byte[4], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                flagsE, flags1, flags2, flags3, flags4, flags5, flags6, flags7, flags8, flags9, flags10, flags11, flags12, flags13, flags14, flags15, flags16, flags17, flags18, new byte[2], flags19, 0) ;

            controladoraLinear.MemoriaPlaca memoriaPlacaTemp = new controladoraLinear.MemoriaPlaca(dispositivos, setup, rotas, gruposLeitoras, jornadas, turnos, feriados, labelsRotas, mensagensDisplay);
            for (i = 0; i < 255; i++) bufferMsgRecebida[i] = new controladoraLinear.MemoriaPlaca2(null, IPAddress.Any, 0, null, 0, new byte[256, 1500]);

            memoriaPlaca = memoriaPlacaTemp;
            DateTime result = DateTime.Today.Add(TimeSpan.FromDays(3650)); // + 10 anos
            dateTimePicker2.Value = result;

            txtIPDestino.Text = GetLocalIP();

            inicializaPortaSerial();

            comboBoxEntradaSaida_Tipo.SelectedIndex = 0;
            comboBoxEntradaSaida_Antipassback.SelectedIndex = 0;
            comboBoxEntradasSaidas_origemAcionamento.SelectedIndex = 0;

            // PLACAS
            IPAddress ipTemp = IPAddress.Parse( GetLocalIP() );
            byte[] ipBytes = ipTemp.GetAddressBytes();
            

            comboBoxEvento_indiceEvento.Items.Clear();
            for (i = 0; i <= controladoraLinear.N_LINES_LOG; i++)
            {
                comboBoxEvento_indiceEvento.Items.Add(i.ToString("0000"));
                comboBoxUltimoEnderecoEventoRecebido.Items.Add(i.ToString("0000"));
            }
            comboBoxEvento_indiceEvento.SelectedIndex = 0;
            comboBoxEvento_operacaoEvento.SelectedIndex = 0;
            comboBoxUltimoEnderecoEventoRecebido.SelectedIndex = 0;

            for (i = 0; i < 256; i++)
            {
                comboBoxIndiceEventoPacote.Items.Add(i.ToString("000"));
            }
            comboBoxIndiceEventoPacote.SelectedIndex = 0;

            for (i = 1; i < 16384; i++) comboBoxEntradasSaidas_nVezes.Items.Add(i.ToString("00000"));
            comboBoxEntradasSaidas_nVezes.SelectedIndex = 0;

            // setup
            comboBox_SETUP_setor.SelectedIndex = 0;

            // sd card
            cboxArquivo.SelectedIndex = 0;
            cboxDataArquivo.Enabled = false;
            listView_SDCARD_MIDIA_Clear();

            // controle de vagas id
            comboBox_controleVagas_vagasId.SelectedIndex = 0;

            // panico
            comboBox_eventos_saidaCancelarPanico.SelectedIndex = 0;

            comboBoxNivelSegurancaBiometria.SelectedIndex = 0;

            //rs485
            comboBox_setup_rs485_funcaoLeitoraRS485_1.SelectedIndex = 0;
            comboBox_setup_rs485_funcaoLeitoraRS485_2.SelectedIndex = 0;
            comboBox_setup_rs485_funcaoLeitoraRS485_3.SelectedIndex = 0;
            comboBox_setup_rs485_funcaoLeitoraRS485_4.SelectedIndex = 0;
            /*
            comboBox_setup_setupRS485_potencia_L1.SelectedIndex = 0;
            comboBox_setup_setupRS485_potencia_L2.SelectedIndex = 0;
            comboBox_setup_setupRS485_potencia_L3.SelectedIndex = 0;
            comboBox_setup_setupRS485_potencia_L4.SelectedIndex = 0;
             * */

            comboBox_setup_rs485_baudrate_rs485_1.SelectedIndex = 0;
            comboBox_setup_rs485_baudrate_rs485_2.SelectedIndex = 0;
            comboBox_setup_rs485_baudrate_rs485_3.SelectedIndex = 0;
            comboBox_setup_rs485_baudrate_rs485_4.SelectedIndex = 0;


            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.button_deviceManager, "Gerenciador de Dispositivos");
            toolTip1.SetToolTip(this.button_atualizarPortasUsb, "Atualizar Portas COM - USB");
            toolTip1.SetToolTip(this.button_desktopReader, "Desktop Reader - Modo Teclado");
            toolTip1.SetToolTip(this.button_sniffer, "Sniffer (Mostra o que é enviado e recebido)");

            comboBox_setup_setupCatraca_retardoDesligarSolenoide.SelectedIndex = 0;

            cBoxAjusteRelogio.SelectedIndex = 0;
        }
        //--------------------------------------------------------------------
        //
        //-------------------------------------------------------------------
        public Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private void TcpSever_clientAccepted_callback(IAsyncResult ar)
        {
            try
            {
                clientSocket = (Socket)ar;
                
                tcpServer.BeginAcceptTcpClient(TcpSever_clientAccepted_callback, null);

            }
            catch { }
        }
        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public string GetLocalIP()
        {
            string _IP = null;
            try
            {                
                System.Net.IPHostEntry _IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

                foreach (System.Net.IPAddress _IPAddress in _IPHostEntry.AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        _IP = _IPAddress.ToString();

                    }
                }
                return _IP;
            }
            catch { return _IP;  };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void verificaMensagemRecebida(byte[] buf, int tamanho, IPAddress ipPlaca, int porta )
        {
            try
            {
                int i,j;
                int tamanhoMsg;
                
                for (i = 0; i < tamanho; i++ )
                {
                    if ((buf[i    ] == 'S') &&
                        (buf[i + 1] == 'T') &&
                        (buf[i + 2] == 'X'))
                    {
                        tamanhoMsg = (buf[i + 3] << 8) + buf[i + 4];
                        if (verificaChecksum(buf, i + 5, tamanhoMsg) == true)
                        {
                            int k;
                            byte[] newBuf = new byte[tamanhoMsg];
                            for (k = 0; k < tamanhoMsg; k++) newBuf[k] = buf[i + 5 + k];

                            String epOrigem = ipPlaca.ToString() + porta.ToString();
                            
                            trataMensagemRecebida(newBuf, tamanhoMsg, epOrigem );
                            

                            i += tamanhoMsg + 8; // soma header("STX"), tamanho(2 bytes) e rodape("ETX")

                            byte[] v = ipPlaca.GetAddressBytes();
                            byte msbIP = v[3];

                            // guarda mensagem recebida                        
                            if (bufferMsgRecebida[msbIP].indiceMsgRecebida > 255) bufferMsgRecebida[msbIP].indiceMsgRecebida = 0;

                            for (j = 0; j < 1500; j++)
                            {
                                if (j < tamanhoMsg)
                                {
                                    bufferMsgRecebida[msbIP].mensagemRecebida[bufferMsgRecebida[msbIP].indiceMsgRecebida, j] = buf[j];
                                }
                                else
                                {
                                    bufferMsgRecebida[msbIP].mensagemRecebida[bufferMsgRecebida[msbIP].indiceMsgRecebida, j] = 0;
                                }
                            }

                            bufferMsgRecebida[msbIP].indiceMsgRecebida++;
                            if (bufferMsgRecebida[msbIP].indiceMsgRecebida > 255) bufferMsgRecebida[msbIP].indiceMsgRecebida = 0;

                            bufferMsgRecebida[msbIP].port = porta;
                        }
                        else
                        {
                            lblErro.Text = "Erro de checksum";
                        }
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        // 
        //--------------------------------------------------------------------
        public bool verificaChecksum( byte[] buf, int indice, int tamanho)
        {
            try
            {
                int i;
                byte soma = 0;
                for (i = 0; i < tamanho - 1; i++) soma += buf[indice + i];
                if (soma == buf[ indice + tamanho - 1 ]) return true;
                return false;
            }
            catch { return false; };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void mostraRetorno(byte retorno)
        {
            try
            {
                // InvokeRequired required compares the thread ID of the 
                // calling thread to the thread ID of the creating thread. 
                // If these threads are different, it returns true. 
                if (this.lblRetorno.InvokeRequired)
                {
                    SetRetornoCallback d1 = new SetRetornoCallback(mostraRetorno);
                    this.Invoke(d1, new object[] { retorno });
                }
                else
                {
                    this.lblRetorno.Text = null;

                    switch (retorno)
                    {
                        case controladoraLinear.RT_OK: this.lblRetorno.Text = "RT_OK"; break;
                        case controladoraLinear.RT_MEMORIA_CHEIA: this.lblRetorno.Text = "RT_MEMORIA_CHEIA"; break;
                        case controladoraLinear.RT_DISP_JA_APRENDIDO: this.lblRetorno.Text = "RT_DISP_JA_APRENDIDO"; break;
                        case controladoraLinear.RT_DISP_NAO_ENCONTRADO: this.lblRetorno.Text = "RT_DISP_NAO_ENCONTRADO"; break;
                        case controladoraLinear.RT_SEM_MAIS_EVENTOS: this.lblRetorno.Text = "RT_SEM_MAIS_EVENTOS"; break;
                        case controladoraLinear.RT_FIM_DO_ARQUIVO: this.lblRetorno.Text = "RT_FIM_DO_ARQUIVO"; break;
                        case controladoraLinear.RT_ESTOURO_BUFFER_INTERNO: this.lblRetorno.Text = "RT_ESTOURO_BUFFER_INTERNO"; break;
                        case controladoraLinear.RT_CONFLITO_DE_PORTAS: this.lblRetorno.Text = "RT_CONFLITO_DE_PORTAS"; break;
                        case controladoraLinear.RT_ERRO_NO_TAMANHO_PACOTE: this.lblRetorno.Text = "RT_ERRO_NO_TAMANHO_PACOTE"; break;
                        case controladoraLinear.RT_ERRO_DE_CHECKSUM: this.lblRetorno.Text = "RT_ERRO_DE_CHECKSUM"; break;
                        case controladoraLinear.RT_ERRO_NO_MODO_OPERACAO: this.lblRetorno.Text = "RT_ERRO_NO_MODO_OPERACAO"; break;
                        case controladoraLinear.RT_ERRO_ESCRITA_LEITURA: this.lblRetorno.Text = "RT_OKRT_ERRO_ESCRITA_LEITURA"; break;
                        case controladoraLinear.RT_ERRO_FORA_DO_LIMITE_DO_DISPOSITIVO: this.lblRetorno.Text = "RT_ERRO_FORA_DO_LIMITE_DO_DISPOSITIVO"; break;
                        case controladoraLinear.RT_ERRO_TIPO_DIFERENTE: this.lblRetorno.Text = "RT_ERRO_TIPO_DIFERENTE"; break;
                        case controladoraLinear.RT_ERRO_OPERACAO_INVALIDA: this.lblRetorno.Text = "RT_ERRO_OPERACAO_INVALIDA"; break;
                        case controladoraLinear.RT_ERRO_DISPOSITIVO_DIFERENTE: this.lblRetorno.Text = "RT_ERRO_DISPOSITIVO_DIFERENTE"; break;
                        case controladoraLinear.RT_ERRO_INDICE_INVALIDO: this.lblRetorno.Text = "RT_ERRO_INDICE_INVALIDO"; break;
                        case controladoraLinear.RT_ERRO_DE_SINTAXE: this.lblRetorno.Text = "RT_ERRO_DE_SINTAXE"; break;
                        case controladoraLinear.RT_ERRO_VALOR_INVALIDO: this.lblRetorno.Text = "RT_ERRO_VALOR_INVALIDO"; break;
                        case controladoraLinear.RT_ERRO_SERIAL_INVALIDO: this.lblRetorno.Text = "RT_ERRO_SERIAL_INVALIDO"; break;
                        case controladoraLinear.RT_ERRO_SETOR_DIFERENTE: this.lblRetorno.Text = "RT_ERRO_SETOR_DIFERENTE"; break;

                        case controladoraLinear.RT_ERRO_ENDERECO_DIFERENTE: this.lblRetorno.Text = "RT_ERRO_ENDERECO_DIFERENTE"; break;

                        case controladoraLinear.RT_ERRO_BIOMETRIA_1_SUCESSO_BIOMETRIA_2: this.lblRetorno.Text = "RT_ERRO_BIOMETRIA_1_SUCESSO_BIOMETRIA_2"; break;
                        case controladoraLinear.RT_ERRO_BIOMETRIA_2_SUCESSO_BIOMETRIA_1: this.lblRetorno.Text = "RT_ERRO_BIOMETRIA_2_SUCESSO_BIOMETRIA_1"; break;
                        case controladoraLinear.RT_ERRO_BIOMETRIA_1_E_2: this.lblRetorno.Text = "RT_ERRO_BIOMETRIA_1_E_2"; break;
                        case controladoraLinear.RT_ERRO_SEM_RESPOSTA_DO_MODULO: this.lblRetorno.Text = "RT_ERRO_SEM_RESPOSTA_DO_MODULO"; break;
                            

                        case controladoraLinear.RT_ERRO_FECHAR_ARQUIVO: this.lblRetorno.Text = "RT_ERRO_FECHAR_ARQUIVO"; break;
                        case controladoraLinear.RT_ERRO_MIDIA_LEITURA: this.lblRetorno.Text = "RT_ERRO_MIDIA_LEITURA"; break;
                        case controladoraLinear.RT_ERRO_MIDIA_ESCRITA: this.lblRetorno.Text = "RT_ERRO_MIDIA_ESCRITA"; break;
                        case controladoraLinear.RT_ERRO_SEM_ARQUIVO_ABERTO: this.lblRetorno.Text = "RT_ERRO_SEM_ARQUIVO_ABERTO"; break;
                        case controladoraLinear.RT_ERRO_ARQUIVO_ABERTO: this.lblRetorno.Text = "RT_ERRO_ARQUIVO_ABERTO"; break;
                        case controladoraLinear.RT_ERRO_EOF: this.lblRetorno.Text = "RT_ERRO_EOF"; break;
                        case controladoraLinear.RT_ERRO_MIDIA: this.lblRetorno.Text = "RT_ERRO_MIDIA"; break;
                        case controladoraLinear.RT_ERRO_TIMEOUT: this.lblRetorno.Text = "RT_ERRO_TIMEOUT"; break;

                        case controladoraLinear.RT_ERRO_ARQUIVO: this.lblRetorno.Text = "RT_ERRO_ARQUIVO"; break;
                        case controladoraLinear.RT_ERRO_PASTA: this.lblRetorno.Text = "RT_ERRO_PASTA"; break;
                        case controladoraLinear.RT_ERRO_SD: this.lblRetorno.Text = "RT_ERRO_SD"; break;
                        case controladoraLinear.RT_ERRO: this.lblRetorno.Text = "RT_ERRO"; break;
                    }

                    int hora, minuto, segundo, miles;
                    hora = DateTime.Now.Hour;
                    minuto = DateTime.Now.Minute;
                    segundo = DateTime.Now.Second;
                    miles = DateTime.Now.Millisecond;

                    TimeSpan tempoAtual = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                
                    this.lblRetorno.Text += "     " + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond);

                    TimeSpan dt = tempoAnterior.Subtract(tempoAtual);

                    this.lblRetorno.Text += "  (" + Convert.ToString(Math.Abs(dt.Hours)) + ':' 
                                                + Convert.ToString(Math.Abs(dt.Minutes)) + ':'
                                                + Convert.ToString(Math.Abs(dt.Seconds)) + ':'
                                                + Convert.ToString(Math.Abs(dt.Milliseconds)) + ")";

                    tempoAnterior = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataMensagemRecebida(byte[] buf, int tamanho, String epOrigem )
        {
            try
            {
                if (this.lblRetorno.InvokeRequired)
                {
                    SetTrataMensagemRecebidaCallback d1 = new SetTrataMensagemRecebidaCallback(trataMensagemRecebida);
                    this.Invoke(d1, new object[] { buf, tamanho, epOrigem });
                }
                else
                {
                    byte cmd = buf[1];
                    byte retorno = buf[2];
                
                    switch (cmd)
                    {
                        //--------------------------------------------------------------
                        // 79:	Editar habilitação das leitoras	
                        // CMD: 00 + 4F + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação (32 bytes)> + cs [37 bytes]
                        // RSP: 00 + 4F + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + cs         [6 bytes]
                        case 79:
                            break;
                        //--------------------------------------------------------------
                        // 80:	Gravar dispositivo	
                        // CMD: 00 + 50 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs          [36 bytes]
                        // RSP: 00 + 50 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs         [11 bytes]
                        case 80:
                            break;
                        //-------------------------------------------------------------- 
                        // 81	Editar dispositivo	
                        // CMD: 00 + 51 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs          [36 bytes]
                        // RSP: 00 + 51 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs        [11 bytes]
                        case 81:
                            break;
                        //--------------------------------------------------------------
                        // 82:	Apagar dispositivo
                        // CMD: 00 + 52 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs                   [10 bytes]
                        // RSP: 00 + 52 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs       [11 bytes]
                        case 82:
                            break;
                        //--------------------------------------------------------------
                        // 83:	Formata memória	
                        // CMD: 00 + 53 + cs          [3 bytes]
                        // RSP: 00 + 53 + <retorno> + cs        [4 bytes]
                        case 83:
                            break;
                        //--------------------------------------------------------------
                        // 84	Contador de atualização do SETUP
                        // CMD: 00 + 54 + <contadorAtualizaçãoSetup> + cs          [4 bytes]
                        // RSP: 00 + 54 + <retorno> + cs       [4 bytes]
                        case 84:
                            break;
                        //--------------------------------------------------------------
                        // 85	Editar turnos
                        // CMD: 00 + 55 + <cód. do turno> + <frame de turno(16 bytes)> + cs         [20 bytes]
                        // RSP: 00 + 55 + <retorno>  + <cód. do turno> + cs       [5 bytes]
                        case 85:
                            break;
                        //--------------------------------------------------------------
                        // 86	Restore de dispositivo (PROGRESSIVO)
                        // CMD: 00 + 56 + <framDisp(32 bytes) + <cs>
                        // RSP: 00 + 56 + <retorno>  + cs       [4 bytes]
                        case 86:
                            trataRespostaCmd_gravaDispositivo_PROGRESSIVO(buf);
                            break;
                        //--------------------------------------------------------------
                        // 87	Ler quantidade de dispositivos
                        // CMD: 00 + 57 +  <tipo> + cs         [4 bytes]
                        // RSP: 00 + 57 + <retorno> + <tipo> + <quantidade H> + <quantidade L> + cs       [7 bytes]
                        case 87:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerQuantidadeDispositivos(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 88	Editar datas dos feriados
                        // CMD: 00 + 58 + <frame datas dos feriados(48 bytes)> + cs         [51 bytes]
                        // RSP: 00 + 58 + <retorno> + cs       [4 bytes]
                        case 88:
                            break;

                        //--------------------------------------------------------------
                        // 89	Ler turnos	00 + 59 + <cód. do turno> + cs         [4 bytes]
                        // CMD: 00 + 59 + <cód. do turno> + cs         [4 bytes]
                        // RSP: 00 + 59 + <retorno> + <cód. do turno> + <frame de turno(16 bytes)> + cs       [21 bytes]
                        case 89:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerTurnos(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 90	Ler quantidade de eventos	
                        // CMD: 00 + 5A + <marca> + cs         [4 bytes]
                        // RSP: 00 + 5A + <retorno> + <marca> + <qtdEventosH> + <qtdEventosL> + cs       [7 bytes]
                        case 90:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerQtdEventos(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 91	Ler / Gravar pacote de turnos (PACOTE DE DADOS)
                        // CMD: 00 + 5B + <operacao> + 00 + <indice pacote turno (de 0 a 3)> + <pacote(512 bytes)> + cs          [518 bytes]
                        // RSP: 00 + 5B + <retorno>  + <operacao> + 00 + <pacote(512 bytes)> + cs          [519 bytes]           (operação de escrita na PORTA_UDP2 = 00 + 5B + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 91:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteTurnos(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 92	Ler datas dos feriados
                        // CMD: 00 + 5C + cs         [3 bytes]
                        // RSP: 00 + 5C + <retorno> + <frame datas dos feriados(48 bytes)> + cs       [52 bytes]
                        case 92:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerDatasFeriados(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 93	Ler dispositivo (PROGRESSIVO)
                        // CMD: 00 + 5D + cs         [3 bytes]
                        // RSP: 00 + 5D + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs       [37 bytes]
                        case 93:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerDispositivoProgressivo(buf);
                            }
                            else if (retorno == controladoraLinear.RT_ERRO )
                            {

                            }
                            break;
                        //--------------------------------------------------------------
                        // 94	Modo remoto 2
                        // CMD: 00 + 5E + <tempo remoto> + <modo remoto> + cs         [5 bytes]
                        // RSP: 00 + 5E + <retorno> + cs       [4 bytes]
                        case 94:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_modoRemoto(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 95	Ler Setup
                        // 00 + 5F + cs         [3 bytes]
                        // 00 + 5F + <retorno> + <setup(384bytes)> + cs       [388 bytes]
                        case 95:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                if (checkBox_setup_controleEntradaSaidaTurnos.Checked == false)
                                {
                                    checkBox_TurnoJornadaFeriados_entradaTurno1.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_saidaTurno1.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_entradaTurno2.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_saidaTurno2.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_entradaTurno3.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_saidaTurno3.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_entradaTurno4.Enabled = false;
                                    checkBox_TurnoJornadaFeriados_saidaTurno4.Enabled = false;
                                }
                                else
                                {
                                    checkBox_TurnoJornadaFeriados_entradaTurno1.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_saidaTurno1.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_entradaTurno2.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_saidaTurno2.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_entradaTurno3.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_saidaTurno3.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_entradaTurno4.Enabled = true;
                                    checkBox_TurnoJornadaFeriados_saidaTurno4.Enabled = true;
                                }

                                trataRespostaCmd_lerSetup(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 96	Grava Setup
                        // CMD: 00 + 60 +  <setup(384 bytes)> + cs         [387 bytes]
                        // RSP: 00 + 60 + <retorno> + cs       [4 bytes]
                        case 96:
                            break;
                        //--------------------------------------------------------------
                        // 97	Escreve data e hora
                        // CMD: 00 + 61 + <hora> + <min.> + <seg.> + <dia> + <mês> + <ano> + <cs>         [9 bytes]
                        // RSP: 00 + 61 + <retorno> + cs       [4 bytes]
                        case 97:
                            break;
                        //--------------------------------------------------------------
                        // 98	Ler data e hora
                        // CMD: 00 + 62 + cs         [3 bytes]
                        // RSP: 00 + 62 + <retorno> + <hora> + <min.> + <seg.> + <dia> + <mês> + <ano> + cs       [10bytes]
                        case 98:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerDataHora(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 99	Ler\Gravar pacote de dispositivos  (PACOTE DE DADOS)
                        // CMD: 00 + 63 + <operacao> + <indicePacote_H(de 0 a 1)> + <indicePacote_L(de 0 a 255)> + <pacote(512 bytes)> + cs      [518 bytes]
                        // RSP: 00 + 63 + <retorno> + <operacao> + <indice pacote (de 0 a 1)> + <indice pacote (de 0 a 255)> + <pacote(512 bytes)> + cs         [519 bytes]            (operação de escrita naPORTA_UDP2 = 00 + 63 + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 99:
                            trataRespostaCmd_operacaoPacoteDispositivos(buf);
                            break;
                        //--------------------------------------------------------------
                        // 100	Atualiza antipassback especifico
                        // CMD: 00 + 64 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <s4> + <s5> + <nivel> + <estado Antipassback> + cs         [12 bytes]
                        // RSP: 00 + 64 + <retorno> + cs       [4 bytes]
                        case 100:
                            break;
                        //--------------------------------------------------------------
                        // 101	Editar Grupo da leitora (GL)
                        // CMD: 00 + 65 + <códGrupoLeitoraH> +  <códGrupoLeitoraL> + <endCan> + <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + cs         [10 bytes]
                        // RSP: 00 + 65 + <retorno> + <códGrupoLeitoraH> +  <códGrupoLeitoraL> + cs       [6 bytes]
                        case 101:
                            break;
                        //--------------------------------------------------------------
                        // 102	Ler Grupo da leitora (GL)
                        // CMD: 00 + 66 + <códGrupoLeitoraH> + <códGrupoLeitoraL> + <endCan> + cs         [6 bytes]
                        // RSP: 00 + 66 + <retorno> + <códGrupoLeitoraH> + <códGrupoLeitoraL> + <endCan> + <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + cs       [11 bytes]
                        case 102:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerGrupoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 103	Cancela timout de PC
                        // 00 + 67 + cs         [3 bytes]
                        // 00 + 67 + <retorno> + cs       [4 bytes]
                        case 103:
                            break;
                        //--------------------------------------------------------------
                        // 104	Atualiza antipassback e nível (TODOS OS DISP. CADASTRADOS)
                        // CMD: 00 + 68 + <nivel> + <estado Antipassback> + cs         [5 bytes]
                        // RSP: 00 + 67 + <retorno> + cs       [4 bytes]
                        case 104:
                            break;
                        //--------------------------------------------------------------
                        // 105	Ler eventos  (PROGRESSIVO)
                        // CMD: 00 + 69 + <marca> + <cs>         [4 bytes]
                        // RSP: 00 + 69 + <retorno> + <marca> + <frame de evt. (16 bytes)> + cs       [21 bytes]
                        case 105:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerEventos( buf, epOrigem );
                            }
                            break;
                        //--------------------------------------------------------------
                        // 106	Gravar / Editar - Biometria / Identificar Template
                        // CMD: 00 + 6A + <operacao>+  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs         [40 bytes + template]
                        // RSP: 00 + 6A + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
                        case 156:
                        case 106:
                            trataRespostaCmd_operacoesBiometria1(buf);
                            break;
                        //--------------------------------------------------------------
                        // 107	Apagar / Apagar Todos / Verificar ID gravado / ID vago - Biometria
                        // CMD: 00 + 6B + <operacao> + <indiceLeitora> + <s4> + <s5> + cs         [7 bytes]
                        // RSP: 00 + 6B + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
                        case 107:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacoesBiometria2(buf);
                            }
                            break;
                        //--------------------------------------------------------------
                        // 108 Ler Biometria
                        // 00 + 6C + <operacao> + <indiceLeitora> + <s4> + <s5> + cs
                        // 00 + 6C + <retorno> + <operacao> +  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs
                        case 108:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerBiometria(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 109 Marca todos os eventos como lidos
                        // 00 + 6D + cs
                        // 00 + 6D + <retorno> + cs
                        case 109:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 110	Ler habilitação das Leitoras
                        // 00 + 6E + <cód. da habilitação H> + <cód. da habilitação L> + cs 
                        // 00 + 6E + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação(32 bytes)> + cs
                        case 110:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerHabilitacaoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 111	Ler informações
                        // 00 + 6F + <nDisp> + cs
                        // 00 + 6F + <retorno> + <nDisp> +  <modo> + <DNS (16 bytes)> + <IP(4bytes)> + <versão SW (6bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <MAC ADDRESS(8 bytes)> + <cs>
                        case 111:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerInformacoes(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 112	Gravar/Ler contador atual de vagas
                        // 00 + 70 + <operacao> + <vagas_atual_H> + <vagas_atual_L> + cs
                        // 00 + 70 + <retorno> + <operacao> + <vagas_atual_H> + <vagas_atual_L> + cs
                        case 112:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_gravarLerContadorAtualVagas(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 113	Ler / Gravar pacote de jornada  (PACOTE DE DADOS)
                        // 00 + 71 + <operacao> + 00 + 00 + <pacote(512 bytes)> + cs        [518 bytes]
                        // 00 + 71 + <retorno>  + <operacao> + 00 + <pacote(512 bytes)> + cs          [519 bytes]
                        //           (operação de escrita na PORTA_UDP2 = 00 + 5B + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 113:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteJornada(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 114 - Edita jornada
                        // 00 + 72 + <cód. Jornada> + <cód. turno segunda>  + <cód. turno terça> + <cód. turno quarta> + <cód. turno quinta> + <cód. turno sexta> + <cód. turno sábado> + <cód. turno domingo> + <flagsJornada2> + cs
                        // 00 + 72 + <retorno> + <cód. Jornada> + cs
                        case 114:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 115	Ler jornada
                        // 00 + 73 + <cód. Jornada> + cs         [4 bytes]
                        // 00 + 73 + <retorno> + <cód. Jornada> + <cód. turno segunda>  + <cód. turno terça> + <cód. turno quarta> + <cód. turno quinta> + <cód. turno sexta> + <cód. turno sábado> + <cód. turno domingo> + <cód. turno feriados> + cs       [13 bytes]
                        case 115:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerJornada(buf);
                            }
                            break;
                        //----------------------------------------------------
                        // 116 - Evento automático
                        // ---
                        // 00 + 74 + <retorno> + <cntAtual> + <evento> + <nDisp> + <tipoDisp> + <s0> + <s1> + <s2> + <s3> + <s4/cntH> + <s5/cntL>  + <hora> + <Minuto> + <segundo> + <dia> + <mês> + <ano> + <nivel> + <flagsCtrl> + <cs>
                        case 116:
                            trataRespostaCmd_eventoAutomatico( buf, epOrigem );
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 117 - Acionamento
                        // 00 + 75 + <dispositivo> + <nDisp> + <saida> + cs
                        // 00 + 75 + <retorno> + <dispositivo> + <nDisp> + <saida> + cs
                        case 117:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_acionamento(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 118 - Ler dispositivo
                        // 00 + 76 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs	
                        // 00 + 76 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> + <flagsCadastro> + <flagsStatus> + 3x<reservado> + <nivel> + <validade/créditos> + 17x<userLabel> + cs	
                        case 118:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerDispositivo(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 119	Bypass
                        // 00 + 77 + <bypassOn> + <dispositivo> + <tamanhoComandoH>+ <tamanhoComandoL> + <frameComando> + cs
                        // 00 + 77 + <retorno> + <bypassOn> + <dispositivo> + <tamanhoRespostaH>+ <tamanhoRespostaL> + <frameComando> + cs
                        case 119:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 120	Edita parâmetro Setup	
                        // 00 + 78 + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                        // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                        case 120:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoParametroSetup(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 121	Ler/Edita habilitação das Leitoras individual
                        // 00 + 79 + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)>+ cs
                        // 00 + 79 + <retorno> + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)>+  cs 
                        case 121:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoRotaIndividual(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 122	Edita data de feriado individual
                        // 00 + 7A + <cód. do feriado> + <dia> + <mês> + cs
                        // 00 + 7A + <retorno> + <cód. do feriado> + cs
                        case 122:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_editaDataFeriadoIndividual(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 123	Ler/Editar mensagens display externo
                        // 00 + 7B + <operação> + <cód. MsgDisplayExterno> + <mensagemDisplayExterno(32bytes)> + cs
                        // 00 + 7B + <retorno> + <operação> +  <cód. MsgDisplayExterno> + <mensagemDisplayExterno(32bytes)> + cs		
                        case 123:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoMensagensDisplayExterno(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 124	Ler / Gravar pacote de grupos de leitora (GL)  (PACOTE DE DADOS)
                        // 00 + 7C + <operacao> + 00 + <indicePacote_L (de 0 a 207)> + <pacote(512 bytes)> + cs          [518 bytes]
                        // 00 + 7C + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 207)> + <pacote(512 bytes)> + cs          [519 bytes]
                        //            (operação de escrita na PORTA_UDP2 = 00 + 7C + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 124:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteGrupoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 125	Ler / Gravar pacote de habilitação  (PACOTE DE DADOS)           [518 bytes]
                        // 00 + 7D + <operacao> + 00 + <indicePacote_L(de 0 a 25)> + <pacote(512 bytes)> + cs
                        // 00 + 7D + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 25)> + <pacote(512 bytes)> + cs          [519 bytes]
                        //           (operação de escrita na PORTA_UDP2 = 00 + 7D + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 125:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteHabilitacao(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 126	Ler / Gravar pacote de Labels de rota  (PACOTE DE DADOS)
                        // 00 + 7E + <operacao> + 00 + <indicePacote_L (de 0 a 1)> + <pacote(512 bytes)> + cs          [518 bytes]
                        // 00 + 7E + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 1)> + <pacote(512 bytes)> + cs          [519 bytes]
                        //           (operação de escrita na PORTA_UDP2 = 00 + 7E + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
                        case 126:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteLabelsRota(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 127	Grava configuração de rede no MAC ADDRESS especificado
                        // 00 + 7F + <MAC ADDRESS(6 bytes)> + <DNS (16 bytes)> + <IP(4bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <cs>  [33 bytes]
                        // 00 + 7F + <retorno> + <MAC ADDRESS(6 bytes)> + <DNS (16 bytes)> + <IP(4bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <cs>   [34 bytes]
                        case 127:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 128	
                        // Editar habilitação e grupo da leitora
                        // 00 + 80 + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> +
                        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + cs      [20 bytes]
                        // 00 + 80 + <retorno> + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> + 
                        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + cs      [21 bytes]
                        case 128:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoRotaGrupoLeitoras(buf);
                            }                            
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 129	Ler dados do SD CARD (PROGRESSIVO)
                        // 00 + 81 + <arquivo sdcard> + <dia> + <mês> + <ano> + <hora> + <minutos> + <segundos> + cs       [10 bytes]
                        // 00 + 81 + <retorno> + <arquivo sdcard> + <dia> + <mês> + <ano> + <hora> + <minutos> + <segundos> +  <tamanhoLinhaH> + tamanhoLinhaL> + <linha do arquivo> + cs    [13 bytes + linha do arquivo]
                        case 129:
                            trataRespostaCmd_lerDadosSDCARD(buf);
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        /// 130	Apagar último evento	
                        // 00 + 82 + 82 [3 bytes]
                        // 00 + 82 + <retorno> + cs   [4 bytes]
                        case 130:                            
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 131	VAGO
                        // CMD:
                        // RSP:
                        case 131:
                            trataRespostaCmd_operacaoPacoteVagasHabilitação(buf);
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 132 Acionamento remoto com identificação
                        // 00 + 84 + <dispositivo> + <nDisp> + <saída> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <serial 4> + <serial 5> + <flagsCadastro> + <nivel> + cs
                        // 00 + 84 + <retorno> + cs   [4 bytes]
                        case 132:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 133	Ler evento/Marcar evento como lido/Ler último evento não lido (EVENTO INDEXADO)
                        // CMD: 00 + 85 + <operacao> + <endereçoEventoH> + <endereçoEventoL> + cs     [6 bytes]
                        // RSP: 00 + 85 + <retorno> + <operacao> + <cntAtual> + <frame Evento(16 bytes)> + <endereçoEventoH> + <endereçoEventoL> + cs    [24 bytes]
                        case 133:
                            trataRespostaCmd_operacaoEvento(buf, epOrigem );
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 134	Evento automático com endereço do ponteiro (EVENTO INDEXADO)
                        // CMD: -
                        // RSP: 00 + 86 + <retorno> + <cntAtual> + <frame de evt. (16 bytes)> + <endereçoEventoH> + <endereçoEventoL> + cs   [23 bytes]
                        case 134:
                            trataRespostaCmd_eventoAutomatico( buf, epOrigem );
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 135 Leitura de evento com endereço do ponteiro (EVENTO INDEXADO em PACOTE DE DADOS)
                        // 00 + 87 + cs [3 bytes]
                        // 00 + 87 + <retorno> + < 32 x (frame de evt. (16 bytes) + <endereçoEventoH> + <endereçoEventoL>)> + cs [ 580 bytes]
                        case 135:
                            trataRespostaCmd_lerPacoteEventos( buf, epOrigem );
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        case 136:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        case 137:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        case 138:
                            trataRespostaCmd_operacaoVagasHabilitacao(buf, epOrigem);
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        case 139:
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 140	Ler Vagas especifico
                        // CMD: 00 + 8C + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + cs           [10 bytes]	
                        // RSP: 00 + 8C + <retorno> + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + <codHAB(High)> + <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs  [15 bytes]
                        case 140:
                            trataRespostaCmd_lerVagasEspecifico(buf, epOrigem);
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 141	Marcar PACOTE de evento como "lido" a partir do endereço do evento (EVENTO INDEXADO em PACOTE DE DADOS)	
                        // CMD: 00 + 8D + <endereçoEventoH> + <endereçoEventoL> + cs [4 bytes]
                        // RSP: 00 + 8D + <retorno> + <endereçoEventoH> + <endereçoEventoL> + cs [5 bytes]
                        case 141:
                            trataRespostaCmd_marcarPacoteEventoIndexadoComoLido(buf, epOrigem);
                            break;

                        //--------------------------------------------------------------------------------------------------------
                        // 142	Ler habilitação das leitoras 2 (c/ SETOR)
                        // CMD: 00 + 8E + <cód. da habilitação H> +  <cód. da habilitação L> + <setor> + cs         [6 bytes]
                        // RSP: 00 + 8E + <retorno> + <cód. da habilitação H> +  <cód. da habilitação L> + <frame de habilitação (256 bytes)> + <setor> + cs       [263 bytes]
                        case 142:
                            // case 110
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerHabilitacaoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //143	Editar habilitação das leitoras 2 (c/ SETOR)									
                        //CMD: 00 + 8F + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação (256 bytes)> + <setor> + cs          [262 bytes]	
                        //RSP: 00 + 8F + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <setor> + cs         [7 bytes]
                        case 143:
                            // case 79
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //144	Ler / Gravar pacote de habilitação das leitoras 2 (c/ SETOR) (PACOTE DE DADOS)	
                        //CMD: 00 + 90 + <operacao> + 00 + <indicePacote_L(de 0 a 25)> + <pacote(512 bytes)> + <setor> + cs          [519 bytes]						
                        //RSP: 00 + 90 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 25)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes] 
                        case 144:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteHabilitacao(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //145	Ler Grupo da leitora 2 (c/ SETOR) (GL)											
                        //CMD: 00 + 91+ <cód. da habilitação H> + <cód. da habilitação L> + <endCan> + <setor> + cs         [7 bytes]									
                        //RSP: 00 + 91 + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <endCan> + <código da Jornada (leitora 1)> + <código da Jornada (leitora 2)> + <código da Jornada (leitora 3)> + <código da Jornada (leitora 4)> + <setor> + cs       [12 bytes]
                        case 145:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerGrupoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //146	Editar Grupo da leitora 2 (c/ SETOR) (GL)										
                        //CMD: 00 + 92 + <setor> + <cód. da habilitação H> +  <cód. da habilitação L> + <endCan> + <código da Jornada (leitora 1)> + <código da Jornada (leitora 2)> + <código da Jornada (leitora 3)> + <código da Jornada (leitora 4)>  + cs         [11 bytes]	
                        //RSP: 00 + 92 + <retorno> + <setor> + <cód. da habilitação H> +  <cód. da habilitação L> + cs       [7 bytes]
                        case 146:
                            // case 101
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //147	Ler / Gravar pacote de grupos de  leitora  2 (c/ SETOR) (GL)  (PACOTE DE DADOS)	
                        //CMD: 00 + 93 + <operacao> + 00 + <indicePacote_L (de 0 a 207)> + <pacote(512 bytes)> + <setor> + cs          [519 bytes]	
                        //RSP: 00 + 93 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 207)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes] 
                        case 147:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteGrupoLeitoras(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        //148	Ler/Edita habilitação das Leitoras individual 2 (c/ SETOR)						
                        //CMD: 00 + 94 + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)> + <setor> + cs     [10 bytes]	
                        //RSP: 00 + 94 + <retorno> + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)> +  <setor> + cs     [11 bytes]
                        case 148:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoRotaIndividual(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 149	
                        // Editar habilitação e grupo da leitora
                        // 00 + 95 + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> +
                        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + <setor> + cs      [21 bytes]
                        // 00 + 95 + <retorno> + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> + 
                        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + <setor> + cs      [22 bytes]
                        case 149:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoRotaGrupoLeitoras(buf);
                            }        
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 150	Ler / Gravar pacote de Labels de rota  (PACOTE DE DADOS)
                        // 00 + 96 + <operacao> + 00 + <indicePacote_L (de 0 a 1)> + <pacote(512 bytes)> + <setor> + cs          			[519 bytes]
                        // 00 + 96 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 1)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes]
                        case 150:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoPacoteLabelsRota(buf);
                            }
                        break;
                        //--------------------------------------------------------------------------------------------------------
                        // 151	Editar midia (sdcard)
                        // CMD: 00 + 97 + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 61 + nBytes ]
                        // RSP: 00 + 97 + <retorno> + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 62 + nBytes ]
                        case 151:
                            trataRespostaCmd_lerEditarMidia(buf);
                            break;

                        //--------------------------------------------------------------------------------------------------------
                        // 152	Ler/Editar Controle de Vagas ID	
                        // CMD: 00 + 98 + <operação> + <indice_id_L> + indice_id_H> + <id(6 bytes)> + <qtd_total_vagas> + <qtd_atual_vagas> + cs   [14 bytes] (FINALIZAR COM 0xFF no campo "id")	
                        // RSP: 00 + 98 + <retorno> + <operação> + <indice_id_H> + indice_id_L> + <id(6 bytes)> + <qtd_total_vagas> + <qtd_atual_vagas> + cs  [15 bytes]
                        case 152:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoVagasID(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 158 Lêr turnos 2
                        // 00 + 59 + <cód. do turno> + cs	[ 4 bytes]
                        // 00 + 59 + <retorno> + <cód. do turno> + <frame de turno(16 bytes)> + <direcaoTurnos> + cs  [ 22 bytes ]
                        case 158:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_lerTurnos2(buf);
                            }
                            break;
                        //--------------------------------------------------------------------------------------------------------
                        // 162	Ler/ Gravar ajuste do relógio
                        // CMD: 00 + A2 + <operação> + <offset> + cs					[5 bytes]
                        // RSP: 00 + A2 + <retorno> + <operação> + <offset> + cs		[6 bytes]
                        case 162:
                            if (retorno == controladoraLinear.RT_OK)
                            {
                                trataRespostaCmd_operacaoAjusteRelogio(buf);
                            }
                            break;


                    }
                    mostraRetorno(retorno);               
                }
            }
            catch
            {

            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void trataRespostaCmd_operacaoAjusteRelogio(byte[] buf)
        {
            if( buf[3] == controladoraLinear.OP_LEITURA )
            {
                double ajuste = 0;
                if( buf[4] < 128 )
                {
                    ajuste = 127 - buf[4];
                }
                else
                {
                    ajuste = (byte)(256 - buf[4]);
                    ajuste += 127;
                }
                trackBarAjusteRelogio.Value = (byte)ajuste;

            }

        }

        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void trataRespostaCmd_lerTurnos2(byte[] buf)
        {
            try
            {
                txtT1HI.Text = Convert.ToString(buf[4]);
                txtT1MI.Text = Convert.ToString(buf[5]);
                txtT1HF.Text = Convert.ToString(buf[6]);
                txtT1MF.Text = Convert.ToString(buf[7]);
                txtT2HI.Text = Convert.ToString(buf[8]);
                txtT2MI.Text = Convert.ToString(buf[9]);
                txtT2HF.Text = Convert.ToString(buf[10]);
                txtT2MF.Text = Convert.ToString(buf[11]);
                txtT3HI.Text = Convert.ToString(buf[12]);
                txtT3MI.Text = Convert.ToString(buf[13]);
                txtT3HF.Text = Convert.ToString(buf[14]);
                txtT3MF.Text = Convert.ToString(buf[15]);
                txtT4HI.Text = Convert.ToString(buf[16]);
                txtT4MI.Text = Convert.ToString(buf[17]);
                txtT4HF.Text = Convert.ToString(buf[18]);
                txtT4MF.Text = Convert.ToString(buf[19]);

                checkBox_TurnoJornadaFeriados_entradaTurno1.Checked = false;
                checkBox_TurnoJornadaFeriados_saidaTurno1.Checked = false;
                checkBox_TurnoJornadaFeriados_entradaTurno2.Checked = false;
                checkBox_TurnoJornadaFeriados_saidaTurno2.Checked = false;
                checkBox_TurnoJornadaFeriados_entradaTurno3.Checked = false;
                checkBox_TurnoJornadaFeriados_saidaTurno3.Checked = false;
                checkBox_TurnoJornadaFeriados_entradaTurno4.Checked = false;
                checkBox_TurnoJornadaFeriados_saidaTurno4.Checked = false;

                if ((buf[20] & 0x01) > 0 )  checkBox_TurnoJornadaFeriados_entradaTurno1.Checked = true;
                if ((buf[20] & 0x02) > 0 )  checkBox_TurnoJornadaFeriados_saidaTurno1.Checked = true;
                if ((buf[20] & 0x04) > 0 )  checkBox_TurnoJornadaFeriados_entradaTurno2.Checked = true;
                if ((buf[20] & 0x08) > 0 )  checkBox_TurnoJornadaFeriados_saidaTurno2.Checked = true;
                if ((buf[20] & 0x10) > 0 )  checkBox_TurnoJornadaFeriados_entradaTurno3.Checked = true;
                if ((buf[20] & 0x20) > 0 )  checkBox_TurnoJornadaFeriados_saidaTurno3.Checked = true;
                if ((buf[20] & 0x40) > 0 )  checkBox_TurnoJornadaFeriados_entradaTurno4.Checked = true;
                if ((buf[20] & 0x80) > 0 )  checkBox_TurnoJornadaFeriados_saidaTurno4.Checked = true;

            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 98 + <retorno> + <operação> + <indice_id_H> + indice_id_L> + <id(6 bytes)> + <qtd_total_vagas> + <qtd_atual_vagas> + cs  [15 bytes]
        private void trataRespostaCmd_operacaoVagasID(byte[] buf)
        {
            UInt16 indice = 0;
            byte totalVagas = 0, atualVagas = 0;
            StringBuilder id = new StringBuilder();

            indice = (UInt16)((buf[4] << 8) + buf[5]);
            
            for(int i=0;i<6;i++) id.Append((char)buf[6+i]);

            totalVagas = buf[12];
            atualVagas = buf[13];

            textBox_controleVagasID_atualVagas.Text = atualVagas.ToString();
            textBox_controleVagasID_totalVagas.Text = totalVagas.ToString();
            textBox_controleVagasID_id.Text = id.ToString();

            ListViewItem novoEvento = new ListViewItem();
            novoEvento.Text = indice.ToString();
            novoEvento.SubItems.Add(id.ToString());
            novoEvento.SubItems.Add(totalVagas.ToString());
            novoEvento.SubItems.Add(atualVagas.ToString());
            listView_controleVagasId.Items[indice] = novoEvento;
           
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // CMD: 00 + 97 + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 61 + nBytes ]
        // RSP: 00 + 97 + <retorno> + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 62 + nBytes ]
        public string diretorioAtual;
        public bool bool_Limpar_listView_SDCARD_MIDIA = false;
        public bool bool_LimparrichTextBox_SDCARD_MIDIA_dados = false;
        public int nDados = 0;
        public const int INDICE_CAMINHO_SDCARD_MIDIA = 5;
        public const int INDICE_N_BYTES_DADOS_SDCARD_MIDIA = 59;
        public const int INDICE_DADOS_SDCARD_MIDIA = 61;
        public void trataRespostaCmd_lerEditarMidia(byte[] buf)
        {
            UInt16 tamanho = 0;
            String s = null;
            StringBuilder sb = new StringBuilder();
            ListViewItem listTemp = new ListViewItem();
            StringBuilder dirTemp = new StringBuilder();

            if (buf[2] == controladoraLinear.RT_OK)
            {
                if (bool_Limpar_listView_SDCARD_MIDIA)
                {
                    bool_Limpar_listView_SDCARD_MIDIA = false;
                    listView_SDCARD_MIDIA.Items.Clear();
                }

                if (bool_LimparrichTextBox_SDCARD_MIDIA_dados)
                {
                    bool_LimparrichTextBox_SDCARD_MIDIA_dados = false;
                    richTextBox_SDCARD_MIDIA_dados.Clear();
                    nDados = 0;
                }


                tamanho = (UInt16)((buf[INDICE_N_BYTES_DADOS_SDCARD_MIDIA] << 8) + buf[INDICE_N_BYTES_DADOS_SDCARD_MIDIA+1]);
                if (tamanho > 0)
                {
                    for (int i = 0; i < tamanho; i++) sb.Append((char)buf[INDICE_DADOS_SDCARD_MIDIA + i]);
                    s = sb.ToString();
                }

                byte[] dados = new byte[1024];
                try
                {
                    switch (buf[3])
                    {
                        case controladoraLinear.OP_ABRIR_ARQUIVO_LEITURA:
                            nDados = 0;
                            radioButton_SDCARD_op_lerArquivoLeitura.Checked = true;
                            break;

                        case controladoraLinear.OP_LER_ARQUIVO_LEITURA:
                            nDados += tamanho;
                            richTextBox_SDCARD_MIDIA_dados.AppendText(s);
                            richTextBox_SDCARD_MIDIA_dados.ScrollToCaret();
                            break;

                        case controladoraLinear.OP_CRIAR_ABRIR_ARQUIVO_ESCRITA:
                            nDados = tamanhoEnviando;
                            radioButton_SDCARD_op_escreverArquivoEscrita.Checked = true;

                            if (enviandoArquivo)
                            {
                                if (tamanhoEnviando == 0)
                                {
                                    enviandoArquivo = false;
                                    progressBar_SDCARD_MIDIA_enviandoArquivo.Visible = false;
                                }
                                else
                                {
                                    ushort i = 0;
                                    for (i = 0; i < 1024; i++)
                                    {
                                        dados[i] = (byte)arquivoEnviando[arquivoEnviando.Length - tamanhoEnviando];
                                        tamanhoEnviando--;
                                        if (tamanhoEnviando == 0) break;
                                    }

                                    if( i < 1024 ) i++;

                                    progressBar_SDCARD_MIDIA_enviandoArquivo.Increment(i);

                                    byte[] b = pControl.lerEditarMidia(controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA, midiaEnviando, pathEnviando, i, dados);
                                    enviarMensagem(b);
                                }
                            }

                            break;

                        case controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA:
                            

                            if (enviandoArquivo)
                            {
                                if (tamanhoEnviando == 0)
                                {
                                    byte[] b = pControl.lerEditarMidia(controladoraLinear.OP_FECHAR_ARQUIVO, midiaEnviando, pathEnviando, 0, dados);
                                    enviarMensagem(b);
                                }
                                else
                                {
                                    ushort i = 0;
                                    for (i = 0; i < 1024; i++)
                                    {
                                        dados[i] = (byte)arquivoEnviando[arquivoEnviando.Length - tamanhoEnviando];
                                        tamanhoEnviando--;
                                        if (tamanhoEnviando == 0) break;                                        
                                    }
                                    if (i < 1024) i++;

                                    progressBar_SDCARD_MIDIA_enviandoArquivo.Increment(i);

                                    byte[] b = pControl.lerEditarMidia(controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA, midiaEnviando, pathEnviando, i, dados);
                                    enviarMensagem(b);
                                }
                            }
                            break;

                        case controladoraLinear.OP_FECHAR_ARQUIVO:
                            if (enviandoArquivo)
                            {
                                enviandoArquivo = false;
                                progressBar_SDCARD_MIDIA_enviandoArquivo.Visible = false;
                                MessageBox.Show("Arquivo enviado com sucesso!");
                            }
                            else
                            {
                                nDados = 0;
                            }
                            break;

                        case controladoraLinear.OP_LISTAR:
                            nDados = 0;
                            char[] separator = new char[] { '\r', '\n' };
                            string[] itens = s.Split(separator);
                            listView_SDCARD_MIDIA.Items.Add(itens[0]);
                            if (itens.Count<string>() > 1) listView_SDCARD_MIDIA.Items[listView_SDCARD_MIDIA.Items.Count - 1].SubItems.Add(itens[2]);
                            if (itens.Count<string>() > 3) listView_SDCARD_MIDIA.Items[listView_SDCARD_MIDIA.Items.Count - 1].SubItems.Add(itens[4]);
                            if (itens.Count<string>() > 5) listView_SDCARD_MIDIA.Items[listView_SDCARD_MIDIA.Items.Count - 1].SubItems.Add(itens[6]);

                            break;

                        case controladoraLinear.OP_REINICIAR_MIDIA:
                        case controladoraLinear.OP_MUDAR_DIRETORIO:
                            enviandoArquivo = false;
                            progressBar_SDCARD_MIDIA_enviandoArquivo.Visible = false;
                            nDados = 0;
                            for (int i = 0; i < tamanho; i++) dirTemp.Append(Convert.ToChar(buf[INDICE_DADOS_SDCARD_MIDIA + i]));
                            diretorioAtual = dirTemp.ToString();
                            textBox_SDCARD_MIDIA_nome.Text = diretorioAtual;
                            listView_SDCARD_MIDIA.Items.Clear();
                            listView_SDCARD_MIDIA.Items.Add(diretorioAtual);
                            radioButton_SDCARD_op_listar.Checked = true;

                            break;

                        case controladoraLinear.OP_CRIAR_DIRETORIO:
                            nDados = 0;
                            radioButton_SDCARD_op_mudarDiretorio.Checked = true;
                            break;

                        case controladoraLinear.OP_APAGAR_ARQUIVO_DIRETORIO:
                            nDados = 0;
                            radioButton_SDCARD_op_reiniciarMidia.Checked = true;
                            break;

                    }
                }
                catch { }

                label_SDCARD_MIDIA_dados.Text = "Dados:";
                label_SDCARD_MIDIA_dados.Text += " " + nDados.ToString() + " bytes";

            }
            else
            {
                
                if (enviandoArquivo)
                {
                    MessageBox.Show("Erro no envio do arquivo!");
                    enviandoArquivo = false;
                    progressBar_SDCARD_MIDIA_enviandoArquivo.Visible = false;
                }

                switch (buf[3])
                {
                    case controladoraLinear.OP_ABRIR_ARQUIVO_LEITURA:
                        break;

                    case controladoraLinear.OP_LER_ARQUIVO_LEITURA:
                        if (buf[2] == controladoraLinear.RT_ERRO_EOF)
                        {
                            bool_LimparrichTextBox_SDCARD_MIDIA_dados = true;
                        }
                        break;

                    case controladoraLinear.OP_CRIAR_ABRIR_ARQUIVO_ESCRITA:
                        break;

                    case controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA:
                        break;

                    case controladoraLinear.OP_FECHAR_ARQUIVO:
                        break;

                    case controladoraLinear.OP_LISTAR:
                        if( buf[2] == controladoraLinear.RT_ERRO_EOF )
                        {
                            bool_Limpar_listView_SDCARD_MIDIA = true;
                        }
                        break;

                    case controladoraLinear.OP_APAGAR_ARQUIVO_DIRETORIO:
                        break;

                    case controladoraLinear.OP_REINICIAR_MIDIA:
                        break;

                    case controladoraLinear.OP_MUDAR_DIRETORIO:
                        break;

                    case controladoraLinear.OP_CRIAR_DIRETORIO:
                        break;

                }
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 62 + <retorno> + <hora> + <min.> + <seg.> + <dia> + <mês> + <ano> + cs       [10bytes]
        public void trataRespostaCmd_lerDataHora(byte[] buf)
        {
            try
            {
                txtDataHora_hora.Text = Convert.ToString(buf[3]);
                txtDataHora_minuto.Text = Convert.ToString(buf[4]);
                txtDataHora_segundo.Text = Convert.ToString(buf[5]);
                txtDataHora_dia.Text = Convert.ToString(buf[6]);
                txtDataHora_mes.Text = Convert.ToString(buf[7]);
                txtDataHora_ano.Text = Convert.ToString(buf[8]);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 99	Ler\Gravar pacote de dispositivos  (PACOTE DE DADOS)
        // CMD: 00 + 63 + <operacao> + <indicePacote_H(de 0 a 1)> + <indicePacote_L(de 0 a 255)> + <pacote(512 bytes)> + cs      [518 bytes]
        // RSP: 00 + 63 + <retorno> + <operacao> + <indice pacote (de 0 a 1)> + <indice pacote (de 0 a 255)> + <pacote(512 bytes)> + cs         [519 bytes]            (operação de escrita naPORTA_UDP2 = 00 + 63 + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public void trataRespostaCmd_operacaoPacoteDispositivos(byte[] buf)
        {
            //UInt16 indiceLimite = (controladoraLinear.N_LINES_DISPOSITIVO * controladoraLinear.SIZE_OF_DISPOSITIVO) / 512;
            UInt16 indiceLimite = 0; // = (8192 * controladoraLinear.SIZE_OF_DISPOSITIVO) / 512;

            indiceLimite = (ushort)(comboBox_nPacotes.SelectedIndex + 1);

            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_DISPOSITIVO, controladoraLinear.FILE_DISP, indiceLimite, controladoraLinear.DIV_OF_DISPOSITIVO );
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataOperacaoPacote(byte[] buf, UInt16 tamanhoParametro, byte indiceArquivo, UInt16 indiceLimite, UInt16 tamanhoLinhaArquivo)
        {
            byte operacao = buf[3];
            UInt16 indice = (UInt16)((buf[4] << 8) + buf[5]);

            if(( operacao == controladoraLinear.OP_ESCRITA )&&( indice < indiceLimite ))
            {
                btPacoteDados_gravarArquivo.Enabled = false;

                indice++;
                if ((indice < indiceLimite) && (buf[2] != 0xFF))
                {
                    byte[] bufTemp = converteStringParaPacoteDados(indice, arquivoRestore);
                    enviaCmdOperacaoPacote(indiceArquivo, operacao, indice, bufTemp);
                }
                else
                {
                    // fim 
                    mostraFimCronometro();
                }
            }
            else if (operacao == controladoraLinear.OP_LEITURA)
            {
                btPacoteDados_gravarArquivo.Enabled = true;

                if (indice < indiceLimite)
                {
                    if (indice == 0)
                    {
                        richTextPacote.Clear();
                        arquivoRestore = null;
                    }

                    converteArrayPacoteParaString(buf, tamanhoLinhaArquivo);

                    indice++;
                    if ((indice < indiceLimite) && (buf[2] != 0xFF))
                    {
                        byte[] bufTemp = new byte[512];
                        enviaCmdOperacaoPacote(indiceArquivo, operacao, indice, bufTemp);
                    }
                    else
                    {
                        // formato da primeira linha
                        //27/02/12 16:41:15 00020x02000
                        String sTemp = criaPrimeiraLinhaArquivo((UInt32)arquivoRestore.Length, (UInt16)tamanhoLinhaArquivo);
                        txtPrimeiraLinhaPacote.Text = sTemp;
                        sTemp += '\r';
                        sTemp += '\n';
                        for (int i = 0; i < (tamanhoLinhaArquivo * 2); i++) arquivoRestore += 'F';
                        sTemp += arquivoRestore;
                        arquivoRestore = sTemp;
                        if (ckBoxGravarLeitura.Checked == true) salvaArquivo(indiceArquivo, arquivoRestore);
                        lblTamanho.Text = "Tamanho: " + Convert.ToString(sTemp.Length) + " bytes";
                        richTextPacote.Text = arquivoRestore.Replace("\r", "");
                    }
                }
                else
                {
                    // formato da primeira linha
                    //27/02/12 16:41:15 00020x02000
                    String sTemp = criaPrimeiraLinhaArquivo((UInt32)arquivoRestore.Length, (UInt16)tamanhoLinhaArquivo);
                    txtPrimeiraLinhaPacote.Text = sTemp;
                    sTemp += '\r';
                    sTemp += '\n';
                    for (int i = 0; i < (tamanhoLinhaArquivo * 2); i++) arquivoRestore += 'F';
                    sTemp += arquivoRestore;
                    arquivoRestore = sTemp;
                    if (ckBoxGravarLeitura.Checked == true) salvaArquivo(indiceArquivo, arquivoRestore);
                    lblTamanho.Text = "Tamanho: " + Convert.ToString(sTemp.Length) + " bytes";
                    richTextPacote.Text = arquivoRestore.Replace("\r", "");

                    if (ckBoxGravarLeitura.Checked == true) salvaArquivo(indiceArquivo, arquivoRestore);

                    // fim 
                    mostraFimCronometro();
                }
            }
            else
            {
                // fim 
                mostraFimCronometro();
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void mostraFimCronometro()
        {
            tempoFim = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            TimeSpan dt = tempoFim.Subtract(tempoInicio);

            MessageBox.Show("Tempo decorrido: " + Convert.ToString(Math.Abs(dt.Hours)) + ':'
                                        + Convert.ToString(Math.Abs(dt.Minutes)) + ':'
                                        + Convert.ToString(Math.Abs(dt.Seconds)) + ':'
                                        + Convert.ToString(Math.Abs(dt.Milliseconds)) + ")");
            cronometrometrando = false;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void enviaCmdOperacaoPacote(byte cmd, byte operacao, UInt16 indice, byte[] bufTemp)
        {
            byte[] buf2 = new byte[controladoraLinear.N_BYTES_RSP_CMD_124];
            switch (cmd)
            {
                case controladoraLinear.FILE_DISP: buf2 = pControl.operacaoPacoteDispositivos(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_TURNOS: buf2 = pControl.operacaoPacoteTurnos(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_JORNADA: buf2 = pControl.operacaoPacoteJornada(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_GRUPOS: buf2 = pControl.operacaoPacoteGrupoLeitoras(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_LABELS: buf2 = pControl.operacaoPacoteLabelsRota(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_ROTAS: buf2 = pControl.operacaoPacoteHabilitacao(operacao, indice, bufTemp); break;
                case controladoraLinear.FILE_DTURNOS: buf2 = pControl.operacaoPacoteTurnos2(operacao, indice, bufTemp); break;
                default: return;
            }
            enviarMensagem(buf2);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void enviaCmdOperacaoPacoteComSetor(byte cmd, byte operacao, UInt16 indice, byte[] bufTemp, byte setor )
        {
            byte[] buf2 = new byte[controladoraLinear.N_BYTES_RSP_CMD_144];
            switch (cmd)
            {
                case controladoraLinear.FILE_GRUPOS: buf2 = pControl.operacaoPacoteGrupoLeitorasComSetor(operacao, indice, bufTemp, setor ); break;
                case controladoraLinear.FILE_LABELS: buf2 = pControl.operacaoPacoteLabelsRotaComSetor(operacao, indice, bufTemp, setor ); break;
                case controladoraLinear.FILE_ROTAS: buf2 = pControl.operacaoPacoteHabilitacaoComSetor(operacao, indice, bufTemp, setor); break;
                default: return;
            }
            enviarMensagem(buf2);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public string criaPrimeiraLinhaArquivo(UInt32 tamanhoArquivo, UInt16 tamanhoParametro)
        {
            String sTemp = DateTime.Now.Day.ToString("00") + '/' + DateTime.Now.Month.ToString("00") + '/' + (DateTime.Now.Year - 2000).ToString("00") + ' ' +
                           DateTime.Now.Hour.ToString("00") + ':' + DateTime.Now.Minute.ToString("00") + ':' + DateTime.Now.Second.ToString("00") + ' ' +
                           tamanhoParametro.ToString("x05") + 'x' + (tamanhoArquivo / (tamanhoParametro * 2 + 2)).ToString("x05");
            return sTemp;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void converteArrayPacoteParaString(byte[] buf, UInt16 tamanhoLinhaArquivo )
        {
            UInt16 n = (UInt16)(512 / tamanhoLinhaArquivo );
            int i, j = 0;
            String sTemp;
            byte[] bufTemp2 = new byte[tamanhoLinhaArquivo ]; // soma 32 caracteres a mais 16 CR e 16 LF
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < tamanhoLinhaArquivo; j++) bufTemp2[j] = buf[6 + j + i * tamanhoLinhaArquivo];
                sTemp = BitConverter.ToString(bufTemp2);
                arquivoRestore += sTemp.Replace("-", "");
                arquivoRestore += '\r';
                arquivoRestore += '\n';
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 66 + <retorno> + <códGrupoLeitoraH> + <códGrupoLeitoraL> + <endCan> + <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + cs       [11 bytes]
        public void trataRespostaCmd_lerGrupoLeitoras(byte[] buf)
        {
            try
            {
                cboxJornadaL1.SelectedIndex = acertaValorLimite((byte)cboxJornadaL1.SelectedIndex, buf[6], (byte)cboxJornadaL1.Items.Count);
                cboxJornadaL2.SelectedIndex = acertaValorLimite((byte)cboxJornadaL1.SelectedIndex, buf[7], (byte)cboxJornadaL2.Items.Count);
                cboxJornadaL3.SelectedIndex = acertaValorLimite((byte)cboxJornadaL1.SelectedIndex, buf[8], (byte)cboxJornadaL3.Items.Count);
                cboxJornadaL4.SelectedIndex = acertaValorLimite((byte)cboxJornadaL1.SelectedIndex, buf[9], (byte)cboxJornadaL4.Items.Count);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 106	Gravar / Editar - Biometria / Identificar Template
        // CMD: 00 + 6A + <operacao>+  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs         [40 bytes + template]
        // RSP: 00 + 6A + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public void trataRespostaCmd_operacoesBiometria1(byte[] buf)
        {
            switch( buf[3] )
            {
                case controladoraLinear.OP_ESCRITA:
                case controladoraLinear.OP_EDICAO:
                case controladoraLinear.OP_RESTORE:
                    if (buf[2] == controladoraLinear.RT_OK)
                    {
                        tentativaProgressivo = 5;
                        toutGravarTemplateProgressivo = TOUT_PROGRESSIVO;
                        if (executarNvezes > 0)
                        {
                            executarNvezes--;
                        }
                        else
                        {
                            btGravarNCopiasBm.Text = "Gravar N Copias";
                            toutGravarTemplateProgressivo = 0;
                            if (emRestoreBiometria)
                            {
                                emRestoreBiometria = false;
                                MessageBox.Show("Sucesso no restore de arquivo biometrias");
                            }

                        }
                        if (emRestoreBiometria)
                        {
                            try
                            {
                                byte[] buf3 = listaRestoreBiometria[executarNvezes];
                                enviarMensagem(buf3);
                            }
                            catch
                            {
                                executarNvezes = 0;
                                emRestoreBiometria = false;
                                MessageBox.Show("Erro no restore do arquivo biometrias!");
                            }
                        }
                        else
                        {
                            trataGravarTemplateProgressivo();
                        }
                    }
                    else
                    {
                        if ((buf[3] == controladoraLinear.OP_RESTORE) || (executarNvezes > 0))
                        {
                            if (tentativaProgressivo > 0)
                            {
                                tentativaProgressivo--;
                                byte[] buf3 = listaRestoreBiometria[executarNvezes];
                                enviarMensagem(buf3);
                            }
                            else
                            {
                                executarNvezes = 0;
                                emRestoreBiometria = false;
                                MessageBox.Show("Restore cancelado após 5 erros");
                            }
                        }
                    }
                break;
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataGravarTemplateProgressivo()
        {            
            if ((executarNvezes > 0) && (btGravarNCopiasBm.Text == "Cancelar envio..." ))
            {
                tentativaProgressivo = 5;
                byte tipo = 0;
                UInt64 serial = 0;
                int contadorHCS = 0, codHabilitacao = 0, tamanhoTemplate = 0;
                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(0, 0, 0, 0, 0, 0);
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 0);
                controladoraLinear.s_validade dataValidade = new controladoraLinear.s_validade(1, 1, 0, 1, 1, 0);
                byte nivel = 0, creditos = 0, tipoBiometria = 0, operacao = 0;
                string userLabel = null;
                byte[] template = new byte[800];

                carregaFrameDisp_Form(ref tipo, ref serial, ref contadorHCS, ref codHabilitacao, ref fCadastro, ref fStatus, ref nivel, ref creditos, ref dataValidade, ref userLabel, ref operacao, ref tipoBiometria, ref tamanhoTemplate, ref template);

                serial = (UInt64)executarNvezes;
                frameDisp = pControl.dispositivoParaVetor(tipo, serial, contadorHCS, codHabilitacao, fCadastro, fStatus, nivel, creditos, dataValidade, userLabel);
                byte[] buf2 = pControl.operacoesBiometria1(operacao, (byte)cboxIndiceBM.SelectedIndex, (byte)tipoBiometria, frameDisp, (UInt16)tamanhoTemplate, template);
                enviarMensagem(buf2);
            }

            if (executarNvezes == 0) btGravarNCopiasBm.Text = "Gravar N Copias";
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 107	Apagar / Apagar Todos / Verificar ID gravado / ID vago - Biometria
        // CMD: 00 + 6B + <operacao> + <indiceLeitora> + <s4> + <s5> + cs         [7 bytes]
        // RSP: 00 + 6B + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public void trataRespostaCmd_operacoesBiometria2(byte[] buf)
        {
            byte[] serialArray = new byte[6];
            serialArray[4] = buf[5];
            serialArray[5] = buf[6];
            txtSerial.Text = BytesToHexString(serialArray);

            try
            {
                txtSerial.Text = txtSerial.Text.Substring(1, 11);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 69 + <retorno> + <marca> + <frame de evt. (16 bytes)> + cs       [21 bytes]
        public void trataRespostaCmd_lerEventos(byte[] buf, String epOrigem )
        {
            try
            {
                byte contadorAtual = 0, evento = 0, modo = 0, endereco = 0, tipo = 0, dia = 0, mes = 0, ano = 0, hora = 0, minuto = 0, segundo = 0, nivel = 0, setor = 0;
                controladoraLinear.FLAGS_EVENTO flagsEvento = new controladoraLinear.FLAGS_EVENTO(0, 0, 0, 0);
                UInt64 serial = 0;
                UInt16 contadorHCS = 0;
                pControl.vetorParaEvento( buf, ref contadorAtual, ref evento, ref modo, ref endereco, ref tipo, ref serial, ref contadorHCS, ref dia, ref mes, ref ano, ref hora, ref minuto, ref segundo, ref nivel, ref flagsEvento, ref setor );

                // tipo evento
                switch ( evento )
                {
                    case controladoraLinear.LOG_DISP_ACIONADO: lblTipoEvento.Text = "LOG_DISP_ACIONADO"; break;
                    case controladoraLinear.LOG_PASSAGEM: lblTipoEvento.Text = "LOG_PASSAGEM"; break;
                    case controladoraLinear.LOG_LIGADO: lblTipoEvento.Text = "LOG_LIGADO"; break;
                    case controladoraLinear.LOG_DESP_PORT: lblTipoEvento.Text = "LOG_DESP_PORT"; break;
                    case controladoraLinear.LOG_MUDANCA_PROG: lblTipoEvento.Text = "LOG_MUDANCA_PROG"; break;
                    case controladoraLinear.LOG_VAGO_5: lblTipoEvento.Text = "LOG_VAGO_5"; break;
                    case controladoraLinear.LOG_ACIONAMENTO_PC: lblTipoEvento.Text = "LOG_ACIONAMENTO_PC"; break;
                    case controladoraLinear.LOG_CONT_ATUALIZACAO: lblTipoEvento.Text = "LOG_CONT_ATUALIZACAO"; break;
                    case controladoraLinear.LOG_CLONAGEM: lblTipoEvento.Text = "LOG_CLONAGEM"; break;
                    case controladoraLinear.LOG_PANICO: lblTipoEvento.Text = "LOG_PANICO"; break;
                    case controladoraLinear.LOG_SDCARD_REMOVIDO: lblTipoEvento.Text = "LOG_SDCARD_REMOVIDO"; break;
                    case controladoraLinear.LOG_RESTORE: lblTipoEvento.Text = "LOG_RESTORE"; break;
                    case controladoraLinear.LOG_ERRO_DE_GRAVACAO: lblTipoEvento.Text = "LOG_ERRO_DE_GRAVACAO"; break;
                    case controladoraLinear.LOG_BACKUP_AUTO: lblTipoEvento.Text = "LOG_BACKUP_AUTO"; break;
                    case controladoraLinear.LOG_BACKUP_MANUAL: lblTipoEvento.Text = "LOG_BACKUP_MANUAL"; break;
                    case controladoraLinear.LOG_SERIAL_NAO_CADASTRADO: lblTipoEvento.Text = "LOG_SERIAL_NAO_CADASTRADO"; break;
                    case controladoraLinear.LOG_DUPLAPASSAGEM: lblTipoEvento.Text = "LOG_DUPLAPASSAGEM"; break;
                    case controladoraLinear.LOG_NAO_HABILITADO: lblTipoEvento.Text = "LOG_NAO_HABILITADO"; break;
                    case controladoraLinear.LOG_BOOTLOADER: lblTipoEvento.Text = "LOG_BOOTLOADER"; break;
                    case controladoraLinear.LOG_ALARME_ENTRADA_DIGITAL: lblTipoEvento.Text = "LOG_ALARME_ENTRADA_DIGITAL"; break;
                    case controladoraLinear.LOG_RESET_WATCHDOG_TIMER: lblTipoEvento.Text = "LOG_RESET_WATCHDOG_TIMER"; break;
                    case controladoraLinear.LOG_ATUALIZACAO_ASSINCRONA: lblTipoEvento.Text = "LOG_ATUALIZACAO_ASSINCRONA"; break;
                    default: lblTipoEvento.Text = "FIM LOGS"; break;
                }

                // modo operacao
                switch (modo)
                {
                    case controladoraLinear.MODO_CATRACA: lblModoOperacao.Text = "MODO_CATRACA"; break;
                    case controladoraLinear.MODO_CTRL_PORTA: lblModoOperacao.Text = "MODO_CTRL_PORTA"; break;
                    case controladoraLinear.MODO_CTRL_CANCELA: lblModoOperacao.Text = "MODO_CTRL_CANCELA"; break;
                }

                // endereco
                lblEnderecoPlaca.Text = Convert.ToString(endereco + 1);

                lblSetor.Text = Convert.ToString(setor + 1);

                // tipo disp
                switch( tipo )
                {
                    case controladoraLinear.DISP_TX: lblTipoDispositivo.Text = "DISP_TX"; break;
                    case controladoraLinear.DISP_TA: lblTipoDispositivo.Text = "DISP_TA"; break;
                    case controladoraLinear.DISP_CT: lblTipoDispositivo.Text = "DISP_CT"; break;
                    case controladoraLinear.DISP_BM: lblTipoDispositivo.Text = "DISP_BM"; break;
                    case controladoraLinear.DISP_TP: lblTipoDispositivo.Text = "DISP_TP"; break;
                    case controladoraLinear.DISP_SN: lblTipoDispositivo.Text = "DISP_SN"; break;
                }

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
                    serialTemp[5] = (byte)(serial);
                }
                else
                {
                    serialTemp[0] = (byte)(serial >> 40);
                    serialTemp[1] = (byte)(serial >> 32);
                    serialTemp[2] = (byte)(serial >> 24);
                    serialTemp[3] = (byte)(serial >> 16);
                    serialTemp[4] = (byte)(serial >> 8);
                    serialTemp[5] = (byte)(serial);
                }
                contadorTemp[0] = (byte)(contadorHCS >> 8);
                contadorTemp[1] = (byte)(contadorHCS);

                lblNumeroSerie.Text = BytesToHexString(serialTemp);

                if (tipo == controladoraLinear.DISP_TX)
                {
                    lblContador.Text = BytesToHexString(contadorTemp);
                }
                else
                {
                    lblContador.Text = " -- ";
                }

                // data e hora
                lblDataHoraEvento.Text =
                                         dia.ToString("00") + "/" +
                                         mes.ToString("00") + "/" +
                                         ano.ToString("00") +
                                         "   " +
                                         hora.ToString("00") + ":" +
                                         minuto.ToString("00") + ":" +
                                         segundo.ToString("00");

                // nivel
                lblNivel.Text = Convert.ToString(nivel);

                // info evento
                if((tipo == controladoraLinear.DISP_TX)||(tipo == controladoraLinear.DISP_TA))
                {
                    if (flagsEvento.bateriaFraca == 0)
                    {
                        lblBateriaEvento.Text = "OK";
                    }
                    else
                    {
                        lblBateriaEvento.Text = "Fraca";
                    }
                }
                else
                {
                    lblBateriaEvento.Text = " -- ";
                }

                // saída acionada
                lblSaidaAcionada.Text = Convert.ToString( flagsEvento.leitoraAcionada + 1 );

                // evento lido
                if( flagsEvento.eventoLido == 1 )
                {
                    lblEventoLido.Text = "Sim";
                }
                else
                {
                    lblEventoLido.Text = "Não";
                }

                if (evento == 7)
                {
                    int m = 0;
                    m++;
                }

                // info evento
                switch (evento)
                {
                    case controladoraLinear.LOG_DISP_ACIONADO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_ENTRADA: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_ENTRADA"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_SAIDA: lblInformacaoEvento.Text   = "SUB_LOG_ACIONAMENTO_SAIDA"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_BOTAO_1: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_BOTAO_1"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_BOTAO_2: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_BOTAO_2"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_BOTAO_3: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_BOTAO_3"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_BOTAO_4: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_BOTAO_4"; break;
                            case controladoraLinear.SUB_LOG_AGUARDANDO_SEGUNDA_VALIDACAO: lblInformacaoEvento.Text = "SUB_LOG_AGUARDANDO_SEGUNDA_VALIDACAO"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_PASSAGEM:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_PASSAGEM_ENTRADA_1: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_ENTRADA_1"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_1: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_1"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_ENTRADA_2: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_ENTRADA_2"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_2: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_2"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_ENTRADA_3: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_ENTRADA_3"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_3: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_3"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_ENTRADA_4: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_ENTRADA_4"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_4: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_4"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_TOUT: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_TOUT"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_LIVRE: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_LIVRE"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_BOTAO_1: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_BOTAO_1"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_BOTAO_2: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_BOTAO_2"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_BOTAO_3: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_BOTAO_3"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_BOTAO_4: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_BOTAO_4"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_ENTRADA_APB_DESLIGADO: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_ENTRADA_APB_DESLIGADO"; break;
                            case controladoraLinear.SUB_LOG_PASSAGEM_SAIDA_APB_DESLIGADO: lblInformacaoEvento.Text = "SUB_LOG_PASSAGEM_SAIDA_APB_DESLIGADO"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_LIGADO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_DESP_PORT:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_MUDANCA_PROG:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_VAGO_5:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_ACIONAMENTO_PC:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_OK: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_OK"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_ERRO: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_ERRO"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_OK: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_OK"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_ERRO: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_ERRO"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_RELE_5: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_RELE_5"; break;
                            case controladoraLinear.SUB_LOG_ACIONAMENTO_REMOTO_RELE_6: lblInformacaoEvento.Text = "SUB_LOG_ACIONAMENTO_REMOTO_RELE_6"; break;
                                    
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_CONT_ATUALIZACAO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "SUB_LOG_INCONSISTENCIA_ENTRE_BASES_DE_DADOS"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_CLONAGEM:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_PANICO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_PANICO: lblInformacaoEvento.Text = "SUB_LOG_PANICO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_NAO_ATENDIDO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_NAO_ATENDIDO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_CANCELADO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_CANCELADO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_2X_CARTAO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_2X_CARTAO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_IMEDIATO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_IMEDIATO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_TEMPORIZADO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_TEMPORIZADO"; break;
                            case controladoraLinear.SUB_LOG_PANICO_DISPOSITIVO_PANICO: lblInformacaoEvento.Text = "SUB_LOG_PANICO_DISPOSITIVO_PANICO"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_SDCARD_REMOVIDO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_RESTORE:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_ERRO_DE_GRAVACAO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1: lblInformacaoEvento.Text = "SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1"; break;
                            case controladoraLinear.SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_2: lblInformacaoEvento.Text = "SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1"; break;
                            case controladoraLinear.SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_3: lblInformacaoEvento.Text = "SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1"; break;
                            case controladoraLinear.SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_4: lblInformacaoEvento.Text = "SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1"; break;
                            case controladoraLinear.SUB_LOG_ERRO_DE_GRAVACAO_ATUALIZACAO_DE_BIOMETRIA_CANCELADA: lblInformacaoEvento.Text = "SUB_LOG_ERRO_DE_GRAVACAO_ATUALIZACAO_DE_BIOMETRIA_CANCELADA"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_BACKUP_AUTO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_BACKUP_AUTO_OK: lblInformacaoEvento.Text = "SUB_LOG_BACKUP_AUTO_OK"; break;
                            case controladoraLinear.SUB_LOG_BACKUP_AUTO_SD_CARD_NAO_ENCONTRADO: lblInformacaoEvento.Text = "SUB_LOG_BACKUP_AUTO_SD_CARD_NAO_ENCONTRADO"; break;
                            case controladoraLinear.SUB_LOG_BACKUP_AUTO_FALHA: lblInformacaoEvento.Text = "SUB_LOG_BACKUP_AUTO_FALHA"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_BACKUP_MANUAL:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_BACKUP_MANUAL_SUCESSO: lblInformacaoEvento.Text = "SUB_LOG_BACKUP_MANUAL_SUCESSO"; break;
                            case controladoraLinear.SUB_LOG_BACKUP_MANUAL_FALHA: lblInformacaoEvento.Text = "SUB_LOG_BACKUP_MANUAL_FALHA"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_SERIAL_NAO_CADASTRADO:
                    {
                        switch (tipo)
                        {
                            case controladoraLinear.DISP_TX: cboxTipoDispositivo.SelectedIndex = 0; break;
                            case controladoraLinear.DISP_TA: cboxTipoDispositivo.SelectedIndex = 1; break;
                            case controladoraLinear.DISP_CT: cboxTipoDispositivo.SelectedIndex = 2; break;
                            case controladoraLinear.DISP_BM: cboxTipoDispositivo.SelectedIndex = 3; break;
                            case controladoraLinear.DISP_TP: cboxTipoDispositivo.SelectedIndex = 4; break;
                            case controladoraLinear.DISP_SN: cboxTipoDispositivo.SelectedIndex = 5; break;
                        }

                        txtSerial.Text = lblNumeroSerie.Text;
                        txtIdentificacao.Text = lblNumeroSerie.Text;

                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_NAO_CADASTRADO: lblInformacaoEvento.Text = "SUB_LOG_NAO_CADASTRADO"; break;
                            case controladoraLinear.SUB_LOG_LEITORA_EXPEDIDORA: lblInformacaoEvento.Text = "SUB_LOG_LEITORA_EXPEDIDORA"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                    case controladoraLinear.LOG_DUPLAPASSAGEM:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_ENTRADA_1: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_ENTRADA_1"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_SAIDA_1: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_SAIDA_1"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_ENTRADA_2: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_ENTRADA_2"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_SAIDA_2: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_SAIDA_2"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_ENTRADA_3: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_ENTRADA_3"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_SAIDA_3: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_SAIDA_3"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_ENTRADA_4: lblInformacaoEvento.Text = "SUB_DUPLA_LOG_PASSAGEM_ENTRADA_4"; break;
                            case controladoraLinear.SUB_LOG_DUPLA_PASSAGEM_SAIDA_4: lblInformacaoEvento.Text = "SUB_LOG_DUPLA_PASSAGEM_SAIDA_4"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_NAO_HABILITADO:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.INVALIDO_NAO_CADASTRADO: lblInformacaoEvento.Text = "INVALIDO_NAO_CADASTRADO"; break;
                            case controladoraLinear.INVALIDO_LEITORA_COFRE: lblInformacaoEvento.Text = "INVALIDO_LEITORA_COFRE"; break;
                            case controladoraLinear.INVALIDO_ANTI_PASSBACK: lblInformacaoEvento.Text = "INVALIDO_ANTI_PASSBACK"; break;
                            case controladoraLinear.INVALIDO_SEM_CREDITOS: lblInformacaoEvento.Text = "INVALIDO_SEM_CREDITOS"; break;
                            case controladoraLinear.INVALIDO_DATA_VALIDADE: lblInformacaoEvento.Text = "INVALIDO_DATA_VALIDADE"; break;
                            case controladoraLinear.INVALIDO_TEMPO_ANTICARONA: lblInformacaoEvento.Text = "INVALIDO_TEMPO_ANTICARONA"; break;
                            case controladoraLinear.INVALIDO_LEITORA_NAO_HABILITADA: lblInformacaoEvento.Text = "INVALIDO_LEITORA_NAO_HABILITADA"; break;
                            case controladoraLinear.INVALIDO_FERIADO: lblInformacaoEvento.Text = "INVALIDO_FERIADO"; break;
                            case controladoraLinear.INVALIDO_JORNADA_TURNO: lblInformacaoEvento.Text = "INVALIDO_JORNADA_TURNO"; break;
                            case controladoraLinear.INVALIDO_SEM_VAGAS_NIVEL: lblInformacaoEvento.Text = "INVALIDO_SEM_VAGAS_NIVEL"; break;
                            case controladoraLinear.INVALIDO_REMOTO: lblInformacaoEvento.Text = "INVALIDO_REMOTO"; break;
                            case controladoraLinear.INVALIDO_LEITORA_INIBIDA: lblInformacaoEvento.Text = "INVALIDO_LEITORA_INIBIDA"; break;
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_BOOTLOADER:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_ALARME_ENTRADA_DIGITAL:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_ALARME_ED_1: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ED_1"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ED_2: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ED_2"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ED_3: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ED_3"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ED_4: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ED_4"; break;
							case controladoraLinear.SUB_LOG_ALARME_ARROMBAMENTO_1: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ARROMBAMENTO_1"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ARROMBAMENTO_2: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ARROMBAMENTO_2"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ARROMBAMENTO_3: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ARROMBAMENTO_3"; break;
                            case controladoraLinear.SUB_LOG_ALARME_ARROMBAMENTO_4: lblInformacaoEvento.Text = "SUB_LOG_ALARME_ARROMBAMENTO_4"; break;
							default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_RESET_WATCHDOG_TIMER:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;

                    case controladoraLinear.LOG_ATUALIZACAO_ASSINCRONA:
                    {
                        switch (flagsEvento.infoEvento)
                        {
                            case controladoraLinear.SUB_LOG_ATUALIZACAO_FALHA_DE_GRAVACAO: lblInformacaoEvento.Text = "SUB_LOG_ATUALIZACAO_FALHA_DE_GRAVACAO"; break;
                            case controladoraLinear.SUB_LOG_ATUALIZACAO_FALHA_AO_LER_ARQUIVO: lblInformacaoEvento.Text = "SUB_LOG_ATUALIZACAO_FALHA_AO_LER_ARQUIVO"; break;
                            case controladoraLinear.SUB_LOG_ATUALIZACAO_CONCLUIDA_COM_SUCESSO: lblInformacaoEvento.Text = "SUB_LOG_ATUALIZACAO_CONCLUIDA_COM_SUCESSO"; break;
                            case controladoraLinear.SUB_LOG_ATUALIZACAO_SERIAL_FORA_DO_LIMITE: lblInformacaoEvento.Text = "SUB_LOG_ATUALIZACAO_SERIAL_FORA_DO_LIMITE"; break;
                            case controladoraLinear.SUB_LOG_ATUALIZACAO_SEM_BIOMETRIA: lblInformacaoEvento.Text = "SUB_LOG_ATUALIZACAO_SEM_BIOMETRIA"; break;                                
                            default: lblInformacaoEvento.Text = "---"; break;
                        }
                    }
                    break;
                }

                if ( ckMostrarEventoAutomatico.Checked == true )
                {
                    ListViewItem novoEvento = new ListViewItem();
                    novoEvento.Text = lblTipoEvento.Text;
                    novoEvento.SubItems.Add(lblModoOperacao.Text);
                    novoEvento.SubItems.Add(lblEnderecoPlaca.Text);
                    novoEvento.SubItems.Add(lblTipoDispositivo.Text);
                    novoEvento.SubItems.Add(lblNumeroSerie.Text);
                    novoEvento.SubItems.Add(lblContador.Text);
                    novoEvento.SubItems.Add(lblNivel.Text);
                    novoEvento.SubItems.Add(lblDataHoraEvento.Text);
                    novoEvento.SubItems.Add(lblBateriaEvento.Text);
                    novoEvento.SubItems.Add(lblSaidaAcionada.Text);
                    novoEvento.SubItems.Add(lblEventoLido.Text);
                    novoEvento.SubItems.Add(lblInformacaoEvento.Text);
                    novoEvento.Tag = evento;

                    if( lstEventos.Items.Count > 0 )
                    {
                        lstEventos.Items.Insert( 0, novoEvento );
                    }
                    else
                    {
                        lstEventos.Items.Add( novoEvento );
                    }
                }

                if (lblNumeroSerie.Text != "000000000000")
                    EventoAoReceberSerial(lblNumeroSerie.Text, lblEnderecoPlaca.Text, lblSaidaAcionada.Text);


                if (( tabPage.SelectedTab.Text == "Controle de Vagas" )&&(evento == controladoraLinear.LOG_PASSAGEM)&&( flagsEvento.infoEvento < controladoraLinear.SUB_LOG_PASSAGEM_TOUT ))
                {
                    try
                    {
                        byte[] pacote = new byte[1024];
                        buf = pControl.operacaoPacoteVagasHabilitação(controladoraLinear.OP_LEITURA, 0, pacote);
                        enviarMensagem(buf);
                    }
                    catch { };

                    try
                    {
                        byte[] pacote = new byte[1024];
                        buf = pControl.operacaoPacoteVagasHabilitação(controladoraLinear.OP_LEITURA, 1, pacote);
                        enviarMensagem(buf);
                    }
                    catch { };

                }
                
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_acionamento(byte[] buf)
        {
            if (repetir > 0)
            {
                repetir--;
                acionamentoEntradaSaida();
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 76 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +
        //     <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs       [37 bytes]
        public void trataRespostaCmd_lerDispositivo(byte[] buf)
        {
            try
            {
                bool dispositivoInvalido = false;
                byte[] serialTemp = new byte[6];
                byte[] contadorTemp = new byte[2];

                switch (buf[3])
                {
                    case controladoraLinear.DISP_TX: cboxTipoDispositivo.SelectedIndex = 0; break;
                    case controladoraLinear.DISP_TA: cboxTipoDispositivo.SelectedIndex = 1; break;
                    case controladoraLinear.DISP_CT: cboxTipoDispositivo.SelectedIndex = 2; break;
                    case controladoraLinear.DISP_BM: cboxTipoDispositivo.SelectedIndex = 3; break;
                    case controladoraLinear.DISP_TP: cboxTipoDispositivo.SelectedIndex = 4; break;
                    case controladoraLinear.DISP_SN: cboxTipoDispositivo.SelectedIndex = 5; break;
                    default: dispositivoInvalido = true; break;
                }

                int i;

                contadorTemp[0] = 0;
                contadorTemp[1] = 0;
                if( buf[3] == controladoraLinear.DISP_TX )
                {
                    serialTemp[2] = buf[4 + 0];
                    serialTemp[3] = buf[4 + 1];
                    serialTemp[4] = buf[4 + 2];
                    serialTemp[5] = buf[4 + 3];
                    contadorTemp[0] = buf[8];
                    contadorTemp[1] = buf[9];
                }
                else
                {
                    serialTemp[0] = buf[4 + 0];
                    serialTemp[1] = buf[4 + 1];
                    serialTemp[2] = buf[4 + 2];
                    serialTemp[3] = buf[4 + 3];
                    serialTemp[4] = buf[4 + 4];
                    serialTemp[5] = buf[4 + 5];
                }

                txtSerial.Text = BytesToHexString(serialTemp);
                try
                {
                    txtSerial.Text = txtSerial.Text.Substring(1, 11);
                }
                catch { };

                txtContadorHCS.Text = BytesToHexString(contadorTemp);

                UInt16 codigoRota = ((UInt16)((buf[10] << 8) + buf[11]));
                if( codigoRota < controladoraLinear.N_LINES_HABILITACAO )
                {
                    cboxRota.SelectedIndex = ((UInt16)((buf[10] << 8) + buf[11]));
                }
                else
                {
                    MessageBox.Show("Erro: Código de habilitacao fora do limite(Rota 0..415)! (Rota =" + Convert.ToString(controladoraLinear.N_LINES_HABILITACAO) + ")");
                    codigoRota = 0;
                }
            
                // flags cadastro
                ckBoxSaidaCofre.Checked = false;                
                if ((buf[12] & 0x01) > 0) ckBoxSaidaCofre.Checked = true;
                
                cboxAntipassback.SelectedIndex = (byte)((buf[12] >> 1) & 0x03);

                checkBoxDispositivos_visitante.Checked = false;
                if ((buf[12] & 0x08) > 0) checkBoxDispositivos_visitante.Checked = true;

                checkBoxDispositivos_controleVagasId.Checked = false;
                if ((buf[12] & 0x10) > 0) checkBoxDispositivos_controleVagasId.Checked = true;

                checkBoxDispositivos_duplaValidacao.Checked = false;
                if ((buf[12] & 0x40) > 0) checkBoxDispositivos_duplaValidacao.Checked = true;
                
                tratalabelsAcionamento( buf[3], buf[13]);

                txtNivel.Text = Convert.ToString(buf[14]);
                txtCréditos.Text = Convert.ToString(buf[15]);

                if ( (buf[17] > 12) || (buf[20] > 12) || (buf[17] == 0) || (buf[20] == 0) ||
                     (buf[16] > 31) || (buf[19] > 31) || (buf[16] == 0) || (buf[19] == 0) ||
                     (buf[18] > 99) || (buf[21] > 99 ) )
                {
                    MessageBox.Show("Erro: " + "Data de válidade inválida");
                }
                else
                {
                    dateTimePicker1.Value = new DateTime(((int)(2000 + buf[18])), (int)(buf[17]), (int)(buf[16]));
                    dateTimePicker2.Value = new DateTime(((int)(2000 + buf[21])), (int)(buf[20]), (int)(buf[19]));
                }

                txtIdentificacao.Text = null;
                for(i=0;i<14;i++) txtIdentificacao.Text += (char)buf[22+i];


                if (dispositivoInvalido == true)
                {
                    MessageBox.Show("Erro: " + "Dispositivo vago ou inválido (" + Convert.ToString(buf[3]) + ")");
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
        public void trataRespostaCmd_operacaoParametroSetup(byte[] buf)
        {
            try
            {
                int i;
                byte operacao = controladoraLinear.OP_LEITURA;
                UInt16 indiceParametro;
                UInt16 tamanhoParametro;
                if (cboxOperacao_parametros.SelectedIndex == 1) operacao = controladoraLinear.OP_ESCRITA;
                indiceParametro = (UInt16)pControl.parametrosSetup[cboxParametro.SelectedIndex];
                tamanhoParametro = (UInt16)pControl.sizeOfparametrosSetup[cboxParametro.SelectedIndex];

                if (operacao == controladoraLinear.OP_LEITURA)
                {
                    if (tamanhoParametro == 1)
                    {
                        txtValorParametro.Text = Convert.ToString(buf[8]);
                    }
                    else if (tamanhoParametro == 2)
                    {
                        txtValorParametro.Text = Convert.ToString(((buf[8] << 8) + buf[9]));
                    }
                    else if (tamanhoParametro == 4)
                    {
                        byte[] hexString = new byte[4];
                        hexString[0] = buf[8];
                        hexString[1] = buf[9];
                        hexString[2] = buf[10];
                        hexString[3] = buf[11];
                        txtValorParametro.Text = BytesToHexString(hexString);
                    }
                    else if (tamanhoParametro > 4)
                    {
                        StringBuilder s = new StringBuilder();
                        s.Capacity = tamanhoParametro;
                        for (i = 0; i < tamanhoParametro; i++) s.Append((char)buf[8+i]);
                        txtValorParametro.Text = s.ToString();
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 79 + <retorno> + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)>+  cs 
        public void trataRespostaCmd_operacaoRotaIndividual(byte[] buf)
        {
            try
            {
                byte operacao = buf[3];
                if (operacao == controladoraLinear.OP_LEITURA )
                {
                    ckLstHabilitacaoRota2.SetItemChecked(0, false);
                    ckLstHabilitacaoRota2.SetItemChecked(1, false);
                    ckLstHabilitacaoRota2.SetItemChecked(2, false);
                    ckLstHabilitacaoRota2.SetItemChecked(3, false);
                    if ((buf[7] & 0x01) > 0) ckLstHabilitacaoRota2.SetItemChecked(0, true);
                    if ((buf[7] & 0x02) > 0) ckLstHabilitacaoRota2.SetItemChecked(1, true);
                    if ((buf[7] & 0x04) > 0) ckLstHabilitacaoRota2.SetItemChecked(2, true);
                    if ((buf[7] & 0x08) > 0) ckLstHabilitacaoRota2.SetItemChecked(3, true);

                    int i;
                    StringBuilder s = new StringBuilder();
                    for(i=0;i<8;i++) s.Append((char)buf[8+i]);
                    txtLabelRota.Text = s.ToString();
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 7A + <retorno> + <cód. do feriado> + cs
        public void trataRespostaCmd_editaDataFeriadoIndividual(byte[] buf)
        {            

        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 7B + <retorno> + <operação> +  <cód. MsgDisplayExterno> + <mensagemDisplayExterno(32bytes)> + cs		
        public void trataRespostaCmd_operacaoMensagensDisplayExterno(byte[] buf)
        {
            try
            {
                StringBuilder s = new StringBuilder();
                int i;
                for (i = 0; i < 16; i++) s.Append((char)buf[5+i ]);
                txtLinha1Display.Text = s.ToString();
                s.Clear();
                for (i = 0; i < 16; i++) s.Append((char)buf[5+16 + i]);
                txtLinha2Display.Text = s.ToString();
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void  trataRespostaCmd_operacaoPacoteGrupoLeitoras( byte[] buf )
        {
            UInt16 indiceLimite = ((controladoraLinear.SIZE_OF_GRUPO_LEITORA * controladoraLinear.N_LINES_GRUPO_LEITORA) / 512);
            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_GRUPO_LEITORA, controladoraLinear.FILE_GRUPOS, indiceLimite, controladoraLinear.DIV_OF_GRUPO_LEITORA);

        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_operacaoPacoteHabilitacao(byte[] buf)
        {
            UInt16 indiceLimite = ((controladoraLinear.SIZE_OF_HABILITACAO * controladoraLinear.N_LINES_HABILITACAO) / 512);
            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_HABILITACAO, controladoraLinear.FILE_ROTAS, indiceLimite, controladoraLinear.DIV_OF_HABILITACAO);
        }        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_operacaoPacoteLabelsRota(byte[] buf)
        {
            UInt16 indiceLimite = ((controladoraLinear.SIZE_OF_LABEL_ROTAS * controladoraLinear.N_LINES_LABEL_ROTAS) / 512);
            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_LABEL_ROTAS, controladoraLinear.FILE_LABELS, indiceLimite, controladoraLinear.DIV_OF_LABEL_ROTAS);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 128	
        // Editar habilitação e grupo da leitora
        // RSP: 00 + 80 + <retorno> + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> + 
        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + cs      [21 bytes]
        public void trataRespostaCmd_operacaoRotaGrupoLeitoras(byte[] buf)
        {
            try
            {
                byte operacao = buf[3];
                if (operacao == controladoraLinear.OP_LEITURA)
                {
                    ckLstHabilitacaoRota2.SetItemChecked(0, false);
                    ckLstHabilitacaoRota2.SetItemChecked(1, false);
                    ckLstHabilitacaoRota2.SetItemChecked(2, false);
                    ckLstHabilitacaoRota2.SetItemChecked(3, false);
                    if ((buf[7] & 0x01) > 0) ckLstHabilitacaoRota2.SetItemChecked(0, true);
                    if ((buf[7] & 0x02) > 0) ckLstHabilitacaoRota2.SetItemChecked(1, true);
                    if ((buf[7] & 0x04) > 0) ckLstHabilitacaoRota2.SetItemChecked(2, true);
                    if ((buf[7] & 0x08) > 0) ckLstHabilitacaoRota2.SetItemChecked(3, true);

                    cboxJornadaL1_ROTA2.SelectedIndex = buf[8];
                    cboxJornadaL2_ROTA2.SelectedIndex = buf[9];
                    cboxJornadaL3_ROTA2.SelectedIndex = buf[10];
                    cboxJornadaL4_ROTA2.SelectedIndex = buf[11];

                    int i;
                    StringBuilder s = new StringBuilder();
                    for (i = 0; i < 8; i++) s.Append((char)buf[12 + i]);
                    txtLabelRota.Text = s.ToString();
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 129	Ler dados do SD CARD (PROGRESSIVO)
        // 00 + 81 + <retorno> + <arquivo sdcard> + <dia> + <mês> + <ano> + <hora> + <minutos> + <segundos> +  <tamanhoLinhaH> + tamanhoLinhaL> + <linha do arquivo> + cs    [13 bytes + linha do arquivo]
        public void trataRespostaCmd_lerDadosSDCARD(byte[] buf)
        {
            byte retorno = buf[2];
            int tamanhoLinha = (UInt16)((buf[10] << 8) + buf[11]);
            int i;

            if (retorno == controladoraLinear.RT_OK )
            {
                String sTemp = null;

                for (i = 0; i < tamanhoLinha; i++)
                {
                    if ((buf[12 + i] != 0x0D) && (buf[12 + i] != 0x0A))
                    {
                        sTemp += (char)buf[12 + i];
                    }
                    else
                    {
                        if ((buf[3] == controladoraLinear.FILE_INDEX ) && ((sTemp[0] != 'F') && (sTemp[1] != 'F')))
                        {
                            cboxDataArquivo.Items.Add(sTemp);
                            cboxDataArquivo.SelectedIndex = 0;
                            cboxDataArquivo.Enabled = true;
                            sTemp += '\r';
                            sTemp += '\n';
                            break;
                        }
                        else if (buf[12 + i] != 0x0D)
                        {
                            sTemp += '\n';
                        }
                        else if (buf[12 + i] != 0x0a)
                        {
                            sTemp += '\r';
                        }
                    }
                }

                try
                {
                    arquivoString.Append(sTemp);
                }
                catch (Exception e )
                {
                    MessageBox.Show( e.ToString() );
                }
                lerArquivoSDCARD();
            }
            else if (retorno == controladoraLinear.RT_FIM_DO_ARQUIVO )
            {
                salvaArquivo( cboxArquivo.SelectedIndex + 1, arquivoString.ToString() );
                arquivoString = new StringBuilder();
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 131	Ler / Gravar pacote de Vagas por Habilitação  (PACOTE DE DADOS)	
        // CMD: 00 + 83 + <operacao> + 00 + <indicePacote_L(de 0 a 3)> + <pacote(512 bytes)> + cs  [518 bytes]
        // RSP:	00 + 83 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 3)> + <pacote(512 bytes)> + cs [519 bytes]
        public void trataRespostaCmd_operacaoPacoteVagasHabilitação(byte[] buf)
        {
            if (buf[3] == controladoraLinear.OP_LEITURA)
            {
                int j = 0;
                j = 256 * buf[5];
                for (int i = 0; i < 256; i += 2)
                {
                    listView_vagas.Items[j].SubItems[1].Text = buf[6 + i].ToString("000");
                    listView_vagas.Items[j].SubItems[2].Text = buf[6 + i + 1].ToString("000");
                    j++;
                }
            }
            else
            {
                int k = 0;
                k++;
            }

        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 133	Ler evento/Marcar evento como lido/Ler último evento não lido (EVENTO INDEXADO)
        // CMD: 00 + 85 + <operacao> + <endereçoEventoH> + <endereçoEventoL> + cs     [6 bytes]
        // RSP: 00 + 85 + <retorno> + <operacao> + <cntAtual> + <frame Evento(16 bytes)> + <endereçoEventoH> + <endereçoEventoL> + cs    [24 bytes]
        public void trataRespostaCmd_operacaoEvento( byte[] buf, String epOrigem )
        {
            if (buf[5] == 0xFF) return;

            if(( buf[3] == controladoraLinear.OP_LER_EVENTO_INDEXADO )||( buf[3] == controladoraLinear.OP_LER_ULTIMO_EVENTO_NAO_LIDO ))
            {                
                byte[] buf2 = new byte[21];
                for(int i=0;i<16;i++) buf2[4+i] = buf[5+i];
                buf2[1] = 0x69;
                buf2[2] = buf[2];
                buf2[3] = 0;

                if (buf[3] == controladoraLinear.OP_LER_ULTIMO_EVENTO_NAO_LIDO)
                {
                    comboBoxEvento_indiceEvento.SelectedIndex = (int)((buf[21] << 8) + buf[22]);
                }


                // RSP: 00 + 69 + <retorno> + <marca> + <frame de evt. (16 bytes)> + cs       [21 bytes]
                trataRespostaCmd_lerEventos( buf2, epOrigem );
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void salvaArquivo(int indiceArquivo, String sArquivo)
        {
            int i;
            Boolean teste = true;
            Stream myStream;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName =
            saveFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
            saveFileDialog1.Title = "Salvar no diretorio:";

            switch ( indiceArquivo )
            {
                case controladoraLinear.FILE_INDEX: saveFileDialog1.FileName = "INDEX.TXT"; break;
                case controladoraLinear.FILE_DISP: saveFileDialog1.FileName = "DISP.DPT"; break;
                case controladoraLinear.FILE_SETUP: saveFileDialog1.FileName = "SETUP.STP"; break;
                case controladoraLinear.FILE_EVENTO: saveFileDialog1.FileName = "EVENT.EVT"; break;
                case controladoraLinear.FILE_ROTAS: saveFileDialog1.FileName = "ROTAS.TXT"; break;
                case controladoraLinear.FILE_GRUPOS: saveFileDialog1.FileName = "GRUPO.TXT"; break;
                case controladoraLinear.FILE_JORNADA: saveFileDialog1.FileName = "JORNA.TXT"; break;
                case controladoraLinear.FILE_TURNOS: saveFileDialog1.FileName = "TURNO.TXT"; break;
                case controladoraLinear.FILE_FERIADOS: saveFileDialog1.FileName = "FERIA.TXT"; break;
                case controladoraLinear.FILE_LABELS: saveFileDialog1.FileName = "LABEL.TXT"; break;
                case controladoraLinear.FILE_DISPLAY: saveFileDialog1.FileName = "DISPL.TXT"; break;
                case controladoraLinear.FILE_CSV: saveFileDialog1.FileName = "EXCEL.CSV"; break;
                case controladoraLinear.FILE_MIAXIS: saveFileDialog1.FileName = "MIAXIS.BIO"; break;
                case controladoraLinear.FILE_VIRDI: saveFileDialog1.FileName = "VIRDI.BIO"; break;
                case controladoraLinear.FILE_NITGEN: saveFileDialog1.FileName = "NITGEN.BIO"; break;
                case controladoraLinear.FILE_SUPREMA: saveFileDialog1.FileName = "SUPREM.BIO"; break;
                case controladoraLinear.FILE_ANVIZ: saveFileDialog1.FileName = "ANVIZ.BIO"; break;
                case controladoraLinear.FILE_DTURNOS: saveFileDialog1.FileName = "DTURNO.TXT"; break;
                default: teste = false; break;
            }

            if (teste == true)
            {
                saveFileDialog1.DefaultExt = "TXT";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (sArquivo.Length > 0)
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        if ((myStream = saveFileDialog1.OpenFile()) != null)
                        {
                            byte[] buf2 = new byte[sArquivo.Length];
                            for (i = 0; i < sArquivo.Length; i++) buf2[i] = (Convert.ToByte(sArquivo[i]));
                            // Code to write the stream goes here.
                            myStream.Write(buf2, 0, sArquivo.Length);
                            myStream.Close();
                        }
                    }
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 6C + <retorno> + <operacao> +  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs
        public void trataRespostaCmd_lerBiometria(byte[] buf)
        {
            try
            {
                byte operacao = buf[3];
                int i;
                int tamanhoTemplate = (UInt16)((buf[38] << 8) + buf[39]);
                byte[] bufTemp = new byte[tamanhoTemplate];

                for (i = 0; i < tamanhoTemplate; i++) bufTemp[i] += buf[40 + i];
                /*
                0 MIAXIS(1)
                1 VIRDI(2)
                2 SUPREMA(3)
                3 NITGEN(4) // não implementado
                4 ANVIZ(5)
                5 T5S(6) 
                */

                cboxIndiceBM.SelectedIndex = (buf[4]);
                if (cboxIndiceBM.SelectedIndex > cboxIndiceBM.Items.Count) cboxIndiceBM.SelectedIndex = 0;

                cboxModuloBio.SelectedIndex = (buf[5] - 1);
                if (cboxModuloBio.SelectedIndex > cboxModuloBio.Items.Count ) cboxModuloBio.SelectedIndex = 0;
                
                rTxtTemplate.Clear();
                StringBuilder sb = new StringBuilder();
                String msg = BytesToHexString(bufTemp);
                int n = msg.Length;
                for (i = 0; i < n; i+=2)
                {
                    sb.Append(msg[i]);
                    sb.Append(msg[i + 1]);
                    sb.Append(' ');
                }
                rTxtTemplate.Text = sb.ToString();

                if(( operacao == controladoraLinear.OP_BIOMETRIA_NAO_CADASTRADA)&&(tamanhoTemplate == 338 ))
                {
                    switch (matchTemplate())
                    {
                        case 0:
                            labelDispositivos_MatchScan.Text = "Template invalido!!";
                            break;

                        case 1:
                            labelDispositivos_MatchScan.Text = "Diferente do Scan!!";
                            break;

                        case 2:
                            labelDispositivos_MatchScan.Text = "Igual ao Scan!!";
                            break;
                    }
                }

                lblTamanhoTemplate.Text = "Tamanho: " + Convert.ToString(tamanhoTemplate) + " bytes";

                if (operacao == controladoraLinear.OP_LEITURA)
                {
                    bool dispositivoInvalido = false;
                    byte[] serialTemp = new byte[6];
                    byte[] contadorTemp = new byte[2];
                    byte tipo;

                    tipo = (byte)(buf[6] >> 4);
                    if (tipo != controladoraLinear.DISP_BM) dispositivoInvalido = true;
                    cboxTipoDispositivo.SelectedIndex = 3;

                    if (tipo == controladoraLinear.DISP_TX)
                    {
                        serialTemp[2] = (byte)(buf[6 + 0] & 0x0F);
                        serialTemp[3] = buf[6 + 1];
                        serialTemp[4] = buf[6 + 2];
                        serialTemp[5] = buf[6 + 3];
                    }
                    else
                    {
                        serialTemp[0] = (byte)(buf[6 + 0] & 0x0F);
                        serialTemp[1] = buf[6 + 1];
                        serialTemp[2] = buf[6 + 2];
                        serialTemp[3] = buf[6 + 3];
                        serialTemp[4] = buf[6 + 4];
                        serialTemp[5] = buf[6 + 5];
                    }

                    txtSerial.Text = BytesToHexString(serialTemp);
                    try
                    {
                        txtSerial.Text = txtSerial.Text.Substring(1, 11);
                    }
                    catch { };
                    txtContadorHCS.Text = BytesToHexString(contadorTemp);

                    UInt16 codigoRota = ((UInt16)((buf[12] << 8) + buf[13]));
                    if (codigoRota < controladoraLinear.N_LINES_HABILITACAO)
                    {
                        cboxRota.SelectedIndex = codigoRota;
                    }
                    else
                    {
                        lblErro.Text = "Erro: " + "Código de habilitacao fora do limite (" + Convert.ToString(codigoRota) + ")";
                        codigoRota = 0;
                    }

                    // flags cadastro
                    ckBoxSaidaCofre.Checked = false;
                    if ((buf[14] & 0x01) > 0) ckBoxSaidaCofre.Checked = true;
                    
                    cboxAntipassback.SelectedIndex = (byte)((buf[14] >> 1) & 0x03);
                    
                    checkBoxDispositivos_visitante.Checked = false;
                    if ((buf[12] & 0x08) > 0) checkBoxDispositivos_visitante.Checked = true;

                    checkBoxDispositivos_controleVagasId.Checked = false;
                    if ((buf[12] & 0x10) > 0) checkBoxDispositivos_controleVagasId.Checked = true;

                    checkBoxDispositivos_duplaValidacao.Checked = false;
                    if ((buf[12] & 0x40) > 0) checkBoxDispositivos_duplaValidacao.Checked = true;


                    // flags status
                    tratalabelsAcionamento(tipo, buf[15] );

                    // nivel
                    txtNivel.Text = Convert.ToString(buf[16]);
                    // créditos
                    txtCréditos.Text = Convert.ToString(buf[17]);
                    // data validade
                    if ((buf[18] > 31) || (buf[21] > 31) || (buf[18] == 0) || (buf[21] == 0) || // dia
                         (buf[19] > 12) || (buf[22] > 12) || (buf[19] == 0) || (buf[22] == 0) || // mês
                         (buf[20] > 99) || (buf[23] > 99)) // ano
                    {
                        lblErro.Text = "Erro: " + "Data de válidade inválida";
                    }
                    else
                    {
                        dateTimePicker1.Value = new DateTime(((int)(2000 + buf[20])), (int)(buf[19]), (int)(buf[18]));
                        dateTimePicker2.Value = new DateTime(((int)(2000 + buf[23])), (int)(buf[22]), (int)(buf[21]));
                    }

                    txtIdentificacao.Text = null;
                    for (i = 0; i < 14; i++) txtIdentificacao.Text += (char)buf[24 + i];

                    if (dispositivoInvalido == true)
                    {
                        lblErro.Text = "Erro: " + "Dispositivo vago ou inválido (" + Convert.ToString(tipo) + ")";
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void tratalabelsAcionamento(byte tipo, byte val)
        {
            if ((tipo == controladoraLinear.DISP_TX) || (tipo == controladoraLinear.DISP_TA))
            {
                if (( val & 0x0F) == 0)
                {
                    lblBateria.Text = "Bateria: OK";
                }
                else
                {
                    lblBateria.Text = "Bateria: Fraca";
                }
            }
            else
            {
                lblBateria.Text = "Bateria: -- ";
            }

            lblUltimoAcionamento.Text = "Último Acionamento: " + Convert.ToString((byte)(val >> 4));
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 6F + <retorno> + <nDisp> +  <modo> + <DNS (16 bytes)> + <IP(4bytes)> + <versão SW (6bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <MAC ADDRESS(8 bytes)> + <cs>
        private void trataRespostaCmd_lerInformacoes(byte[] buf)
        {
            try
            {
                int i;
                StringBuilder sb = new StringBuilder(2);
                StringBuilder sIp = new StringBuilder();
                for (i = 0; i < 4; i++)
                {
                    sIp.Append(Convert.ToString(buf[21 + i]));
                    if (i < 3) sIp.Append('.');
                }

                String ip = sIp.ToString();
                String host = ",HOST=";
                for (i = 0; i < 16; i++)
                {
                    if (buf[i + 5] != 0)
                    {
                        host += Convert.ToChar(buf[i + 5]);
                    }
                    else
                    {
                        host += ' ';
                    }
                }

                String portaTCPClient = ",TCP=" + Convert.ToString((buf[33] << 8) + buf[34]);
                String portaUDPClient = ",UDP=" + Convert.ToString((buf[31] << 8) + buf[32]);
                String versao = (",versão= " + (char)buf[25] + (char)buf[26] + (char)buf[27] + (char)buf[28] + (char)buf[29] + (char)buf[30]);
                sb.Clear();
                for (i = 0; i < 6; i++)
                {
                    sb.AppendFormat("{0:X2}", buf[35 + i]);
                    if (i < 5) sb.Append(":");
                }
                String mac = ",Mac=" + sb.ToString();
                String novoItem = "IP=" + ip + portaUDPClient + portaTCPClient + host + versao + mac;

                textBoxSetup_lerInfo.Text = novoItem;
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 00 + 70 + <retorno> + <operacao> + <vagas_atual_H> + <vagas_atual_L> + cs
        public void trataRespostaCmd_gravarLerContadorAtualVagas(byte[] buf)
        {
            try
            {
                if (cboxContadorAtualVagas.SelectedIndex == 0) // operacao = controladoraLinear.OP_LEITURA;
                {
                    txtContadorAtualVagas.Text = Convert.ToString((buf[4] << 8) + buf[5]);
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_operacaoPacoteJornada(byte[] buf)
        {
            UInt16 indiceLimite = ((controladoraLinear.SIZE_OF_JORNADAS * controladoraLinear.N_LINES_JORNADAS) / 512);
            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_JORNADAS, controladoraLinear.FILE_JORNADA, indiceLimite, controladoraLinear.DIV_OF_JORNADAS);
        }        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void trataRespostaCmd_lerSetup(byte[] buf)
        {
            try
            {
                memoriaPlaca.setup = pControl.vetorParaSetup(buf);
                leSetupForm();
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void trataRespostaCmd_modoRemoto(byte[] buf)
        {
            try
            {
                switch (cboxModoRemoto.SelectedIndex)
                {
                    case controladoraLinear.MODO_REMOTO_FIXO_90S: lblTimerRemoto.Text= Convert.ToString(90); break;
                    case controladoraLinear.MODO_REMOTO_STAND_BY: lblTimerRemoto.Text = "0"; break;
                    case controladoraLinear.MODO_REMOTO_CANCEL_STAND_BY: lblTimerRemoto.Text = "0"; break;
                    case controladoraLinear.MODO_REMOTO_TEMPO_CONFIGURAVEL: lblTimerRemoto.Text = txtTempoRemoto.Text; break;                    
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 86	Restore de dispositivo (PROGRESSIVO)
        // CMD: 00 + 56 + <framDisp(32 bytes) + <cs>
        // RSP: 00 + 56 + <retorno>  + cs       [4 bytes]
        public void trataRespostaCmd_gravaDispositivo_PROGRESSIVO(byte[] buf)
        {
            int i = 0;
            string stringDisp;
            byte[] frameDisp = new byte[controladoraLinear.SIZE_OF_DISPOSITIVO];

            if (buf[2] == controladoraLinear.RT_OK)
            {
                indiceProgressivo++;
                progressBar1.PerformStep();

                if( indiceProgressivo < nDispArquivoProgressivo ) 
                {

                    Invoke((MethodInvoker)delegate
                    {
                        lblQuantidadeDispositivos.Text = Convert.ToString(indiceProgressivo);
                    });


                    stringDisp = arquivoRestore.Substring(indiceProgressivo * 66, controladoraLinear.SIZE_OF_DISPOSITIVO * 2 + 2); // copia uma linha do arquivo (em ASCII(
                    StringBuilder s = new StringBuilder(64);

                    for (i = 0; i < 64; i++) s.Append(stringDisp[i]);
                    frameDisp = HexStringToBytes(s.ToString());    // converte para HEXADECIMAL
                    if (((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_TX) ||
                        ((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_TA) ||
                        ((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_CT) ||
                        ((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_BM) ||
                        ((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_TP) ||
                        ((frameDisp[0] & 0xF0) == controladoraLinear.S_DISP_SN))
                    {
                        for (i = 0; i < controladoraLinear.SIZE_OF_DISPOSITIVO; i++) memoriaPlaca.dispositivos[indiceProgressivo, i] = frameDisp[i]; // atualiza a base de dados
                        byte[] buf2 = pControl.gravaDispositivo_PROGRESSIVO(frameDisp);
                        enviarMensagem(buf2);
                    }
                    else
                    {
                        progressBar1.Visible = false;

                        mostraFimCronometro();
                    }
                }
                else
                {

                    progressBar1.Visible = false;

                    mostraFimCronometro();
                }
            }
            else
            {

                progressBar1.Visible = false;

                MessageBox.Show("Erro: gravar Progressivo");

                // fim 
                mostraFimCronometro();
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 87	Ler quantidade de dispositivos
        // 00 + 57 + <dispositivo> + cs
        // 00 + 57 + <retorno> + <dispositivo> + <quantidade H> + <quantidade L> + cs
        public void trataRespostaCmd_lerQuantidadeDispositivos(byte[] buf)
        {
            lblQuantidadeDispositivos.Text = Convert.ToString( (buf[4]<<8) + buf[5] );
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_operacaoPacoteTurnos(byte[] buf)
        {
            UInt16 indiceLimite = (controladoraLinear.N_LINES_TURNOS * controladoraLinear.SIZE_OF_TURNOS) / 512;
            trataOperacaoPacote(buf, controladoraLinear.SIZE_OF_TURNOS, controladoraLinear.FILE_TURNOS, indiceLimite, controladoraLinear.DIV_OF_TURNOS);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // 90 Ler quantidade de eventos
        // 00 + 5A + <marca> + cs
        // 00 + 5A + <retorno> + <marca> + <qtdEventosH> + <qtdEventosL> + cs
        public void trataRespostaCmd_lerQtdEventos(byte[] buf)
        {
            lblLerQtdEventos.Text = "Quantidade de eventos: " + Convert.ToString((buf[4] << 8) + buf[5]);
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 59 + <retorno> + <cód. do turno> + <frame de turno(16 bytes)> + cs       [21 bytes]
        public void trataRespostaCmd_lerTurnos(byte[] buf)
        {
            try
            {
                txtT1HI.Text = Convert.ToString(buf[4]);
                txtT1MI.Text = Convert.ToString(buf[5]);
                txtT1HF.Text = Convert.ToString(buf[6]);
                txtT1MF.Text = Convert.ToString(buf[7]);
                txtT2HI.Text = Convert.ToString(buf[8]);
                txtT2MI.Text = Convert.ToString(buf[9]);
                txtT2HF.Text = Convert.ToString(buf[10]);
                txtT2MF.Text = Convert.ToString(buf[11]);
                txtT3HI.Text = Convert.ToString(buf[12]);
                txtT3MI.Text = Convert.ToString(buf[13]);
                txtT3HF.Text = Convert.ToString(buf[14]);
                txtT3MF.Text = Convert.ToString(buf[15]);
                txtT4HI.Text = Convert.ToString(buf[16]);
                txtT4MI.Text = Convert.ToString(buf[17]);
                txtT4HF.Text = Convert.ToString(buf[18]);
                txtT4MF.Text = Convert.ToString(buf[19]);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 5C + <retorno> + <frame datas dos feriados(48 bytes)> + cs       [52 bytes]
        private void trataRespostaCmd_lerDatasFeriados(byte[] buf)
        {
            try
            {
                int i = 3;
                txtDataF1D.Text = Convert.ToString(buf[i++]);
                txtDataF1M.Text = Convert.ToString(buf[i++]);
                txtDataF2D.Text = Convert.ToString(buf[i++]);
                txtDataF2M.Text = Convert.ToString(buf[i++]);
                txtDataF3D.Text = Convert.ToString(buf[i++]);
                txtDataF3M.Text = Convert.ToString(buf[i++]);
                txtDataF4D.Text = Convert.ToString(buf[i++]);
                txtDataF4M.Text = Convert.ToString(buf[i++]);
                txtDataF5D.Text = Convert.ToString(buf[i++]);
                txtDataF5M.Text = Convert.ToString(buf[i++]);
                txtDataF6D.Text = Convert.ToString(buf[i++]);
                txtDataF6M.Text = Convert.ToString(buf[i++]);
                txtDataF7D.Text = Convert.ToString(buf[i++]);
                txtDataF7M.Text = Convert.ToString(buf[i++]);
                txtDataF8D.Text = Convert.ToString(buf[i++]);
                txtDataF8M.Text = Convert.ToString(buf[i++]);
                txtDataF9D.Text = Convert.ToString(buf[i++]);
                txtDataF9M.Text = Convert.ToString(buf[i++]);
                txtDataF10D.Text = Convert.ToString(buf[i++]);
                txtDataF10M.Text = Convert.ToString(buf[i++]);
                txtDataF11D.Text = Convert.ToString(buf[i++]);
                txtDataF11M.Text = Convert.ToString(buf[i++]);
                txtDataF12D.Text = Convert.ToString(buf[i++]);
                txtDataF12M.Text = Convert.ToString(buf[i++]);
                txtDataF13D.Text = Convert.ToString(buf[i++]);
                txtDataF13M.Text = Convert.ToString(buf[i++]);
                txtDataF14D.Text = Convert.ToString(buf[i++]);
                txtDataF14M.Text = Convert.ToString(buf[i++]);
                txtDataF15D.Text = Convert.ToString(buf[i++]);
                txtDataF15M.Text = Convert.ToString(buf[i++]);
                txtDataF16D.Text = Convert.ToString(buf[i++]);
                txtDataF16M.Text = Convert.ToString(buf[i++]);
                txtDataF17D.Text = Convert.ToString(buf[i++]);
                txtDataF17M.Text = Convert.ToString(buf[i++]);
                txtDataF18D.Text = Convert.ToString(buf[i++]);
                txtDataF18M.Text = Convert.ToString(buf[i++]);
                txtDataF19D.Text = Convert.ToString(buf[i++]);
                txtDataF19M.Text = Convert.ToString(buf[i++]);
                txtDataF20D.Text = Convert.ToString(buf[i++]);
                txtDataF20M.Text = Convert.ToString(buf[i++]);
                txtDataF21D.Text = Convert.ToString(buf[i++]);
                txtDataF21M.Text = Convert.ToString(buf[i++]);
                txtDataF22D.Text = Convert.ToString(buf[i++]);
                txtDataF22M.Text = Convert.ToString(buf[i++]);
                txtDataF23D.Text = Convert.ToString(buf[i++]);
                txtDataF23M.Text = Convert.ToString(buf[i++]);
                txtDataF24D.Text = Convert.ToString(buf[i++]);
                txtDataF24M.Text = Convert.ToString(buf[i++]);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 5D + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +
        //     <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs       [37 bytes]
        private void trataRespostaCmd_lerDispositivoProgressivo(byte[] buf)
        {
            try
            {
                bool dispositivoInvalido = false;
                byte[] serialTemp = new byte[4];
                byte[] contadorTemp = new byte[2];
                byte tipo = buf[3];

                switch (tipo)
                {
                    case controladoraLinear.DISP_TX: cboxTipoDispositivo.SelectedIndex = 0; break;
                    case controladoraLinear.DISP_TA: cboxTipoDispositivo.SelectedIndex = 1; break;
                    case controladoraLinear.DISP_CT: cboxTipoDispositivo.SelectedIndex = 2; break;
                    case controladoraLinear.DISP_BM: cboxTipoDispositivo.SelectedIndex = 3; break;
                    case controladoraLinear.DISP_TP: cboxTipoDispositivo.SelectedIndex = 4; break;
                    case controladoraLinear.DISP_SN: cboxTipoDispositivo.SelectedIndex = 5; break;
                    default: dispositivoInvalido = true; break;
                }

                int i;

                contadorTemp[0] = 0;
                contadorTemp[1] = 0;
                if (tipo == controladoraLinear.DISP_TX)
                {
                    serialTemp[0] = buf[4 + 0];
                    serialTemp[1] = buf[4 + 1];
                    serialTemp[2] = buf[4 + 2];
                    serialTemp[3] = buf[4 + 3];
                    contadorTemp[0] = buf[8];
                    contadorTemp[1] = buf[9];
                }
                else
                {
                    if((tipo == controladoraLinear.DISP_SN)&&( memoriaPlaca.setup.cfg16.senha13digitos == 1 ))
                    {
                        serialTemp = new byte[6];
                        serialTemp[0] = buf[4 + 0];
                        serialTemp[1] = buf[4 + 1];
                        serialTemp[2] = buf[4 + 2];
                        serialTemp[3] = buf[4 + 3];
                        serialTemp[4] = buf[4 + 4];
                        serialTemp[5] = buf[4 + 5];
                    }
                    else
                    {
                        serialTemp[0] = buf[4 + 2];
                        serialTemp[1] = buf[4 + 3];
                        serialTemp[2] = buf[4 + 4];
                        serialTemp[3] = buf[4 + 5];
                    }
                }

                txtSerial.Text = BytesToHexString(serialTemp);
                try
                {
                    txtSerial.Text = txtSerial.Text.Substring(1, 11);
                }
                catch { };
                txtContadorHCS.Text = BytesToHexString(contadorTemp);

                UInt16 codigoRota = ((UInt16)((buf[10] << 8) + buf[11]));
                if (codigoRota < controladoraLinear.N_LINES_HABILITACAO)
                {
                    cboxRota.SelectedIndex = codigoRota;
                }
                else
                {
                    lblErro.Text = "Erro: " + "Código de habilitacao fora do limite (" + Convert.ToString(codigoRota) + ")";
                    codigoRota = 0;
                }

                // flags cadastro
                ckBoxSaidaCofre.Checked = false;
                if ((buf[12] & 0x01) > 0) ckBoxSaidaCofre.Checked = true;
                
                cboxAntipassback.SelectedIndex = (byte)((buf[12] >> 1) & 0x03);
                
                checkBoxDispositivos_visitante.Checked = false;
                if ((buf[12] & 0x08) > 0) checkBoxDispositivos_visitante.Checked = true;

                checkBoxDispositivos_controleVagasId.Checked = false;
                if ((buf[12] & 0x10) > 0) checkBoxDispositivos_controleVagasId.Checked = true;

                checkBoxDispositivos_duplaValidacao.Checked = false;
                if ((buf[12] & 0x40) > 0) checkBoxDispositivos_duplaValidacao.Checked = true;

                // flags status
                tratalabelsAcionamento(tipo, buf[13]);

                // nivel
                txtNivel.Text = Convert.ToString(buf[14]);
                // créditos
                txtCréditos.Text = Convert.ToString(buf[15]);
                // data validade
                if ((buf[16] > 31) || (buf[19] > 31) || (buf[16] == 0) || (buf[19] == 0) || // dia
                     (buf[17] > 12) || (buf[20] > 12) || (buf[17] == 0) || (buf[20] == 0) || // mês
                     (buf[18] > 99) || (buf[21] > 99)) // ano
                {
                    lblErro.Text = "Erro: " + "Data de válidade inválida";
                }
                else
                {
                    dateTimePicker1.Value = new DateTime(((int)(2000 + buf[18])), (int)(buf[17]), (int)(buf[16]));
                    dateTimePicker2.Value = new DateTime(((int)(2000 + buf[21])), (int)(buf[20]), (int)(buf[19]));
                }

                txtIdentificacao.Text = null;
                for (i = 0; i < 14; i++) txtIdentificacao.Text += (char)buf[22 + i];

                if (dispositivoInvalido == true)
                {
                    lblErro.Text = "Erro: " + "Fim dos dispositivos, vago ou inválido (" + Convert.ToString(tipo) + ")";                    
                    if (tipo == 0x0F)
                    {
                        MessageBox.Show("Fim dos dispositivos! ('Tipo de dispositivo = 0x0F)'");
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btAcionamento_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.acionamento((byte)comboBoxEntradaSaida_tipoSaida.SelectedIndex,
                                        (byte)comboBoxEntradaSaida_endereco.SelectedIndex,
                                        (byte)(comboBoxEntradaSaida_saida.SelectedIndex));

                enviarMensagem(buf);
            }
            catch { };
        }

        public void Acionamento(int Tipo, int Endereco)
        {
            try
            {
                byte[] buf = pControl.acionamento((byte)Tipo,
                                        (byte)0,
                                        (byte)Endereco);

                enviarMensagem(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btEscreverDataHora_Click(object sender, EventArgs e)
        {
            try
            {
                if (ckBoxDataHoraPc.Checked == true)
                {
                    byte[] buf = pControl.escreveDataHora( (byte)DateTime.Now.Hour,
                                                           (byte)DateTime.Now.Minute,
                                                           (byte)DateTime.Now.Second,
                                                           (byte)DateTime.Now.Day,
                                                           (byte)DateTime.Now.Month,
                                                           DateTime.Now.Year);
                    enviarMensagem(buf);
                }
                else
                {
                    byte[] buf = pControl.escreveDataHora( Convert.ToByte(txtDataHora_hora.Text),
                                                           Convert.ToByte(txtDataHora_minuto.Text),
                                                           Convert.ToByte(txtDataHora_segundo.Text),
                                                           Convert.ToByte(txtDataHora_dia.Text),
                                                           Convert.ToByte(txtDataHora_mes.Text),
                                                           Convert.ToByte(txtDataHora_ano.Text));
                    enviarMensagem(buf);
                }
            }
            catch
            {
                MessageBox.Show("Valor Inválido");
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btEditarHabilitacaoLeitoras_Click(object sender, EventArgs e)
        {
            editarHabilitacaoLeitoras( 79 );
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void editarHabilitacaoLeitoras( byte cmd )
        {
            try
            {
                int i,j;
                byte[] bufHabilitacao = new byte[32];

                for (j = 0; j < controladoraLinear.N_ENDERECOS; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        if ((j == 0) && (checkedListBox1.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[0] |= (byte)(1 << i);
                        if ((j == 1) && (checkedListBox2.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[0] |= (byte)(1 << (i + 4));
                        if ((j == 2) && (checkedListBox3.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[1] |= (byte)(1 << i);
                        if ((j == 3) && (checkedListBox4.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[1] |= (byte)(1 << (i + 4));
                        if ((j == 4) && (checkedListBox5.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[2] |= (byte)(1 << i);
                        if ((j == 5) && (checkedListBox6.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[2] |= (byte)(1 << (i + 4));
                        if ((j == 6) && (checkedListBox7.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[3] |= (byte)(1 << i);
                        if ((j == 7) && (checkedListBox8.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[3] |= (byte)(1 << (i + 4));
                        if ((j == 8) && (checkedListBox9.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[4] |= (byte)(1 << i);
                        if ((j == 9) && (checkedListBox10.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[4] |= (byte)(1 << (i + 4));
                        if ((j == 10) && (checkedListBox11.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[5] |= (byte)(1 << i);
                        if ((j == 11) && (checkedListBox12.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[5] |= (byte)(1 << (i + 4));
                        if ((j == 12) && (checkedListBox13.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[6] |= (byte)(1 << i);
                        if ((j == 13) && (checkedListBox14.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[6] |= (byte)(1 << (i + 4));
                        if ((j == 14) && (checkedListBox15.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[7] |= (byte)(1 << i);
                        if ((j == 15) && (checkedListBox16.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[7] |= (byte)(1 << (i + 4));
                        if ((j == 16) && (checkedListBox17.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[8] |= (byte)(1 << i);
                        if ((j == 17) && (checkedListBox18.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[8] |= (byte)(1 << (i + 4));
                        if ((j == 18) && (checkedListBox19.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[9] |= (byte)(1 << i);
                        if ((j == 19) && (checkedListBox20.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[9] |= (byte)(1 << (i + 4));
                        if ((j == 20) && (checkedListBox21.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[10] |= (byte)(1 << i);
                        if ((j == 21) && (checkedListBox22.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[10] |= (byte)(1 << (i + 4));
                        if ((j == 22) && (checkedListBox23.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[11] |= (byte)(1 << i);
                        if ((j == 23) && (checkedListBox24.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[11] |= (byte)(1 << (i + 4));
                        if ((j == 24) && (checkedListBox25.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[12] |= (byte)(1 << i);
                        if ((j == 25) && (checkedListBox26.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[12] |= (byte)(1 << (i + 4));
                        if ((j == 26) && (checkedListBox27.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[13] |= (byte)(1 << i);
                        if ((j == 27) && (checkedListBox28.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[13] |= (byte)(1 << (i + 4));
                        if ((j == 28) && (checkedListBox29.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[14] |= (byte)(1 << i);
                        if ((j == 29) && (checkedListBox30.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[14] |= (byte)(1 << (i + 4));
                        if ((j == 30) && (checkedListBox31.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[15] |= (byte)(1 << i);
                        if ((j == 31) && (checkedListBox32.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[15] |= (byte)(1 << (i + 4));
                        if ((j == 32) && (checkedListBox33.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[16] |= (byte)(1 << i);
                        if ((j == 33) && (checkedListBox34.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[16] |= (byte)(1 << (i + 4));
                        if ((j == 34) && (checkedListBox35.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[17] |= (byte)(1 << i);
                        if ((j == 35) && (checkedListBox36.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[17] |= (byte)(1 << (i + 4));
                        if ((j == 36) && (checkedListBox37.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[18] |= (byte)(1 << i);
                        if ((j == 37) && (checkedListBox38.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[18] |= (byte)(1 << (i + 4));
                        if ((j == 38) && (checkedListBox39.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[19] |= (byte)(1 << i);
                        if ((j == 39) && (checkedListBox40.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[19] |= (byte)(1 << (i + 4));
                        if ((j == 40) && (checkedListBox41.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[20] |= (byte)(1 << i);
                        if ((j == 41) && (checkedListBox42.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[20] |= (byte)(1 << (i + 4));
                        if ((j == 42) && (checkedListBox43.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[21] |= (byte)(1 << i);
                        if ((j == 43) && (checkedListBox44.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[21] |= (byte)(1 << (i + 4));
                        if ((j == 44) && (checkedListBox45.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[22] |= (byte)(1 << i);
                        if ((j == 45) && (checkedListBox46.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[22] |= (byte)(1 << (i + 4));
                        if ((j == 46) && (checkedListBox47.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[23] |= (byte)(1 << i);
                        if ((j == 47) && (checkedListBox48.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[23] |= (byte)(1 << (i + 4));
                        if ((j == 48) && (checkedListBox49.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[24] |= (byte)(1 << i);
                        if ((j == 49) && (checkedListBox50.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[24] |= (byte)(1 << (i + 4));
                        if ((j == 50) && (checkedListBox51.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[25] |= (byte)(1 << i);
                        if ((j == 51) && (checkedListBox52.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[25] |= (byte)(1 << (i + 4));
                        if ((j == 52) && (checkedListBox53.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[26] |= (byte)(1 << i);
                        if ((j == 53) && (checkedListBox54.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[26] |= (byte)(1 << (i + 4));
                        if ((j == 54) && (checkedListBox55.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[27] |= (byte)(1 << i);
                        if ((j == 55) && (checkedListBox56.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[27] |= (byte)(1 << (i + 4));
                        if ((j == 56) && (checkedListBox57.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[28] |= (byte)(1 << i);
                        if ((j == 57) && (checkedListBox58.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[28] |= (byte)(1 << (i + 4));
                        if ((j == 58) && (checkedListBox59.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[29] |= (byte)(1 << i);
                        if ((j == 59) && (checkedListBox60.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[29] |= (byte)(1 << (i + 4));
                        if ((j == 60) && (checkedListBox61.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[30] |= (byte)(1 << i);
                        if ((j == 61) && (checkedListBox62.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[30] |= (byte)(1 << (i + 4));
                        if ((j == 62) && (checkedListBox63.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[31] |= (byte)(1 << i);
                        if ((j == 63) && (checkedListBox64.GetItemCheckState(i) == CheckState.Checked)) bufHabilitacao[31] |= (byte)(1 << (i + 4));
                    }
                }

                for (i = 0; i < controladoraLinear.SIZE_OF_HABILITACAO; i++) memoriaPlaca.rotas[cbox_codigoRota.SelectedIndex, i] = bufHabilitacao[i];

                byte[] buf;
                if( cmd == 79 )
                {
                    buf = pControl.editarHabilitacaoLeitoras(cbox_codigoRota.SelectedIndex, bufHabilitacao);
                }
                else
                {
                    buf = pControl.editarHabilitacaoLeitorasComSetor(cbox_codigoRota.SelectedIndex, bufHabilitacao, (byte)comboBox_SETUP_setor.SelectedIndex);
                }
                enviarMensagem(buf);
                
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btHabilitarTodos_Click(object sender, EventArgs e)
        {
            try
            {
                int i, j;
                for (j = 0; j < controladoraLinear.N_ENDERECOS; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        checkedListBox1.SetItemChecked(i, true);
                        checkedListBox2.SetItemChecked(i, true);
                        checkedListBox3.SetItemChecked(i, true);
                        checkedListBox4.SetItemChecked(i, true);
                        checkedListBox5.SetItemChecked(i, true);
                        checkedListBox6.SetItemChecked(i, true);
                        checkedListBox7.SetItemChecked(i, true);
                        checkedListBox8.SetItemChecked(i, true);
                        checkedListBox9.SetItemChecked(i, true);
                        checkedListBox10.SetItemChecked(i, true);
                        checkedListBox11.SetItemChecked(i, true);
                        checkedListBox12.SetItemChecked(i, true);
                        checkedListBox13.SetItemChecked(i, true);
                        checkedListBox14.SetItemChecked(i, true);
                        checkedListBox15.SetItemChecked(i, true);
                        checkedListBox16.SetItemChecked(i, true);
                        checkedListBox17.SetItemChecked(i, true);
                        checkedListBox18.SetItemChecked(i, true);
                        checkedListBox19.SetItemChecked(i, true);
                        checkedListBox20.SetItemChecked(i, true);
                        checkedListBox21.SetItemChecked(i, true);
                        checkedListBox22.SetItemChecked(i, true);
                        checkedListBox23.SetItemChecked(i, true);
                        checkedListBox24.SetItemChecked(i, true);
                        checkedListBox25.SetItemChecked(i, true);
                        checkedListBox26.SetItemChecked(i, true);
                        checkedListBox27.SetItemChecked(i, true);
                        checkedListBox28.SetItemChecked(i, true);
                        checkedListBox29.SetItemChecked(i, true);
                        checkedListBox30.SetItemChecked(i, true);
                        checkedListBox31.SetItemChecked(i, true);
                        checkedListBox32.SetItemChecked(i, true);
                        checkedListBox33.SetItemChecked(i, true);
                        checkedListBox34.SetItemChecked(i, true);
                        checkedListBox35.SetItemChecked(i, true);
                        checkedListBox36.SetItemChecked(i, true);
                        checkedListBox37.SetItemChecked(i, true);
                        checkedListBox38.SetItemChecked(i, true);
                        checkedListBox39.SetItemChecked(i, true);
                        checkedListBox40.SetItemChecked(i, true);
                        checkedListBox41.SetItemChecked(i, true);
                        checkedListBox42.SetItemChecked(i, true);
                        checkedListBox43.SetItemChecked(i, true);
                        checkedListBox44.SetItemChecked(i, true);
                        checkedListBox45.SetItemChecked(i, true);
                        checkedListBox46.SetItemChecked(i, true);
                        checkedListBox47.SetItemChecked(i, true);
                        checkedListBox48.SetItemChecked(i, true);
                        checkedListBox49.SetItemChecked(i, true);
                        checkedListBox50.SetItemChecked(i, true);
                        checkedListBox51.SetItemChecked(i, true);
                        checkedListBox52.SetItemChecked(i, true);
                        checkedListBox53.SetItemChecked(i, true);
                        checkedListBox54.SetItemChecked(i, true);
                        checkedListBox55.SetItemChecked(i, true);
                        checkedListBox56.SetItemChecked(i, true);
                        checkedListBox57.SetItemChecked(i, true);
                        checkedListBox58.SetItemChecked(i, true);
                        checkedListBox59.SetItemChecked(i, true);
                        checkedListBox60.SetItemChecked(i, true);
                        checkedListBox61.SetItemChecked(i, true);
                        checkedListBox62.SetItemChecked(i, true);
                        checkedListBox63.SetItemChecked(i, true);
                        checkedListBox64.SetItemChecked(i, true);
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btDesabilitarTodos_Click(object sender, EventArgs e)
        {
            try
            {
                desabilitarTodasLeitoras();
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void desabilitarTodasLeitoras()
        {
            try
            {
                int i, j;
                for (j = 0; j < controladoraLinear.N_ENDERECOS; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                        checkedListBox2.SetItemChecked(i, false);
                        checkedListBox3.SetItemChecked(i, false);
                        checkedListBox4.SetItemChecked(i, false);
                        checkedListBox5.SetItemChecked(i, false);
                        checkedListBox6.SetItemChecked(i, false);
                        checkedListBox7.SetItemChecked(i, false);
                        checkedListBox8.SetItemChecked(i, false);
                        checkedListBox9.SetItemChecked(i, false);
                        checkedListBox10.SetItemChecked(i, false);
                        checkedListBox11.SetItemChecked(i, false);
                        checkedListBox12.SetItemChecked(i, false);
                        checkedListBox13.SetItemChecked(i, false);
                        checkedListBox14.SetItemChecked(i, false);
                        checkedListBox15.SetItemChecked(i, false);
                        checkedListBox16.SetItemChecked(i, false);
                        checkedListBox17.SetItemChecked(i, false);
                        checkedListBox18.SetItemChecked(i, false);
                        checkedListBox19.SetItemChecked(i, false);
                        checkedListBox20.SetItemChecked(i, false);
                        checkedListBox21.SetItemChecked(i, false);
                        checkedListBox22.SetItemChecked(i, false);
                        checkedListBox23.SetItemChecked(i, false);
                        checkedListBox24.SetItemChecked(i, false);
                        checkedListBox25.SetItemChecked(i, false);
                        checkedListBox26.SetItemChecked(i, false);
                        checkedListBox27.SetItemChecked(i, false);
                        checkedListBox28.SetItemChecked(i, false);
                        checkedListBox29.SetItemChecked(i, false);
                        checkedListBox30.SetItemChecked(i, false);
                        checkedListBox31.SetItemChecked(i, false);
                        checkedListBox32.SetItemChecked(i, false);
                        checkedListBox33.SetItemChecked(i, false);
                        checkedListBox34.SetItemChecked(i, false);
                        checkedListBox35.SetItemChecked(i, false);
                        checkedListBox36.SetItemChecked(i, false);
                        checkedListBox37.SetItemChecked(i, false);
                        checkedListBox38.SetItemChecked(i, false);
                        checkedListBox39.SetItemChecked(i, false);
                        checkedListBox40.SetItemChecked(i, false);
                        checkedListBox41.SetItemChecked(i, false);
                        checkedListBox42.SetItemChecked(i, false);
                        checkedListBox43.SetItemChecked(i, false);
                        checkedListBox44.SetItemChecked(i, false);
                        checkedListBox45.SetItemChecked(i, false);
                        checkedListBox46.SetItemChecked(i, false);
                        checkedListBox47.SetItemChecked(i, false);
                        checkedListBox48.SetItemChecked(i, false);
                        checkedListBox49.SetItemChecked(i, false);
                        checkedListBox50.SetItemChecked(i, false);
                        checkedListBox51.SetItemChecked(i, false);
                        checkedListBox52.SetItemChecked(i, false);
                        checkedListBox53.SetItemChecked(i, false);
                        checkedListBox54.SetItemChecked(i, false);
                        checkedListBox55.SetItemChecked(i, false);
                        checkedListBox56.SetItemChecked(i, false);
                        checkedListBox57.SetItemChecked(i, false);
                        checkedListBox58.SetItemChecked(i, false);
                        checkedListBox59.SetItemChecked(i, false);
                        checkedListBox60.SetItemChecked(i, false);
                        checkedListBox61.SetItemChecked(i, false);
                        checkedListBox62.SetItemChecked(i, false);
                        checkedListBox63.SetItemChecked(i, false);
                        checkedListBox64.SetItemChecked(i, false);
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btCopiarPadrao_Click(object sender, EventArgs e)
        {
            try
            {
                int i, j;
                bool checkboxstate = false;

                for (j = 0; j < controladoraLinear.N_ENDERECOS; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        checkboxstate = false;
                        if (ckBoxPadrao.GetItemCheckState(i) == CheckState.Checked) checkboxstate = true;

                        checkedListBox1.SetItemChecked(i, checkboxstate);
                        checkedListBox2.SetItemChecked(i, checkboxstate);
                        checkedListBox3.SetItemChecked(i, checkboxstate);
                        checkedListBox4.SetItemChecked(i, checkboxstate);
                        checkedListBox5.SetItemChecked(i, checkboxstate);
                        checkedListBox6.SetItemChecked(i, checkboxstate);
                        checkedListBox7.SetItemChecked(i, checkboxstate);
                        checkedListBox8.SetItemChecked(i, checkboxstate);
                        checkedListBox9.SetItemChecked(i, checkboxstate);
                        checkedListBox10.SetItemChecked(i, checkboxstate);
                        checkedListBox11.SetItemChecked(i, checkboxstate);
                        checkedListBox12.SetItemChecked(i, checkboxstate);
                        checkedListBox13.SetItemChecked(i, checkboxstate);
                        checkedListBox14.SetItemChecked(i, checkboxstate);
                        checkedListBox15.SetItemChecked(i, checkboxstate);
                        checkedListBox16.SetItemChecked(i, checkboxstate);
                        checkedListBox17.SetItemChecked(i, checkboxstate);
                        checkedListBox18.SetItemChecked(i, checkboxstate);
                        checkedListBox19.SetItemChecked(i, checkboxstate);
                        checkedListBox20.SetItemChecked(i, checkboxstate);
                        checkedListBox21.SetItemChecked(i, checkboxstate);
                        checkedListBox22.SetItemChecked(i, checkboxstate);
                        checkedListBox23.SetItemChecked(i, checkboxstate);
                        checkedListBox24.SetItemChecked(i, checkboxstate);
                        checkedListBox25.SetItemChecked(i, checkboxstate);
                        checkedListBox26.SetItemChecked(i, checkboxstate);
                        checkedListBox27.SetItemChecked(i, checkboxstate);
                        checkedListBox28.SetItemChecked(i, checkboxstate);
                        checkedListBox29.SetItemChecked(i, checkboxstate);
                        checkedListBox30.SetItemChecked(i, checkboxstate);
                        checkedListBox31.SetItemChecked(i, checkboxstate);
                        checkedListBox32.SetItemChecked(i, checkboxstate);
                        checkedListBox33.SetItemChecked(i, checkboxstate);
                        checkedListBox34.SetItemChecked(i, checkboxstate);
                        checkedListBox35.SetItemChecked(i, checkboxstate);
                        checkedListBox36.SetItemChecked(i, checkboxstate);
                        checkedListBox37.SetItemChecked(i, checkboxstate);
                        checkedListBox38.SetItemChecked(i, checkboxstate);
                        checkedListBox39.SetItemChecked(i, checkboxstate);
                        checkedListBox40.SetItemChecked(i, checkboxstate);
                        checkedListBox41.SetItemChecked(i, checkboxstate);
                        checkedListBox42.SetItemChecked(i, checkboxstate);
                        checkedListBox43.SetItemChecked(i, checkboxstate);
                        checkedListBox44.SetItemChecked(i, checkboxstate);
                        checkedListBox45.SetItemChecked(i, checkboxstate);
                        checkedListBox46.SetItemChecked(i, checkboxstate);
                        checkedListBox47.SetItemChecked(i, checkboxstate);
                        checkedListBox48.SetItemChecked(i, checkboxstate);
                        checkedListBox49.SetItemChecked(i, checkboxstate);
                        checkedListBox50.SetItemChecked(i, checkboxstate);
                        checkedListBox51.SetItemChecked(i, checkboxstate);
                        checkedListBox52.SetItemChecked(i, checkboxstate);
                        checkedListBox53.SetItemChecked(i, checkboxstate);
                        checkedListBox54.SetItemChecked(i, checkboxstate);
                        checkedListBox55.SetItemChecked(i, checkboxstate);
                        checkedListBox56.SetItemChecked(i, checkboxstate);
                        checkedListBox57.SetItemChecked(i, checkboxstate);
                        checkedListBox58.SetItemChecked(i, checkboxstate);
                        checkedListBox59.SetItemChecked(i, checkboxstate);
                        checkedListBox60.SetItemChecked(i, checkboxstate);
                        checkedListBox61.SetItemChecked(i, checkboxstate);
                        checkedListBox62.SetItemChecked(i, checkboxstate);
                        checkedListBox63.SetItemChecked(i, checkboxstate);
                        checkedListBox64.SetItemChecked(i, checkboxstate);
                    }
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btLerHabilitacaoLeitoras_Click(object sender, EventArgs e)
        {
            try
            {
                desabilitarTodasLeitoras();
                byte[] buf = pControl.lerHabilitacaoLeitoras( cbox_codigoRota.SelectedIndex );
                enviarMensagem(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btGravarDispositivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = (byte)cboxTipoDispositivo.SelectedIndex;
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
                UInt16 contadorHCS = UInt16.Parse(txtContadorHCS.Text, System.Globalization.NumberStyles.HexNumber);
                int codigoHabilitacao = cboxRota.SelectedIndex; // rota
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte saidaCofre = 0;
                if (ckBoxSaidaCofre.Checked) saidaCofre = 1;
                byte visitante = 0;
                if (checkBoxDispositivos_visitante.Checked) visitante = 1;
                byte controleVagasId = 0;
                if( checkBoxDispositivos_controleVagasId.Checked ) controleVagasId = 1;
                byte duplaValidacao = 0;
                if (checkBoxDispositivos_duplaValidacao.Checked) duplaValidacao = 1;
                byte panico = 0;
                if (checkBoxDispositivos_panico.Checked) panico = 1;

                DateTime dataIni = dateTimePicker1.Value;
                DateTime dataFim = dateTimePicker2.Value;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte creditos = Convert.ToByte(txtCréditos.Text);
                StringBuilder labelUsuario = new StringBuilder(14);

                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(saidaCofre, antipassback, visitante, controleVagasId, duplaValidacao, panico );
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 1); // bateria = ok(0), última saída acionada = 1
                controladoraLinear.s_validade validade = new controladoraLinear.s_validade((byte)dataIni.Day, (byte)dataIni.Month, dataIni.Year, (byte)dataFim.Day, (byte)dataFim.Month, dataFim.Year );

                for (int i = 0; i < 14; i++)
                {
                    if (i < txtIdentificacao.TextLength)
                    {
                        labelUsuario.Append(txtIdentificacao.Text[i]);
                    }
                    else
                    {
                        labelUsuario.Append(' ');
                    }
                }

            
                switch( tipo )
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                }

                byte[] buf = pControl.gravarDispositivo(tipo, serial, contadorHCS, codigoHabilitacao, fCadastro, fStatus, nivel, creditos, validade, labelUsuario.ToString() );

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btLerDispositivoProgressivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerDispositivo_PROGRESSIVO();
                enviarMensagem(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btLerDispositivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = (byte)cboxTipoDispositivo.SelectedIndex;
                switch (tipo)
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                    default: MessageBox.Show("Valor inválido!!");  return;
                }
                
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);

                byte[] buf = pControl.lerDispositivo(tipo,serial);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btEditarDispositivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = (byte)cboxTipoDispositivo.SelectedIndex;
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);

                UInt16 contadorHCS = UInt16.Parse( txtContadorHCS.Text, System.Globalization.NumberStyles.AllowHexSpecifier);
                int codigoHabilitacao = cboxRota.SelectedIndex; // rota
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte saidaCofre = 0;
                if (ckBoxSaidaCofre.Checked == true) saidaCofre = 1;
                byte visitante = 0;
                if (checkBoxDispositivos_visitante.Checked == true) visitante = 1;
                byte controleVagasId = 0;
                if (checkBoxDispositivos_controleVagasId.Checked) controleVagasId = 1;
                byte duplaValidacao = 0;
                if (checkBoxDispositivos_duplaValidacao.Checked) duplaValidacao = 1;
                byte panico = 0;
                if (checkBoxDispositivos_panico.Checked) panico = 1;

                
                DateTime dataIni = dateTimePicker1.Value;
                DateTime dataFim = dateTimePicker2.Value;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte creditos = Convert.ToByte(txtCréditos.Text);
                StringBuilder labelUsuario = new StringBuilder(14);


                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(saidaCofre, antipassback, visitante, controleVagasId, duplaValidacao, panico );
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 1); // bateria = ok(0), última saída acionada = 1
                controladoraLinear.s_validade validade = new controladoraLinear.s_validade((byte)dataIni.Day, (byte)dataIni.Month, dataIni.Year, (byte)dataFim.Day, (byte)dataFim.Month, dataFim.Year);

                for (int i = 0; i < 14; i++)
                {
                    if (i < txtIdentificacao.TextLength)
                    {
                        labelUsuario.Append(txtIdentificacao.Text[i]);
                    }
                    else
                    {
                        labelUsuario.Append(' ');
                    }
                }


                switch (tipo)
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                }

                byte[] buf = pControl.editarDispositivo(tipo, serial, contadorHCS, codigoHabilitacao, fCadastro, fStatus, nivel, creditos, validade, labelUsuario.ToString() );

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btApagarDispositivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = convertIndiceParaTipoDispositivo((byte)cboxTipoDispositivo.SelectedIndex);
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);

                byte[] buf = pControl.apagarDispositivo(tipo, serial);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public byte convertIndiceParaTipoDispositivo(int indice)
        {
            /*
            Controle - TX (1)     0
            Tag Ativo -TA (2)     1
            Cartão - CT (3)       2
            Biometria - BM (5)    3
            Tag Passivo - TP (6)  4
            Senha - SN (7)        5
            */
            switch (indice)
            {
                default: return (byte)(indice + 1);
                case 3:
                case 4:
                case 5:  return (byte)(indice + 2);
            }
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btFormatarMemoria_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.formataMemoria();
                enviarMensagem(buf);
            }
            catch { };
        }
        
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btContadorAtualizacao_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.contadorAtualizacao(Convert.ToByte(txtContadorAtualizacao.Text));

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void lblArquivoRestore_Click(object sender, EventArgs e)
        {

        }


        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void tabPage_Selected(object sender, TabControlEventArgs e)
        {
            try
            {
                arquivoRestore = null;
                label_txtArquivo.Text = null;
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public int nDispArquivoProgressivo = 0;
        private void btGravarDispositivo_PROGRESSIVO_Click(object sender, EventArgs e)
        {
            try
            {
                int i;
                string stringDisp;
                byte[] frameDisp = new byte[controladoraLinear.SIZE_OF_DISPOSITIVO];
                stringDisp = arquivoRestore.Substring(0, controladoraLinear.SIZE_OF_DISPOSITIVO * 2 + 2); // copia uma linha do arquivo (em ASCII )
                StringBuilder s = new StringBuilder(64);
                for (i = 0; i < 64; i++) s.Append(stringDisp[i]);
                frameDisp = HexStringToBytes(s.ToString());    // converte para HEXADECIMAL
                indiceProgressivo = 0;
                for (i = 0; i < controladoraLinear.SIZE_OF_DISPOSITIVO; i++)
                {
                    memoriaPlaca.dispositivos[0, i] = frameDisp[i]; // atualiza a base de dados
                }

                nDispArquivoProgressivo = Convert.ToUInt16(label_txtArquivo.Text.Substring(label_txtArquivo.Text.Length - 5, 5)); 

                byte[] buf = pControl.gravaDispositivo_PROGRESSIVO(frameDisp);


                progressBar1.Visible = true;
                progressBar1.Value = 0;
                progressBar1.Maximum = nDispArquivoProgressivo;
                progressBar1.Minimum = 0;
                progressBar1.Step = 1;

                if (cronometrometrando == false)
                {
                    cronometrometrando = true;
                    tempoInicio = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                }

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor ou arquivo inválido!!"); }
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public byte[] HexStringToBytes(string str)
        {
            const string hexDigits = "0123456789ABCDEF";
            // Determine how many bytes there are.     
            byte[] bytes = new byte[str.Length >> 1];
            for (int i = 0; i < str.Length; i += 2)
            {
                try
                {
                    int highDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i]));
                    int lowDigit = hexDigits.IndexOf(Char.ToUpperInvariant(str[i + 1]));
                    if (highDigit == -1 || lowDigit == -1)
                    {
                        throw new ArgumentException("The string contains an invalid digit.", "s");
                    }
                    bytes[i >> 1] = (byte)((highDigit << 4) | lowDigit);
                }
                catch { MessageBox.Show("Valor inválido!!"); }
            }
            return bytes;
        }


        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public static string BytesToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                try
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                catch { MessageBox.Show("Valor inválido!!"); }
            }
            return sb.ToString();
        }

        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public static string ByteUnicoToHexString(byte bytes)
        {
            StringBuilder sb = new StringBuilder(2);
            try
            {
                sb.AppendFormat("{0:X2}", bytes);
            }
            catch { MessageBox.Show("Valor inválido!!"); }

            return sb.ToString();
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void cboxIPHost_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboxIPHost.SelectedIndex == 0)
                {
                    txtBox_IP1.Text = "127.0.0.1";
                    //txtBox_IP1.Text = "100.0.0.194";
                }
                else
                {
                    txtBox_IP1.Text = "CONTROLADORA";
                }
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerQuantidadeDispositivos_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = Convert.ToByte(cboxTipo2.SelectedIndex);
                switch (tipo)
                {
                    case 0: break; // todos
                    case 1: tipo = controladoraLinear.DISP_TX; break;
                    case 2: tipo = controladoraLinear.DISP_TA; break;
                    case 3: tipo = controladoraLinear.DISP_CT; break;
                    case 4: tipo = controladoraLinear.DISP_BM; break;
                    case 5: tipo = controladoraLinear.DISP_TP; break;
                    case 6: tipo = controladoraLinear.DISP_SN; break;
                }

                byte[] buf = pControl.lerQuantidadeDispositivos( tipo );

                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public bool cmd_85_naoUtilizarEmNovasVersoes = false;
        private void btEditarTurnos_Click(object sender, EventArgs e)
        {
            if (cmd_85_naoUtilizarEmNovasVersoes == false)
            {
                cmd_85_naoUtilizarEmNovasVersoes = true;
                MessageBox.Show("Para novas versões utilizar comando 157");
            }

            try
            {
                byte[] turnos = new byte[16];
                turnos[0] = (byte)int.Parse(txtT1HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[1] = (byte)int.Parse(txtT1MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[2] = (byte)int.Parse(txtT1HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[3] = (byte)int.Parse(txtT1MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[4] = (byte)int.Parse(txtT2HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[5] = (byte)int.Parse(txtT2MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[6] = (byte)int.Parse(txtT2HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[7] = (byte)int.Parse(txtT2MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[8] = (byte)int.Parse(txtT3HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[9] = (byte)int.Parse(txtT3MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[10] = (byte)int.Parse(txtT3HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[11] = (byte)int.Parse(txtT3MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[12] = (byte)int.Parse(txtT4HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[13] = (byte)int.Parse(txtT4MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[14] = (byte)int.Parse(txtT4HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[15] = (byte)int.Parse(txtT4MF.Text, System.Globalization.NumberStyles.Integer);

                byte[] buf = pControl.editarTurnos(cboxCodigoTurno.SelectedIndex, turnos);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }


        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public bool cmd_89_naoUtilizarEmNovasVersoes = false;
        private void btLerTurnos_Click(object sender, EventArgs e)
        {
            if (cmd_89_naoUtilizarEmNovasVersoes == false)
            {
                cmd_89_naoUtilizarEmNovasVersoes = true;
                MessageBox.Show("Para novas versões utilizar comando 158");
            }

            try
            {
                byte[] buf = pControl.lerTurnos( cboxCodigoTurno.SelectedIndex );
                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btEditarDatasFeriados_Click(object sender, EventArgs e)
        {
            try
            {
                byte i = 0;
                byte[] feriados = new byte[48];
                feriados[i++] = (byte)int.Parse(txtDataF1D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF1M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF2D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF2M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF3D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF3M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF4D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF4M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF5D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF5M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF6D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF6M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF7D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF7M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF8D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF8M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF9D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF9M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF10D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF10M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF11D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF11M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF12D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF12M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF13D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF13M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF14D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF14M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF15D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF15M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF16D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF16M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF17D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF17M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF18D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF18M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF19D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF19M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF20D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF20M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF21D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF21M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF22D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF22M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF23D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF23M.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF24D.Text, System.Globalization.NumberStyles.Integer);
                feriados[i++] = (byte)int.Parse(txtDataF24M.Text, System.Globalization.NumberStyles.Integer);

                byte[] buf = pControl.editarDatasFeriados(feriados);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }


        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerDatasFeriados_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerDatasFeriados();
                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerQuantidadeEventos_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerQuantidadeEventos(Convert.ToByte( cboxMarcaEvento.SelectedIndex ));
                enviarMensagem(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerGravarPacoteTurnos_Click(object sender, EventArgs e)
        {
            try
            {
                int nPacotes;
                UInt16 indice;
                byte[] pacote = new byte[512];
                byte operacao = (byte)cboxOperacaoPacote.SelectedIndex;

                operacao = controladoraLinear.OP_ESCRITA;
                if (cboxOperacaoPacote.SelectedIndex == 0) operacao = controladoraLinear.OP_LEITURA;
            
                nPacotes = (int)Math.Round(Convert.ToDouble((double)((controladoraLinear.SIZE_OF_TURNOS * controladoraLinear.N_LINES_TURNOS) / 512)), 1);
                        
                int i,j;
                for (indice = 0; indice < nPacotes; indice++)
                {
                    i = 0;
                    while( ( i * controladoraLinear.SIZE_OF_TURNOS ) < 512 )
                    {
                        for( j = 0; j < controladoraLinear.SIZE_OF_TURNOS; j++ )
                        {
                            pacote[i * controladoraLinear.SIZE_OF_TURNOS + j] = memoriaPlaca.turnos[i + indice * 32, j];
                        }
                        i++;
                    }

                    byte[] buf = pControl.operacaoPacoteTurnos(operacao, indice, pacote);

                    enviarMensagem(buf);
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        // 00 + 6E + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação(32 bytes)> + cs
        public void trataRespostaCmd_lerHabilitacaoLeitoras(byte[] buf)
        {
            try
            {
                byte indice = 5;
                int i, j, val1, val2;

                for (j = 0; j < controladoraLinear.N_ENDERECOS; j++)
                {
                    for (i = 0; i < 4; i++)
                    {
                        val1 = (0x01 << i);
                        val2 = (0x10 << i);
                        if ((j == 0) && ((buf[indice + 0] & val1) > 0)) checkedListBox1.SetItemChecked(i, true);
                        if ((j == 1) && ((buf[indice + 0] & val2) > 0)) checkedListBox2.SetItemChecked(i, true);
                        if ((j == 2) && ((buf[indice + 1] & val1) > 0)) checkedListBox3.SetItemChecked(i, true);
                        if ((j == 3) && ((buf[indice + 1] & val2) > 0)) checkedListBox4.SetItemChecked(i, true);
                        if ((j == 4) && ((buf[indice + 2] & val1) > 0)) checkedListBox5.SetItemChecked(i, true);
                        if ((j == 5) && ((buf[indice + 2] & val2) > 0)) checkedListBox6.SetItemChecked(i, true);
                        if ((j == 6) && ((buf[indice + 3] & val1) > 0)) checkedListBox7.SetItemChecked(i, true);
                        if ((j == 7) && ((buf[indice + 3] & val2) > 0)) checkedListBox8.SetItemChecked(i, true);
                        if ((j == 8) && ((buf[indice + 4] & val1) > 0)) checkedListBox9.SetItemChecked(i, true);
                        if ((j == 9) && ((buf[indice + 4] & val2) > 0)) checkedListBox10.SetItemChecked(i, true);
                        if ((j == 10) && ((buf[indice + 5] & val1) > 0)) checkedListBox11.SetItemChecked(i, true);
                        if ((j == 11) && ((buf[indice + 5] & val2) > 0)) checkedListBox12.SetItemChecked(i, true);
                        if ((j == 12) && ((buf[indice + 6] & val1) > 0)) checkedListBox13.SetItemChecked(i, true);
                        if ((j == 13) && ((buf[indice + 6] & val2) > 0)) checkedListBox14.SetItemChecked(i, true);
                        if ((j == 14) && ((buf[indice + 7] & val1) > 0)) checkedListBox15.SetItemChecked(i, true);
                        if ((j == 15) && ((buf[indice + 7] & val2) > 0)) checkedListBox16.SetItemChecked(i, true);
                        if ((j == 16) && ((buf[indice + 8] & val1) > 0)) checkedListBox17.SetItemChecked(i, true);
                        if ((j == 17) && ((buf[indice + 8] & val2) > 0)) checkedListBox18.SetItemChecked(i, true);
                        if ((j == 18) && ((buf[indice + 9] & val1) > 0)) checkedListBox19.SetItemChecked(i, true);
                        if ((j == 19) && ((buf[indice + 9] & val2) > 0)) checkedListBox20.SetItemChecked(i, true);
                        if ((j == 20) && ((buf[indice + 10] & val1) > 0)) checkedListBox21.SetItemChecked(i, true);
                        if ((j == 21) && ((buf[indice + 10] & val2) > 0)) checkedListBox22.SetItemChecked(i, true);
                        if ((j == 22) && ((buf[indice + 11] & val1) > 0)) checkedListBox23.SetItemChecked(i, true);
                        if ((j == 23) && ((buf[indice + 11] & val2) > 0)) checkedListBox24.SetItemChecked(i, true);
                        if ((j == 24) && ((buf[indice + 12] & val1) > 0)) checkedListBox25.SetItemChecked(i, true);
                        if ((j == 25) && ((buf[indice + 12] & val2) > 0)) checkedListBox26.SetItemChecked(i, true);
                        if ((j == 26) && ((buf[indice + 13] & val1) > 0)) checkedListBox27.SetItemChecked(i, true);
                        if ((j == 27) && ((buf[indice + 13] & val2) > 0)) checkedListBox28.SetItemChecked(i, true);
                        if ((j == 28) && ((buf[indice + 14] & val1) > 0)) checkedListBox29.SetItemChecked(i, true);
                        if ((j == 29) && ((buf[indice + 14] & val2) > 0)) checkedListBox30.SetItemChecked(i, true);
                        if ((j == 30) && ((buf[indice + 15] & val1) > 0)) checkedListBox31.SetItemChecked(i, true);
                        if ((j == 31) && ((buf[indice + 15] & val2) > 0)) checkedListBox32.SetItemChecked(i, true);
                        if ((j == 32) && ((buf[indice + 16] & val1) > 0)) checkedListBox33.SetItemChecked(i, true);
                        if ((j == 33) && ((buf[indice + 16] & val2) > 0)) checkedListBox34.SetItemChecked(i, true);
                        if ((j == 34) && ((buf[indice + 17] & val1) > 0)) checkedListBox35.SetItemChecked(i, true);
                        if ((j == 35) && ((buf[indice + 17] & val2) > 0)) checkedListBox36.SetItemChecked(i, true);
                        if ((j == 36) && ((buf[indice + 18] & val1) > 0)) checkedListBox37.SetItemChecked(i, true);
                        if ((j == 37) && ((buf[indice + 18] & val2) > 0)) checkedListBox38.SetItemChecked(i, true);
                        if ((j == 38) && ((buf[indice + 19] & val1) > 0)) checkedListBox39.SetItemChecked(i, true);
                        if ((j == 39) && ((buf[indice + 19] & val2) > 0)) checkedListBox40.SetItemChecked(i, true);
                        if ((j == 40) && ((buf[indice + 20] & val1) > 0)) checkedListBox41.SetItemChecked(i, true);
                        if ((j == 41) && ((buf[indice + 20] & val2) > 0)) checkedListBox42.SetItemChecked(i, true);
                        if ((j == 42) && ((buf[indice + 21] & val1) > 0)) checkedListBox43.SetItemChecked(i, true);
                        if ((j == 43) && ((buf[indice + 21] & val2) > 0)) checkedListBox44.SetItemChecked(i, true);
                        if ((j == 44) && ((buf[indice + 22] & val1) > 0)) checkedListBox45.SetItemChecked(i, true);
                        if ((j == 45) && ((buf[indice + 22] & val2) > 0)) checkedListBox46.SetItemChecked(i, true);
                        if ((j == 46) && ((buf[indice + 23] & val1) > 0)) checkedListBox47.SetItemChecked(i, true);
                        if ((j == 47) && ((buf[indice + 23] & val2) > 0)) checkedListBox48.SetItemChecked(i, true);
                        if ((j == 48) && ((buf[indice + 24] & val1) > 0)) checkedListBox49.SetItemChecked(i, true);
                        if ((j == 49) && ((buf[indice + 24] & val2) > 0)) checkedListBox50.SetItemChecked(i, true);
                        if ((j == 50) && ((buf[indice + 25] & val1) > 0)) checkedListBox51.SetItemChecked(i, true);
                        if ((j == 51) && ((buf[indice + 25] & val2) > 0)) checkedListBox52.SetItemChecked(i, true);
                        if ((j == 52) && ((buf[indice + 26] & val1) > 0)) checkedListBox53.SetItemChecked(i, true);
                        if ((j == 53) && ((buf[indice + 26] & val2) > 0)) checkedListBox54.SetItemChecked(i, true);
                        if ((j == 54) && ((buf[indice + 27] & val1) > 0)) checkedListBox55.SetItemChecked(i, true);
                        if ((j == 55) && ((buf[indice + 27] & val2) > 0)) checkedListBox56.SetItemChecked(i, true);
                        if ((j == 56) && ((buf[indice + 28] & val1) > 0)) checkedListBox57.SetItemChecked(i, true);
                        if ((j == 57) && ((buf[indice + 28] & val2) > 0)) checkedListBox58.SetItemChecked(i, true);
                        if ((j == 58) && ((buf[indice + 29] & val1) > 0)) checkedListBox59.SetItemChecked(i, true);
                        if ((j == 59) && ((buf[indice + 29] & val2) > 0)) checkedListBox60.SetItemChecked(i, true);
                        if ((j == 60) && ((buf[indice + 30] & val1) > 0)) checkedListBox61.SetItemChecked(i, true);
                        if ((j == 61) && ((buf[indice + 30] & val2) > 0)) checkedListBox62.SetItemChecked(i, true);
                        if ((j == 62) && ((buf[indice + 31] & val1) > 0)) checkedListBox63.SetItemChecked(i, true);
                        if ((j == 63) && ((buf[indice + 31] & val2) > 0)) checkedListBox64.SetItemChecked(i, true);
                    }
                }
            }
            catch { };
        }


        //--------------------------------------------------------------------
        // Base de tempo de 100ms
        //
        // 
        //
        //--------------------------------------------------------------------
        public int tentativaProgressivo = 5;
        public int toutTcpClient_startListen = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // time out de comando progressivo 5 segundos
                if (timeoutCmdProgressivo > 0 )
                {
                    timeoutCmdProgressivo--;
                    if ( timeoutCmdProgressivo == 0 ) indiceProgressivo = 0;                    
                }
            }
            catch { };

            try
            {
                if( toutEstadoBiometria > 0 )
                {
                    toutEstadoBiometria--;
                    if(toutEstadoBiometria == 0)
                    {
                        lblResposta.Text = "Falha";
                        estadoBiometria = 0;
                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                    }
                }
            }
            catch { };

            try
            {
                if (toutSerial > 0)
                {
                    toutSerial--;
                    if (toutSerial == 0)
                    {
                        trataSerial();
                        indiceSerial = 0;
                    }
                }
            }
            catch { }

            try
            {
                if (toutConectar1 > 0)
                {
                    toutConectar1--;
                    if( toutConectar1 == 0 )
                    {
                        Conectar_TCP1();
                    }
                }
            }
            catch { }

            try
            {
                if (toutConectar5 > 0)
                {
                    toutConectar5--;
                    if (toutConectar5 == 0)
                    {
                        Conectar_TCP5();
                    }
                }
            }
            catch { }

            try
            {
                if (toutConectar6 > 0)
                {
                    toutConectar6--;
                    if (toutConectar6 == 0)
                    {
                        Conectar_TCP6();
                    }
                }
            }
            catch { }

            if( tout3s > 0 )
            {
                tout3s--;
                if (tout3s == 0)
                {
                    tout3s = 30;
                    if ( checkBox_ControleVagas_atualizar.Checked )
                    {
                        lerPacoteVagas();
                    }
                }
            }
            
            
            if (toutGravarTemplateProgressivo > 0)
            {
                toutGravarTemplateProgressivo--;
                if (toutGravarTemplateProgressivo == 0)
                {
                    if (emRestoreBiometria)
                    {
                        emRestoreBiometria = false;
                        executarNvezes = 0;
                        MessageBox.Show("Restore de arquivo biometria cancelado por timeout!");
                    }                    
                }
            }
            

            if (timeoutCmdStatusColor > 0 )
            {
                timeoutCmdStatusColor--;
                if (timeoutCmdStatusColor == 0)
                {
                    txtBox_IP1.BackColor = Color.White;
                    txt_portaTCP_1.BackColor = Color.White;
                    if (listView_cadastro_placasRede.Items.Count > 0)
                    {
                        for (int i = 0; i < listView_cadastro_placasRede.Items.Count; i++)
                        {
                            listView_cadastro_placasRede.Items[i].BackColor = Color.White;
                        }
                    }
                }
            }

            if( toutTcpClient_startListen > 0 )
            {
                toutTcpClient_startListen--;
                if ((toutTcpClient_startListen == 0) && ((listener == null) || ((listener != null) && (listener.IsBound == false))))
                {
                    StartListening();                    
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerSetup_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerSetup();
                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public void btLerInformacoes_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerInformacoes( Convert.ToByte(cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex) );
                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btGravarLerContadorAtualVagas_Click(object sender, EventArgs e)
        {
            try
            {
                byte operacao = controladoraLinear.OP_LEITURA;
                if( cboxContadorAtualVagas.SelectedIndex == 1 ) operacao = controladoraLinear.OP_ESCRITA;

                byte[] buf = pControl.gravarLerContadorAtualVagas(operacao, Convert.ToUInt16(txtContadorAtualVagas.Text));

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerGravarPacoteJornada_Click(object sender, EventArgs e)
        {
            try
            {
                int nPacotes;
                UInt16 indice;
                byte[] pacote = new byte[512];
                byte operacao = (byte)cboxOperacaoPacote.SelectedIndex;

                operacao = controladoraLinear.OP_ESCRITA;
                if (cboxOperacaoPacote.SelectedIndex == 0) operacao = controladoraLinear.OP_LEITURA;

                nPacotes = (int)Math.Round(Convert.ToDouble((double)((controladoraLinear.SIZE_OF_JORNADAS * controladoraLinear.N_LINES_JORNADAS) / 512)), 1);

                int i, j;
                for (indice = 0; indice < nPacotes; indice++)
                {
                    i = 0;
                    while ((i * controladoraLinear.SIZE_OF_JORNADAS) < 512)
                    {
                        for (j = 0; j < controladoraLinear.SIZE_OF_JORNADAS; j++)
                        {
                            pacote[i * controladoraLinear.SIZE_OF_JORNADAS + j] = memoriaPlaca.turnos[i + indice * 32, j];
                        }
                        i++;
                    }

                    byte[] buf = pControl.operacaoPacoteJornada(operacao, indice, pacote);

                    enviarMensagem(buf);
                }
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btEditarJornada_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.editarJornada((byte)(Convert.ToByte(cboxCodigoJornadaJFT.SelectedIndex)),
                                                 (byte)cboxSegunda.SelectedIndex,
                                                 (byte)cboxTerca.SelectedIndex,
                                                 (byte)cboxQuarta.SelectedIndex,
                                                 (byte)cboxQuinta.SelectedIndex,
                                                 (byte)cboxSexta.SelectedIndex,
                                                 (byte)cboxSabado.SelectedIndex,
                                                 (byte)cboxDomingo.SelectedIndex,
                                                 (byte)cboxFeriados.SelectedIndex);

                enviarMensagem(buf);
            }
            catch{}

        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        // 00 + 73 + <retorno> + <cód. Jornada> + <cód. turno segunda>  + <cód. turno terça> + <cód. turno quarta> + <cód. turno quinta> + <cód. turno sexta> + <cód. turno sábado> + <cód. turno domingo> + <cód. turno feriados> + cs       [13 bytes]
        private void trataRespostaCmd_lerJornada(byte[] buf)
        {
            try
            {
                cboxSegunda.SelectedIndex = (buf[4] & 0x7F);
                cboxTerca.SelectedIndex = (buf[5] & 0x7F);
                cboxQuarta.SelectedIndex = (buf[6] & 0x7F);
                cboxQuinta.SelectedIndex = (buf[7] & 0x7F);
                cboxSexta.SelectedIndex = (buf[8] & 0x7F);
                cboxSabado.SelectedIndex = (buf[9] & 0x7F);
                cboxDomingo.SelectedIndex = (buf[10] & 0x7F);
                cboxFeriados.SelectedIndex = (buf[11] & 0x7F);
            }
            catch { }
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        // RSP: 00 + 74 + <retorno> + <cntAtual> + <frame de evt. (16 bytes)> + cs      [21 bytes]
        private void trataRespostaCmd_eventoAutomatico(byte[] buf, String epOrigem )
        {
            try
            {
                if (ckMostrarEventoAutomatico.Checked == true)
                {
                    trataRespostaCmd_lerEventos(buf, epOrigem );
                }

                if( ckBoxControleVagas_SETUP_GERAL.Checked )
                {
                    buf = pControl.gravarLerContadorAtualVagas(controladoraLinear.OP_LEITURA, Convert.ToUInt16(txtContadorAtualVagas.Text));
                    enviarMensagem(buf);
                }
            }
            catch { }
        }
        //--------------------------------------------------------------------
        // 135 Leitura de evento com endereço do ponteiro (EVENTO INDEXADO em PACOTE DE DADOS)
        // 00 + 87 + cs [3 bytes]
        // 00 + 87 + <retorno> + < 32 x (frame de evt. (16 bytes) + <endereçoEventoH> + <endereçoEventoL>)> + cs [ 580 bytes]
        //
        //--------------------------------------------------------------------
        public void trataRespostaCmd_lerPacoteEventos(byte[] buf, String epOrigem)
        {
            int i, j = 0;
            String sTemp = null;
            String s = null;
            byte[] buf2 = new byte[16];
            UInt16 val = 0;

            for (i = 0; i < 32; i++)
            {
                for (j = 0; j < 16; j++) buf2[j] = buf[5 + j + (i * 18)];
                val = (UInt16)((buf[5 + 16 + (i * 18)] << 8) + buf[5 + 17 + (i * 18)]);
                sTemp = BitConverter.ToString(buf2);
                labelUltimoEnderecoEventoRecebido.Text = val.ToString("0000");
                s = sTemp.Replace("-", " ");
                s += "  [indice:" + val.ToString("0000");
                s += "   marca:";
                if(( buf2[15] & 0x08 ) > 0 )
                {
                    s += "LIDO    ]";
                }
                else
                {
                    s += "NÃO LIDO]";
                }
                s += '\r';
                s += '\n';
                richTextPacote.Text += s;
            }


            try
            {
                comboBoxUltimoEnderecoEventoRecebido.SelectedIndex = val;
            }
            catch
            {

            }

        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        // 138	Ler / Gravar Vagas por Habilitação	
        // CMD: 00 + 8A + <operação> + <codHAB(High)> +  <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs              [8 bytes]
        // RSP:	00 + 8A + <retorno> + <operação> + <codHAB(High)> +  <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs  [9 bytes] 
        public void trataRespostaCmd_operacaoVagasHabilitacao( byte[] buf, String epOrigem )
        {
            comboBoxCtrlVagasHab_Rota1.SelectedIndex = (UInt16)((buf[4] << 8) + buf[5]);
            comboBoxCtrlVagasPreset.SelectedIndex = buf[6];
            comboBoxCtrlVagasAtual.SelectedIndex = buf[7];
        }            
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        // 140	Ler Vagas especifico
        // CMD: 00 + 8C + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + cs           [10 bytes]	
        // RSP: 00 + 8C + <retorno> + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + <codHAB(High)> + <codHAB(Low)> 
        //   + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs  [15 bytes]
        public void trataRespostaCmd_lerVagasEspecifico(byte[] buf, String epOrigem)
        {
            String serial_1, serial_2;
            byte[] serialTemp = new byte[6];
            int indiceList = 0xFFFF;

            if( eventoPassagem == false )
            {
                textBoxCtrlVagas_rota.Text = Convert.ToString((UInt16)((buf[10] << 8) + buf[11] + 1 ));
                textBoxCtrlVagas_presetVagas.Text = Convert.ToString(buf[12]);
                textBoxCtrlVagas_VagasAtual.Text = Convert.ToString(buf[13]);
            }
            else
            {
                eventoPassagem = false;
                if (buf[3] == controladoraLinear.DISP_TX)
                {
                    serialTemp[2] = buf[4];
                    serialTemp[3] = buf[5];
                    serialTemp[4] = buf[6];
                    serialTemp[5] = buf[7];
                }
                else
                {
                    serialTemp[0] = buf[4];
                    serialTemp[1] = buf[5];
                    serialTemp[2] = buf[6];
                    serialTemp[3] = buf[7];
                    serialTemp[4] = buf[8];
                    serialTemp[5] = buf[9];
                }
            }

            /*
            0 novoEvento.Text = lblNumeroSerie.Text;
            1 novoEvento.SubItems.Add(lblTipoDispositivo.Text);
            2 novoEvento.SubItems.Add("-"); // preset
            3 novoEvento.SubItems.Add("-"); // vagas
            4 novoEvento.SubItems.Add(lblEnderecoPlaca.Text);
            5 novoEvento.SubItems.Add(lblNivel.Text);
            6 novoEvento.SubItems.Add(lblDataHoraEvento.Text);
            7 novoEvento.SubItems.Add(lblInformacaoEvento.Text);
            novoEvento.Tag = evento;
             */

            serial_2 = BytesToHexString(serialTemp);

            for(int i=0;i<listView_vagas.Items.Count;i++)
            {
                serial_1 = listView_vagas.Items[i].SubItems[0].Text;
                if (serial_1 == serial_2)
                {
                    indiceList = i;
                    break;
                }
            }

            if ( indiceList != 0xFFFF )
            {
                listView_vagas.Items[indiceList].SubItems[2].Text = Convert.ToString(buf[12]);
                listView_vagas.Items[indiceList].SubItems[3].Text = Convert.ToString(buf[13]);
            }
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        //--------------------------------------------------------------------------------------------------------
        // 141	Marcar PACOTE de evento como "lido" a partir do endereço do evento (EVENTO INDEXADO em PACOTE DE DADOS)	
        // CMD: 00 + 8D + <endereçoEventoH> + <endereçoEventoL> + cs [4 bytes]
        // RSP: 00 + 8D + <retorno> + <endereçoEventoH> + <endereçoEventoL> + cs [5 bytes]
        public void trataRespostaCmd_marcarPacoteEventoIndexadoComoLido(byte[] buf, String epOrigem)
        {


        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btLerJornada_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerJornada(Convert.ToByte(cboxCodigoJornadaJFT.SelectedIndex));                                                 
                enviarMensagem(buf);
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btAbrirArquivoDispositivos_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.FileName = "DISP.DPT";
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                    label_txtArquivo.Text = sr.ReadLine();
                    arquivoRestore = sr.ReadToEnd();
                    if (arquivoRestore != null) btGravarDispositivo_PROGRESSIVO.Enabled = true;

                    sr.Close();
                }
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        private void btGravarSetup_Click(object sender, EventArgs e)
        {
            try
            {
                ckBoxUdpBroadcast.Checked = false;
                gravaSetupForm();
            }
            catch { };
        }



        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public void gravaSetupForm()
        {
            try
            {
                int i;

                memoriaPlaca.setup.cfg1.brCan = (byte)cboxBrCan_SETUP_GERAL.SelectedIndex;
                memoriaPlaca.setup.cfg1.ctrlVagasNivel = 0;
                if (ckBoxControleVagas_SETUP_GERAL.Checked) memoriaPlaca.setup.cfg1.ctrlVagasNivel = 1;

                memoriaPlaca.setup.cfg1.RFHabilitado = 0;
                if (ckRFHabilitado.Checked == true) memoriaPlaca.setup.cfg1.RFHabilitado = 1;

                memoriaPlaca.setup.cfg2.funcaoUart2 = (byte)cboxFuncaoUart2.SelectedIndex;
                memoriaPlaca.setup.cfg2.funcaoUart3 = (byte)cboxFuncaoUart3.SelectedIndex;
                memoriaPlaca.setup.cfg3.baudRateUart1 = (byte)cboxBrUart1.SelectedIndex;
                memoriaPlaca.setup.cfg3.baudRateUart2 = (byte)cboxBrUart2.SelectedIndex;
                memoriaPlaca.setup.cfg3.paridadeUart1 = (byte)cboxParidadeUart1.SelectedIndex;
                memoriaPlaca.setup.cfg3.stopBitsUart1 = (byte)cboxStopBitsUart1.SelectedIndex;
                memoriaPlaca.setup.cfg4.baudRateUart3 = (byte)cboxBrUart3.SelectedIndex;
                memoriaPlaca.setup.cfg4.tipoDispLeitora1 = converteIndexParaTipoDispositivo((byte)cboxTipoDispL1.SelectedIndex);
                memoriaPlaca.setup.cfg4.tipoDispLeitora2 = converteIndexParaTipoDispositivo((byte)cboxTipoDispL2.SelectedIndex);
                memoriaPlaca.setup.cfg5.tipoDispLeitora3 = converteIndexParaTipoDispositivo((byte)cboxTipoDispL3.SelectedIndex);
                memoriaPlaca.setup.cfg5.tipoDispLeitora4 = converteIndexParaTipoDispositivo((byte)cboxTipoDispL4.SelectedIndex);
                memoriaPlaca.setup.cfg5.sinalizacaoSaidasDigitais = (byte)cboxSinalizaSaida.SelectedIndex;
                memoriaPlaca.setup.cfg6.eventoSensor1 = (byte)cboxEventoS1.SelectedIndex;
                memoriaPlaca.setup.cfg6.eventoSensor2 = (byte)cboxEventoS2.SelectedIndex;
                memoriaPlaca.setup.cfg6.eventoSensor3 = (byte)cboxEventoS3.SelectedIndex;
                memoriaPlaca.setup.cfg6.eventoSensor4 = (byte)cboxEventoS4.SelectedIndex;

                memoriaPlaca.setup.cfg3.enviarTemplateNaoCadastrado = 0;
                if (ckBoxEnviarTemplateNaoCadastrado.Checked) memoriaPlaca.setup.cfg3.enviarTemplateNaoCadastrado = 1;
                
                memoriaPlaca.setup.cfg7.avisoLowBat = 0;
                if (ckBoxAvisoBateriaBaixa.Checked) memoriaPlaca.setup.cfg7.avisoLowBat = 1;

                memoriaPlaca.setup.cfg1.RFHabilitado = 0;
                if (ckRFHabilitado.Checked) memoriaPlaca.setup.cfg1.RFHabilitado = 1;

                memoriaPlaca.setup.cfg7.toutMsgDisplay = (byte)cboxTempoMsgDisplay.SelectedIndex;
                memoriaPlaca.setup.cfg7.toutPanico = (byte)cboxTempoPanico.SelectedIndex;
                memoriaPlaca.setup.cfg8.autonomia = (byte)cboxAutonomia.SelectedIndex;
                memoriaPlaca.setup.cfg8.catraca2010 = (byte)cboxCatraca2010.SelectedIndex;

                memoriaPlaca.setup.cfg8.emHorarioVerao = 0;
                if (ckBoxEmHorarioVerao.Checked == true) memoriaPlaca.setup.cfg8.emHorarioVerao = 1;

                memoriaPlaca.setup.cfg8.horarioVeraoHabilitado = 0;
                if (ckBoxHorarioVeraoHabilitado.Checked == true) memoriaPlaca.setup.cfg8.horarioVeraoHabilitado = 1;

                memoriaPlaca.setup.cfg8.leitoraManchester = 0;
                if (ckBoxLeitoraManchester.Checked == true) memoriaPlaca.setup.cfg8.leitoraManchester = 1;

                // catraca
                memoriaPlaca.setup.cfg1.giroCatraca = (byte)cboxGiroCatraca.SelectedIndex;

                memoriaPlaca.setup.cfg2.sentidoCatraca = 0;
                if (ckBoxInverterCatraca.Checked == true) memoriaPlaca.setup.cfg2.sentidoCatraca = 1;


                memoriaPlaca.setup.cfg19.antipassbackDesligado_entrada = 0;
                if (ckboxAntipassbackDesligadoEntrada.Checked == true) memoriaPlaca.setup.cfg19.antipassbackDesligado_entrada = 1;

                memoriaPlaca.setup.cfg19.antipassbackDesligado_saida = 0;
                if (ckboxAntipassbackDesligadoSaida.Checked == true) memoriaPlaca.setup.cfg19.antipassbackDesligado_saida = 1;

                memoriaPlaca.setup.cfg2.umaSolenoide = 0;
                if (ckBoxUmaSolenoide.Checked == true) memoriaPlaca.setup.cfg2.umaSolenoide = 1;
                
                memoriaPlaca.setup.cfg8.saidaLivre = 0;
                if (ckBoxSaidaLivre.Checked == true) memoriaPlaca.setup.cfg8.saidaLivre = 1;

                memoriaPlaca.setup.cfg9.catracaAdaptada = 0;
                if (ckBoxCatracaAdaptada.Checked == true) memoriaPlaca.setup.cfg9.catracaAdaptada = 1;

                memoriaPlaca.setup.cfg9.tratarBiometria_1_1 = 0;
                if (ckBoxValidarBiometria1_1.Checked == true) memoriaPlaca.setup.cfg9.tratarBiometria_1_1 = 1;

                memoriaPlaca.setup.cfg9.antipassbackDesligado = 0;
                if (checkBoxAntiPassbackDesligado.Checked == true) memoriaPlaca.setup.cfg9.antipassbackDesligado = 1;

                memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_1e2 = 0;
                if (checkBoxSetupPorta_Sensor12_AP.Checked == true) memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_1e2 = 1;
                
                memoriaPlaca.setup.cfg9.considerarWiegand26bits = 0;
                if (checkBoxTruncar32bits.Checked == true) memoriaPlaca.setup.cfg9.considerarWiegand26bits = 1;

                memoriaPlaca.setup.cfg9.alarmeEntradaDigitalAberta = 0;
                if (checkBoxSetupPorta_AlarmePortaAberta.Checked == true) memoriaPlaca.setup.cfg9.alarmeEntradaDigitalAberta = 1;

                memoriaPlaca.setup.cfg9.eventoEntradaDigitalAberta = 0;
                if (checkBoxSetupPorta_EventoPortaAberta.Checked == true) memoriaPlaca.setup.cfg9.eventoEntradaDigitalAberta = 1;

                memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_3e4 = 0;
                if ( checkBoxSetupPorta_Sensor34_AP.Checked == true ) memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_3e4 = 1;

                memoriaPlaca.setup.tempoEventoEntradaDigitalAberta = Convert.ToByte( textBoxSetupPorta_TempoEntreEventosPortaAberta.Text );
                memoriaPlaca.setup.toutAlarmeEntradaDigitalAberta = Convert.ToByte( textBoxSetupPorta_ToutAlarmePortaAberta.Text );
                memoriaPlaca.setup.toutAcionamentoLedBuzzer = Convert.ToByte(textBoxSetupPorta_tempoLedBuzzer.Text);

                

                memoriaPlaca.setup.cntAtual = Convert.ToByte(txtContadorAtual_SETUP_GERAL.Text);
                memoriaPlaca.setup.endCan = (byte)cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex;
                memoriaPlaca.setup.modo = (byte)cboxModoOperacao.SelectedIndex;

                memoriaPlaca.setup.setor = (byte)(comboBox_SETUP_setor.SelectedIndex & 0x07);
                memoriaPlaca.setup.reservado_10 = 0;

                memoriaPlaca.setup.nivel = Convert.ToByte(txtNivel_SETUP_GERAL.Text);
                memoriaPlaca.setup.vagas = Convert.ToUInt16(txtVagas_SETUP_GERAL.Text);

                memoriaPlaca.setup.tRele[0] = Convert.ToByte(txtTempoRele1.Text);
                memoriaPlaca.setup.tRele[1] = Convert.ToByte(txtTempoRele2.Text);
                memoriaPlaca.setup.tRele[2] = Convert.ToByte(txtTempoRele3.Text);
                memoriaPlaca.setup.tRele[3] = Convert.ToByte(txtTempoRele4.Text);

                memoriaPlaca.setup.atrasoEntradaLeitora[0] = Convert.ToByte(textBox_SETUP_PORTA_atrasoEntradaLeitora1.Text);
                memoriaPlaca.setup.atrasoEntradaLeitora[1] = Convert.ToByte(textBox_SETUP_PORTA_atrasoEntradaLeitora2.Text);
                memoriaPlaca.setup.atrasoEntradaLeitora[2] = Convert.ToByte(textBox_SETUP_PORTA_atrasoEntradaLeitora3.Text);
                memoriaPlaca.setup.atrasoEntradaLeitora[3] = Convert.ToByte(textBox_SETUP_PORTA_atrasoEntradaLeitora4.Text);
                
                memoriaPlaca.setup.tSaida[0] = Convert.ToByte(txtTempoSaida1_SETUP_PORTA.Text);
                memoriaPlaca.setup.tSaida[1] = Convert.ToByte(txtTempoSaida2_SETUP_PORTA.Text);
                memoriaPlaca.setup.tSaida[2] = Convert.ToByte(txtTempoSaida3_SETUP_PORTA.Text);
                memoriaPlaca.setup.tSaida[3] = Convert.ToByte(txtTempoSaida4_SETUP_PORTA.Text);
                memoriaPlaca.setup.tempoPassagem = Convert.ToByte(txtTempoPassagemCatraca_SETUP_CATRACA.Text);
                memoriaPlaca.setup.tempoLeituraCofre = Convert.ToByte(txtTempoLeituraCofre_SETUP_CATRACA.Text);

                memoriaPlaca.setup.toutPassagem[0] = Convert.ToByte(txtTempoPassagem1.Text);
                memoriaPlaca.setup.toutPassagem[1] = Convert.ToByte(txtTempoPassagem2.Text);
                memoriaPlaca.setup.toutPassagem[2] = Convert.ToByte(txtTempoPassagem3.Text);
                memoriaPlaca.setup.toutPassagem[3] = Convert.ToByte(txtTempoPassagem4.Text);

                memoriaPlaca.setup.anticarona[0] = Convert.ToByte(txtTempoAnticarona1.Text);
                memoriaPlaca.setup.anticarona[1] = Convert.ToByte(txtTempoAnticarona2.Text);
                memoriaPlaca.setup.anticarona[2] = Convert.ToByte(txtTempoAnticarona3.Text);
                memoriaPlaca.setup.anticarona[3] = Convert.ToByte(txtTempoAnticarona4.Text);
                memoriaPlaca.setup.cntAtualDisp = Convert.ToByte(txtContadorAtualDisp.Text);

                for (i = 0; i < controladoraLinear.SIZE_OF_BYTES_RESERVADOS; i++) memoriaPlaca.setup.reservado[i] = 0;

                IPAddress ipTemp = IPAddress.Parse(txtIP_SETUP_TCPIP.Text);
                byte[] ipBuf = new byte[4];

                ipBuf = ipTemp.GetAddressBytes();
                memoriaPlaca.setup.ip = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipTemp = IPAddress.Parse(txtGateway_SETUP_TCPIP.Text);
                ipBuf = ipTemp.GetAddressBytes();
                memoriaPlaca.setup.gw = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipTemp = IPAddress.Parse(txtMask_SETUP_TCPIP.Text);
                ipBuf = ipTemp.GetAddressBytes();
                memoriaPlaca.setup.mask = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipTemp = IPAddress.Parse(txtIPDestino.Text);
                ipBuf = ipTemp.GetAddressBytes();
                memoriaPlaca.setup.ipDestino = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                memoriaPlaca.setup.cfge.dhcp = 0;
                if (ckHabilitaDHCP.Checked == true) memoriaPlaca.setup.cfge.dhcp = 1;

                memoriaPlaca.setup.cfge.DDNS = (byte)cboxServidorDDNS.SelectedIndex;

                memoriaPlaca.setup.toutEthernet1 = Convert.ToByte(txtTimeoutEthernet1.Text);
                memoriaPlaca.setup.toutEthernet2 = Convert.ToByte(txtTimeoutEthernet2.Text);

                memoriaPlaca.setup.porta1 = Convert.ToUInt16(txtPorta1_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta2 = Convert.ToUInt16(txtPorta2_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta3 = Convert.ToUInt16(txtPorta3_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta4 = Convert.ToUInt16(txtPorta4_SETUP_TCPIP.Text);

                for (i = 0; i < controladoraLinear.SIZE_OF_BYTES_RESERVADOS; i++) memoriaPlaca.setup.reservado[i] = 0;


                memoriaPlaca.setup.cfge.DDNShabilitado = 0;
                if (checkBox_setup_setupTcpIp_habilitarDDNS.Checked) memoriaPlaca.setup.cfge.DDNShabilitado = 1;

                convertStringParaByteArray(ref memoriaPlaca.setup.DDNSsenha, txtDDNSSenha.Text, txtDDNSSenha.TextLength);
                convertStringParaByteArray(ref memoriaPlaca.setup.DDNSusuario, txtDDNSUsuario.Text, txtDDNSUsuario.TextLength);
                convertStringParaByteArray(ref memoriaPlaca.setup.DDNSdevice, txtDDNSHost.Text, txtDDNSHost.TextLength);

                convertStringParaByteArray(ref memoriaPlaca.setup.usuarioLogin, txtUsuarioLogin.Text, txtUsuarioLogin.TextLength);
                convertStringParaByteArray(ref memoriaPlaca.setup.senhaLogin, txtSenhaLogin.Text, txtSenhaLogin.TextLength);
                convertStringParaByteArray(ref memoriaPlaca.setup.dnsHost, txtHost_SETUP_TCPIP.Text, txtHost_SETUP_TCPIP.TextLength);

                memoriaPlaca.setup.toleranciaHorario = Convert.ToByte(txtToleranciaHorarios.Text);
                memoriaPlaca.setup.funcaoLeitora[0] = (byte)cboxFuncaoLeitora1.SelectedIndex;
                memoriaPlaca.setup.funcaoLeitora[1] = (byte)cboxFuncaoLeitora2.SelectedIndex;
                memoriaPlaca.setup.funcaoLeitora[2] = (byte)cboxFuncaoLeitora3.SelectedIndex;
                memoriaPlaca.setup.funcaoLeitora[3] = (byte)cboxFuncaoLeitora4.SelectedIndex;


                memoriaPlaca.setup.cfg17.baudrateRS485_1 = (byte)comboBox_setup_rs485_baudrate_rs485_1.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_2 = (byte)comboBox_setup_rs485_baudrate_rs485_2.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_3 = (byte)comboBox_setup_rs485_baudrate_rs485_3.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_4 = (byte)comboBox_setup_rs485_baudrate_rs485_4.SelectedIndex;

                memoriaPlaca.setup.cfg18.retardoDesligarSolenoide = (byte)comboBox_setup_setupCatraca_retardoDesligarSolenoide.SelectedIndex;

                memoriaPlaca.setup.cfg18.desligaRetornoAutoT5S = 0;
                if (checkBox_SETUP_SETUPGERAL_desligaRetornoAutoT5S.Checked) memoriaPlaca.setup.cfg18.desligaRetornoAutoT5S = 1;
                memoriaPlaca.setup.cfg18.facilityWieg66 = 0;
                if (checkBox_SETUP_SETUPGERAL_facilityWieg66.Checked) memoriaPlaca.setup.cfg18.facilityWieg66 = 1;
                memoriaPlaca.setup.cfg18.habilitaAtualizacaoAuto = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaAtualizacaoAuto.Checked) memoriaPlaca.setup.cfg18.habilitaAtualizacaoAuto = 1;
                memoriaPlaca.setup.cfg18.habilitaContagemPassagem = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaContagemPassagem.Checked) memoriaPlaca.setup.cfg18.habilitaContagemPassagem = 1;
                memoriaPlaca.setup.cfg18.habilitaIDMestre = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaIDMestre.Checked) memoriaPlaca.setup.cfg18.habilitaIDMestre = 1;
                memoriaPlaca.setup.cfg18.habilitaAcomodacao = 0;
                if (checkBoxSetupPorta_HabilitaAcomodacao.Checked) memoriaPlaca.setup.cfg18.habilitaAcomodacao = 1;

                memoriaPlaca.setup.cfg19.sensorPassagemCofre = 0;
                if (checkBox_SETUP_SETUPGERAL_SensorPassagem.Checked) memoriaPlaca.setup.cfg19.sensorPassagemCofre = 1;
                memoriaPlaca.setup.cfg19.sw_watchdog = 0;
                if (checkBox_SETUP_SETUPGERAL_sw_watchdog.Checked) memoriaPlaca.setup.cfg19.sw_watchdog = 1;
                
                memoriaPlaca.setup.cfg19.verificarFirmwareFTP = 0; // apenas reservado, não implementado

                memoriaPlaca.setup.cfg19.sensorGiroInvertido = 0;
                if ( ckBoxSensorGiroInvertido.Checked ) memoriaPlaca.setup.cfg19.sensorGiroInvertido = 1;
                memoriaPlaca.setup.cfg19.desativaRelePassagem = 0;
                if (checkBoxSetupPorta_desativaRelePassagem.Checked) memoriaPlaca.setup.cfg19.desativaRelePassagem = 1;
                memoriaPlaca.setup.cfg19.duplaValidacaoVisitante = 0;
                if(checkBox_SETUP_SETUPGERAL_duplaValidacaoVisitante.Checked) memoriaPlaca.setup.cfg19.duplaValidacaoVisitante = 1;


                memoriaPlaca.setup.tempoLedBuzzerLeitoras = Convert.ToByte(txtTempoLedBuzzer.Text);
                memoriaPlaca.setup.toutPassback = Convert.ToByte(txtTempoAntipassback.Text);

                DateTime result = DTPickerHorarioVeraoIni.Value;
                memoriaPlaca.setup.horarioVerao[0] = (byte)result.Date.Day;
                memoriaPlaca.setup.horarioVerao[1] = (byte)result.Date.Month;
                result = DTPickerHorarioVeraoFim.Value;
                memoriaPlaca.setup.horarioVerao[2] = (byte)result.Date.Day;
                memoriaPlaca.setup.horarioVerao[3] = (byte)result.Date.Month;

                memoriaPlaca.setup.porta5 = Convert.ToUInt16(txtPorta5_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta6 = Convert.ToUInt16(txtPorta6_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta7 = Convert.ToUInt16(txtPorta7_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta8 = Convert.ToUInt16(txtPorta8_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta9 = Convert.ToUInt16(txtPorta9_SETUP_TCPIP.Text);
                memoriaPlaca.setup.portaHttp = Convert.ToUInt16(textBox_setup_setupip_portaHttp.Text);

                memoriaPlaca.setup.porta10 = Convert.ToUInt16(txtPorta10_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta11 = Convert.ToUInt16(txtPorta11_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta12 = Convert.ToUInt16(txtPorta12_SETUP_TCPIP.Text);
                memoriaPlaca.setup.porta13 = Convert.ToUInt16(txtPorta13_SETUP_TCPIP.Text);



                // SETUP EVENTOS
                memoriaPlaca.setup.cfg10.filtrarEvento0 = 0;
                if (checkBoxSetupEventos_filtroEvento0.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento0 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento1 = 0;
                if (checkBoxSetupEventos_filtroEvento1.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento1 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento2 = 0;
                if (checkBoxSetupEventos_filtroEvento2.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento2 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento3 = 0;
                if (checkBoxSetupEventos_filtroEvento3.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento3 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento4 = 0;
                if (checkBoxSetupEventos_filtroEvento4.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento4 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento5 = 0;
                if (checkBoxSetupEventos_filtroEvento5.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento5 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento6 = 0;
                if (checkBoxSetupEventos_filtroEvento6.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento6 = 1;
                memoriaPlaca.setup.cfg10.filtrarEvento7 = 0;
                if (checkBoxSetupEventos_filtroEvento7.Checked == true) memoriaPlaca.setup.cfg10.filtrarEvento7 = 1;


                memoriaPlaca.setup.cfg11.filtrarEvento8 = 0;
                if (checkBoxSetupEventos_filtroEvento8.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento8 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento9 = 0;
                if (checkBoxSetupEventos_filtroEvento9.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento9 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento10 = 0;
                if (checkBoxSetupEventos_filtroEvento10.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento10 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento11 = 0;
                if (checkBoxSetupEventos_filtroEvento11.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento11 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento12 = 0;
                if (checkBoxSetupEventos_filtroEvento12.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento12 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento13 = 0;
                if (checkBoxSetupEventos_filtroEvento13.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento13 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento14 = 0;
                if (checkBoxSetupEventos_filtroEvento14.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento14 = 1;
                memoriaPlaca.setup.cfg11.filtrarEvento15 = 0;
                if (checkBoxSetupEventos_filtroEvento15.Checked == true) memoriaPlaca.setup.cfg11.filtrarEvento15 = 1;


                memoriaPlaca.setup.cfg12.filtrarEvento16 = 0;
                if (checkBoxSetupEventos_filtroEvento16.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento16 = 1;
                memoriaPlaca.setup.cfg12.filtrarEvento17 = 0;
                if (checkBoxSetupEventos_filtroEvento17.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento17 = 1;
                memoriaPlaca.setup.cfg12.filtrarEvento18 = 0;
                if (checkBoxSetupEventos_filtroEvento18.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento18 = 1;
                memoriaPlaca.setup.cfg12.filtrarEvento19 = 0;
                if (checkBoxSetupEventos_filtroEvento19.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento19 = 1;
                
                memoriaPlaca.setup.cfg12.filtrarEvento20 = 0;
                if (checkBoxSetupEventos_filtroEvento20.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento20 = 1;
                
                memoriaPlaca.setup.cfg12.filtrarEvento21 = 0;
                if (checkBoxSetupEventos_filtroEvento21.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento21 = 1;
                
                /*
                memoriaPlaca.setup.cfg12.filtrarEvento22 = 0;
                if (checkBoxSetupEventos_filtroEvento22.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento22 = 1;
                memoriaPlaca.setup.cfg12.filtrarEvento23 = 0;
                if (checkBoxSetupEventos_filtroEvento23.Checked == true) memoriaPlaca.setup.cfg12.filtrarEvento23 = 1;
                */

                memoriaPlaca.setup.cfg16.senha13digitos = 0;
                if (checkBox_setup_setupGeral_senha13.Checked) memoriaPlaca.setup.cfg16.senha13digitos = 1;

                memoriaPlaca.setup.cfg16.direcaoTurnos = 0;
                if (checkBox_setup_controleEntradaSaidaTurnos.Checked) memoriaPlaca.setup.cfg16.direcaoTurnos = 1;


                memoriaPlaca.setup.cfg16.panicoSenha99 = 0;
                if (checkBox_setup_panicoSenha99.Checked) memoriaPlaca.setup.cfg16.panicoSenha99 = 1;

                memoriaPlaca.setup.cfg16.desligarBuzzer = 0;
                if (checkBox_SETUP_SETUPGERAL_desligarBuzzer.Checked) memoriaPlaca.setup.cfg16.desligarBuzzer = 1;


                memoriaPlaca.setup.cfg16.tipoCatraca = (byte)comboBox_setup_setupCatraca_tipoCatraca.SelectedIndex;


                //if( memoriaPlaca.setup.cfg16.tipoCatraca == controladoraLinear.TIPO_CA
                memoriaPlaca.setup.cfg9.catracaAdaptada = 0;
                if (ckBoxCatracaAdaptada.Checked == true) memoriaPlaca.setup.cfg9.catracaAdaptada = 1;

                memoriaPlaca.setup.cfg13.controleVagasRota = 0;
                if (checkBoxSetupGeral_ctrlVagasHab.Checked) memoriaPlaca.setup.cfg13.controleVagasRota = 1;

                memoriaPlaca.setup.cfg13.nivelSegurancaBiometria = (byte)comboBoxNivelSegurancaBiometria.SelectedIndex;
                
                memoriaPlaca.setup.cfg13.desvincularVisitante = 0;
                if( checkBoxSetupGeral_desvincularVisitante.Checked ) memoriaPlaca.setup.cfg13.desvincularVisitante = 1;

                memoriaPlaca.setup.cfg13.enviarEventoIndexado = 0;
                if (checkBoxEventoIndexado.Checked) memoriaPlaca.setup.cfg13.enviarEventoIndexado = 1;

                memoriaPlaca.setup.cfg13.validarBiometriaOnline_emRemoto = 0;
                if (checkBoxValidarBiometriaOnline.Checked) memoriaPlaca.setup.cfg13.validarBiometriaOnline_emRemoto = 1;

                // CFG_14
                memoriaPlaca.setup.cfg14.panicoSaidaDigital = (byte)(comboBox_setup_ctrlPorta_panicoSaidaDigital.SelectedIndex & 0x03);
                
                memoriaPlaca.setup.cfg14.cartao2xPanico = 0;
                if (checkBox_setup_porta_2xCartaoPanico.Checked) memoriaPlaca.setup.cfg14.cartao2xPanico = 1;

                if( radioButton_setup_setupRS485_tagTipo_LINEAR.Checked )  memoriaPlaca.setup.cfg14.tipoTag_UHF = 0;
                if (radioButton_setup_setupRS485_tagTipo_EPCTID.Checked) memoriaPlaca.setup.cfg14.tipoTag_UHF = 1;
                if( radioButton_setup_setupRS485_tagTipo_EPC.Checked ) memoriaPlaca.setup.cfg14.tipoTag_UHF = 2;
                if (radioButton_setup_setupRS485_tagTipo_TID.Checked) memoriaPlaca.setup.cfg14.tipoTag_UHF = 3;
                
                if ( radioButton_setup_setupRS485_varredura_pooling.Checked ) memoriaPlaca.setup.cfg14.varreSimultanea_UHF = 0;
                if ( radioButton_setup_setupRS485_varredura_simultanea.Checked) memoriaPlaca.setup.cfg14.varreSimultanea_UHF = 1;

                if ( radioButton_setup_setupRS485_wiegand_26.Checked ) memoriaPlaca.setup.cfg14.wiegand34_UHF = 0;
                if ( radioButton_setup_setupRS485_wiegand_34.Checked ) memoriaPlaca.setup.cfg14.wiegand34_UHF = 1;

                if ( radioButton_setup_setupRS485_freq_fixed.Checked ) memoriaPlaca.setup.cfg14.frequenciaHopping_UHF = 0;
                if (radioButton_setup_setupRS485_freq_hopping.Checked) memoriaPlaca.setup.cfg14.frequenciaHopping_UHF = 1;

                // CFG_15
                if (radioButton_setup_setupRS485_bzOn_L1.Checked) memoriaPlaca.setup.cfg15.buzzer_L1_UHF_on = 1;
                if (radioButton_setup_setupRS485_bzOff_L1.Checked) memoriaPlaca.setup.cfg15.buzzer_L1_UHF_on = 0;

                if (radioButton_setup_setupRS485_bzOn_L2.Checked) memoriaPlaca.setup.cfg15.buzzer_L2_UHF_on = 1;
                if (radioButton_setup_setupRS485_bzOff_L2.Checked) memoriaPlaca.setup.cfg15.buzzer_L2_UHF_on = 0;

                if (radioButton_setup_setupRS485_bzOn_L3.Checked) memoriaPlaca.setup.cfg15.buzzer_L3_UHF_on = 1;
                if (radioButton_setup_setupRS485_bzOff_L3.Checked) memoriaPlaca.setup.cfg15.buzzer_L3_UHF_on = 0;

                if (radioButton_setup_setupRS485_bzOn_L4.Checked) memoriaPlaca.setup.cfg15.buzzer_L4_UHF_on = 1;
                if (radioButton_setup_setupRS485_bzOff_L4.Checked) memoriaPlaca.setup.cfg15.buzzer_L4_UHF_on = 0;


                memoriaPlaca.setup.cfg15.rele5_eventoLigado = 0;
                memoriaPlaca.setup.cfg15.rele5_eventoClonagem = 0;
                memoriaPlaca.setup.cfg15.rele5_eventoPanico = 0;
                memoriaPlaca.setup.cfg15.rele5_eventoAlarme = 0;
                if (checkBox_setup_setupReles_ev2.Checked)
                {
                    memoriaPlaca.setup.cfg15.rele5_eventoLigado = 1;
                }
                else
                {
                    if (checkBox_setup_setupReles_ev8.Checked) memoriaPlaca.setup.cfg15.rele5_eventoClonagem = 1;
                    if (checkBox_setup_setupReles_ev9.Checked) memoriaPlaca.setup.cfg15.rele5_eventoPanico = 1;
                    if (checkBox_setup_setupReles_ev19.Checked) memoriaPlaca.setup.cfg15.rele5_eventoAlarme = 1;
                }

                //
                memoriaPlaca.setup.funcaoRS485[0] = (byte)comboBox_setup_rs485_funcaoLeitoraRS485_1.SelectedIndex;
                memoriaPlaca.setup.funcaoRS485[1] = (byte)comboBox_setup_rs485_funcaoLeitoraRS485_2.SelectedIndex;
                memoriaPlaca.setup.funcaoRS485[2] = (byte)comboBox_setup_rs485_funcaoLeitoraRS485_3.SelectedIndex;
                memoriaPlaca.setup.funcaoRS485[3] = (byte)comboBox_setup_rs485_funcaoLeitoraRS485_4.SelectedIndex;
                
                memoriaPlaca.setup.cfg17.baudrateRS485_1 = (byte)comboBox_setup_rs485_baudrate_rs485_1.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_2 = (byte)comboBox_setup_rs485_baudrate_rs485_2.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_3 = (byte)comboBox_setup_rs485_baudrate_rs485_3.SelectedIndex;
                memoriaPlaca.setup.cfg17.baudrateRS485_4 = (byte)comboBox_setup_rs485_baudrate_rs485_4.SelectedIndex;

                memoriaPlaca.setup.cfg18.retardoDesligarSolenoide = (byte)comboBox_setup_setupCatraca_retardoDesligarSolenoide.SelectedIndex;

                memoriaPlaca.setup.cfg18.desligaRetornoAutoT5S = 0;
                if (checkBox_SETUP_SETUPGERAL_desligaRetornoAutoT5S.Checked) memoriaPlaca.setup.cfg18.desligaRetornoAutoT5S = 1;
                memoriaPlaca.setup.cfg18.facilityWieg66 = 0;
                if (checkBox_SETUP_SETUPGERAL_facilityWieg66.Checked) memoriaPlaca.setup.cfg18.facilityWieg66 = 1;
                memoriaPlaca.setup.cfg18.habilitaAtualizacaoAuto = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaAtualizacaoAuto.Checked) memoriaPlaca.setup.cfg18.habilitaAtualizacaoAuto = 1;
                memoriaPlaca.setup.cfg18.habilitaContagemPassagem = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaContagemPassagem.Checked) memoriaPlaca.setup.cfg18.habilitaContagemPassagem = 1;
                memoriaPlaca.setup.cfg18.habilitaIDMestre = 0;
                if (checkBox_SETUP_SETUPGERAL_habilitaIDMestre.Checked) memoriaPlaca.setup.cfg18.habilitaIDMestre = 1;

                memoriaPlaca.setup.cfg19.sensorPassagemCofre = 0;
                if (checkBox_SETUP_SETUPGERAL_SensorPassagem.Checked) memoriaPlaca.setup.cfg19.sensorPassagemCofre = 1;
                memoriaPlaca.setup.cfg19.sw_watchdog = 0;
                if (checkBox_SETUP_SETUPGERAL_sw_watchdog.Checked) memoriaPlaca.setup.cfg19.sw_watchdog = 1;
				memoriaPlaca.setup.cfg19.sensorGiroInvertido = 0;
                if (ckBoxSensorGiroInvertido.Checked) memoriaPlaca.setup.cfg19.sensorGiroInvertido = 1;
                memoriaPlaca.setup.cfg19.desativaRelePassagem = 0;
                if (checkBoxSetupPorta_desativaRelePassagem.Checked) memoriaPlaca.setup.cfg19.desativaRelePassagem = 1;
                memoriaPlaca.setup.cfg19.duplaValidacaoVisitante = 0;
                if (checkBox_SETUP_SETUPGERAL_duplaValidacaoVisitante.Checked) memoriaPlaca.setup.cfg19.duplaValidacaoVisitante = 1;


                memoriaPlaca.setup.parametro_01_Leitora[0] = Convert.ToByte(textBox_setup_setupRs485_potenciaL1.Text);
                memoriaPlaca.setup.parametro_01_Leitora[1] = Convert.ToByte(textBox_setup_setupRs485_potenciaL2.Text);
                memoriaPlaca.setup.parametro_01_Leitora[2] = Convert.ToByte(textBox_setup_setupRs485_potenciaL3.Text);
                memoriaPlaca.setup.parametro_01_Leitora[3] = Convert.ToByte(textBox_setup_setupRs485_potenciaL4.Text);

                if (memoriaPlaca.setup.parametro_01_Leitora[0] < 140) memoriaPlaca.setup.parametro_01_Leitora[0] = 140;
                if (memoriaPlaca.setup.parametro_01_Leitora[0] > 250) memoriaPlaca.setup.parametro_01_Leitora[0] = 250;
                if (memoriaPlaca.setup.parametro_01_Leitora[1] < 140) memoriaPlaca.setup.parametro_01_Leitora[1] = 140;
                if (memoriaPlaca.setup.parametro_01_Leitora[1] > 250) memoriaPlaca.setup.parametro_01_Leitora[1] = 250;
                if (memoriaPlaca.setup.parametro_01_Leitora[2] < 140) memoriaPlaca.setup.parametro_01_Leitora[2] = 140;
                if (memoriaPlaca.setup.parametro_01_Leitora[2] > 250) memoriaPlaca.setup.parametro_01_Leitora[2] = 250;
                if (memoriaPlaca.setup.parametro_01_Leitora[3] < 140) memoriaPlaca.setup.parametro_01_Leitora[3] = 140;
                if (memoriaPlaca.setup.parametro_01_Leitora[3] > 250) memoriaPlaca.setup.parametro_01_Leitora[3] = 250;

                    try
                    {
                        memoriaPlaca.setup.tLeitoraRS485_10ms[0] = Convert.ToByte(textBox_setup_setupRS485_tout_L1.Text);
                        memoriaPlaca.setup.tLeitoraRS485_10ms[1] = Convert.ToByte(textBox_setup_setupRS485_tout_L2.Text);
                        memoriaPlaca.setup.tLeitoraRS485_10ms[2] = Convert.ToByte(textBox_setup_setupRS485_tout_L3.Text);
                        memoriaPlaca.setup.tLeitoraRS485_10ms[3] = Convert.ToByte(textBox_setup_setupRS485_tout_L4.Text);
                    }
                    catch
                    {
                        memoriaPlaca.setup.tLeitoraRS485_10ms[0] = 0;
                        memoriaPlaca.setup.tLeitoraRS485_10ms[1] = 0;
                        memoriaPlaca.setup.tLeitoraRS485_10ms[2] = 0;
                        memoriaPlaca.setup.tLeitoraRS485_10ms[3] = 0;
                        MessageBox.Show("Valor inválido: Potência UHF");
                    }

                memoriaPlaca.setup.tRele5 = Convert.ToByte(textBox_setup_setupReles_tRele5.Text);

                memoriaPlaca.setup.horarioAtualizaAuto[0] = byte.Parse(textBox_setup_horarioAtualizaAuto0.Text, System.Globalization.NumberStyles.HexNumber);
                memoriaPlaca.setup.horarioAtualizaAuto[1] = byte.Parse(textBox_setup_horarioAtualizaAuto1.Text, System.Globalization.NumberStyles.HexNumber);

                memoriaPlaca.setup.tRele6 = Convert.ToByte(textBox_setup_setupReles_tRele6.Text);

				memoriaPlaca.setup.TempoQuedaCartao = byte.Parse(txtTempoQuedaCartao_SETUP_CATRACA.Text, System.Globalization.NumberStyles.HexNumber);
                
				controladoraLinear.s_setup setupTemp = memoriaPlaca.setup;

                byte[] buf = pControl.gravarSetup(setupTemp);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public void convertStringParaByteArray(ref byte[] buf, String txt, int tamanhoTxt)
        {
            try
            {
                int i;
                for (i = 0; i < buf.Length; i++)
                {
                    if (i < tamanhoTxt )
                    {
                        buf[i] = (byte)txt[i];
                    }
                    else
                    {
                        buf[i] = 0;
                    }
                }
            }
            catch { };
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public void leSetupForm()
        {
            try
            {
                int i;

                cboxBrCan_SETUP_GERAL.SelectedIndex = acertaValorLimite((byte)cboxBrCan_SETUP_GERAL.SelectedIndex, memoriaPlaca.setup.cfg1.brCan, (byte)cboxBrCan_SETUP_GERAL.Items.Count);

                ckBoxControleVagas_SETUP_GERAL.Checked = false;
                if (memoriaPlaca.setup.cfg1.ctrlVagasNivel > 0) ckBoxControleVagas_SETUP_GERAL.Checked = true;

                ckRFHabilitado.Checked = false;
                if (memoriaPlaca.setup.cfg1.RFHabilitado > 0) ckRFHabilitado.Checked = true;

                cboxFuncaoUart2.SelectedIndex = acertaValorLimite((byte)cboxFuncaoUart2.SelectedIndex,memoriaPlaca.setup.cfg2.funcaoUart2 , (byte)cboxFuncaoUart2.Items.Count);
                cboxFuncaoUart3.SelectedIndex = acertaValorLimite((byte)cboxFuncaoUart3.SelectedIndex,memoriaPlaca.setup.cfg2.funcaoUart3 , (byte)cboxFuncaoUart3.Items.Count);
                cboxBrUart1.SelectedIndex = acertaValorLimite((byte)cboxBrUart1.SelectedIndex,memoriaPlaca.setup.cfg3.baudRateUart1 , (byte)cboxBrUart1.Items.Count);
                cboxBrUart2.SelectedIndex = acertaValorLimite((byte)cboxBrUart2.SelectedIndex,memoriaPlaca.setup.cfg3.baudRateUart2 , (byte)cboxBrUart2.Items.Count);
                cboxParidadeUart1.SelectedIndex = acertaValorLimite((byte)cboxParidadeUart1.SelectedIndex,memoriaPlaca.setup.cfg3.paridadeUart1 , (byte)cboxParidadeUart1.Items.Count);
                cboxStopBitsUart1.SelectedIndex = acertaValorLimite((byte)cboxStopBitsUart1.SelectedIndex,memoriaPlaca.setup.cfg3.stopBitsUart1 , (byte)cboxStopBitsUart1.Items.Count);
                cboxBrUart3.SelectedIndex = acertaValorLimite((byte)cboxBrUart3.SelectedIndex,memoriaPlaca.setup.cfg4.baudRateUart3 , (byte)cboxBrUart3.Items.Count);

                cboxTipoDispL1.SelectedIndex = acertaValorLimite((byte)cboxTipoDispL1.SelectedIndex, converteTipoDispositivoParaIndice(memoriaPlaca.setup.cfg4.tipoDispLeitora1), (byte)cboxTipoDispL1.Items.Count);
                cboxTipoDispL2.SelectedIndex = acertaValorLimite((byte)cboxTipoDispL2.SelectedIndex, converteTipoDispositivoParaIndice(memoriaPlaca.setup.cfg4.tipoDispLeitora2), (byte)cboxTipoDispL2.Items.Count);
                cboxTipoDispL3.SelectedIndex = acertaValorLimite((byte)cboxTipoDispL3.SelectedIndex, converteTipoDispositivoParaIndice(memoriaPlaca.setup.cfg5.tipoDispLeitora3), (byte)cboxTipoDispL3.Items.Count);
                cboxTipoDispL4.SelectedIndex = acertaValorLimite((byte)cboxTipoDispL4.SelectedIndex, converteTipoDispositivoParaIndice(memoriaPlaca.setup.cfg5.tipoDispLeitora4), (byte)cboxTipoDispL4.Items.Count);

                cboxSinalizaSaida.SelectedIndex = acertaValorLimite((byte)cboxSinalizaSaida.SelectedIndex, memoriaPlaca.setup.cfg5.sinalizacaoSaidasDigitais, (byte)cboxSinalizaSaida.Items.Count);
                cboxEventoS1.SelectedIndex = acertaValorLimite((byte)cboxEventoS1.SelectedIndex, memoriaPlaca.setup.cfg6.eventoSensor1, (byte)cboxEventoS1.Items.Count);
                cboxEventoS2.SelectedIndex = acertaValorLimite((byte)cboxEventoS2.SelectedIndex, memoriaPlaca.setup.cfg6.eventoSensor2, (byte)cboxEventoS2.Items.Count);
                cboxEventoS3.SelectedIndex = acertaValorLimite((byte)cboxEventoS3.SelectedIndex, memoriaPlaca.setup.cfg6.eventoSensor3, (byte)cboxEventoS3.Items.Count);
                cboxEventoS4.SelectedIndex = acertaValorLimite((byte)cboxEventoS4.SelectedIndex, memoriaPlaca.setup.cfg6.eventoSensor4, (byte)cboxEventoS4.Items.Count);

                ckBoxEnviarTemplateNaoCadastrado.Checked = false;
                if (memoriaPlaca.setup.cfg3.enviarTemplateNaoCadastrado > 0) ckBoxEnviarTemplateNaoCadastrado.Checked = true;

                ckBoxAvisoBateriaBaixa.Checked = false;
                if (memoriaPlaca.setup.cfg7.avisoLowBat > 0) ckBoxAvisoBateriaBaixa.Checked = true;

                ckRFHabilitado.Checked = false;
                if (memoriaPlaca.setup.cfg1.RFHabilitado > 0) ckRFHabilitado.Checked = true;

                cboxTempoMsgDisplay.SelectedIndex = acertaValorLimite((byte)cboxTempoMsgDisplay.SelectedIndex, memoriaPlaca.setup.cfg7.toutMsgDisplay, (byte)cboxTempoMsgDisplay.Items.Count);
                cboxTempoPanico.SelectedIndex = acertaValorLimite((byte)cboxTempoPanico.SelectedIndex, memoriaPlaca.setup.cfg7.toutPanico, (byte)cboxTempoPanico.Items.Count);
                cboxAutonomia.SelectedIndex = acertaValorLimite((byte)cboxAutonomia.SelectedIndex, memoriaPlaca.setup.cfg8.autonomia, (byte)cboxAutonomia.Items.Count);
                cboxCatraca2010.SelectedIndex = acertaValorLimite((byte)cboxCatraca2010.SelectedIndex, memoriaPlaca.setup.cfg8.catraca2010, (byte)cboxCatraca2010.Items.Count);

                // catraca
                cboxGiroCatraca.SelectedIndex = acertaValorLimite((byte)cboxGiroCatraca.SelectedIndex, memoriaPlaca.setup.cfg1.giroCatraca, (byte)(cboxGiroCatraca.Items.Count));
    
                ckBoxInverterCatraca.Checked = false;
                if (memoriaPlaca.setup.cfg2.sentidoCatraca > 0) ckBoxInverterCatraca.Checked = true;


                ckboxAntipassbackDesligadoSaida.Checked = false;
                if (memoriaPlaca.setup.cfg19.antipassbackDesligado_saida > 0) ckboxAntipassbackDesligadoSaida.Checked = true;

                ckboxAntipassbackDesligadoEntrada.Checked = false;
                if (memoriaPlaca.setup.cfg19.antipassbackDesligado_entrada > 0) ckboxAntipassbackDesligadoEntrada.Checked = true;


                ckBoxUmaSolenoide.Checked = false;
                if (memoriaPlaca.setup.cfg2.umaSolenoide > 0) ckBoxUmaSolenoide.Checked = true;

                ckBoxSaidaLivre.Checked = false;
                if (memoriaPlaca.setup.cfg8.saidaLivre > 0) ckBoxSaidaLivre.Checked = true;

                ckBoxCatracaAdaptada.Checked = false;
                if (memoriaPlaca.setup.cfg9.catracaAdaptada > 0) ckBoxCatracaAdaptada.Checked = true;

                comboBox_setup_setupCatraca_tipoCatraca.SelectedIndex = acertaValorLimite((byte)comboBox_setup_setupCatraca_tipoCatraca.SelectedIndex, memoriaPlaca.setup.cfg16.tipoCatraca, (byte)comboBox_setup_setupCatraca_tipoCatraca.Items.Count);

                ckBoxEmHorarioVerao.Checked = false;
                if (memoriaPlaca.setup.cfg8.emHorarioVerao > 0 ) ckBoxEmHorarioVerao.Checked = true;

                ckBoxHorarioVeraoHabilitado.Checked = false;
                if ( memoriaPlaca.setup.cfg8.horarioVeraoHabilitado > 0 ) ckBoxHorarioVeraoHabilitado.Checked = true;

                ckBoxLeitoraManchester.Checked = false;
                if (memoriaPlaca.setup.cfg8.leitoraManchester > 0) ckBoxLeitoraManchester.Checked = true;

                ckBoxValidarBiometria1_1.Checked = false;
                if (memoriaPlaca.setup.cfg9.tratarBiometria_1_1 > 0) ckBoxValidarBiometria1_1.Checked = true;

                checkBoxAntiPassbackDesligado.Checked = false;
                if (memoriaPlaca.setup.cfg9.antipassbackDesligado > 0) checkBoxAntiPassbackDesligado.Checked = true;

                checkBoxSetupPorta_Sensor12_AP.Checked = false;
                if (memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_1e2 > 0) checkBoxSetupPorta_Sensor12_AP.Checked = true;

                checkBoxTruncar32bits.Checked = false;
                if (memoriaPlaca.setup.cfg9.considerarWiegand26bits > 0) checkBoxTruncar32bits.Checked = true;

                checkBoxSetupPorta_AlarmePortaAberta.Checked = false;
                if (memoriaPlaca.setup.cfg9.alarmeEntradaDigitalAberta > 0) checkBoxSetupPorta_AlarmePortaAberta.Checked = true;

                checkBoxSetupPorta_EventoPortaAberta.Checked = false;
                if (memoriaPlaca.setup.cfg9.eventoEntradaDigitalAberta > 0 ) checkBoxSetupPorta_EventoPortaAberta.Checked = true;

                checkBoxSetupPorta_Sensor34_AP.Checked = false;
                if (memoriaPlaca.setup.cfg9.umSensorParaDuasLeitoras_3e4 > 0) checkBoxSetupPorta_Sensor34_AP.Checked = true;

                textBoxSetupPorta_TempoEntreEventosPortaAberta.Text = memoriaPlaca.setup.tempoEventoEntradaDigitalAberta.ToString();
                textBoxSetupPorta_ToutAlarmePortaAberta.Text = memoriaPlaca.setup.toutAlarmeEntradaDigitalAberta.ToString();
                textBoxSetupPorta_tempoLedBuzzer.Text = memoriaPlaca.setup.toutAcionamentoLedBuzzer.ToString();
                                
                txtContadorAtual_SETUP_GERAL.Text = Convert.ToString( memoriaPlaca.setup.cntAtual);

                cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex = acertaValorLimite((byte)cboxEndereçoPlaca_SETUP_GERAL.SelectedIndex, memoriaPlaca.setup.endCan, (byte)cboxEndereçoPlaca_SETUP_GERAL.Items.Count);

                cboxModoOperacao.SelectedIndex = acertaValorLimite((byte)cboxModoOperacao.SelectedIndex, memoriaPlaca.setup.modo, (byte)cboxModoOperacao.Items.Count);

                comboBox_SETUP_setor.SelectedIndex = (int)(memoriaPlaca.setup.setor & 0x07);
                //txtReservado_10.Text = null;

                txtNivel_SETUP_GERAL.Text = Convert.ToString(memoriaPlaca.setup.nivel);
                txtVagas_SETUP_GERAL.Text = Convert.ToString(memoriaPlaca.setup.vagas); 

                txtTempoRele1.Text = Convert.ToString(memoriaPlaca.setup.tRele[0]);
                txtTempoRele2.Text = Convert.ToString(memoriaPlaca.setup.tRele[1]);
                txtTempoRele3.Text = Convert.ToString(memoriaPlaca.setup.tRele[2]);
                txtTempoRele4.Text = Convert.ToString(memoriaPlaca.setup.tRele[3]);


                textBox_SETUP_PORTA_atrasoEntradaLeitora1.Text = Convert.ToString(memoriaPlaca.setup.atrasoEntradaLeitora[0]);
                textBox_SETUP_PORTA_atrasoEntradaLeitora2.Text = Convert.ToString(memoriaPlaca.setup.atrasoEntradaLeitora[1]);
                textBox_SETUP_PORTA_atrasoEntradaLeitora3.Text = Convert.ToString(memoriaPlaca.setup.atrasoEntradaLeitora[2]);
                textBox_SETUP_PORTA_atrasoEntradaLeitora4.Text = Convert.ToString(memoriaPlaca.setup.atrasoEntradaLeitora[3]);


                txtTempoSaida1_SETUP_PORTA.Text = Convert.ToString(memoriaPlaca.setup.tSaida[0]);
                txtTempoSaida2_SETUP_PORTA.Text = Convert.ToString(memoriaPlaca.setup.tSaida[1]);
                txtTempoSaida3_SETUP_PORTA.Text = Convert.ToString(memoriaPlaca.setup.tSaida[2]);
                txtTempoSaida4_SETUP_PORTA.Text = Convert.ToString(memoriaPlaca.setup.tSaida[3]);

                txtTempoPassagemCatraca_SETUP_CATRACA.Text = Convert.ToString(memoriaPlaca.setup.tempoPassagem);
                txtTempoLeituraCofre_SETUP_CATRACA.Text = Convert.ToString(memoriaPlaca.setup.tempoLeituraCofre);

                txtTempoPassagem1.Text = Convert.ToString(memoriaPlaca.setup.toutPassagem[0]);
                txtTempoPassagem2.Text = Convert.ToString(memoriaPlaca.setup.toutPassagem[1]);
                txtTempoPassagem3.Text = Convert.ToString(memoriaPlaca.setup.toutPassagem[2]);
                txtTempoPassagem4.Text = Convert.ToString(memoriaPlaca.setup.toutPassagem[3]);


                txtTempoAnticarona1.Text = Convert.ToString(memoriaPlaca.setup.anticarona[0]);
                txtTempoAnticarona2.Text = Convert.ToString(memoriaPlaca.setup.anticarona[1]);
                txtTempoAnticarona3.Text = Convert.ToString(memoriaPlaca.setup.anticarona[2]);
                txtTempoAnticarona4.Text = Convert.ToString(memoriaPlaca.setup.anticarona[3]);

                txtContadorAtualDisp.Text = Convert.ToString(memoriaPlaca.setup.cntAtualDisp);


                txtIP_SETUP_TCPIP.Text =  
                                          Convert.ToString((memoriaPlaca.setup.ip & 0xFF000000)>>24) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.ip & 0x00FF0000)>>16) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.ip & 0x0000FF00)>>8) + "." + 
                                          Convert.ToString(memoriaPlaca.setup.ip & 0x000000FF);

                txtGateway_SETUP_TCPIP.Text =  
                                          Convert.ToString((memoriaPlaca.setup.gw & 0xFF000000)>>24) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.gw & 0x00FF0000)>>16) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.gw & 0x0000FF00)>>8) + "." + 
                                          Convert.ToString(memoriaPlaca.setup.gw & 0x000000FF);

                txtMask_SETUP_TCPIP.Text =  
                                          Convert.ToString((memoriaPlaca.setup.mask & 0xFF000000)>>24) + "." +
                                          Convert.ToString((memoriaPlaca.setup.mask & 0x00FF0000) >> 16) + "." +
                                          Convert.ToString((memoriaPlaca.setup.mask & 0x0000FF00) >> 8) + "." +
                                          Convert.ToString(memoriaPlaca.setup.mask & 0x000000FF);

                txtIPDestino.Text =
                                          Convert.ToString((memoriaPlaca.setup.ipDestino & 0xFF000000)>>24) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.ipDestino & 0x00FF0000)>>16) + "." + 
                                          Convert.ToString((memoriaPlaca.setup.ipDestino & 0x0000FF00)>>8) + "." + 
                                          Convert.ToString(memoriaPlaca.setup.ipDestino & 0x000000FF);

                ckHabilitaDHCP.Checked = false;
                if (memoriaPlaca.setup.cfge.dhcp > 0) ckHabilitaDHCP.Checked = true;

                cboxServidorDDNS.SelectedIndex = acertaValorLimite((byte)cboxServidorDDNS.SelectedIndex, memoriaPlaca.setup.cfge.DDNS, (byte)cboxServidorDDNS.Items.Count);

                txtTimeoutEthernet1.Text = Convert.ToString( memoriaPlaca.setup.toutEthernet1 );
                txtTimeoutEthernet2.Text = Convert.ToString( memoriaPlaca.setup.toutEthernet2 );

                txtPorta1_SETUP_TCPIP.Text = Convert.ToString( memoriaPlaca.setup.porta1 );
                txtPorta2_SETUP_TCPIP.Text = Convert.ToString( memoriaPlaca.setup.porta2 );
                txtPorta3_SETUP_TCPIP.Text = Convert.ToString( memoriaPlaca.setup.porta3 );
                txtPorta4_SETUP_TCPIP.Text = Convert.ToString( memoriaPlaca.setup.porta4 );

                checkBox_setup_setupTcpIp_habilitarDDNS.Checked = false;
                if( memoriaPlaca.setup.cfge.DDNShabilitado == 1 ) checkBox_setup_setupTcpIp_habilitarDDNS.Checked = true;

                txtDDNSSenha.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_DDNS_SENHA; i++) txtDDNSSenha.Text += Convert.ToChar(memoriaPlaca.setup.DDNSsenha[i]);

                txtDDNSUsuario.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_DDNS_USUARIO; i++) txtDDNSUsuario.Text += Convert.ToChar(memoriaPlaca.setup.DDNSusuario[i]);

                txtDDNSHost.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_DDNS_HOST; i++) txtDDNSHost.Text += Convert.ToChar(memoriaPlaca.setup.DDNSdevice[i]);

                //for (i = 0; i < controladoraLinear.SIZE_OF_DDNS_DEVICE; i++) .Text += Convert.ToChar(memoriaPlaca.setup.DDNSdevice[i]); // não implementado

                txtUsuarioLogin.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_USUARIO_LOGIN; i++) txtUsuarioLogin.Text  += Convert.ToChar(memoriaPlaca.setup.usuarioLogin[i]);

                txtSenhaLogin.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_SENHA_LOGIN; i++) txtSenhaLogin.Text += Convert.ToChar(memoriaPlaca.setup.senhaLogin[i]);

                txtHost_SETUP_TCPIP.Text = null;
                for (i = 0; i < controladoraLinear.SIZE_OF_DNS_HOST; i++) txtHost_SETUP_TCPIP.Text += Convert.ToChar(memoriaPlaca.setup.dnsHost[i]);

                textBox_setup_setupip_portaHttp.Text = Convert.ToString(memoriaPlaca.setup.portaHttp);

                txtToleranciaHorarios.Text = Convert.ToString(memoriaPlaca.setup.toleranciaHorario);

                cboxFuncaoLeitora1.SelectedIndex = acertaValorLimite((byte)cboxFuncaoLeitora1.SelectedIndex, memoriaPlaca.setup.funcaoLeitora[0], (byte)cboxFuncaoLeitora1.Items.Count);
                cboxFuncaoLeitora2.SelectedIndex = acertaValorLimite((byte)cboxFuncaoLeitora1.SelectedIndex, memoriaPlaca.setup.funcaoLeitora[1], (byte)cboxFuncaoLeitora2.Items.Count);
                cboxFuncaoLeitora3.SelectedIndex = acertaValorLimite((byte)cboxFuncaoLeitora1.SelectedIndex, memoriaPlaca.setup.funcaoLeitora[2], (byte)cboxFuncaoLeitora3.Items.Count);
                cboxFuncaoLeitora4.SelectedIndex = acertaValorLimite((byte)cboxFuncaoLeitora1.SelectedIndex, memoriaPlaca.setup.funcaoLeitora[3], (byte)cboxFuncaoLeitora4.Items.Count);

                txtTempoLedBuzzer.Text = Convert.ToString(memoriaPlaca.setup.tempoLedBuzzerLeitoras);

                txtTempoAntipassback.Text = Convert.ToString(memoriaPlaca.setup.toutPassback);

                if ((memoriaPlaca.setup.horarioVerao[0] < 1) || (memoriaPlaca.setup.horarioVerao[0] > 31) || (memoriaPlaca.setup.horarioVerao[1] < 1) || (memoriaPlaca.setup.horarioVerao[1] > 12) ||
                    (memoriaPlaca.setup.horarioVerao[2] < 1) || (memoriaPlaca.setup.horarioVerao[2] > 31) || (memoriaPlaca.setup.horarioVerao[3] < 1) || (memoriaPlaca.setup.horarioVerao[3] > 12))
                {
                    memoriaPlaca.setup.horarioVerao[0] = 21;
                    memoriaPlaca.setup.horarioVerao[1] = 10;
                    memoriaPlaca.setup.horarioVerao[2] = 17;
                    memoriaPlaca.setup.horarioVerao[3] = 2;
                }


                DTPickerHorarioVeraoIni.Value = new DateTime( DateTime.Now.Year, memoriaPlaca.setup.horarioVerao[1], memoriaPlaca.setup.horarioVerao[0]);
                DTPickerHorarioVeraoFim.Value = new DateTime(DateTime.Now.Year, memoriaPlaca.setup.horarioVerao[3], memoriaPlaca.setup.horarioVerao[2]);

                txtPorta5_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta5);
                txtPorta6_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta6);
                txtPorta7_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta7);
                txtPorta8_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta8);
                txtPorta9_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta9);
                txtPorta5.Text = txtPorta5_SETUP_TCPIP.Text;
                txtPorta6.Text = txtPorta6_SETUP_TCPIP.Text;


                txtPorta10_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta10);
                txtPorta11_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta11);
                txtPorta12_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta12);
                txtPorta13_SETUP_TCPIP.Text = Convert.ToString(memoriaPlaca.setup.porta13);


                // SETUP EVENTOS
                checkBoxSetupEventos_filtroEvento0.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento0 > 0) checkBoxSetupEventos_filtroEvento0.Checked = true;
                checkBoxSetupEventos_filtroEvento1.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento1 > 0) checkBoxSetupEventos_filtroEvento1.Checked = true;
                checkBoxSetupEventos_filtroEvento2.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento2 > 0) checkBoxSetupEventos_filtroEvento2.Checked = true;
                checkBoxSetupEventos_filtroEvento3.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento3 > 0) checkBoxSetupEventos_filtroEvento3.Checked = true;
                checkBoxSetupEventos_filtroEvento4.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento4 > 0) checkBoxSetupEventos_filtroEvento4.Checked = true;
                checkBoxSetupEventos_filtroEvento5.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento5 > 0) checkBoxSetupEventos_filtroEvento5.Checked = true;
                checkBoxSetupEventos_filtroEvento6.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento6 > 0) checkBoxSetupEventos_filtroEvento6.Checked = true;
                checkBoxSetupEventos_filtroEvento7.Checked = false;
                if (memoriaPlaca.setup.cfg10.filtrarEvento7 > 0) checkBoxSetupEventos_filtroEvento7.Checked = true;

                checkBoxSetupEventos_filtroEvento8.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento8 > 0) checkBoxSetupEventos_filtroEvento8.Checked = true;
                checkBoxSetupEventos_filtroEvento9.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento9 > 0) checkBoxSetupEventos_filtroEvento9.Checked = true;
                checkBoxSetupEventos_filtroEvento10.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento10 > 0) checkBoxSetupEventos_filtroEvento10.Checked = true;
                checkBoxSetupEventos_filtroEvento11.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento11 > 0) checkBoxSetupEventos_filtroEvento11.Checked = true;
                checkBoxSetupEventos_filtroEvento12.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento12 > 0) checkBoxSetupEventos_filtroEvento12.Checked = true;
                checkBoxSetupEventos_filtroEvento13.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento13 > 0) checkBoxSetupEventos_filtroEvento13.Checked = true;
                checkBoxSetupEventos_filtroEvento14.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento14 > 0) checkBoxSetupEventos_filtroEvento14.Checked = true;
                checkBoxSetupEventos_filtroEvento15.Checked = false;
                if (memoriaPlaca.setup.cfg11.filtrarEvento15 > 0) checkBoxSetupEventos_filtroEvento15.Checked = true;

                checkBoxSetupEventos_filtroEvento16.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento16 > 0) checkBoxSetupEventos_filtroEvento16.Checked = true;
                checkBoxSetupEventos_filtroEvento17.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento17 > 0) checkBoxSetupEventos_filtroEvento17.Checked = true;
                checkBoxSetupEventos_filtroEvento18.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento18 > 0) checkBoxSetupEventos_filtroEvento18.Checked = true;
                checkBoxSetupEventos_filtroEvento19.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento19 > 0) checkBoxSetupEventos_filtroEvento19.Checked = true;
                checkBoxSetupEventos_filtroEvento20.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento20 > 0) checkBoxSetupEventos_filtroEvento20.Checked = true;
                checkBoxSetupEventos_filtroEvento21.Checked = false;
                if (memoriaPlaca.setup.cfg12.filtrarEvento21 > 0) checkBoxSetupEventos_filtroEvento21.Checked = true;

                comboBoxNivelSegurancaBiometria.SelectedIndex = memoriaPlaca.setup.cfg13.nivelSegurancaBiometria;

                // CFG_16
                checkBox_setup_setupGeral_senha13.Checked = false;
                if (memoriaPlaca.setup.cfg16.senha13digitos > 0) checkBox_setup_setupGeral_senha13.Checked = true;

                checkBox_setup_controleEntradaSaidaTurnos.Checked = false;
                if (memoriaPlaca.setup.cfg16.direcaoTurnos > 0) checkBox_setup_controleEntradaSaidaTurnos.Checked = true;



                checkBox_setup_panicoSenha99.Checked = false;
                if (memoriaPlaca.setup.cfg16.panicoSenha99 > 0) checkBox_setup_panicoSenha99.Checked = true;


                checkBox_SETUP_SETUPGERAL_desligarBuzzer.Checked = false;
                if ( memoriaPlaca.setup.cfg16.desligarBuzzer > 0 ) checkBox_SETUP_SETUPGERAL_desligarBuzzer.Checked = true;

                // CFG_13
                checkBoxSetupGeral_ctrlVagasHab.Checked = false;
                if (memoriaPlaca.setup.cfg13.controleVagasRota > 0) checkBoxSetupGeral_ctrlVagasHab.Checked = true;

                checkBoxSetupGeral_desvincularVisitante.Checked = false;
                if (memoriaPlaca.setup.cfg13.desvincularVisitante > 0) checkBoxSetupGeral_desvincularVisitante.Checked = true;

                checkBoxEventoIndexado.Checked = false;
                if( memoriaPlaca.setup.cfg13.enviarEventoIndexado > 0 ) checkBoxEventoIndexado.Checked = true;

                checkBoxValidarBiometriaOnline.Checked = false;
                if (memoriaPlaca.setup.cfg13.validarBiometriaOnline_emRemoto > 0) checkBoxValidarBiometriaOnline.Checked = true;

                checkBox_setup_bio2panicoT5S.Checked = false;
                if (memoriaPlaca.setup.cfg13.bimetria2panico > 0) checkBox_setup_bio2panicoT5S.Checked = true;


                // CFG_14
                comboBox_setup_ctrlPorta_panicoSaidaDigital.SelectedIndex = (memoriaPlaca.setup.cfg14.panicoSaidaDigital & 0x03);

                checkBox_setup_porta_2xCartaoPanico.Checked = false;
                if (memoriaPlaca.setup.cfg14.cartao2xPanico > 0) checkBox_setup_porta_2xCartaoPanico.Checked = true;

                radioButton_setup_setupRS485_tagTipo_EPCTID.Checked = false;
                radioButton_setup_setupRS485_tagTipo_LINEAR.Checked = false;
                radioButton_setup_setupRS485_tagTipo_EPC.Checked = false;
                radioButton_setup_setupRS485_tagTipo_TID.Checked = false;
                if (memoriaPlaca.setup.cfg14.tipoTag_UHF == 0) radioButton_setup_setupRS485_tagTipo_LINEAR.Checked = true;
                if (memoriaPlaca.setup.cfg14.tipoTag_UHF == 1) radioButton_setup_setupRS485_tagTipo_EPCTID.Checked = true;
                if( memoriaPlaca.setup.cfg14.tipoTag_UHF == 2 ) radioButton_setup_setupRS485_tagTipo_EPC.Checked = true;
                if( memoriaPlaca.setup.cfg14.tipoTag_UHF == 3 ) radioButton_setup_setupRS485_tagTipo_TID.Checked = true;

                radioButton_setup_setupRS485_varredura_pooling.Checked = false;
                radioButton_setup_setupRS485_varredura_simultanea.Checked = false;
                if( memoriaPlaca.setup.cfg14.varreSimultanea_UHF == 0 ) radioButton_setup_setupRS485_varredura_pooling.Checked = true; 
                if( memoriaPlaca.setup.cfg14.varreSimultanea_UHF == 1 ) radioButton_setup_setupRS485_varredura_simultanea.Checked = true; 

                radioButton_setup_setupRS485_wiegand_26.Checked = false;
                radioButton_setup_setupRS485_wiegand_34.Checked = false;
                if( memoriaPlaca.setup.cfg14.wiegand34_UHF == 0 ) radioButton_setup_setupRS485_wiegand_26.Checked = true;
                if( memoriaPlaca.setup.cfg14.wiegand34_UHF == 1 ) radioButton_setup_setupRS485_wiegand_34.Checked = true;

                radioButton_setup_setupRS485_freq_fixed.Checked = false;
                radioButton_setup_setupRS485_freq_hopping.Checked = false;
                if( memoriaPlaca.setup.cfg14.wiegand34_UHF == 0 ) radioButton_setup_setupRS485_freq_fixed.Checked = true;
                if( memoriaPlaca.setup.cfg14.wiegand34_UHF == 1 ) radioButton_setup_setupRS485_freq_hopping.Checked = true;

                // CFG_15
                radioButton_setup_setupRS485_bzOff_L1.Checked = false;
                radioButton_setup_setupRS485_bzOn_L1.Checked = false;
                radioButton_setup_setupRS485_bzOff_L2.Checked = false;
                radioButton_setup_setupRS485_bzOn_L2.Checked = false;
                radioButton_setup_setupRS485_bzOff_L3.Checked = false;
                radioButton_setup_setupRS485_bzOn_L3.Checked = false;
                radioButton_setup_setupRS485_bzOff_L4.Checked = false;
                radioButton_setup_setupRS485_bzOn_L4.Checked = false;
                if (memoriaPlaca.setup.cfg15.buzzer_L1_UHF_on == 0) radioButton_setup_setupRS485_bzOff_L1.Checked = true; else radioButton_setup_setupRS485_bzOn_L1.Checked = true;
                if (memoriaPlaca.setup.cfg15.buzzer_L2_UHF_on == 0) radioButton_setup_setupRS485_bzOff_L2.Checked = true; else radioButton_setup_setupRS485_bzOn_L2.Checked = true;
                if (memoriaPlaca.setup.cfg15.buzzer_L3_UHF_on == 0) radioButton_setup_setupRS485_bzOff_L3.Checked = true; else radioButton_setup_setupRS485_bzOn_L3.Checked = true;
                if (memoriaPlaca.setup.cfg15.buzzer_L4_UHF_on == 0) radioButton_setup_setupRS485_bzOff_L4.Checked = true; else radioButton_setup_setupRS485_bzOn_L4.Checked = true;

                //
                comboBox_setup_rs485_funcaoLeitoraRS485_1.SelectedIndex = memoriaPlaca.setup.funcaoRS485[0];
                comboBox_setup_rs485_funcaoLeitoraRS485_2.SelectedIndex = memoriaPlaca.setup.funcaoRS485[1];
                comboBox_setup_rs485_funcaoLeitoraRS485_3.SelectedIndex = memoriaPlaca.setup.funcaoRS485[2];
                comboBox_setup_rs485_funcaoLeitoraRS485_4.SelectedIndex = memoriaPlaca.setup.funcaoRS485[3];

                comboBox_setup_rs485_baudrate_rs485_1.SelectedIndex = memoriaPlaca.setup.cfg17.baudrateRS485_1;
                comboBox_setup_rs485_baudrate_rs485_2.SelectedIndex = memoriaPlaca.setup.cfg17.baudrateRS485_2;
                comboBox_setup_rs485_baudrate_rs485_3.SelectedIndex = memoriaPlaca.setup.cfg17.baudrateRS485_3;
                comboBox_setup_rs485_baudrate_rs485_4.SelectedIndex = memoriaPlaca.setup.cfg17.baudrateRS485_4;


                comboBox_setup_setupCatraca_retardoDesligarSolenoide.SelectedIndex = memoriaPlaca.setup.cfg18.retardoDesligarSolenoide & 0x03;
                if (memoriaPlaca.setup.cfg18.desligaRetornoAutoT5S == 0) checkBox_SETUP_SETUPGERAL_desligaRetornoAutoT5S.Checked = false; else checkBox_SETUP_SETUPGERAL_desligaRetornoAutoT5S.Checked = true;
                if (memoriaPlaca.setup.cfg18.facilityWieg66 == 0) checkBox_SETUP_SETUPGERAL_facilityWieg66.Checked = false; else checkBox_SETUP_SETUPGERAL_facilityWieg66.Checked = true;
                if (memoriaPlaca.setup.cfg18.habilitaContagemPassagem == 0) checkBox_SETUP_SETUPGERAL_habilitaContagemPassagem.Checked = false; else checkBox_SETUP_SETUPGERAL_habilitaContagemPassagem.Checked = true;
                if (memoriaPlaca.setup.cfg18.habilitaAtualizacaoAuto == 0) checkBox_SETUP_SETUPGERAL_habilitaAtualizacaoAuto.Checked = false; else checkBox_SETUP_SETUPGERAL_habilitaAtualizacaoAuto.Checked = true;
                if (memoriaPlaca.setup.cfg18.habilitaIDMestre == 0) checkBox_SETUP_SETUPGERAL_habilitaIDMestre.Checked = false; else checkBox_SETUP_SETUPGERAL_habilitaIDMestre.Checked = true;

                if (memoriaPlaca.setup.cfg19.sensorPassagemCofre == 0) checkBox_SETUP_SETUPGERAL_SensorPassagem.Checked = false; else checkBox_SETUP_SETUPGERAL_SensorPassagem.Checked = true;
                if (memoriaPlaca.setup.cfg19.sw_watchdog == 0) checkBox_SETUP_SETUPGERAL_sw_watchdog.Checked = false; else checkBox_SETUP_SETUPGERAL_sw_watchdog.Checked = true;
				if (memoriaPlaca.setup.cfg19.sensorGiroInvertido == 0) ckBoxSensorGiroInvertido.Checked = false; else ckBoxSensorGiroInvertido.Checked = true;
                if (memoriaPlaca.setup.cfg19.desativaRelePassagem == 0) checkBoxSetupPorta_desativaRelePassagem.Checked = false; else checkBoxSetupPorta_desativaRelePassagem.Checked = true;
                if (memoriaPlaca.setup.cfg19.duplaValidacaoVisitante == 0) checkBox_SETUP_SETUPGERAL_duplaValidacaoVisitante.Checked = false; else checkBox_SETUP_SETUPGERAL_duplaValidacaoVisitante.Checked = true;


                textBox_setup_setupRs485_potenciaL1.Text = memoriaPlaca.setup.parametro_01_Leitora[0].ToString();
                textBox_setup_setupRs485_potenciaL2.Text = memoriaPlaca.setup.parametro_01_Leitora[1].ToString();
                textBox_setup_setupRs485_potenciaL3.Text = memoriaPlaca.setup.parametro_01_Leitora[2].ToString();
                textBox_setup_setupRs485_potenciaL4.Text = memoriaPlaca.setup.parametro_01_Leitora[3].ToString();
                

                textBox_setup_setupRS485_tout_L1.Text = memoriaPlaca.setup.tLeitoraRS485_10ms[0].ToString();
                textBox_setup_setupRS485_tout_L2.Text = memoriaPlaca.setup.tLeitoraRS485_10ms[1].ToString();
                textBox_setup_setupRS485_tout_L3.Text = memoriaPlaca.setup.tLeitoraRS485_10ms[2].ToString();
                textBox_setup_setupRS485_tout_L4.Text = memoriaPlaca.setup.tLeitoraRS485_10ms[3].ToString();

                textBox_setup_setupReles_tRele5.Text = memoriaPlaca.setup.tRele5.ToString();

                textBox_setup_horarioAtualizaAuto0.Text = ByteUnicoToHexString(memoriaPlaca.setup.horarioAtualizaAuto[0]);
                textBox_setup_horarioAtualizaAuto1.Text = ByteUnicoToHexString(memoriaPlaca.setup.horarioAtualizaAuto[1]);
                //textBox_setup_horarioAtualizaAuto1.Text = 
                //textBox_setup_horarioAtualizaAuto1.Text = ;

                textBox_setup_setupReles_tRele6.Text = memoriaPlaca.setup.tRele6.ToString();

				txtTempoQuedaCartao_SETUP_CATRACA.Text = ByteUnicoToHexString( memoriaPlaca.setup.TempoQuedaCartao );
            }
            catch { };
        }


        //--------------------------------------------------------------------
        //
        // 
        //
        //--------------------------------------------------------------------
        public byte acertaValorLimite( byte varRecebe, byte val, byte limiteSup )
        {
            if (val < limiteSup ) return val;
            return (byte)(limiteSup-1);
        }

        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public byte converteIndexParaTipoDispositivo( byte indice )
        {
            switch ( indice )
            {
                case 0: return controladoraLinear.DISP_TX;
                case 1: return controladoraLinear.DISP_TA;
                case 2: return controladoraLinear.DISP_CT;
                case 3: return controladoraLinear.DISP_BM;
                case 4: return controladoraLinear.DISP_TP;
                case 5: return controladoraLinear.DISP_SN;
            }
            // default:
            return controladoraLinear.DISP_CT;
        }


        //--------------------------------------------------------------------
        //
        //
        // 
        //
        //--------------------------------------------------------------------
        public byte converteTipoDispositivoParaIndice( byte tipo )
        {
            switch ( tipo )
            {
                case controladoraLinear.DISP_TX: return 0;
                case controladoraLinear.DISP_TA: return 1;
                case controladoraLinear.DISP_CT: return 2;
                case controladoraLinear.DISP_BM: return 3;
                case controladoraLinear.DISP_TP: return 4;
                case controladoraLinear.DISP_SN: return 5;
            }
            // default:
            return controladoraLinear.DISP_CT;
        }

        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btLerIpServidor_Click(object sender, EventArgs e)
        {
            txtIPDestino.Text = GetLocalIP();
        }

        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void btModoRemoto_Click_1(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.modoRemoto(Convert.ToByte(txtTempoRemoto.Text), cboxModoRemoto.SelectedIndex);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }


        //--------------------------------------------------------------------
        //
        //
        //
        //
        //
        //--------------------------------------------------------------------
        public void enviarMensagem( byte[] buf )
        {
            try
            {
                int tamanhoMsg = buf.Length;
                String msg = BytesToHexString(buf);
                msg = msg.Substring(0, tamanhoMsg * 2);
                for (int j = 2; j < tamanhoMsg * 3; j += 3) msg = msg.Insert(j, " ");
                Invoke((MethodInvoker)delegate
                {
                    textBoxComando.Text = msg;
                    lblTamanhoComando.Text = buf.Length.ToString() + " bytes";
                });
            }
            catch { }


            tempoAnterior = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);

            try
            {
                // TCP client
                if ((tcpClient1 != null) && (tcpClient1.Connected == true) && (tcpClient1Closed == false))
                {
                    tcpClient1.Client.Send(buf);

                    if (form_sniffer.IsHandleCreated == true)
                    {
                        string t = "TX TCP SERVER:" + txt_portaTCP_1.Text + "[" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                        rt_sniffer.Text += t + textBoxComando.Text + "\r\n\r\n";
                        form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                    }
                }
            }
            catch{}

            try
            {
                // UDP
                if (btConectarUDP1.Text == "Desconectar")
                {
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    s.Connect(IPAddress.Parse(txtBox_IP1.Text), Convert.ToUInt16(txtPorta_UDP1.Text));
                    byte[] buf2 = carregaMensagemUDP(buf);
                    s.Send(buf2);
                    s.Close();

                    if (form_sniffer.IsHandleCreated == true)
                    {
                        string t = "TX UDP:" + txtPorta_UDP1.Text + "[" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                        rt_sniffer.Text += t + textBoxComando.Text + "\r\n\r\n";
                        form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                    }
                }
            }
            catch{}

            try
            {
                // serial
                if (btConectarSerial.Text == "Desconectar")
                {
                    serialPort1.Write(buf, 0, buf.Length);

                    if (form_sniffer.IsHandleCreated == true)
                    {
                        string t = "TX SERIAL:" + PortadeConexao + "[" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                        rt_sniffer.Text += t + textBoxComando.Text + "\r\n\r\n";
                        form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                    }
                }
            }
            catch{}
            
            try
            {
                // server
                if(client.Connected) // if the client is connected
                {
                    ServerSend(buf); // uses the Function ClientSend and the msg as txtSend.Text

                    if (form_sniffer.IsHandleCreated == true)
                    {
                        string t = "TX TCP CLIENT:" + txtPorta7_SETUP_TCPIP.Text + "[" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                        rt_sniffer.Text += t + textBoxComando.Text + "\r\n\r\n";
                        form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                    }
                }
            }
            catch{}

            try
            {
                Send(state.workSocket, buf );
            }
            catch
            {
            }
        }


        //--------------------------------------------------------------------
        /*
        IP de origem	IP de origem	hexadecimal	4	0
        Porta de origem	Porta de origem	hexadecimal	2	4
        Endereço CAN de origem	Endereço CAN de origem - obs.: não verificado se origem for "PC" 	hexadecimal	1	6
        counter	Contador de mensagens de 0 a 65535	hexadecimal	2	7
        Flags	(Reservado)	hexadecimal	1	9
        0x0D	Carriage Return	hexadecimal	1	10
        0x0A	Line Feed	hexadecimal	1	11
        "LINEAR-HCS"	Identificação LINEAR-HCS	String(Ascii)	10	12
        (CA):	Identificação da origem da mensagem (CA, CT, TX, BM, TA, TP ou PC)	String(Ascii)	5	22
        "STX"	Cabeçalho da mensagem (inicio)	String(Ascii)	3	27
        tamanho	Tamanho da mensagem	hexadecimal	2	30
        mensagem	Mensagem menor 1442 bytes (1500 - 58 bytes do header UDP)	hexadecimal	até 1442	32
        "ETX"	Rodapé da mensagem (fim)	String(Ascii)	3	
        */
        //--------------------------------------------------------------------
        public UInt16 counter = 0;
        public byte[] carregaMensagemUDP(byte[] buf)
        {
            try
            {
                byte[] bufUDP = new byte[1500];
                int i, j;

                IPAddress ip = IPAddress.Parse(GetLocalIP());
                byte[] ipBytes = ip.GetAddressBytes();
                UInt16 porta = Convert.ToUInt16(txtPorta_UDP1.Text);
                byte endCan = 0;
                
                byte flags = 0;
                String linear = "LINEAR-HCS(PC):";
                //String linear = "LINEAR-HCS(CA):";

                i = 0;

                bufUDP[i++] = (byte)'U';
                bufUDP[i++] = (byte)'D';
                bufUDP[i++] = (byte)'P';
                bufUDP[i++] = ipBytes[0];
                bufUDP[i++] = ipBytes[1];
                bufUDP[i++] = ipBytes[2];
                bufUDP[i++] = ipBytes[3];
                bufUDP[i++] = (byte)(porta >> 8);
                bufUDP[i++] = (byte)porta;
                bufUDP[i++] = endCan;
                bufUDP[i++] = (byte)(counter >> 8);
                bufUDP[i++] = (byte)counter;
                bufUDP[i++] = flags;
                bufUDP[i++] = 0x0D;
                bufUDP[i++] = 0x0A;

                for (j = 0; j < linear.Length; j++) bufUDP[i++] = (byte)linear[j];
                int tamanho = (int)((buf[3]<<8) + buf[4] + 8 );
                for (j = 0; j < tamanho; j++) bufUDP[i++] = buf[j];

                Array.Resize<byte>(ref bufUDP, i );
                counter++;
                return bufUDP;
            }
            catch { return null; };
        }

        
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btConcelarProgressivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.cancelaTimeoutProgressivo();
                enviarMensagem(buf);
            }
            catch { }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerDataHora_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerDataHora();
                enviarMensagem(buf);
            }
            catch { }

        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btAtualizarAntipassbackEspecifico_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = convertIndiceParaTipoDispositivo((byte)cboxTipoDispositivo.SelectedIndex);
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
                UInt16 contadorHCS = Convert.ToUInt16(txtContadorHCS.Text);
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte[] buf = pControl.atualizarAntipassbackEspecifico( tipo, serial, nivel, antipassback );
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }


        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btEditarGrupoLeitoras_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.editarGrupoLeitoras(Convert.ToUInt16(cboxCodigoGrupoLeitoras.SelectedIndex),
                                                            Convert.ToByte(cboxEnderecoGL.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL1.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL2.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL3.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL4.SelectedIndex));
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerGrupoLeitoras_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerGrupoLeitoras(Convert.ToUInt16(cboxCodigoGrupoLeitoras.SelectedIndex),
                                                            Convert.ToByte(cboxEnderecoGL.SelectedIndex));
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerEvento_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerEventos( Convert.ToByte(cboxMarcaEvento.SelectedIndex));
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        //106	Gravar / Editar - Biometria / Identificar Template

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void cboxOperacoesBM1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            /*
            0 OP_ESCRITA(0) = CMD 106/156
            1 OP_EDICAO (1) = CMD 106/156
            2 OP_RESTORE(2) = CMD 106/156
            3 OP_APAGAR(4) = CMD 107
            4 OP_APAGAR_TODOS_OS_TEMPLATES(10) = CMD 107
            5 OP_VERIFICAR_ID_GRAVADO(11) = CMD 107
            6 OP_SOLICITAR_ID_VAGO(12) = CMD 107
            7 OP_IDENTIFICAR_TEMPLATE(13) = CMD 106/156
             */
            switch ( cboxOperacoesBM1.SelectedIndex )
            {
                // 106
                // 156
                case 0:
                case 1:
                case 2:
                case 7:
                    btOperacoesBiometria1.Enabled = true;
                    btOperacoesBiometria3.Enabled = true;
                    btOperacoesBiometria2.Enabled = false;
                break;
                // 107        
                case 3:
                case 4:
                case 5:
                case 6:
                    btOperacoesBiometria1.Enabled = false;
                    btOperacoesBiometria3.Enabled = false;
                    btOperacoesBiometria2.Enabled = true;
                break;

                default:
                    btOperacoesBiometria1.Enabled = false;
                    btOperacoesBiometria3.Enabled = false;
                    btOperacoesBiometria2.Enabled = false;
                break;

            }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        // 106	Gravar / Editar - Biometria / Identificar Template
        // CMD: 00 + 6A + <operacao>+  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs         [40 bytes + template]
        // RSP: 00 + 6A + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public bool naoUtilizar = false;
        private void btOperacoesBiometria1_Click(object sender, EventArgs e)
        {
            try
            {
                if (naoUtilizar == false)
                {
                    naoUtilizar = true;
                    MessageBox.Show("Para novas versões utilizar comando 156");
                }

                String t = rTxtTemplate.Text.Replace(" ", "");

                byte[] template = HexStringToBytes(t);
                UInt16 tamanhoTemplate = (UInt16)template.Length;
                if (tamanhoTemplate == 0)
                {
                    MessageBox.Show("Nenhum biometria! Habilite a opção Enviar template não cadastrado.");
                    return;
                }

                if((cboxModuloBio.SelectedIndex == 1)&&( tamanhoTemplate == 400 )) // ANVIZ
                {
                    Array.Resize<byte>(ref template, 800);
                    for (int i = 0; i < tamanhoTemplate; i++)
                    {
                        template[i + 400] = template[i];
                    }
                    tamanhoTemplate += 400;
                }


                /*
                0 OP_ESCRITA(0) = CMD 106
                1 OP_EDICAO (1) = CMD 106
                2 OP_RESTORE(2) = CMD 106
                3 OP_APAGAR(4) = CMD 107
                4 OP_APAGAR_TODOS_OS_TEMPLATES(10) = CMD 107
                5 OP_VERIFICAR_ID_GRAVADO(11) = CMD 107
                6 OP_SOLICITAR_ID_VAGO(12) = CMD 107
                7 OP_IDENTIFICAR_TEMPLATE(13) = CMD 106
                */
                byte operacao = 0;
                switch (cboxOperacoesBM1.SelectedIndex)
                {
                    case 0: operacao = controladoraLinear.OP_ESCRITA; break;
                    case 1: operacao = controladoraLinear.OP_EDICAO; break;
                    case 2: operacao = controladoraLinear.OP_RESTORE; break;
                    case 7: operacao = controladoraLinear.OP_IDENTIFICAR_TEMPLATE; break;
                    default: return;
                }

                if (tamanhoTemplate == 0)
                {
                    lblErro.Text = "Carregar Template!!";
                    return;
                }


                byte tipo = (byte)cboxTipoDispositivo.SelectedIndex;
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);

                UInt16 contadorHCS = Convert.ToUInt16(txtContadorHCS.Text);
                int codHabilitacao = cboxRota.SelectedIndex; // rota
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte saidaCofre = 0;
                if (ckBoxSaidaCofre.Checked == true) saidaCofre = 1;
                byte visitante = 0;
                if (checkBoxDispositivos_visitante.Checked == true) visitante = 1;
                byte controleVagasId = 0;
                if (checkBoxDispositivos_controleVagasId.Checked) controleVagasId = 1;
                byte duplaValidacao = 0;
                if (checkBoxDispositivos_duplaValidacao.Checked) duplaValidacao = 1;
                byte panico = 0;
                if (checkBoxDispositivos_panico.Checked) panico = 1;

                DateTime dataIni = dateTimePicker1.Value;
                DateTime dataFim = dateTimePicker2.Value;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte creditos = Convert.ToByte(txtCréditos.Text);
                StringBuilder labelUsuario = new StringBuilder(14);



                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(saidaCofre, antipassback, visitante, controleVagasId, duplaValidacao, panico );
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 1); // bateria = ok(0), última saída acionada = 1
                controladoraLinear.s_validade validade = new controladoraLinear.s_validade((byte)dataIni.Day, (byte)dataIni.Month, dataIni.Year, (byte)dataFim.Day, (byte)dataFim.Month, dataFim.Year);

                for (int i = 0; i < 14; i++)
                {
                    if (i < txtIdentificacao.TextLength)
                    {
                        labelUsuario.Append(txtIdentificacao.Text[i]);
                    }
                    else
                    {
                        labelUsuario.Append(' ');
                    }
                }

                tipo = controladoraLinear.DISP_BM;

                /*
                #define TIPO_BIOMETRIA_SEM_BIOMETRIA		0
                #define TIPO_BIOMETRIA_MIAXIS				1
                #define TIPO_BIOMETRIA_VIRDI				2
                #define TIPO_BIOMETRIA_SUPREMA				3
                #define TIPO_BIOMETRIA_NITGEN				4
                #define TIPO_BIOMETRIA_ANVIZ				5
                 * 
                    MIAXIS(1)
                    VIRDI(2)
                    SUPREMA(3)
                    NITGEN(4) // não implementado
                    ANVIZ(5)
                */
                byte tipoBiometria = 0;
                switch (cboxModuloBio.SelectedIndex)
                {
                    case 0: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                    case 1: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                    case 2: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                    case 3: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_NITGEN; break;
                    case 4: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_ANVIZ; break;
                    case 5: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_T5S; break;
                    case 6: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_LN3000; break;
                }

                if (tipoBiometria == 0) return;

                byte[] frameDisp = pControl.dispositivoParaVetor(tipo, serial, contadorHCS, codHabilitacao, fCadastro, fStatus, nivel, creditos, validade, labelUsuario.ToString());

                byte[] buf = pControl.operacoesBiometria1(operacao, (byte)cboxIndiceBM.SelectedIndex, tipoBiometria, frameDisp, tamanhoTemplate, template);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        // 107	Apagar / Apagar Todos / Verificar ID gravado / ID vago - Biometria
        // CMD: 00 + 6B + <operacao> + <indiceLeitora> + <s4> + <s5> + cs         [7 bytes]
        // RSP: 00 + 6B + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        private void btOperacoesBiometria2_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                0 OP_ESCRITA(0) = CMD 106
                1 OP_EDICAO (1) = CMD 106
                2 OP_RESTORE(2) = CMD 106
                3 OP_APAGAR(4) = CMD 107
                4 OP_APAGAR_TODOS_OS_TEMPLATES(10) = CMD 107
                5 OP_VERIFICAR_ID_GRAVADO(11) = CMD 107
                6 OP_SOLICITAR_ID_VAGO(12) = CMD 107
                7 OP_IDENTIFICAR_TEMPLATE(13) = CMD 106
                */
                byte operacao = 0;
                switch (cboxOperacoesBM1.SelectedIndex)
                {
                    case 3: operacao = controladoraLinear.OP_APAGAR; break;
                    case 4: operacao = controladoraLinear.OP_APAGAR_TODOS_OS_TEMPLATES; break;
                    case 5: operacao = controladoraLinear.OP_VERIFICAR_ID_GRAVADO; break;
                    case 6: operacao = controladoraLinear.OP_SOLICITAR_ID_VAGO; break;
                    default: return;
                }

                UInt16 id = 0;
                if ((operacao != controladoraLinear.OP_SOLICITAR_ID_VAGO) && ( operacao != controladoraLinear.OP_APAGAR_TODOS_OS_TEMPLATES))
                {
                    id = UInt16.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
                }
                byte[] buf = pControl.operacoesBiometria2(operacao, (byte)cboxIndiceBM.SelectedIndex, id);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido: serial(id) "); }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        // 108 Ler Biometria
        // 00 + 6C + <operacao> + <indiceLeitora> + <s4> + <s5> + cs
        // 00 + 6C + <retorno> + <operacao> +  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs
        private void btLerBiometria_Click(object sender, EventArgs e)
        {
            try
            {
                UInt16 id = UInt16.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
                byte[] buf = pControl.lerBiometria(controladoraLinear.OP_LEITURA, (byte)cboxIndiceBM.SelectedIndex, id);
                enviarMensagem(buf);
            }
            catch
            {
                MessageBox.Show("Serial inválido");
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void cboxTipoDispositivo_SelectedIndexChanged(object sender, EventArgs e)
        {            
            comboBoxEntradaSaida_Tipo.SelectedIndex = cboxTipoDispositivo.SelectedIndex;
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void ckMostrarEventoAutomatico_CheckedChanged(object sender, EventArgs e)
        {
            if (ckMostrarEventoAutomatico.Checked == true)
            {
                btLerEvento.Enabled = false;
            }
            else
            {
                btLerEvento.Enabled = true;
            }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoParametroSetup_Click(object sender, EventArgs e)
        {
            try
            {
                int i;
                byte operacao = controladoraLinear.OP_LEITURA;
                UInt16 indiceParametro;
                UInt16 tamanhoParametro;
                if (cboxOperacao_parametros.SelectedIndex == 1) operacao = controladoraLinear.OP_ESCRITA;
                indiceParametro = (UInt16)pControl.parametrosSetup[cboxParametro.SelectedIndex];
                tamanhoParametro = (UInt16)pControl.sizeOfparametrosSetup[cboxParametro.SelectedIndex];

                byte[] parametro = new byte[1];
                if (cboxParametro.SelectedIndex == (cboxParametro.Items.Count - 1))
                {                    
                    parametro[0] = 0xCC;
                    byte[] buf2 = pControl.operacaoParametroSetup(controladoraLinear.OP_ESCRITA, 0xAABB, 1, parametro);
                    enviarMensagem(buf2);
                    return;
                }

                Array.Resize<byte>(ref parametro, tamanhoParametro);

                if (operacao == controladoraLinear.OP_ESCRITA)
                {
                    if (tamanhoParametro == 1)
                    {
                        parametro[0] = Convert.ToByte(txtValorParametro.Text);
                    }
                    else if (tamanhoParametro == 2)
                    {
                        UInt16 valParametro = Convert.ToUInt16(txtValorParametro.Text);
                        parametro[0] = (byte)(valParametro >> 8);
                        parametro[1] = (byte)(valParametro);
                    }
                    else if (tamanhoParametro == 4)
                    {
                        UInt32 valParametro = Convert.ToUInt32(txtValorParametro.Text);

                        parametro[0] = (byte)(valParametro >> 24);
                        parametro[1] = (byte)(valParametro >> 16);
                        parametro[2] = (byte)(valParametro >> 8);
                        parametro[3] = (byte)(valParametro);
                    }
                    else if (tamanhoParametro > 4)
                    {
                        for (i = 0; i < tamanhoParametro; i++)
                        {
                            if (i < txtValorParametro.TextLength)
                            {
                                parametro[i] = (byte)txtValorParametro.Text[i];
                            }
                            else
                            {
                                parametro[i] = (byte)' ';
                            }
                        }
                    }
                }

                byte[] buf = pControl.operacaoParametroSetup(operacao, indiceParametro, tamanhoParametro, parametro);
                enviarMensagem(buf);
            }
            catch
            {
                MessageBox.Show("Valor Inválido!!");
            };
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btReboot_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] parametro = new byte[1];
                parametro[0] = 0xCC;
                byte[] buf = pControl.operacaoParametroSetup(controladoraLinear.OP_ESCRITA, 0xAABB, 1, parametro );
                enviarMensagem(buf);
            }
            catch { }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerEditarRotaIndividual_Click(object sender, EventArgs e)
        {
            lerEditarRotaIndividual(121);
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        public void lerEditarRotaIndividual(byte cmd )
        {
            try
            {
                byte operacao = controladoraLinear.OP_ESCRITA;
                if( cboxOperacaoRota2.SelectedIndex == 0 ) operacao = controladoraLinear.OP_LEITURA;
                byte habilitacao = 0;
            
                if (operacao == controladoraLinear.OP_ESCRITA)
                {
                    if (ckLstHabilitacaoRota2.GetItemCheckState(0) == CheckState.Checked) habilitacao |= 0x01;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(1) == CheckState.Checked) habilitacao |= 0x02;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(2) == CheckState.Checked) habilitacao |= 0x04;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(3) == CheckState.Checked) habilitacao |= 0x08;
                }

                byte[] buf;
                if( cmd == 121 )
                {
                    buf = pControl.operacaoRotaIndividual(operacao, (ushort)cboxCodigoRota2.SelectedIndex, (byte)cboxEnderecoRota2.SelectedIndex, habilitacao, txtLabelRota.Text);
                }
                else
                {
                    buf = pControl.operacaoRotaIndividualComSetor(operacao, (ushort)cboxCodigoRota2.SelectedIndex, (byte)cboxEnderecoRota2.SelectedIndex, habilitacao, txtLabelRota.Text, (byte)comboBox_SETUP_setor.SelectedIndex);
                }
                enviarMensagem(buf);
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btEditaDataFeriadoIndividual_Click(object sender, EventArgs e)
        {
            try
            {
                byte dia, mes;
                dia = Convert.ToByte(txtDiaFeriado.Text);
                mes = Convert.ToByte(txtMesFeriado.Text);
                byte[] buf = pControl.editaDataFeriadoIndividual((byte)cboxIndiceFeriado.SelectedIndex, dia, mes);
                enviarMensagem(buf);
            }
            catch { }
        }


        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerEditarMensagensDisplayExterno_Click(object sender, EventArgs e)
        {
            try
            {
                byte operacao = controladoraLinear.OP_ESCRITA;
                if ( cboxOperacaoDisplay.SelectedIndex == 0) operacao = controladoraLinear.OP_LEITURA;

                txtLinha1Display.Text = txtLinha1Display.Text.ToUpper();
                txtLinha2Display.Text = txtLinha2Display.Text.ToUpper();

                byte[] buf = pControl.operacaoMensagensDisplayExterno( operacao, (byte)(cboxEventoDisplay.SelectedIndex), txtLinha1Display.Text, txtLinha2Display.Text );
                enviarMensagem(buf);
            }
            catch { }
        }

        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void ckBoxDataHoraPc_CheckedChanged(object sender, EventArgs e)
        {
            if (ckBoxDataHoraPc.Checked == true)
            {
                txtDataHora_dia.Enabled = false;
                txtDataHora_mes.Enabled = false;
                txtDataHora_ano.Enabled = false;
                txtDataHora_hora.Enabled = false;
                txtDataHora_minuto.Enabled = false;
                txtDataHora_segundo.Enabled = false;
            }
            else
            {
                txtDataHora_dia.Enabled = true;
                txtDataHora_mes.Enabled = true;
                txtDataHora_ano.Enabled = true;
                txtDataHora_hora.Enabled = true;
                txtDataHora_minuto.Enabled = true;
                txtDataHora_segundo.Enabled = true;
            }
        }


        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        // 00 + 7F + <MAC ADDRESS(6 bytes)> + <DNS (16 bytes)> + <IP(4bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <cs>  [33 bytes]
        private void btGravaConfiguracaoNoMacAddress_Click(object sender, EventArgs e)
        {
            try
            {
                int i;

                // mac
                StringBuilder macString = new StringBuilder();
                macString.Clear();
                for (i = 0; i < txtMac.TextLength; i++)
                {
                    if (txtMac.Text[i] != ':') macString.Append(txtMac.Text[i]);
                }
                byte[] mac = hexStringToByteArray(macString.ToString(), (macString.Length) );

                // host
                byte[] host = new byte[16];
                for (i = 0; i < 16; i++)
                {
                    if( i < txtHostMac.TextLength )
                    {
                        host[i] = Convert.ToByte(txtHostMac.Text[i]);
                    }
                    else
                    {
                        host[i] = Convert.ToByte(' ');
                    }
                }

                // ip
                IPAddress ipTemp = IPAddress.Parse(txtIPMac.Text);
                byte[] ip = ipTemp.GetAddressBytes();
                txtIPMac.Text = ipTemp.ToString();

                byte[] buf = pControl.gravaConfiguracaoNoMacAddress(mac, host, ip, Convert.ToUInt16(txtPortaUDPMac.Text), Convert.ToUInt16(txtPortaTCPMac.Text));
                enviarMensagem(buf);
            }
            catch
            {
                MessageBox.Show("Valor Inválido");
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void txtMac_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                int i;
                StringBuilder txtTemp = new StringBuilder();
                txtTemp.Clear();

                txtMac.Text = txtMac.Text.ToUpper();

                if( txtMac.TextLength > 0 )
                {
                    for (i = 0; i < txtMac.TextLength; i++)
                    {
                        if ((( txtMac.Text[i] < '0') || ( txtMac.Text[i] > '9'))&&( txtMac.Text[i] != ':') && (( txtMac.Text[i] < 'A') || ( txtMac.Text[i] > 'F')))
                        {
                            txtMac.Clear();
                            txtMac.Text = "00:00.00.00.00.00";
                            txtMac.SelectionStart = txtMac.TextLength;
                            return;
                        }
                    }

                    // salva o que foi digitado até o momento, sem o último caracter;
                    for (i = 0; i < txtMac.TextLength - 1; i++) txtTemp.Append(txtMac.Text[i]);
                    // fitra valores válidos
                    if (((txtMac.Text[txtMac.TextLength - 1] >= '0') && (txtMac.Text[txtMac.TextLength - 1] <= '9')) || (txtMac.Text[txtMac.TextLength - 1] == ':') || ((txtMac.Text[txtMac.TextLength - 1] >= 'A') && (txtMac.Text[txtMac.TextLength - 1] <= 'F')))
                    {
                        // limita total de caracteres
                        if (txtMac.TextLength < 18)
                        {
                            if ((txtTemp.Length == 2) || (txtTemp.Length == 5) || (txtTemp.Length == 8) || (txtTemp.Length == 11) || (txtTemp.Length == 14) || (txtTemp.Length == 17))
                            {
                                txtTemp.Append(':');
                                // salva último caractere digitado
                                txtTemp.Append(txtMac.Text[txtMac.TextLength - 1]);
                            }
                            else
                            {
                                // salva último caractere digitado
                                txtTemp.Append(txtMac.Text[txtMac.TextLength - 1]);

                                // insere ":" a cada 2 digitos
                                if ((txtMac.TextLength == 3) || (txtMac.TextLength == 5) || (txtMac.TextLength == 8) || (txtMac.TextLength == 11) || (txtMac.TextLength == 14) || (txtMac.TextLength == 17))
                                {
                                    if (txtMac.TextLength < 16) txtTemp.Append(':');
                                }
                            }
                        }
                    }
                }

                txtMac.Text = txtTemp.ToString();
                txtMac.SelectionStart = txtMac.TextLength;
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btMarcaEventosComoLidos_Click(object sender, EventArgs e)
        {
            byte[] buf = pControl.todosEventosLidos();
            enviarMensagem(buf);
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btEditarRotaEGrupoLeitoras_Click(object sender, EventArgs e)
        {
            editarRotaEGrupoLeitoras(128);
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        public void editarRotaEGrupoLeitoras( byte cmd )
        {
            try
            {
                byte operacao = controladoraLinear.OP_ESCRITA;
                if (cboxOperacaoRota2.SelectedIndex == 0) operacao = controladoraLinear.OP_LEITURA;
                byte habilitacao = 0;

                if (operacao == controladoraLinear.OP_ESCRITA)
                {
                    if (ckLstHabilitacaoRota2.GetItemCheckState(0) == CheckState.Checked) habilitacao |= 0x01;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(1) == CheckState.Checked) habilitacao |= 0x02;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(2) == CheckState.Checked) habilitacao |= 0x04;
                    if (ckLstHabilitacaoRota2.GetItemCheckState(3) == CheckState.Checked) habilitacao |= 0x08;
                }

                byte[] buf;
                if (cmd == 128)
                {
                    buf = pControl.operacaoRotaGrupoLeitoras(operacao,
                                                        (UInt16)cboxCodigoRota2.SelectedIndex,
                                                        (byte)cboxEnderecoRota2.SelectedIndex,
                                                        habilitacao,
                                                        Convert.ToByte(cboxJornadaL1_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL2_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL3_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL4_ROTA2.SelectedIndex),
                                                        txtLabelRota.Text);
                }
                else
                {
                    buf = pControl.operacaoRotaGrupoLeitorasComSetor(operacao,
                                                        (UInt16)cboxCodigoRota2.SelectedIndex,
                                                        (byte)cboxEnderecoRota2.SelectedIndex,
                                                        habilitacao,
                                                        Convert.ToByte(cboxJornadaL1_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL2_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL3_ROTA2.SelectedIndex),
                                                        Convert.ToByte(cboxJornadaL4_ROTA2.SelectedIndex),
                                                        txtLabelRota.Text, (byte)comboBox_SETUP_setor.SelectedIndex);
                }
                enviarMensagem(buf);
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btLerDadosSDCARD_Click(object sender, EventArgs e)
        {
            lerArquivoSDCARD();

            if (cboxArquivo.SelectedIndex == 0) cboxDataArquivo.Items.Clear();

        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        public void lerArquivoSDCARD()
        {
            byte arquivoSDCARD = 0;
            byte dia = 0, mes = 0, ano = 0, hora = 0, minuto = 0, segundo = 0;

            switch (cboxArquivo.SelectedIndex)
            {
                case 0: arquivoSDCARD = controladoraLinear.FILE_INDEX; break;
                case 1: arquivoSDCARD = controladoraLinear.FILE_DISP; break;
                case 2: arquivoSDCARD = controladoraLinear.FILE_SETUP; break;
                case 3: arquivoSDCARD = controladoraLinear.FILE_EVENTO; break;
                case 4: arquivoSDCARD = controladoraLinear.FILE_ROTAS; break;
                case 5: arquivoSDCARD = controladoraLinear.FILE_GRUPOS; break;
                case 6: arquivoSDCARD = controladoraLinear.FILE_JORNADA; break;
                case 7: arquivoSDCARD = controladoraLinear.FILE_TURNOS; break;
                case 8: arquivoSDCARD = controladoraLinear.FILE_LABELS; break;
                case 9: arquivoSDCARD = controladoraLinear.FILE_FERIADOS; break;
                case 10: arquivoSDCARD = controladoraLinear.FILE_DISPLAY; break;
                case 11: arquivoSDCARD = controladoraLinear.FILE_CSV; break;
                case 12: arquivoSDCARD = controladoraLinear.FILE_MIAXIS; break;
                case 13: arquivoSDCARD = controladoraLinear.FILE_VIRDI; break;
                case 14: arquivoSDCARD = controladoraLinear.FILE_NITGEN; break;
                case 15: arquivoSDCARD = controladoraLinear.FILE_SUPREMA; break;
                case 16: arquivoSDCARD = controladoraLinear.FILE_ANVIZ; break;
                case 17: arquivoSDCARD = controladoraLinear.FILE_DTURNOS; break;
            }

            try
            {
                if (arquivoSDCARD > controladoraLinear.FILE_INDEX)
                {
                    String s = cboxDataArquivo.SelectedItem.ToString();

                    dia = (byte)((Convert.ToByte(s[0]) - 0x30) * 10 + (Convert.ToByte(s[1]) - 0x30));
                    // "/" 2
                    mes = (byte)((Convert.ToByte(s[3]) - 0x30) * 10 + (Convert.ToByte(s[4]) - 0x30));
                    // "/" 5 
                    ano = (byte)((Convert.ToByte(s[6]) - 0x30) * 10 + (Convert.ToByte(s[7]) - 0x30));
                    // " " 8
                    hora = (byte)((Convert.ToByte(s[9]) - 0x30) * 10 + (Convert.ToByte(s[10]) - 0x30));
                    // ":" 11
                    minuto = (byte)((Convert.ToByte(s[12]) - 0x30) * 10 + (Convert.ToByte(s[13]) - 0x30));
                    // ":" 14
                    segundo = (byte)((Convert.ToByte(s[15]) - 0x30) * 10 + (Convert.ToByte(s[16]) - 0x30));
                }

                if ((cboxDataArquivo.Items.Count > 0) || (cboxArquivo.SelectedIndex == 0))
                {
                    byte[] buf = pControl.lerDadosSDCARD(arquivoSDCARD, dia, mes, ano, hora, minuto, segundo);
                    enviarMensagem(buf);
                }
            }
            catch
            {
                MessageBox.Show("Ler Primeiro \"INDEX.TXT\"!");

            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            fecharConeccoes();
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void Form1_Leave(object sender, EventArgs e)
        {
            fecharConeccoes();
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void fecharConeccoes()
        {
            try
            {
                tcpClient1Closed = true;
                tcpClient1.Close();
            }
            catch { };
            try
            {
                tcpClient5Closed = true;
                tcpClient5.Close();
            }
            catch { };
            try
            {
                tcpClient6Closed = true;
                tcpClient6.Close();
            }
            catch { };
            try
            {
                serialPort1.Close();
            }
            catch { }

            try { Win32.AvzCloseDevice(0); }
            catch { }

            try { state.workSocket.Shutdown(SocketShutdown.Both); }
            catch{}
            
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public TimeSpan tempoInicio, tempoFim;
        public bool cronometrometrando = false;
        private void btOperacaoPacoteDispositivos_Click(object sender, EventArgs e)
        {
            if (cronometrometrando == false)
            {
                cronometrometrando = true;
                tempoInicio = new TimeSpan(DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            }
            
            operacaoPacoteClick(controladoraLinear.FILE_DISP, "DISP.DPT");
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public byte[] converteStringParaPacoteDados(UInt16 indice, String stringRestore )
        {
            char c;
            int i = 0;

            //---------------------------------------------------------------
            // remove CR e LF (0x0D e 0x0A)
            String s = stringRestore.Replace("\n", "").Replace("\r", "");
            stringRestore = s;

            s = null;
            while (true)
            {
                if (stringRestore.Length > (i + indice * 1024))
                {
                    c = stringRestore[i + indice * 1024];
                }
                else
                {
                    c = 'F';
                }
                s += c;
                if (s.Length >= 1024) break;
                i++;
            }
            return hexStringToByteArray(s, s.Length );
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void ckboxTudo_CheckedChanged(object sender, EventArgs e)
        {
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoPacoteJornada_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_JORNADA, "JORNA.TXT");
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoPacoteGrupoLeitoras_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_GRUPOS, "GRUPO.TXT");
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoPacoteHabilitacao_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_ROTAS, "ROTAS.TXT");
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoPacoteTurnos_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_TURNOS, "TURNO.TXT");
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btOperacaoPacoteLabelsRota_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_LABELS, "LABEL.TXT");
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        public void operacaoPacoteClick(byte indiceArquivo, string fileName)
        {
            try
            {
                UInt16 indice = 0;
                byte operacao = controladoraLinear.OP_LEITURA;
                if (cboxOperacaoPacote.SelectedIndex == 1) operacao = controladoraLinear.OP_ESCRITA;


                if (operacao == controladoraLinear.OP_ESCRITA) // GRAVAR PACOTE
                {
                    openFileDialog1.FileName = fileName;
                    if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                        txtPrimeiraLinhaPacote.Text = sr.ReadLine();
                        if (txtPrimeiraLinhaPacote.Text == null)
                        {
                            MessageBox.Show("Arquivo invalido!");
                            return;
                        }

                        arquivoRestore = sr.ReadToEnd();
                        sr.Close();
                        richTextPacote.Text = arquivoRestore;
                        //---------------------------------------------------------------
                        byte[] bufTemp = converteStringParaPacoteDados(indice, arquivoRestore);

                        enviaCmdOperacaoPacote(indiceArquivo, operacao, indice, bufTemp);
                    }
                }
                else // OP_LEITURA
                {
                    byte[] bufTemp = new byte[512];
                    enviaCmdOperacaoPacote(indiceArquivo, operacao, indice, bufTemp);
                }
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        public void operacaoPacoteComSetorClick(byte indiceArquivo, string fileName, byte setor )
        {
            try
            {
                UInt16 indice = (ushort)comboBox_nPacotes.SelectedIndex;
                byte operacao = controladoraLinear.OP_LEITURA;
                if (cboxOperacaoPacote.SelectedIndex == 1) operacao = controladoraLinear.OP_ESCRITA;


                if (operacao == controladoraLinear.OP_ESCRITA) // GRAVAR PACOTE
                {
                    openFileDialog1.FileName = fileName;
                    if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                        txtPrimeiraLinhaPacote.Text = sr.ReadLine();
                        if (txtPrimeiraLinhaPacote.Text == null)
                        {
                            MessageBox.Show("Arquivo invalido!");
                            return;
                        }

                        arquivoRestore = sr.ReadToEnd();
                        sr.Close();
                        richTextPacote.Text = arquivoRestore;
                        //---------------------------------------------------------------
                        byte[] bufTemp = converteStringParaPacoteDados(indice, arquivoRestore);

                        enviaCmdOperacaoPacoteComSetor(indiceArquivo, operacao, indice, bufTemp, setor );
                    }
                }
                else // OP_LEITURA
                {
                    byte[] bufTemp = new byte[512];
                    enviaCmdOperacaoPacoteComSetor(indiceArquivo, operacao, indice, bufTemp, setor );
                }
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btTCP_Porta5_Click(object sender, EventArgs e)
        {
            Conectar_TCP5();
        }
        //--------------------------------------------------------------------
        //
        //
        //
        //--------------------------------------------------------------------
        private void btTCP_Porta6_Click(object sender, EventArgs e)
        {
            Conectar_TCP6();

        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btEnviarCmdPorta5_Click(object sender, EventArgs e)
        {
            try
            {
                int i;
                StringBuilder sb = new StringBuilder();

                for (i = 0; i < txtCmdPorta5.TextLength; i++) if (txtCmdPorta5.Text[i] != ' ') sb.Append(txtCmdPorta5.Text[i]);

                byte[] buf = hexStringToByteArray(sb.ToString(), sb.Length);
                // TCP client
                if ((tcpClient5.Connected == true) && (tcpClient5Closed == false)) tcpClient5.Client.Send(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btDemoCadastrarDigital_Click(object sender, EventArgs e)
        {
            modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 0: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_CADASTRAR_DIGITAL;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);

        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void trataEstadoBiometria(int cmd, ref byte estado, byte modelo )
        {
            byte[] buf = new byte[1500];
            byte[] bufRsp = new byte[1500];

            try
            {
                if( radioButton_conversorSerialTcp_portaTcp5.Checked )
                    if (tcpClient5.Connected == false) return;
                else
                    if (tcpClient6.Connected == false) return;

            }
            catch
            {
                return;
            }

 
            try
            {
                switch (cmd)
                {
                    case CMD_CADASTRAR_DIGITAL:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // DETECT FINGER
                                        buf= anviCmd.CmdDetectFinger();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Detect Finger";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_FP_REGISTER_START(Convert.ToUInt16(txtDemoID.Text), 0, 0);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Gravar digital";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_ES(Convert.ToUInt16(txtDemoID.Text), BiometriaSuprema.ENROLL_NONE);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Gravar digital";
                                        break;

                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if( ((radioButton_conversorSerialTcp_portaTcp5.Checked)&&(txtRspCmdPorta5.TextLength > 10))||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked)&&(txtRspCmdPorta6.TextLength > 10)) )
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGetImage();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Finger Detected ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "No Finger Detected";
                                            }
                                        }
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                buf= virdiCmd.VIRDI_CMD_FP_REGISTER_END(Convert.ToUInt16(txtDemoID.Text));
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                toutEstadoBiometria = 500;
                                                estado++;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Ok.Gravar digital novamente";
                                            }
                                            else if (bufRsp[16] == BiometriaVirdi.M2ERROR_ALREADY_REGISTERED_USER)
                                            {
                                                lblResposta.Text = "ID já cadastrado";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Erro código: " + Convert.ToString(bufRsp[17]);
                                            }
                                        }
                                        break;


                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                buf= supremaCmd.SUPREMA_CMD_ES(Convert.ToUInt16(txtDemoID.Text), BiometriaSuprema.ENROLL_CONTINUE );
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                toutEstadoBiometria = 500;
                                                estado++;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Gravar digital novamente";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 2:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGenTemplet(BiometriaAnviz.CHAR_BUFFER_A);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                lblResposta.Text = "Get Image ok";
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha Get Image";
                                            }
                                        }
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)) )
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 3:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdDetectFinger();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                toutEstadoBiometria = 500;
                                                estado++;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "GenTemplage ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha GenTemplate";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 4:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGetImage();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Finger Detected ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "No Finger Detected";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 5:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGenTemplet(BiometriaAnviz.CHAR_BUFFER_B);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                lblResposta.Text = "Get Image ok";
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha Get Image";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 6:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))                                            
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdMatchTwoTemplet();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "GenTemplage ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha GenTemplate";
                                            }
                                        }
                                        break;
                                }
                                break;



                            case 7:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdMergeTwoTemplet();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Match ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha Match";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 8:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdStoreTemplet(BiometriaAnviz.MODEL_BUFFER, Convert.ToUInt16(txtDemoID.Text));
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 500;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Merge ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Merge falha";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 9:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;
                                }
                                break;

                        }
                        break;

                    case CMD_BUSCAR_DIGITAL:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // DETECT FINGER
                                        buf= anviCmd.CmdDetectFinger();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Coloque o dedo no sensor...";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_FP_IDENTIFY();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Coloque o dedo no sensor...";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_IS();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Coloque o dedo no sensor";
                                        break;
                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGetImage();
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 100;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Finger Detected ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "No Finger Detected";
                                            }
                                        }
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                txtDemoID.Text = Convert.ToString((UInt16)((bufRsp[5] << 8) + bufRsp[4]));
                                                lblResposta.Text = "Digital encontrada";
                                            }
                                            else if (bufRsp[16] == BiometriaVirdi.M2ERROR_IDENTIFY_FAILED)
                                            {
                                                lblResposta.Text = "Digital não encontrada";
                                            }
                                            else if (bufRsp[16] == BiometriaVirdi.M2ERROR_DB_NO_DATA)
                                            {
                                                lblResposta.Text = "Nenhuma digital na memoria";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                txtDemoID.Text = Convert.ToString((bufRsp[3] << 8) + bufRsp[2]);
                                                lblResposta.Text = "Digital encontrada";
                                            }
                                            else if (bufRsp[10] == BiometriaSuprema.SFM_NOT_FOUND)
                                            {
                                                lblResposta.Text = "Digital não encontrada";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        break;

                                }
                                break;

                            case 2:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdGenTemplet(BiometriaAnviz.CHAR_BUFFER_A);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                estado++;
                                                toutEstadoBiometria = 100;
                                                lblResposta.Text = "Get Image ok";
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha Get Image";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 3:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdSearch(BiometriaAnviz.CHAR_BUFFER_A, 0, 1770);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                toutEstadoBiometria = 500;
                                                estado++;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Procurando...";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha GenTemplate";
                                            }
                                        }
                                        break;
                                }
                                break;

                            case 4:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                txtDemoID.Text = Convert.ToString((bufRsp[9] << 8) + bufRsp[10]);
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else if ((bufRsp[9] == 0xFF) && (bufRsp[10] == 0xFF))
                                            {
                                                lblResposta.Text = "Não encontrado";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;
                                }
                                break;

                        }
                        break;

                    case CMD_APAGAR_DIGITAL:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // DETECT FINGER
                                        buf= anviCmd.CmdDeletOneTemplet(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando...";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_DB_DELETE_REC(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando..";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_DT(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando..";
                                        break;
                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                lblResposta.Text = "Digital apagada";
                                            }
                                            else if (bufRsp[16] == BiometriaVirdi.M2ERROR_DB_WRONG_USERID)
                                            {
                                                lblResposta.Text = "Digital não encontrada";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                lblResposta.Text = "Digital apagada";
                                            }
                                            else if (bufRsp[10] == BiometriaSuprema.SFM_NOT_FOUND)
                                            {
                                                lblResposta.Text = "Digital não encontrada";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        break;
                                }
                                break;
                        }
                        break;

                    case CMD_APAGAR_TODOS:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // DETECT FINGER
                                        buf= anviCmd.CmdEraseAllTemplet();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando...";
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_DB_DELETE_ALL();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando..";
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_DA();
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando..";
                                    break;
                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else 
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                     break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                     if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                         ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                     {
                                             if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                             if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                             {
                                                 lblResposta.Text = "Sucesso";
                                             }
                                             else
                                             {
                                                 lblResposta.Text = "Falha";
                                             }
                                         }
                                         estado = 0;
                                         toutEstadoBiometria = 0;
                                     break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                     if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                         ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                     {
                                             if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                             if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                             {
                                                 lblResposta.Text = "Sucesso";
                                             }
                                             else
                                             {
                                                 lblResposta.Text = "Falha";
                                             }
                                         }
                                     break;
                                }
                                break;
                        }
                        break;


                    case CMD_LER_DIGITAL:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // DETECT FINGER
                                        buf= anviCmd.CmdLoadTemplet(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Load...";
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_DB_GET_REC(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Lendo...";
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_RT(Convert.ToUInt16(txtDemoID.Text));
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Apagando..";
                                    break;
                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                buf= anviCmd.CmdUpTemplet(BiometriaAnviz.MODEL_BUFFER);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                                toutEstadoBiometria = 500;
                                                estado++;
                                                if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                                lblResposta.Text = "Load ok";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha Load";
                                            }
                                        }
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                    if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                        ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                    {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                lblResposta.Text = "Sucesso";

                                                int i;
                                                buf= new byte[bufRsp.Length-20];
                                                for (i = 0; i < bufRsp.Length-20; i++) buf[i] = bufRsp[i + 20];
                                                Array.Resize<byte>(ref buf, 859);
                                                String msg = BytesToHexString(buf);
                                                rtxtBoxTemplateDemo.Clear();
                                                StringBuilder sb = new StringBuilder();                                                
                                                int n = msg.Length;
                                                for (i = 0; i < n; i += 2)
                                                {
                                                    sb.Append(msg[i]);
                                                    sb.Append(msg[i + 1]);
                                                    sb.Append(' ');
                                                }
                                                rtxtBoxTemplateDemo.Text = sb.ToString();

                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                    if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                        ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                    {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                lblResposta.Text = "Sucesso";

                                                int i, tamanho = ((bufRsp[7] << 8) + bufRsp[6]);
                                                buf= new byte[bufRsp.Length-13];
                                                for (i = 0; i < bufRsp.Length - 13; i++) buf[i] = bufRsp[i + 13];
                                                Array.Resize<byte>(ref buf, tamanho );

                                                String msg = BytesToHexString(buf);
                                                rtxtBoxTemplateDemo.Clear();
                                                StringBuilder sb = new StringBuilder();
                                                int n = msg.Length;
                                                for (i = 0; i < n; i += 2)
                                                {
                                                    sb.Append(msg[i]);
                                                    sb.Append(msg[i + 1]);
                                                    sb.Append(' ');
                                                }
                                                rtxtBoxTemplateDemo.Text = sb.ToString();
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                    break;
                                }
                                break;


                            case 2:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_MIAXIS:
                                        // AGUARDA RESPOSTA
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 10)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 10)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[8] == BiometriaAnviz.MX_OK)
                                            {
                                                int i;
                                                buf= new byte[bufRsp.Length - 12];
                                                for (i = 0; i < bufRsp.Length - 12; i++) buf[i] = bufRsp[i + 12];
                                                if (bufRsp[12 + 7] == 0xA9)
                                                {
                                                    Array.Resize<byte>(ref buf, 362);
                                                }
                                                else
                                                {
                                                    Array.Resize<byte>(ref buf, 281);
                                                }

                                                String msg = BytesToHexString(buf);
                                                rtxtBoxTemplateDemo.Clear();
                                                StringBuilder sb = new StringBuilder();
                                                int n = msg.Length;
                                                for (i = 0; i < n; i += 2)
                                                {
                                                    sb.Append(msg[i]);
                                                    sb.Append(msg[i + 1]);
                                                    sb.Append(' ');
                                                }
                                                rtxtBoxTemplateDemo.Text = sb.ToString(); 
                                                
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Load";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                        break;
                                }
                                break;
                        }
                    break;

                    case CMD_AUTO_SENSE_ON:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_AUTO_ONOFF(1, 1);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Ligar auto sensing...";
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_SW( 0x31, BiometriaSuprema.PARAMETER_FREE_SCAN );
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Ligar auto sensing...";
                                    break;
                                }
                            break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                    break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                    if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                        ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                    {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                    break;
                                }
                            break;
                        }
                    break;

                    case CMD_AUTO_SENSE_OFF:
                        switch (estado)
                        {
                            case 0:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        buf= virdiCmd.VIRDI_CMD_AUTO_ONOFF(0, 1);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Desligar auto sensing...";
                                        break;

                                    case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                        buf= supremaCmd.SUPREMA_CMD_SW(0x30, BiometriaSuprema.PARAMETER_FREE_SCAN);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtCmdPorta5.Text = BytesToHexString(buf); else txtCmdPorta6.Text = BytesToHexString(buf);
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) tcpClient5.Client.Send(buf); else tcpClient6.Client.Send(buf);
                                        toutEstadoBiometria = 500;
                                        estado++;
                                        if (radioButton_conversorSerialTcp_portaTcp5.Checked) txtRspCmdPorta5.Clear(); else txtRspCmdPorta6.Clear();
                                        lblResposta.Text = "Desligar auto sensing...";
                                        break;

                                }
                                break;

                            case 1:
                                switch (modelo)
                                {
                                    case controladoraLinear.TIPO_BIOMETRIA_VIRDI:
                                        if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 15)) ||
                                            ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 15)))
                                        {
                                            if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                            if (bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                                            {
                                                lblResposta.Text = "Sucesso";
                                            }
                                            else
                                            {
                                                lblResposta.Text = "Falha";
                                            }
                                        }
                                        estado = 0;
                                        toutEstadoBiometria = 0;
                                   break;

                                   case controladoraLinear.TIPO_BIOMETRIA_SUPREMA:
                                   if (((radioButton_conversorSerialTcp_portaTcp5.Checked) && (txtRspCmdPorta5.TextLength > 12)) ||
                                       ((radioButton_conversorSerialTcp_portaTcp6.Checked) && (txtRspCmdPorta6.TextLength > 12)))
                                   {
                                           if (radioButton_conversorSerialTcp_portaTcp5.Checked) bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength); else bufRsp = hexStringToByteArray(txtRspCmdPorta6.Text, txtRspCmdPorta6.TextLength);
                                           if ((bufRsp[10] == BiometriaSuprema.SFM_SUCCESS) || (bufRsp[10] == BiometriaSuprema.SFM_SCAN_SUCCESS))
                                           {
                                               lblResposta.Text = "Sucesso";
                                           }
                                           else
                                           {
                                               lblResposta.Text = "Falha";
                                           }
                                       }
                                       estado = 0;
                                       toutEstadoBiometria = 0;
                                   break;
                                }
                                break;
                        }
                    break;
                }
            }
            catch { MessageBox.Show("Erro ou valor Inválido"); }
        }        
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btBuscarDigital_Click(object sender, EventArgs e)
        {
            modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 0: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_BUSCAR_DIGITAL;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btDemoApagarDigital_Click(object sender, EventArgs e)
        {
            modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 0: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_APAGAR_DIGITAL;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btDemoLerTemplate_Click(object sender, EventArgs e)
        {
            modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 0: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_LER_DIGITAL;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btDemoApagarTodos_Click(object sender, EventArgs e)
        {
            modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 0: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_APAGAR_TODOS;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btEnviarMensagemDisplayExterno_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.enviarMensagemDisplayExterno(Convert.ToByte(txtMensagemDisplayExterno.TextLength), txtMensagemDisplayExterno.Text);
                enviarMensagem(buf);
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btDesligaAutoSense_Click(object sender, EventArgs e)
        {
            modeloBiometria = 0;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_AUTO_SENSE_OFF;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btLigaAutoSense_Click(object sender, EventArgs e)
        {
            modeloBiometria = 0;
            switch (cboxModeloBiometria.SelectedIndex)
            {
                case 1: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: modeloBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                default: return;
            }
            estadoBiometria = 0;
            ultimoCmdBiometria = CMD_AUTO_SENSE_ON;
            trataEstadoBiometria(ultimoCmdBiometria, ref estadoBiometria, modeloBiometria);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void cboxModeloBiometria_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboxModeloBiometria.SelectedIndex != 0) // MIAXIS
            {
                if (tcpClient5Closed == false)
                {
                    btDesligaAutoSense.Enabled = true;
                    btLigaAutoSense.Enabled = true;
                }
            }
            else
            {
                btDesligaAutoSense.Enabled = false;
                btLigaAutoSense.Enabled = false;
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void txtRspCmdPorta5_TextChanged(object sender, EventArgs e)
        {
            if((txtRspCmdPorta5.TextLength > 15)&&(cboxModeloBiometria.SelectedIndex == 1 )) // VIRDI
            {
                byte[] bufRsp = hexStringToByteArray(txtRspCmdPorta5.Text, txtRspCmdPorta5.TextLength);
                if((bufRsp[0] == 0x02)&&(bufRsp[1] == BiometriaVirdi.CMD_FP_IDENTIFY))
                {
                    if(bufRsp[16] == BiometriaVirdi.M2ERROR_NONE)
                    {
                        txtDemoID.Text = Convert.ToString((UInt16)((bufRsp[5] << 8) + bufRsp[4]));
                        lblResposta.Text = "Digital encontrada (auto sensing)";
                    }
                    else
                    {
                        lblResposta.Text = "Digital não encontrada (auto sensing)";
                    }
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btGravarMsgPadraoDisplayExterno_Click(object sender, EventArgs e)
        {
            try
            {
                byte i,j;
                string[] s = new string[] {   // 0123456789012345   0123456789012345  
                                                "     ACESSO     ","   AUTORIZADO   ",  // 0 
                                                "NAO CADASTRADO  ","   NO SISTEMA   ",  // 1 
                                                "DEVOLVER CARTAO ","NO COFRE COLETOR",  // 2 
                                                "  DUPLO ACESSO	 ","    PROIBIDO    ",  // 3 
                                                "      SEM	     ","    CREDITOS    ",  // 4 
                                                "FORA DO PERIODO ","  DE VALIDADE   ",  // 5 
                                                " FAVOR AGUARDAR ","TEMPO DE ACESSO ",  // 6 
                                                "      ROTA	     "," INVALIDA       ",  // 7 
                                                " NAO HABILITADO ","  PARA FERIADO  ",  // 8 
                                                " DIA OU HORARIO ","   INVALIDO     ",  // 9 
                                                "   SEM VAGAS    ","   NO MOMENTO   ",  // 10
                                                "   AGUARDANDO   ","DUPLA VALIDACAO "
                                                };

                j = 0;
                for (i = 0; i <controladoraLinear.N_LINES_MSG_DISPLAY*2; i+=2)
                {
                    byte[] buf = pControl.operacaoMensagensDisplayExterno( controladoraLinear.OP_ESCRITA, j, s[i], s[i+1]);
                    enviarMensagem(buf);
                    Thread.Sleep(100);
                    j++;
                }
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btApagarUltimoEvento_Click(object sender, EventArgs e)
        {
            byte[] buf = pControl.apagarUltimoEvento();
            enviarMensagem(buf);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btGravarNCopiasBm_Click(object sender, EventArgs e)
        {
            if (btGravarNCopiasBm.Text == "Gravar N Copias")
            {
                UInt16 n = Convert.ToUInt16(txtGravarNCopias.Text);

                byte tipo = 0;
                UInt64 serial = 0;
                int contadorHCS = 0, codHabilitacao = 0, tamanhoTemplate = 0;
                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(0, 0, 0, 0, 0, 0);
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 0);
                controladoraLinear.s_validade dataValidade = new controladoraLinear.s_validade(1, 1, 0, 1, 1, 0);
                byte nivel = 0, creditos = 0, tipoBiometria = 0, operacao = controladoraLinear.OP_RESTORE;
                string userLabel = null;
                byte[] template = new byte[800];

                carregaFrameDisp_Form(ref tipo, ref serial, ref contadorHCS, ref codHabilitacao, ref fCadastro, ref fStatus, ref nivel, ref creditos, ref dataValidade, ref userLabel, ref operacao, ref tipoBiometria, ref tamanhoTemplate, ref template);

                executarNvezes = n;
                serial = (UInt64)executarNvezes;
                frameDisp = pControl.dispositivoParaVetor(tipo, serial, contadorHCS, codHabilitacao, fCadastro, fStatus, nivel, creditos, dataValidade, userLabel);
                byte[] buf = pControl.operacoesBiometria1(operacao, (byte)cboxIndiceBM.SelectedIndex, (byte)tipoBiometria, frameDisp, (UInt16)tamanhoTemplate, template);
                enviarMensagem(buf);
                btGravarNCopiasBm.Text = "Cancelar envio...";
            }
            else
            {
                btGravarNCopiasBm.Text = "Gravar N Copias";
                executarNvezes = 0;
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void carregaFrameDisp_Form(ref byte tipo, ref UInt64 serial, ref int contadorHCS, ref int codHabilitacao, ref controladoraLinear.flagsCadastro fCadastro,
                                          ref controladoraLinear.flagsStatus fStatus, ref byte nivel, ref byte creditos, ref controladoraLinear.s_validade dataValidade,
                                          ref string userLabel, ref byte operacao, ref byte tipoBiometria, ref int tamanhoTemplate, ref byte[] template )
        {
            String t = rTxtTemplate.Text.Replace(" ", ""); 
            template = HexStringToBytes(t);
            tamanhoTemplate = (UInt16)template.Length;
            if (tamanhoTemplate == 0)
            {
                MessageBox.Show("Nenhum biometria! Habilite a opção Enviar template não cadastrado.");
                return;
            }


            /*
                0 OP_ESCRITA(0)
                1 OP_EDICAO (1)
                2 OP_RESTORE (2)
                3 OP_APAGAR(4)
                4 OP_APAGAR_TODOS_OS_TEMPLATES(10)
                5 OP_VERIFICAR_ID_GRAVADO(11)
                6 OP_SOLICITAR_ID_VAGO(12)
                7 OP_IDENTIFICAR_TEMPLATE(13)
                */

            operacao = 0;
            switch (cboxOperacoesBM1.SelectedIndex)
            {
                case 0: operacao = controladoraLinear.OP_ESCRITA; break;
                case 2: operacao = controladoraLinear.OP_RESTORE; break;
                case 1: operacao = controladoraLinear.OP_EDICAO; break;
                case 7: operacao = controladoraLinear.OP_IDENTIFICAR_TEMPLATE; break;
                default: return;
            }

            if (tamanhoTemplate == 0)
            {
                lblErro.Text = "Carregar Template!!";
                return;
            }

            tipo = (byte)cboxTipoDispositivo.SelectedIndex;
            serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
            contadorHCS = Convert.ToUInt16(txtContadorHCS.Text);
            codHabilitacao = cboxRota.SelectedIndex; // rota

            byte antipassback = (byte)cboxAntipassback.SelectedIndex;
            byte saidaCofre = 0;
            if(ckBoxSaidaCofre.Checked == true) saidaCofre = 1;
            byte visitante = 0;
            if (checkBoxDispositivos_visitante.Checked == true) visitante = 1;
            byte controleVagasId = 0;
            if (checkBoxDispositivos_controleVagasId.Checked) controleVagasId = 1;
            byte duplaValidacao = 0;
            if (checkBoxDispositivos_duplaValidacao.Checked) duplaValidacao = 1;
            byte panico = 0;
            if (checkBoxDispositivos_panico.Checked) panico = 1;


            DateTime dataIni = dateTimePicker1.Value;
            DateTime dataFim = dateTimePicker2.Value;
            nivel = Convert.ToByte(txtNivel.Text);
            creditos = Convert.ToByte(txtCréditos.Text);
            StringBuilder labelUsuario = new StringBuilder(14);

            fCadastro = new controladoraLinear.flagsCadastro(saidaCofre, antipassback, visitante, controleVagasId, duplaValidacao, panico );
            fStatus = new controladoraLinear.flagsStatus(0, 1); // bateria = ok(0), última saída acionada = 1
            dataValidade = new controladoraLinear.s_validade((byte)dataIni.Day, (byte)dataIni.Month, dataIni.Year, (byte)dataFim.Day, (byte)dataFim.Month, dataFim.Year);

            for (int i = 0; i < 14; i++)
            {
                if (i < txtIdentificacao.TextLength)
                {
                    labelUsuario.Append(txtIdentificacao.Text[i]);
                }
                else
                {
                    labelUsuario.Append(' ');
                }
            }

            userLabel = labelUsuario.ToString();

            tipo = controladoraLinear.DISP_BM;
            tipoBiometria = 0;
            switch (cboxModuloBio.SelectedIndex)
            {
                case 0: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                case 1: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                case 2: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                case 3: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_NITGEN; break;
                case 4: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_ANVIZ; break;
                case 5: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_T5S; break;
                case 6: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_LN3000; break;
            }

            if (tipoBiometria == 0) return;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btApagarTodos_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.apagarDispositivo(0x0F, 0);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void txtTempoRele3Catraca_TextChanged(object sender, EventArgs e)
        {
            txtTempoRele3.Text = txtTempoRele3Catraca.Text;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void txtTempoRele3_TextChanged(object sender, EventArgs e)
        {
            txtTempoRele3Catraca.Text = txtTempoRele3.Text;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void btConectarSerial_Click(object sender, EventArgs e)
        {            
            if (btConectarSerial.Text == "Desconectar")
            {
                try
                {
                    serialPort1.Close();
                    btConectarSerial.Text = "Conectar Serial";
                    cboxBaudrate.Enabled = true;
                    cboxPorta.Enabled = true;
                }
                catch { }
            }
            else
            {
                try
                {
                    serialPort1.Close();
                    serialPort1.Dispose();
                }
                catch { }

                try
                {
                    serialPort1 = new System.IO.Ports.SerialPort();
                    serialPort1.DataReceived += serialPort1_DataReceived;
                    Invoke((MethodInvoker)delegate
                    {
                        serialPort1.BaudRate = 19200;
                        serialPort1.PortName = PortadeConexao;
                        serialPort1.DataBits = 8;
                        serialPort1.Parity = 0;
                    });

                    serialPort1.Open();
                    btConectarSerial.Text = "Desconectar";
                    cboxBaudrate.Enabled = false;
                    cboxPorta.Enabled = false;

                    byte[] buf = pControl.lerSetup();
                    enviarMensagem(buf);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    MessageBox.Show("Porta inválida!");
                    btConectarSerial.Text = "Conectar Serial";
                    cboxBaudrate.Enabled = true;
                    cboxPorta.Enabled = true;
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void inicializaPortaSerial()
        {
            try
            {
                serialPort1.Close();
            }
            catch { }
            cboxBaudrate.SelectedIndex = 2;
            //cboxPorta.SelectedIndex = 0;
            cboxBaudrate.Enabled = true;
            cboxPorta.Enabled = true;

            cboxPorta.Items.Clear();
            //string[] ports = new string[255];

            verificaPortaSerialDisponivel();

            if (cboxPorta.Items.Count != 0)
            {
                cboxPorta.SelectedIndex = 0;
            }           
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void verificaPortaSerialDisponivel()
        {
            for (int i = 1; i < 128; i++)
            {
                try
                {
                    serialPort1.BaudRate = 115200;
                    serialPort1.PortName = PortadeConexao;
                    serialPort1.DataBits = 8;
                    serialPort1.Parity = 0;
                    serialPort1.Open();
                    serialPort1.Close();
                    //cboxPorta.Items.Add("COM" + i.ToString());
                }
                catch { }
            }

            if (cboxPorta.Items.Count != 0)
            {
                cboxPorta.SelectedIndex = 0;
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                int tamanho = serialPort1.BytesToRead;
                byte[] buf = new byte[tamanho];
                serialPort1.Read(buf, 0, tamanho);
                for (int i = 0; i < tamanho; i++)
                {
                    bufferSerial[indiceSerial++] = buf[i];
                    if (indiceSerial >= bufferSerial.Length) indiceSerial = 0;
                }
                toutSerial = 3;
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void trataSerial()
        {
            try
            {
                int tamanhoMsg;
                int tamanho = indiceSerial;
                byte[] buf = new byte[tamanho];

                for (int i = 0; i < indiceSerial; i++) buf[i] = bufferSerial[i];
                indiceSerial = 0;

                for (int i = 0; i < tamanho; i++)
                {
                    if ((buf[i] == 'S') &&
                        (buf[i + 1] == 'T') &&
                        (buf[i + 2] == 'X'))
                    {
                        tamanhoMsg = (buf[i + 3] << 8) + buf[i + 4];
                        if (verificaChecksum(buf, i + 5, tamanhoMsg) == true)
                        {
                            byte[] newBuf = new byte[tamanhoMsg];
                            for (int k = 0; k < tamanhoMsg; k++) newBuf[k] = buf[i + 5 + k];

                            // mostra resposta    
                            byte[] newBuf2 = new byte[tamanhoMsg + 8];
                            for (int k = 0; k < (tamanhoMsg + 8); k++) newBuf2[k] = buf[i + k];
                            string nBytes = Convert.ToString(tamanhoMsg + 8);
                            string msg = BytesToHexString(newBuf2);
                            msg = msg.Substring(0, (tamanhoMsg + 8) * 2);
                            for (int j = 2; j < (tamanhoMsg + 8) * 3; j += 3) msg = msg.Insert(j, " ");

                            SetResposta( msg, nBytes, "SERIAL", PortadeConexao );

                            // trata resposta
                            trataMensagemRecebida(newBuf, tamanhoMsg, "serial" );
                        }
                        break;
                    }                    
                }
            }
            catch(Exception er )
            {
                MessageBox.Show(er.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btAcionamentoComId_Click(object sender, EventArgs e)
        {
            try
            {
                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro( 0, (byte)comboBoxEntradaSaida_Antipassback.SelectedIndex, 0, 0, 0, 0 );
                UInt64 serial = UInt64.Parse(textBoxEntradaSaida_Serial.Text, System.Globalization.NumberStyles.HexNumber);
                UInt16 contadorHCS = UInt16.Parse(textBoxEntradaSaida_Contador.Text, System.Globalization.NumberStyles.HexNumber);

                byte tipo = (byte)comboBoxEntradaSaida_Tipo.SelectedIndex;
                switch( tipo )
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                }


                byte[] buf = pControl.acionamentoComIdentificacao( (byte)comboBoxEntradaSaida_tipoSaida.SelectedIndex,
                                                                   (byte)comboBoxEntradaSaida_endereco.SelectedIndex,
                                                                   (byte)comboBoxEntradaSaida_saida.SelectedIndex,
                                                                   tipo,
                                                                   serial,
                                                                   contadorHCS,
                                                                   fCadastro,
                                                                   Convert.ToByte(textBoxEntradaSaida_nivel.Text),
                                                                   (byte)comboBoxEntradasSaidas_origemAcionamento.SelectedIndex );
                enviarMensagem(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void txtSerial_TextChanged(object sender, EventArgs e)
        {
            textBoxEntradaSaida_Serial.Text = txtSerial.Text;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void buttonPacoteDados_limpar_Click(object sender, EventArgs e)
        {
            richTextPacote.Clear();
            arquivoRestore = null;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void cboxAntipassback_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBoxEntradaSaida_Antipassback.SelectedIndex = cboxAntipassback.SelectedIndex;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void txtContadorHCS_TextChanged(object sender, EventArgs e)
        {
            textBoxEntradaSaida_Contador.Text = txtContadorHCS.Text;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        private void textBoxEntradaSaida_nivel_TextChanged(object sender, EventArgs e)
        {
            textBoxEntradaSaida_nivel.Text = txtNivel.Text;
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void Conectar_TCP1()
        {
            if (btConectarTCP1.Text == "Conectar TCP")
            {
                try
                {                    
                    ConnectToServer_TCP1();
                    if (tcpClient1.Connected ==  true )
                    {
                        tcpClient1Closed = false;
                        btConectarTCP1.Text = "Desconectar";
                        toutConectar1 = 0;
                        byte[] buf = pControl.lerSetup();
                        enviarMensagem(buf);
                    }
                    else
                    {
                        toutConectar1 = 100;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha na conexão");
                }
            }
            else
            {
                try
                {
                    toutConectar1 = 0;
                    tcpClient1Closed = true;
                    tcpClient1.Close();
                    btConectarTCP1.Text = "Conectar TCP";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha ao fechar o socket!");
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void ConnectToServer_TCP1()
        {
            try
            {
                tcpClient1 = new TcpClient(AddressFamily.InterNetwork);
                //Start the async connect operation           
                tcpClient1.BeginConnect(IPAddress.Parse(txtBox_IP1.Text), Convert.ToUInt16(txt_portaTCP_1.Text), new AsyncCallback(ConnectCallback_TCP1), tcpClient1);
                Thread.Sleep(500);                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                try
                {
                    tcpClient1Closed = true;
                    tcpClient1.Close();
                }
                catch { };
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void ConnectCallback_TCP1(IAsyncResult result)
        {
            try
            {
                if (tcpClient1.Connected)
                {
                    //We are connected successfully.
                    NetworkStream networkStream = tcpClient1.GetStream();
                    byte[] buffer = new byte[tcpClient1.ReceiveBufferSize];
                    //Now we are connected start asyn read operation.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP1, buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        /// Callback for Read operation
        private void ReadCallback_TCP1( IAsyncResult result )
        {
            if (tcpClient1Closed == true) return;

            NetworkStream networkStream = null;
            try
            {
                if (tcpClient1.Connected)
                {
                    networkStream = tcpClient1.GetStream();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            try
            {
                byte[] buffer = result.AsyncState as byte[];
                int nBytesRead = networkStream.EndRead(result);
                byte[] buf = new byte[nBytesRead];
                for (int i = 0; i < nBytesRead; i++) buf[i] = buffer[i];

                try
                {
                    mostraResposta(buf, "TCP SERVER", tcpClient1.Client.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                //Do something with the data object here.
                //Then start reading from the network again.
                if (tcpClient1.Connected)
                {
                    try
                    {
                        networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP1, buffer);
                    }
                    catch
                    {
                        toutConectar1 = 0;
                        tcpClient1Closed = true;
                        try
                        {
                            tcpClient1.Close();
                        }
                        catch
                        {

                        }
                        btConectarTCP1.Text = "Conectar TCP";
                        MessageBox.Show("Falha ao fechar o socket!");
                    }
                }
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void Conectar_TCP6()
        {
            if (btConectarTCP6.Text == "Conectar")
            {
                try
                {
                    ConnectToServer_TCP6();
                    if (tcpClient6.Connected == true)
                    {
                        tcpClient6Closed = false;
                        btConectarTCP6.Text = "Desconectar";
                        toutConectar6 = 0;
                    }
                    else
                    {
                        toutConectar6 = 100;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha na conexão");
                }
            }
            else
            {
                try
                {
                    toutConectar6 = 0;
                    tcpClient6Closed = true;
                    tcpClient6.Close();
                    btConectarTCP6.Text = "Conectar";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha ao fechar o socket!");
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void ConnectToServer_TCP6()
        {
            try
            {
                tcpClient6 = new TcpClient(AddressFamily.InterNetwork);
                //Start the async connect operation           
                tcpClient6.BeginConnect(IPAddress.Parse(txtBox_IP1.Text), Convert.ToUInt16( txtPorta6.Text), new AsyncCallback(ConnectCallback_TCP6), tcpClient6);
                Thread.Sleep(500);
                btConectarTCP6.Text = "Desconectar";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void ConnectCallback_TCP6(IAsyncResult result)
        {
            try
            {
                if (tcpClient6.Connected)
                {
                    //We are connected successfully.
                    NetworkStream networkStream = tcpClient6.GetStream();
                    byte[] buffer = new byte[tcpClient6.ReceiveBufferSize];
                    //Now we are connected start asyn read operation.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP6, buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        /// Callback for Read operation
        private void ReadCallback_TCP6(IAsyncResult result)
        {
            if (tcpClient6Closed == true) return;

            NetworkStream networkStream = null;
            try
            {
                if (tcpClient6.Connected)
                {
                    networkStream = tcpClient6.GetStream();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            byte[] buffer = result.AsyncState as byte[];
            int nBytesRead = networkStream.EndRead(result);
            byte[] buf = new byte[nBytesRead];
            for (int i = 0; i < nBytesRead; i++) buf[i] = buffer[i];

            try
            {
                mostraResposta_TCP6(buf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //Do something with the data object here.
            //Then start reading from the network again.
            if (tcpClient6.Connected)
            {
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP6, buffer);
            }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void Conectar_TCP5()
        {
            if (btConectarTCP5.Text == "Conectar")
            {
                try
                {
                    ConnectToServer_TCP5();
                    if (tcpClient5.Connected == true)
                    {
                        tcpClient5Closed = false;
                        btConectarTCP5.Text = "Desconectar";
                        toutConectar5 = 0;
                    }
                    else
                    {
                        toutConectar5 = 100;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha na conexão");
                }
            }
            else
            {
                try
                {
                    toutConectar5 = 0;
                    tcpClient5Closed = true;
                    tcpClient5.Close();
                    btConectarTCP5.Text = "Conectar";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Falha ao fechar o socket!");
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void ConnectToServer_TCP5()
        {
            try
            {
                tcpClient5 = new TcpClient(AddressFamily.InterNetwork);
                //Start the async connect operation           
                tcpClient5.BeginConnect(IPAddress.Parse(txtBox_IP1.Text), Convert.ToUInt16(txtPorta5.Text), new AsyncCallback(ConnectCallback_TCP5), tcpClient5 );
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void ConnectCallback_TCP5(IAsyncResult result)
        {
            try
            {
                if (tcpClient5.Connected)
                {
                    //We are connected successfully.
                    NetworkStream networkStream = tcpClient5.GetStream();
                    byte[] buffer = new byte[tcpClient5.ReceiveBufferSize];
                    //Now we are connected start asyn read operation.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP5, buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }        
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        /// Callback for Read operation
        private void ReadCallback_TCP5(IAsyncResult result)
        {
            if (tcpClient5Closed == true) return;

            NetworkStream networkStream = null;
            try
            {
                if (tcpClient5.Connected)
                {
                    networkStream = tcpClient5.GetStream();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }

            byte[] buffer = result.AsyncState as byte[];
            int nBytesRead = networkStream.EndRead(result);
            byte[] buf = new byte[nBytesRead];
            for (int i = 0; i < nBytesRead; i++) buf[i] = buffer[i];

            try
            {
                mostraResposta_TCP5(buf);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //Do something with the data object here.
            //Then start reading from the network again.
            if (tcpClient5.Connected)
            {
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback_TCP5, buffer);
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btEnviarCmdPorta6_Click(object sender, EventArgs e)
        {
            try
            {
                int i;
                StringBuilder sb = new StringBuilder();

                for (i = 0; i < txtCmdPorta6.TextLength; i++) if (txtCmdPorta6.Text[i] != ' ') sb.Append(txtCmdPorta6.Text[i]);

                byte[] buf = hexStringToByteArray(sb.ToString(), sb.Length);
                // TCP client
                if ((tcpClient6.Connected == true) && (tcpClient6Closed == false)) tcpClient6.Client.Send(buf);
            }
            catch { };
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btCapturaTemplate1_Click(object sender, EventArgs e)
        {
            switch ( cboxModuloBio.SelectedIndex )
            {
                case 4: // ANVIZ
                case 5: // T5S
                case 6: // LN3000
                    if (conectarHamster() == false)
                    {
                        if (conectarHamster() == false)
                        {
                            MessageBox.Show("Falha ao tentar conectar com o Hasmter!");
                            return;
                        }
                    }

                    try
                    {
                        UInt16 Ret;
                        UInt16 uStatus = 0;
                        Win32.AvzGetImage(0, Win32.gpImage, ref uStatus);

                        if (segundoTemplate_1 == false)
                        {
                            Win32.AvzProcess(Win32.gpImage, Win32.gpFeatureA, Win32.gpBin, 1, 1, COMSType);
                        }
                        else
                        {
                            Win32.AvzProcess(Win32.gpImage, Win32.gpFeatureB, Win32.gpBin, 1, 1, COMSType);
                            UInt16 r = Win32.AvzMatch(Win32.gpFeatureA, Win32.gpFeatureB, 3, 180);
                            if (r != 0)
                            {
                                MessageBox.Show("Primeira digital diferente da segunda ");
                                segundoTemplate_1 = false;
                                return;
                            }
                        }

                        pathAnviz = Directory.GetCurrentDirectory() + "\\bioTemp.bmp";


                        // mostra a imagem bynaria
                        Win32.AvzSaveClrBMPFile(pathAnviz, Win32.gpBin);

                        Image photo = Image.FromFile(pathAnviz);
                        System.IO.MemoryStream ms = new System.IO.MemoryStream();
                        photo.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                        photo.Dispose();
                        Image bitmap2 = Image.FromStream(ms);

                        pictureBoxCapturarDigital.Image = bitmap2;

                        if (segundoTemplate_1 == true)
                        {
                            segundoTemplate_1 = false;
                            btCapturaTemplate1.Text = "Capturar Digital";

                            // compacta template
                            Array.Resize<byte>(ref Win32.gpFeatureBuf1, 512);
                            Ret = Win32.AvzPackFeature(Win32.gpFeatureA, Win32.gpFeatureB, Win32.gpFeatureBuf1);
                            if (Ret > 300)
                            {
                                Array.Resize<byte>(ref Win32.gpFeatureBuf1, Ret);

                                byte[] bufTemplate = new byte[338];
                                for (int i = 0; i < 169; i++)
                                {
                                    bufTemplate[i] = Win32.gpFeatureA[i];
                                    bufTemplate[i + 169] = Win32.gpFeatureB[i];
                                }

                                rTxtTemplate.Clear();
                                StringBuilder sb = new StringBuilder();
                                String msg = BytesToHexString(bufTemplate);
                                int n = msg.Length;
                                for (int i = 0; i < n; i += 2)
                                {
                                    sb.Append(msg[i]);
                                    sb.Append(msg[i + 1]);
                                    sb.Append(' ');
                                }
                                rTxtTemplate.Text = sb.ToString();
                                lblTamanhoTemplate.Text = "Tamanho: " + Convert.ToString(338) + " bytes";
                            }
                            else
                            {
                                Array.Resize<byte>(ref Win32.gpFeatureBuf1, 512);
                            }
                        }
                        else
                        {
                            segundoTemplate_1 = true;
                            btCapturaTemplate1.Text = "Capturar Novamente";
                        }
                    }
                    catch { }

                    try { Win32.AvzCloseDevice(0); }
                    catch { }
                    try { Win32.AvzCloseDevice(0); }
                    catch { }
                break; // 4 = ANVIZ

                case 1: //VIRDI
                    try
                    {
                        capturaTemplateVirdi();
                    }
                    catch { };
                break;

                default:
                    MessageBox.Show("Selecione o 'Módulo Biométrico' referente ao Hamster");
                break;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public Boolean conectarHamster()
        {
            try
            {
                // enum device
                byte[] pName = new byte[8 * 128];
                int iNum;
                iNum = Win32.AvzFindDevice(pName);
                if (iNum > 0)
                {
                    // open device
                    long lRet, ret2;
                    byte[] pID = new byte[512];
                    lRet = Win32.AvzOpenDevice(0, 0);
                    if (lRet == 0)
                    {
                        ret2 = Win32.AvzGetID(0, pID);
                        COMSType = 94;
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }                
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public String ipHighest3Bytes(String ip)
        {
            int i = ip.Length - 1;
            while (i > 0)
            {
                if (ip[i] == '.') break;
                i--;
            }
            return ip.Remove(i + 1);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        ///*************************************************
        /// SENDARP FUNCTION IS USED TO GET MAC ADDRESS
        ///*************************************************
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);
        private string getMACAddress(IPAddress ipAddress)
        {
            byte[] ab = new byte[6];
            int len = ab.Length;
            // This Function Used to Get The Physical Address
            try
            {
                int r = SendARP(ipAddress.GetHashCode(), 0, ab, ref len);
                string mac = BitConverter.ToString(ab, 0, 6);
                return mac;
            }
            catch { }
            return null;
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void listView_cadastro_placasRede_Click(object sender, EventArgs e)
        {
            try
            {
                if((btConectarTCP1.Text != "Conectar TCP")||( btConectarUDP1.Text != "Conectar UDP")) return;

                IPAddress ip = IPAddress.Parse("0.0.0.0");
                UInt16 port = 0;
                if (listView_cadastro_placasRede.Items.Count > 0)
                {
                    for (int i = 0; i < listView_cadastro_placasRede.Items.Count; i++)
                    {
                        if (listView_cadastro_placasRede.Items[i].Selected == true)
                        {
                            lerIPePortaListView(ref ip, ref port, i);
                            if (ip.ToString() != "0.0.0.0.0")
                            {
                                timeoutCmdStatusColor = 5;
                                txtBox_IP1.BackColor = Color.Yellow;
                                txtBox_IP1.Text = ip.ToString();
                                ipEscolhido = ip;

                                txt_portaTCP_1.Text = port.ToString();
                                txt_portaTCP_1.BackColor = Color.Yellow;


                           } 
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void lerIPePortaListView(ref IPAddress ip, ref UInt16 port, int i)
        {
            try
            {
                //       0123456789012346
                // ep = "100.0.0.147:5010"
                int indice_doisPontos = 0;
                for (indice_doisPontos = 0; indice_doisPontos < listView_cadastro_placasRede.Items[i].SubItems[0].Text.Length; indice_doisPontos++)
                {
                    if (listView_cadastro_placasRede.Items[i].SubItems[0].Text[indice_doisPontos] == ':') break;
                }

                ip = IPAddress.Parse(listView_cadastro_placasRede.Items[i].SubItems[0].Text.Substring(0, indice_doisPontos));
                port = Convert.ToUInt16(listView_cadastro_placasRede.Items[i].SubItems[1].Text);



            }
            catch { }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void lerRegistrosPlacas()
        {
            try
            {
                byte[] parametro = new byte[1] { 0 };
                int estadoLerQtdDisp = 0;
                byte[] buf = new byte[1];
                byte[] resposta = new byte[1];
                UInt16[] qtdDisp = new UInt16[15];
                byte[] tiposDisp = new byte[6]{ (byte)controladoraLinear.DISP_TX,
                                                (byte)controladoraLinear.DISP_TA,
                                                (byte)controladoraLinear.DISP_CT,
                                                (byte)controladoraLinear.DISP_BM,
                                                (byte)controladoraLinear.DISP_TP,
                                                (byte)controladoraLinear.DISP_SN };
                UInt16 qtdEventos = 0;

                for (UInt16 i = 0; i < listView_cadastro_placasRede.Items.Count; i++)
                {
                    IPAddress ip = IPAddress.Parse("0.0.0.0");
                    UInt16 port = 0;
                    lerIPePortaListView(ref ip, ref port, i);
                    byte[] ipTemp = ip.GetAddressBytes();


                    // maquina de estados para ler todos os "novos eventos" 
                    estadoLerQtdDisp = 0;
                    while (estadoLerQtdDisp < 11)
                    {
                        if (cancelarProcura == true)
                        {
                            break;
                        }

                        switch (estadoLerQtdDisp)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                //--------------------------------------------------------------------------------------------------------
                                // 87	Ler quantidade de dispositivos
                                // 00 + 57 + <dispositivo> + cs
                                // 00 + 57 + <retorno> + <dispositivo> + <quantidade H> + <quantidade L> + cs
                                buf = pControl.lerQuantidadeDispositivos(tiposDisp[estadoLerQtdDisp]);
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                if ((resposta.Length == 15) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    qtdDisp[estadoLerQtdDisp] = (UInt16)((resposta[5 + 4] << 8) + resposta[5 + 5]);
                                    listView_cadastro_placasRede.Items[i].SubItems[5 + estadoLerQtdDisp].Text = qtdDisp[estadoLerQtdDisp].ToString();
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;

                            case 6:
                                //--------------------------------------------------------------------------------------------------------
                                buf = pControl.operacaoParametroSetup(controladoraLinear.OP_LEITURA, (ushort)controladoraLinear.INDICE_ENDCAN , (ushort)pControl.sizeOfparametrosSetup[controladoraLinear.INDICE_ENDCAN], parametro);
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                //--------------------------------------------------------------------------------------------------------
                                // 120	Edita parâmetro Setup	
                                // 00 + 78 + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                if ((resposta.Length == 18) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    listView_cadastro_placasRede.Items[i].SubItems[3].Text = Convert.ToString(resposta[5 + 8] + 1);
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;

                            case 7:
                                //--------------------------------------------------------------------------------------------------------                            
                                buf = pControl.operacaoParametroSetup(controladoraLinear.OP_LEITURA, controladoraLinear.INDICE_NIVEL, (UInt16)pControl.sizeOfparametrosSetup[controladoraLinear.INDICE_NIVEL], parametro);
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                //--------------------------------------------------------------------------------------------------------
                                // 120	Edita parâmetro Setup	
                                // 00 + 78 + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                if ((resposta.Length == 18) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    listView_cadastro_placasRede.Items[i].SubItems[4].Text = Convert.ToString(resposta[5 + 8] + 1);
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;

                            //--------------------------------------------------------------------------------------------------------                            
                            case 8:
                                parametro = new byte[ pControl.sizeOfparametrosSetup[controladoraLinear.INDICE_DNSHOST] ];

                                UInt16 indiceParametro;
                                UInt16 tamanhoParametro;
                                indiceParametro = (UInt16)pControl.parametrosSetup[cboxParametro.Items.IndexOf("INDICE_DNSHOST(201)")];
                                tamanhoParametro = (UInt16)pControl.sizeOfparametrosSetup[cboxParametro.Items.IndexOf("INDICE_DNSHOST(201)")];
                                Array.Resize<byte>(ref parametro, tamanhoParametro);

                                buf = pControl.operacaoParametroSetup(controladoraLinear.OP_LEITURA, indiceParametro, tamanhoParametro, parametro);
                                
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                //--------------------------------------------------------------------------------------------------------
                                // 120	Edita parâmetro Setup	
                                // 00 + 78 + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
                                if ((resposta.Length == 33) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    for (int j = 0; j < tamanhoParametro; j++) sb.Append((char)resposta[5 + 8 + j]);
                                    listView_cadastro_placasRede.Items[i].SubItems[1].Text = sb.ToString();
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;

                            case 9:
                                //--------------------------------------------------------------------------------------------------------
                                // 90 Ler quantidade de eventos
                                // 00 + 5A + <marca> + cs
                                // 00 + 5A + <retorno> + <marca> + <qtdEventosH> + <qtdEventosL> + cs
                                buf = pControl.lerQuantidadeEventos(controladoraLinear.LOG_TODOS);
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                if ((resposta.Length == 15) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    qtdEventos = (UInt16)((resposta[5 + 4] << 8) + resposta[5 + 5]);
                                    listView_cadastro_placasRede.Items[i].SubItems[11].Text = qtdEventos.ToString();
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;

                            case 10:
                                //--------------------------------------------------------------------------------------------------------
                                // 90 Ler quantidade de eventos
                                // 00 + 5A + <marca> + cs
                                // 00 + 5A + <retorno> + <marca> + <qtdEventosH> + <qtdEventosL> + cs
                                buf = pControl.lerQuantidadeEventos(controladoraLinear.LOG_NAO_LIDO);
                                //---------------------------------------------------------------------
                                //
                                try
                                {
                                    Thread.Sleep(10);
                                    resposta = enviaMsgSocket(ip, port, buf);
                                }
                                catch
                                {
                                    estadoLerQtdDisp = 255;
                                }

                                if ((resposta.Length == 15) && (resposta[2] != controladoraLinear.RT_OK))
                                {
                                    qtdEventos = (UInt16)((resposta[5 + 4] << 8) + resposta[5 + 5]);
                                    listView_cadastro_placasRede.Items[i].SubItems[12].Text = qtdEventos.ToString();
                                    estadoLerQtdDisp++;
                                }
                                else
                                {
                                    estadoLerQtdDisp = 255;
                                }
                            break;
                        }
                    }

                    if (estadoLerQtdDisp == 255)
                    {
                        listView_cadastro_placasRede.Items[i].BackColor = Color.Red;
                    }
                    else
                    {
                        listView_cadastro_placasRede.Items[i].BackColor = Color.Green;
                        timeoutCmdStatusColor = 50;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Valor Inválido!!");
            };
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public byte[] enviaMsgSocket(IPAddress ip, UInt16 port, byte[] buf )
        {
            byte[] resposta = new byte[1]{0xFF};
            IPEndPoint remoteEndPoint = new IPEndPoint(ip, port);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.ReceiveTimeout = 1000;
                s.Connect(remoteEndPoint);
                if (s.Connected)
                {
                    s.Send(buf);
                    byte[] bufResposta = new byte[1500];
                    int nBytesResposta = 0;
                    do
                    {
                        nBytesResposta = s.Receive(bufResposta);
                    }
                    while ((s.Available > 0) && (nBytesResposta == 0) && (s.Connected == true));
                    Array.Resize<byte>(ref bufResposta, nBytesResposta);
                    resposta = bufResposta;
                }
            }
            catch { }
            disconectSocket(s);
            return resposta;
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void disconectSocket(Socket s)
        {
            try
            {
                if (s.Connected)
                {
                    try
                    {
                        s.Disconnect(true);
                    }
                    catch { }

                    try
                    {
                        s.Close();
                    }
                    catch { }

                    try
                    {
                        s.Dispose();
                    }
                    catch { }
                }
            }
            catch { }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btEnviaTemplate_Click(object sender, EventArgs e)
        {
            if (rTxtTemplate.Text == null) return;

            if (btConectarSerial.Text != "Desconectar")
            {
                MessageBox.Show("Não conectado na serial");
                return;
            }

            String t = rTxtTemplate.Text.Replace(" ", "");
            byte[] template = HexStringToBytes(t);
            UInt16 tamanhoTemplate = (UInt16)template.Length;

            byte[] buf = new byte[template.Length + 7];

            int j = 0;
            buf[j++] = 0;
            buf[j++] = 200;
            buf[j++] = controladoraLinear.TIPO_BIOMETRIA_ANVIZ;
            buf[j++] = 0;
            buf[j++] = (byte)(template.Length >> 8);
            buf[j] = (byte)(template.Length);
            for (int i = 0; i < template.Length; i++)
            {
                j++;
                buf[j] = template[i];
            }
            buf[j] = 0;
            for (int i = 0; i < j; i++) buf[j] += buf[i];

            if( tamanhoTemplate == 338 )
            {
                serialPort1.Write( buf, 0, buf.Length );
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonEventos_operacaoEvento_Click(object sender, EventArgs e)
        {
            byte operacao = controladoraLinear.OP_LER_EVENTO_INDEXADO;

            switch (comboBoxEvento_operacaoEvento.SelectedIndex)
            {
                case 0:
                    operacao = controladoraLinear.OP_LER_EVENTO_INDEXADO;
                    break;
                case 1:
                    operacao = controladoraLinear.OP_MARCAR_EVENTO_COMO_LIDO_INDEXADO;
                    break;
                case 2:
                    operacao = controladoraLinear.OP_LER_ULTIMO_EVENTO_NAO_LIDO;
                    break;
            }

            byte[] buf = pControl.operacaoEvento((byte)operacao, (UInt16)comboBoxEvento_indiceEvento.SelectedIndex);
            enviarMensagem(buf);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void cboxModuloBio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((cboxModuloBio.SelectedIndex != 4 )&&(cboxModuloBio.SelectedIndex != 5) && (cboxModuloBio.SelectedIndex != 6)) // ANVIZ , T5S e LN3000
            {
                btCapturaTemplate1.Enabled = false;
            }
            else
            {
                btCapturaTemplate1.Enabled = true;
            }

        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonDispositivos_MatchFile_Click(object sender, EventArgs e)
        {
            switch( matchTemplate() )
            {
                case 0:
                    labelDispositivos_MatchScan.Text = "Template invalido!!";
                break;

                case 1:
                     labelDispositivos_MatchScan.Text = "Diferente do Scan!!";
                break;

                case 2:
                     labelDispositivos_MatchScan.Text = "Igual ao Scan!!";
                break;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public int matchTemplate()
        {
            byte[] template = new byte[0];
            try
            {
                String t = rTxtTemplate.Text.Replace(" ", "");
                template = HexStringToBytes(t);
                UInt16 tamanhoTemplate = (UInt16)template.Length;
                if (tamanhoTemplate != 338)
                {
                    return 0; // invalido
                }
            }
            catch
            {
                return 0; // invalido
            }

            int i = 0;
            for (i = 0; i < 169; i++)
            {
                Win32.gpFeatureC[i] = template[i];
            }

            int Ret = Win32.AvzMatch(Win32.gpFeatureA, Win32.gpFeatureC, 3, 180);
            if (Ret != 0)
            {
                return 1; // diferente
            }
            else
            {
                return 2; // igual
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonLerPacoteEventos_Click(object sender, EventArgs e)
        {            
            try
            {
                byte[] buf = pControl.lerPacoteEventos((ushort)comboBoxIndiceEventoPacote.SelectedIndex );
                enviarMensagem(buf);
            }
            catch
            {
                MessageBox.Show("Indice inválido!");
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void button_Eventos_Limpar_Click(object sender, EventArgs e)
        {
            while (lstEventos.Items.Count > 0)
            {
                lstEventos.Items.RemoveAt(0);
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void button_Eventos_CancelaTimeoutProgressivo_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.cancelaTimeoutProgressivo();
                enviarMensagem(buf);
            }
            catch { }
        }
        //-----------------------------------------------------------------------------
        // 1s
        //-----------------------------------------------------------------------------
        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                UInt16 tempo = (UInt16)(Convert.ToUInt16(lblTimerRemoto.Text));
                if (tempo > 0)
                {
                    tempo--;
                    lblTimerRemoto.Text = Convert.ToString(tempo);
                }
            }
            catch { };
        }        
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public byte[] eventoUdp(byte evento, byte[] serial, byte modo, byte nivel, byte flagsEvento)
        {
            /*
            "UDP" ou 0x55 0x44 0x50
            IP de origem
            Porta de origem
            Endereço de origem
            counter
            Flags
            0x0D
            0x0A
            "LINEAR-HCS" ou 0x4C 0x49 0x4E 0x45 0x41 0x52 0x2D 0x48 0x43 0x53
            "(CA):" ou 0x28 0x43 0x41 0x29
            "STX" ou 0x53 0x54 0x58
            tamanho
            */ 
            byte[] buf = new byte[21 + 35 + 3];
            int i = 0;
            IPAddress ipLocal = IPAddress.Parse(GetLocalIP());
            byte[] ipBytes = ipLocal.GetAddressBytes();
            UInt16 porta = Convert.ToUInt16(txtPorta2_SETUP_TCPIP.Text);

            buf[i++] = (byte)'U';
            buf[i++] = (byte)'D';
            buf[i++] = (byte)'P';
            buf[i++] = ipBytes[0];
            buf[i++] = ipBytes[1];
            buf[i++] = ipBytes[2];
            buf[i++] = ipBytes[3];
            buf[i++] = (byte)(porta >> 8);
            buf[i++] = (byte)porta;
            buf[i++] = 63;
            buf[i++] = 0x00; // counter H
            buf[i++] = 0x00; // counter L
            buf[i++] = 0x00; // flags
            buf[i++] = 0x0D;
            buf[i++] = 0x0A;
            buf[i++] = (byte)'L';
            buf[i++] = (byte)'I';
            buf[i++] = (byte)'N';
            buf[i++] = (byte)'E';
            buf[i++] = (byte)'A';
            buf[i++] = (byte)'R';
            buf[i++] = (byte)'-';
            buf[i++] = (byte)'H';
            buf[i++] = (byte)'C';
            buf[i++] = (byte)'S';

            buf[i++] = (byte)'(';
            buf[i++] = (byte)'C';
            buf[i++] = (byte)'A';
            buf[i++] = (byte)')';
            buf[i++] = (byte)':';

            buf[i++] = (byte)'S';
            buf[i++] = (byte)'T';
            buf[i++] = (byte)'X';

            buf[i++] = 0x00;
            buf[i++] = 21;

            //---------------------------------
            // i == 35
            // 00 + 74 + <retorno> + <cntAtual> + <frame de evt. (16 bytes)> + cs      [21 bytes] 56

            buf[i++] = 0x00;
            buf[i++] = 0x74;
            buf[i++] = 0x00; // retorno
            buf[i++] = 0x00; // cntAtual
            buf[i++] = evento; // frame evento...
            buf[i++] = (byte)((modo<<5) + (63));
            buf[i++] = (byte)((0x00<<4) + (controladoraLinear.DISP_CT)); ; //s0 + tipoDisp
            buf[i++] = 0x00; //s1
            buf[i++] = serial[0]; //s2
            buf[i++] = serial[1]; //s3
            buf[i++] = serial[2]; //s4
            buf[i++] = serial[3]; //s5
            buf[i++] = Convert.ToByte(DateTime.Now.Hour.ToString("00"));
            buf[i++] = Convert.ToByte(DateTime.Now.Minute.ToString("00"));
            buf[i++] = Convert.ToByte(DateTime.Now.Second.ToString("00"));
            buf[i++] = Convert.ToByte(DateTime.Now.Day.ToString("00"));
            buf[i++] = Convert.ToByte(DateTime.Now.Month.ToString("00"));
            buf[i++] = Convert.ToByte(DateTime.Now.Second.ToString("00"));
            buf[i++] = nivel;
            buf[i++] = flagsEvento;

            // checksum            
            byte checksum = 0;
            for (int j = 0; j < 20; j++) checksum += buf[35 + j];
            buf[i++] = checksum;

            //---------------------------------
            buf[i++] = (byte)'E';
            buf[i++] = (byte)'T';
            buf[i++] = (byte)'X';

            return buf;
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonSistema_lerAntipassbackEspecifico_Click(object sender, EventArgs e)
        {
            try
            {
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);
                byte tipo = (byte)comboBoxSistema_tipo.SelectedIndex;
                switch( tipo )
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                    default: return;
                }

                byte[] buf = pControl.lerAntipassbackEspecifico((byte)comboBoxSistema_endereco.SelectedIndex, tipo, serial);
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btOperacaoCtrlVagasHab_Click(object sender, EventArgs e)
        {
            try
            {
                byte operacao = (byte)cboxOperacaoPacote.SelectedIndex;
                operacao = controladoraLinear.OP_ESCRITA;
                if( comboBoxCtrlVagasHab_operacao.SelectedIndex == 0 ) operacao = controladoraLinear.OP_LEITURA;
                
                byte[] buf = pControl.operacaoVagasHabilitacao( operacao, 
                                                                (UInt16)comboBoxCtrlVagasHab_Rota1.SelectedIndex, 
                                                                (byte)comboBoxCtrlVagasPreset.SelectedIndex,
                                                                (byte)comboBoxCtrlVagasAtual.SelectedIndex ); 
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btRestaurarVagasHab_Click(object sender, EventArgs e)
        {            
            try
            {
                UInt16 aux = 0xFFFF;
                if( comboBoxCtrlVagasHab_Rota2.SelectedIndex != (comboBoxCtrlVagasHab_Rota2.Items.Count - 1))
                {
                    aux = (UInt16)comboBoxCtrlVagasHab_Rota2.SelectedIndex;
                }

                byte[] buf = pControl.restaurarVagasHabilitacao( aux );
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btLerVagasEspecifico_Click(object sender, EventArgs e)
        {
            try
            {
                byte tipo = 0;
                UInt64 serial = UInt64.Parse( textBoxCtrlVagas_serial.Text, System.Globalization.NumberStyles.HexNumber);

                switch( comboBoxCtrlVagas_tipo.SelectedIndex )
                {
                    case 0: tipo = controladoraLinear.DISP_TX; break;
                    case 1: tipo = controladoraLinear.DISP_TA; break;
                    case 2: tipo = controladoraLinear.DISP_CT; break;
                    case 3: tipo = controladoraLinear.DISP_BM; break;
                    case 4: tipo = controladoraLinear.DISP_TP; break;
                    case 5: tipo = controladoraLinear.DISP_SN; break;
                }

                byte[] buf = pControl.lerVagasEspecifico( tipo, serial );
                enviarMensagem(buf);
            }
            catch { };            
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void button_ControleVagas_pacoteVagas_Click(object sender, EventArgs e)
        {
            lerPacoteVagas();
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void lerPacoteVagas( )
        {
            try
            {
                byte[] pacote = new byte[1024];
                byte[] buf = pControl.operacaoPacoteVagasHabilitação(controladoraLinear.OP_LEITURA, 0, pacote);
                enviarMensagem(buf);

                pacote = new byte[1024];
                buf = pControl.operacaoPacoteVagasHabilitação(controladoraLinear.OP_LEITURA, 1, pacote);
                enviarMensagem(buf);
            }
            catch { };
        }        
        //-----------------------------------------------------------------------------
        // PORTA 7
        //-----------------------------------------------------------------------------
        public void ServerReceive()
        {
            byte[] buf = new byte[1500];

            stream = client.GetStream(); //Gets The Stream of The Connection
            new Thread(() => // Thread (like Timer)
            {
                try
                {
                    stream.Read(buf, 0, buf.Length);//Keeps Trying to Receive the Size of the Message or Data
                }
                catch { }

                if ((buf[0] == 'S') &&
                    (buf[1] == 'T') &&
                    (buf[2] == 'X'))
                {
                    this.Invoke((MethodInvoker)delegate // To Write the Received data
                    {
                        try
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                mostraResposta(buf, "TCP CLIENT", client.Client.RemoteEndPoint.ToString() );
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                }
            }).Start(); // Start the Thread

        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void ServerSend(byte[] data)
        {
            stream = client.GetStream(); //Gets The Stream of The Connection
            int length = data.Length; // Gets the length of the byte data
            stream.Write(data, 0, data.Length); //Sends the real data
        }        
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btPacoteDados_gravarArquivo_Click(object sender, EventArgs e)
        {
            if( arquivoRestore.Length > 0 )
            {
                salvaArquivo(controladoraLinear.FILE_DISP, arquivoRestore);
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public int repetir = 0;
        private void buttonEntradasSaidas_Repetir_Click(object sender, EventArgs e)
        {
            repetir = comboBoxEntradasSaidas_nVezes.SelectedIndex;
            acionamentoEntradaSaida();
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void acionamentoEntradaSaida()
        {
            try
            {
                byte[] buf = pControl.acionamento((byte)comboBoxEntradaSaida_tipoSaida.SelectedIndex,
                                        (byte)comboBoxEntradaSaida_endereco.SelectedIndex,
                                        (byte)(comboBoxEntradaSaida_saida.SelectedIndex));

                enviarMensagem(buf);
            }
            catch { };
        }        
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        //           1         2         3         4         5   [54]
        // 012345678901234567890123456789012345678901234567890123
        // 0:/123456789012/123456789012/123456789012/12345678.012
        // CMD: 00 + 97 + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(nbytesDados de 0 a 1024)> + cs   [ 61 + nBytes ]
        // RSP: 00 + 97 + <retorno> + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(nbytesDados de 0 a 1024)> + cs   [ 62 + nBytes ]
        private void button_SD_CARD_operação_Click(object sender, EventArgs e)
        {
            byte operacao = 26;
            byte midia = 0;
            if (radioButtonSDCARD_MIDIA_pendrive.Checked)
            {
                midia = 1;
            }
            byte[] path = new byte[54];
            byte[] dados = new byte[1024];
            UInt16 tamanho = 0;


            if (radioButton_SDCARD_op_abrirArquivoLeitura.Checked)
            {
                operacao = controladoraLinear.OP_ABRIR_ARQUIVO_LEITURA;
            }
            else if (radioButton_SDCARD_op_lerArquivoLeitura.Checked)
            {
                operacao = controladoraLinear.OP_LER_ARQUIVO_LEITURA;
            }
            else if (radioButton_SDCARD_op_abrirArquivoEscrita.Checked)
            {
                operacao = controladoraLinear.OP_CRIAR_ABRIR_ARQUIVO_ESCRITA;
            }
            else if (radioButton_SDCARD_op_escreverArquivoEscrita.Checked )
            {
                operacao = controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA;
            }
            else if (radioButton_SDCARD_op_fecharArquivo.Checked)
            {
                operacao = controladoraLinear.OP_FECHAR_ARQUIVO;
            }
            else if (radioButton_SDCARD_op_listar.Checked)
            {
                operacao = controladoraLinear.OP_LISTAR;
            }
            else if (radioButton_SDCARD_op_criarDiretorio.Checked)
            {
                operacao = controladoraLinear.OP_CRIAR_DIRETORIO;
            }
            else if (radioButton_SDCARD_op_mudarDiretorio.Checked)
            {
                operacao = controladoraLinear.OP_MUDAR_DIRETORIO;
            }
            else if (radioButton_SDCARD_op_reiniciarMidia.Checked)
            {
                operacao = controladoraLinear.OP_REINICIAR_MIDIA;
            }
            else if ( radioButton_SDCARD_op_apagarArquivoDiretorio.Checked )
            {
                operacao = controladoraLinear.OP_APAGAR_ARQUIVO_DIRETORIO;
            }
            else return;

            for (int i = 0; i < 54; i++)
            {
                if (textBox_SDCARD_MIDIA_nome.Text.Length > i)
                {
                    path[i] = (byte)textBox_SDCARD_MIDIA_nome.Text[i];
                }
            }

            tamanho = 0;
            if (operacao == controladoraLinear.OP_ESCREVER_ARQUIVO_ESCRITA )
            {
                if (richTextBox_SDCARD_MIDIA_dados.Text.Length > 0)
                {
                    Array.Resize<byte>(ref dados, richTextBox_SDCARD_MIDIA_dados.Text.Length);
                    tamanho = (UInt16)richTextBox_SDCARD_MIDIA_dados.Text.Length;
                    
                    if (tamanho > 1024)
                    {
                        tamanho = 1024;
                    }

                    for (int i = 0; i < tamanho; i++)
                    {
                        dados[i] = (byte)richTextBox_SDCARD_MIDIA_dados.Text[i];
                    }
                }
            }
            

            byte[] buf = pControl.lerEditarMidia(operacao, midia, path, tamanho, dados);
            enviarMensagem(buf);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btEditarHabilitacaoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            editarHabilitacaoLeitoras(143);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btLerEditarRotaIndividualComSetor_Click(object sender, EventArgs e)
        {
            lerEditarRotaIndividual(148);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btEditarRotaEGrupoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            editarRotaEGrupoLeitoras(149);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btOperacaoPacoteLabelsRotaComSetor_Click(object sender, EventArgs e)
        {
            operacaoPacoteComSetorClick(controladoraLinear.FILE_LABELS, "LABEL.TXT", (byte)comboBox_SETUP_setor.SelectedIndex);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btOperacaoPacoteGrupoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            operacaoPacoteComSetorClick(controladoraLinear.FILE_GRUPOS, "GRUPO.TXT", (byte)comboBox_SETUP_setor.SelectedIndex);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btOperacaoPacoteHabilitacaoComSetor_Click(object sender, EventArgs e)
        {
            operacaoPacoteComSetorClick(controladoraLinear.FILE_ROTAS, "ROTAS.TXT", (byte)comboBox_SETUP_setor.SelectedIndex);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btEditarGrupoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.editarGrupoLeitorasComSetor(Convert.ToUInt16(cboxCodigoGrupoLeitoras.SelectedIndex),
                                                            Convert.ToByte(cboxEnderecoGL.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL1.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL2.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL3.SelectedIndex),
                                                            Convert.ToByte(cboxJornadaL4.SelectedIndex),
                                                            (byte)comboBox_SETUP_setor.SelectedIndex);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void btLerGrupoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerGrupoLeitorasComSetor( Convert.ToUInt16(cboxCodigoGrupoLeitoras.SelectedIndex),
                                                                Convert.ToByte(cboxEnderecoGL.SelectedIndex),
                                                                (byte)comboBox_SETUP_setor.SelectedIndex);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonSDCARD_MIDIA_limpar_Click(object sender, EventArgs e)
        {
            listView_SDCARD_MIDIA_Clear();
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        public void listView_SDCARD_MIDIA_Clear( )
        {
            ListViewItem listTemp = new ListViewItem();
            listView_SDCARD_MIDIA.Items.Clear();
            listTemp.Text = "...";
            listView_SDCARD_MIDIA.Items.Add(listTemp);
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void listView_SDCARD_MIDIA_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView_SDCARD_MIDIA.Items.Count > 0)
                {
                    for (int i = 0; i < listView_SDCARD_MIDIA.Items.Count; i++)
                    {
                        if (listView_SDCARD_MIDIA.Items[i].Selected)
                        {
                            String s = listView_SDCARD_MIDIA.Items[i].SubItems[0].Text;
                            textBox_SDCARD_MIDIA_nomeArquivoRestore.Text = null;
                            if( s.Contains('.') ) textBox_SDCARD_MIDIA_nomeArquivoRestore.Text = s;

                            char c = diretorioAtual[diretorioAtual.Length-1];
                            if ( c != (char)'/' )
                            {
                                diretorioAtual += "/";
                            }

                            textBox_SDCARD_MIDIA_nome.Text = diretorioAtual + s;

                            if( ( listView_SDCARD_MIDIA.Items[i].SubItems.Count > 1 )&&
                                ( Convert.ToInt32( listView_SDCARD_MIDIA.Items[i].SubItems[1].Text ) > 0 ) )
                            {
                                radioButton_SDCARD_op_abrirArquivoLeitura.Checked = true;
                            }
                            else
                            {
                                radioButton_SDCARD_op_mudarDiretorio.Checked = true;
                            }
                            break;
                        }
                    }
                }
            }
            catch { }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void buttonSDCARD_MIDIA_limparRT_Click(object sender, EventArgs e)
        {
            richTextBox_SDCARD_MIDIA_dados.Clear();
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void btLerHabilitacaoLeitorasComSetor_Click(object sender, EventArgs e)
        {
            try
            {
                desabilitarTodasLeitoras();
                byte[] buf = pControl.lerHabilitacaoLeitorasComSetor(cbox_codigoRota.SelectedIndex, (byte)comboBox_SETUP_setor.SelectedIndex);
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        public Boolean emRestoreBiometria = false;
        private void button_dispositivos_restoreArquivo_Click(object sender, EventArgs e)
        {
           
            try
            {
                openFileDialog1.FileName = "DISP.DPT";
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(openFileDialog1.FileName);
                    arquivoRestore = sr.ReadToEnd();
                    sr.Close();
                    if (arquivoRestore != null)
                    {
                        indiceProgressivo = 0;

                        int n = arquivoRestore.Length;
                        StringBuilder sb = new StringBuilder();
                        for( int i = 0; i < arquivoRestore.Length; i++ )
                        {
                            if ((arquivoRestore[i] == '\r') || (arquivoRestore[i] == '\n'))
                            {
                                listaRestoreBiometria.Add(hexStringToByteArray(sb.ToString(), sb.ToString().Length));
                                sb = new StringBuilder();
                                i += 1;
                            }
                            else
                            {
                                sb.Append(arquivoRestore[i]);
                            }
                        }

                        if( listaRestoreBiometria.Count > 0 )
                        {
                            executarNvezes = listaRestoreBiometria.Count - 1;
                            byte[] buf = listaRestoreBiometria[executarNvezes];
                            enviarMensagem(buf);
                            emRestoreBiometria = true;
                        }
                    }
                    
                }
            }
            catch { };
            
        }        
        //-----------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------
        private void button_procurarPlacasNaRede_Click(object sender, EventArgs e)
        {
            buscarPlacas();

            if (form_sniffer.IsHandleCreated == true)
            {
                string t = "TX UDP:30303 [" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                rt_sniffer.Text += t + "44\r\n\r\n";
                form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
            }

        }
        //----------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private void buscarPlacas()
        {
            listView_cadastro_placasRede.Items.Clear();
            buscaDispositivosNaRede();
        }        
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public UInt16 portaUdp = 0;
        public void conectarUdp()
        {
            try
            {
                IPEndPoint localEp = new IPEndPoint(IPAddress.Any, portaUdp);
                udpClient1 = new UdpClient();
                udpClient1.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient1.ExclusiveAddressUse = false;
                udpClient1.Client.Bind(localEp);
                udpClient1.BeginReceive(new AsyncCallback(ReadCallback_UDP2), null);
            }
            catch { }
        }
        //--------------------------------------------------------------------
        //
        //--------------------------------------------------------------------
        public void buscaDispositivosNaRede()
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Connect(IPAddress.Broadcast, 30303);
                portaUdp = (UInt16)((IPEndPoint)s.LocalEndPoint).Port;
                byte[] buf = new byte[1];
                buf[0] = (byte)'D';
                s.Send(buf);
                s.Close();
            }
            catch { }



            conectarUdp();
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        /// Callback for Read operation
        private void ReadCallback_UDP2(IAsyncResult result)
        {
            byte[] buf = new byte[1];
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 30303);
            String hostname = null;
            String macAddress = null;
            String descricao = null;
            String porta1 = null, porta2 = null, modo = null, endereco = null, setor = null, nivel = null;
            StringBuilder sb = new StringBuilder();
            Boolean doisPontos = false;
            try
            {
                buf = udpClient1.EndReceive(result, ref RemoteIpEndPoint);
                udpClient1.BeginReceive(new AsyncCallback(ReadCallback_UDP2), udpClient1.Client);
                int sm = 0;
                StringBuilder sb2 = new StringBuilder();
                for (int i = 0; i < buf.Length; i++) sb2.Append(Convert.ToChar(buf[i]));

                
                for (int i = 0; i < buf.Length; i++)
                {
                    switch (sm)
                    {
                        //----------------------------------------------
                        // HOSTNAME
                        case 0:
                            if (buf[i] == 0x0d)
                            {
                                i += 1;
                                sm++;
                                hostname = sb.ToString();
                                sb = new StringBuilder();
                                break;
                            }
                            sb.Append((char)buf[i]);
                            break;
                        //----------------------------------------------
                        // MAC ADDRESS
                        case 1:
                            if (buf[i] == 0x0d)
                            {
                                i += 1;
                                sm++;
                                macAddress = sb.ToString();
                                sb = new StringBuilder();
                                break;
                            }
                            sb.Append((char)buf[i]);
                            break;
                        //----------------------------------------------
                        // DESCRIÇÃO
                        case 2:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                descricao = sb.ToString();
                                if (((i + 1) >= buf.Length) || (!descricao.Contains("Placa Controladora")))
                                {
                                    sm = 255;
                                    break;
                                }
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }
                            sb.Append((char)buf[i]);
                            break;
                        //----------------------------------------------
                        // PORTA TCP1
                        case 3:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                porta1 = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;
                        //----------------------------------------------
                        // PORTA TCP2
                        case 4:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                porta2 = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;
                        //----------------------------------------------
                        // MODO
                        case 5:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                modo = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;
                        //----------------------------------------------
                        // ENDEREÇO
                        case 6:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                endereco = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;
                        //----------------------------------------------
                        // SETOR
                        case 7:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                setor = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;
                        //----------------------------------------------
                        // NIVEL
                        case 8:
                            if ((buf[i] == 0x0d) || ((i + 1) == buf.Length))
                            {
                                i += 1;
                                sm++;
                                nivel = sb.ToString();
                                sb = new StringBuilder();
                                doisPontos = false;
                                break;
                            }

                            if (buf[i] == (byte)(':'))
                            {
                                doisPontos = true;
                            }
                            else
                            {
                                if (doisPontos)
                                {
                                    sb.Append((char)buf[i]);
                                }
                            }
                            break;

                    }

                    if (sm > 8) break;
                }
                // 

                if (descricao.Contains("Placa Controladora"))
                {
                    ListViewItem listTemp = new ListViewItem();
                    listTemp.Text = RemoteIpEndPoint.Address.ToString(); // IP
                    if (porta1 == null)
                    {
                        listTemp.SubItems.Add("9762"); // PORTA PADRÃO
                    }
                    else
                    {
                        listTemp.SubItems.Add(porta1); // PORTA TCP 1
                    }

                    if (porta2 == null)
                    {
                        listTemp.SubItems.Add("9763"); // PORTA PADRÃO
                    }
                    else
                    {
                        listTemp.SubItems.Add(porta2); // PORTA TCP 2
                    }

                    if (modo == null)
                    {
                        listTemp.SubItems.Add("PORTA"); // MODO PADRÃO
                    }
                    else
                    {
                        switch (modo)
                        {
                            case "0": modo = "CATRACA"; break;
                            case "1": modo = "PORTA"; break;
                            case "2": modo = "CANCELA"; break;
                        }
                        listTemp.SubItems.Add(modo); // MODO
                    }

                    listTemp.SubItems.Add(hostname); // HOSTNAME

                    if (endereco == null)
                    {
                        listTemp.SubItems.Add("1"); // ENDERECO PADRÃO
                    }
                    else
                    {
                        listTemp.SubItems.Add((Convert.ToByte(endereco) + 1).ToString()); // ENDERECO
                    }

                    if (setor == null)
                    {
                        listTemp.SubItems.Add("1"); // SETOR PADRÃO
                    }
                    else
                    {
                        listTemp.SubItems.Add((Convert.ToByte(setor) + 1).ToString()); // SETOR
                    }

                    listTemp.SubItems.Add(descricao); // DESCRIÇÃO


                    Invoke((MethodInvoker)delegate
                    {
                        listView_cadastro_placasRede.Items.Add(listTemp);
                        label_placasNaRede.Text = listView_cadastro_placasRede.Items.Count.ToString();
                    });


                    Invoke((MethodInvoker)delegate
                    {
                        if (form_sniffer.IsHandleCreated == true)
                        {
                            int tamanhoMsg = buf.Length;
                            String msg = BytesToHexString(buf);
                            msg = msg.Substring(0, tamanhoMsg * 2);
                            for (int j = 2; j < tamanhoMsg * 3; j += 3) msg = msg.Insert(j, " ");

                            string t = "RX UDP:30303 [" + Convert.ToString(DateTime.Now.Hour) + ':' + Convert.ToString(DateTime.Now.Minute) + ':' + Convert.ToString(DateTime.Now.Second) + ':' + Convert.ToString(DateTime.Now.Millisecond) + "] ";
                            rt_sniffer.Text += t + msg + "\r\n\r\n";

                            form_sniffer.richTextBox_sniffer.Text = rt_sniffer.Text;
                        }
                    });
                }
            }
            catch
            {
                return;
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void button_controleVagas_operacaoVagasID_Click(object sender, EventArgs e)
        {

            byte operacao = 0;
            switch (comboBox_controleVagas_vagasId.SelectedIndex)
            {
                case 0: operacao = controladoraLinear.OP_LEITURA; break;
                case 1: operacao = controladoraLinear.OP_ESCRITA; break;
                default: return;
            }
            
            String id = textBox_controleVagasID_id.Text;
            UInt16 indice = 0;
            byte totalVagas = 0, atualVagas = 0;
            byte[] buf = new byte[1];

            // indice
            try
            {
                indice = Convert.ToUInt16(textBox_controleVagasID_indice.Text);
            }
            catch
            {
                MessageBox.Show("Valor inválido: indice");
                return;
            }


            if (operacao == controladoraLinear.OP_ESCRITA)
            {
                // total vagas
                try
                {
                    totalVagas = Convert.ToByte(textBox_controleVagasID_totalVagas.Text);
                }
                catch
                {
                    MessageBox.Show("Valor inválido: Qtd. Total vagas ('0' a '255'");
                    return;
                }

                // atual vagas
                try
                {
                    atualVagas = Convert.ToByte(textBox_controleVagasID_atualVagas.Text);
                }
                catch
                {
                    MessageBox.Show("Valor inválido: Qtd. Atual Vagas ('0' a '255'");
                    return;
                }

                

                buf = pControl.operacaoControleVagasID(operacao, indice, id, totalVagas, atualVagas);
            }
            else if( operacao == controladoraLinear.OP_LEITURA )
            {
                buf = pControl.operacaoControleVagasID(operacao, indice, id, totalVagas, atualVagas);
            }
            enviarMensagem(buf);

        }

        private void comboBox_controleVagas_vagasId_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_controleVagas_vagasId.SelectedIndex != 0)
            {
                textBox_controleVagasID_id.Enabled = true;
                textBox_controleVagasID_totalVagas.Enabled = true;
                textBox_controleVagasID_atualVagas.Enabled = true;
            }
            else
            {
                textBox_controleVagasID_id.Enabled = false;
                textBox_controleVagasID_totalVagas.Enabled = false;
                textBox_controleVagasID_atualVagas.Enabled = false;
            }
        }

        private void listView_controleVagasId_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView_controleVagasId.Items.Count; i++)
            {
                if (listView_controleVagasId.Items[i].Selected)
                {
                    ListViewItem item = listView_controleVagasId.Items[i];

                    textBox_controleVagasID_indice.Text = item.SubItems[0].Text;
                    textBox_controleVagasID_id.Text = item.SubItems[1].Text;
                    textBox_controleVagasID_totalVagas.Text = item.SubItems[2].Text;
                    textBox_controleVagasID_atualVagas.Text = item.SubItems[3].Text;


                    break;
                }
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void button_controleVagas_inicializarVagasID_Click(object sender, EventArgs e)
        {
            UInt16 indice = 0;
            // indice
            try
            {
                indice = Convert.ToUInt16(textBox_controleVagasID_indice.Text);
            }
            catch
            {
                MessageBox.Show("Valor inválido: indice");
                return;
            }

            byte[] buf = pControl.inicializarVagasId(indice, checkBox_controleVagasId_totalVagas.Checked);
            enviarMensagem(buf);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void button_eventos_cancelarPanico_Click(object sender, EventArgs e)
        {
            byte[] buf = pControl.cancelarPanico( (byte)comboBox_eventos_saidaCancelarPanico.SelectedIndex );
            enviarMensagem(buf);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            txtTempoAntipassback.Text = textBox_setup_setupRS485_antipassback.Text;
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void checkBox_setup_setupReles_ev2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox_setup_setupReles_ev8.Enabled = true;
            checkBox_setup_setupReles_ev9.Enabled = true;
            checkBox_setup_setupReles_ev19.Enabled = true;

            if( checkBox_setup_setupReles_ev2.Checked )
            {
                checkBox_setup_setupReles_ev8.Enabled = false;
                checkBox_setup_setupReles_ev9.Enabled = false;
                checkBox_setup_setupReles_ev19.Enabled = false;
            }            

        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private void btAtualizarAntipassbackTodos_Click(object sender, EventArgs e)
        {
            try
            {
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte[] buf = pControl.atualizaAntipassback_e_nivel( nivel, antipassback);
                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public Socket listener;
        public void StartListening()
        {
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            }
            catch
            {
                Invoke((MethodInvoker)delegate
                {
                    label_setup_setupTcpIp_clientStatus.Text = "Client: Aguarde! Conectando...";
                });
                toutTcpClient_startListen = 10;
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public StateObject state = new StateObject();
        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            state = new StateObject();
            state.workSocket = handler;

            Invoke((MethodInvoker)delegate
            {
                txtBox_IP1.Text = state.workSocket.RemoteEndPoint.ToString().Remove(state.workSocket.RemoteEndPoint.ToString().IndexOf(':'));
            });

            Invoke((MethodInvoker)delegate
            {
                button_setup_setupTcpIp_desconectarClient.Text = "Desconectar Client";
                label_setup_setupTcpIp_clientStatus.Text = "Client: CONECTADO => " + state.workSocket.RemoteEndPoint.ToString();
                label_setup_setupTcpIp_clientStatus.ForeColor = Color.Green;
            });

            byte[] buf = pControl.lerSetup();
            enviarMensagem(buf);

            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  new AsyncCallback(ReadCallback), state);
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch
            {
                return;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        mostraResposta( state.buffer, "TCP CLIENT", state.workSocket.RemoteEndPoint.ToString() );
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                

                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }
        
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private static void Send(Socket handler, byte[] byteData )
        {
            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler );
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        //--------------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------------
        public IPEndPoint localEndPoint;
        private void button_setup_setupTcpIp_desconectarClient_Click(object sender, EventArgs e)
        {
            if( button_setup_setupTcpIp_desconectarClient.Text == "Aceitar Client" )
            {                
                localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToUInt16(txtPorta7_SETUP_TCPIP.Text));
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    toutTcpClient_startListen = 10;
                    label_setup_setupTcpIp_clientStatus.Text = "Client: Aguarde! Conectando...";
                }
                catch 
                {
                    MessageBox.Show("Aguarde. Endpoint ocupado!!");
                }
            }
            else
            {
                button_setup_setupTcpIp_desconectarClient.Text = "Aceitar Client";

                try
                {                    
                    listener.Close();
                }
                catch { }
                try
                {
                    state.workSocket.Close();
                }
                catch { }
                toutTcpClient_startListen = 0;
                label_setup_setupTcpIp_clientStatus.Text = "Client: DESCONECTADO";
                label_setup_setupTcpIp_clientStatus.ForeColor = Color.Red;
            }            
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void button_atualizarPortasUsb_Click(object sender, EventArgs e)
        {
            if( btConectarSerial.Text != "Desconectar" )
            {
                cboxPorta.Items.Clear();
                verificaPortaSerialDisponivel();
                cboxPorta.SelectedIndex = 0;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void button_deviceManager_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("devmgmt.msc");  
        }
        //-----------------------------------------------------------------------------
        //
        //-----------------------------------------------------------------------------
        private void cboxTipoDispositivo_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*
            0 Controle - TX (1)
            1 Tag Ativo -TA (2)
            2 Cartão - CT (3)
            3 Biometria - BM (5)
            4 Tag Passivo - TP (6)
            5 Senha - SN (7)
             */
            byte k = Convert.ToByte(e.KeyChar.ToString());
            if ((k >= 1) && (k != 4) && (k <= 7))
            {
                switch (k)
                {
                    default:
                    case 1: cboxTipoDispositivo.SelectedIndex = 0; break;
                    case 2: cboxTipoDispositivo.SelectedIndex = 1; break;
                    case 3: cboxTipoDispositivo.SelectedIndex = 2; break;
                    case 5: cboxTipoDispositivo.SelectedIndex = 3; break;
                    case 6: cboxTipoDispositivo.SelectedIndex = 4; break;
                    case 7: cboxTipoDispositivo.SelectedIndex = 5; break;
                }
                txtSerial.Text = null;
                txtContadorHCS.Text = null;
            }
            k++;
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte k = Convert.ToByte(e.KeyChar.ToString());
            if ((k >= 1) && (k != 4) && (k <= 7))
            {
                switch (k)
                {
                    default:
                    case 1: cboxTipoDispositivo.SelectedIndex = 0; break;
                    case 2: cboxTipoDispositivo.SelectedIndex = 1; break;
                    case 3: cboxTipoDispositivo.SelectedIndex = 2; break;
                    case 5: cboxTipoDispositivo.SelectedIndex = 3; break;
                    case 6: cboxTipoDispositivo.SelectedIndex = 4; break;
                    case 7: cboxTipoDispositivo.SelectedIndex = 5; break;
                }
                txtSerial.Focus();
                txtSerial.Text = null;
                txtContadorHCS.Text = null;
            }
            k++;
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl3.SelectedIndex == 1)
            {
                cboxTipoDispositivo.SelectedIndex = 3;

                label_serialHexa.Text = "ID(hexa):";
                cboxTipoDispositivo.Enabled = false;
                txtContadorHCS.Enabled = false;
                txtContadorHCS.Text = "0000";
            }
            else
            {
                label_serialHexa.Text = "serial(hexa):";
                txtContadorHCS.Enabled = true;
                cboxTipoDispositivo.Enabled = true;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void tabPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl3.SelectedIndex == 1)
            {
                label_serialHexa.Text = "ID(hexa):";
                cboxTipoDispositivo.SelectedIndex = 3;
                cboxTipoDispositivo.Enabled = false;
                txtContadorHCS.Enabled = false;
                txtContadorHCS.Text = "0000";
            }
            else
            {
                if (tabControl3.SelectedIndex == 1)
                {
                    cboxTipoDispositivo.SelectedIndex = 3;

                    label_serialHexa.Text = "ID(hexa):";
                    cboxTipoDispositivo.Enabled = false;
                    txtContadorHCS.Enabled = false;
                    txtContadorHCS.Text = "0000";
                }
                else
                {
                    label_serialHexa.Text = "serial(hexa):";
                    txtContadorHCS.Enabled = true;
                    cboxTipoDispositivo.Enabled = true;
                }
            }
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void cboxTipoDispositivo_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cboxTipoDispositivo.SelectedIndex > 0)
            {
                txtContadorHCS.Enabled = false;
                txtContadorHCS.Text = "0000";
                if ((cboxTipoDispositivo.SelectedIndex == 5) && (tabControl3.SelectedIndex == 0))
                {
                    label_serialHexa.Text = "senha(hexa):";
                }
                else
                {
                    label_serialHexa.Text = "serial(hexa):";
                }
            }
            else
            {
                txtContadorHCS.Enabled = true;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        private void button_desktopReader_Click(object sender, EventArgs e)
        {
            cboxTipoDispositivo.Focus();
        }
        //-----------------------------------------------------------------------------
        //
        //---------------------------------------------------------------------------
        public static TecladoSenha form_senha = new TecladoSenha();
        private void txtSerial_Click(object sender, EventArgs e)
        {
            if (cboxTipoDispositivo.SelectedIndex == 5) // senha
            {
                try
                {
                    if (form_senha.IsHandleCreated == false)
                    {
                        form_senha = new TecladoSenha();
                        ControleLinearHCS.form_senha.evento_close += new TecladoSenha.Evento_TecladoSenhaClose( tecladoSenha_close );
                        form_senha.checkBox_setup_setupGeral_senha13.Checked = checkBox_setup_setupGeral_senha13.Checked;
                        form_senha.Show();    
                        
                    }
                    else
                    {
                        form_senha.BringToFront();
                    }
                }
                catch { }
            }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void tecladoSenha_close(string val)
        {
            txtSerial.Text = val;
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void button_carregarPadraoFabrica_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] ipBuf = new byte[4];
                byte[] buf = new byte[384];
                memoriaPlaca.setup = pControl.vetorParaSetup(buf);



                ipBuf = IPAddress.Parse("127.0.0.1").GetAddressBytes();
                memoriaPlaca.setup.ip = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipBuf = IPAddress.Parse("192.168.1.1").GetAddressBytes(); 
                memoriaPlaca.setup.gw = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipBuf = IPAddress.Parse("255.255.255.0").GetAddressBytes(); 
                memoriaPlaca.setup.mask = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                ipBuf = IPAddress.Parse("0.0.0.0").GetAddressBytes(); 
                memoriaPlaca.setup.ipDestino = (uint)((ipBuf[0] << 24) + (ipBuf[1] << 16) + (ipBuf[2] << 8) + ipBuf[3]);

                memoriaPlaca.setup.cfge.dhcp = 1;

                convertStringParaByteArray( ref memoriaPlaca.setup.dnsHost, "CONTROLADOR", "CONTROLADOR".Length );
                convertStringParaByteArray(ref memoriaPlaca.setup.usuarioLogin, "admin", "admin".Length);
                convertStringParaByteArray(ref memoriaPlaca.setup.senhaLogin, "linear", "linear".Length);


                memoriaPlaca.setup.porta1 = 9761;
                memoriaPlaca.setup.porta2 = 9762;
                memoriaPlaca.setup.porta3 = 9763;
                memoriaPlaca.setup.porta4 = 9764;
                memoriaPlaca.setup.porta5 = 9765;
                memoriaPlaca.setup.porta6 = 9766;
                memoriaPlaca.setup.porta7 = 9767;
                memoriaPlaca.setup.portaHttp = 80;

                memoriaPlaca.setup.modo = 1;

                memoriaPlaca.setup.cfg1.brCan = 2;

                memoriaPlaca.setup.cfg4.tipoDispLeitora1 = controladoraLinear.DISP_CT;
                memoriaPlaca.setup.cfg4.tipoDispLeitora2 = controladoraLinear.DISP_CT;
                memoriaPlaca.setup.cfg5.tipoDispLeitora3 = controladoraLinear.DISP_CT;
                memoriaPlaca.setup.cfg5.tipoDispLeitora4 = controladoraLinear.DISP_CT;


                memoriaPlaca.setup.tRele[0] = 2;
                memoriaPlaca.setup.tRele[1] = 2;
                memoriaPlaca.setup.tRele[2] = 2;
                memoriaPlaca.setup.tRele[3] = 2;
                memoriaPlaca.setup.tRele5 = 2;
                memoriaPlaca.setup.tRele6 = 2;


                memoriaPlaca.setup.parametro_01_Leitora[0] = 150;
                memoriaPlaca.setup.parametro_01_Leitora[1] = 150;
                memoriaPlaca.setup.parametro_01_Leitora[2] = 150;
                memoriaPlaca.setup.parametro_01_Leitora[3] = 150;

                memoriaPlaca.setup.tempoPassagem = 5;
                memoriaPlaca.setup.tempoLeituraCofre = 5;

                memoriaPlaca.setup.cfg14.wiegand34_UHF = 1;

                checkBox_setup_setupReles_ev2.Checked = false;
                checkBox_setup_setupReles_ev8.Enabled = true;
                checkBox_setup_setupReles_ev9.Enabled = true;
                checkBox_setup_setupReles_ev19.Enabled = true;

                leSetupForm();
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void comboBox_setup_setupCatraca_tipoCatraca_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_setup_setupCatraca_tipoCatraca.SelectedIndex)
            {
                case 0: pictureBox1.Image = Properties.Resources.CATRACA_TORNIQUETE; break;
                case 1: pictureBox1.Image = Properties.Resources.CATRACA_CADEIRANTE_HENRY_LUMEN; break;
                case 2: pictureBox1.Image = Properties.Resources.CATRACA_CADEIRANTE_FLEX; break;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void comboBox_setup_setupCatraca_tipoCatraca_MouseClick(object sender, MouseEventArgs e)
        {

        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void button_cadastro_biometria_help_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hamster UBio: Apresenta problemas com Windows 8."
                             + " Caso não efetue a captura da digital, feche o programa e execute-o como \"Administrador\".",
                             "Importante",
		                     MessageBoxButtons.OK,
		                     MessageBoxIcon.Warning                        
                             );
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public static Sniffer form_sniffer = new Sniffer();
        public static RichTextBox rt_sniffer;
        private void button_sniffer_Click(object sender, EventArgs e)
        {
            if (form_sniffer.IsHandleCreated == false)
            {
                rt_sniffer = new RichTextBox();
                form_sniffer = new Sniffer();
                form_sniffer.Name = "sniffer";
                form_sniffer.Width = this.Width;
                form_sniffer.Height = 225;
                form_sniffer.Location = new Point(this.Location.X, this.Height + 200);
                form_sniffer.Show();

            }
            else
            {
                form_sniffer.BringToFront();
            }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void buttonMarcarPacoteEventoIndexadoComoLido_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.marcarPacoteEventoIndexadoComoLido((UInt16)comboBoxUltimoEnderecoEventoRecebido.SelectedIndex);
                enviarMensagem(buf);
            }
            catch
            {
                MessageBox.Show("Indice inválido!");
            }
            
        }        
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void buttonTurnoJornadaFeriados_EditarTurnos2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] turnos = new byte[16];
                turnos[0] = (byte)int.Parse(txtT1HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[1] = (byte)int.Parse(txtT1MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[2] = (byte)int.Parse(txtT1HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[3] = (byte)int.Parse(txtT1MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[4] = (byte)int.Parse(txtT2HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[5] = (byte)int.Parse(txtT2MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[6] = (byte)int.Parse(txtT2HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[7] = (byte)int.Parse(txtT2MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[8] = (byte)int.Parse(txtT3HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[9] = (byte)int.Parse(txtT3MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[10] = (byte)int.Parse(txtT3HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[11] = (byte)int.Parse(txtT3MF.Text, System.Globalization.NumberStyles.Integer);
                turnos[12] = (byte)int.Parse(txtT4HI.Text, System.Globalization.NumberStyles.Integer);
                turnos[13] = (byte)int.Parse(txtT4MI.Text, System.Globalization.NumberStyles.Integer);
                turnos[14] = (byte)int.Parse(txtT4HF.Text, System.Globalization.NumberStyles.Integer);
                turnos[15] = (byte)int.Parse(txtT4MF.Text, System.Globalization.NumberStyles.Integer);

                byte aux = 0;
                if (checkBox_TurnoJornadaFeriados_entradaTurno1.Checked) aux |= 0x01;
                if (checkBox_TurnoJornadaFeriados_saidaTurno1.Checked) aux |= 0x02;

                if (checkBox_TurnoJornadaFeriados_entradaTurno2.Checked) aux |= 0x04;
                if (checkBox_TurnoJornadaFeriados_saidaTurno2.Checked) aux |= 0x08;
                
                if (checkBox_TurnoJornadaFeriados_entradaTurno3.Checked) aux |= 0x10;
                if (checkBox_TurnoJornadaFeriados_saidaTurno3.Checked) aux |= 0x20;
                
                if (checkBox_TurnoJornadaFeriados_entradaTurno4.Checked) aux |= 0x40;
                if (checkBox_TurnoJornadaFeriados_saidaTurno4.Checked) aux |= 0x80;

                byte[] buf = pControl.editarTurnos2(cboxCodigoTurno.SelectedIndex, turnos, aux );

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void buttonTurnoJornadaFeriados_LerTurnos2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.lerTurnos2(cboxCodigoTurno.SelectedIndex);
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void btOperacoesBiometria3_Click(object sender, EventArgs e)
        {
            try
            {
                String t = rTxtTemplate.Text.Replace(" ", "");

                byte[] template = HexStringToBytes(t);
                UInt16 tamanhoTemplate = (UInt16)template.Length;
                if (tamanhoTemplate == 0)
                {
                    MessageBox.Show("Nenhum biometria! Habilite a opção Enviar template não cadastrado.");
                    return;
                }

                if ((cboxModuloBio.SelectedIndex == 1) && (tamanhoTemplate == 400)) // Virdi
                {
                    Array.Resize<byte>(ref template, 800);
                    for (int i = 0; i < tamanhoTemplate; i++)
                    {
                        template[i + 400] = template[i];
                    }
                    tamanhoTemplate += 400;
                }


                /*
                0 OP_ESCRITA(0) = CMD 106
                1 OP_EDICAO (1) = CMD 106
                2 OP_RESTORE(2) = CMD 106
                3 OP_APAGAR(4) = CMD 107
                4 OP_APAGAR_TODOS_OS_TEMPLATES(10) = CMD 107
                5 OP_VERIFICAR_ID_GRAVADO(11) = CMD 107
                6 OP_SOLICITAR_ID_VAGO(12) = CMD 107
                7 OP_IDENTIFICAR_TEMPLATE(13) = CMD 106
                */
                byte operacao = 0;
                switch (cboxOperacoesBM1.SelectedIndex)
                {
                    case 0: operacao = controladoraLinear.OP_ESCRITA; break;
                    case 1: operacao = controladoraLinear.OP_EDICAO; break;
                    case 2: operacao = controladoraLinear.OP_RESTORE; break;
                    case 7: operacao = controladoraLinear.OP_IDENTIFICAR_TEMPLATE; break;
                    default: return;
                }

                if (tamanhoTemplate == 0)
                {
                    lblErro.Text = "Carregar Template!!";
                    return;
                }


                byte tipo = (byte)cboxTipoDispositivo.SelectedIndex;
                UInt64 serial = UInt64.Parse(txtSerial.Text, System.Globalization.NumberStyles.HexNumber);

                UInt16 contadorHCS = Convert.ToUInt16(txtContadorHCS.Text);
                int codHabilitacao = cboxRota.SelectedIndex; // rota
                byte antipassback = (byte)cboxAntipassback.SelectedIndex;
                byte saidaCofre = 0;
                if (ckBoxSaidaCofre.Checked == true) saidaCofre = 1;
                byte visitante = 0;
                if (checkBoxDispositivos_visitante.Checked == true) visitante = 1;
                byte controleVagasId = 0;
                if (checkBoxDispositivos_controleVagasId.Checked) controleVagasId = 1;
                byte duplaValidacao = 0;
                if (checkBoxDispositivos_duplaValidacao.Checked) duplaValidacao = 1;
                byte panico = 0;
                if (checkBoxDispositivos_panico.Checked) panico = 1;

                DateTime dataIni = dateTimePicker1.Value;
                DateTime dataFim = dateTimePicker2.Value;
                byte nivel = Convert.ToByte(txtNivel.Text);
                byte creditos = Convert.ToByte(txtCréditos.Text);
                StringBuilder labelUsuario = new StringBuilder(14);


                controladoraLinear.flagsCadastro fCadastro = new controladoraLinear.flagsCadastro(saidaCofre, antipassback, visitante, controleVagasId, duplaValidacao, panico );
                controladoraLinear.flagsStatus fStatus = new controladoraLinear.flagsStatus(0, 1); // bateria = ok(0), última saída acionada = 1
                controladoraLinear.s_validade validade = new controladoraLinear.s_validade((byte)dataIni.Day, (byte)dataIni.Month, dataIni.Year, (byte)dataFim.Day, (byte)dataFim.Month, dataFim.Year);

                for (int i = 0; i < 14; i++)
                {
                    if (i < txtIdentificacao.TextLength)
                    {
                        labelUsuario.Append(txtIdentificacao.Text[i]);
                    }
                    else
                    {
                        labelUsuario.Append(' ');
                    }
                }

                tipo = controladoraLinear.DISP_BM;

                /*
                #define TIPO_BIOMETRIA_SEM_BIOMETRIA		0
                #define TIPO_BIOMETRIA_MIAXIS				1
                #define TIPO_BIOMETRIA_VIRDI				2
                #define TIPO_BIOMETRIA_SUPREMA				3
                #define TIPO_BIOMETRIA_NITGEN				4
                #define TIPO_BIOMETRIA_ANVIZ				5
                #define TIPO_BIOMETRIA_LN3000               7
                 * 
                    MIAXIS(1)
                    VIRDI(2)
                    SUPREMA(3)
                    NITGEN(4) // não implementado
                    ANVIZ(5)
                    LN3000(7)
                */
                byte tipoBiometria = 0;
                switch (cboxModuloBio.SelectedIndex)
                {
                    case 0: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_MIAXIS; break;
                    case 1: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_VIRDI; break;
                    case 2: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_SUPREMA; break;
                    case 3: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_NITGEN; break;
                    case 4: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_ANVIZ; break;
                    case 5: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_T5S; break;
                    case 6: tipoBiometria = controladoraLinear.TIPO_BIOMETRIA_LN3000; break;
                }

                if (tipoBiometria == 0) return;

                byte[] frameDisp = pControl.dispositivoParaVetor(tipo, serial, contadorHCS, codHabilitacao, fCadastro, fStatus, nivel, creditos, validade, labelUsuario.ToString());

                byte[] buf = pControl.operacoesBiometria3(operacao, (byte)cboxIndiceBM.SelectedIndex, tipoBiometria, frameDisp, tamanhoTemplate, template);

                enviarMensagem(buf);
            }
            catch { MessageBox.Show("Valor inválido!!"); }
        }

        private void checkBox_setup_controleEntradaSaidaTurnos_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_setup_controleEntradaSaidaTurnos.Checked == false )
            {
                checkBox_TurnoJornadaFeriados_entradaTurno1.Enabled = false;
                checkBox_TurnoJornadaFeriados_saidaTurno1.Enabled = false;
                checkBox_TurnoJornadaFeriados_entradaTurno2.Enabled = false;
                checkBox_TurnoJornadaFeriados_saidaTurno2.Enabled = false;
                checkBox_TurnoJornadaFeriados_entradaTurno3.Enabled = false;
                checkBox_TurnoJornadaFeriados_saidaTurno3.Enabled = false;
                checkBox_TurnoJornadaFeriados_entradaTurno4.Enabled = false;
                checkBox_TurnoJornadaFeriados_saidaTurno4.Enabled = false;
            }
            else
            {
                checkBox_TurnoJornadaFeriados_entradaTurno1.Enabled = true;
                checkBox_TurnoJornadaFeriados_saidaTurno1.Enabled = true;
                checkBox_TurnoJornadaFeriados_entradaTurno2.Enabled = true;
                checkBox_TurnoJornadaFeriados_saidaTurno2.Enabled = true;
                checkBox_TurnoJornadaFeriados_entradaTurno3.Enabled = true;
                checkBox_TurnoJornadaFeriados_saidaTurno3.Enabled = true;
                checkBox_TurnoJornadaFeriados_entradaTurno4.Enabled = true;
                checkBox_TurnoJornadaFeriados_saidaTurno4.Enabled = true;
            }
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void btOperacaoPacoteTurnos2_Click(object sender, EventArgs e)
        {
            operacaoPacoteClick(controladoraLinear.FILE_TURNOS, "TURNO2.TXT");
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public byte[] md5_calc;
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void btnOperacaoAjusteRelogio_Click(object sender, EventArgs e)
        {
            try
            {
                byte operacao = controladoraLinear.OP_ESCRITA;
                double val = (byte)trackBarAjusteRelogio.Value;
                byte ajuste = 0;

                val = 127 - val;

                // 128 - 0   ->   128
                // 128 - 255 ->  -127

                ajuste = (byte)(val);

                if (cBoxAjusteRelogio.SelectedIndex == 0) operacao = controladoraLinear.OP_LEITURA;

                byte[] buf = pControl.operacaoAjusteRelogio(operacao, (byte)ajuste);

                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void trackBarAjusteRelogio_Scroll(object sender, EventArgs e)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    lblAjusteRelogio.Text = (127 - trackBarAjusteRelogio.Value).ToString();
                });
            }
            catch
            {
                
            }            
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void trackBarAjusteRelogio_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    lblAjusteRelogio.Text = (127 - trackBarAjusteRelogio.Value).ToString();
                });
            }
            catch
            {

            }
        }


        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        public bool enviandoArquivo = false;
        public int tamanhoEnviando = 0;
        public byte[] arquivoEnviando;
        public byte[] pathEnviando = new byte[54];
        public byte midiaEnviando = controladoraLinear.MIDIA_SD;


        private void button_SDCARD_MIDIA_arquivoDados_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog1.DefaultExt = "*.*";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.FileName = "*.*";

                

                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BinaryReader b = new BinaryReader(File.Open(openFileDialog1.FileName, FileMode.Open));
                    
                    int length = (int)b.BaseStream.Length;
                    arquivoEnviando = b.ReadBytes(length);
                    
                    
                    richTextBox_SDCARD_MIDIA_dados.Text = arquivoEnviando.ToString();
                    label_SDCARD_MIDIA_dados.Text = "Dados: " + arquivoEnviando.Length.ToString() + " bytes";
                    b.Close();

                    string filename = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf('\\') + 1); ;
                    tamanhoEnviando = arquivoEnviando.Length;
                    progressBar_SDCARD_MIDIA_enviandoArquivo.Value = 0;
                    progressBar_SDCARD_MIDIA_enviandoArquivo.Step = tamanhoEnviando / 1024;
                    progressBar_SDCARD_MIDIA_enviandoArquivo.Maximum = tamanhoEnviando;
                    progressBar_SDCARD_MIDIA_enviandoArquivo.Visible = true;
                    enviandoArquivo = true;

                    byte[] dados = new byte[1024];
                    midiaEnviando = controladoraLinear.MIDIA_SD;
                    if (radioButtonSDCARD_MIDIA_pendrive.Checked) midiaEnviando = controladoraLinear.MIDIA_USB;
                    
                    char c = diretorioAtual[diretorioAtual.Length - 1];
                    textBox_SDCARD_MIDIA_nome.Text = diretorioAtual;
                    if (c != (char)'/')
                    {
                        textBox_SDCARD_MIDIA_nome.Text += "/";
                    }

                    textBox_SDCARD_MIDIA_nome.Text += filename;

                    for (int i = 0; i < 54; i++)
                    {
                        pathEnviando[i] = 0;
                        if (textBox_SDCARD_MIDIA_nome.Text.Length > i)
                        {
                            pathEnviando[i] = (byte)textBox_SDCARD_MIDIA_nome.Text[i];
                        }
                    }

                    byte[] buf = pControl.lerEditarMidia(controladoraLinear.OP_CRIAR_ABRIR_ARQUIVO_ESCRITA, midiaEnviando, pathEnviando, 0, dados);
                    enviarMensagem(buf);
                }
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------
        private void button_restoreBiometriaSdcard_Click(object sender, EventArgs e)
        {
            byte[] filename = new byte[13];

            if (textBox_SDCARD_MIDIA_nomeArquivoRestore.Text == null)
            {
                MessageBox.Show("Nenhum arquivo selecionado!");
                //return;
            }

            for (int i = 0; i < 13; i++)
            {
                if (i < textBox_SDCARD_MIDIA_nomeArquivoRestore.Text.Length)
                {
                    filename[i] = (byte)textBox_SDCARD_MIDIA_nomeArquivoRestore.Text[i];
                }
                else break;
            }

            byte[] buf = pControl.restoreBiomtriasSdcard( filename );
            enviarMensagem(buf);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buf = pControl.atualizaCreditoContagem();
                enviarMensagem(buf);
            }
            catch { };
        }
        //-----------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------

       

    }
    //-----------------------------------------------------------------------------
    //
    //--------------------------------------------------------------------------
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
