using Newtonsoft.Json;
using smart.unidade;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace smart.info.Pesquisa
{
    public class PesquisaHTTP
    {
        public async Task<Registro> Pesquisar(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.ContentType = "text/html";
                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = 20000;
                request.Proxy = null;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string info = await reader.ReadToEndAsync();

                    if (info != null && info != "")
                    {
                        Registro registro = new Registro();
                        registro = JsonConvert.DeserializeObject<Registro>(info);
                        return registro;
                    }

                    return null;

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
