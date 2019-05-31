using System.Collections.Generic;

namespace smart.unidade
{
    public enum EstadoIdentificacao
    {
        Localizado,
        NaoLocalizado
    }

    public class Passagem
    {
        public string rfid { get; set; }
        public int equipamento { get; set; }
        public string localizacao_equipamento { get; set; }
        public int leitor { get; set; }
        public string localizacao_leitor { get; set; }
        public string sentido { get; set; }
        public string idade_minima { get; set; }
        public Unidade Unidade { get; set; }
        public EstadoIdentificacao Status { get; set; }

        public bool Autorizado
        {
            get
            {
                if (Unidade != null && Unidade.PedestreIdentificado.Situacao == "Autorizado")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    public class Registro
    {
        public string filedate { get; set; }
        public string num { get; set; }
    }
}