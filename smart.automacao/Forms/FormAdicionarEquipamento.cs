using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace smart.automacao
{
    public partial class FormAdicionarEquipamento : DevComponents.DotNetBar.OfficeForm
    {
        private Setor _setor;
        public FormAdicionarEquipamento(Setor Setor)
        {
            InitializeComponent();
            _setor = Setor;
            buttonSalvar.Click += ButtonSalvar_Click;
        }

        private void ButtonSalvar_Click(object sender, EventArgs e)
        {   
            if (gridcontroles.Rows.Count == 0)
            {
                MessageBox.Show("Nenhum controle foi definido para este equipamento");
                return;
            }

            Equipamento equipamento = new Equipamento();
            equipamento.Id = _setor.Equipamentos.Count + 1;
            equipamento.Titulo = textBoxTitulo.Text;
            equipamento.MonitorRede = checkBoxMonitorRede.Checked;
            equipamento.Endereco = ipAddressInput.Text;

            List<Controle> controles = new List<Controle>();

            for(int i = 0; i< gridcontroles.Rows.Count; i++)
            {
                if (gridcontroles.Rows[i].Cells["Titulo"].Value == null)
                    continue;

                Controle controle = new Controle();
                controle.Id = i;
                controle.Titulo = gridcontroles.Rows[i].Cells["Titulo"].Value.ToString();
                controle.Acao = (TipoAcao)int.Parse(gridcontroles.Rows[i].Cells["Acao"].Value.ToString());
                controle.Comando = gridcontroles.Rows[i].Cells["Comando"].Value.ToString();

                controles.Add(controle);
            }

            equipamento.Controles = controles;
            

            Setores.Lista[_setor.Id].Equipamentos.Add(equipamento);
            Setores.Salvar();
            Close();
        }
    }
}