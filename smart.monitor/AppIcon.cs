using System;
using System.Windows.Forms;

namespace smart.monitor
{
    internal class AppIcon : IDisposable
    {
        NotifyIcon _iconenotificador;

        public AppIcon()
        {
            _iconenotificador = new NotifyIcon();
        }

        public void Dispose()
        {
            _iconenotificador.Visible = false;
            _iconenotificador.Dispose();
            _iconenotificador = null;
        }

        internal void Exibir()
        {
            _iconenotificador.Icon = Properties.Resources.app;
            _iconenotificador.Text = "Smart Monitor";
            _iconenotificador.Visible = true;
            _iconenotificador.ContextMenuStrip = new AppMenu().Montar();
        }
    }
}