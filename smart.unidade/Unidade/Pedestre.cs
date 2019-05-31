using smart.imagem;
using System.Drawing;

namespace smart.unidade
{
    public class Pedestre
    {
        public string Bloco { get; set; }
        public string Unidade { get; set; }
        public string Nome { get; set; }
        public string Foto1 { get; set; }
        public int usuario_identificado { get; set; }
        public string Idade { get; set; }
        public string situacao_usuario { get; set; }
        public string Situacao { get; set; }

        public Image Foto()
        {
            Imagem imagem = new Imagem();
            return imagem.Base64ToImage(imagem.PreparaTextoParaImagem(Foto1));
        }
    }
}
