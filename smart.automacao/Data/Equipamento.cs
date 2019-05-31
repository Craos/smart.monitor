using System.Collections.Generic;

namespace smart.automacao
{
    public class Equipamento
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public List<Controle> Controles { get; set; }
        public bool MonitorRede { get; set; }
        public string Endereco { get; set; }
        public string Setor { get; internal set; }
    }
}