using smart.automacao.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smart.info;
using smart.unidade;

namespace smart.automacao
{
    public class Comando
    {
        public Registro Registro { get; set; }
        public string Setor { get; set; }
        public string Equipamento { get; set; }
        public string Controle { get; set; }
        public object NomeComando { get; set; }

        public override string ToString()
        {
            return $"Filedate={Registro};Num={Registro.num};Setor={Setor};Equipamento={Equipamento};Controle={Controle}";
        }
    }
}
