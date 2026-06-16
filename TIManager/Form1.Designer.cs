namespace TIManager;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Panel panelSidebar;
    private System.Windows.Forms.Panel panelContent;
    private System.Windows.Forms.Panel pnlSubMenuChamados;
    private System.Windows.Forms.Button btnHome, btnChamados, btnInventario, btnWiki, btnCredenciais, btnRelatorios;
    private System.Windows.Forms.Button btnChamadosTodos, btnChamadosAbertos, btnChamadosAtendimento, btnChamadosFinalizados;
    private System.Windows.Forms.Button btnToggleSidebar;
    private System.Windows.Forms.Label lblLogo;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.panelSidebar = new System.Windows.Forms.Panel();
        this.panelContent = new System.Windows.Forms.Panel();
        this.pnlSubMenuChamados = new System.Windows.Forms.Panel();
        this.btnHome = new System.Windows.Forms.Button();
        this.btnChamados = new System.Windows.Forms.Button();
        this.btnChamadosTodos = new System.Windows.Forms.Button();
        this.btnChamadosAbertos = new System.Windows.Forms.Button();
        this.btnChamadosAtendimento = new System.Windows.Forms.Button();
        this.btnChamadosFinalizados = new System.Windows.Forms.Button();
        this.btnInventario = new System.Windows.Forms.Button();
        this.btnWiki = new System.Windows.Forms.Button();
        this.btnCredenciais = new System.Windows.Forms.Button();
        this.btnRelatorios = new System.Windows.Forms.Button();
        this.btnToggleSidebar = new System.Windows.Forms.Button();
        this.lblLogo = new System.Windows.Forms.Label();

        // panelSidebar
        this.panelSidebar.BackColor = System.Drawing.ColorTranslator.FromHtml("#1B676B");
        this.panelSidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.panelSidebar.Width = 220;

        // btnToggleSidebar
        this.btnToggleSidebar.Dock = System.Windows.Forms.DockStyle.Top;
        this.btnToggleSidebar.Height = 40;
        this.btnToggleSidebar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnToggleSidebar.FlatAppearance.BorderSize = 0;
        this.btnToggleSidebar.ForeColor = System.Drawing.Color.White;
        this.btnToggleSidebar.Text = "«";
        this.btnToggleSidebar.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
        this.btnToggleSidebar.Cursor = System.Windows.Forms.Cursors.Hand;
        this.btnToggleSidebar.Click += new System.EventHandler(this.btnToggleSidebar_Click);

        // pnlSubMenuChamados
        this.pnlSubMenuChamados.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlSubMenuChamados.Height = 0;
        this.pnlSubMenuChamados.BackColor = System.Drawing.ColorTranslator.FromHtml("#144D50");
        this.pnlSubMenuChamados.Controls.Add(this.btnChamadosFinalizados);
        this.pnlSubMenuChamados.Controls.Add(this.btnChamadosAtendimento);
        this.pnlSubMenuChamados.Controls.Add(this.btnChamadosAbertos);
        this.pnlSubMenuChamados.Controls.Add(this.btnChamadosTodos);

        // Logo
        this.lblLogo.ForeColor = System.Drawing.Color.White;
        this.lblLogo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
        this.lblLogo.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblLogo.Height = 100;
        this.lblLogo.Text = "TI MANAGER";
        this.lblLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // Button Styles Helper
        void StyleMain(System.Windows.Forms.Button b, string t) {
            b.Dock = System.Windows.Forms.DockStyle.Top;
            b.Height = 55;
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.ForeColor = System.Drawing.Color.White;
            b.Text = "   " + t;
            b.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            b.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            b.MouseEnter += (s, e) => b.BackColor = System.Drawing.ColorTranslator.FromHtml("#519548");
            b.MouseLeave += (s, e) => b.BackColor = System.Drawing.Color.Transparent;
        }

        void StyleSub(System.Windows.Forms.Button b, string t) {
            b.Dock = System.Windows.Forms.DockStyle.Top;
            b.Height = 35;
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            b.Text = "        • " + t;
            b.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            b.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            b.MouseEnter += (s, e) => b.ForeColor = System.Drawing.ColorTranslator.FromHtml("#BEF202");
            b.MouseLeave += (s, e) => b.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
        }

        // Apply Styles
        StyleMain(btnRelatorios, "Relatórios");
        StyleMain(btnCredenciais, "Credenciais");
        StyleMain(btnWiki, "Wiki");
        StyleMain(btnInventario, "Inventário");
        StyleSub(btnChamadosFinalizados, "Finalizados");
        StyleSub(btnChamadosAtendimento, "Em Atendimento");
        StyleSub(btnChamadosAbertos, "Abertos");
        StyleSub(btnChamadosTodos, "Todos");
        StyleMain(btnChamados, "Chamados   ▸");
        StyleMain(btnHome, "Home");

        // Add to Sidebar in Reverse Order (Dock.Top)
        this.panelSidebar.Controls.Add(this.btnRelatorios);
        this.panelSidebar.Controls.Add(this.btnCredenciais);
        this.panelSidebar.Controls.Add(this.btnWiki);
        this.panelSidebar.Controls.Add(this.btnInventario);
        this.panelSidebar.Controls.Add(this.pnlSubMenuChamados);
        this.panelSidebar.Controls.Add(this.btnChamados);
        this.panelSidebar.Controls.Add(this.btnHome);
        this.panelSidebar.Controls.Add(this.lblLogo);
        this.panelSidebar.Controls.Add(this.btnToggleSidebar);

        // panelContent
        this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panelContent.BackColor = System.Drawing.ColorTranslator.FromHtml("#EAFDE6");

        // Form1
        this.ClientSize = new System.Drawing.Size(1200, 750);
        this.Controls.Add(this.panelContent); // Adicionar o Fill primeiro ou garantir Z-order
        this.Controls.Add(this.panelSidebar);
        this.Name = "Form1";
        this.Text = "TI Manager - Gestão de TI";

        // Handlers
        this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
        this.btnChamados.Click += new System.EventHandler(this.btnChamados_Click);
        this.btnChamadosTodos.Click += new System.EventHandler(this.btnChamadosTodos_Click);
        this.btnChamadosAbertos.Click += new System.EventHandler(this.btnChamadosAbertos_Click);
        this.btnChamadosAtendimento.Click += new System.EventHandler(this.btnChamadosAtendimento_Click);
        this.btnChamadosFinalizados.Click += new System.EventHandler(this.btnChamadosFinalizados_Click);
        this.btnInventario.Click += new System.EventHandler(this.btnInventario_Click);
        this.btnWiki.Click += new System.EventHandler(this.btnWiki_Click);
        this.btnCredenciais.Click += new System.EventHandler(this.btnCredenciais_Click);
        this.btnRelatorios.Click += new System.EventHandler(this.btnRelatorios_Click);
    }
}
