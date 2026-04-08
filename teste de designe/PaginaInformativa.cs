using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace teste_de_designe
{
    public partial class PaginaInformativa : Form
    {
        private string emailDoUsuario;

        public PaginaInformativa(string email)
        {
            InitializeComponent();
            emailDoUsuario = email;
        }

        private void btnPaginaInformativa_Agendar_Click(object sender, EventArgs e)
        {
            PaginaAgendamentoDoula tela = new PaginaAgendamentoDoula(emailDoUsuario);

            this.Hide();
            tela.ShowDialog();
            this.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
