using System;
using System.Windows.Forms;
using smart.info;
using smart.monitor.pedestre;
using smart.monitor.veiculos;

namespace smart.monitor
{
    internal class AppMenu
    {
        FormSobre _formsobre;
        
        bool _formMonitorPedestres = false;
        bool _formMonitorVeiculos = false;
        bool _formconfigacionado = false;
        bool _formsobreacionado = false;
        

        public AppMenu()
        {
        }

        internal ContextMenuStrip Montar()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            // Monta um item do menu para cada plugin carregado
            foreach (var plugin in Program.Plugins)
            {
                ISmartPlugins item = Program.Plugins[plugin.Key];

                ToolStripMenuItem itemmenu = new ToolStripMenuItem
                {
                    Text = item.Titulo,
                    Image = item.Icone
                };

                itemmenu.Click += (object sender, EventArgs e) =>
                {
                    item.Exibir();
                };
                menu.Items.Add(itemmenu);

            }


            // Monitor de pessoas
            ToolStripMenuItem monitor_pedestres = new ToolStripMenuItem
            {
                Text = "Monitor de pedestres",
                Image = Properties.Resources.monitor_pedestre1
            };
            monitor_pedestres.Click += Monitor_pedestres_Click; ;
            menu.Items.Add(monitor_pedestres);

            // Monitor de veiculos
            ToolStripMenuItem monitor_veiculos = new ToolStripMenuItem
            {
                Text = "Monitor de veículos",
                Image = Properties.Resources.monitor_veiculos1
            };
            monitor_veiculos.Click += Monitor_veiculos_Click;
            menu.Items.Add(monitor_veiculos);


            // Adiciona um separador
            menu.Items.Add(new ToolStripSeparator());

            // Configurações
            ToolStripMenuItem configuracoes = new ToolStripMenuItem
            {
                Text = "Configurações",
                Image = Properties.Resources.info1
            };
            configuracoes.Click += Configuracoes_Click;
            menu.Items.Add(configuracoes);

            // Mostra o formulário sobre
            ToolStripMenuItem sobre = new ToolStripMenuItem
            {
                Text = "Sobre",
                Image = Properties.Resources.info1
            };
            sobre.Click += Sobre_Click;
            menu.Items.Add(sobre);

            // Adiciona um separador
            menu.Items.Add(new ToolStripSeparator());

            // Finaliza o sistema
            ToolStripMenuItem sair = new ToolStripMenuItem
            {
                Text = "Sair",
                Image = Properties.Resources.sair1
            };
            sair.Click += Sair_Click;
            menu.Items.Add(sair);

            return menu;
        }

        private void Monitor_veiculos_Click(object sender, EventArgs e)
        {
            if (_formMonitorVeiculos == false)
            {
                Program.MonitorVeiculos = new MonitorVeiculos();
                Program.MonitorVeiculos.FormClosed += (object fcsender, FormClosedEventArgs fce) => { _formMonitorVeiculos = false; };
                Program.MonitorVeiculos.Show();
                _formMonitorVeiculos = true;
            }
        }
        
        private void Monitor_pedestres_Click(object sender, EventArgs e)
        {
            if (_formMonitorPedestres == false)
            {
                Program.MonitorPedestres = new MonitorPedestres();
                Program.MonitorPedestres.FormClosed += (object fcsender, FormClosedEventArgs fce) => { _formMonitorPedestres = false; };
                Program.MonitorPedestres.Show();
                _formMonitorPedestres = true;
            }
        }

        private void Configuracoes_Click(object sender, EventArgs e)
        {
            if (_formconfigacionado == false)
            {
                Program.Config = new FormConfig();
                Program.Config.FormClosed += (object fcsender, FormClosedEventArgs fce) => { _formconfigacionado = false; };
                Program.Config.Show();
                _formconfigacionado = true;
            }
        }

        private void Sobre_Click(object sender, EventArgs e)
        {
            if (_formsobreacionado == false)
            {
                _formsobre = new FormSobre();
                _formsobre.FormClosed += (object fcsender, FormClosedEventArgs fce) => { _formsobreacionado = false; };
                _formsobre.Show();
                _formsobreacionado = true;
            }
        }

        private void Sair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}