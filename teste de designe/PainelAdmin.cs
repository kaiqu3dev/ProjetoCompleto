using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace teste_de_designe
{
    public partial class PainelAdmin : Form
    {
        public string adminEmail;

        DataTable tabelaAgendamentos = new DataTable();
        bool atualizandoPainel = false;

        public PainelAdmin(string email)
        {
            InitializeComponent();

            adminEmail = email;

            dgvPainelAdm_Agendamentos.AutoGenerateColumns = true;

            txtPainelAdm_Nome.TextChanged += Filtro_TextChanged;
            txtPainelAdm_Email.TextChanged += Filtro_TextChanged;
            mskPainelAdm_Telefone.TextChanged += Filtro_TextChanged;
            mskPainelAdm_CPF.TextChanged += Filtro_TextChanged;
        }

        private void PainelAdmin_Load(object sender, EventArgs e)
        {
            CarregarHorariosAdmin();
            CarregarAgendamentos();
        }

        private void CarregarAgendamentos()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                {
                    conn.Open();

                    string sql = @"
SELECT
    S.Id AS ItemServicoId,
    U.Nome,
    U.Email,
    U.Telefone,
    U.CPF,
    ISNULL(U.Status,'Ativo') AS StatusUsuario,
    S.Tipo,
    S.Servico,
    S.Data,
    S.Horario,
    A.QuantidadePessoas,
    S.Valor,
    S.Status
FROM AgendamentoServicos S
INNER JOIN Agendamentos A ON A.Id = S.AgendamentoId
INNER JOIN Usuarios U ON U.Email = A.Email
ORDER BY S.Data DESC";

                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);

                    tabelaAgendamentos = new DataTable();
                    da.Fill(tabelaAgendamentos);

                    dgvPainelAdm_Agendamentos.DataSource = null;
                    dgvPainelAdm_Agendamentos.DataSource = tabelaAgendamentos;

                    if (dgvPainelAdm_Agendamentos.Columns.Contains("ItemServicoId"))
                        dgvPainelAdm_Agendamentos.Columns["ItemServicoId"].Visible = false;

                    dgvPainelAdm_Agendamentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgvPainelAdm_Agendamentos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvPainelAdm_Agendamentos.MultiSelect = false;
                    dgvPainelAdm_Agendamentos.ReadOnly = true;
                    dgvPainelAdm_Agendamentos.AllowUserToAddRows = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar agendamentos: " + ex.Message);
            }
        }

        private string PegarValorLinha(DataGridViewRow row, string nomeColuna)
        {
            if (row == null)
                return "";

            if (row.DataBoundItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains(nomeColuna))
                    return drv[nomeColuna]?.ToString() ?? "";
            }

            return "";
        }

        private void Filtro_TextChanged(object sender, EventArgs e)
        {
            if (atualizandoPainel)
                return;

            try
            {
                string filtro = "";

                if (!string.IsNullOrWhiteSpace(txtPainelAdm_Nome.Text))
                    filtro += $"Nome LIKE '%{txtPainelAdm_Nome.Text.Replace("'", "''")}%'";

                if (!string.IsNullOrWhiteSpace(txtPainelAdm_Email.Text))
                    filtro += (filtro != "" ? " AND " : "") +
                              $"Email LIKE '%{txtPainelAdm_Email.Text.Replace("'", "''")}%'";

                if (!string.IsNullOrWhiteSpace(mskPainelAdm_Telefone.Text))
                    filtro += (filtro != "" ? " AND " : "") +
                              $"Telefone LIKE '%{mskPainelAdm_Telefone.Text.Replace("'", "''")}%'";

                if (!string.IsNullOrWhiteSpace(mskPainelAdm_CPF.Text))
                    filtro += (filtro != "" ? " AND " : "") +
                              $"CPF LIKE '%{mskPainelAdm_CPF.Text.Replace("'", "''")}%'";

                tabelaAgendamentos.DefaultView.RowFilter = filtro;
            }
            catch
            {
            }
        }

        private void dgvPainelAdm_Agendamentos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = dgvPainelAdm_Agendamentos.Rows[e.RowIndex];

            txtPainelAdm_Nome.Text = PegarValorLinha(row, "Nome");
            txtPainelAdm_Email.Text = PegarValorLinha(row, "Email");
            mskPainelAdm_Telefone.Text = PegarValorLinha(row, "Telefone");
            mskPainelAdm_CPF.Text = PegarValorLinha(row, "CPF");

            cbbPainelAdm_NovoHorario.Text = PegarValorLinha(row, "Horario");

            string dataTexto = PegarValorLinha(row, "Data");
            DateTime dataSelecionada;

            if (DateTime.TryParse(dataTexto, out dataSelecionada))
                dtpPainelAdm_NovaData.Value = dataSelecionada;
        }

        private void dgvPainelAdm_Agendamentos_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgvPainelAdm_Agendamentos.Rows[e.RowIndex];

            string statusItem = PegarValorLinha(row, "Status");
            string statusUsuario = PegarValorLinha(row, "StatusUsuario");

            if (statusItem == "Ativo")
                row.DefaultCellStyle.BackColor = Color.LightGreen;
            else if (statusItem == "Cancelado")
                row.DefaultCellStyle.BackColor = Color.LightCoral;
            else if (statusItem == "Pendente")
                row.DefaultCellStyle.BackColor = Color.Orange;

            if (!string.IsNullOrWhiteSpace(statusUsuario) &&
                dgvPainelAdm_Agendamentos.Columns.Contains("StatusUsuario"))
            {
                if (statusUsuario == "Banido")
                    row.Cells["StatusUsuario"].Style.ForeColor = Color.DarkRed;
                else
                    row.Cells["StatusUsuario"].Style.ForeColor = Color.DarkBlue;
            }
        }

        private void CarregarHorariosAdmin()
        {
            cbbPainelAdm_NovoHorario.Items.Clear();

            string[] horarios =
            {
                "08:00","09:00","10:00","11:00",
                "13:00","14:00","15:00","16:00",
                "17:00","18:00"
            };

            cbbPainelAdm_NovoHorario.Items.AddRange(horarios);

            if (cbbPainelAdm_NovoHorario.Items.Count > 0)
                cbbPainelAdm_NovoHorario.SelectedIndex = 0;
        }

        private void btnPainelAdmin_Reagendar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecione um item.");
                    return;
                }

                DataGridViewRow row = dgvPainelAdm_Agendamentos.SelectedRows[0];

                int idServico = Convert.ToInt32(PegarValorLinha(row, "ItemServicoId"));
                string tipoServico = PegarValorLinha(row, "Tipo");
                string statusAtual = PegarValorLinha(row, "Status");
                DateTime novaData = dtpPainelAdm_NovaData.Value.Date;
                string novoHorario = cbbPainelAdm_NovoHorario.Text;

                if (statusAtual == "Cancelado")
                {
                    MessageBox.Show("Não é possível reagendar um item cancelado.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(novoHorario))
                {
                    MessageBox.Show("Escolha um horário.");
                    return;
                }

                if (novaData < DateTime.Today)
                {
                    MessageBox.Show("Não pode reagendar para datas passadas.");
                    return;
                }

                DateTime dataHoraNova = DateTime.Parse($"{novaData:dd/MM/yyyy} {novoHorario}");
                if (dataHoraNova < DateTime.Now)
                {
                    MessageBox.Show("Não pode reagendar para data e horário passados.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                {
                    conn.Open();

                    if (tipoServico == "Doula")
                    {
                        string sqlVerificar = @"
SELECT COUNT(*)
FROM AgendamentoServicos
WHERE Tipo = 'Doula'
AND Data = @Data
AND Horario = @Horario
AND Status <> 'Cancelado'
AND Id <> @Id";

                        SqlCommand cmdVerificar = new SqlCommand(sqlVerificar, conn);
                        cmdVerificar.Parameters.AddWithValue("@Data", novaData);
                        cmdVerificar.Parameters.AddWithValue("@Horario", novoHorario);
                        cmdVerificar.Parameters.AddWithValue("@Id", idServico);

                        int quantidade = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show("Esse horário já está ocupado para a doula.");
                            return;
                        }
                    }

                    string sql = @"
UPDATE AgendamentoServicos
SET Data=@Data, Horario=@Horario
WHERE Id=@Id";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Data", novaData);
                    cmd.Parameters.AddWithValue("@Horario", novoHorario);
                    cmd.Parameters.AddWithValue("@Id", idServico);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Reagendado com sucesso!");
                CarregarAgendamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao reagendar: " + ex.Message);
            }
        }

        private void btnPainelAdmin_Reembolsar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecione um item.");
                    return;
                }

                DataGridViewRow row = dgvPainelAdm_Agendamentos.SelectedRows[0];
                int idServico = Convert.ToInt32(PegarValorLinha(row, "ItemServicoId"));

                using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                {
                    conn.Open();

                    string sql = "UPDATE AgendamentoServicos SET Status='Cancelado' WHERE Id=@Id";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", idServico);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Reembolso realizado!");
                CarregarAgendamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao reembolsar: " + ex.Message);
            }
        }

        private void btnPainelAdmin_Banir_Usuario_Click(object sender, EventArgs e)
        {
            AtualizarStatusUsuario("Banido");
        }

        private void btnPainelAdmin_DesbanirUsuario_Click(object sender, EventArgs e)
        {
            AtualizarStatusUsuario("Ativo");
        }

        private void AtualizarStatusUsuario(string status)
        {
            try
            {
                if (dgvPainelAdm_Agendamentos.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Selecione um usuário.");
                    return;
                }

                DataGridViewRow row = dgvPainelAdm_Agendamentos.SelectedRows[0];
                string emailUsuario = PegarValorLinha(row, "Email");

                if (string.IsNullOrWhiteSpace(emailUsuario))
                {
                    MessageBox.Show("Não foi possível localizar o e-mail do usuário.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
                {
                    conn.Open();

                    string sql = "UPDATE Usuarios SET Status=@Status WHERE Email=@Email";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Email", emailUsuario);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Status atualizado com sucesso!");
                CarregarAgendamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void btnPainelAdmin_Atualizar_Click(object sender, EventArgs e)
        {
            try
            {
                atualizandoPainel = true;

                txtPainelAdm_Nome.Clear();
                txtPainelAdm_Email.Clear();
                mskPainelAdm_Telefone.Clear();
                mskPainelAdm_CPF.Clear();

                CarregarHorariosAdmin();

                if (cbbPainelAdm_NovoHorario.Items.Count > 0)
                    cbbPainelAdm_NovoHorario.SelectedIndex = 0;
                else
                    cbbPainelAdm_NovoHorario.Text = "";

                dtpPainelAdm_NovaData.Value = DateTime.Today;

                CarregarAgendamentos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar painel: " + ex.Message);
            }
            finally
            {
                atualizandoPainel = false;
            }
        }
    }
}