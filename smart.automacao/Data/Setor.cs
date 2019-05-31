using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace smart.automacao
{
    public class Setor
    {
        private string _titulo;
        public int Id { get; set; }
        public string Titulo
        {
            get
            {
                return _titulo;
            }
            set
            {
                _titulo = value;
            }
        }
        public List<Equipamento> Equipamentos { get; set; }
        public string UserLocation { get; set; }
        public string UserSize { get; set; }
        public bool UserVisible { get; set; } = true;

    }
}
