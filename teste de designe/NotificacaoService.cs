using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace teste_de_designe
{
    public class NotificacaoService
    {
        private readonly EmailService emailService;

        public NotificacaoService(EmailService emailService)
        {
            this.emailService = emailService;
        }

        public void ProcessarNotificacoes24h()
        {
            using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
            {
                conn.Open();

                string sql = @"
SELECT Id, EmailCliente, Data, Horarios, Servicos, ValorTotal, Status
FROM Agendamentos
WHERE Status = 'Ativo'
  AND EmailCliente IS NOT NULL
  AND LTRIM(RTRIM(EmailCliente)) <> ''
  AND ISNULL(Notificacao24hEnviada, 0) = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    DataTable tabela = new DataTable();
                    tabela.Load(dr);

                    foreach (DataRow row in tabela.Rows)
                    {
                        int id = Convert.ToInt32(row["Id"]);
                        string emailCliente = row["EmailCliente"].ToString() ?? "";
                        DateTime data = Convert.ToDateTime(row["Data"]);
                        string horarios = row["Horarios"].ToString() ?? "";
                        string servicos = row["Servicos"].ToString() ?? "";
                        decimal valorTotal = row["ValorTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(row["ValorTotal"]);

                        DateTime? primeiroHorario = ObterPrimeiraDataHoraAgendamento(data, horarios);
                        if (!primeiroHorario.HasValue)
                            continue;

                        TimeSpan diferenca = primeiroHorario.Value - DateTime.Now;

                        if (diferenca.TotalHours <= 24 && diferenca.TotalHours > 23)
                        {
                            string assunto = "Lembrete: seu agendamento é amanhã";
                            string corpo = $@"Olá!

Este é um lembrete do seu agendamento.

Data: {data:dd/MM/yyyy}
Horário(s): {horarios}
Serviço(s): {servicos}
Valor total: R$ {valorTotal:N2}

Aguardamos você.
Sistema Doula";

                            try
                            {
                                emailService.EnviarEmail(emailCliente, assunto, corpo);
                                MarcarNotificacao24hComoEnviada(id);
                            }
                            catch
                            {
                                // evita travar o sistema se um envio falhar
                            }
                        }
                    }
                }
            }
        }

        public void ProcessarNotificacoes1h()
        {
            using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
            {
                conn.Open();

                string sql = @"
SELECT Id, EmailCliente, Data, Horarios, Servicos, ValorTotal, Status
FROM Agendamentos
WHERE Status = 'Ativo'
  AND EmailCliente IS NOT NULL
  AND LTRIM(RTRIM(EmailCliente)) <> ''
  AND ISNULL(Notificacao1hEnviada, 0) = 0";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    DataTable tabela = new DataTable();
                    tabela.Load(dr);

                    foreach (DataRow row in tabela.Rows)
                    {
                        int id = Convert.ToInt32(row["Id"]);
                        string emailCliente = row["EmailCliente"].ToString() ?? "";
                        DateTime data = Convert.ToDateTime(row["Data"]);
                        string horarios = row["Horarios"].ToString() ?? "";
                        string servicos = row["Servicos"].ToString() ?? "";
                        decimal valorTotal = row["ValorTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(row["ValorTotal"]);

                        DateTime? primeiroHorario = ObterPrimeiraDataHoraAgendamento(data, horarios);
                        if (!primeiroHorario.HasValue)
                            continue;

                        TimeSpan diferenca = primeiroHorario.Value - DateTime.Now;

                        if (diferenca.TotalMinutes <= 60 && diferenca.TotalMinutes > 0)
                        {
                            string assunto = "Lembrete: seu agendamento começa em breve";
                            string corpo = $@"Olá!

Seu agendamento está próximo.

Data: {data:dd/MM/yyyy}
Horário(s): {horarios}
Serviço(s): {servicos}
Valor total: R$ {valorTotal:N2}

Nos vemos em breve!
Sistema Doula";

                            try
                            {
                                emailService.EnviarEmail(emailCliente, assunto, corpo);
                                MarcarNotificacao1hComoEnviada(id);
                            }
                            catch
                            {
                                // evita travar o sistema se um envio falhar
                            }
                        }
                    }
                }
            }
        }

        private DateTime? ObterPrimeiraDataHoraAgendamento(DateTime data, string horarios)
        {
            if (string.IsNullOrWhiteSpace(horarios))
                return null;

            List<TimeSpan> listaHorarios = new List<TimeSpan>();

            string[] partes = horarios.Split(',');

            foreach (string parte in partes)
            {
                string horarioLimpo = parte.Trim();

                if (TimeSpan.TryParse(horarioLimpo, out TimeSpan hora))
                {
                    listaHorarios.Add(hora);
                }
            }

            if (listaHorarios.Count == 0)
                return null;

            TimeSpan menorHorario = listaHorarios.Min();
            return data.Date.Add(menorHorario);
        }

        private void MarcarNotificacao24hComoEnviada(int id)
        {
            using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
            {
                conn.Open();

                string sql = @"
UPDATE Agendamentos
SET Notificacao24hEnviada = 1
WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void MarcarNotificacao1hComoEnviada(int id)
        {
            using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
            {
                conn.Open();

                string sql = @"
UPDATE Agendamentos
SET Notificacao1hEnviada = 1
WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}