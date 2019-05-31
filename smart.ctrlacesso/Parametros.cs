using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smart.ctrlacesso
{
    public class Parametros
    {
        public int Num { get; set; }
        public bool UsarBaseLocal { get; set; }
        public string Descricao { get; set; }
        public string Tipo_conexao { get; set; }
        public string Endereco { get; set; }
        public int Porta { get; set; }
        public int Tempo_reconexao { get; set; }
        public string Fabricante { get; set; }
        public List<Leitor> Leitores { get; set; }
    }
}
