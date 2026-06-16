using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace TIManager.Views
{
    public class MonthYearPickerForm : Form
    {
        public int SelectedMonth { get; private set; }
        public int SelectedYear { get; private set; }
        public bool Confirmed { get; private set; }

        private Label lblYear;
        private int currentYear;
        private FlowLayoutPanel pnlMonths;
        private string[] monthsNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        public MonthYearPickerForm(int initialYear)
        {
            this.currentYear = initialYear;
            InitializeComponent();
            this.Confirmed = false;
        }

        private void InitializeComponent()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.White;
            this.Size = new Size(250, 250);
            this.ShowInTaskbar = false;

            // Borda sutil
            this.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Color.LightGray, 1), 0, 0, this.Width - 1, this.Height - 1);
            };

            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(10) };
            
            lblYear = new Label {
                Text = currentYear.ToString(),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true,
                Location = new Point(20, 12)
            };

            var btnPrev = new Button { 
                Text = "↑", 
                Size = new Size(30, 30), 
                Location = new Point(170, 10),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnPrev.Click += (s, e) => { currentYear--; lblYear.Text = currentYear.ToString(); };

            var btnNext = new Button { 
                Text = "↓", 
                Size = new Size(30, 30), 
                Location = new Point(210, 10),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            btnNext.Click += (s, e) => { currentYear++; lblYear.Text = currentYear.ToString(); };

            pnlHeader.Controls.Add(lblYear);
            pnlHeader.Controls.Add(btnPrev);
            pnlHeader.Controls.Add(btnNext);
            this.Controls.Add(pnlHeader);

            pnlMonths = new FlowLayoutPanel { 
                Dock = DockStyle.Fill, 
                Padding = new Padding(10),
                BackColor = Color.White
            };

            for (int i = 1; i <= 12; i++)
            {
                int monthNum = i;
                var btn = new Button {
                    Text = monthsNames[i - 1],
                    Size = new Size(50, 45),
                    Margin = new Padding(4),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Cursor = Cursors.Hand
                };

                btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(232, 240, 250);
                btn.MouseLeave += (s, e) => btn.BackColor = Color.White;
                
                btn.Click += (s, e) => {
                    SelectedMonth = monthNum;
                    SelectedYear = currentYear;
                    Confirmed = true;
                    this.Close();
                };

                pnlMonths.Controls.Add(btn);
            }

            this.Controls.Add(pnlMonths);

            // Fechar ao perder o foco
            this.Deactivate += (s, e) => this.Close();
        }
    }
}
