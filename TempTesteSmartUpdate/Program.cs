using smart.transfer;
using System;
using System.Diagnostics;

namespace TempTesteSmartUpdate
{
    class Program
    {
        private static int eventId = 0;
        private static EventLog eventLog1;

        static void Main(string[] args)
        {
            eventLog1 = new EventLog();
            eventLog1.Log = "Application";

            if (!EventLog.SourceExists("SmartUpdate"))
                EventLog.CreateEventSource("SmartUpdate", "Updates");

            eventLog1.Source = "SmartUpdate";

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

            Console.ReadLine();
        }

        private static void AoExecutar(string ID, string Mensagem, EventLogEntryType Tipo)
        {
            eventLog1.WriteEntry($"{ID}: {Mensagem}", Tipo, eventId++);
            Console.WriteLine($"{ID}: {Mensagem}");
        }
    }
}
