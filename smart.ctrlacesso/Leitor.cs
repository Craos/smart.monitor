using System.Collections.Generic;

namespace smart.ctrlacesso
{
    public class Leitor
    {
        public int Num { get; set; }
        public string Porta { get; set; }
        public string Descricao { get; set; }
        public string Procedimento { get; set; }
        public string Tipo { get; set; }
        public string Sentido { get; set; }
        public string idade_minima { get; set; }
        public List<Acionador> Acionadores { get; set; }

    }
}