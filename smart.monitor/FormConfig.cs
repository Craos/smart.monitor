using DevComponents.DotNetBar;
using DevComponents.DotNetBar.SuperGrid;
using Newtonsoft.Json;
using smart.info;
using smart.monitor;
using smart.monitor.Properties;
using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace smart
{
    public partial class FormConfig : OfficeForm
    {

        public FormConfig()
        {
            InitializeComponent();
            Shown += FormConfig_Shown;

        }

        private void FormConfig_Shown(object sender, EventArgs e)
        {
            foreach (var item in Program.Plugins)
            {
                ISmartPlugins plugin = Program.Plugins[item.Key];

                TabControlPanel tabCorpodoab = new TabControlPanel();
                TabItem tabNomedoTab = new TabItem();
                UserControl painel = plugin.PainelConfiguracoes;
                painel.Dock = DockStyle.Fill;

                // 
                // tabCorpodoab
                // 
                tabCorpodoab.DisabledBackColor = Color.Empty;
                tabCorpodoab.Dock = DockStyle.Fill;
                tabCorpodoab.Padding = new System.Windows.Forms.Padding(1);
                tabCorpodoab.TabItem = tabNomedoTab;
                tabCorpodoab.Controls.Add(painel);

                // 
                // tabNomedoTab
                // 
                tabNomedoTab.AttachedControl = tabCorpodoab;
                tabNomedoTab.Text = plugin.Titulo;

                ControleTabulacao.Controls.Add(tabCorpodoab);
                ControleTabulacao.Tabs.Add(tabNomedoTab);

            }
        }
    }
}
