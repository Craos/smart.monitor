using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace smart.info
{
    public interface ISmartPlugins
    {
        string Nome { get; }
        string Titulo { get; }
        Image Icone { get; }
        bool ApresentaMenu { get; }
        void Iniciar();
        void Finalizar();
        void Exibir();
        UserControl PainelConfiguracoes { get; }

        event EventHandler Logs;
        event EventHandler AoIdentificarDispositivo;

        void NovoLog();
        void NovoDispositivo();
    }
}
