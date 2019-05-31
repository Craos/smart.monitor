using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linear.core
{
    public class LinearException : Exception
    {
        public LinearException(string mensagem, Exception excessao) : base(mensagem, excessao)
        {
        }
    }
}
