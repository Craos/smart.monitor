using DevComponents.DotNetBar;
using System.Windows.Forms;
using smart.info;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System;

namespace smart.automacao
{
    public partial class FormSetores : OfficeForm
    {
        public delegate void Adicionar(Setor setor);
        public event Adicionar AoAdicionar;

        public delegate void Editar(Setor setor, int Index);
        public event Editar AoEditar;

        public delegate void Remover(Setor setor, int Index);
        public event Remover AoRemover;

        protected virtual void RaiseAdicionar(Setor setor)
        {
            AoAdicionar(setor);
        }
        protected virtual void RaiseEditar(Setor setor, int Index)
        {
            AoEditar(setor, Index);
        }

        protected virtual void RaiseRemover(Setor setor, int Index)
        {
            AoRemover(setor, Index);
        }

        public FormSetores()
        {
            InitializeComponent();
            
            foreach (Setor setor in Setores.Lista)
                grid.Rows.Insert(setor.Id, setor.Titulo, setor.UserVisible);


            grid.CellClick += Grid_CellClick;
            grid.RowsRemoved += Grid_RowsRemoved;
            grid.CellValueChanged += Grid_CellValueChanged;
            
            FormClosing += FormSetores_FormClosing;
        }
        
        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {   
            if (e.ColumnIndex == 2)
                new FormEquipamentos(Setores.Lista[e.RowIndex]).Show();
        }

        private void Grid_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Atenção: A Exclusão deste setor irá excluir também os equipamentos e controles associados a ele! \nVocê confirma este procedimentos?", "Smart Monitor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialog == DialogResult.Yes)
            {
                Setor setor = Setores.Lista[e.RowIndex];
                if (setor != null)
                {
                    RaiseRemover(setor, e.RowIndex);
                    Setores.Lista.Remove(setor);
                }
                    
            }
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow linha = grid.Rows[e.RowIndex];

            object cell = linha.Cells[0].Value;
            if (cell == null)
                return;

            Setor setor = Setores.Lista.Find(x => x.Id == e.RowIndex);
            if (setor != null)
            {   
                Setores.Lista[e.RowIndex].Titulo = cell.ToString();
                RaiseEditar(Setores.Lista[e.RowIndex], e.RowIndex);
            }
            else
            {
                Setor novosetor = new Setor
                {
                    Id = e.RowIndex,
                    Titulo = cell.ToString()
                };

                RaiseAdicionar(novosetor);
                Setores.Lista.Add(novosetor);
            }
        }
        
        private void FormSetores_FormClosing(object sender, FormClosingEventArgs e)
        {
            Setores.Salvar();
        }
    }
}