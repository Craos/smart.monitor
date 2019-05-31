using NLog;
using smart.info;
//using smart.login;
using smart.monitor.pedestre;
using smart.monitor.Properties;
using smart.monitor.veiculos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace smart.monitor
{
    static class Program
    {
        private static string appGuid = "A0695711-2B50-41D7-AAAB-9B463A274410";

        public static Dictionary<string, ISmartPlugins> Plugins { get; internal set; }
        public static FormConfig Config { get; set; }
        public static MonitorPedestres MonitorPedestres { get; set; }
        public static MonitorVeiculos MonitorVeiculos { get; set; }
        public static Logger Log { get; set; }
        // public static Login FormLogin { get; set; } = new Login(Settings.Default.OrigemLogin);

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            PreparaLog();

            if (Settings.Default.ExigirAutenticacaoAntesdeIniciar == true)
            {
                //FormLogin.FormClosed += (object sender, FormClosedEventArgs e) =>
                //{
                Iniciar();
                //};

                //FormLogin.Show();
            }

            Log.Info("Iniciando o sistema Smart Monitor");
            using (AppIcon iconebandeja = new AppIcon())
            {
                // Carrega os plugins do sistema antes de montar o menu.
                CarregaPlugins();
                Iniciar();

                // Executa o metodo iniciar contido nos plugins.
                ExecutarProcedimentoInicialdoPlugin();

                // Monta o icone na bandeja de notificação do windows
                iconebandeja.Exibir();

                // Evita que uma nova instancia seja executada
                using (Mutex mutex = new Mutex(false, "Global\\" + appGuid))
                {
                    if (!mutex.WaitOne(0, false))
                    {
                        MessageBox.Show(
                         "Uma outra instância deste aplicativo ainda está em execução", "Identificador RFID", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                         MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                        return;
                    }

                    Application.ApplicationExit += Application_ApplicationExit;

                    // Inicia o aplicativo
                    Application.Run();
                }
            }
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (Plugins != null)
            {
                foreach (var item in Plugins)
                {
                    ISmartPlugins plugin = Plugins[item.Key];
                    plugin.Finalizar();
                }
            }
        }

        private static void CarregaPlugins()
        {
            string diretorioplugins = $@"{AppDomain.CurrentDomain.BaseDirectory}\plugins\";
            string[] diretorios = Directory.GetDirectories(diretorioplugins);

            Plugins = new Dictionary<string, ISmartPlugins>();
            foreach (var diretorio in diretorios)
            {
                ICollection<ISmartPlugins> plugins = GenericPluginLoader<ISmartPlugins>.LoadPlugins(diretorio);
                foreach (var item in plugins)
                    Plugins.Add(item.Nome, item);
            }
        }

        private static void ExecutarProcedimentoInicialdoPlugin()
        {
            foreach (var item in Plugins)
            {
                ISmartPlugins plugin = Plugins[item.Key];
                plugin.Logs += (object sender, EventArgs e) => { Log.Info(sender); };
                plugin.AoIdentificarDispositivo += (object sender, EventArgs e) =>
                {
                    if (MonitorPedestres != null && MonitorPedestres.Visible == true)
                    {
                        MonitorPedestres.ExibirIdentificacao(sender);
                    }
                };
                plugin.Iniciar();
            }
        }

        private static void Iniciar()
        {
            Config = new FormConfig();

            MonitorPedestres = new MonitorPedestres();
            MonitorVeiculos = new MonitorVeiculos();

            if (Settings.Default.AbrirMonitordePedestre == true)
                MonitorPedestres.Show();

            if (Settings.Default.AbrirMonitordeVeiculos == true)
                MonitorVeiculos.Show();


        }

        private static void PreparaLog()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "Monitor.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
            Log = LogManager.GetCurrentClassLogger();
        }

        public static void ExibirPopup(string Mensagem)
        {
            var notification = new NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                // optional - BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info,
                // optional - BalloonTipTitle = "My Title",
                BalloonTipText = Mensagem,
                BalloonTipTitle = "Smart Controle de acesso",
            };

            // Display for 5 seconds.
            notification.ShowBalloonTip(5000);

            // This will let the balloon close after it's 5 second timeout
            // for demonstration purposes. Comment this out to see what happens
            // when dispose is called while a balloon is still visible.
            Thread.Sleep(10000);

            // The notification should be disposed when you don't need it anymore,
            // but doing so will immediately close the balloon if it's visible.
            notification.Dispose();
        }
    }
}
