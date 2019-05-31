using Newtonsoft.Json;
using smart.info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace smart.automacao
{
    public class Principal : ISmartPlugins
    {
        bool formaberto = false;

        public string Nome => "Automacao";
        public string Titulo => "Gerenciador de automação";
        public Image Icone => Properties.Resources.icone;
        public bool ApresentaMenu => true;
        public UserControl PainelConfiguracoes => new Configuracoes();

        public event EventHandler Logs;
        public event EventHandler AoIdentificarDispositivo;

        public void Exibir()
        {
            if (formaberto == false)
            {
                MDIComandos MDIComandos = new MDIComandos();
                MDIComandos.FormClosed += (object fcsender, FormClosedEventArgs fce) => { formaberto = false; };
                MDIComandos.Show();
                formaberto = true;
            }
        }

        public void Finalizar()
        {
            Setores.Salvar();
        }

        public void Iniciar()
        {
            Setores.Carregar();
        }

        public void NovoDispositivo()
        {
            throw new NotImplementedException();
        }

        public void NovoLog()
        {
            throw new NotImplementedException();
        }
    }

    public static class Setores
    {
        private static string arquivoconfiguracao = $@"{AssemblyDirectory}\configpainel.json";
        public static List<Setor> Lista { get; set; }

        public static void Carregar()
        {

            if (File.Exists(arquivoconfiguracao))
                Lista = JsonConvert.DeserializeObject<List<Setor>>(File.ReadAllText(arquivoconfiguracao));
        }

        public static void Salvar()
        {
            File.WriteAllText(arquivoconfiguracao, JsonConvert.SerializeObject(Lista));
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
