using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using smart.unidade;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace smart.pesquisa
{
    public class PesquisaLocal : IPesquisaDispositivo
    {
        NpgsqlConnection connection;
        public string Address { get; set; }

        private void IniciarConexao()
        {
            try
            {
                connection = new NpgsqlConnection
                {
                    ConnectionString = Address
                };

                connection.Open();
            }
            catch (NpgsqlException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task<Unidade> Pesquisar<T>(Passagem passagem, string Procedimento = null)
        {
            try
            {
                IniciarConexao();
                using (var cmd = new NpgsqlCommand("buscarpedestre", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("info", NpgsqlDbType.Json, passagem.rfid);

                    NpgsqlDataAdapter ad = new NpgsqlDataAdapter(cmd);
                    DataTable dt = new DataTable();

                    ad.Fill(dt);

                    using (var reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        if (dt.Rows.Count > 0)
                        {
                            Unidade unidade = new Unidade();
                            T checkType = default(T);

                            if (checkType is Pedestre)
                            {
                                unidade.ListaPessoas = JsonConvert.DeserializeObject<List<Pedestre>>(dt.Rows[0].ItemArray[0].ToString());
                            }
                            else
                            {
                                unidade.ListaVeiculos = JsonConvert.DeserializeObject<List<Veiculo>>(dt.Rows[0].ItemArray[0].ToString());
                            }
                            unidade.CarregarInformacoes();
                            return unidade;
                        }

                    }
                }
            }
            catch (NpgsqlException exception)
            {
                throw new Exception(exception.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        Task<Unidade> IPesquisaDispositivo.BuscarPedestre(Passagem passagem, string Procedimento)
        {
            return Pesquisar<Pedestre>(passagem, Procedimento);
        }

        Task<Unidade> IPesquisaDispositivo.BuscarVeiculo(Passagem passagem, string Procedimento)
        {
            return Pesquisar<Veiculo>(passagem, Procedimento);
        }
    }
}