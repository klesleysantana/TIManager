using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
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

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            ConfigurarTimerSidebar();
            AtualizarTextoBotoes(true); // Garante ícones na inicialização
            btnHome_Click(null, EventArgs.Empty);
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
                panelSidebar.Width += 35;
                AnimarOpacidadeTexto(panelSidebar.Width);

                if (panelSidebar.Width >= 250)
                {
                    panelSidebar.Width = 250;
                    timerSidebar.Stop();
                    UIHelper.IsSidebarAnimating = false;
                    isSidebarCollapsed = false; // Restaurado: Agora ele sabe que está aberto
                    AtualizarTextoBotoes(true);
                    this.OnResize(EventArgs.Empty);
                }
            }
            else
            {
                panelSidebar.Width -= 35;
                AnimarOpacidadeTexto(panelSidebar.Width);

                if (panelSidebar.Width <= 150) // Esconde o texto um pouco antes para não "apertar"
                {
                    AtualizarTextoBotoes(false);
                }

                if (panelSidebar.Width <= 60)
                {
                    panelSidebar.Width = 60;
                    timerSidebar.Stop();
                    UIHelper.IsSidebarAnimating = false; // Fim da animação
                    isSidebarCollapsed = true;
                    this.OnResize(EventArgs.Empty); // Força um redimensionamento final
                }
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
            
            lblLogo.Visible = expandido;
            pnlSubMenuChamados.Visible = expandido && chamadosExpandido;
            
            btnToggleSidebar.Text = expandido ? "«" : "»";
        }

        private void btnToggleSidebar_Click(object sender, EventArgs e)
        {
            UIHelper.IsSidebarAnimating = true; // Início da animação
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
            AtualizarTextoBotoes(!isSidebarCollapsed); // Atualiza ícone e seta (▸/▾)
            
            int alvo = chamadosExpandido ? ALTURA_SUB_MENU : 0;
            int passo = 14; // Velocidade da animação

            while (pnlSubMenuChamados.Height != alvo)
            {
                if (chamadosExpandido)
                {
                    pnlSubMenuChamados.Height = Math.Min(alvo, pnlSubMenuChamados.Height + passo);
                }
                else
                {
                    pnlSubMenuChamados.Height = Math.Max(alvo, pnlSubMenuChamados.Height - passo);
                }
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
    }
}
