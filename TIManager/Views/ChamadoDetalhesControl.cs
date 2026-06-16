using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using TIManager.Models;
using TIManager.Utils;
using TIManager.Data;

namespace TIManager.Views
{
    public class ChamadoDetalhesControl : UserControl
    {
        public event EventHandler OnVoltar;
        private Chamado chamado;

        private Panel pnlChatArea;
        private RoundedButton btnVoltar;
        private RoundedButton btnAcompanhar;
        private RoundedButton btnFinalizar;

        public ChamadoDetalhesControl(Chamado chamado)
        {
            this.chamado = chamado;
            InitializeComponent();
            CarregarConversa();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(248, 248, 248);
            this.Padding = new Padding(0);

            // 1. Painel Superior (Header) - Título Centralizado
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 10, 20, 10)
            };
            var lblTitulo = new Label
            {
                Text = chamado.Titulo,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#1B676B"),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblTitulo);
            var headerLine = new Label { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(225, 225, 225) };
            pnlHeader.Controls.Add(headerLine);
            this.Controls.Add(pnlHeader);

            // 2. Área de Cards (Centro - Scrollável)
            pnlChatArea = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(248, 248, 248),
                Padding = new Padding(30, 20, 30, 20)
            };
            UIHelper.SetDoubleBuffered(pnlChatArea);
            pnlChatArea.Resize += (s, e) => RecalcularLayout();
            this.Controls.Add(pnlChatArea);
            pnlChatArea.BringToFront();

            // 3. Barra de Botões (Inferior)
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(20, 14, 20, 14)
            };
            var footerLine = new Label { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(225, 225, 225) };
            pnlFooter.Controls.Add(footerLine);

            btnVoltar = new RoundedButton
            {
                Text = "← Voltar",
                Size = new Size(120, 40),
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                BorderColor = Color.LightGray,
                BorderSize = 1,
                BorderRadius = 20
            };
            btnVoltar.Click += (s, e) => OnVoltar?.Invoke(this, EventArgs.Empty);

            btnAcompanhar = new RoundedButton
            {
                Text = "Acompanhar",
                Size = new Size(155, 40),
                BackColor = ColorTranslator.FromHtml("#88C425"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 20
            };
            btnAcompanhar.Click += BtnAcompanhar_Click;

            btnFinalizar = new RoundedButton
            {
                Text = "Finalizar",
                Size = new Size(155, 40),
                BackColor = ColorTranslator.FromHtml("#D9534F"),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 20
            };
            btnFinalizar.Click += BtnFinalizar_Click;
            AtualizarBotaoFinalizar();

            pnlFooter.Controls.Add(btnVoltar);
            pnlFooter.Controls.Add(btnAcompanhar);
            pnlFooter.Controls.Add(btnFinalizar);

            pnlFooter.Resize += (s, e) =>
            {
                btnVoltar.Location = new Point(0, 14);
                btnFinalizar.Location = new Point(pnlFooter.Width - btnFinalizar.Width - 20, 14);
                btnAcompanhar.Location = new Point(btnFinalizar.Left - btnAcompanhar.Width - 10, 14);
            };

            this.Controls.Add(pnlFooter);
            pnlFooter.BringToFront();
        }

        private void AtualizarBotaoFinalizar()
        {
            if (chamado.Status == "Finalizado")
            {
                btnFinalizar.Enabled = false;
                btnFinalizar.BackColor = Color.FromArgb(210, 210, 210);
                btnFinalizar.ForeColor = Color.Gray;
                btnFinalizar.BorderColor = Color.Transparent;
            }
            else
            {
                btnFinalizar.Enabled = true;
                btnFinalizar.BackColor = ColorTranslator.FromHtml("#D9534F");
                btnFinalizar.ForeColor = Color.White;
                btnFinalizar.BorderColor = Color.Transparent;
            }
        }

        private void CarregarConversa()
        {
            pnlChatArea.SuspendLayout();
            pnlChatArea.Controls.Clear();

            // Card de Criação (branco, esquerda)
            var cardCriacao = new ChatCard(
                tipoCard: "Criacao",
                isRightAligned: false,
                dataHora: chamado.DataAbertura,
                metadados: new[] { ("Solicitante", chamado.Solicitante, false), ("Setor", chamado.Setor, false), ("Prioridade", chamado.Prioridade, true) },
                bodyText: chamado.Descricao,
                imagePath: chamado.CaminhoImagem
            );
            pnlChatArea.Controls.Add(cardCriacao);

            // Cards de Acompanhamento e Finalização
            var acompanhamentos = DatabaseService.Instance.GetAcompanhamentos(chamado.Id);
            foreach (var ac in acompanhamentos)
            {
                bool isFinal = ac.Tipo == "Finalizacao";
                var card = new ChatCard(
                    tipoCard: ac.Tipo,
                    isRightAligned: true,
                    dataHora: ac.Data,
                    metadados: null,
                    bodyText: ac.Texto,
                    imagePath: null
                );
                pnlChatArea.Controls.Add(card);
            }

            RecalcularLayout();
            pnlChatArea.ResumeLayout();
            ScrollParaFinal();
        }

        private void RecalcularLayout()
        {
            pnlChatArea.SuspendLayout();
            int scrollW = SystemInformation.VerticalScrollBarWidth;
            int areaW = pnlChatArea.ClientSize.Width - pnlChatArea.Padding.Horizontal - scrollW;
            if (areaW < 300) areaW = 300;

            int cardW = (int)(areaW * 0.72);
            if (cardW < 300) cardW = 300;

            int y = 0;
            foreach (Control ctrl in pnlChatArea.Controls)
            {
                if (ctrl is ChatCard card)
                {
                    card.Width = cardW;
                    card.RecalcularAltura();

                    if (card.IsRightAligned)
                        card.Location = new Point(areaW - cardW, y);
                    else
                        card.Location = new Point(0, y);

                    y += card.Height + 16;
                }
            }
            pnlChatArea.ResumeLayout();
        }

        private void BtnAcompanhar_Click(object sender, EventArgs e)
        {
            using (var form = new TextoAcompanhamentoForm("Adicionar Acompanhamento", "Texto do Acompanhamento:", ColorTranslator.FromHtml("#88C425"), "Adicionar"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string texto = form.TextoDigitado;
                    if (string.IsNullOrEmpty(texto)) return;

                    DatabaseService.Instance.InsertAcompanhamento(new Acompanhamento
                    {
                        ChamadoId = chamado.Id,
                        Texto = texto,
                        Data = DateTime.Now,
                        Tipo = "Acompanhamento"
                    });
                    CarregarConversa();
                }
            }
        }

        private void BtnFinalizar_Click(object sender, EventArgs e)
        {
            using (var form = new TextoAcompanhamentoForm("Finalizar Chamado", "Descrição da Finalização (Opcional):", ColorTranslator.FromHtml("#D9534F"), "Finalizar"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    string texto = form.TextoDigitado;
                    DatabaseService.Instance.InsertAcompanhamento(new Acompanhamento
                    {
                        ChamadoId = chamado.Id,
                        Texto = string.IsNullOrEmpty(texto) ? "Chamado finalizado pelo suporte." : texto,
                        Data = DateTime.Now,
                        Tipo = "Finalizacao"
                    });
                    chamado.Status = "Finalizado";
                    chamado.DataFinalizacao = DateTime.Now;
                    DatabaseService.Instance.UpdateChamado(chamado);
                    AtualizarBotaoFinalizar();
                    CarregarConversa();
                }
            }
        }

        private void ScrollParaFinal()
        {
            if (pnlChatArea.Controls.Count > 0)
                pnlChatArea.ScrollControlIntoView(pnlChatArea.Controls[pnlChatArea.Controls.Count - 1]);
        }
    }

    // ---------------------------------------------------------------
    // FORMULÁRIO MODAL DE TEXTO
    // ---------------------------------------------------------------
    internal class TextoAcompanhamentoForm : Form
    {
        public string TextoDigitado { get; private set; } = string.Empty;
        private TextBox txtTexto;

        public TextoAcompanhamentoForm(string titulo, string labelText, Color confirmarCor, string confirmarTexto)
        {
            this.Text = titulo;
            this.Size = new Size(500, 310);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            var lbl = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Dock = DockStyle.Top,
                Height = 28
            };
            this.Controls.Add(lbl);

            txtTexto = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10.5f),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtTexto);
            txtTexto.BringToFront();

            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(0, 14, 0, 0) };

            var btnCancelar = new RoundedButton
            {
                Text = "Cancelar",
                Size = new Size(120, 36),
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                BorderColor = Color.LightGray,
                BorderSize = 1,
                BorderRadius = 18
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            var btnConfirmar = new RoundedButton
            {
                Text = confirmarTexto,
                Size = new Size(120, 36),
                BackColor = confirmarCor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BorderRadius = 18
            };
            btnConfirmar.Click += (s, e) => { TextoDigitado = txtTexto.Text.Trim(); this.DialogResult = DialogResult.OK; this.Close(); };

            pnlFooter.Controls.Add(btnCancelar);
            pnlFooter.Controls.Add(btnConfirmar);
            pnlFooter.Resize += (s, e) =>
            {
                btnConfirmar.Location = new Point(pnlFooter.Width - btnConfirmar.Width, 14);
                btnCancelar.Location = new Point(btnConfirmar.Left - btnCancelar.Width - 10, 14);
            };

            this.Controls.Add(pnlFooter);
            pnlFooter.BringToFront();
        }
    }

    // ---------------------------------------------------------------
    // CARD DE CHAT — replica exatamente a aparência da imagem
    // ---------------------------------------------------------------
    internal class ChatCard : Panel
    {
        public bool IsRightAligned { get; }

        private readonly string tipoCard;
        private readonly DateTime dataHora;
        private readonly (string label, string value, bool colorirPorPrioridade)[]? metadados;
        private readonly string bodyText;
        private readonly string imagePath;

        // Cores por tipo
        private Color cardBackground;
        private Color cardBorder;
        private Color badgeBg;
        private Color badgeText;
        private Color headerTextColor;
        private string prefixoData;

        public ChatCard(
            string tipoCard,
            bool isRightAligned,
            DateTime dataHora,
            (string label, string value, bool colorirPorPrioridade)[]? metadados,
            string bodyText,
            string? imagePath)
        {
            this.tipoCard = tipoCard;
            this.IsRightAligned = isRightAligned;
            this.dataHora = dataHora;
            this.metadados = metadados;
            this.bodyText = bodyText ?? string.Empty;
            this.imagePath = imagePath ?? string.Empty;

            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(0);

            DefinirCores();
            ConstruirConteudo();
        }

        private void DefinirCores()
        {
            switch (tipoCard)
            {
                case "Acompanhamento":
                    cardBackground = ColorTranslator.FromHtml("#EAF5E9");
                    cardBorder = ColorTranslator.FromHtml("#C3D9C1");
                    badgeBg = ColorTranslator.FromHtml("#C9E8C6");
                    badgeText = ColorTranslator.FromHtml("#2D6A2B");
                    headerTextColor = ColorTranslator.FromHtml("#2D6A2B");
                    prefixoData = "Acompanhado em: ";
                    break;
                case "Finalizacao":
                    cardBackground = ColorTranslator.FromHtml("#FDECEA");
                    cardBorder = ColorTranslator.FromHtml("#E8B4B2");
                    badgeBg = ColorTranslator.FromHtml("#F5CECE");
                    badgeText = ColorTranslator.FromHtml("#922B21");
                    headerTextColor = ColorTranslator.FromHtml("#922B21");
                    prefixoData = "Finalizado em: ";
                    break;
                default: // "Criacao"
                    cardBackground = Color.White;
                    cardBorder = ColorTranslator.FromHtml("#D0D0D0");
                    badgeBg = ColorTranslator.FromHtml("#EAEAEA");
                    badgeText = ColorTranslator.FromHtml("#555555");
                    headerTextColor = ColorTranslator.FromHtml("#555555");
                    prefixoData = "Criado em: ";
                    break;
            }
        }

        private void ConstruirConteudo()
        {
            this.Controls.Clear();

            // --- CARD CONTAINER (drawn via OnPaint) ---
            var pnlCard = new DoubleBufferedPanel
            {
                BackColor = cardBackground,
                Location = new Point(0, 0)
            };

            // Badge (pill) no topo com os metadados
            var pnlBadgeRow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Painel interno da badge (fundo arredondado via OnPaint)
            var badgeHolder = new BadgePanel(badgeBg, ColorTranslator.FromHtml("#B0B0B0"))
            {
                AutoSize = false,
                BackColor = Color.Transparent,
                Height = 26,
                Margin = new Padding(0),
                Padding = new Padding(8, 0, 8, 0)
            };

            // Conteúdo da badge como FlowLayoutPanel de labels
            var badgeFlow = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.None,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = Color.Transparent,
                Location = new Point(8, 4)
            };

            // Data/hora
            string dataStr = $"{prefixoData}{dataHora:dd/MM/yyyy - HH:mm}";

            if (metadados != null && metadados.Length > 0)
            {
                // Criação: "Criado em: X, Solicitante: Y, Setor: Z, Prioridade: [VALOR COLORIDO]"
                string prefixo = dataStr;
                foreach (var (label, value, colorirPorPrioridade) in metadados)
                {
                    prefixo += $", {label}: ";
                    if (!colorirPorPrioridade)
                        prefixo += value;
                }

                // Parte não-colorida
                var lblPrefixo = new Label
                {
                    Text = prefixo,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = badgeText,
                    AutoSize = true,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.Transparent
                };
                badgeFlow.Controls.Add(lblPrefixo);

                // Parte colorida da prioridade
                foreach (var (label, value, colorirPorPrioridade) in metadados)
                {
                    if (colorirPorPrioridade)
                    {
                        Color prioColor = GetPriorityColor(value);
                        var lblPrio = new Label
                        {
                            Text = value,
                            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                            ForeColor = prioColor,
                            AutoSize = true,
                            Margin = new Padding(0),
                            Padding = new Padding(0),
                            BackColor = Color.Transparent
                        };
                        badgeFlow.Controls.Add(lblPrio);
                    }
                }
            }
            else
            {
                // Acompanhamento / Finalização: só a data
                var lblData = new Label
                {
                    Text = dataStr,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = badgeText,
                    AutoSize = true,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    BackColor = Color.Transparent
                };
                badgeFlow.Controls.Add(lblData);
            }

            badgeHolder.Controls.Add(badgeFlow);

            // Calcular largura do badge
            badgeHolder.Width = badgeFlow.PreferredSize.Width + 20;

            pnlBadgeRow.Controls.Add(badgeHolder);
            pnlCard.Controls.Add(pnlBadgeRow);

            // --- CORPO DO CARD ---
            var lblBody = new Label
            {
                Text = bodyText,
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize = true,
                MaximumSize = new Size(1, 0), // será ajustado em RecalcularAltura
                Location = new Point(16, 52),
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };
            pnlCard.Controls.Add(lblBody);

            // --- RODAPÉ: ANEXOS ---
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var pnlAnexo = new FlowLayoutPanel
                {
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0),
                    Padding = new Padding(0)
                };

                var lblAnexosTitle = new Label
                {
                    Text = "Imagens Anexas:",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(0, 1, 6, 0),
                    BackColor = Color.Transparent
                };
                pnlAnexo.Controls.Add(lblAnexosTitle);

                var lnkArquivo = new LinkLabel
                {
                    Text = "🖼 " + Path.GetFileName(imagePath),
                    Font = new Font("Segoe UI", 8.5f),
                    AutoSize = true,
                    Margin = new Padding(0, 1, 0, 0),
                    BackColor = Color.Transparent,
                    LinkColor = ColorTranslator.FromHtml("#1B676B")
                };
                lnkArquivo.Click += (s, ev) =>
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(imagePath) { UseShellExecute = true }); } catch { }
                };
                pnlAnexo.Controls.Add(lnkArquivo);

                // posicionamento dinâmico — será definido em RecalcularAltura
                pnlAnexo.Tag = "anexo";
                pnlCard.Controls.Add(pnlAnexo);
            }

            pnlCard.Tag = "card";
            this.Controls.Add(pnlCard);

            // Eventos de repaint para o card
            pnlCard.Paint += (s, e) => PintarCard(e.Graphics, pnlCard.ClientRectangle);
        }

        private void PintarCard(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int radius = 8;
            using var path = RoundedRect(rect, radius);

            // Fundo
            using var bgBrush = new SolidBrush(cardBackground);
            g.FillPath(bgBrush, path);

            // Borda
            using var pen = new Pen(cardBorder, 1f);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color GetPriorityColor(string prioridade)
        {
            return prioridade switch
            {
                "Crítica" or "Alta" => Color.FromArgb(192, 0, 0),
                "Média" => Color.DarkOrange,
                _ => Color.DarkGreen
            };
        }

        public void RecalcularAltura()
        {
            if (this.Controls.Count == 0) return;
            var pnlCard = this.Controls[0] as Panel;
            if (pnlCard == null) return;

            int innerW = this.Width - 32; // padding 16px cada lado
            if (innerW < 100) innerW = 100;

            // Atualiza MaximumSize do label do corpo
            Label? lblBody = null;
            Panel? pnlAnexo = null;
            foreach (Control c in pnlCard.Controls)
            {
                if (c is Label lbl && lbl.Location.Y > 40) lblBody = lbl;
                if (c is FlowLayoutPanel fp && fp.Tag?.ToString() == "anexo") pnlAnexo = (Panel)(object)fp;
            }

            int currentY = 52;

            if (lblBody != null)
            {
                lblBody.MaximumSize = new Size(innerW, 0);
                lblBody.Location = new Point(16, currentY);
                currentY = lblBody.Bottom + 16;
            }

            if (pnlAnexo != null)
            {
                // Separator line
                pnlAnexo.Location = new Point(16, currentY);
                currentY = pnlAnexo.Bottom + 14;
            }

            int cardHeight = currentY;
            pnlCard.Size = new Size(this.Width, cardHeight);
            this.Height = cardHeight;
        }
    }

    // Painel com double buffering habilitado (auxiliar)
    internal class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }

    // Badge com fundo arredondado desenhado via OnPaint
    internal class BadgePanel : Panel
    {
        private readonly Color bg;
        private readonly Color border;

        public BadgePanel(Color bg, Color border)
        {
            this.bg = bg;
            this.border = border;
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int r = this.Height / 2;
            using var path = RoundedRect(rect, r);
            using var brush = new SolidBrush(bg);
            g.FillPath(brush, path);
            using var pen = new Pen(border, 1f);
            g.DrawPath(pen, path);

            this.Region = new Region(path);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
