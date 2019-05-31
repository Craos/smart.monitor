using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smart.unidade
{
    public class Unidade
    {
        public List<Pedestre> ListaPessoas { get; set; }

        private Pedestre _pedestreIdentificado;
        public Pedestre PedestreIdentificado
        {
            get
            {
                return _pedestreIdentificado;
            }
        }


        public List<Veiculo> ListaVeiculos { get; set; }

        private Veiculo _veiculoIdentificado;
        public Veiculo VeiculoIdentificado
        {
            get
            {
                return _veiculoIdentificado;
            }
        }

        public string Bloco { get; internal set; }
        public string Numero { get; internal set; }

        internal void CarregarInformacoes()
        {
            if (ListaPessoas != null)
            {
                _pedestreIdentificado = ListaPessoas.Find(x => x.usuario_identificado == 1);
                ListaPessoas.Remove(_pedestreIdentificado);
                Bloco = _pedestreIdentificado.Bloco;
                Numero = _pedestreIdentificado.Unidade;

            }
            else if (ListaVeiculos != null)
            {
                _veiculoIdentificado = ListaVeiculos.Find(x => x.Registro_identificado == 1);
                ListaVeiculos.Remove(_veiculoIdentificado);
                Bloco = _veiculoIdentificado.Bloco;
                Numero = _veiculoIdentificado.Unidade;

            }
        }
    }
}
