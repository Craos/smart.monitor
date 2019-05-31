using smart.update.Properties;
using System.Diagnostics;
using System.ServiceProcess;
using smart.transfer;

namespace smart.update
{
    public partial class SmartUpdate : ServiceBase
    {
        
        private int eventId = 0;
        
        public SmartUpdate()
        {
            InitializeComponent();

            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("SmartUpdate"))
                EventLog.CreateEventSource("SmartUpdate", "Updates");

            eventLog1.Source = "SmartUpdate";
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Iniciando o serviço de atualização Smart");

            Transfer trfmoradores = new Transfer
            {
                ID = "Moradores",
                Intervalo = Settings.Default.MoradoresIntervaloDownload,
                LimiteTransferencia = Settings.Default.MoradoresLimiteDownload,
                Procedimento = Settings.Default.MoradoresProcedimentoDownload,
                StringConexaoLocal = Settings.Default.ConexaoLocal,
                StringConexaoRemota = Settings.Default.ConexaoRemota
            };

            Transfer trfveiculos = new Transfer
            {
                ID = "Veículos",
                Intervalo = Settings.Default.VeiculosIntervaloDownload,
                LimiteTransferencia = Settings.Default.VeiculosLimiteDownload,
                Procedimento = Settings.Default.VeiculosProcedimentoDownload,
                StringConexaoLocal = Settings.Default.ConexaoLocal,
                StringConexaoRemota = Settings.Default.ConexaoRemota
            };

            Transfer trfpassagens = new Transfer
            {
                ID = "Passagens",
                Intervalo = Settings.Default.UploadIntervalo,
                LimiteTransferencia = Settings.Default.UploadLimiteTransferencia,
                Procedimento = Settings.Default.UploadProcedimento,
                StringConexaoLocal = Settings.Default.ConexaoLocal,
                StringConexaoRemota = Settings.Default.ConexaoRemota
            };


            trfmoradores.AoExecutar += AoExecutar;
            trfmoradores.Iniciar();

            trfveiculos.AoExecutar += AoExecutar;
            trfveiculos.Iniciar();

            trfpassagens.AoExecutar += AoExecutar;
            trfpassagens.Iniciar();

        }

        private void AoExecutar(string ID, string Mensagem, EventLogEntryType Tipo)
        {
            eventLog1.WriteEntry($"{ID}:{Mensagem}", Tipo, eventId++);
        }

        protected override void OnStop()
        {
            Finalizar();
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("O serviço de atualização Smart irá continuar agora");
        }

        protected override void OnShutdown()
        {
            Finalizar();
            eventLog1.WriteEntry("O serviço de atualização Smart finalizado por desligamento do computador");
        }

        protected override void OnPause()
        {
            eventLog1.WriteEntry("O serviço de atualização Smart foi pausado");
        }

        private void Finalizar()
        {  
            eventLog1.WriteEntry("O serviço de atualização Smart foi finalizado");
        }
    }
}