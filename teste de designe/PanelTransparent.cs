using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
//Classe para deixar painel Transparente , para deixar com uma cor mais clara entre os locais de digitação
//e dar mais leveza/calma de forma indireta na pagina de Login.
//E tambem pq os jogos de cores fazem parte da marca da Empresa!
namespace teste_de_designe
{
    public class PanelTransparente : Panel
    {
        public PanelTransparente()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.SupportsTransparentBackColor, true);

            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(80,180,230,200);
        }
    }
}
