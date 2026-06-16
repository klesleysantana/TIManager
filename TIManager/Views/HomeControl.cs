using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using TIManager.Models;
using TIManager.Utils;

namespace TIManager.Views
{
    // ─── Panel de Barras ──────────────────────────────────────────────────────
    public class BarChartPanel : Panel
    {
        private List<Chamado> _chamados = new List<Chamado>();
        private static readonly Color ColorPrimary   = ColorTranslator.FromHtml("#1B676B");
        private static readonly Color ColorAccent    = ColorTranslator.FromHtml("#88C425");
        private static readonly Color ColorGrid      = Color.FromArgb(220, 230, 230);
        private static readonly Color ColorText      = Color.FromArgb(80, 80, 80);

        public BarChartPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        public void SetData(List<Chamado> chamados)
        {
            _chamados = chamados ?? new List<Chamado>();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            const int marginLeft = 50, marginRight = 20, marginTop = 20, marginBottom = 40;
            var chartRect = new Rectangle(marginLeft, marginTop, Width - marginLeft - marginRight, Height - marginTop - marginBottom);

            g.FillRectangle(Brushes.White, this.ClientRectangle);

            if (_chamados.Count == 0 || chartRect.Width <= 0 || chartRect.Height <= 0)
            {
                using (var br = new SolidBrush(ColorText))
                using (var f = new Font("Segoe UI", 10))
                    g.DrawString("Sem dados", f, br, chartRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            var hoje = DateTime.Today;
            var meses = Enumerable.Range(0, 6).Select(i => hoje.AddMonths(-5 + i)).ToList();
            var grupos = meses.Select(m => new {
                Label  = m.ToString("MMM/yy"),
                Count  = _chamados.Count(c => c.DataAbertura.Year == m.Year && c.DataAbertura.Month == m.Month)
            }).ToList();

            int maxVal = Math.Max(grupos.Max(g2 => g2.Count), 1);

            using (var penGrid = new Pen(ColorGrid, 1))
            using (var brText = new SolidBrush(ColorText))
            using (var fSmall = new Font("Segoe UI", 8))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int yy = chartRect.Bottom - (int)(chartRect.Height * i / 4.0);
                    g.DrawLine(penGrid, chartRect.Left, yy, chartRect.Right, yy);
                    int labelVal = (int)(maxVal * i / 4.0);
                    g.DrawString(labelVal.ToString(), fSmall, brText, new RectangleF(0, yy - 8, marginLeft - 5, 16), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                }
            }

            float slotW = chartRect.Width / (float)grupos.Count;
            float barW = slotW * 0.5f;

            for (int i = 0; i < grupos.Count; i++)
            {
                float barH = chartRect.Height * (grupos[i].Count / (float)maxVal);
                float x = chartRect.Left + i * slotW + (slotW - barW) / 2f;
                float y = chartRect.Bottom - barH;

                if (barH > 0)
                {
                    using (var lgb = new LinearGradientBrush(new PointF(x, y), new PointF(x, chartRect.Bottom), ColorAccent, ColorPrimary))
                    {
                        var path = RoundedTopRect(new RectangleF(x, y, barW, barH), 6);
                        g.FillPath(lgb, path);
                    }
                    using (var fVal = new Font("Segoe UI", 8f, FontStyle.Bold))
                        g.DrawString(grupos[i].Count.ToString(), fVal, new SolidBrush(ColorPrimary), new RectangleF(x, y - 18, barW, 16), new StringFormat { Alignment = StringAlignment.Center });
                }

                using (var fLbl = new Font("Segoe UI", 8))
                    g.DrawString(grupos[i].Label, fLbl, new SolidBrush(ColorText), new RectangleF(x - 10, chartRect.Bottom + 5, slotW + 20, 20), new StringFormat { Alignment = StringAlignment.Center });
            }
        }

        private GraphicsPath RoundedTopRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float r = radius;
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddLine(rect.Right, rect.Y + r, rect.Right, rect.Bottom);
            path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
            path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + r);
            path.CloseFigure();
            return path;
        }
    }

    // ─── Panel de Pizza Genérico ──────────────────────────────────────────────
    public class PieChartPanel : Panel
    {
        private List<Chamado> _chamados = new List<Chamado>();
        private Func<Chamado, string> _groupSelector;

        private static readonly Color[] Palette = new[] {
            Color.FromArgb(30, 144, 255), Color.FromArgb(138, 43, 226), 
            ColorTranslator.FromHtml("#88C425"), Color.FromArgb(255, 165, 0),
            Color.FromArgb(255, 105, 180), Color.FromArgb(0, 206, 209),
            Color.FromArgb(180, 180, 180)
        };

        public PieChartPanel(Func<Chamado, string> selector)
        {
            _groupSelector = selector;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
        }

        public void SetData(List<Chamado> chamados)
        {
            _chamados = chamados ?? new List<Chamado>();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            g.FillRectangle(Brushes.White, this.ClientRectangle);
            if (_chamados.Count == 0) return;

            var grupos = _chamados.GroupBy(_groupSelector)
                .Select(grp => new { Name = grp.Key ?? "N/A", Count = grp.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();

            if (grupos.Count == 0) return;
            int total = grupos.Sum(x => x.Count);
            if (total == 0) return;

            int pieSize = Math.Min(Width / 2, Height) - 10;
            if (pieSize < 40) pieSize = 40;
            
            int pieX = 5, pieY = (Height - pieSize) / 2;
            float startAngle = -90f;

            for (int i = 0; i < grupos.Count; i++)
            {
                float sweep = 360f * grupos[i].Count / total;
                using (var br = new SolidBrush(Palette[i % Palette.Length]))
                    g.FillPie(br, pieX, pieY, pieSize, pieSize, startAngle, sweep);
                using (var pen = new Pen(Color.White, 1.5f))
                    g.DrawPie(pen, pieX, pieY, pieSize, pieSize, startAngle, sweep);
                startAngle += sweep;
            }

            int lx = pieX + pieSize + 10;
            int ly = (Height - grupos.Count * 20) / 2;
            if (lx > Width - 20) lx = Width - 20;

            using (var f = new Font("Segoe UI", 8f))
            {
                for (int i = 0; i < grupos.Count; i++)
                {
                    int yy = ly + i * 20;
                    if (yy + 15 > Height) break;
                    using (var br = new SolidBrush(Palette[i % Palette.Length]))
                        g.FillRectangle(br, lx, yy + 4, 10, 10);
                    string txt = $"{grupos[i].Name} ({grupos[i].Count})";
                    g.DrawString(txt, f, Brushes.DimGray, lx + 14, yy + 2);
                }
            }
        }
    }

    // ─── HomeControl Reformulada ──────────────────────────────────────────────
    public class HomeControl : UserControl
    {
        private BarChartPanel barChart;
        private PieChartPanel pieStatus;
        private PieChartPanel pieSetor;
        private FlowLayoutPanel pnlRecent;
        private Panel pnlChartsContainer;
        
        public static List<Chamado> ChamadosGlobais { get; set; } = new List<Chamado>();
        public event Action<int> OnVerDetalhes;

        public HomeControl()
        {
            InitializeComponent();
            CarregarDados();
            AtualizarTudo();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(245, 248, 248);
            this.Padding = new Padding(25);

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68f));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32f));

            var pnlLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 15, 0), AutoScroll = true };
            UIHelper.SetDoubleBuffered(pnlLeft);

            var lblDashTitle = new Label { Text = "Dashboard", Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1B676B"), Dock = DockStyle.Top, Height = 45 };
            var lblDashSub = new Label { Text = "Visão geral dos chamados", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 30 };

            pnlChartsContainer = new Panel { 
                Dock = DockStyle.Top, 
                Height = 610, // Altura fixa para evitar clipping em containers AutoSize
                BackColor = Color.FromArgb(238, 242, 242),
                Padding = new Padding(20),
                Margin = new Padding(0, 10, 0, 0)
            };
            
            // ATUALIZAÇÃO CRÍTICA: Recalcula os cantos arredondados sempre que redimensionar
            pnlChartsContainer.Resize += (s, e) => UIHelper.ArredondarCantos(pnlChartsContainer, 20);

            var cardBar = CriarCard("📊 Chamados por Mês", 280);
            cardBar.Margin = new Padding(0, 0, 0, 35);
            barChart = new BarChartPanel { Dock = DockStyle.Fill };
            ((Panel)cardBar.Tag).Controls.Add(barChart);

            var pnlPies = new TableLayoutPanel { 
                Dock = DockStyle.Top, 
                Height = 250, 
                ColumnCount = 2, 
                RowCount = 1, 
                Margin = new Padding(0), 
                BackColor = Color.Transparent 
            };
            pnlPies.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlPies.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            var cardStatus = CriarCard("🍕 Por Status", 230);
            cardStatus.Margin = new Padding(0, 0, 8, 0);
            pieStatus = new PieChartPanel(c => c.Status) { Dock = DockStyle.Fill };
            ((Panel)cardStatus.Tag).Controls.Add(pieStatus);

            var cardSetor = CriarCard("🏢 Por Setor", 230);
            cardSetor.Margin = new Padding(8, 0, 0, 0);
            pieSetor = new PieChartPanel(c => c.Setor) { Dock = DockStyle.Fill };
            ((Panel)cardSetor.Tag).Controls.Add(pieSetor);

            pnlPies.Controls.Add(cardStatus, 0, 0);
            pnlPies.Controls.Add(cardSetor, 1, 0);

            pnlChartsContainer.Controls.Add(pnlPies);
            pnlChartsContainer.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 25, BackColor = Color.Transparent });
            pnlChartsContainer.Controls.Add(cardBar);

            pnlLeft.Controls.Add(pnlChartsContainer);
            pnlLeft.Controls.Add(lblDashSub);
            pnlLeft.Controls.Add(lblDashTitle);

            var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 0, 0, 0) };

            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 45, BackColor = Color.White, Padding = new Padding(10, 5, 5, 5) };
            pnlSearch.Paint += (s, e) => {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                using(var p = new Pen(Color.LightGray, 1)) g.DrawPath(p, UIHelper.GetRoundedRectanglePath(new Rectangle(0,0,pnlSearch.Width-1,pnlSearch.Height-1), 20));
            };
            var txtSearch = new TextBox { Text = "Pesquisar chamados...", ForeColor = Color.Gray, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Width = 180, Location = new Point(15, 12) };
            var btnSearch = new Button { Text = "Search", FlatStyle = FlatStyle.Flat, BackColor = ColorTranslator.FromHtml("#88C425"), ForeColor = Color.White, Dock = DockStyle.Right, Width = 80 };
            btnSearch.FlatAppearance.BorderSize = 0;
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnSearch);

            var lblRecentTitle = new Label { Text = "Chamados Recentes", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(60, 60, 60), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.BottomLeft, Padding = new Padding(0,0,0,10) };

            pnlRecent = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

            pnlRight.Controls.Add(pnlRecent);
            pnlRight.Controls.Add(lblRecentTitle);
            pnlRight.Controls.Add(pnlSearch);

            mainTable.Controls.Add(pnlLeft, 0, 0);
            mainTable.Controls.Add(pnlRight, 1, 0);
            this.Controls.Add(mainTable);

            // Garante que o layout seja forçado quando o controle for mostrado
            this.Load += (s, e) => {
                this.PerformLayout();
                pnlChartsContainer.Refresh();
            };
        }

        private Panel CriarCard(string titulo, int height)
        {
            var card = new Panel { Dock = DockStyle.Top, Height = height, BackColor = Color.White, Margin = new Padding(0, 0, 0, 20), Padding = new Padding(1) };
            var header = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(15, 0, 0, 0) };
            var lbl = new Label { Text = titulo, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#1B676B"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var line = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = ColorTranslator.FromHtml("#1B676B") };
            var body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            header.Controls.Add(lbl);
            card.Controls.Add(body);
            card.Controls.Add(header);
            card.Controls.Add(line);
            card.Tag = body;
            
            card.Paint += (s, e) => {
                var g = e.Graphics;
                using(var p = new Pen(Color.FromArgb(230, 230, 230))) g.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            };
            return card;
        }

        private void CarregarDados()
        {
            ChamadosGlobais = TIManager.Data.DatabaseService.Instance.GetAllChamados();
        }

        private void AtualizarTudo()
        {
            barChart.SetData(ChamadosGlobais);
            pieStatus.SetData(ChamadosGlobais);
            pieSetor.SetData(ChamadosGlobais);
            AtualizarListaRecentes();
        }

        private void AtualizarListaRecentes()
        {
            pnlRecent.Controls.Clear();
            var recentes = ChamadosGlobais.Take(6).ToList();

            if (recentes.Count == 0)
            {
                var lblEmpty = new Label { Text = "Nenhum chamado recente", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Margin = new Padding(10) };
                pnlRecent.Controls.Add(lblEmpty);
                return;
            }

            foreach (var cham in recentes)
            {
                // Usamos uma largura fixa inicial ou a largura do painel se já existir
                int itemWidth = Math.Max(pnlRecent.Width - 25, 200);

                var item = new Panel { Width = itemWidth, Height = 70, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10), Cursor = Cursors.Hand };
                
                // Força o item a ocupar a largura do pai quando o painel for redimensionado
                pnlRecent.SizeChanged += (s, e) => {
                    item.Width = pnlRecent.Width - 25;
                };

                item.Paint += (s, e) => {
                    var g = e.Graphics;
                    using(var p = new Pen(Color.FromArgb(240, 240, 240))) g.DrawLine(p, 0, 69, item.Width, 69);
                };

                var lblT = new Label { 
                    Text = cham.Titulo, 
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), 
                    ForeColor = Color.FromArgb(50, 50, 50), 
                    AutoSize = false, 
                    Width = item.Width - 20, 
                    Height = 25, 
                    Location = new Point(10, 12), 
                    AutoEllipsis = true 
                };
                var lblS = new Label { 
                    Text = $"{cham.Setor} • {cham.DataAbertura:dd/MM HH:mm}", 
                    Font = new Font("Segoe UI", 8.5f), 
                    ForeColor = Color.Gray, 
                    Location = new Point(10, 38), 
                    AutoSize = true 
                };
                
                item.Controls.Add(lblT);
                item.Controls.Add(lblS);

                // Efeito Hover
                item.MouseEnter += (s, e) => item.BackColor = Color.FromArgb(250, 252, 252);
                item.MouseLeave += (s, e) => item.BackColor = Color.White;
                
                // Navegação ao clicar
                Action click = () => OnVerDetalhes?.Invoke(cham.Id);
                item.Click += (s, e) => click();
                lblT.Click += (s, e) => click();
                lblS.Click += (s, e) => click();

                pnlRecent.Controls.Add(item);
            }
        }

        public void RefreshData(List<Chamado> chamados)
        {
            ChamadosGlobais = chamados;
            AtualizarTudo();
        }
    }
}
