using Npgsql;
using NpgsqlTypes;
using System.Data;
using System;
using System.Diagnostics;


namespace smart.transfer
{
    public class Transfer
    {
        private NpgsqlConnection connection;
        private System.Timers.Timer Temporizador = new System.Timers.Timer();

        public delegate void AoExecutarEvento(string ID, string Mensagem, EventLogEntryType Tipo);
        public event AoExecutarEvento AoExecutar;

        public string ID { get; set; }
        public int Intervalo { get;  set; }
        public string Procedimento { get;  set; }
        public string StringConexaoRemota { get;  set; }
        public int LimiteTransferencia { get;  set; }
        public string StringConexaoLocal { get;  set; }

        public virtual void ChamaEventoAoExecutar(string Mensagem, EventLogEntryType Tipo)
        {
            AoExecutar(ID, Mensagem, Tipo);
        }

        public Transfer()
        {

        }
        
        public void Iniciar()
        {
            Temporizador.Interval = ((1000 * 60) * Intervalo);
            Temporizador.Enabled = true;
            Temporizador.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => { Executar(); };
            Temporizador.Start();
        }

        private void Executar()
        {
            int retorno = 0;
            try
            {
                ChamaEventoAoExecutar("Iniciando a conexão com o banco de dados local", EventLogEntryType.Information);
                if (IniciarConexao() == false)
                    return;

                using (var cmd = new NpgsqlCommand(Procedimento, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("origem", NpgsqlDbType.Text, StringConexaoRemota);
                    cmd.Parameters.AddWithValue("limite", NpgsqlDbType.Bigint, LimiteTransferencia);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retorno = reader.GetInt32(0);
                            ChamaEventoAoExecutar($"Atualização de registros finalizada. {retorno} registros.", EventLogEntryType.SuccessAudit);
                            break;
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }

            if (connection.State == ConnectionState.Open)
                connection.Close();

        }

        private bool IniciarConexao()
        {
            bool retorno = false;
            try
            {
                connection = new NpgsqlConnection
                {
                    ConnectionString = StringConexaoLocal
                };
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    retorno = true;

            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            return retorno;
        }
    }
}
