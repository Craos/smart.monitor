using smart.unidade;
using System.Threading.Tasks;

namespace smart.pesquisa
{
    public interface IPesquisaDispositivo
    {
        string Address { get; set; }
        Task<Unidade> BuscarPedestre(Passagem passagem, string Procedimento = null);
        Task<Unidade> BuscarVeiculo(Passagem passagem, string Procedimento = null);
    }
}
