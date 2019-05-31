using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace smart.automacao
{
    public partial class FormEquipamentos : DevComponents.DotNetBar.OfficeForm
    {
        Setor _setor;

        public FormEquipamentos(Setor setor)
        {
            InitializeComponent();

            _setor = setor;
            Shown += FormEquipamentos_Shown;
        }

        private void FormEquipamentos_Shown(object sender, EventArgs e)
        {
            _setor.Equipamentos.ForEach(equipamento =>
            {
                grid.Rows.Insert(equipamento.Id, equipamento.Titulo, equipamento.MonitorRede, equipamento.Endereco);
            });
                

            grid.CellValueChanged += Grid_CellValueChanged;
            grid.RowsRemoved += Grid_RowsRemoved;
            grid.CellClick += Grid_CellClick;
            FormClosing += FormSetores_FormClosing;
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 1)
                return;

            DataGridViewRow linha = grid.Rows[e.RowIndex];

            object cell = linha.Cells[0].Value;
            if (cell == null)
                return;

            //Setor setor = Program.Config.Setores.Find(x => x.Id == e.RowIndex);
            throw new NotImplementedException();
        }

        private void Grid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Atenção: A Exclusão deste equipamento irá excluir também os controles associados a ele! \nVocê confirma este procedimentos?", "Smart Monitor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog == DialogResult.Yes)
            {
                Equipamento equipamento = Setores.Lista[_setor.Id].Equipamentos.Find(x=>x.Id == e.RowIndex);
                if (equipamento != null)
                {
                    Setores.Lista[_setor.Id].Equipamentos.Remove(equipamento);
                    //Program.MDIComandos.RemoverEquipamentos(_setor, equipamento);
                }
            }
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            object celula = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (celula == null)
                return;

            if (e.ColumnIndex == 0)
            {
                Equipamento equipamento = Setores.Lista[_setor.Id].Equipamentos.Find(x => x.Id == e.RowIndex);
                if (equipamento != null)
                {
                    Setores.Lista[_setor.Id].Equipamentos[equipamento.Id].Titulo = celula.ToString();
                    //Program.MDIComandos.EditarEquipamento(_setor, equipamento);
                }
                else
                {
                    Equipamento novo = new Equipamento
                    {
                        Id = e.RowIndex,
                        Titulo = celula.ToString()
                    };

                    Setores.Lista[_setor.Id].Equipamentos.Add(novo);
                    //Program.MDIComandos.AdicionarEquipamento(_setor, novo);

                }
            }
        }

        private void FormSetores_FormClosing(object sender, FormClosingEventArgs e)
        {
            Setores.Salvar();
        }
    }
}