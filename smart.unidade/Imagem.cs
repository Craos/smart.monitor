using System;
using System.Drawing;
using System.IO;

namespace smart.imagem
{
    public class Imagem
    {

        public Imagem()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public byte[] ConvertImagemParaByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }



        /// <summary>
        /// Processa a string que contem a imagem dos familiares. Procedimento especifico para o objeto MetroTileItem
        /// </summary>
        /// <param name="Familiar">O objeto com os detalhes da pessoa (Foto, posicao familiar, nome, idade etc)</param>
        /// <param name="Picturebox">O objeto que ira receber a foto do familiar</param>
        /// <param name="NomeFamiliar">Nome da pessoa</param>
        public string PreparaTextoParaImagem(string Imagem)
        {
            string foto = "";
            if (Imagem != null && Imagem.Length > 200)
            {
                string imagemparaconversao = Imagem.Replace("data:image/png;base64,", "");
                while (IsBase64(imagemparaconversao) == false)
                    imagemparaconversao = imagemparaconversao + "=";
                foto = imagemparaconversao;
            }
            return foto;
        }
        /// <summary>
        /// Verifica se os dados estao no formato base 64 esperado
        /// </summary>
        /// <param name="base64String">Uma string com base 64</param>
        /// <returns></returns>
        private bool IsBase64(string base64String)
        {
            bool retorno = false;
            if (base64String.Replace(" ", "").Length % 4 != 0)
                retorno = false;
            try
            {
                Convert.FromBase64String(base64String);
                retorno = true;
            }
            catch
            {
            }
            return retorno;
        }

        /// <summary>
        /// Converte uma string base 64 para imagem
        /// </summary>
        /// <param name="base64Styring">Uma string com a imagem</param>
        /// <returns></returns>
        public Image Base64ToImage(string base64Styring)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Styring);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
    }
}
