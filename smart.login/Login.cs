using System;
using Gecko;
using System.Windows.Forms;

namespace smart.login
{
    public partial class Login : Form
    {
        string _target;

        public Login(string Target)
        {
            InitializeComponent();

            _target = Target;

            Xpcom.Initialize("Firefox");
            GeckoPreferences.User["plugin.state.flash"] = true;
            GeckoPreferences.User["browser.xul.error_pages.enabled"] = true;
            GeckoPreferences.User["media.navigator.enabled"] = true;
            GeckoPreferences.User["media.navigator.permission.disabled"] = true;

            Navegador.NSSError += (object sender, Gecko.Events.GeckoNSSErrorEventArgs e) =>
            {
                if (e.Message.Contains("Certificate"))
                {
#pragma warning disable CS0618 // O tipo ou membro é obsoleto
                    CertOverrideService.GetService().RememberRecentBadCert(e.Uri, e.SSLStatus);
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
                    Navegador.Navigate(e.Uri.AbsoluteUri);
                    e.Handled = true;
                }
            };
            Navegador.Navigated += Navegador_Navigated;
            Navegador.Navigate(_target);
            Refresh();
        }
        
        private void Navegador_Navigated(object sender, GeckoNavigatedEventArgs e)
        {
            GeckoWebBrowser webBrowser = (GeckoWebBrowser)sender;
            if (webBrowser.ReferrerUrl.AbsoluteUri == "about:blank")
                return;

            Close();
        }
    }
}