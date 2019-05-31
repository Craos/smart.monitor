using AmRoNetworkMonitor;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace linear.core
{
    public class ConexaoTCP : TcpClient
    {
        public delegate void AoReceberInfo(byte[] buf, string tipoPorta, string epOrigem);
        public event AoReceberInfo EventoAoReceberInfo;

        public delegate void AoFalharRede(string mensagem);
        public event AoFalharRede EventoAoFalharRede;

        public string Endereco { get; set; }
        public int Porta { get; set; }

        public bool State
        {
            get
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(
                    Client.LocalEndPoint) && x.RemoteEndPoint.Equals(Client.RemoteEndPoint)).ToArray();

                if (tcpConnections != null && tcpConnections.Length > 0 && tcpConnections.First().State == TcpState.Established)
                    return true;

                return false;
            }
        }

        public ConexaoTCP(AddressFamily family) : base(family)
        {
        }

        public void Connect()
        {
            if (TestedeRedeLocal() == false)
            {
                EventoAoFalharRede("Falha de rede do computador local");
                return;
            }
            
            if (TestedeHostRemoto() == false)
            {
                EventoAoFalharRede("Falha de ping. Não foi possível obter uma resposta do host remoto.");
                return;
            }
            
            var waititem = BeginConnect(IPAddress.Parse(Endereco), Convert.ToUInt16(Porta), new AsyncCallback(ConnectCallback), this);
            var sucess = waititem.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
        }

        private void ConnectCallback(IAsyncResult result)
        {
            if (Connected)
            {
                NetworkStream networkStream = GetStream();
                byte[] buffer = new byte[ReceiveBufferSize];
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            }

        }

        private void ReadCallback(IAsyncResult result)
        {
            NetworkStream networkStream = null;

            if (Connected)
                networkStream = GetStream();

            byte[] buffer = result.AsyncState as byte[];
            int nBytesRead = 0;
            try {
                if (Connected)
                    nBytesRead = networkStream.EndRead(result);
            }
            catch (IOException ioe)
            {
                if (ioe.InnerException != null)
                {
                    if (ioe.InnerException is SocketException se)
                    {
                        if (se.SocketErrorCode == SocketError.ConnectionReset)
                        {
                            EventoAoFalharRede("Client closed connection!");
                            Connect();
                            return;
                        }
                    }

                    if (ioe.InnerException is ObjectDisposedException ode)
                    {
                        EventoAoFalharRede("Client closed connection.");
                        Connect();
                        return;
                    }
                }
                throw ioe;
            }
            catch (Exception ex)
            {
                EventoAoFalharRede("Error in readcallback: " + ex.ToString());
            }

            byte[] buf = new byte[nBytesRead];

            for (int i = 0; i < nBytesRead; i++)
                buf[i] = buffer[i];

            if (Client.Connected == true)
            {
                EventoAoReceberInfo(buf, "TCP SERVER", Client.RemoteEndPoint.ToString());

                while (networkStream.DataAvailable)
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
            }

        }

        private bool TestedeRedeLocal()
        {
            NetworkMonitor.StateChanged += (object sender, StateChangeEventArgs e) => {
                if (e.IsAvailable == false)
                   EventoAoFalharRede("A conexão de rede deste computador está indisponível");
            };

            NetworkMonitor.StartMonitor();
            Thread.Sleep(1000);

            return NetworkMonitor.CurrentState;
        }

        private bool TestedeHostRemoto()
        {
            try
            {
                Ping myPing = new Ping();
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(Endereco, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
