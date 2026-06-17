using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using TIManager.Views;
using TIManager.Utils;

namespace TIManager
{
    public partial class Form1 : Form
    {
        private UserControl currentControl;
        private bool chamadosExpandido = false;
        private const int ALTURA_SUB_MENU = 140; // 4 botões de 35px

        private bool isSidebarCollapsed = false;
        private System.Windows.Forms.Timer timerSidebar;

        private ToolTip tooltipSidebar;
        private Panel pnlHeader;
        private PictureBox picLogo;
        private Label lblAppName;
        private bool isToggleHovered = false;
        private Image appLogoImage = null;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true; // Habilita captura de teclas (ex: F11)
            this.FormBorderStyle = FormBorderStyle.None; // Remove as bordas nativas
            // A responsabilidade de limites máximos agora está no WM_GETMINMAXINFO
            this.WindowState = FormWindowState.Maximized;
            ConfigurarTooltipSidebar();
            ConfigurarTimerSidebar();
            ConfigurarHeaderSidebar();
            ConfigurarTitleBar(); // Configura a nova barra de título integrada
            AtualizarTextoBotoes(true); // Garante ícones na inicialização
            btnHome_Click(null, EventArgs.Empty);
        }

        private void ConfigurarHeaderSidebar()
        {
            pnlHeader = new Panel();
            pnlHeader.Height = 75; // Reduzido ligeiramente para ajustar o espaçamento
            pnlHeader.Dock = DockStyle.Top;
            
            picLogo = new PictureBox();
            picLogo.Width = 26; // Reduzido para ficar do mesmo tamanho do ícone Home
            picLogo.Height = 26;
            picLogo.Location = new Point(17, 20); // Centralizado horizontalmente em 60px
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            
            lblAppName = new Label();
            lblAppName.Text = "TI Manager";
            lblAppName.ForeColor = Color.White;
            lblAppName.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblAppName.AutoSize = true;
            lblAppName.Location = new Point(picLogo.Right + 12, 21); // Alinhado verticalmente com o ícone
            
            string imgPath = @"C:\Users\TI CIRAS\.project\TIManager\TIManager\Resources\TIManagerIconMenu.png";
            if (System.IO.File.Exists(imgPath)) 
            {
                appLogoImage = Image.FromFile(imgPath);
                picLogo.Image = appLogoImage;
            }

            btnToggleSidebar.MouseEnter += (s, e) => {
                isToggleHovered = true;
                btnToggleSidebar.Invalidate();
            };

            btnToggleSidebar.MouseLeave += (s, e) => {
                isToggleHovered = false;
                btnToggleSidebar.Invalidate();
            };

            btnToggleSidebar.Dock = DockStyle.None;
            btnToggleSidebar.Width = 30;
            btnToggleSidebar.Height = 30;
            btnToggleSidebar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnToggleSidebar.Text = ""; // O ícone será desenhado no evento Paint
            btnToggleSidebar.Paint += BtnToggleSidebar_Paint;
            
            // Posição inicial (250px - 15px de margem direita - 30px largura = 205)
            btnToggleSidebar.Location = new Point(205, 18);

            panelSidebar.Controls.Remove(btnToggleSidebar);
            if (lblLogo != null)
            {
                panelSidebar.Controls.Remove(lblLogo);
                lblLogo.Visible = false;
            }
            
            pnlHeader.Controls.Add(picLogo);
            pnlHeader.Controls.Add(lblAppName);
            pnlHeader.Controls.Add(btnToggleSidebar);
            
            panelSidebar.Controls.Add(pnlHeader);
            pnlHeader.SendToBack(); // Envia para o fundo do Z-Order, fazendo com que ancore no topo real
        }

        private void ConfigurarTooltipSidebar()
        {
            tooltipSidebar = new ToolTip();
            tooltipSidebar.OwnerDraw = true;
            tooltipSidebar.Draw += TooltipSidebar_Draw;
            tooltipSidebar.Popup += TooltipSidebar_Popup;

            btnToggleSidebar.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            btnToggleSidebar.FlatAppearance.MouseDownBackColor = Color.FromArgb(50, 255, 255, 255);
        }

        private void TooltipSidebar_Popup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedControl == btnToggleSidebar)
            {
                string text = tooltipSidebar.GetToolTip(btnToggleSidebar);
                Size textSize = TextRenderer.MeasureText(text, new Font("Segoe UI", 9, FontStyle.Bold));
                e.ToolTipSize = new Size(textSize.Width + 20, textSize.Height + 14);
            }
        }

        private void TooltipSidebar_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Desenha um fundo arredondado preto (estilo Gemini)
            using (var brush = new SolidBrush(Color.Black))
            {
                Rectangle rect = new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
                using (var path = UIHelper.GetRoundedRectanglePath(rect, 6))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }
            
            TextRenderer.DrawText(e.Graphics, e.ToolTipText, new Font("Segoe UI", 9, FontStyle.Bold), e.Bounds, Color.White, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void BtnToggleSidebar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Só é visualmente "colapsado" se estiver fechado e parado, ou se estiver encolhendo e já passou do meio
            bool isVisualCollapsed = (isSidebarCollapsed && !UIHelper.IsSidebarAnimating) || 
                                     (!isSidebarCollapsed && UIHelper.IsSidebarAnimating && panelSidebar.Width <= 150);

            if (isVisualCollapsed && !isToggleHovered)
            {
                // Quando colapsado e sem hover, o botão simula ser o ícone do app
                if (appLogoImage != null)
                {
                    e.Graphics.DrawImage(appLogoImage, new Rectangle(2, 2, 26, 26));
                }
            }
            else
            {
                // Desenha um ícone de menu lateral (estilo painel UI)
                using (Pen pen = new Pen(Color.White, 1.8f))
                {
                    Rectangle box = new Rectangle(6, 6, 18, 18);
                    using (var path = UIHelper.GetRoundedRectanglePath(box, 4))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                    
                    // Linha vertical dividindo o painel (simulando a barra lateral)
                    e.Graphics.DrawLine(pen, 13, 6, 13, 24);
                }
            }
        }

        private void ConfigurarTimerSidebar()
        {
            timerSidebar = new System.Windows.Forms.Timer();
            timerSidebar.Interval = 10;
            timerSidebar.Tick += TimerSidebar_Tick;
        }

        private void TimerSidebar_Tick(object sender, EventArgs e)
        {
            if (isSidebarCollapsed)
            {
                // Expandindo
                int step = (250 - panelSidebar.Width) / 4;
                if (step < 2) step = 2;

                panelSidebar.Width += step;

                if (panelSidebar.Width >= 250)
                {
                    panelSidebar.Width = 250;
                    timerSidebar.Stop();
                    UIHelper.IsSidebarAnimating = false;
                    isSidebarCollapsed = false; 
                    btnToggleSidebar.Invalidate(); // Garante o redesenho final
                    this.OnResize(EventArgs.Empty);
                }
                AnimarOpacidadeTexto(panelSidebar.Width);
            }
            else
            {
                // Colapsando
                int step = (panelSidebar.Width - 60) / 4;
                if (step < 2) step = 2;

                panelSidebar.Width -= step;

                if (panelSidebar.Width <= 150)
                {
                    if (picLogo != null && picLogo.Visible) 
                    {
                        AtualizarTextoBotoes(false);
                    }
                }

                if (panelSidebar.Width <= 60)
                {
                    panelSidebar.Width = 60;
                    timerSidebar.Stop();
                    UIHelper.IsSidebarAnimating = false;
                    isSidebarCollapsed = true;
                    btnToggleSidebar.Invalidate(); // Força o botão a desenhar a logo
                    this.OnResize(EventArgs.Empty);
                }
                AnimarOpacidadeTexto(panelSidebar.Width);
            }
        }

        private void AnimarOpacidadeTexto(int larguraAtual)
        {
            // Calcula um fator de 0 a 255 baseado na largura (entre 60 e 250)
            int alpha = (int)(((larguraAtual - 60) / 190.0) * 255);
            if (alpha < 0) alpha = 0;
            if (alpha > 255) alpha = 255;

            Color textoCor = Color.FromArgb(alpha, Color.White);
            
            // Aplica a cor suavizada a todos os botões principais
            btnHome.ForeColor = textoCor;
            btnChamados.ForeColor = textoCor;
            btnInventario.ForeColor = textoCor;
            btnWiki.ForeColor = textoCor;
            btnCredenciais.ForeColor = textoCor;
            btnRelatorios.ForeColor = textoCor;
        }

        private void AtualizarTextoBotoes(bool expandido)
        {
            string seta = chamadosExpandido ? "▾" : "▸";
            
            btnHome.Text = expandido ? "  🏠   Home" : "🏠";
            btnChamados.Text = expandido ? $"  🎫   Chamados   {seta}" : "🎫";
            btnInventario.Text = expandido ? "  📦   Inventário" : "📦";
            btnWiki.Text = expandido ? "  📖   Wiki" : "📖";
            btnCredenciais.Text = expandido ? "  🔑   Credenciais" : "🔑";
            btnRelatorios.Text = expandido ? "  📊   Relatórios" : "📊";

            var alinhamento = expandido ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleCenter;
            btnHome.TextAlign = alinhamento;
            btnChamados.TextAlign = alinhamento;
            btnInventario.TextAlign = alinhamento;
            btnWiki.TextAlign = alinhamento;
            btnCredenciais.TextAlign = alinhamento;
            btnRelatorios.TextAlign = alinhamento;
            
            pnlSubMenuChamados.Visible = expandido && chamadosExpandido;
            
            if (expandido)
            {
                if (picLogo != null) picLogo.Visible = true;
                if (lblAppName != null) lblAppName.Visible = true;
                btnToggleSidebar.Visible = true;
                btnToggleSidebar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnToggleSidebar.Location = new Point(pnlHeader.Width - 45, 18);
                if (tooltipSidebar != null) tooltipSidebar.SetToolTip(btnToggleSidebar, "Fechar barra lateral");
            }
            else
            {
                if (picLogo != null) picLogo.Visible = false; // Esconde o picLogo real
                if (lblAppName != null) lblAppName.Visible = false;
                btnToggleSidebar.Visible = true; // Botão fica SEMPRE visível no modo colapsado
                btnToggleSidebar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                btnToggleSidebar.Location = new Point(15, 18);
                if (tooltipSidebar != null) tooltipSidebar.SetToolTip(btnToggleSidebar, "Abrir barra lateral");
                btnToggleSidebar.Invalidate(); // Força redesenho
            }
        }

        private void btnToggleSidebar_Click(object sender, EventArgs e)
        {
            if (UIHelper.IsSidebarAnimating) return; // Previne múltiplos cliques
            
            UIHelper.IsSidebarAnimating = true; 
            
            if (isSidebarCollapsed)
            {
                // Mostra a logo e o texto instantaneamente antes de começar a expandir
                AtualizarTextoBotoes(true);
            }
            
            timerSidebar.Start();
        }

        private async void ShowControl(UserControl control)
        {
            if (currentControl != null)
            {
                panelContent.Controls.Remove(currentControl);
                currentControl.Dispose();
            }

            currentControl = control;
            currentControl.Dock = DockStyle.Fill;
            currentControl.Left = 20; 
            panelContent.Controls.Add(currentControl);
            currentControl.BringToFront(); // Garante que o painel de conteúdo fique atrás do TitleBar no Z-Order de Docking

            for (int i = 0; i < 5; i++)
            {
                currentControl.Left -= 4;
                await Task.Delay(10);
            }
            currentControl.Left = 0;
        }

        private void EnsureChamadosAndFilter(string status)
        {
            if (!(currentControl is ChamadosControl))
            {
                ShowControl(new ChamadosControl());
            }
            
            if (currentControl is ChamadosControl c)
            {
                c.FiltrarPorStatus(status);
            }
        }

        private async void ToggleChamadosMenu()
        {
            chamadosExpandido = !chamadosExpandido;
            AtualizarTextoBotoes(!isSidebarCollapsed); 
            
            int alvo = chamadosExpandido ? ALTURA_SUB_MENU : 0;

            while (pnlSubMenuChamados.Height != alvo)
            {
                int step = (alvo - pnlSubMenuChamados.Height) / 4;
                if (step > 0 && step < 2) step = 2;
                if (step < 0 && step > -2) step = -2;

                pnlSubMenuChamados.Height += step;

                if (chamadosExpandido && pnlSubMenuChamados.Height > alvo) pnlSubMenuChamados.Height = alvo;
                if (!chamadosExpandido && pnlSubMenuChamados.Height < alvo) pnlSubMenuChamados.Height = alvo;

                await Task.Delay(10);
            }
        }

        private void btnHome_Click(object sender, EventArgs e) 
        {
            var home = new HomeControl();
            home.OnVerDetalhes += (id) => {
                var chamados = new ChamadosControl();
                ShowControl(chamados);
                chamados.SelecionarEAbrirChamado(id);
            };
            ShowControl(home);
        }
        
        private void btnChamados_Click(object sender, EventArgs e) 
        {
            ToggleChamadosMenu();
            EnsureChamadosAndFilter("Todos");
        }

        private void btnChamadosTodos_Click(object sender, EventArgs e) => EnsureChamadosAndFilter("Todos");
        private void btnChamadosAbertos_Click(object sender, EventArgs e) => EnsureChamadosAndFilter("Aberto");
        private void btnChamadosAtendimento_Click(object sender, EventArgs e) => EnsureChamadosAndFilter("Em Atendimento");
        private void btnChamadosFinalizados_Click(object sender, EventArgs e) => EnsureChamadosAndFilter("Finalizado");
        
        private void btnInventario_Click(object sender, EventArgs e) => ShowControl(new InventarioControl());
        private void btnWiki_Click(object sender, EventArgs e) => ShowControl(new WikiControl());
        private void btnCredenciais_Click(object sender, EventArgs e) => ShowControl(new CredenciaisControl());
        private void btnRelatorios_Click(object sender, EventArgs e) => ShowControl(new RelatoriosControl());

        // --- Custom Title Bar & Window Resize ---
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void ArrastarJanela(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        } // <- Missing brace added here

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCCALCSIZE = 0x0083;
            const int WM_NCHITTEST = 0x0084;
            const int WM_GETMINMAXINFO = 0x0024;
            const int resizeAreaSize = 10;

            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            // Remove a borda branca/gap nativo completamente
            if (m.Msg == WM_NCCALCSIZE && m.WParam.ToInt32() == 1)
            {
                m.Result = IntPtr.Zero;
                return;
            }

            // Conserta o bug do monitor em modo retrato e bleed de 8px ao maximizar
            if (m.Msg == WM_GETMINMAXINFO)
            {
                base.WndProc(ref m);
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO));
                
                Screen screen = Screen.FromHandle(this.Handle);
                Rectangle workingArea = screen.WorkingArea;
                Rectangle monitorArea = screen.Bounds;

                mmi.ptMaxPosition.X = Math.Abs(workingArea.Left - monitorArea.Left);
                mmi.ptMaxPosition.Y = Math.Abs(workingArea.Top - monitorArea.Top);
                mmi.ptMaxSize.X = workingArea.Width;
                mmi.ptMaxSize.Y = workingArea.Height;

                Marshal.StructureToPtr(mmi, m.LParam, true);
                return;
            }

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if (this.WindowState == FormWindowState.Normal && (int)m.Result == HTCLIENT)
                {
                    Point cursor = this.PointToClient(Cursor.Position);
                    if (cursor.X <= resizeAreaSize && cursor.Y <= resizeAreaSize)
                        m.Result = (IntPtr)HTTOPLEFT;
                    else if (cursor.X >= this.ClientSize.Width - resizeAreaSize && cursor.Y <= resizeAreaSize)
                        m.Result = (IntPtr)HTTOPRIGHT;
                    else if (cursor.X <= resizeAreaSize && cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                        m.Result = (IntPtr)HTBOTTOMLEFT;
                    else if (cursor.X >= this.ClientSize.Width - resizeAreaSize && cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                        m.Result = (IntPtr)HTBOTTOMRIGHT;
                    else if (cursor.X <= resizeAreaSize)
                        m.Result = (IntPtr)HTLEFT;
                    else if (cursor.X >= this.ClientSize.Width - resizeAreaSize)
                        m.Result = (IntPtr)HTRIGHT;
                    else if (cursor.Y <= resizeAreaSize)
                        m.Result = (IntPtr)HTTOP;
                    else if (cursor.Y >= this.ClientSize.Height - resizeAreaSize)
                        m.Result = (IntPtr)HTBOTTOM;
                }
                return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20000; // CS_DROPSHADOW
                
                // Ativa o Aero Snap completamente e bordas nativas redimensionáveis
                cp.Style |= 0x00C00000; // WS_CAPTION (Fundamental para evitar o tema clássico do Windows 98)
                cp.Style |= 0x00040000; // WS_THICKFRAME
                cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                cp.Style |= 0x00010000; // WS_MAXIMIZEBOX
                cp.Style |= 0x00080000; // WS_SYSMENU
                
                return cp;
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            // Removido o this.MaximizedBounds pois o WM_GETMINMAXINFO agora gerencia perfeitamente
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.F11)
            {
                if (this.WindowState == FormWindowState.Maximized && this.MaximizedBounds == Rectangle.Empty)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.MaximizedBounds = Rectangle.Empty; // Tela Cheia ignorando barra de tarefas
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        }

        private void ConfigurarTitleBar()
        {
            Panel pnlTitleBar = new Panel();
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.Height = 32;
            pnlTitleBar.BackColor = panelContent.BackColor;
            pnlTitleBar.MouseDown += ArrastarJanela;

            Button btnClose = CriarBotaoTitleBar("\uE8BB", Color.White, Color.Crimson);
            btnClose.Click += (s, e) => Application.Exit();
            
            Button btnMax = CriarBotaoTitleBar("\uE922", Color.FromArgb(40, 40, 40), Color.FromArgb(210, 210, 210));
            btnMax.Click += (s, e) => {
                this.WindowState = this.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
            };

            pnlTitleBar.DoubleClick += (s, e) => btnMax.PerformClick();

            Button btnMin = CriarBotaoTitleBar("\uE921", Color.FromArgb(40, 40, 40), Color.FromArgb(210, 210, 210));
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            pnlTitleBar.Controls.Add(btnMin);
            pnlTitleBar.Controls.Add(btnMax);
            pnlTitleBar.Controls.Add(btnClose);
            
            panelContent.Controls.Add(pnlTitleBar);
            pnlTitleBar.SendToBack(); // Garante que a barra empurre o conteúdo pra baixo (Dock Top)
            
            // Atualiza o ícone ao redimensionar
            this.Resize += (s, e) => {
                btnMax.Text = this.WindowState == FormWindowState.Maximized ? "\uE923" : "\uE922";
            };
            
            // Permite arrastar segurando o painel do cabeçalho da lateral também
            pnlHeader.MouseDown += ArrastarJanela;
            if (lblAppName != null) lblAppName.MouseDown += ArrastarJanela;
            if (picLogo != null) picLogo.MouseDown += ArrastarJanela;
        }

        private Button CriarBotaoTitleBar(string text, Color colorHover, Color backHover)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Dock = DockStyle.Right;
            btn.Width = 45;
            btn.Font = new Font("Segoe MDL2 Assets", 10F, FontStyle.Regular);
            btn.ForeColor = Color.Gray;
            btn.BackColor = Color.Transparent;
            btn.Cursor = Cursors.Arrow;

            btn.MouseEnter += (s, e) => {
                btn.BackColor = backHover;
                btn.ForeColor = colorHover;
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.Transparent;
                btn.ForeColor = Color.Gray;
            };

            return btn;
        }
    }
}
