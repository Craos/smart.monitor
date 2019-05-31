using DevComponents.DotNetBar.Controls;
using DevComponents.DotNetBar.Metro;
using System.Windows.Forms;
using smart.automacao.Data;
using System.Data;
using System.Linq;
using System;
using System.Drawing;

namespace smart.automacao
{
    public partial class MDIComandos : MetroForm
    {
        delegate void InvokeControle(DataGridViewX control, int Index, params object[] Values);
        bool formsetoresaberto = false;
        RegistroComandos registro;

        public MDIComandos()
        {
            InitializeComponent();
            MontarSetores();
            SettingsButtonClick += ButtonSetores_Click;
            buttonItemCascata.Click += ButtonItemCascata_Click;
            buttonItemLadoaladoVertical.Click += ButtonItemLadoaladoVertical_Click;
            buttonItemLadoaladoHorizontal.Click += ButtonItemLadoaladoHorizontal_Click;

            registro = new RegistroComandos(Properties.Settings.Default.registro);
            registro.Carregar();
            gridHistorico.DataSource = registro;

            foreach (DataColumn item in registro.Columns)
            {
                gridHistorico.Columns[item.ColumnName].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                gridHistorico.Columns[item.ColumnName].HeaderText = item.Caption;
            }

            gridHistorico.Columns["observacoes"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            registro.RowChanged += Registro_RowChanged;
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void Registro_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            registro.Editar(e.Row);
            foreach (DataGridViewRow row in gridHistorico.Rows)
                if (row.Cells["num"].Value.ToString() == e.Row["num"].ToString())
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }
        }

        private void ButtonItemLadoaladoHorizontal_Click(object sender, System.EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ButtonItemLadoaladoVertical_Click(object sender, System.EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void ButtonItemCascata_Click(object sender, System.EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        public void MontarSetores()
        {
            if (Setores.Lista == null)
                return;

            foreach (Setor item in Setores.Lista)
            {
                if (item.UserVisible == true)
                {
                    AdicionarSetor(item);
                }
            }
        }

        private void ButtonSetores_Click(object sender, System.EventArgs e)
        {
            if (formsetoresaberto == false)
            {
                formsetoresaberto = true;
                FormSetores setoresform = new FormSetores();
                setoresform.AoAdicionar += AdicionarSetor;
                setoresform.AoEditar += Setoresform_AoEditar;
                setoresform.AoRemover += Setoresform_AoRemover;
                setoresform.FormClosed += (object s_sender, FormClosedEventArgs s_e) =>
                {
                    formsetoresaberto = false;
                };
                setoresform.Show();
            }
        }

        private void Setoresform_AoRemover(Setor setor, int Index)
        {
            foreach (Form item in MdiChildren)
                if ((int)item.Tag == Index)
                    item.Close();
        }

        private void Setoresform_AoEditar(Setor setor, int Index)
        {
            foreach (Form item in MdiChildren)
                if ((int)item.Tag == setor.Id)
                    item.Text = setor.Titulo;
        }

        private void AdicionarSetor(Setor setor)
        {
            FormPainelSetor painelsetor = new FormPainelSetor(setor);
            painelsetor.MdiParent = this;
            painelsetor.Text = setor.Titulo;
            painelsetor.Tag = setor.Id;
            painelsetor.AoReceberComando += Painelsetor_AoReceberComando;
            painelsetor.Show();
        }

        private void Painelsetor_AoReceberComando(Comando comando)
        {
            registro.Adicionar(comando);
        }

    }
}
