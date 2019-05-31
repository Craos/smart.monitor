using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smart.hardware
{
    public class Terminal
    {
        public DateTime filedate { get; set; }
        public DateTime timerg { get; set; }
        public string uidins { get; set; }
        public int num { get; set; }
        public string localizacao { get; set; }
        public string macaddress { get; set; }
        public string licenca { get; set; }
        public List<Controladora> Controladoras { get; set; }

    }
}
