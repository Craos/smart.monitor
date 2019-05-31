using Npgsql;
using smart.unidade;
using System;
using System.Data;


namespace smart.automacao.Data
{
    public class RegistroComandos : DataTable
    {
        string connString;

        public RegistroComandos(string Parametros)
        {
            connString = Parametros;

            Clear();

            DataColumn filedate = new DataColumn();
            filedate.ColumnName = "filedate";
            filedate.Caption = "Data";
            filedate.DataType = typeof(string);
            filedate.ReadOnly = true;

            DataColumn num = new DataColumn();
            num.ColumnName = "num";
            num.Caption = "Registro";
            num.DataType = typeof(int);
            num.ReadOnly = true;
            num.Unique = true;

            DataColumn setor = new DataColumn();
            setor.ColumnName = "setor";
            setor.Caption = "Setor";
            setor.DataType = typeof(string);
            setor.ReadOnly = true;

            DataColumn equipamento = new DataColumn();
            equipamento.ColumnName = "equipamento";
            equipamento.Caption = "Equipamento";
            equipamento.DataType = typeof(string);
            equipamento.ReadOnly = true;

            DataColumn controle = new DataColumn();
            controle.ColumnName = "controle";
            controle.Caption = "Controle";
            controle.DataType = typeof(string);
            controle.ReadOnly = true;

            DataColumn observacoes = new DataColumn();
            observacoes.ColumnName = "observacoes";
            observacoes.Caption = "Observações";
            observacoes.DataType = typeof(string);

            Columns.AddRange(new DataColumn[] { filedate, num, setor, equipamento, controle, observacoes });



        }

        internal void Editar(DataRow row)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = $"UPDATE acesso.automacao_log SET observacoes = '{row["observacoes"].ToString()}' WHERE num = {row["num"].ToString()}";
                    cmd.ExecuteReader();
                }
            }
        }

        public void Carregar()
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT filedate, num, setor, equipamento, controle, observacoes FROM acesso.automacao_log ORDER BY num DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    Clear();

                    while (reader.Read())
                    {
                        DataRow linha = NewRow();
                        linha["filedate"] = reader["filedate"].ToString();
                        linha["num"] = reader["num"].ToString();
                        linha["setor"] = reader["setor"].ToString();
                        linha["equipamento"] = reader["equipamento"].ToString();
                        linha["controle"] = reader["controle"].ToString();
                        linha["observacoes"] = reader["observacoes"].ToString();

                        Rows.Add(linha);
                    }
                }
            }
        }

        public void Adicionar(Comando comando)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO acesso.automacao_log (setor, equipamento, controle, comando) VALUES (@setor, @equipamento, @controle, @comando) RETURNING filedate, num";
                    cmd.Parameters.AddWithValue("setor", comando.Setor);
                    cmd.Parameters.AddWithValue("equipamento", comando.Equipamento);
                    cmd.Parameters.AddWithValue("controle", comando.Controle);
                    cmd.Parameters.AddWithValue("comando", comando.NomeComando);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataRow linha = NewRow();
                            linha["filedate"] = reader["filedate"].ToString();
                            linha["num"] = reader["num"].ToString();
                            linha["setor"] = comando.Setor;
                            linha["equipamento"] = comando.Equipamento;
                            linha["controle"] = comando.Controle;
                            Rows.Add(linha);
                        }


                        DefaultView.Sort = "num desc";
                        DefaultView.ToTable();
                    }
                }
            }
        }
    }
}
