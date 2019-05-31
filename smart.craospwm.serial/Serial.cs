using LibUsbDotNet.DeviceNotify;
using smart.info;
using System;
using System.IO.Ports;

namespace smart.craospwm.serial
{

    /// <summary>
    /// Responsável pelas conexões USB das placas da Craos e Linear
    /// Faz recebe a leitura das placas e executa o tratamento caso seja necessário
    /// </summary>
    public class Serial : SerialPort
    {
        private IDeviceNotifier devNotifier;
        private const string finaldelinha = "\r\n";

        /// <summary>
        /// Recebe a string com o id do dispositivo
        /// </summary>
        string _dispositivoID = "";

        /// <summary>
        /// Utilizado para o envio do evento após a abertura da conexão
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AoAbrirEventHandler(object sender, EventArgs e);
        public event AoAbrirEventHandler AoAbrir;

        /// <summary>
        /// Utilizado para o envio do evento após a solicitação de fechamento da conexão
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AoFecharEventHandler(object sender, EventArgs e);
        public event AoFecharEventHandler AoFechar;

        /// <summary>
        /// Utilizado para o envio do evento ao finalizar o tratamento do ID do dispositivo identificado
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AoIdentificarEventHandler(object sender, EventArgs e);
        public event AoIdentificarEventHandler AoIdentificar;

        /// <summary>
        /// Utilizado para o envio do evento ao encontrar erros nas operações de abertura e identificação
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AoEncontrarErroEventHandler(object sender, EventArgs e);
        public event AoEncontrarErroEventHandler AoEncontrarErro;


        /// <summary>
        /// Utilizado para o envio do evento ao conectar o cabo USB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void AoConectarCaboUSBEventHandler(object sender, EventArgs e);
        public event AoConectarCaboUSBEventHandler AoConectarCaboUSB;


        /// <summary>
        /// Dispara o evento ao Abrir a conexão
        /// </summary>
        /// <param name="info"></param>
        public virtual void RaiseAoAbrir(string info)
        {
            AoAbrir(info, new EventArgs());
        }

        /// <summary>
        /// Dispara o evento ao Fechar a conexão
        /// </summary>
        /// <param name="info"></param>
        public virtual void RaiseAoFechar(string info)
        {
            AoFechar(info, new EventArgs());
        }

        /// <summary>
        /// Dispara o envento ao Identificar um novo dispositivo
        /// </summary>
        /// <param name="info"></param>
        public virtual void RaiseAoIdentificar(string info)
        {
            AoIdentificar(info, new EventArgs());
        }

        /// <summary>
        /// Dispara o evento ao Encontrar um novo erro
        /// </summary>
        /// <param name="info"></param>
        public virtual void RaiseAoEncontrarErro(TratamentoErros info)
        {
            AoEncontrarErro(info, new EventArgs());
        }

        /// <summary>
        /// Dispara o evento ao Conectar o cabo USB
        /// </summary>
        /// <param name="info"></param>
        public virtual void RaiseAoConectarCaboUSB(string info)
        {
            AoConectarCaboUSB(info, new EventArgs());
        }

        public HardwareModel Tipo { get; set; }

        /// <summary>
        /// Objeto construtor
        /// </summary>
        public Serial() : base()
        {
            devNotifier = DeviceNotifier.OpenDeviceNotifier();
            devNotifier.OnDeviceNotify += (object sender, DeviceNotifyEventArgs e) =>
            {
                if (e == null || e.Device == null)
                    return;

                string idportcom = PortCom.HardwareID.Split('&')[1];
                string iddevnoti = e.Device.Name.Split('#')[1].Split('&')[0];

                if (e.EventType == EventType.DeviceArrival)
                {
                    if (idportcom == iddevnoti)
                        RaiseAoConectarCaboUSB($"O equipamento {PortCom.Fabricante} {PortCom.Modelo} foi localizado\nporém não foi possível determinar se ele ainda está utilizando a mesma porta de conexão");

                    return;
                }

                if (idportcom == iddevnoti)
                {
                    TratamentoErros erro = new TratamentoErros();
                    string mensagem = $"O equipamento  {PortCom.Fabricante} {PortCom.Modelo} foi desconectado da porta {PortCom.PortName}";
                    erro.Mensagem = mensagem;
                    erro.Numero = 1;
                    erro.Detalhes = new Exception(mensagem);
                    RaiseAoEncontrarErro(erro);
                }
            };
        }

        /// <summary>
        /// Inicia uma nova conexão com a porta serial informada
        /// </summary>
        public void Abrir()
        {
            try
            {
                DataReceived += Serial_DataReceived;
                ErrorReceived += Serial_ErrorReceived;

                if (Tipo == HardwareModel.FTDIBUS)
                {
                    DataBits = Convert.ToInt32(8);
                    StopBits = StopBits.One;
                    Parity = Parity.None;
                    Handshake = Handshake.None;
                }

                Open();
                RaiseAoAbrir($"Conectado a porta serial:{PortName}");

            }
            catch (Exception e)
            {
                string message = "Não foi possível abrir a conexão serial. Verifique se existe outro aplicativo utilizando a porta de conexão";
                RaiseAoEncontrarErro(new TratamentoErros { Mensagem = message, Detalhes = e });
                return;
            }
            finally
            {
                if (Tipo == HardwareModel.FTDIBUS && IsOpen)
                {
                    RtsEnable = true;
                    DtrEnable = true;
                    WriteTimeout = 5000;
                }
            }
        }

        /// <summary>
        /// Dispara o evento ao encontrar um erro na conexão serial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            RaiseAoEncontrarErro(new TratamentoErros { Mensagem = sender });
        }

        /// <summary>
        /// Dispara o envento ao identificar a leitura do serial
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string RxString = ReadExisting();
            _dispositivoID += RxString;

            if (_dispositivoID.IndexOf(finaldelinha) > -1)
            {
                _dispositivoID = _dispositivoID.Replace(finaldelinha, "").Replace("|", "");

                if (_dispositivoID.Length < 8)
                    return;

                RaiseAoIdentificar(_dispositivoID.Substring(8 - 5).ToUpper());
                _dispositivoID = "";
            }
        }

        public void EnviarComando(string e)
        {
            if (!IsOpen) return;

            char[] buff = new char[1];

            buff[0] = e.ToCharArray()[0];

            try
            {
                Write(buff, 0, 1);
            }
            catch (Exception error)
            {
                string message = "Não foi possível enviar através da conexão serial.";
                RaiseAoEncontrarErro(new TratamentoErros { Mensagem = message, Detalhes = error });
                return;
            }
        }

        /// <summary>
        /// Fecha a conexão serial caso ela esteja aberta
        /// </summary>
        public void Finalizar()
        {
            if (IsOpen == true)
                Close();

            RaiseAoFechar("A conexão está fechada");
        }
    }
}
