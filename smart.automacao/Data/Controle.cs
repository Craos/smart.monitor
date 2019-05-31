using System.Data;

namespace smart.automacao
{
    public enum TipoAcao
    {
        Monitoramento,
        Acionamento,
        Restart
    }

    public class Controle
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public TipoAcao Acao { get; set; }
        public string Comando { get; set; }
        public string Registro { get; set; }
        public string Setor { get; internal set; }
        public string Equipamento { get; internal set; }
    }
}
