using System;
using System.Windows.Forms;
using System.Drawing;

namespace TIManager.Views
{
    public class InventarioControl : UserControl
    {
        public InventarioControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var label = new Label
            {
                Text = "Inventário de Ativos",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };

            this.Controls.Add(label);
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
        }
    }
}
