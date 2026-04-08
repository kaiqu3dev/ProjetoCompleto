using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace teste_de_designe
{
    public partial class PainelAdmin : Form
    {
        public string adminEmail;  // Armazena o e-mail do administrador
        List<Agendamento> listaAgendamentos = new List<Agendamento>();  // Lista de agendamentos

        public PainelAdmin(string email)
        {
            InitializeComponent();
            adminEmail = email;
            CarregarAgendamentos();  // Carrega os agendamentos ao inicializar o painel
        }

        // Carregar todos os agendamentos do banco
        private void CarregarAgendamentos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                {
                    conn.Open();
                    string sql = @"
                    SELECT A.Id, A.Data, A.Horarios, A.Servicos, A.ValorTotal, A.QuantidadePessoas, A.Status, 
                           U.Nome, U.Telefone, U.Email, U.CPF
                    FROM Agendamentos A
                    INNER JOIN Usuarios U ON A.Email = U.Email";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    listaAgendamentos.Clear();
                    while (reader.Read())
                    {
                        listaAgendamentos.Add(new Agendamento
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Data = Convert.ToDateTime(reader["Data"]),
                            Horarios = reader["Horarios"].ToString().Split(',').ToList(),
                            Servicos = reader["Servicos"].ToString().Split(',').ToList(),
                            ValorTotal = Convert.ToDecimal(reader["ValorTotal"]),
                            QuantidadePessoas = Convert.ToInt32(reader["QuantidadePessoas"]),
                            Status = reader["Status"].ToString(),
                            Nome = reader["Nome"].ToString(),
                            Telefone = reader["Telefone"].ToString(),
                            Email = reader["Email"].ToString(),
                            CPF = reader["CPF"].ToString()
                        });
                    }

                    // Preenche o DataGridView com os agendamentos
                    dgvPainelAdm_Agendamentos.DataSource = listaAgendamentos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar agendamentos: " + ex.Message);
            }
        }

        // Botão para reagendar um agendamento
        private void btnPainelAdmin_Reagendar_Click(object sender, EventArgs e)
        {
            try
            {
                // Verifica se há uma linha selecionada
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count > 0)
                {
                    // Pega o ID do agendamento selecionado
                    int agendamentoId = Convert.ToInt32(dgvPainelAdm_Agendamentos.SelectedRows[0].Cells["Id"].Value);

                    // Nova data do agendamento
                    DateTime novaData = dtpPainelAdm_NovaData.Value;
                    // Novo horário do agendamento
                    string novoHorario = cbbPainelAdm_NovoHorario.SelectedItem.ToString();

                    // Verifica se a nova data é no passado
                    if (novaData < DateTime.Today)
                    {
                        MessageBox.Show("Você não pode reagendar para datas passadas.");
                        return;
                    }

                    using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                    {
                        conn.Open();

                        // Atualiza o agendamento com a nova data e horário
                        string sqlUpdate = @"
                        UPDATE Agendamentos
                        SET Data = @NovaData, Horarios = @NovoHorario
                        WHERE Id = @AgendamentoId";

                        SqlCommand cmd = new SqlCommand(sqlUpdate, conn);
                        cmd.Parameters.AddWithValue("@NovaData", novaData);
                        cmd.Parameters.AddWithValue("@NovoHorario", novoHorario);
                        cmd.Parameters.AddWithValue("@AgendamentoId", agendamentoId);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Agendamento reagendado com sucesso!");
                    CarregarAgendamentos();  // Atualiza a lista de agendamentos
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um agendamento.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao reagendar: " + ex.Message);
            }
        }

        // Botão para reembolsar um agendamento
        private void btnPainelAdmin_Reembolsar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count > 0)
                {
                    // Pega o ID do agendamento selecionado
                    int agendamentoId = Convert.ToInt32(dgvPainelAdm_Agendamentos.SelectedRows[0].Cells["Id"].Value);

                    using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                    {
                        conn.Open();

                        // Atualiza o status do agendamento para "Cancelado"
                        string sqlCancelamento = @"
                        UPDATE Agendamentos
                        SET Status = 'Cancelado'
                        WHERE Id = @AgendamentoId";

                        SqlCommand cmdCancelamento = new SqlCommand(sqlCancelamento, conn);
                        cmdCancelamento.Parameters.AddWithValue("@AgendamentoId", agendamentoId);
                        cmdCancelamento.ExecuteNonQuery();
                    }

                    MessageBox.Show("Agendamento cancelado com sucesso!");
                    CarregarAgendamentos();  // Atualiza a lista de agendamentos
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um agendamento.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao realizar o reembolso: " + ex.Message);
            }
        }

        // Botão para banir um usuário
        private void btnPainelAdmin_Banir_Usuario_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count > 0)
                {
                    // Pega o e-mail do usuário
                    string emailUsuario = dgvPainelAdm_Agendamentos.SelectedRows[0].Cells["Email"].Value.ToString();

                    using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                    {
                        conn.Open();

                        // Atualiza o status do usuário para "Banido"
                        string sqlBanir = @"
                        UPDATE Usuarios
                        SET Status = 'Banido'
                        WHERE Email = @EmailUsuario";

                        SqlCommand cmdBanir = new SqlCommand(sqlBanir, conn);
                        cmdBanir.Parameters.AddWithValue("@EmailUsuario", emailUsuario);
                        cmdBanir.ExecuteNonQuery();
                    }

                    MessageBox.Show("Usuário banido com sucesso!");
                    CarregarAgendamentos();  // Atualiza a lista de agendamentos
                }
                else
                {
                    MessageBox.Show("Por favor, selecione um usuário.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao banir o usuário: " + ex.Message);
            }
        }
    }
}