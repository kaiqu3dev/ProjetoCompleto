using System;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace teste_de_designe
{
    public partial class PaginaLogin : Form
    {
        public string emailDoUsuario;

        // Arredondamento dos botões para dar mais charme na página inicial.
        private void ArredondarBotao()
        {
            IntPtr hRgn;

            hRgn = CreateRoundRectRgn(0, 0, btnCadastrar.Width, btnCadastrar.Height, 20, 20);
            btnCadastrar.Region = Region.FromHrgn(hRgn);
            DeleteObject(hRgn);

            hRgn = CreateRoundRectRgn(0, 0, btnEntrar.Width, btnEntrar.Height, 20, 20);
            btnEntrar.Region = Region.FromHrgn(hRgn);
            DeleteObject(hRgn);

            hRgn = CreateRoundRectRgn(0, 0, btnEsqueciSenha.Width, btnEsqueciSenha.Height, 20, 20);
            btnEsqueciSenha.Region = Region.FromHrgn(hRgn);
            DeleteObject(hRgn);
        }

        // Arredondamento dos TextBox para dar mais charme na página inicial.
        private void ArredondarTextBox()
        {
            IntPtr hRgn;

            hRgn = CreateRoundRectRgn(0, 0, txtE_mail.Width, txtE_mail.Height, 15, 15);
            txtE_mail.Region = Region.FromHrgn(hRgn);
            DeleteObject(hRgn);

            hRgn = CreateRoundRectRgn(0, 0, txtSenha.Width, txtSenha.Height, 15, 15);
            txtSenha.Region = Region.FromHrgn(hRgn);
            DeleteObject(hRgn);
        }

        public PaginaLogin()
        {
            // Código da Classe panel inserida na página inicial.
            InitializeComponent();
            pictureBox1.Controls.Add(panelTransparente1);
            Controls.Add(pictureBox1);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);

            this.UpdateStyles();
            panelTransparente1.BackColor = Color.FromArgb(100, 160, 220, 190);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Jogo de cores do panel para deixar em uma tonalidade mais esverdeada e transparente.
            panelTransparente1.Region = System.Drawing.Region.FromHrgn(
            CreateRoundRectRgn(0, 0, panelTransparente1.Width, panelTransparente1.Height, 30, 30));

            panelTransparente1.BackColor = Color.FromArgb(100, 160, 220, 190);
            ArredondarBotao();
            ArredondarTextBox();
        }

        // Declaração de uma função externa que existe na biblioteca, também utilizada para arredondar os campos.
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        // DLL para liberar memória (ESSENCIAL)
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        // Função para verificar se o login é válido e retorna o tipo de usuário (Admin ou Cliente)
        private string VerificarLogin()
        {
            using (SqlConnection conn = new SqlConnection(Conexao.StringConexao))
            {
                conn.Open();
                string query = "SELECT TipoUsuario FROM Usuarios WHERE Email = @Email AND Senha = @Senha";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", txtE_mail.Text);
                cmd.Parameters.AddWithValue("@Senha", txtSenha.Text);

                object resultado = cmd.ExecuteScalar();

                if (resultado != null)
                {
                    return resultado.ToString(); // Retorna o TipoUsuario (Admin ou Cliente)
                }

                return null; // Se o login não for válido
            }
        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            TelaDeCadastro Cadastro = new TelaDeCadastro();
            this.Hide();
            Cadastro.ShowDialog();
            this.Show();
        }

        private void btnEntrar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtE_mail.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                MessageBox.Show("Preencha email e senha!");
                return;
            }

            string tipoUsuario = VerificarLogin(); // Verifica o tipo do usuário

            if (tipoUsuario != null)
            {
                emailDoUsuario = txtE_mail.Text; // Guarda o email do usuário
                MessageBox.Show("Login realizado com sucesso!");

                if (tipoUsuario == "Admin")
                {
                    // Se for admin, abre o painel administrativo
                    PainelAdmin admin = new PainelAdmin(emailDoUsuario);
                    this.Hide();
                    admin.ShowDialog();
                    this.Show();
                }
                else
                {
                    // Se for cliente, abre a tela informativa ou agendamento
                    PaginaInformativa tela = new PaginaInformativa(emailDoUsuario);
                    this.Hide();
                    tela.ShowDialog();
                    this.Show();
                }
            }
            else
            {
                MessageBox.Show("Email ou senha incorretos.");
            }
        }
    }
}