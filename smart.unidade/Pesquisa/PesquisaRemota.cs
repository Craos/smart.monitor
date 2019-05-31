using Newtonsoft.Json;
using smart.info;
using smart.unidade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace smart.pesquisa
{
    public class PesquisaRemota : IPesquisaDispositivo
    {
        public string Address { get; set; }

        public async Task<Unidade> BuscarPedestre(Passagem passagem, string Procedimento = null)
        {
            string url = Address + Procedimento + passagem.rfid;
            string info = await Pesquisar(url);

            if (info != null)
            {
                Unidade unidade = new Unidade();
                unidade.ListaPessoas = JsonConvert.DeserializeObject<List<Pedestre>>(info);
                unidade.CarregarInformacoes();
                return unidade;
            }

            return null;
        }

        public async Task<Unidade> BuscarVeiculo(Passagem passagem, string Procedimento = null)
        {
            string url = Address + Procedimento + passagem.rfid;
            string info = await Pesquisar(url);

            if (info != null)
            {
                Unidade unidade = new Unidade();
                unidade.ListaVeiculos = JsonConvert.DeserializeObject<List<Veiculo>>(info);
                unidade.CarregarInformacoes();
                return unidade;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Serial"></param>
        /// <returns></returns>
        public async Task<string> Pesquisar(string URL)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
                request.ContentType = "text/html";
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = 20000;
                request.Proxy = null;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string retorno = await reader.ReadToEndAsync();

                    if (retorno == "")
                        return null;

                    return retorno;
                }
            }
            catch (WebException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
