using smart.automacao.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DevComponents.DotNetBar.Metro;

namespace smart.automacao
{
    public partial class FormPainelSetor : MetroForm
    {
        public delegate void delegateAoRecebercomando(Comando comando);
        public event delegateAoRecebercomando AoReceberComando;

        Setor _setor;
        public FormPainelSetor(Setor Setor)
        {
            InitializeComponent();

            _setor = Setor;
            CarregaEquipamentos();
            Shown += FormPainelSetor_Shown;

        }

        private void FormPainelSetor_Shown(object sender, System.EventArgs e)
        {
            if (_setor.UserLocation != null)
            {
                string[] l = _setor.UserLocation.Split(',');
                string locationX = Regex.Match(l[0], @"\d+").Value;
                string locationY = Regex.Match(l[1], @"\d+").Value;
                Location = new Point(int.Parse(locationX), int.Parse(locationY));
            }

            /*if (_setor.UserSize != null)
            {
                string[] s = _setor.UserSize.Split(',');
                string width = Regex.Match(s[0], @"\d+").Value;
                string heigh = Regex.Match(s[0], @"\d+").Value;
                Size = new Size(int.Parse(width), int.Parse(heigh));
            }*/
            LocationChanged += FormPainelSetor_LocationChanged;
            SizeChanged += FormPainelSetor_SizeChanged;

        }

        private void FormPainelSetor_SizeChanged(object sender, System.EventArgs e)
        {
            _setor.UserSize = Size.ToString();
        }

        private void FormPainelSetor_LocationChanged(object sender, System.EventArgs e)
        {
            _setor.UserLocation = Location.ToString();
        }

        private void Equipamento_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            CarregaEquipamentos();
        }

        void CarregaEquipamentos()
        {
            if (_setor.Equipamentos == null)
                return;

            Layout.Controls.Clear();
            foreach (Equipamento equipamento in _setor.Equipamentos)
            {
                equipamento.Setor = _setor.Titulo;
                UserControlEquipamento userControl = new UserControlEquipamento(equipamento);

                if (equipamento.Controles.Count > 0)
                {
                    userControl.AoExecutarComando += UserControl_AoExecutarComando;
                    userControl.Size = new Size(userControl.Size.Width, userControl.Height + (equipamento.Controles.Count * 30));
                }

                userControl.Dock = DockStyle.Top;
                userControl.BorderStyle = BorderStyle.None;
                Layout.Controls.Add(userControl);
                Layout.Refresh();
            }
            Refresh();
        }

        private void UserControl_AoExecutarComando(Comando comando)
        {
            if (AoReceberComando != null)
                AoReceberComando(comando);
        }
    }
}